using System.IO;
using System.Diagnostics;
using Godot;
using System.Runtime.InteropServices;
using ADLMidi.NET;
using SerdesNet;
using System;


namespace Underworld
{
    /// <summary>
    /// Converts the UW xmi files to WAV files. Based on example implementation from ADLMidi https://github.com/csinkers/AdlMidi.NET
    /// </summary>
    public class XMIMusic : UWClass
    {      
        public static byte CurrentThemeNo;
        public static bool LoopTheme = false;
        static byte[] UW2WorldThemes = [0xA, 0xC, 0xE, 0x9, 0xA, 0xF, 0xB, 0xD, 0xA, 0xC, 0xD, 0x9, 0x8, 0xB, 0xE, 0xD, 0x8, 0xF, 0xE, 0x9, 0x8, 0xF, 0xB, 0xA, 0x8, 0xC, 0x9];
        static int CurrentWorldTheme;
        const int SampleRate = 44100;
        static double _time;

        static string bankfile
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return "UW.OPL";
                }
                else
                {
                    return "UW.AD";
                }
            }
        }


        //Playlists, returns a possibly random theme tune to play in a particular scenario.
        public const byte IntroTheme = 1;
        public const byte MapsAndLegends = 15;// UW1 specific conversations and maps viewing them

        public static byte Armed
        {
            get
            {
                if (_RES==GAME_UW2)
                {
                    return 5;
                }
                else
                {
                    return 10;
                }
            }
        } 


        /// <summary>
        /// Changes the music theme that is playing based on theme file number.
        /// </summary>
        /// <param name="themeNo"></param>
        public static void ChangeTheme(byte themeNo, bool Loop = false)
        {
            CurrentThemeNo = themeNo;
            switch(_RES)
            {
                case GAME_UW2:
                    ChangeTheme($"UWA{themeNo:D2}.WAV",Loop);//TODO provide a way to choose between playing UWAxx.xmi or UWRxx.xmi)
                    break;
                default:
                    ChangeTheme($"UW{themeNo:D2}.WAV",Loop);
                    break;
            }
        }

        /// <summary>
        /// Changes the theme that is playing based on the .WAV filename.
        /// </summary>
        /// <param name="filename"></param>
        static void ChangeTheme(string filename, bool Loop = false)
        {   
            if (!playerdat.MusicEnabled)
            {
                return;
            }
            filename = filename.ToUpper().Replace(".XMI",".WAV");//since I keep putting in XMI instead of WAV this will always force the WAV version to play...
            //var Result = new AudioSample();
            var fullPath = Path.Combine(ProjectSettings.GlobalizePath("user://"), _RES.ToString(), "SOUND", filename); 
            if (File.Exists(fullPath))
            {                
                var stream = new AudioStreamWav();
                stream.Data = File.ReadAllBytes(fullPath);//TODO: cache these themes
                stream.Format = AudioStreamWav.FormatEnum.Format16Bits;
                stream.Stereo = true;
                stream.MixRate = SampleRate;
                main.instance.MusicPlayer.Stream = stream;
                LoopTheme = Loop;
                main.instance.MusicPlayer.Play(); 
            }
            else
            {
                Debug.Print($"Theme {filename} not found1");
            }
        }


        /// <summary>
        /// For UW1 the theme one of tracks 2 to 5, for UW2 the theme is selected based on the game world the player is in.
        /// </summary>
        public static byte PickLevelThemeMusic(int arg0 = -1)
        {
            {
               switch(_RES)
               {
                case GAME_UW2:
                    //playerdat.CurrentWorld;
                    if (CurrentWorldTheme != playerdat.CurrentWorld)
                        {
                            CurrentWorldTheme = playerdat.CurrentWorld;
                            arg0 = 0;
                        }
                    else
                        {
                            if (arg0 == -2 )
                            {
                                return CurrentThemeNo;//No change in theme. Probably this is triggered by a level change to the same world.
                            }
                        }
                    if (arg0 == -1)
                        {
                            arg0 = Rng.r.Next(3);
                        }
                    
                    //Look up table.
                    return UW2WorldThemes[(playerdat.CurrentWorld * 3) + arg0];
                default:
                    return (byte)(2 + Rng.r.Next(3));
               } 
            }
        }


        /// <summary>
        /// Converts XMI files to WAV files and saves them to appdata folder. If files already exist do nothing.
        /// </summary>
        public static void ConvertXMIMusic()
        {
            SetupDllLoader();
            //var toLoad = Path.Combine(BasePath, "SOUND", bankfile);
            var outputfolder = Path.Combine(ProjectSettings.GlobalizePath("user://"), _RES.ToString(), "SOUND");
            if (!Path.Exists(outputfolder))
            {
                System.IO.Directory.CreateDirectory(outputfolder);
            }
            var NewWoplPath = Path.Combine(outputfolder, bankfile.Replace(".", "_") + ".wopl");
            GlobalTimbreLibrary oplFile = ReadOpl(Path.Combine(BasePath, "SOUND", bankfile));
            WoplFile wopl = OplToWopl(oplFile);
            WriteWopl(wopl, NewWoplPath);

            byte[] bankData = File.ReadAllBytes(NewWoplPath);
            var filelist = System.IO.Directory.EnumerateFiles(Path.Combine(BasePath, "SOUND"),"*.XMI");
            foreach (var f in filelist)
            {
                var outputFile = f.GetFile().Replace(".XMI",".WAV");
                if (!System.IO.File.Exists( Path.Combine(outputfolder,outputFile )))
                {
                    ExportXMI(xmifile: f, outfile: Path.Combine(outputfolder,outputFile ), bankData: bankData);
                }
            }
            // for (int i = 0; i <= 45; i++)
            // {
            //     Console.WriteLine($"Dumping {i}");
            //     ExportXMI(i, bankData);
            // }
        }

        static void ExportXMI(string xmifile, string outfile, byte[] bankData)
        {
            //string path = $@"C:\Depot\bb\ualbion\Data\Exported\SONGS{songId / 100}.XLD\{songId % 100:D2}.xmi";
            if (!File.Exists(xmifile))
                return;

            using var outputFile = new WavFile(
                outfile,
                SampleRate, 2, 2);

            using var player = AdlMidi.Init();
   
            //player.SetNoteHook(NoteHook, IntPtr.Zero);
            player.OpenBankData(bankData);
            player.OpenFile(xmifile);
            player.SetLoopEnabled(false);
            short[] bufferArray = new short[4096];
            long totalSamples = 0;
            for (; ; )
            {
                _time = (double)totalSamples / (2 * SampleRate);
                int samplesWritten = player.Play(bufferArray);
                totalSamples += samplesWritten;

                if (samplesWritten <= 0)
                    break;

                var byteSpan = MemoryMarshal.Cast<short, byte>(new ReadOnlySpan<short>(bufferArray, 0, samplesWritten));
                outputFile.Write(byteSpan);
            }
        }

        static WoplFile OplToWopl(GlobalTimbreLibrary oplFile)
        {
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
                WoplInstrument x =
                    i < 128
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

            return wopl;
        }

        public sealed class WavFile : IDisposable
        {
            readonly FileStream _stream;
            readonly BinaryWriter _bw;
            readonly long _riffSizeOffset;
            readonly long _dataSizeOffset;
            uint _dataSize;

            public WavFile(string filename, uint sampleRate, ushort numChannels, ushort bytesPerSample)
            {
                _stream = File.Open(filename, FileMode.Create);
                _bw = new BinaryWriter(_stream);
                _bw.Write("RIFF"u8.ToArray()); // Container format chunk
                _riffSizeOffset = _stream.Position;
                _bw.Write(0); // Dummy write to start with, will be overwritten at the end.

                _bw.Write("WAVEfmt "u8.ToArray()); // Subchunk1 (format metadata)
                _bw.Write(16);
                _bw.Write((ushort)1); // Format = Linear Quantisation
                _bw.Write(numChannels); // NumChannels
                _bw.Write(sampleRate); // SampleRate
                _bw.Write(sampleRate * numChannels * bytesPerSample); // ByteRate
                _bw.Write((ushort)(numChannels * bytesPerSample)); // BlockAlign
                _bw.Write((ushort)(bytesPerSample * 8)); // BitsPerSample

                _bw.Write("data"u8.ToArray()); // Subchunk2 (raw sample data)
                _dataSizeOffset = _stream.Position;
                _bw.Write(0); // Dummy write, will be overwritten at the end
            }

            public void Write(ReadOnlySpan<byte> buffer)
            {
                _bw.Write(buffer);
                _dataSize += (uint)buffer.Length;
            }

            public void Dispose()
            {
                var totalLength = _stream.Position; // Write actual length to container format chunk
                _stream.Position = _riffSizeOffset;
                _bw.Write((uint)(totalLength - 8));

                _stream.Position = _dataSizeOffset;
                _bw.Write(_dataSize);

                _bw.Dispose();
                _stream.Dispose();
            }
        }
        /*
                static WoplFile ReadWopl(string filename)
                {
                    using var stream2 = File.OpenRead(filename);
                    using var br = new BinaryReader(stream2);
                    return WoplFile.Serdes(null, new GenericBinaryReader(br, br.BaseStream.Length, Encoding.ASCII.GetString, Console.WriteLine));
                }
        */

        static void WriteWopl(WoplFile wopl, string filename)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            WoplFile.Serdes(wopl, new WriterSerdes(bw, Console.WriteLine));
            byte[] bytes = ms.ToArray();
            File.WriteAllBytes(filename, bytes);
        }

        static GlobalTimbreLibrary ReadOpl(string filename)
        {
            using var stream = File.OpenRead(filename);
            using var br = new BinaryReader(stream);
            return GlobalTimbreLibrary.Serdes(
                null,
                new ReaderSerdes(
                    br,
                    br.BaseStream.Length,
                    Console.WriteLine));
        }


        static void SetupDllLoader()
        {
            NativeLibrary.SetDllImportResolver(
                typeof(AdlMidi).Assembly,
                (name, assembly, path) =>
            {
                var root = AppContext.BaseDirectory; //Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

                string filename;
                string runtime = RuntimeInformation.RuntimeIdentifier;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    filename = string.Equals(Path.GetExtension(name), ".DLL", StringComparison.OrdinalIgnoreCase)
                        ? name
                        : name + ".dll";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    filename = string.Equals(Path.GetExtension(name), ".SO", StringComparison.OrdinalIgnoreCase)
                        ? name
                        : name + ".so";
                }
                else throw new PlatformNotSupportedException();

                var fullPath = Path.Combine(root, "runtimes", runtime, "native", filename);
                return File.Exists(fullPath)
                    ? NativeLibrary.Load(fullPath)
                    : IntPtr.Zero;
            });
        }

    }//end class
}//end namespace