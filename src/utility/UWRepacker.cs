using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Bridge;

namespace Underworld
{

    /// <summary>
    /// For repacking of UW2 ark files
    /// </summary>
    public class Repacker : Loader
    {

        static byte[] packed;


        /// <summary>
        /// Launch repack and test if it matches the original compressed data.
        /// </summary>
        /// <param name="blockNoToCompress"></param>
        public static void TestRepack(int blockNoToCompress)
        {
            LevArkLoader.LoadLevArkFileData();
            bool Pass = true;
            var lev_ark_block = LevArkLoader.LoadLevArkBlock(blockNoToCompress);
            if (((lev_ark_block.CompressionFlag >> 1) & 0x01) == 1)
            {
                byte[] repacked;
                Pass = CompressData(lev_ark_block, out repacked);
                if (Pass)
                {
                    for (int i = 0; i <= lev_ark_block.Data.GetUpperBound(0); i++)
                    {
                        if (i <= repacked.GetUpperBound(0))
                        {
                            if (repacked[i] != lev_ark_block.Data[i])
                            {
                                Debug.Print($"Data mismatch at offset {i}");
                                Pass = false;
                                break;
                            }
                        }
                        else
                        {
                            Debug.Print("Out of bounds error on repacked block.");
                            Pass = false;
                            break;
                        }
                    }
                }
                else
                {
                    Debug.Print("Result of compression is false");
                    Pass = false;
                }
            }
            else
            {
                Debug.Print("Test file is not a compressed Ark file");
                Pass = false;
            }
            if (Pass)
            {
                Debug.Print("Repack has suceeded. What a day!");
            }
            else
            {
                Debug.Print("Repack has failed. Try try agin.!");
            }
        }


        static bool CompressData(UWBlock InputArk_arg0, out byte[] packed, int MaybeMaxSize_argA = 0x8DD0, int argc_datasize = 0x7E08)
        {
            short ReadDataBufferPtrArg0 = 0;
            short OutputBufferPtr = 0;
            packed = new byte[0x40000]; //to confirm size requirements. possibly should be  0x722f (29231d). Ref GetOffsetsForCompression_ovr153_
            //Do stuff with the data to repack it.
            var Packed = new UWBlock();
            Packed.Data = new byte[5];


            //implementation
            var WriteAddress_arg8 = 0;
            var WriteAddress_var12 = WriteAddress_arg8;
            //set some values first in CompressedData.
            //ovr_127_7C3
            var var14 = MaybeMaxSize_argA; //probably not needed in this implementation.

            setAt16(buffer: packed, address: OutputBufferPtr + 1, val: 0);
            setAt16(buffer: packed, address: OutputBufferPtr + 3, val: 0);
            setAt16(buffer: packed, address: OutputBufferPtr + 5, val: 0);
            setAt16(buffer: packed, address: OutputBufferPtr + 7, val: 0);
            setAt16(buffer: packed, address: OutputBufferPtr + 9, val: 0);
            setAt16(buffer: packed, address: OutputBufferPtr + 0xB, val: 0);
            setAt8(buffer: packed, address: OutputBufferPtr + 0xD, val: 1);
            setAt16(buffer: packed, address: OutputBufferPtr + 0x722B, val: 0xFFFF);

            short si = 0x1001;
            //ovr_127_82F
            while (si <= 0x1100)
            {
                setAt16(buffer: packed, address: OutputBufferPtr + 0x3027 + (si << 1), val: 0x1000);
                si++;
            }
            si = 0;

            //ovr_127_84B
            while (si < 0x1000)
            {
                setAt16(buffer: packed, address: OutputBufferPtr + 0x5229 + (si << 1), val: 0x1000);
                si++;
            }

            byte[] var28 =  new byte[12]; //guess at size passed on next var allocated is at 0x16
            byte varb = 1;
            short varA = 0;// index into var28
            var di = 0;
            short var6 = 0xFEE;
            si = 0;

            //ovr_127_874
            while (si < var6)
            {
                setAt8(buffer: packed, address: OutputBufferPtr + si + 0x12, val: 0x20);
                si++;
            }
            short ByteCounter_var4 = 0;

            while (ByteCounter_var4 < 0x12)
            {
                ushort CurrentByte_var2;
                byte DataBufferByte_varE;
                if (argc_datasize == 0)
                {
                    CurrentByte_var2 = 0xFFFF;
                }
                else
                {
                    argc_datasize--;
                    //read in the next byte from the input data
                    DataBufferByte_varE = (byte)getAt(InputArk_arg0.Data, ReadDataBufferPtrArg0, 8);
                    ReadDataBufferPtrArg0++;
                    CurrentByte_var2 = DataBufferByte_varE;
                }

                if (CurrentByte_var2 != 0xFFFF)
                {
                    //ovr127_880
                    setAt8(buffer: packed, address: OutputBufferPtr + var6 + ByteCounter_var4 + 0x12, val: CurrentByte_var2);
                    //ovr127_88D
                    ByteCounter_var4++;
                }
                else
                {
                    break; // to ovr127_8be
                }
            }

            //ovr127_8be
            setAt32(buffer: packed, address: OutputBufferPtr + 1, val: (int)ByteCounter_var4);

            if (ByteCounter_var4 != 0)
            {
                si = 1;
                while (si <= 0x12)
                {
                    DataCompressionSubfunction_127_0((short)(var6 - si));
                }
                //ovr127_8f0
                DataCompressionSubfunction_127_0(var6);

                //ovr127_8f5
                if (getAt(packed, OutputBufferPtr + 0x10, 16) > ByteCounter_var4)
                {
                    //ovr127_902
                    setAt16(buffer: packed, address: OutputBufferPtr + 0x10, val: ByteCounter_var4);
                }
                //ovr127_909
                if (getAt(packed, OutputBufferPtr + 0x10, 16) > 2)
                {
                    //copy
                    //ovr127_92D
                    //ovr127_93D
                    var28[varA] = (byte)getAt(packed, Address: OutputBufferPtr + 0xE, size: 8);
                    varA++;

                    //ovr127_958
                    var ax = getAt(packed, OutputBufferPtr + 0xE, 16);
                    ax = ax>>4;
                    byte al = (byte)(ax & 0xF0);
                    var dl = (byte)getAt(packed, OutputBufferPtr + 0x10, 8);
                    dl = (byte)(dl + 0xFD);//should this calc as -3
                    al = (byte)(al | dl);

                    //ovr127_958
                    var28[varA] = al;
                }
                else
                {
                    //transfer
                    //ovr127_914
                    setAt16(packed, OutputBufferPtr + 0x10, 1);
                    var28[0] |= varb;
                    OutputBufferPtr += var6;
                    
                    //ovr127_958
                    var28[varA] = (byte)getAt(packed, OutputBufferPtr + 0x12, 8);   
                }
                //ovr127_958->> ovr 127_962
                varA++;
                varb= (byte)(varb << 1);

                if (varb == 0)
                {
                    si = 0;
                    while (si<varA)
                    {
                        //ovr127_976
                        if (WriteAddress_var12>= var14) //could be var16 instead/
                        {
                            //ovr127_992
                        }
                        else
                        {
                            //ovr127_97E
                            
                        }
                        //ovr127_9E8

                        si++;    
                    }
                    //ovr127_9EE

                }

                //ovr127_A0C



            }
            else
            {
                //ovr 127_c0b;
                return false;
            }







            return true;


        }

        static void DataCompressionSubfunction_127_23C(short arg0)
        {

        }

        static void DataCompressionSubfunction_127_0(short arg0)
        {

        }
    }
}