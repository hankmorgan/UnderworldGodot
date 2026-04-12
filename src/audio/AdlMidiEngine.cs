using System;
using System.IO;
using System.Runtime.InteropServices;
using ADLMidi.NET;
using Godot;
using Munt.NET;
using SerdesNet;

namespace Underworld;

/// <summary>
/// ISynthEngine backed by AdlMidi.NET (OPL/AdLib FM synthesis).
/// Uses the game's own OPL bank file (UW.OPL for UW2, UW.AD for UW1) converted
/// to WOPL format for AdlMidi. Sets up a DllImportResolver on first construction.
/// </summary>
public sealed class AdlMidiEngine : ISynthEngine
{
    private static bool _dllLoaderSetUp;
    private static readonly object _dllLoaderLock = new();

    private readonly MidiPlayer _player;
    private bool _disposed;

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
            _player.Dispose();
            throw;
        }
    }

    public void PlayMsg(uint msg)
    {
        CheckDisposed();
        byte status = (byte)(msg & 0xFF);
        byte data1 = (byte)((msg >> 8) & 0xFF);
        byte data2 = (byte)((msg >> 16) & 0xFF);
        byte channel = (byte)(status & 0x0F);
        byte command = (byte)(status & 0xF0);

        switch (command)
        {
            case 0x80: _player.RealTimeNoteOff(channel, data1); break;
            case 0x90:
                if (data2 == 0)
                    _player.RealTimeNoteOff(channel, data1);
                else
                    _player.RealTimeNoteOn(channel, data1, data2);
                break;
            case 0xA0: _player.RealTimeNoteAfterTouch(channel, data1, data2); break;
            case 0xB0: _player.RealTimeControllerChange(channel, data1, data2); break;
            case 0xC0: _player.RealTimePatchChange(channel, data1); break;
            case 0xD0: _player.RealTimeChannelAfterTouch(channel, data1); break;
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
