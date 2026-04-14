using System;
using System.IO;
using Godot;
using MeltySynth;
using Munt.NET;

namespace Underworld;

/// <summary>
/// ISynthEngine backed by MeltySynth — a pure-C# SoundFont 2 synthesizer
/// targeting the General MIDI instrument set. This is our default engine
/// because it has no native dependencies (works everywhere .NET runs) and
/// SoundFonts are freely available, including tiny bundled defaults.
/// </summary>
/// <remarks>
/// Not period-authentic: UW1/UW2 were composed for MT-32 (and FM fallback).
/// SoundFont playback sounds "modern GM" rather than like the original
/// hardware — choose <see cref="Mt32EmuEngine"/> for authenticity.
/// Discovery order for soundfont (see <see cref="ResolveSoundfont"/>):
///   1. explicit soundfontPath parameter (from uwsettings)
///   2. {BasePath}/SOUND/default.sf2 (drop-in alongside game files)
///   3. res://soundfonts/default.sf2 (bundled in the Godot project)
/// </remarks>
public sealed class MeltySynthEngine : ISynthEngine
{
    private readonly Synthesizer _synth;
    // Scratch float buffers — MeltySynth renders into separate L/R float
    // spans; we interleave and convert to int16 in Render().
    private readonly float[] _left;
    private readonly float[] _right;
    private bool _disposed;

    /// <summary>
    /// Construct the synth. Throws <see cref="InvalidOperationException"/>
    /// if no SoundFont can be found at any of the three discovery locations.
    /// </summary>
    /// <param name="soundfontPath">Explicit .sf2 path, or empty to fall through to auto-discovery.</param>
    /// <param name="sampleRate">Output sample rate in Hz.</param>
    public MeltySynthEngine(string soundfontPath, int sampleRate = 44100)
    {
        string resolvedPath = ResolveSoundfont(soundfontPath);
        var settings = new SynthesizerSettings(sampleRate);
        _synth = new Synthesizer(resolvedPath, settings);
        _left = new float[4096];
        _right = new float[4096];
    }

    /// <summary>
    /// Locate a SoundFont file using a 3-level discovery order:
    ///   1. explicit user path (uwsettings.synthpath) — wins if the file exists
    ///   2. {BasePath}/SOUND/default.sf2 — lets users drop a SoundFont next to the game
    ///   3. res://soundfonts/default.sf2 — fallback shipped with the project
    /// Throws if none of these exist; the message lists all three so users
    /// know exactly where to put a SoundFont.
    /// </summary>
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

    /// <inheritdoc/>
    public void PlayMsg(uint msg)
    {
        CheckDisposed();
        // Unpack the packed MIDI message. Layout:
        //   byte 0 (status): top nibble = command (0x80 NoteOff ... 0xE0 PitchBend),
        //                    bottom nibble = channel (0..15).
        //   byte 1 (data1):  first data byte (e.g. note number, controller index).
        //   byte 2 (data2):  second data byte (e.g. velocity, controller value).
        byte status = (byte)(msg & 0xFF);
        byte data1 = (byte)((msg >> 8) & 0xFF);
        byte data2 = (byte)((msg >> 16) & 0xFF);
        int channel = status & 0x0F;
        int command = status & 0xF0;
        _synth.ProcessMidiMessage(channel, command, data1, data2);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Silently ignored. SoundFont synths have no meaningful interpretation
    /// of SysEx — the SoundFont file itself defines the entire instrument
    /// bank, and unlike an MT-32 there's no runtime patch/timbre loading
    /// protocol to apply. Keeping this as a no-op is preferable to throwing
    /// so that XMI streams containing GM reset / device-ID SysEx still play.
    /// </remarks>
    public void PlaySysex(byte[] data)
    {
        CheckDisposed();
        // MeltySynth doesn't support arbitrary SysEx. Silently ignored.
    }

    /// <inheritdoc/>
    public void Render(short[] buffer, uint frameCount)
    {
        CheckDisposed();

        int framesRemaining = (int)frameCount;
        int bufferOffset = 0;

        // Render in chunks no larger than our pre-allocated L/R scratch
        // buffers; MeltySynth takes L/R spans and produces float samples,
        // which we then interleave + convert to int16 for the caller.
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

    /// <inheritdoc/>
    /// <remarks>
    /// Order matters. CC 120 (All Sound Off) fires first as a panic cut —
    /// it silences every active partial immediately, including reverb /
    /// release tails that would otherwise bleed into the new track. CC 123
    /// (All Notes Off) then sends a normal note-off to anything that somehow
    /// survived, and CC 121 (Reset All Controllers) returns pitch bend,
    /// modulation, expression, etc. to their defaults so the next track
    /// starts from a clean channel state.
    /// </remarks>
    public void Reset()
    {
        CheckDisposed();
        for (int ch = 0; ch < 16; ch++)
        {
            _synth.ProcessMidiMessage(ch, 0xB0, 120, 0); // All Sound Off (hard cut — panic first)
            _synth.ProcessMidiMessage(ch, 0xB0, 123, 0); // All Notes Off
            _synth.ProcessMidiMessage(ch, 0xB0, 121, 0); // Reset All Controllers
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    private void CheckDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(MeltySynthEngine));
    }

    /// <summary>
    /// Scale a normalised float sample (nominally in [-1, 1]) to signed 16-bit
    /// PCM and clip to the int16 range. We multiply by 32767 (not 32768) so
    /// that +1.0 maps exactly to <see cref="short.MaxValue"/>; the clamp to
    /// -32768 handles the asymmetric negative range. Synths can (and do) emit
    /// samples slightly outside [-1, 1] during transients — hard clipping is
    /// preferable to wrap-around noise.
    /// </summary>
    private static short FloatToShort(float f)
    {
        int s = (int)(f * 32767f);
        if (s > 32767) s = 32767;
        if (s < -32768) s = -32768;
        return (short)s;
    }
}
