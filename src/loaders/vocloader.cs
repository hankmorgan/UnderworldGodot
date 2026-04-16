using System.IO;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    public class AudioSample
    {

        public byte[] AudioBuffer;
        public short FreqDivisor;
        public short Codec;  //uw is 0 : 8 bits unsigned PCM 

        public int SampleRate
        {
            get
            {
                if (Sample16)
                {//16 bit freq
                    return 256000000 / ((NumChannels + 1) * (65536 - FreqDivisor));
                }
                else
                {//8 bit freq
                    return 1000000 / (256 - FreqDivisor);
                }
            }
        }

        //Based on specification in https://moddingwiki.shikadi.net/wiki/VOC_Format
        public string signature;
        public byte signature2;

        public short MainHeaderSize;
        public short Version;
        public short CheckSum;

        public short NumChannels = 0;

        public bool Sample16 = false; //Is the sample freq 16 bits instead of 8
        public AudioStreamWav toWav()
        {
            var wav = new AudioStreamWav();
            wav.Data = AudioBuffer;
            wav.Format = AudioStreamWav.FormatEnum.Format8Bits;
            wav.MixRate = SampleRate;
            return wav;
        }
    }

    public class vocLoader : Loader
    {
        public static AudioSample Load(string vocfile)
        {
            var Result = new AudioSample();
            string file = Path.Combine(BasePath, "SOUND", vocfile);

            if (File.Exists(file))
            {
                byte[] buffer;
                if (ReadStreamFile(file, out buffer))
                {
                    int addptr;
                    for (addptr = 0; addptr < 19; addptr++)
                    {
                        Result.signature += (char)buffer[addptr];
                    }
                    addptr = 19;
                    Result.signature2 = buffer[addptr++];
                    Result.MainHeaderSize = (short)getAt(buffer, addptr, 16);
                    addptr += 2;
                    Result.Version = (short)getAt(buffer, addptr, 16);  //Version number. Usually 0x010A (266d) for the old format
                    addptr += 2;
                    Result.CheckSum = (short)getAt(buffer, addptr, 16);
                    //Now at the start of the blocks
                    addptr = 26;

                    switch (buffer[addptr])
                    {
                        case 0: //terminate
                            Debug.Print("terminate");
                            return Result;

                        case 1: //Sound data with type
                            {
                                addptr++;
                                int BlockSize = (int)getAt(buffer, addptr, 24);
                                addptr += 3;
                                Result.FreqDivisor = buffer[addptr++];
                                Result.Codec = buffer[addptr++];
                                //then sound data.
                                int ubound = addptr + BlockSize;
                                int OutPtr = 0;
                                Result.AudioBuffer = new byte[BlockSize]; //What would happen here if there were more than 1 audio block. Should I resize the array if already existing
                                while ((addptr < ubound) && (addptr < buffer.Length))
                                {
                                    Result.AudioBuffer[OutPtr++] = ((byte)(buffer[addptr++] - 128));
                                }
                                return Result; //assuming only one sample block in uw1/uw2. Testing has only shown 1 block in these files
                            }
                        case 8:
                            {//Only Appears in UW2/sound/sp18
                                Result.Sample16 = true;
                                int BlockSize = (int)getAt(buffer, addptr, 24);
                                Result.FreqDivisor = (short)getAt(buffer, addptr, 16); //uses 16 bit instead of 8 as above
                                addptr += 2;
                                Result.Codec = buffer[addptr++];
                                // Debug.Print("Num Channels: " + buffer[addptr++].ToString());
                                Result.NumChannels = buffer[addptr++];
                                int ubound = addptr + BlockSize;
                                int OutPtr = 0;
                                Result.AudioBuffer = new byte[BlockSize]; //What would happen here if there were more than 1 audio block. Should I resize the array if already existing
                                while ((addptr < ubound) && (addptr < buffer.Length))
                                {
                                    Result.AudioBuffer[OutPtr++] = ((byte)(buffer[addptr++] - 128));
                                }
                                return Result;
                            }


                        default:
                            Debug.Print("Unimplemented block type." + buffer[addptr] + " Read the specifications!");
                            return null;
                    }
                }
            }
            else
            {
                Debug.Print($"unable to find sfx {vocfile}");
            }
            return Result;
        }
    }//end class


    public class sounddat : Loader
    {
        static byte[] buffer;
        public static sounddat[] SoundData;
        int index;
        int blocksize
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return 8;
                }
                else
                {
                    return 5;
                }
            }
        }

        /// <summary>
        /// Byte 0 in sound.dat record.
        /// </summary>
        public byte tvfx_num
        {
            get
            {
                return (byte)getAt(buffer, (index * blocksize) + 1 + 0, 8); //skip over first byte.               
            }
        }

        public byte note
        {
            get
            {
                return (byte)getAt(buffer, (index * blocksize) + 1 + 2, 8); //skip over first byte.               
            }
        }


        /// <summary>
        /// Aka loudness
        /// </summary>
        public byte velocity
        {
            get
            {
                return (byte)getAt(buffer, (index * blocksize) + 1 + 2, 8); //skip over first byte.               
            }
        }

        public byte pan
        {
            get
            {
                return (byte)getAt(buffer, (index * blocksize) + 1 + 3, 16);
            }
        }

        public byte unk5
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return (byte)getAt(buffer, (index * blocksize) + 1 + 5, 8);
                }
                else
                {
                    return 0;
                }
            }
        }

        public byte unk6
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return (byte)getAt(buffer, (index * blocksize) + 1 + 6, 16);
                }
                else
                {
                    return 0;
                }
            }
        }

        static sounddat()
        {
            var path = System.IO.Path.Combine(Loader.BasePath, "SOUND", "SOUNDS.DAT");
            if (System.IO.File.Exists(path))
            {
                buffer = File.ReadAllBytes(path);
                var noOfSounds = getAt(buffer, 0, 8);
                SoundData = new sounddat[noOfSounds];
                for (int i = 0; i <= SoundData.GetUpperBound(0); i++)
                {
                    SoundData[i] = new sounddat(i);
                }
            }
            else
            {
                Debug.Print($"File not found. {path}");
            }

        }

        public sounddat(int _index)
        {
            index = _index;
        }
    }
    /// <summary>
    /// Placeholder to start including sfx calls in code
    /// </summary>
    public class soundeffects : UWClass
    {

        static bool LoadAndPlayVocFile(int EffectNo, int volumerelatedarg2, int arg4)
        {
            if (_RES == GAME_UW2)
            {
                var sound = vocLoader.Load(
                            System.IO.Path.Combine(
                                BasePath, "SOUND",
                                $"SP{EffectNo:0#}.VOC"));

                if (sound.AudioBuffer != null)
                {
                    //TODO: only one audio player is set up so far. Integrate with better sound output methods
                    main.instance.DigitalAudioPlayer.Stream = sound.toWav();
                    main.instance.DigitalAudioPlayer.Play();
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// UW2 version of load basic sound.
        /// </summary>
        /// <param name="EffectNo"></param>
        /// <param name="tvfx_num_arg2"></param>
        /// <param name="note_arg4"></param>
        /// <param name="velocity_arg6"></param>
        /// <param name="arg8"></param>
        /// <param name="pan_arga"></param>
        /// <returns></returns>
        static bool LoadBasicSound(int EffectNo, byte tvfx_num_arg2, byte note_arg4, sbyte velocity_arg6, byte arg8, byte pan_arga)
        {
            return false;
        }

        /// <summary>
        /// Plays a sound that the avatar has generated themselves. Eg eating noises.
        /// </summary>
        /// <param name="effectno"></param>
        /// <param name="arg2"></param>
        /// <param name="arg4"></param>
        public static void PlaySoundEffectAtAvatar(byte effectno, byte arg2, byte arg4)
        {
            sbyte var2_velocity;
            if (_RES == GAME_UW2)
            {
                if (playerdat.SoundEffectsEnabled)
                {

                    if (playerdat.SoundEffectsEnabled)
                    {
                        if (effectno != 0xFF)
                        {
                            if (effectno <= 0x63)
                            {
                                //Seg_16_1DE2
                                //Lookup sound effect in sounds.dat
                                var2_velocity = (sbyte)(arg4 + sounddat.SoundData[effectno].velocity);
                                if (var2_velocity > 0x7F)
                                {
                                    var2_velocity = 0x7F;
                                }
                                else
                                {
                                    if (var2_velocity < 0)
                                    {
                                        var2_velocity = 0;
                                    }
                                }
                                if (var2_velocity == 0)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                var2_velocity = 0x7F;
                            }
                            //seg16_1E2B
                            if (LoadAndPlayVocFile(effectno, var2_velocity, arg2))
                            {
                                //in disassembly a global value is set to 0 here? possibly controlling if the sound is directional?
                            }
                            else
                            {
                                //fall back to basic sound
                                if (effectno <= 0x63)
                                {
                                    //Load basic sound.
                                    LoadBasicSound(
                                        EffectNo: effectno,
                                        tvfx_num_arg2: sounddat.SoundData[effectno].tvfx_num,
                                        note_arg4: sounddat.SoundData[effectno].note,
                                        velocity_arg6: var2_velocity,
                                        arg8: arg2,
                                        pan_arga: sounddat.SoundData[effectno].pan);
                                }
                            }

                        }
                    }
                }
            }
            else
            {
                //uw1 logic (only supports basic sounds)
            }
        }

        /// <summary>
        /// Generate a sound that comes from an object. Eg a door opening/closing
        /// </summary>
        /// <param name="effectNo"></param>
        /// <param name="obj"></param>
        /// <param name="arg6"></param>
        public static void PlaySoundEffectAtObject(byte effectNo, uwObject obj, int arg6)
        {
            PlaySoundEffectAtCoordinate(effectNo, obj.tileX, obj.tileY, arg6);
        }

        /// <summary>
        /// Generate a sound that comes from a location. Eg a missile impacting on a target.
        /// </summary>
        /// <param name="effectNo"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="arg6"></param>
        public static void PlaySoundEffectAtCoordinate(byte effectNo, int x, int y, int arg6)
        {
            var var8 = CalculateSoundFallOff(x, y, 0, 0);
            //TODO.. all this stuff.
            PlaySoundEffectAtAvatar(effectNo, 0, 0);
        }




        /// <summary>
        /// Based on values in sounds.dat work out how loud the sound should be at a distance.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="SoundDatByte"></param>
        /// <param name="arg6"></param>
        /// <returns></returns>
        public static int CalculateSoundFallOff(int x, int y, int SoundDatByte, int arg6)
        {
            return 0x7F;
        }
    }

}//end namespace