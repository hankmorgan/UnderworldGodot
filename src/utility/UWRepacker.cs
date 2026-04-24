using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    /// <summary>
    /// For repacking of UW2 ark files
    /// </summary>
    public class Repacker : Loader
    {


        static byte[] packed;
        static byte[] ArkWorkData = new byte[0x40000];

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
                byte[] repacked = new byte[1];//todo replace with compress output.
                Pass = CompressData(lev_ark_block);
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


        static bool CompressData(UWBlock InputArk_arg0, int MaybeMaxSize_argA = 0x8DD0, int argc_datasize = 0x7E08)
        {
            short ReadDataBufferPtrArg0 = 0;
            short ArkWorkDataPtr = 0;
            ArkWorkData = new byte[0x40000]; //to confirm size requirements. possibly should be  0x722f (29231d). Ref GetOffsetsForCompression_ovr153_
            //Do stuff with the data to repack it.
            var Packed = new UWBlock();
            Packed.Data = new byte[5];
            ushort CurrentByte_var2;

            //implementation
            var WriteAddress_arg8 = 0;
            var WriteAddress_var12 = WriteAddress_arg8;
            //set some values first in CompressedData.
            //ovr_127_7C3
            var var14 = MaybeMaxSize_argA; //probably not needed in this implementation.

            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 1, val: 0);
            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 3, val: 0);
            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 5, val: 0);
            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 7, val: 0);
            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 9, val: 0);
            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 0xB, val: 0);
            setAt8(buffer: ArkWorkData, address: ArkWorkDataPtr + 0xD, val: 1);
            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 0x722B, val: 0xFFFF);

            short si = 0x1001;
            //ovr_127_82F
            while (si <= 0x1100)
            {
                setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 0x3027 + (si << 1), val: 0x1000);
                si++;
            }
            si = 0;

            //ovr_127_84B
            while (si < 0x1000)
            {
                setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 0x5229 + (si << 1), val: 0x1000);
                si++;
            }

            byte[] var28 =  new byte[12]; //guess at size passed on next var allocated is at 0x16
            byte varb = 1;
            short varA = 0;// index into var28
            short di = 0;
            short var6 = 0xFEE;
            si = 0;

            //ovr_127_874
            while (si < var6)
            {
                setAt8(buffer: ArkWorkData, address: ArkWorkDataPtr + si + 0x12, val: 0x20);
                si++;
            }
            short ByteCounter_var4 = 0;

            while (ByteCounter_var4 < 0x12)
            {
                
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
                    setAt8(buffer: ArkWorkData, address: ArkWorkDataPtr + var6 + ByteCounter_var4 + 0x12, val: CurrentByte_var2);
                    //ovr127_88D
                    ByteCounter_var4++;
                }
                else
                {
                    break; // to ovr127_8be
                }
            }

            //ovr127_8be
            setAt32(buffer: ArkWorkData, address: ArkWorkDataPtr + 1, val: (int)ByteCounter_var4);

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
            ovr127_8f5:
                if (getAt(ArkWorkData, ArkWorkDataPtr + 0x10, 16) > ByteCounter_var4)
                {
                    //ovr127_902
                    setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 0x10, val: ByteCounter_var4);
                }
                //ovr127_909
                if (getAt(ArkWorkData, ArkWorkDataPtr + 0x10, 16) > 2)
                {
                    //copy
                    //ovr127_92D
                    //ovr127_93D
                    var28[varA] = (byte)getAt(ArkWorkData, Address: ArkWorkDataPtr + 0xE, size: 8);
                    varA++;

                    //ovr127_958
                    var ax_temp = getAt(ArkWorkData, ArkWorkDataPtr + 0xE, 16);
                    ax_temp = ax_temp>>4;
                    byte al = (byte)(ax_temp & 0xF0);
                    var dl = (byte)getAt(ArkWorkData, ArkWorkDataPtr + 0x10, 8);
                    dl = (byte)(dl + 0xFD);//should this calc as -3
                    al = (byte)(al | dl);

                    //ovr127_958
                    var28[varA] = al;
                }
                else
                {
                    //transfer
                    //ovr127_914
                    setAt16(ArkWorkData, ArkWorkDataPtr + 0x10, 1);
                    var28[0] |= varb;
                    ArkWorkDataPtr += var6;
                    
                    //ovr127_958
                    var28[varA] = (byte)getAt(ArkWorkData, ArkWorkDataPtr + 0x12, 8);   
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
                            //write out some data to a tmp file.
                            //If no of bytes written out <> expected bytes
                            // return error

                            //ovr127_9C

                            //todo update write pointers
                        }
                        else
                        {
                            //ovr127_97E
                            //todo update write pointer with value from var28 array 
                            //new add = var28[si];
                            //var12++
                            
                        }
                        //ovr127_9E8

                        si++;    //loop back to ovr127_976
                    }

                    //ovr127_9EE
                    var tmp = (short)getAt(ArkWorkData, ArkWorkDataPtr + 5,32);
                    tmp += varA;
                    setAt32(ArkWorkData, ArkWorkDataPtr + 5, tmp);

                    var28[0] = 0;
                    varb = 1;
                    varA = 0;
                }

                //ovr127_A0C
                byte var8 = (byte)getAt(ArkWorkData, ArkWorkDataPtr + 0x10, 8);
                si = 0;

                //Ovr127_a57
                while (si< var8)
                {
                    if (argc_datasize == 0)
                    {
                        //ovr127_A75
                        CurrentByte_var2 = 0xFFFF;
                    }
                    else
                    {
                        //ovr127_A65
                        argc_datasize--;
                        CurrentByte_var2 = (byte)getAt(InputArk_arg0.Data, ReadDataBufferPtrArg0, 8);
                        ReadDataBufferPtrArg0++;
                    }

                    if (CurrentByte_var2 != 0xFFFF)
                    {
                        //ovr_127_A1B
                        DataCompressionSubfunction_127_23C(di);
                        setAt8(ArkWorkData, ArkWorkDataPtr + di + 0x12, CurrentByte_var2);
                        if (di<0x11)
                        {
                            //ovr127_a32
                            setAt8(ArkWorkData, ArkWorkDataPtr + di + 0x1012, CurrentByte_var2);
                        }
                        //ovr127_A3B
                        di = (short)((di+1) & 0x0FFF);
                        var6 = (short)((var6+1) & 0x0FFF);
                        DataCompressionSubfunction_127_0(var6);
                        si++; // loop to Ovr_A57
                    }
                    else
                    {
                        //ovr127_A80
                        var ax_si = (short)((int)si & 0xFFFF);  //CWD AX->DX:AX
                        var dx_si = (short)((int)(si>>16));
                        var tmp = (short)getAt(ArkWorkData, ArkWorkDataPtr + 1, 16);
                        tmp += ax_si;
                        setAt16(ArkWorkData, ArkWorkDataPtr + 1,tmp);
                        tmp = (short)getAt(ArkWorkData, ArkWorkDataPtr + 3, 16);
                        tmp += dx_si;
                        setAt16(ArkWorkData, ArkWorkDataPtr + 3,tmp);  
                        ax_si = (short)getAt(ArkWorkData, ArkWorkDataPtr + 1, 16);
                        dx_si = (short)getAt(ArkWorkData, ArkWorkDataPtr + 3, 16);
                        tmp = (short)getAt(ArkWorkData, ArkWorkDataPtr + 0xB, 16); 
                        if (dx_si>=tmp)
                        {
                            if(dx_si<=tmp)
                            {
                                 //ovr127_aa9
                                var tmp2 = (int)getAt(ArkWorkData, ArkWorkDataPtr + 0x9, 32); 
                                tmp2 += 0x400;
                                setAt32(ArkWorkData, ArkWorkDataPtr + 0x9, tmp2); 
                            }
                            else
                            {
                                if (ax_si>(short)getAt(ArkWorkData, ArkWorkDataPtr + 9, 16))
                                {
                                    //ovr127_aa9 again.
                                    var tmp2 = (int)getAt(ArkWorkData, ArkWorkDataPtr + 0x9, 32); 
                                    tmp2 += 0x400;
                                    setAt32(ArkWorkData, ArkWorkDataPtr + 0x9, tmp2); 
                                }
                            }
                        } 
                    }
                }
                //Ovr127_AE1
                ovr127_Ae1:
                var ax = si;
                si++;
                if (ax>=var8)
                {
                    if (ByteCounter_var4>0)
                    {
                        goto ovr127_8f5;
                    }
                    else
                    {
                        //ovr127_Af2
                        if (varA > 1)
                        {
                            si = 0;

                            while (si< varA)
                            {
                                //ovr127_AFF
                                if (WriteAddress_var12>var14)//todo var12 and var14 need to be sorted out as to what they are
                                {
                                    //ovr127_B1B
                                    //write out some data to file at writeAddress
                                    //do a check for remaining space. if none raise an error

                                    //ovr127_b53
                                    WriteAddress_var12 = WriteAddress_arg8;
                                    //change value at write address to array28[si]
                                    WriteAddress_var12++;
                                }
                                else
                                {
                                    //ovr127_B10
                                    WriteAddress_var12 = var28[si];
                                    WriteAddress_var12++;
                                }
                                si++;
                            }
                            //ovr127_B7B
                            var ax_temp = (int)getAt(ArkWorkData, ArkWorkDataPtr + 5, 32);
                            ax_temp+=varA;
                            setAt32(ArkWorkData, ArkWorkDataPtr + 0x5, ax_temp);
                        }

                        //ovr127_B87

                        //more to do here but basically write out some data to file

                        return true;

                    }
                }
                else
                {
                    //ovr127_ABA
                    DataCompressionSubfunction_127_23C(di);
                    di = (short)((di+1) & 0x0FFF);
                    var6 = (short)((var6+1) & 0x0FFF);
                    ByteCounter_var4--;
                    if (ByteCounter_var4==0)
                    {
                        goto ovr127_8f5;
                    }
                    else
                    {
                        DataCompressionSubfunction_127_0(var6);
                        goto ovr127_Ae1;
                    }
                }
            }
            else
            {
                //ovr 127_c0b;
                return false;
            }
        }

        static void DataCompressionSubfunction_127_23C(short arg0)
        {

        }

        static void DataCompressionSubfunction_127_0(short arg0)
        {

        }
    }
}