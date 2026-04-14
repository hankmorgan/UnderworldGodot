using System;
using System.IO;
using System.Runtime.InteropServices;
using ADLMidi.NET;
using Godot;
using Munt.NET;
using SerdesNet;

namespace Underworld;

/// <summary>
/// ISynthEngine backed by AdlMidi.NET — an OPL3 FM-synthesis emulator that
/// reproduces the period-authentic Sound Blaster / AdLib soundscape most
/// DOS players would have heard (MT-32 being the premium alternative).
/// </summary>
/// <remarks>
/// <para>The game ships its own proprietary OPL instrument bank
/// (<c>UW.OPL</c> for UW2, <c>UW.AD</c> for UW1). That format is not
/// something libADLMIDI understands directly, so at construction we parse
/// it with <see cref="GlobalTimbreLibrary"/> and convert each timbre into a
/// <see cref="WoplInstrument"/>, packaging the result as a WOPL file —
/// libADLMIDI's native bank format — which we hand to the player as a
/// byte[] via <see cref="MidiPlayer.OpenBankData"/>.</para>
/// <para>This means the in-game FM music is driven by the game's own
/// timbres, not a generic GM bank — so MIDI patch numbers map to the
/// instruments the original sound designers actually authored for UW.</para>
/// <para>AdlMidi also serves as the always-available fallback engine
/// (see <c>MusicStreamPlayer.CreateSynthEngine</c>) because every copy of
/// the game has the bank file — there are no external assets to install.</para>
/// </remarks>
public sealed class AdlMidiEngine : ISynthEngine
{
    private static bool _dllLoaderSetUp;
    private static readonly object _dllLoaderLock = new();

    private readonly MidiPlayer _player;
    private bool _disposed;

    /// <summary>
    /// Construct the engine. Two failure modes can throw:
    ///   (1) the OPL bank file is missing (wrong BasePath, incomplete install);
    ///   (2) the bank file exists but fails to parse / convert to WOPL.
    /// The try/catch ensures <see cref="_player"/> is disposed on either
    /// failure so we don't leak the unmanaged MidiPlayer handle on the way
    /// out. Callers should treat any throw as "OPL unavailable".
    /// </summary>
    public AdlMidiEngine(int sampleRate = 44100)
    {
        SetupDllLoader();

        _player = AdlMidi.Init(sampleRate);
        try
        {
            var bankData = LoadGameBank();
            _player.OpenBankData(bankData);
        }
        catch
        {
            // Don't leak the native player on bank-load failure.
            _player.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public void PlayMsg(uint msg)
    {
        CheckDisposed();
        byte status = (byte)(msg & 0xFF);
        byte data1 = (byte)((msg >> 8) & 0xFF);
        byte data2 = (byte)((msg >> 16) & 0xFF);
        byte channel = (byte)(status & 0x0F);
        byte command = (byte)(status & 0xF0);

        // Dispatch by MIDI command nibble. Unlike a SoundFont synth we can't
        // just hand AdlMidi a packed message — libADLMIDI exposes distinct
        // "real time" entry points per command, so we unpack and route.
        switch (command)
        {
            case 0x80: _player.RealTimeNoteOff(channel, data1); break;
            case 0x90:
                // MIDI running-status quirk: a NoteOn with velocity 0 is
                // defined by the spec as equivalent to NoteOff. XMI (and many
                // MIDI files) exploit this to omit explicit NoteOff status
                // bytes, so we must honour it here or notes will hang forever.
                if (data2 == 0)
                    _player.RealTimeNoteOff(channel, data1);
                else
                    _player.RealTimeNoteOn(channel, data1, data2);
                break;
            case 0xA0: _player.RealTimeNoteAfterTouch(channel, data1, data2); break;
            case 0xB0: _player.RealTimeControllerChange(channel, data1, data2); break;
            case 0xC0: _player.RealTimePatchChange(channel, data1); break;
            case 0xD0: _player.RealTimeChannelAfterTouch(channel, data1); break;
            // Pitch bend: data2 is the MSB, data1 the LSB — libADLMIDI takes
            // (msb, lsb) in that order, so we pass data2 first.
            case 0xE0: _player.RealTimePitchBendML(channel, data2, data1); break;
        }
    }

    public unsafe void PlaySysex(byte[] data)
    {
        CheckDisposed();
        fixed (byte* ptr = data)
        {
            _player.RealTimeSystemExclusive((IntPtr)ptr, (UIntPtr)data.Length);
        }
    }

    public void Render(short[] buffer, uint frameCount)
    {
        CheckDisposed();
        int samples = (int)frameCount * 2; // stereo interleaved
        _player.Generate(buffer.AsSpan(0, samples));
    }

    public void Reset()
    {
        CheckDisposed();
        _player.RealTimeResetState();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _player.Dispose();
    }

    private void CheckDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(AdlMidiEngine));
    }

    /// <summary>
    /// Read the game's proprietary OPL timbre bank and convert it to a WOPL
    /// byte[] suitable for <see cref="MidiPlayer.OpenBankData"/>.
    /// </summary>
    /// <remarks>
    /// The source format is a flat <see cref="GlobalTimbreLibrary"/> — each
    /// entry describes one OPL instrument as two operator blocks (a carrier
    /// + a modulator, the classic 2-op FM voice) plus a feedback-connection
    /// byte that wires them together. WOPL is libADLMIDI's container format
    /// and stores the same operator data plus extra metadata (4-operator
    /// support, global flags, banking, names, note offsets). We build a
    /// single melodic bank + single percussion bank in WOPL v3.
    ///
    /// Indexing: timbres 0..127 become the melodic patches 0..127. Timbres
    /// at index 128 and above are percussion instruments; we write them into
    /// the percussion bank at <c>(i - 128) + 35</c>. The +35 offset follows
    /// the General MIDI percussion key map, which starts at note 35
    /// (Acoustic Bass Drum) — the first percussion timbre in the game's
    /// bank corresponds to GM percussion key 35, and so on.
    /// </remarks>
    private static byte[] LoadGameBank()
    {
        string bankFilename = UWClass._RES == UWClass.GAME_UW2 ? "UW.OPL" : "UW.AD";
        string bankPath = Path.Combine(UWClass.BasePath, "SOUND", bankFilename);
        if (!File.Exists(bankPath))
            throw new InvalidOperationException($"OPL bank file not found: {bankPath}");

        GlobalTimbreLibrary oplFile;
        using (var stream = File.OpenRead(bankPath))
        using (var br = new BinaryReader(stream))
        {
            oplFile = GlobalTimbreLibrary.Serdes(
                null,
                new ReaderSerdes(br, br.BaseStream.Length, s => GD.Print(s)));
        }

        var wopl = new WoplFile
        {
            Version = 3,
            GlobalFlags = GlobalBankFlags.DeepTremolo | GlobalBankFlags.DeepVibrato,
            VolumeModel = VolumeModel.Auto
        };
        wopl.Melodic.Add(new WoplBank { Id = 0, Name = "" });
        wopl.Percussion.Add(new WoplBank { Id = 0, Name = "" });

        for (int i = 0; i < oplFile.Data.Count; i++)
        {
            var timbre = oplFile.Data[i];
            var x = i < 128
                ? wopl.Melodic[0].Instruments[i] ?? new WoplInstrument()
                : wopl.Percussion[0].Instruments[i - 128 + 35] ?? new WoplInstrument();

            x.Name = "";
            x.NoteOffset1 = timbre.MidiPatchNumber;
            x.NoteOffset2 = timbre.MidiBankNumber;
            x.InstrumentMode = InstrumentMode.TwoOperator;
            x.FbConn1C0 = timbre.FeedbackConnection;
            x.Operator0 = timbre.Carrier;
            x.Operator1 = timbre.Modulation;
            x.Operator2 = Operator.Blank;
            x.Operator3 = Operator.Blank;

            if (i < 128)
                wopl.Melodic[0].Instruments[i] = x;
            else
                wopl.Percussion[0].Instruments[i - 128 + 35] = x;
        }

        using var ms = new MemoryStream();
        using var bw2 = new BinaryWriter(ms);
        WoplFile.Serdes(wopl, new WriterSerdes(bw2, s => GD.Print(s)));
        return ms.ToArray();
    }

    /// <summary>
    /// One-time DllImportResolver setup for libADLMIDI. Same rationale as
    /// <c>MusicStreamPlayer.SetupMuntDllLoader</c>: AdlMidi.NET ships as
    /// netstandard2.0 and cannot self-register a resolver, so the host app
    /// does it. Native assets are looked up under
    /// <c>{AppContext.BaseDirectory}/runtimes/{rid}/native/</c>, matching
    /// the standard NuGet runtimes layout (and our publish scripts).
    /// </summary>
    private static void SetupDllLoader()
    {
        lock (_dllLoaderLock)
        {
            if (_dllLoaderSetUp) return;
            _dllLoaderSetUp = true;

            NativeLibrary.SetDllImportResolver(
                typeof(AdlMidi).Assembly,
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
