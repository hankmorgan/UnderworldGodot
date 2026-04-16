using System;
using System.IO;
using System.Threading;
using ADLMidi.NET;
using Godot;
using Underworld;

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
    private AudioStreamGeneratorPlayback _playback;
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

        var generator = new AudioStreamGenerator
        {
            MixRate = SampleRate,
            BufferLength = BufferLengthSec,
        };
        _player = new AudioStreamPlayer();
        AddChild(_player);
        _player.Stream = generator;
        _player.Play();
        _playback = (AudioStreamGeneratorPlayback)_player.GetStreamPlayback();

        _audioThreadRunning = true;
        _audioThread = new Thread(AudioThreadLoop)
        {
            IsBackground = true,
            Name = "SFX Producer",
        };
        _audioThread.Start();

        // SFX is currently UW1-only (the TVFX engine targets UW.AD; the UW2
        // path falls back to .voc files which we don't yet wire). Initialising
        // SoundEffects here means the singleton + scene-tree timing matches
        // MusicStreamPlayer's lifecycle.
        if (UWClass._RES == UWClass.GAME_UW1)
        {
            string soundDir = Path.Combine(UWClass.BasePath, "SOUND");
            SoundEffects.Initialize(uwsettings.instance.synth, soundDir);
        }
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
                    voice?.StartKeyon(cmd.Patch, cmd.LifetimeTicks);
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

    public override void _ExitTree()
    {
        _audioThreadRunning = false;
        _audioThread?.Join(500);
        _chip?.Dispose();
        if (Instance == this) Instance = null;
    }
}
