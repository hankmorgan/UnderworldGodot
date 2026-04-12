using System;
using System.IO;
using System.Runtime.InteropServices;
using Godot;
using Munt.NET;

namespace Underworld;

/// <summary>
/// Godot node that owns a synth engine and real-time XmiPlayer, pushes PCM to
/// an AudioStreamGenerator from _Process. Singleton accessible via Instance.
/// </summary>
public partial class MusicStreamPlayer : Node
{
    public static MusicStreamPlayer Instance { get; private set; }

    private const int SampleRate = 44100;
    // Larger buffer to absorb main-thread hitches during cutscene scrolling.
    // Godot rounds up to nearest power-of-2, so actual buffer ~371ms.
    private const float BufferLengthSec = 0.25f;

    private ISynthEngine _synth;
    private XmiPlayer _xmiPlayer;
    private AudioStreamPlayer _player;
    private AudioStreamGeneratorPlayback _playback;

    private short[] _renderBuffer;
    private Vector2[] _frames;

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

        // Sized to cover the ~370ms buffer at 44100 Hz: 16384 frames = 32768 samples.
        _renderBuffer = new short[32768];
        _frames = new Vector2[16384];

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
    }

    public override void _Process(double delta)
    {
        if (_playback == null || _xmiPlayer == null) return;

        int available = _playback.GetFramesAvailable();
        if (available <= 0) return;

        int chunk = Math.Min(available, _frames.Length);
        _xmiPlayer.Render(_renderBuffer, chunk);

        for (int i = 0; i < chunk; i++)
        {
            float l = _renderBuffer[i * 2] / 32768f;
            float r = _renderBuffer[i * 2 + 1] / 32768f;
            _frames[i] = new Vector2(l, r);
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

    public void PlayXmi(string xmiPath, bool loop)
    {
        if (_xmiPlayer == null) return;
        if (!System.IO.File.Exists(xmiPath))
        {
            GD.Print($"XMI file not found: {xmiPath}");
            return;
        }
        _xmiPlayer.Load(xmiPath);
        _xmiPlayer.Loop = loop;
    }

    public void Stop()
    {
        _xmiPlayer?.Stop();
    }

    public override void _ExitTree()
    {
        _xmiPlayer?.Dispose();
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
