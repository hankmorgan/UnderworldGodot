using System;
using System.IO;
using Godot;
using MeltySynth;
using Munt.NET;

namespace Underworld;

/// <summary>
/// ISynthEngine backed by MeltySynth (pure C# SoundFont synthesizer).
/// Discovery order for soundfont:
///   1. explicit soundfontPath parameter
///   2. {BasePath}/SOUND/default.sf2
///   3. res://soundfonts/default.sf2 (bundled)
/// </summary>
public sealed class MeltySynthEngine : ISynthEngine
{
    private readonly Synthesizer _synth;
    private readonly float[] _left;
    private readonly float[] _right;
    private bool _disposed;

    public MeltySynthEngine(string soundfontPath, int sampleRate = 44100)
    {
        string resolvedPath = ResolveSoundfont(soundfontPath);
        var settings = new SynthesizerSettings(sampleRate);
        _synth = new Synthesizer(resolvedPath, settings);
        _left = new float[4096];
        _right = new float[4096];
    }

    private static string ResolveSoundfont(string userPath)
    {
        if (!string.IsNullOrEmpty(userPath) && File.Exists(userPath))
            return userPath;

        var gameSf = Path.Combine(UWClass.BasePath, "SOUND", "default.sf2");
        if (File.Exists(gameSf))
            return gameSf;

        var bundled = ProjectSettings.GlobalizePath("res://soundfonts/default.sf2");
        if (File.Exists(bundled))
            return bundled;

        throw new InvalidOperationException(
            "No soundfont found. Looked in " +
            $"'{userPath}', '{gameSf}', '{bundled}'.");
    }

    public void PlayMsg(uint msg)
    {
        CheckDisposed();
        byte status = (byte)(msg & 0xFF);
        byte data1 = (byte)((msg >> 8) & 0xFF);
        byte data2 = (byte)((msg >> 16) & 0xFF);
        int channel = status & 0x0F;
        int command = status & 0xF0;
        _synth.ProcessMidiMessage(channel, command, data1, data2);
    }

    public void PlaySysex(byte[] data)
    {
        CheckDisposed();
        // MeltySynth doesn't support arbitrary SysEx. Silently ignored.
    }

    public void Render(short[] buffer, uint frameCount)
    {
        CheckDisposed();

        int framesRemaining = (int)frameCount;
        int bufferOffset = 0;

        while (framesRemaining > 0)
        {
            int chunk = Math.Min(framesRemaining, _left.Length);
            var leftSpan = _left.AsSpan(0, chunk);
            var rightSpan = _right.AsSpan(0, chunk);
            _synth.Render(leftSpan, rightSpan);

            for (int i = 0; i < chunk; i++)
            {
                buffer[bufferOffset++] = FloatToShort(_left[i]);
                buffer[bufferOffset++] = FloatToShort(_right[i]);
            }

            framesRemaining -= chunk;
        }
    }

    public void Reset()
    {
        CheckDisposed();
        for (int ch = 0; ch < 16; ch++)
        {
            _synth.ProcessMidiMessage(ch, 0xB0, 123, 0); // All Notes Off
            _synth.ProcessMidiMessage(ch, 0xB0, 120, 0); // All Sound Off
            _synth.ProcessMidiMessage(ch, 0xB0, 121, 0); // Reset All Controllers
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    private void CheckDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(MeltySynthEngine));
    }

    private static short FloatToShort(float f)
    {
        int s = (int)(f * 32767f);
        if (s > 32767) s = 32767;
        if (s < -32768) s = -32768;
        return (short)s;
    }
}
