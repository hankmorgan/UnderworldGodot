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
/// <remarks>
/// Lifecycle: _Ready constructs the synth engine and starts the producer
/// thread; _ExitTree signals shutdown and joins. PlayXmi/Stop/IsPlaying may be
/// called from the main thread at any time and are serialised against the
/// producer thread by <see cref="_playerLock"/>.
/// </remarks>
public partial class MusicStreamPlayer : Node
{
    /// <summary>Global singleton — the first instance added to the tree wins; later ones self-free.</summary>
    public static MusicStreamPlayer Instance { get; private set; }

    private const int SampleRate = 44100;
    // 0.1s of ring buffer. Before the producer thread existed this had to be
    // much larger (several hundred ms) to ride out main-thread stalls; now the
    // producer thread keeps the buffer topped up independently of _Process,
    // so a short buffer (low latency for ChangeTheme) is safe.
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
    // volatile: read from the producer thread every loop iteration, written
    // from the main thread in _ExitTree; no other synchronisation needed.
    private volatile bool _audioThreadRunning;
    // Protects _xmiPlayer method calls between the main thread (PlayXmi/Stop)
    // and the producer thread (Render). XmiPlayer itself is not thread-safe:
    // Load/Stop mutate the event list while Render walks it, so every touch
    // of _xmiPlayer — on either thread — must hold this lock. The synth
    // engine is only touched via _xmiPlayer so it inherits the same guard.
    private readonly object _playerLock = new();

    /// <summary>
    /// True if an XMI track is currently loaded and playing. Thread-safe —
    /// takes <see cref="_playerLock"/> to read XmiPlayer state.
    /// </summary>
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

    /// <summary>
    /// Enforces the singleton, constructs the synth + XmiPlayer, wires up the
    /// AudioStreamGenerator and starts the producer thread. If synth creation
    /// fails entirely (including OPL fallback), the node remains alive but
    /// silent — callers of PlayXmi become no-ops.
    /// </summary>
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
    /// <remarks>
    /// We poll with <c>Thread.Sleep(5)</c> rather than a wait/signal pattern
    /// because the API (GetFramesAvailable/PushBuffer) has no completion event
    /// and a 5 ms sleep is cheap — at 44.1 kHz the ring buffer drains ~220
    /// frames in that window, far below the 4096-frame chunk we render. The
    /// try/catch guards against transient failures in the synth (e.g. a
    /// SoundFont bug) and sleeps 100 ms on error to avoid busy-looping on a
    /// persistent failure mode; errors surface via GD.PushError.
    /// </remarks>
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

    /// <summary>
    /// Load and begin playing an XMI file. Thread-safe — may be called from
    /// the main thread while the producer thread is rendering; the lock
    /// serialises the Load against any in-flight Render call so event-list
    /// mutation never races with iteration.
    /// </summary>
    /// <param name="xmiPath">Absolute path to an XMI file on disk.</param>
    /// <param name="loop">If true, playback loops indefinitely.</param>
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

    /// <summary>
    /// Stop playback and reset synth state (all-notes-off etc.). Thread-safe.
    /// Subsequent Render calls will emit silence until the next PlayXmi.
    /// </summary>
    public void Stop()
    {
        lock (_playerLock)
        {
            _xmiPlayer?.Stop();
        }
    }

    /// <summary>
    /// Signals the producer thread to exit, joins it (bounded wait), then
    /// disposes the XmiPlayer and synth. Join timeout is 500 ms — the loop
    /// sleeps at most 100 ms so this is comfortable headroom.
    /// </summary>
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

    /// <summary>
    /// Construct the synth engine selected by uwsettings.synth, with a two-
    /// step fallback chain: primary (as configured) → OPL (AdlMidi, always
    /// available because the game ships the OPL bank) → null (music disabled).
    /// Any exception from the primary or OPL constructor is caught and logged
    /// rather than propagating, so a missing ROM or SoundFont never crashes
    /// startup.
    /// </summary>
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
    /// netstandard2.0 which does not expose <see cref="NativeLibrary"/> —
    /// so the library can't register its own resolver and we install one
    /// here (from a net9.0 host) on its behalf. The resolver looks in
    /// <c>{AppContext.BaseDirectory}/runtimes/{rid}/native/</c>, which is
    /// the standard NuGet package layout for native assets — that way the
    /// same resolver works whether the dylib came from a NuGet restore or
    /// was copied into the publish output by our build scripts.
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
