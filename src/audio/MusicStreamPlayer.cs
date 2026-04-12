using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Godot;
using Munt.NET;

namespace Underworld;

/// <summary>
/// Godot node that owns a synth engine and real-time XmiPlayer, pushes PCM to
/// an AudioStreamGenerator from a dedicated producer thread. A dedicated thread
/// is required because _Process() runs on the main thread and stalls during
/// cutscene scrolling / render-heavy frames, which would otherwise drain the
/// audio ring buffer. Singleton accessible via Instance.
/// </summary>
public partial class MusicStreamPlayer : Node
{
    public static MusicStreamPlayer Instance { get; private set; }

    private const int SampleRate = 44100;
    private const float BufferLengthSec = 0.1f;

    private ISynthEngine _synth;
    private XmiPlayer _xmiPlayer;
    private AudioStreamPlayer _player;
    private AudioStreamGeneratorPlayback _playback;

    // Pre-allocated buffers for the producer thread
    private short[] _renderBuffer;
    private Vector2[] _frames;

    // Threading
    private Thread _audioThread;
    private volatile bool _audioThreadRunning;
    // Protects _xmiPlayer method calls between the main thread (PlayXmi/Stop)
    // and the producer thread (Render).
    private readonly object _playerLock = new();

    /// <summary>True if an XMI track is currently loaded and playing.</summary>
    public bool IsPlaying
    {
        get
        {
            lock (_playerLock)
            {
                return _xmiPlayer?.IsPlaying == true;
            }
        }
    }

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

        // 4096 stereo frames ≈ 93ms at 44100 Hz — comfortably larger than one
        // _process tick but small enough that ChangeTheme latency is low.
        _renderBuffer = new short[8192];
        _frames = new Vector2[4096];

        _synth = CreateSynthEngine();
        if (_synth == null)
        {
            GD.Print("MusicStreamPlayer: no synth engine available, music disabled");
            return;
        }

        _xmiPlayer = new XmiPlayer(_synth, SampleRate);

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
            Name = "Music Producer",
        };
        _audioThread.Start();
    }

    /// <summary>
    /// Producer-thread loop. Polls GetFramesAvailable and pushes rendered PCM
    /// to the ring buffer. Sleeps briefly when the buffer is full.
    /// </summary>
    private void AudioThreadLoop()
    {
        while (_audioThreadRunning)
        {
            try
            {
                int available = _playback.GetFramesAvailable();
                if (available <= 0)
                {
                    Thread.Sleep(5);
                    continue;
                }

                int chunk = Math.Min(available, _frames.Length);

                lock (_playerLock)
                {
                    if (_xmiPlayer == null)
                    {
                        Thread.Sleep(5);
                        continue;
                    }
                    _xmiPlayer.Render(_renderBuffer, chunk);
                }

                // Convert stereo int16 → two float32 (-1..1) in Vector2 frames
                for (int i = 0; i < chunk; i++)
                {
                    _frames[i] = new Vector2(
                        _renderBuffer[i * 2] / 32768f,
                        _renderBuffer[i * 2 + 1] / 32768f);
                }

                if (chunk == _frames.Length)
                {
                    _playback.PushBuffer(_frames);
                }
                else
                {
                    var slice = new Vector2[chunk];
                    Array.Copy(_frames, slice, chunk);
                    _playback.PushBuffer(slice);
                }
            }
            catch (Exception ex)
            {
                GD.PushError($"Music audio thread error: {ex.Message}");
                Thread.Sleep(100);
            }
        }
    }

    /// <summary>Load and begin playing an XMI file. Thread-safe.</summary>
    public void PlayXmi(string xmiPath, bool loop)
    {
        if (!File.Exists(xmiPath))
        {
            GD.Print($"XMI file not found: {xmiPath}");
            return;
        }
        lock (_playerLock)
        {
            if (_xmiPlayer == null) return;
            _xmiPlayer.Load(xmiPath);
            _xmiPlayer.Loop = loop;
        }
    }

    /// <summary>Stop playback and reset synth state. Thread-safe.</summary>
    public void Stop()
    {
        lock (_playerLock)
        {
            _xmiPlayer?.Stop();
        }
    }

    public override void _ExitTree()
    {
        _audioThreadRunning = false;
        _audioThread?.Join(500);

        lock (_playerLock)
        {
            _xmiPlayer?.Dispose();
            _xmiPlayer = null;
        }
        _synth?.Dispose();
        if (Instance == this) Instance = null;
    }

    private static ISynthEngine CreateSynthEngine()
    {
        string synth = uwsettings.instance.synth?.ToLowerInvariant() ?? "soundfont";
        string path = uwsettings.instance.synthpath ?? "";

        try
        {
            switch (synth)
            {
                case "cm32l":
                case "mt32":
                    SetupMuntDllLoader();
                    return new Mt32EmuEngine(path, SampleRate);
                case "opl":
                    return new AdlMidiEngine(SampleRate);
                case "soundfont":
                default:
                    return new MeltySynthEngine(path, SampleRate);
            }
        }
        catch (Exception ex)
        {
            GD.Print($"Primary synth '{synth}' failed: {ex.Message}. Falling back to OPL.");
            try { return new AdlMidiEngine(SampleRate); }
            catch (Exception ex2)
            {
                GD.Print($"OPL fallback also failed: {ex2.Message}. Music disabled.");
                return null;
            }
        }
    }

    private static bool _muntDllLoaderSetUp;
    private static readonly object _muntDllLoaderLock = new();

    /// <summary>
    /// One-time DllImportResolver setup for libmt32emu. Munt.NET targets
    /// netstandard2.0 so it can't register the resolver itself — we do it here.
    /// </summary>
    private static void SetupMuntDllLoader()
    {
        lock (_muntDllLoaderLock)
        {
            if (_muntDllLoaderSetUp) return;
            _muntDllLoaderSetUp = true;

            NativeLibrary.SetDllImportResolver(
                typeof(Mt32EmuSynth).Assembly,
                (name, assembly, path) =>
                {
                    var root = AppContext.BaseDirectory;
                    string runtime = RuntimeInformation.RuntimeIdentifier;

                    string filename;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        filename = Path.GetExtension(name).Equals(".dll", StringComparison.OrdinalIgnoreCase)
                            ? name : name + ".dll";
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        filename = Path.GetExtension(name).Equals(".dylib", StringComparison.OrdinalIgnoreCase)
                            ? name : name + ".dylib";
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        filename = Path.GetExtension(name).Equals(".so", StringComparison.OrdinalIgnoreCase)
                            ? name : name + ".so";
                    else
                        throw new PlatformNotSupportedException();

                    var fullPath = Path.Combine(root, "runtimes", runtime, "native", filename);
                    return File.Exists(fullPath) ? NativeLibrary.Load(fullPath) : IntPtr.Zero;
                });
        }
    }
}
