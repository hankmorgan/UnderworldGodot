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
                Debug.Print ($"unable to find sfx {vocfile}");
            }
            return Result;
        }
    }//end class

    /// <summary>
    /// Placeholder to start including sfx calls in code
    /// </summary>
    public class soundeffects : UWClass
    {
        /// <summary>
        /// The sound is played at the location of the avatar.
        /// </summary>
        /// <param name="effectno"></param>
        /// <param name="arg2"></param>
        /// <param name="arg4"></param>
        public static void PlaySoundEffectAtAvatar(byte effectno, byte arg2, byte arg4)
        {
            if (playerdat.SoundEffectsEnabled)
            {
                //Only UW2 voc support so far
                if (_RES == GAME_UW2)
                {
                    if (effectno != 0xFF)
                    {
                        string filepath;
                        if (effectno>=100)
                        {
                            //guardian laughter
                            filepath = System.IO.Path.Combine(
                                BasePath, "SOUND",
                                $"UW{effectno-100:0#}.VOC");
                        }
                        else
                        {
                            filepath = System.IO.Path.Combine(
                                BasePath, "SOUND",
                                $"SP{effectno:0#}.VOC");
                        }
                        
                        if (File.Exists(filepath))
                        {
                            Debug.Print($"Playing sound {filepath}");
                            var sound = vocLoader.Load(
                                    System.IO.Path.Combine(
                                        BasePath, "SOUND",
                                        $"SP{effectno:0#}.VOC"));
                            if (sound.AudioBuffer != null)
                            {                                
                                //TODO: only one audio player is set up so far. Integrate with better sound output methods
                                //TODO: calculations on sound falloff need to be made.
                                main.instance.DigitalAudioPlayer.Stream = sound.toWav();
                                main.instance.DigitalAudioPlayer.Play();
                            }
                        }
                        else
                        {
                            //fallback to midi sound if not .voc file.
                            Sfx.SoundEffects.Play(effectno);
                        }

                    }
                }
                else
                {
                    //UW1
                    Sfx.SoundEffects.Play(effectno);
                }
            }
        }

        public static void PlaySoundEffectAtObject(byte effectNo, uwObject obj, int arg6)
        {
            PlaySoundEffectAtCoordinate(effectNo, obj.tileX, obj.tileY, arg6);
        }

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