using System;
using System.Linq;

namespace Underworld
{
    //Utility code for playerdat
    public partial class playerdat : Loader
     {

        /// <summary>
        /// player.dat data buffer
        /// </summary>
        private static byte[] pdat;
        /// <summary>
        /// Size of encrypted area in UW1 player.dat
        /// </summary>
        private const int NoOfEncryptedBytes = 0xD2;

        /// <summary>
        /// Where inventory starts in playerdat
        /// </summary>
        public static int InventoryPtr
        {
            get
            { 
                if (_RES == GAME_UW2)
                {
                    return 0x3E3;
                }
                else
                {
                    return 0x138;
                }

            }
        }
        public static void InitEmptyPlayer(string new_charname="Gronk")
        {
            var InventoryPtr = 0x138;
            if (_RES == GAME_UW2)
            {
                InventoryPtr = 0x3E3;
            }
            pdat = new byte[InventoryPtr+1];
               
            Array.Resize(ref pdat, InventoryPtr + 512 * 8);
            CharName = new_charname;            
        }

        // Func load pdat
        public static void Load(string folder)
        {
            var path = System.IO.Path.Combine(BasePath, folder, "PLAYER.DAT");
            byte[] encoded;
            if (ReadStreamFile(path, out encoded))
            {
                int xOrValue = (int)encoded[0];
                switch (_RES)
                {
                    case GAME_UW2:
                        {
                            //uw2 encoding                                            
                            pdat = EncryptDecryptUW2(encoded, (byte)xOrValue);
                            break;
                        }
                    default:
                        {
                            //uw1 decoding
                            pdat = EncryptDecryptUW1(encoded, xOrValue);
                            break;
                        }
                }
               
                //Copy and initialise inventory
                var CurrentInventoryPtr = InventoryPtr;
                var origUbound = pdat.GetUpperBound(0);
                Array.Resize(ref pdat, InventoryPtr + 512 * 8);
                int oIndex = 1; //starts at one since there is no object zero
                //InventoryBuffer = new byte[512*8];
                //LastItemIndex=0;

                while (CurrentInventoryPtr < origUbound)
                {
                    // for (int i =0; i<8; i++)
                    // {//Copy bytes into storage
                    //     InventoryBuffer[i + oIndex * 8 ] = pdat[InventoryPtr + i];
                    // }
                    //Create new objects for the object list
                    var uwobj = new uwObject
                    {
                        isInventory = true,
                        IsStatic = true,
                        index = (short)(oIndex),
                        PTR = CurrentInventoryPtr,
                        DataBuffer = pdat
                    };
                    //Debug.Print($"{GameStrings.GetObjectNounUW(uwobj.item_id)}");
                    InventoryObjects[oIndex] = uwobj;
                    oIndex++;
                    CurrentInventoryPtr += 8;
                    //LastItemIndex++;                    
                }
            }
        }   //end load

        /// <summary>
        /// Encrypts or decrypts a UW1 player dat file.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="xOrValue"></param>
        public static byte[] EncryptDecryptUW1(byte[] buffer, int xOrValue)
        {
            int incrnum = 3;
            var output = buffer.ToArray();
            for (int i = 1; i <= NoOfEncryptedBytes; i++)
            {
                if ((i == 81) | (i == 161))
                {
                    incrnum = 3;
                }
                output[i] ^= (byte)((xOrValue + incrnum) & 0xFF);
                incrnum += 3;
            }
            return output;
        } //end decrypt uw1

        // Func decode/encode UW2
        /// <summary>
        /// Encrypts or decrypts a UW2 player dat file.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="xOrValue"></param>
        public static byte[] EncryptDecryptUW2(byte[] pDat, byte MS)
        {
            int[] MA = new int[80];
            MS += 7;
            for (int i = 0; i < 80; ++i)
            {
                MS += 6;
                MA[i] = MS;
            }
            for (int i = 0; i < 16; ++i)
            {
                MS += 7;
                MA[i * 5] = MS;
            }
            for (int i = 0; i < 4; ++i)
            {
                MS += 0x29;
                MA[i * 12] = MS;
            }
            for (int i = 0; i < 11; ++i)
            {
                MS += 0x49;
                MA[i * 7] = MS;
            }
            byte[] buffer = new byte[pDat.GetUpperBound(0) + 1];
            int offset = 1;
            int byteCounter = 0;
            for (int l = 0; l <= 11; l++)
            {
                buffer[0 + offset] = (byte)(pDat[0 + offset] ^ MA[0]);
                byteCounter++;
                for (int i = 1; i < 0x50; ++i)
                {
                    if (byteCounter < 0x37D)
                    {
                        buffer[i + offset] = (byte)(((pDat[i + offset] & 0xff) ^ ((buffer[i - 1 + offset] & 0xff) + (pDat[i - 1 + offset] & 0xff) + (MA[i] & 0xff))) & 0xff);
                        byteCounter++;
                    }
                }
                offset += 80;
            }
            //Copy the remainder of the plaintext data
            while (byteCounter <= pDat.GetUpperBound(0))
            {
                buffer[byteCounter] = pDat[byteCounter];
                byteCounter++;
            }
            buffer[0] = pDat[0];
            return buffer;
        } //end decrypt uw2
        public static byte GetAt(int index)
        {
            return pdat[index];
        }
        public static int GetAt16(int index)
        {
            return (int)getAt(pdat, index, 16);
        }
        public static int GetAt32(int index)
        {
            return (int)getAt(pdat, index, 32);
        }
        public static void SetAt(int index, byte value)
        {
            pdat[index] = value;
        }
        public static void SetAt16(int index, int value)
        {
            setAt(pdat, index, 16, value);
        }
        public static void SetAt32(int index, int value)
        {
            setAt(pdat, index, 32, value);
        }

     }
}//end namespace