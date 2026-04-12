using System;
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
    private const float BufferLengthSec = 0.1f;

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
}
