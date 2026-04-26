using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// For repacking of UW2 ark files
    /// Untested. WIP Code that will probably crash
    /// </summary>
    public class Repacker : Loader
    {
        static byte[] ArkWorkData = new byte[0x40000];
        static short ArkWorkDataPtr = 0;

        /// <summary>
        /// Launch repack and test if it matches the original compressed data.
        /// </summary>
        /// <param name="blockNoToCompress"></param>
        public static void TestRepack(int blockNoToCompress)
        {
            //return;

            //load raw data
            LevArkLoader.LoadLevArkFileData();
            bool Pass = true;
            //var lev_ark_block = LevArkLoader.LoadLevArkBlock(blockNoToCompress);
            UWBlock lev_ark_block;
            //load a level map
            DataLoader.LoadUWBlock(LevArkLoader.lev_ark_file_data, 0, -1, out lev_ark_block);

            if (((lev_ark_block.CompressionFlag >> 1) & 0x01) == 1)
            {
                //byte[] repacked = new byte[1];//todo replace with compress output.
                Pass = CompressData(lev_ark_block);
                // if (Pass)
                // {

                //     for (int i = 0; i <= lev_ark_block.Data.GetUpperBound(0); i++)
                //     {
                //         if (i <= repacked.GetUpperBound(0))
                //         {
                //             if (repacked[i] != lev_ark_block.Data[i])
                //             {
                //                 Debug.Print($"Data mismatch at offset {i}");
                //                 Pass = false;
                //                 break;
                //             }
                //         }
                //         else
                //         {
                //             Debug.Print("Out of bounds error on repacked block.");
                //             Pass = false;
                //             break;
                //         }
                //     }
                // }
                // else
                // {
                //     Debug.Print("Result of compression is false");
                //     Pass = false;
                // }
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

        public static void DumpCompressionMemory()
        {
            System.IO.File.WriteAllBytes("C:\\temp\\arkcompress.dat", ArkWorkData);
        }

        static bool CompressData(UWBlock InputArk_arg0, int MaybeMaxSize_argA = 0x8DD0, int argc_datasize = 0x7E08)
        {
            //return;//remove this line for infinite loops.
            short ReadDataBufferPtrArg0 = 0;
            ArkWorkDataPtr = 0;
            ArkWorkData = new byte[0x10000]; //to confirm size requirements. possibly should be  0x722f (29231d). Ref GetOffsetsForCompression_ovr153_
            //Do stuff with the data to repack it.
            // var Packed = new UWBlock();
            // Packed.Data = new byte[5];
            ushort CurrentByte_var2;
            short tmp;

            //implementation
            var WriteAddress_arg8 = 0x722F; //allocated in ovr153_0
            var WriteAddress_var12 = WriteAddress_arg8;
            //set some values first in CompressedData.
            //ovr_127_7C3
            var var14 = WriteAddress_arg8 + MaybeMaxSize_argA; //probably not needed in this implementation.

            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 1, val: 0);
            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 3, val: 0);
            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 5, val: 0);
            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 7, val: 0);
            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 9, val: 0);
            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 0xB, val: 0);
            setAt8(buffer: ArkWorkData, address: ArkWorkDataPtr + 0xD, val: 1);
            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 0x722B, val: 0xFFFF);
            setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 0x722D, val: 0x0);

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

            byte[] var28 = new byte[0x12];
            byte varb = 1;
            short varA = 1;// index into var28
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
            //ovr127_894
            while (ByteCounter_var4 < 0x12)//read in the next 18 bytes
            {
                //ovr127_89A
                byte DataBufferByte_varE;
                if (argc_datasize == 0)
                {
                    CurrentByte_var2 = 0xFFFF;
                }
                else
                {//ovr127_8A6
                    argc_datasize--;
                    //read in the next byte from the input data
                    DataBufferByte_varE = (byte)getAt8(InputArk_arg0.Data, ReadDataBufferPtrArg0);
                    ReadDataBufferPtrArg0++;
                    CurrentByte_var2 = DataBufferByte_varE;
                }
                //ovr127_8B6
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
            {//ovr127_8D5, ok until here.
                si = 1;
                while (si <= 0x12)
                {//ovr127_8DA
                    DataCompressionSubfunction_127_0((short)(var6 - si));
                    si++;
                }
                //ovr127_8f0
                DataCompressionSubfunction_127_0(var6);

                //ovr127_8f5
            ovr127_8f5:
                if (getAt16(ArkWorkData, ArkWorkDataPtr + 0x10) > ByteCounter_var4)
                {
                    //ovr127_902
                    setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 0x10, val: ByteCounter_var4);
                }
                //ovr127_909
                if (getAt16(ArkWorkData, ArkWorkDataPtr + 0x10) > 2)
                {
                    //copy
                    //ovr127_92D
                    //ovr127_93D
                    var28[varA] = (byte)getAt8(ArkWorkData, Address: ArkWorkDataPtr + 0xE);
                    varA++;

                    //ovr127_958
                    var ax_temp = getAt16(ArkWorkData, ArkWorkDataPtr + 0xE);
                    ax_temp = ax_temp >> 4;
                    byte al = (byte)(ax_temp & 0xF0);
                    var dl = (byte)getAt8(ArkWorkData, ArkWorkDataPtr + 0x10);
                    dl = (byte)(dl - 3);
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
                    //ArkWorkDataPtr += var6;

                    //ovr127_958
                    var28[varA] = (byte)getAt8(ArkWorkData, ArkWorkDataPtr + var6 + 0x12);
                }
                //ovr127_958->> ovr 127_962
                varA++;
                varb = (byte)(varb << 1);

                if (varb == 0)
                {
                    si = 0;
                    while (si < varA)
                    {
                        //ovr127_976
                        if (WriteAddress_var12 >= var14) //could be var16 instead/
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
                    tmp = (short)getAt32(ArkWorkData, ArkWorkDataPtr + 5);
                    tmp += varA;
                    setAt32(ArkWorkData, ArkWorkDataPtr + 5, tmp);

                    var28[0] = 0;
                    varb = 1;
                    varA = 0;
                }

                //ovr127_A0C
                byte var8 = (byte)getAt8(ArkWorkData, ArkWorkDataPtr + 0x10);
                si = 0;

            Ovr127_a57:
                if (si < var8)
                {
                    if (argc_datasize == 0)
                    {
                        //ovr127_A75
                        CurrentByte_var2 = 0xFFFF;
                    }
                    else
                    {
                        //ovr127_A62
                        argc_datasize--;
                        CurrentByte_var2 = (byte)getAt8(InputArk_arg0.Data, ReadDataBufferPtrArg0);
                        ReadDataBufferPtrArg0++;
                    }

                    if (CurrentByte_var2 != 0xFFFF)
                    {
                        //ovr_127_A1B
                        DataCompressionSubfunction_127_23C(di);//infinite loop in here.
                        setAt8(ArkWorkData, ArkWorkDataPtr + di + 0x12, CurrentByte_var2);
                        if (di < 0x11)
                        {
                            //ovr127_a32
                            setAt8(ArkWorkData, ArkWorkDataPtr + di + 0x1012, CurrentByte_var2);
                        }
                        //ovr127_A3B
                        di = (short)((di + 1) & 0x0FFF);
                        var6 = (short)((var6 + 1) & 0x0FFF);
                        DataCompressionSubfunction_127_0(var6);
                        si++; // loop to Ovr_A57
                        goto Ovr127_a57;
                    }
                }

                //ovr127_A80
                var itmp = (int)getAt32(ArkWorkData, ArkWorkDataPtr + 1);
                itmp += si;
                setAt32(ArkWorkData, ArkWorkDataPtr + 1, itmp);
                // var ax_si = (short)((int)si & 0xFFFF);  //CWD AX->DX:AX
                // var dx_si = (short)((int)(si >> 16));
                // tmp = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 1);
                // tmp += ax_si;
                // setAt16(ArkWorkData, ArkWorkDataPtr + 1, tmp);
                // tmp = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 3);
                // tmp += dx_si;
                //setAt16(ArkWorkData, ArkWorkDataPtr + 3, tmp);

                var ax_si = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 1);
                var dx_si = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 3);
                var tmpB = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 0xB);
                var tmp9 = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 0x9);
                if (dx_si >= tmpB)
                {
                    if (dx_si > tmpB)
                    {
                        goto ovr127_aa9;
                    }
                    else
                    {
                        if (ax_si <= tmp9)
                        {
                            goto ovr127_Ae1;
                        }
                        else
                        {
                            goto ovr127_aa9;
                        }
                    }
                }
                else
                {
                    goto ovr127_Ae1;
                }

            ovr127_aa9:
                var tmp2 = (int)getAt32(ArkWorkData, ArkWorkDataPtr + 0x9);
                tmp2 += 0x400;
                setAt32(ArkWorkData, ArkWorkDataPtr + 0x9, tmp2);

            ovr127_Ae1:
                var ax = si;
                si++;
                if (ax >= var8)
                {//ovr127_AE9
                    if (ByteCounter_var4 > 0)
                    {
                        goto ovr127_8f5;
                    }
                    else
                    {
                        //ovr127_Af2
                        if (varA > 1)
                        {
                            si = 0;

                            while (si < varA)
                            {
                                //ovr127_AFF
                                if (WriteAddress_var12 > var14)//todo var12 and var14 need to be sorted out as to what they are
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
                            var ax_temp = (int)getAt32(ArkWorkData, ArkWorkDataPtr + 5);
                            ax_temp += varA;
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
                    di = (short)((di + 1) & 0x0FFF);
                    var6 = (short)((var6 + 1) & 0x0FFF);
                    ByteCounter_var4--;
                    if (ByteCounter_var4 == 0)
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
            int si;
            short ax;
            var di_arg0 = arg0;
            //ovr127_24E
            if (getAt16(buffer: ArkWorkData, ArkWorkDataPtr + 0x5229 + (di_arg0 * 2)) != 0x1000)
            {
                //ovr127_25A
                if (getAt16(buffer: ArkWorkData, Address: ArkWorkDataPtr + 0x3027 + (di_arg0 * 2)) != 0x1000)
                {
                    //ovr127_27F
                    if (getAt16(buffer: ArkWorkData, ArkWorkDataPtr + 0x1025 + (di_arg0 * 2)) != 0x1000)
                    {
                        //ovr127_2A4
                        si = (int)getAt16(buffer: ArkWorkData, Address: ArkWorkDataPtr + 0x1025 + (di_arg0 * 2));
                        if (getAt16(buffer: ArkWorkData, Address: ArkWorkDataPtr + 0x3027 + (si * 2)) != 0x1000)
                        {
                            //ovr127_2C9
                        ovr127_2c9:

                            si = (int)getAt16(buffer: ArkWorkData, Address: ArkWorkDataPtr + 0x3027 + (si * 2));  //infinite loop here!
                            if ((int)getAt16(buffer: ArkWorkData, Address: ArkWorkDataPtr + 0x3027 + (si * 2)) != 0x1000)
                            {
                                goto ovr127_2c9;
                            }
                            else
                            {
                                //these are probably all wrong until I can step through and test properly...
                                //ovr127_2ED
                                ax = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (si * 2));
                                //ovr127_304
                                var dx = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (si * 2));
                                //ov127_311
                                setAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (dx * 2), ax);

                                //ovr127_A1A
                                ax = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (si * 2));
                                //ovr127_32F
                                dx = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (si * 2));
                                //ovr127_33C
                                setAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (dx * 2), ax);

                                //ovr127_345
                                ax = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (di_arg0 * 2));
                                //ovr127_354
                                setAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + si * 2, ax);

                                //ovr127_35F
                                ax = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (di_arg0 * 2));
                                setAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + ax * 2, si);
                            }
                        }

                        //ovr127_37B
                        ax = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (di_arg0*2));
                        setAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (si*2), ax);

                        //ovr127_39D
                        ax = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (di_arg0*2));
                        setAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (ax*2), si);

                    }
                    else
                    {
                        //ovr127_29A
                        si = (int)getAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (di_arg0 * 2));
                    }
                }
                else
                {
                    //ovr127_26D
                    si = (int)getAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (di_arg0 * 2));
                }

                //ovr127_3B5
                setAt16(buffer: ArkWorkData, address: ArkWorkDataPtr + 0x5229 + (si * 2),
                    val: (int)getAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (di_arg0 * 2)));

                //ovr127_3D7
                ax = (short)getAt16(buffer: ArkWorkData, Address: ArkWorkDataPtr + 0x5229 + (di_arg0 * 2));
                if (getAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (ax * 2)) != di_arg0)
                {
                    //ovr127_40F 
                    ax = (short)getAt16(buffer: ArkWorkData, Address: ArkWorkDataPtr + 0x5229 + (di_arg0 * 2));
                    setAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (ax * 2), si);
                }
                else
                {
                    //ovr127_3F5
                    ax = (short)getAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (di_arg0 * 2));
                    setAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (ax * 2), si);
                }

                //ovr128_42b
                setAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (di_arg0 * 2), 0x1000);
            }
        }

        static void DataCompressionSubfunction_127_0(short arg0)
        {
            var cx = arg0;
            var var2 = 1;
            //DumpCompressionMemory();
            int ArrayPtrVar6 = ArkWorkDataPtr + 0x12 + cx;

            var di = (int)getAt8(buffer: ArkWorkData, Address: ArrayPtrVar6) + 0x1001;

            setAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (cx * 2), 0x1000);
            setAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (cx * 2), 0x1000);
            setAt16(ArkWorkData, ArkWorkDataPtr + 0x10, 0);

        ovr127_5A:
            if (var2 < 0)
            {
                //ovr127_A5
                if (getAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (di * 2)) == 0x1000)
                {
                    //ovr127_C9 (ok)
                    setAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (di * 2), cx);
                    setAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (cx * 2), di);
                    return;
                }
                else
                {
                    //ovr127_B8
                    di = (int)getAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (di * 2));
                }
            }
            else
            {
                //ovr127_60
                if (getAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (di * 2)) == 0x1000)
                {
                    //ovr127_84 (ok)
                    setAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (di * 2), cx);
                    setAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (cx * 2), di);
                    return;
                }
                else
                {
                    //ovr127_73 (ok)
                    di = (int)getAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (di * 2));
                }
            }
            //ovr127_EA
            var si = 1;

        ovr127_10D: //eventually infinite loops here.
            if (si < 0x12)
            {
                //ovr127_EF
                var ax = getAt8(ArkWorkData, ArrayPtrVar6 + si);
                var dx = getAt8(ArkWorkData, ArkWorkDataPtr + di + si + 0x12);
                var2 = (int)(ax - dx);

                if (var2 == 0)
                {//ovr127_10C
                    si++;
                    goto ovr127_10D;
                }
            }

            //ovr127_112
            if (getAt16(ArkWorkData, ArkWorkDataPtr + 0x10) >= si)
            {
                //ovr127_11C
                goto ovr127_5A;
            }
            else
            {
                //ovr127_11F
                setAt16(ArkWorkData, ArkWorkDataPtr + 0xE, di);
                setAt16(ArkWorkData, ArkWorkDataPtr + 0x10, si);
                if (si < 0x12)
                {
                    goto ovr127_5A;
                }
                else
                {
                    //ovr127_135
                    var ax = (int)getAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (di * 2));
                    setAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (cx * 2), ax);

                    //ovr127_15D
                    ax = (int)getAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (di * 2));
                    setAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (cx * 2), ax);

                    //ovr127_171
                    ax = (int)getAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (di * 2));
                    setAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (cx * 2), ax);

                    //ovr127_18F
                    ax = (int)getAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (di * 2));
                    setAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (ax * 2), cx);

                    //ovr127_1AF
                    ax = (int)getAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (di * 2));
                    setAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (ax * 2), cx);

                    //ovr127_1CB
                    ax = (int)getAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (di * 2));
                    if ((getAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (ax * 2))) != di)
                    {
                        //ovr127_209
                        ax = (int)getAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (di * 2));
                        setAt16(ArkWorkData, ArkWorkDataPtr + 0x1025 + (ax * 2), cx);
                    }
                    else
                    {
                        //ovr127_1E9
                        ax = (int)getAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (di * 2));
                        setAt16(ArkWorkData, ArkWorkDataPtr + 0x3027 + (ax * 2), cx);
                    }
                    //ovr 127_227
                    setAt16(ArkWorkData, ArkWorkDataPtr + 0x5229 + (di * 2), 0x1000);
                    return;
                }
            }
        }
    }
}