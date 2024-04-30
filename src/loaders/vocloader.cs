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
            return Result;
        }
    }//end class

}//end namespace