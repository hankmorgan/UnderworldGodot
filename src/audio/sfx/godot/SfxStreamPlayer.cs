using System;
using System.IO;
using System.Threading;
using ADLMidi.NET;
using Godot;

namespace Underworld.Sfx;

/// <summary>
/// Godot node owning a bare OPL2 chip and 9-voice TVFX allocator. Drains
/// triggers from an SPSC queue (game thread → audio thread), services TVFX
/// at 60 Hz, generates PCM, and pushes to an AudioStreamGenerator.
///
/// Mirrors MusicStreamPlayer's producer-thread pattern: a dedicated thread is
/// required because _Process() stalls during cutscene scrolling / heavy frames
/// and the ring buffer would drain.
///
/// Singleton via <see cref="Instance"/>. The first instance added to the tree
/// wins; later ones self-free (matches MusicStreamPlayer convention).
/// </summary>
public partial class SfxStreamPlayer : Node
{
    public static SfxStreamPlayer Instance { get; private set; }

    private const int SampleRate = 44100;
    private const int FramesPerTick = SampleRate / 60;       // 735, exact
    private const float BufferLengthSec = 0.1f;
    private const int CommandQueueCapacity = 64;             // ample headroom; SFX bursts are small

    private readonly SpscQueue<SfxCommand> _commands = new(CommandQueueCapacity);
    private readonly TvfxVoiceAllocator _allocator = new();

    private OplChip _chip;
    private AudioStreamPlayer _player;
    private AudioStreamGenerator _generator;
    private AudioStreamGeneratorPlayback _playback;
    private bool _producerStopped;
    private bool _bindingsReleased;
    private short[] _renderBuffer;
    private Vector2[] _frames;

    private Thread _audioThread;
    private volatile bool _audioThreadRunning;

    private sealed class ChipSink : IOplRegisterSink
    {
        private readonly OplChip _chip;
        public ChipSink(OplChip chip) { _chip = chip; }
        public void WriteReg(int addr, byte val) => _chip.WriteReg(addr, val);
    }
    private ChipSink _sink;

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
            return;
        }

        try
        {
            _chip = OplChip.Create(SampleRate);
            // OPL2 Waveform Select Enable. Without bit 5 of reg 0x01, every
            // operator emits a pure sine regardless of the waveform nibble in
            // 0xE0+op. TVFX patches select waveforms 1-3 (half-sine, abs-sine,
            // quarter-sine) for most timbres, so this init is load-bearing for
            // authentic sound — psmitty7373's Python reference writes the same.
            // libadlmidi's OPL3_Reset leaves the OPL3 "new" bit cleared, i.e.
            // runs in OPL2-compat mode where this WSE bit is required.
            _chip.WriteReg(0x01, 0x20);
        }
        catch (Exception ex)
        {
            GD.PushError($"SfxStreamPlayer: OplChip.Create failed: {ex.Message}. SFX disabled.");
            return;
        }
        _sink = new ChipSink(_chip);
        _renderBuffer = new short[FramesPerTick * 2];
        _frames = new Vector2[FramesPerTick];

        // Resolve the path before the player and the thread exist. Path.Combine throws if
        // BasePath is null, which is what happens when the settings could not be read, and a
        // throw after the generator was playing and the producer thread was running would leave
        // both going against a construction that had already failed. The chip, the sink and the
        // buffers above are already built by this point, and _ExitTree is what takes those down.
        //
        // SFX is currently UW1-only (the TVFX engine targets UW.AD; the UW2
        // path falls back to .voc files which we don't yet wire).
        //if (UWClass._RES == UWClass.GAME_UW1)
       // {
            if (string.IsNullOrWhiteSpace(UWClass.BasePath))
            {
                GD.PushError("SfxStreamPlayer: no game path is configured. SFX disabled.");
                return;
            }
            string soundDir = Path.Combine(UWClass.BasePath, "SOUND");
        //}

        _generator = new AudioStreamGenerator
        {
            MixRate = SampleRate,
            BufferLength = BufferLengthSec,
        };
        _player = new AudioStreamPlayer();
        AddChild(_player);
        _player.Stream = _generator;
        _player.Play();
        _playback = (AudioStreamGeneratorPlayback)_player.GetStreamPlayback();

        _audioThreadRunning = true;
        _audioThread = new Thread(AudioThreadLoop)
        {
            IsBackground = true,
            Name = "SFX Producer",
        };
        _audioThread.Start();

        // Published last, once there is something behind it. SoundEffects.Initialize makes this
        // node reachable from SoundEffects.Play, so doing it earlier would mean a failure while
        // building the player or starting the thread left callers able to enqueue against a
        // producer that never ran. The thread itself does not read any SoundEffects state, so
        // it is safe already running.
        SoundEffects.Initialize(uwsettings.instance.synth, soundDir);
    }

    /// <summary>
    /// Game-thread API: enqueue a trigger. Returns false if the queue is
    /// saturated (very rare — 64 outstanding triggers means something is wrong).
    /// </summary>
    public bool Enqueue(SfxCommand cmd) => _commands.TryEnqueue(cmd);

    /// <summary>
    /// Producer-thread loop. One iteration = one TVFX service tick (1/60 s).
    /// We sleep when the ring buffer can't hold another tick worth of frames.
    /// </summary>
    private void AudioThreadLoop()
    {
        while (_audioThreadRunning)
        {
            try
            {
                if (_playback.GetFramesAvailable() < FramesPerTick)
                {
                    Thread.Sleep(5);
                    continue;
                }

                // Drain commands: start voices for any pending triggers.
                while (_commands.TryDequeue(out var cmd))
                {
                    var voice = _allocator.Allocate();
                    voice?.StartKeyon(cmd.Patch, cmd.LifetimeTicks, cmd.VolScale);
                    // null = saturated (all 9 voices busy) → drop trigger silently,
                    // matching authentic UW behaviour.
                }

                _allocator.ServiceAll(_sink);
                _chip.GenerateFrames(_renderBuffer, FramesPerTick);

                for (int i = 0; i < FramesPerTick; i++)
                {
                    _frames[i] = new Vector2(
                        _renderBuffer[i * 2]     / 32768f,
                        _renderBuffer[i * 2 + 1] / 32768f);
                }
                _playback.PushBuffer(_frames);
            }
            catch (Exception ex)
            {
                GD.PushError($"SFX audio thread error: {ex.Message}");
                Thread.Sleep(100);
            }
        }
    }

    /// <summary>
    /// Stops the producer and returns the playback to the AudioServer. See
    /// MusicStreamPlayer.BeginGodotAudioShutdown and issue #78 for why disposal is deferred.
    /// </summary>
    public bool BeginGodotAudioShutdown(int joinMs = 500)
    {
        if (_producerStopped) return true;

        _audioThreadRunning = false;
        if (_audioThread != null && !_audioThread.Join(joinMs))
        {
            // The producer drives the chip and the playback; disposing either while it is
            // still running would be a use after dispose.
            GD.PushError($"SFX producer did not stop within {joinMs} ms; retrying before release.");
            return false;
        }
        _chip?.Dispose();
        _chip = null;

        if (_player != null && GodotObject.IsInstanceValid(_player))
        {
            _player.Stop();
            _player.Stream = null;
        }

        _producerStopped = true;
        return true;
    }

    /// <summary>Disposes the audio wrappers once the server has collected the playback.</summary>
    public void ReleaseGodotAudioBindings()
    {
        // Never retries phase one here. Stopping the player and releasing its wrappers in
        // the same call is the ordering that hangs; the caller retries phase one and only
        // then lets the server drain before calling this.
        if (_bindingsReleased || !_producerStopped) return;
        _bindingsReleased = true;

        if (_player != null && GodotObject.IsInstanceValid(_player))
        {
            _player.QueueFree();
            _player = null;
        }
        _playback?.Dispose();
        _playback = null;
        _generator?.Dispose();
        _generator = null;
    }

    public override void _ExitTree()
    {
        // Stop only. Releasing the wrappers here would be the same-frame ordering that
        // leaves the playback in the server's list; UnderworldRoot releases them on the
        // quit path once the server has had time to collect it.
        BeginGodotAudioShutdown();
        if (Instance == this) Instance = null;
    }
}
