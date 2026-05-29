using System;
using System.Collections.Generic;
using System.IO;

namespace Underworld
{
    /// <summary>
    /// Data loader for loading binary files.
    /// </summary>
    public class DataLoader : Loader
    {
        /// <summary>
        /// System shock ark files.
        /// </summary>
        public struct Chunk
        {
            public long chunkUnpackedLength;
            public int chunkCompressionType;//compression type
            public long chunkPackedLength;
            public int chunkContentType;
            public byte[] data;
        };

        //Compression flags for UW2
        public const int UW2_NOCOMPRESSION = 0;
        public const int UW2_SHOULDCOMPRESS = 1;
        public const int UW2_ISCOMPRESS = 2;
        public const int UW2_HASEXTRASPACE = 4;


        /// <summary>
        /// Gets the value at the specified address in the file buffer and performs any necessary -endian conversions
        /// </summary>
        /// <returns>The value at address.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="Address">Address.</param>
        /// <param name="size">Size of the data in bits</param>
        public static long getValAtAddress(UWBlock buffer, long Address, int size)
        {//Gets contents of bytes the the specific integer address. int(8), int(16), int(32) per uw-formats.txt
            return getAt(buffer.Data, Address, size);
        }



        /// <summary>
        /// Unpacks the Uw2 compressed blocks
        /// </summary>
        /// <returns>The Uw2 uncompressed block.</returns>
        /// <param name="tmpBuffer">Tmp.</param>
        /// <param name="address_pointer">Address pointer.</param>
        /// <param name="datalen">Datalen.</param>
        /// Robbed and changed slightly from the Labyrinth of Worlds implementation project.
        ///This decompresses UW2 blocks.
        public static byte[] unpackUW2(byte[] tmpBuffer, int address_pointer, ref int datalen)
        {
            int BlockLen = (int)getAt(tmpBuffer, address_pointer, 32); //lword(base);
            int NoOfSegs = ((BlockLen / 0x1000) + 1) * 0x1000;
            byte[] buf = new byte[Math.Max(NoOfSegs, BlockLen + 100)];

            int upPtr = 0;
            datalen = 0;
            address_pointer += 4;

            while (upPtr < BlockLen)
            {
                byte bits = tmpBuffer[address_pointer++];
                for (int r = 0; r < 8; r++)
                {
                    if (address_pointer > tmpBuffer.GetUpperBound(0))
                    {//No more data!
                        return buf;
                    }
                    if ((bits & 1) == 1)
                    {//Transfer
                        buf[upPtr++] = tmpBuffer[address_pointer++];
                        datalen++;
                    }
                    else
                    {//copy
                        if (address_pointer >= tmpBuffer.GetUpperBound(0))
                        {
                            return buf;//no more data.
                        }
                        int o = tmpBuffer[address_pointer++];
                        int c = tmpBuffer[address_pointer++];

                        o |= ((c & 0xF0) << 4);
                        //if((o&0x800) == 0x800)
                        //	{//Apparently I need to turn this to twos complement when the sign bit is set. 
                        ///Possibly the code below is what achieves this?
                        //	o = (o | 0xFFFFF000);
                        //	//o = 0 & 0x7ff;
                        //	}


                        c = ((c & 15) + 3);
                        o += 18;

                        if (o > upPtr)
                        {
                            o -= 0x1000;
                        }

                        while (o < (upPtr - 0x1000))
                        {
                            o += 0x1000;
                        }

                        while (c-- > 0)
                        {
                            if (o < 0)
                            {
                                //int currentsegment = ((datalen/0x1000) + 1) * 0x1000;
                                //buf[upPtr++]= buf[buf.GetUpperBound(0) + o++];//This is probably very very wrong.
                                //buf[upPtr++]= buf[currentsegment + o++];//This is probably very very wrong.
                                buf[upPtr++] = 0;
                                o++;
                            }
                            else
                            {
                                buf[upPtr++] = buf[o++];
                            }

                            datalen++;    // = datalen+1;
                        }
                    }
                    bits >>= 1;
                }
            }
            return buf;
        }


        /// <summary>
        /// Loads the UW block.
        /// </summary>
        /// <returns><c>true</c>, if UW block was loaded, <c>false</c> otherwise.</returns>
        /// <param name="arkData">Ark data.</param>
        /// <param name="blockNo">Block no.</param>
        /// <param name="targetDataLen">Target data length.</param>
        /// <param name="uwb">Uwb.</param>
        public static bool LoadUWBlock(byte[] arkData, int blockNo, int targetDataLen, out UWBlock uwb)
        {
            uwb = new UWBlock();
            int NoOfBlocks = (int)getAt(arkData, 0, 32);
            switch (_RES)
            {
                case GAME_UW2:
                    {//6 + block *4 + (noOfBlocks*type)
                        uwb.Address = (int)getAt(arkData, 6 + (blockNo * 4), 32);
                        uwb.CompressionFlag = (int)getAt(arkData, 6 + (blockNo * 4) + (NoOfBlocks * 4), 32);
                        uwb.DataLen = (int)getAt(arkData, 6 + (blockNo * 4) + (NoOfBlocks * 8), 32);
                        uwb.ReservedSpace = (int)getAt(arkData, 6 + (blockNo * 4) + (NoOfBlocks * 12), 32);
                        if (uwb.Address != 0)
                        {
                            if (((uwb.CompressionFlag >> 1) & 0x01) == 1)
                            {//data is compressed;
                                uwb.Data = unpackUW2(arkData, uwb.Address, ref uwb.DataLen);
                                if (uwb.DataLen>0)
                                {
                                    Array.Resize(ref uwb.Data, uwb.DataLen);
                                }
                                return true;
                            }
                            else
                            {
                                uwb.Data = new byte[uwb.DataLen];
                                Buffer.BlockCopy(arkData, uwb.Address, uwb.Data, 0, uwb.DataLen);
                                return true;
                            }
                        }
                        else
                        {
                            uwb.DataLen = 0;
                            return false;
                        }
                    }
                default:
                    {
                        uwb.Address = (int)getAt(arkData, (blockNo * 4) + 2, 32);
                        if (uwb.Address != 0)
                        {
                            uwb.Data = new byte[targetDataLen];
                            uwb.DataLen = targetDataLen;
                            int b = 0;
                            for (long i = uwb.Address; i < Math.Min(uwb.Address + uwb.DataLen, arkData.GetUpperBound(0)+1); i++)
                            {//Copy the data to the block.
                                uwb.Data[b++] = arkData[i];
                            }
                            //File.WriteAllBytes($"c:\\temp\\unpacked_{blockNo}.dat", uwb.Data);
                            return true;
                        }
                        else
                        {
                            uwb.DataLen = 0;
                            return false;
                        }
                    }
            }
        }

        /// <summary>
        /// Extracts the specified masked bits from the position in the value.
        /// </summary>
        /// <returns>The bits.</returns>
        /// <param name="value">Value.</param>
        /// <param name="From">From.</param>
        /// <param name="Length">Length.</param>
        public static int ExtractBits(int value, int Shift, int Mask)
        {
            return (value >> Shift) & (Mask);
        }


        /// <summary>
        /// Gets a bit mask of the specified length.
        /// </summary>
        /// <returns>The mask.</returns>
        /// <param name="Length">Length.</param>
        static int getMask(int Length)
        {
            int mask = 0;
            for (int i = 0; i < Length; i++)
            {
                mask = (mask << 1) | 1;
            }
            return mask;
        }

        /// <summary>
        /// Insert the bits into the value.
        /// </summary>
        /// <returns>The bits.</returns>
        /// <param name="valueToChange">Value to change.</param>
        /// <param name="valueToInsert">Value to insert.</param>
        /// <param name="From">From.</param>
        /// <param name="Length">Length.</param>
        public static int InsertBits(int valueToChange, int valueToInsert, int From, int Length)
        {
            int mask = 0;
            for (int i = 0; i < 32; i++)//Create a 32 bit mask
            {
                if ((i >= From) && (i < From + Length))
                {//Mask in a 1
                    mask = (mask << 1) | 1;
                }
                else
                {//Mask in a zero
                    mask = (mask << 1) | 0;
                }
            }

            valueToInsert &= getMask(Length);//Make sure only the require bits are inserted.
            valueToChange = valueToInsert << From;

            return valueToChange;
        }
    } //end class
} //end namespace