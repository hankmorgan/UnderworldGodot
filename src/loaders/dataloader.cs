using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

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
            //byte[] buf = new byte[BlockLen+100];
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
        /// Repacks a byte array into a UW2 compressed block
        /// </summary>
        /// <param name="srcData">Source data.</param>
        /// 
        ///for each byte in src data
        ///read in byte.
        ///if byte count >3
        ///  if find byte in previous data
        ///if previous data header is a copy record then increment its size (up to max of 18)
        ///else create a new copy record.
        /// this is a transfer record
        ///else
        ///this is a transfer record
        public static byte[] RepackUW2(byte[] srcData)
        {
            List<byte> Input = new List<byte>();
            List<byte> Output = new List<byte>();
            int addptr = 0;
            int bit = 0;
            //int PrevMatchingOffset=-1; 
            //int CopyRecordOffset=0;
            int copycount = 0; int HeaderIndex = 0;
            while (addptr <= srcData.GetUpperBound(0))
            {
                //Read in the data to the input list
                Input.Add(srcData[addptr++]);
                if (Input.Count > 3)//One I have at least 3 bytes I can test its contents
                {//THIS IS WRONG> Code will only match up to size 3.
                //At this point I need to start testing increasing sizes of data up to 18 bytes until I find the max copy record to create;
                    if (FindMatchingSequence(ref Output, ref Input, out int MatchingOffset))
                    {//the data is part of a copy sequence. Try and find the biggest block and make a copy record out of that.
                    //TODO
                    //
                        /*	if (MatchingOffset == PrevMatchingOffset )	
                            {//Increment copy count
                                copycount++;
                                IncrementCopyRecord(ref Output, CopyRecordOffset);
                                if (copycount>=18)
                                {
                                    PrevMatchingOffset=-1; //force a new copy record on next loop	
                                    Input.Clear();
                                }								
                            }
                            else
                            {//Create copy record
                                copycount=0;
                                if (bit==0)
                                {
                                        HeaderIndex=CreateHeader(ref Output);	
                                }
                                CopyRecordOffset = CreateCopyRecord(ref Output, MatchingOffset, copycount, HeaderIndex, bit++ );
                            }*/
                    }
                    else
                    {//There is no copy for the specified data. Transfer data that is not copied and clear out the input buffer
                        for (int i = copycount; i < Input.Count; i++)
                        {
                            if (bit == 0)
                            {
                                HeaderIndex = CreateHeader(ref Output);
                            }
                            CreateTransferRecord(ref Output, Input[i], HeaderIndex, bit++);
                        }
                        Input.Clear();
                    }
                }
                if (bit == 8)
                {
                    bit = 0;
                }
            }

            //Write the data to a file.

            WriteListToBytes(Output, Path.Combine(BasePath, "DATA", "recodetest.dat"));
            byte[] outchar = new byte[Output.Count];
            for (int i = 0; i < Output.Count; i++)
            {
                outchar[i] = Output[i];
            }
            return outchar;
        }

        static int CreateHeader(ref List<byte> Output)
        {
            Output.Add((byte)0);
            return Output.Count - 1;
        }

        static void WriteListToBytes(List<byte> Output, string path)
        {
            FileStream file = File.Open(path, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            WriteInt32(writer, Output.Count);
            for (int i = 0; i < Output.Count; i++)
            {
                WriteInt8(writer, Output[i]);
            }
            writer.Close();
        }

        static void CreateTransferRecord(ref List<byte> Output, byte TransferData, int headerIndex, int bit)
        {
            Output[headerIndex] = (byte)(Output[headerIndex] | (1 << bit));
            Output.Add(TransferData);
        }

        static int CreateCopyRecord(ref List<byte> Output, int Offset, int CopyCount, int headerIndex, int bit)
        {
            //The copy record starts with two Int8's:
            //0000   Int8   0..7: position
            //0001   Int8   0..3: copy count
            //				4..7: position
            //int val = Offset
            Output[headerIndex] = (byte)(Output[headerIndex] | (0 << bit));

            //Offset= getTrueOffset(Offset);
            //CopyCount-=3;
            int val1 = Offset & 0xF;
            int val2 = (CopyCount & 0xf) | ((Offset >> 7) << 4);

            Output.Add((byte)val1);
            Output.Add((byte)val2);
            return Output.Count - 1;
        }

        static void IncrementCopyRecord(ref List<byte> Output, int CopyRecordOffset)
        {
            int val = getCopyCountAtOffset(ref Output, CopyRecordOffset);
            val++;
            val &= 0xF;
            byte chardata = (byte)(Output[CopyRecordOffset] & 0xf8);//Clear the bits for the count.
            chardata = (byte)(chardata | val);
            Output[CopyRecordOffset] = chardata;
        }

        static int getCopyCountAtOffset(ref List<byte> Output, int CopyRecordOffset)
        {
            if (CopyRecordOffset < 0)
            {
                return 0;
            }
            else
            {
                int val = Output[CopyRecordOffset];
                val &= 0xF;//Extract copy count
                return val;
            }
        }

        static bool FindMatchingSequence(ref List<byte> records, ref List<byte> searchfor, out int MatchingOffset)
        {
            int lowerbound = (records.Count / 4096) * 4096;
            string searchval = charListToVal(searchfor, 0, searchfor.Count);
            MatchingOffset = -1;
            for (int i = records.Count - searchfor.Count; i >= lowerbound; i--)
            {
                string recordval = charListToVal(records, i, searchfor.Count - 1);
                if (recordval == searchval)
                {
                    MatchingOffset = i;
                    return true;
                }
            }
            return false;
        }

        static string charListToVal(List<byte> input, int start, int len)
        {
            string output = "";
            for (int i = start; i <= start + len; i++)
            {
                if (i < input.Count)
                {
                    output += input[i].ToString();
                }
            }
            return output;
        }


        /// <summary>
        /// Gets the true offset of the copy record.
        /// </summary>
        /// <returns>The true offset.</returns>
        /// <param name="offset">Offset.</param>
        static int getTrueOffset(int offset)
        {
            while (offset > 4096)
            {
                offset -= 4096;
            }
            offset -= 18;

            return offset;
        }

      
        /// <summary>
        /// Writes an int8 to a file
        /// </summary>
        /// <param name="writer">Writer.</param>
        /// <param name="val">Value.</param>
        public static long WriteInt8(BinaryWriter writer, long val)
        {
            byte valOut = (byte)(val & 0xff);
            writer.Write(valOut);

            return 1;
        }


        /// <summary>
        /// Writes an int16 to a file
        /// </summary>
        /// <param name="writer">Writer.</param>
        /// <param name="val">Value.</param>
        public static long WriteInt16(BinaryWriter writer, long val)
        {
            byte valOut = (byte)(val & 0xff);
            writer.Write(valOut);

            valOut = (byte)(val >> 8 & 0xff);
            writer.Write(valOut);

            return 2;
        }

        /// <summary>
        /// Writes an int32 to file
        /// </summary>
        /// <param name="writer">Writer.</param>
        /// <param name="val">Value.</param>
        public static long WriteInt32(BinaryWriter writer, long val)
        {
            byte valOut = (byte)(val & 0xff);
            writer.Write(valOut);

            valOut = (byte)(val >> 8 & 0xff);
            writer.Write(valOut);

            valOut = (byte)(val >> 16 & 0xff);
            writer.Write(valOut);

            valOut = (byte)(val >> 24 & 0xff);
            writer.Write(valOut);

            return 4;
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
                                File.WriteAllBytes($"c:\\temp\\unpacked_{blockNo}.dat", uwb.Data);
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
                            for (long i = uwb.Address; i < uwb.Address + uwb.DataLen; i++)
                            {//Copy the data to the block.
                                uwb.Data[b++] = arkData[i];
                            }
                            File.WriteAllBytes($"c:\\temp\\unpacked_{blockNo}.dat", uwb.Data);
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