using System;
namespace Underworld
{
    public partial class motion : Loader
    {

        /// <summary>
        /// Kicks off the processing of motion.
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="MotionParams"></param>
        /// <param name="MaybeMagicObjectFlag"></param>
        /// <returns></returns>
        public static bool CalculateMotion_TopLevel(uwObject projectile, UWMotionParamArray MotionParams, byte[] SpecialMotionHandler)
        {
            //seg006_1413_D6A            
            MotionParams.speed_12 = (byte)(projectile.Projectile_Speed << 4);
            CalculateMotion(
                projectile: projectile,
                MotionParams: MotionParams,
                SpecialMotionHandler: SpecialMotionHandler);
            return true;
        }


        /// <summary>
        /// A loop that goes through some steps?
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="MotionParams"></param>        
        static void CalculateMotion(uwObject projectile, UWMotionParamArray MotionParams, byte[] SpecialMotionHandler)
        {
            int collisionCounter = 0;
            UWMotionParamArray.PtrTo26D2_DSEG_26B8_MotionHandler = SpecialMotionHandler;
            UWMotionParamArray.MotionParam0x25_dseg_67d6_26A9 = MotionParams.tilestate25;

            MotionCalcArray.Unk17_base = 0;
            UWMotionParamArray.CalculateMotionGlobal_dseg_67d6_26B6 = 0;

            if (InitialMotionCalc_seg031_2CFA_412(MotionParams, true, 1))
            {
                //loop at seg031_2CFA_69
                while (UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E + 1 > UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414)
                {
                    //seg031_2CFA_4C:
                    UWMotionParamArray.CalculateMotionGlobal_dseg_67d6_26B6++;
                    if (UWMotionParamArray.CalculateMotionGlobal_dseg_67d6_26B6 == 0x10)//note the increment
                    {
                        //seg031_2CFA_7A:
                        MotionParams.unk_10_Z = 0;
                        MotionParams.unk_a_pitch = 0;
                        MotionParams.unk_14 = 0;
                        return;
                    }
                    else
                    {

                        //seg031_2CFA_59:
                        if (ProcessCollisions_seg031_2CFA_12B8(MotionParams, 1) != 0)
                        {
                            //seg031_2CFA_64
                            DoTileCollisionMaybe_seg031_2CFA_179C(projectile, MotionParams);
                        }
                        collisionCounter++;
                    }
                }
                //seg031_2CFA_73:  
                StoreNewXYZH_seg031_2CFA_800(MotionParams);
                return;
            }
        }

        static bool InitialMotionCalc_seg031_2CFA_412(UWMotionParamArray MotionParams, bool arg0_CopyToCalcArray, int arg2)
        {
            short var2_xvector = 0; short var4_yvector = 0;
            short var8 = 0;
            SomethingProjectileHeading_seg021_22FD_EAE((ushort)MotionParams.heading_1E, ref var2_xvector, ref var4_yvector);
            //seg031_2CFA_457:
            MotionParams.unk_6_x = (short)((var2_xvector * MotionParams.unk_14) >> 0xF);
            //seg031_2CFA_47F:
            MotionParams.unk_8_y = (short)((var4_yvector * MotionParams.unk_14) >> 0xF);

            //possibly the following are translation vectors  
            //seg031_2CFA_494:          
            MotionParams.unk_6_x = (short)(MotionParams.unk_6_x + (MotionParams.unk_c_X * MotionParams.speed_12));
            //seg031_2CFA_4AB:
            MotionParams.unk_8_y = (short)(MotionParams.unk_8_y + (MotionParams.unk_e_Y * MotionParams.speed_12));
            //seg031_2CFA_4B2:
            MotionParams.unk_a_pitch = (short)(MotionParams.unk_a_pitch + (MotionParams.unk_10_Z * MotionParams.speed_12));

            if (((int)MotionParams.unk_6_x | (int)MotionParams.unk_8_y | (int)MotionParams.unk_a_pitch) == 0)
            {
                return false;
            }
            else
            {
                //seg031_2CFA_4D2
                if (arg0_CopyToCalcArray)
                {
                    CopyMotionValsToAnotherArray_seg031_2CFA_93(MotionParams);
                }
                if (Math.Abs(MotionParams.unk_6_x) <= Math.Abs(MotionParams.unk_8_y))
                {//seg031_2CFA_4FE:
                    UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer = 1;
                }
                else
                {
                    UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer = 0;
                }
                UWMotionParamArray.dseg_67d6_40C_indexer = (short)((UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer + 1) % 2);// note the %

                if (MotionParams.GetParam6_XorYValue(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer) <= 0)
                {//seg031_2CFA_530:
                    UWMotionParamArray.SetMotionXY404(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer, -8192);
                }
                else
                {//seg031_2CFA_526:
                    UWMotionParamArray.SetMotionXY404(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer, +8192);
                }

                //seg031_2CFA_53C
                if (MotionParams.GetParam6_XorYValue(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer) == 0)
                {
                    //seg031_2CFA_5D8:
                    UWMotionParamArray.SetMotionXY404(UWMotionParamArray.dseg_67d6_40C_indexer, 1);
                    var8 = 0;
                    UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E = 0;
                    UWMotionParamArray.dseg_67d6_410 = 0;
                    UWMotionParamArray.dseg_67d6_412 = MotionParams.speed_12;
                }
                else
                {
                    //seg031_2CFA_54F
                    var tmp = UWMotionParamArray.GetMotionXY_404(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer);
                    tmp = (short)(tmp / 0x100);
                    tmp = (short)(MotionParams.GetParam6_XorYValue(UWMotionParamArray.dseg_67d6_40C_indexer) * tmp);
                    tmp = (short)(tmp / MotionParams.GetParam6_XorYValue(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer));
                    tmp = (short)(tmp << 8);

                    //seg031_2CFA_585:  
                    UWMotionParamArray.SetMotionXY404(UWMotionParamArray.dseg_67d6_40C_indexer, tmp);

                    //seg031_2CFA_59E:
                    var8 = (short)(MotionParams.GetParam6_XorYValue(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer) * MotionParams.speed_12); //(short)(Param6Lookup * MotionParams.speed_12);

                    //seg031_2CFA_5A4:
                    UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E = (short)(Math.Abs(var8) >> 0xD);

                    //seg031_2CFA_5BA:
                    UWMotionParamArray.dseg_67d6_410 = (short)(Math.Abs(var8) & 0x1FFF);

                    //seg031_2CFA_5C0:
                    UWMotionParamArray.dseg_67d6_412 = (short)Math.Abs(0x2000 / MotionParams.GetParam6_XorYValue(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer));
                }

                //seg031_2CFA_60D:
                UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414 = 0;

                if (
                    (MotionCalcArray.MotionArrayObjectIndexA_base == 1)
                    ||
                    ((MotionCalcArray.MotionArrayObjectIndexA_base != 1) && MotionParams.unk_a_pitch != 0)
                    )
                {
                    if (arg2 != 0)//seg031_2CFA_627
                    {
                        //seg031_2CFA_62D:
                        ProcessMotionTileHeights_seg028_2941_385(MotionParams.unk_24);//unk24 is 0 in normal projectile processing
                        ScanForCollisions(0, 0);
                    }
                }

                //seg031_2CFA_648:
                if (MotionParams.unk_a_pitch != 0)//seg031_2CFA_648:
                {//seg031_2CFA_655:
                    if (MotionParams.unk_a_pitch <= 0)
                    {// seg031_2CFA_663
                        UWMotionParamArray.Gravity_related_dseg_67d6_408 = -2048;
                    }
                    else
                    {
                        UWMotionParamArray.Gravity_related_dseg_67d6_408 = +2048;
                    }

                    //seg031_2CFA_669
                    SetCollisionTarget_seg031_2CFA_10E(MotionParams, arg2);


                    if (MotionParams.GetParam6_XorYValue(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer) != 0)
                    {//seg031_2CFA_686

                        int varC = (UWMotionParamArray.GetMotionXY_404(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer) / 0x2000) * MotionParams.unk_a_pitch;

                        //seg031_2CFA_6BA:
                        var var10 = MotionParams.GetParam6_XorYValue(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer);
                        //seg031_2CFA_6DD:
                        var10 = (short)(var10 * (UWMotionParamArray.Gravity_related_dseg_67d6_408 / 0x800));
                        //seg031_2CFA_6F8
                        varC = (int)(varC * 0x100);
                        varC = (int)(varC / var10);

                        if (varC < 0)
                        {
                            //seg031_2CFA_738:
                            UWMotionParamArray.dseg_67d6_410 = 0;
                            //var temp = DataLoader.getAt(MotionParams.data, 0xA, 16);
                            //MotionParams.unk_a
                            UWMotionParamArray.Gravity_Related_dseg_67d6_41F = (short)(int)Math.Abs(MotionParams.speed_12 * (MotionParams.unk_a_pitch >> 1) >> 5);
                        }
                        else
                        {
                            //seg031_2CFA_75B
                            UWMotionParamArray.Gravity_Related_dseg_67d6_41F = (short)varC;
                        }
                    }
                    else
                    {
                        //seg031_2CFA_763
                        var temp = (short)DataLoader.getAt(MotionParams.data, 0xA, 16);
                        UWMotionParamArray.Gravity_Related_dseg_67d6_41F = (short)Math.Abs(MotionParams.speed_12 * (temp >> 1) >> 5);
                    }
                }
                else
                {
                    UWMotionParamArray.Gravity_related_dseg_67d6_408 = 0;
                }

            }
            return true;
        }

        static void SomethingProjectileHeading_seg021_22FD_EAE(ushort heading, ref short Result_arg2, ref short Result_arg4)
        {
            HeadingLookupCalc(heading, out short ax, out short bx);
            Result_arg2 = ax;
            Result_arg4 = bx;
        }


        /// <summary>
        /// Does a lookup of a table of values to calculate something.
        /// Below code is a fairly direct copy of the assembly code
        /// </summary>
        /// <param name="bx"></param>
        /// <param name="Result_AX"></param>
        /// <param name="Result_BX"></param>
        /// <returns></returns>
        static int HeadingLookupCalc(ushort bx, out short Result_AX, out short Result_BX)
        {
            var cx = bx & 0xff;
            //bx = (bx & 0xFF00)>>8;
            bx = (ushort)(bx >> 8);
            var bp = HeadingLookupTable[bx];
            var ax = HeadingLookupTable[bx + 1];
            ax = (short)(ax - bp);
            var tmp_1 = (ax * cx);
            ax = (short)((tmp_1 & 0xFFFF) >> 8);
            ax = (short)(ax | ((tmp_1 >> 16) & 0xFF) << 8);
            // if (ax < 0)
            // {
            //     ax = (short)((ax & 0xFFFF) >> 8);
            //     ax = (short)-Math.Abs(ax);
            // }
            // else
            // {
            //     ax = (short)(ax >> 8);
            // }
            ax = (short)(ax + bp);
            Result_AX = ax;

            bp = HeadingLookupTable[64 + bx];
            ax = HeadingLookupTable[65 + bx];
            ax = (short)(ax - bp);
            var tmp_2 = (ax * cx);
            ax = (short)(tmp_2 & 0xFFFF);
            var dl = (short)((tmp_2 >> 16) & 0xFF);
            // sbyte dl;
            // if (ax >= 0)
            // {
            //     dl = 0;
            // }
            // else
            // {
            //     dl = -1;
            // }
            //THIS IS WHERE I GO WRONG
            var bl = (ax & 0xFFFF) >> 8;
            //var bh = dl;
            bx = (ushort)((dl << 8) | bl);
            bx = (ushort)(bp + bx);
            Result_BX = (short)bx;
            return Result_AX;

        }



        /// <summary>
        /// Initialises the MotionCalcArray data from Motion Params
        /// </summary>
        /// <param name="MotionParams"></param>
        static void CopyMotionValsToAnotherArray_seg031_2CFA_93(UWMotionParamArray MotionParams)
        {
            //Set the pointer to use the base byte array
            MotionCalcArray.PtrToMotionCalc = MotionCalcArray.base_dseg_25c4;
            //store some globals

            MotionCalcArray.Heading6 = MotionParams.heading_1E;
            MotionCalcArray.Radius8 = MotionParams.radius_22;
            MotionCalcArray.Height9 = MotionParams.height_23;
            MotionCalcArray.MotionArrayObjectIndexA = MotionParams.index_20;

            MotionCalcArray.x0 = (ushort)(MotionParams.x_0 >> 5);
            MotionCalcArray.y2 = (ushort)(MotionParams.y_2 >> 5);
            MotionCalcArray.z4_base = (ushort)(MotionParams.z_4 >> 3);

            UWMotionParamArray.RelatedToMotionX_dseg_67d6_3FE = (short)((MotionParams.x_0 & 0x1F) << 8);
            UWMotionParamArray.RelatedToMotionY_dseg_67d6_400 = (short)((MotionParams.y_2 & 0x1F) << 8);
            UWMotionParamArray.RelatedToMotionZ_dseg_67d6_402 = (short)((MotionParams.z_4 & 7) << 8);

            //DumpMotionMemory(MotionParams, "AfterCopyingMotionArray");
        }


        /// <summary>
        /// Appears to do stuff with the tile the motion is happening in.
        /// </summary>
        /// <param name="distance_arg0"></param>
        public static void ProcessMotionTileHeights_seg028_2941_385(int distance_arg0)
        {
            //?
            UWMotionParamArray.TileAttributesArray = new short[0x9]; // 9 * 0x1111 or 18 * 0x11?   0-8 entries
            for (int i = 0; i <= UWMotionParamArray.TileAttributesArray.GetUpperBound(0); i++)
            {
                UWMotionParamArray.TileAttributesArray[i] = 0x1111;//default values for the tiles in a 3x3 grid
            }
            var tileX = MotionCalcArray.x0 >> 3;
            var tileY = MotionCalcArray.y2 >> 3;
            UWMotionParamArray.TileRelatedToMotion_dseg_67d6_257E = UWTileMap.current_tilemap.Tiles[tileX, tileY];

            var xposVar8 = MotionCalcArray.x0 & 0x7;
            var yposVarA = MotionCalcArray.y2 & 0x7;
            var XYOffsetVar1 = 4;//4 means the center tile in a 3x3 grid of tiles
                                 //var YPosVarA = 0;
                                 // var XPosVarB = 0;
            UWMotionParamArray.SubArray.Unk16 = 4;
            UWMotionParamArray.SubArray.Unk17 = xposVar8;
            UWMotionParamArray.SubArray.Unk18 = yposVarA;
            if (UWMotionParamArray.TileAttributesArray[4] == 0x1111)
            {
                //set value for this tile.
                var tile = UWMotionParamArray.TileRelatedToMotion_dseg_67d6_257E;
                //int terrain = TerrainDatLoader.GetTerrainTypeNo(tile);
                UWMotionParamArray.TileAttributesArray[4] = (short)((int)(tile.tileType) | (int)(tile.floorHeight << 4) | (int)(TerrainDatLoader.GetTerrainTypeNo(tile) << 8));
            }

            seg028_2941_2CF_terrainrelated(distance_arg0);

            MotionCalcArray.UnkE = MotionCalcArray.UnkC_terrain;
            MotionCalcArray.Unk11 = MotionCalcArray.Unk10_relatedtotileheight;

            //seg028_2941_449
            if (MotionCalcArray.Radius8 != 0)
            {
                yposVarA -= MotionCalcArray.Radius8;
                while (yposVarA < 0)
                {//seg028_2941_45C:
                    XYOffsetVar1 -= 3;
                    yposVarA += 8;
                }
                //seg028_2941_46E:
                xposVar8 -= MotionCalcArray.Radius8;
                while (xposVar8 < 0)
                {//seg028_2941_47C:
                    XYOffsetVar1--;
                    xposVar8 += 8;
                }
                //seg028_2941_48E
                UWMotionParamArray.SubArray.Unk2_offset = XYOffsetVar1;//DS:2564
                UWMotionParamArray.SubArray.Unk3_X = xposVar8;//DS:2565
                UWMotionParamArray.SubArray.Unk4_Y = yposVarA;//DS:2566

                xposVar8 += (MotionCalcArray.Radius8 << 1);
                while (xposVar8 > 7)
                {//seg028_2941_4B0
                    XYOffsetVar1++;
                    xposVar8 -= 8;
                }
                //seg028_2941_4C5:
                UWMotionParamArray.SubArray.Unk7_offset = XYOffsetVar1;
                UWMotionParamArray.SubArray.Unk8_X = xposVar8;
                UWMotionParamArray.SubArray.Unk9_Y = yposVarA;
                yposVarA += (MotionCalcArray.Radius8 << 1);

                //seg028_2941_4F0:
                while (yposVarA > 7)
                {//seg028_2941_4E4: 
                    XYOffsetVar1 += 3;
                    yposVarA -= 8;
                }
                //seg028_2941_4FF:
                UWMotionParamArray.SubArray.Unkc_offset = XYOffsetVar1;
                UWMotionParamArray.SubArray.UnkD_x = xposVar8;
                UWMotionParamArray.SubArray.UnkE = yposVarA;
                //seg028_2941_536:
                xposVar8 -= (MotionCalcArray.Radius8 << 1);
                while (xposVar8 < 0)
                {//again?    seg028_2941_518: 
                    XYOffsetVar1--;
                    xposVar8 += 8;
                }
                //seg028_2941_536
                UWMotionParamArray.SubArray.Unk11_offset = XYOffsetVar1;
                UWMotionParamArray.SubArray.Unk12_x = xposVar8;
                UWMotionParamArray.SubArray.Unk13_y = yposVarA;


                //Populate tile attributes for neighbouring tiles?

                //2941:543
                var searchPTR = (int)UWMotionParamArray.TileRelatedToMotion_dseg_67d6_257E.Ptr + (TileOffsetArray[UWMotionParamArray.SubArray.Unk2_offset] * 4);
                var Tile_Var6 = UWTileMap.GetTileByPTR(searchPTR);
                if (UWMotionParamArray.TileAttributesArray[UWMotionParamArray.SubArray.Unk2_offset] == 0x1111)
                {//seg028_2941_572:
                    UWMotionParamArray.TileAttributesArray[UWMotionParamArray.SubArray.Unk2_offset] = (short)((int)(Tile_Var6.tileType) | ((int)Tile_Var6.floorHeight << 4) | (TerrainDatLoader.GetTerrainTypeNo(Tile_Var6) << 8));
                }


                //seg028_2941_5AA:
                searchPTR = (int)UWMotionParamArray.TileRelatedToMotion_dseg_67d6_257E.Ptr + (TileOffsetArray[UWMotionParamArray.SubArray.Unk7_offset] * 4);
                Tile_Var6 = UWTileMap.GetTileByPTR(searchPTR);
                if (UWMotionParamArray.TileAttributesArray[UWMotionParamArray.SubArray.Unk7_offset] == 0x1111)
                {//seg028_2941_5DA
                    UWMotionParamArray.TileAttributesArray[UWMotionParamArray.SubArray.Unk7_offset] = (short)((int)(Tile_Var6.tileType) | ((int)Tile_Var6.floorHeight << 4) | (TerrainDatLoader.GetTerrainTypeNo(Tile_Var6) << 8));
                }

                //seg028_2941_618:
                searchPTR = (int)UWMotionParamArray.TileRelatedToMotion_dseg_67d6_257E.Ptr + (TileOffsetArray[UWMotionParamArray.SubArray.Unkc_offset] * 4);
                Tile_Var6 = UWTileMap.GetTileByPTR(searchPTR);
                if (UWMotionParamArray.TileAttributesArray[UWMotionParamArray.SubArray.Unkc_offset] == 0x1111)
                {//seg028_2941_648
                    UWMotionParamArray.TileAttributesArray[UWMotionParamArray.SubArray.Unkc_offset] = (short)((int)(Tile_Var6.tileType) | ((int)Tile_Var6.floorHeight << 4) | (TerrainDatLoader.GetTerrainTypeNo(Tile_Var6) << 8));
                }

                //seg028_2941_686:
                searchPTR = (int)UWMotionParamArray.TileRelatedToMotion_dseg_67d6_257E.Ptr + (TileOffsetArray[UWMotionParamArray.SubArray.Unk11_offset] * 4);
                Tile_Var6 = UWTileMap.GetTileByPTR(searchPTR);
                if (UWMotionParamArray.TileAttributesArray[UWMotionParamArray.SubArray.Unk11_offset] == 0x1111)
                {//seg028_2941_6B9
                    UWMotionParamArray.TileAttributesArray[UWMotionParamArray.SubArray.Unk11_offset] = (short)((int)(Tile_Var6.tileType) | ((int)Tile_Var6.floorHeight << 4) | (TerrainDatLoader.GetTerrainTypeNo(Tile_Var6) << 8));
                }


                //seg028_2941_6F4
                UWMotionParamArray.dseg_67d6_2584 = 1;
                var var2 = 0;
                while (var2 < 4)
                {
                    if (GetNeighbourTileHeightDetails_seg028_2941_217(var2, distance_arg0) == 0)
                    {//seg028_2941_718:
                        var temp = getAt(UWMotionParamArray.SubArray.dseg_2562, 5 + var2 * 5, 16);
                        if ((temp & 0x300) == 0)
                        {
                            var var10 = new byte[] { 0x4, 0x10, 0x2, 0x8, 0x4, 0x1 };   //dseg_67d6_3BF
                            var varB = 0;

                            while (varB < 2)
                            {
                                //seg028_2941_75D
                                var tmp = getAt(UWMotionParamArray.SubArray.dseg_2562, 2 + (5 * ((1 + var2 + (varB << 1)) & 0x3)), 8);

                                if (tmp != getAt(UWMotionParamArray.SubArray.dseg_2562, 2 + var2 * 5, 8))
                                {
                                    var tile_index = getAt(UWMotionParamArray.SubArray.dseg_2562, 2 + (var2 * 5), 8);
                                    var tiletype = UWMotionParamArray.TileAttributesArray[tile_index] & 0xF;
                                    //var10[varB+var2]
                                    //test tile attributes
                                    if ((TileTraversalFlags_dseg_67d6_1BA6[tiletype] & var10[varB + var2]) != 0)
                                    {
                                        //seg028_2941_7B2:
                                        DataLoader.setAt(UWMotionParamArray.SubArray.dseg_2562, 5 + (var2 * 5), 16, 0x200);
                                        MotionCalcArray.Unk11 = 0x80; //blocks movement into tile.
                                        varB = 2;
                                    }
                                }
                                varB++;
                            }
                            UWMotionParamArray.dseg_67d6_2584 = 0;
                        }
                    }
                    var2++;
                }
                //seg028_2941_7E8:
                MotionCalcArray.UnkE = (short)((int)MotionCalcArray.UnkE | UWMotionParamArray.SubArray.Unk5 | UWMotionParamArray.SubArray.UnkA | UWMotionParamArray.SubArray.UnkF | UWMotionParamArray.SubArray.Unk14);
            }
        }

        static int seg028_2941_2CF_terrainrelated(int distance_arg0)
        {//this may behave differently in UW1?
            MotionCalcArray.UnkC_terrain = (short)((UWMotionParamArray.TileAttributesArray[4] & 0x300) >> 8);
            int var1;

            MotionCalcArray.Unk10_relatedtotileheight = (byte)GetTileHeightAdjustedByTileType_seg028_2941_E(4, out var1);

            if (MotionCalcArray.Unk10_relatedtotileheight != 0x80)
            {
                if (MotionCalcArray.z4 + distance_arg0 >= MotionCalcArray.Unk10_relatedtotileheight)
                {
                    if (MotionCalcArray.z4 - distance_arg0 <= MotionCalcArray.Unk10_relatedtotileheight)
                    {
                        MotionCalcArray.UnkC_terrain = (short)(MotionCalcArray.UnkC_terrain | 0x4);

                        var tmp = 8 << ((UWMotionParamArray.TileAttributesArray[4] & 0x300) >> 8);

                        MotionCalcArray.UnkC_terrain = (short)(MotionCalcArray.UnkC_terrain | (short)tmp);

                    }
                    else
                    {
                        MotionCalcArray.UnkC_terrain = (short)(MotionCalcArray.UnkC_terrain | 0x800);
                    }
                }
                else
                {
                    MotionCalcArray.UnkC_terrain = (short)(MotionCalcArray.UnkC_terrain | 0x100);
                }
            }
            else
            {
                MotionCalcArray.UnkC_terrain = (short)(MotionCalcArray.UnkC_terrain | 0x200);
            }

            if ((UWMotionParamArray.TileAttributesArray[4] & 0xF) >= 6)
            {
                MotionCalcArray.UnkC_terrain = (short)(MotionCalcArray.UnkC_terrain | 0x2000);
            }
            return SBB(var1);
        }


        /// <summary>
        /// Looks like it gets the height of the tile at the specified position.
        /// </summary>
        /// <param name="MotionParams"></param>
        /// <param name="TileArrayOffset_arg0"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        static byte GetTileHeightAdjustedByTileType_seg028_2941_E(int TileArrayOffset_arg0, out int arg2)
        {
            arg2 = 0;
            var temp_index = Loader.getAt(UWMotionParamArray.SubArray.dseg_2562, 2 + TileArrayOffset_arg0 * 5, 8);     // UWMotionParamArray.SubArray_dseg_67d6_3FC_ptr_to_25C4_maybemotion.GetParam2_BlockSize5(TileArrayOffset_arg0);

            byte cl = (byte)((UWMotionParamArray.TileAttributesArray[temp_index] & 0xF0) >> 1);
            var tiletype = UWMotionParamArray.TileAttributesArray[temp_index] & 0xF;

            switch (tiletype)
            {
                case UWTileMap.TILE_SOLID://0
                    {
                        return 0x80;
                    }
                case UWTileMap.TILE_DIAG_SE://2
                    {
                        if (Loader.getAt(UWMotionParamArray.SubArray.dseg_2562, 4 + TileArrayOffset_arg0 * 5, 8) >= Loader.getAt(UWMotionParamArray.SubArray.dseg_2562, 3 + TileArrayOffset_arg0 * 5, 8))
                        {
                            cl = 0x80;
                        }
                        arg2 = 1;
                        return cl;
                    }
                case UWTileMap.TILE_DIAG_SW://3
                    {
                        if (Loader.getAt(UWMotionParamArray.SubArray.dseg_2562, 3 + TileArrayOffset_arg0 * 5, 8) + Loader.getAt(UWMotionParamArray.SubArray.dseg_2562, 4 + TileArrayOffset_arg0 * 5, 8) >= 7)
                        {
                            cl = 0x80;
                        }
                        arg2 = 1;
                        return (byte)cl;
                    }
                case UWTileMap.TILE_DIAG_NE://4
                    {
                        if (Loader.getAt(UWMotionParamArray.SubArray.dseg_2562, 3 + TileArrayOffset_arg0 * 5, 8) + Loader.getAt(UWMotionParamArray.SubArray.dseg_2562, 4 + TileArrayOffset_arg0 * 5, 8) <= 7)
                        {
                            cl = 0x80;
                        }
                        arg2 = 1;
                        return cl;
                    }
                case UWTileMap.TILE_DIAG_NW://5
                    {
                        if ( Loader.getAt(UWMotionParamArray.SubArray.dseg_2562, 4 + TileArrayOffset_arg0 * 5, 8)  <= Loader.getAt(UWMotionParamArray.SubArray.dseg_2562, 3 + TileArrayOffset_arg0 * 5, 8))
                        {
                            cl = 0x80;
                        }
                        arg2 = 1;
                        return cl;
                    }
                case UWTileMap.TILE_SLOPE_N://6
                    {
                        return (byte)(cl + (Loader.getAt(UWMotionParamArray.SubArray.dseg_2562, 4 + TileArrayOffset_arg0 * 5, 8) & 0x7));
                    }
                case UWTileMap.TILE_SLOPE_S://7
                    {
                        return (byte)(cl + 7 - (Loader.getAt(UWMotionParamArray.SubArray.dseg_2562, 4 + TileArrayOffset_arg0 * 5, 8) & 0x7));
                    }
                case UWTileMap.TILE_SLOPE_E://TILE_SLOPE_E
                    {
                        return (byte)(cl + (Loader.getAt(UWMotionParamArray.SubArray.dseg_2562, 3 + TileArrayOffset_arg0 * 5, 8) & 0x7));
                    }
                case UWTileMap.TILE_SLOPE_W:
                    {
                        return (byte)(cl + 7 - (Loader.getAt(UWMotionParamArray.SubArray.dseg_2562, 3 + TileArrayOffset_arg0 * 5, 8) & 0x7));
                    }
                case UWTileMap.TILE_OPEN://1
                default:
                    {
                        return cl;
                    }
            }
        }

        static int GetNeighbourTileHeightDetails_seg028_2941_217(int arg0, int arg2)
        {
            var si = 0;
            var var2 = GetTileHeightAdjustedByTileType_seg028_2941_E(arg0, out int var1);

            if (var2 != 0x80)
            {//seg028_2941_23D
                if (MotionCalcArray.z4 + arg2 >= var2)
                {//seg028_2941_25A:
                    if (MotionCalcArray.z4 - arg2 <= var2)
                    {//seg028_2941_277
                        var index = getAt(UWMotionParamArray.SubArray.dseg_2562, 2 + arg0 * 5, 8);
                        si = si | 8 << ((UWMotionParamArray.TileAttributesArray[index] & 0x300) >> 8);
                    }
                    else
                    {//seg028_2941_271:
                        si = si | 0x800;
                    }
                }
                else
                {//seg028_2941_258
                    si = si | 0x100;
                }
            }
            else
            {
                si = si | 0x200;
            }
            //seg028_2941_2A0:
            setAt(UWMotionParamArray.SubArray.dseg_2562, 5 + arg0 * 5, 16, si);

            if (MotionCalcArray.Unk11 < var2)
            {//seg028_2941_2BF:
                MotionCalcArray.Unk11 = var2;
            }
            return SBB(var1);
            // if (var1 < 0)//TODO SBB(var1)
            // {
            //     return 0;
            // }
            // else
            // {
            //     if (var1 > 0)
            //     {
            //         return 0;
            //     }
            //     else
            //     {
            //         return 1;
            //     }
            // }

        }


        /// <summary>
        /// Updates x,y,z positions and heading
        /// </summary>
        /// <param name="MotionParams"></param>
        static void StoreNewXYZH_seg031_2CFA_800(UWMotionParamArray MotionParams)
        {
            MotionParams.x_0 = (short)((MotionCalcArray.x0 << 5) + (UWMotionParamArray.RelatedToMotionX_dseg_67d6_3FE >> 8));
            MotionParams.y_2 = (short)((MotionCalcArray.y2 << 5) + (UWMotionParamArray.RelatedToMotionY_dseg_67d6_400 >> 8));
            MotionParams.z_4 = (short)((MotionCalcArray.z4 << 3) + (UWMotionParamArray.RelatedToMotionZ_dseg_67d6_402 >> 8));

            if ((MotionCalcArray.UnkC_terrain_base & 0x2000) != 0)
            {
                if (Math.Abs(MotionCalcArray.z4_base - MotionCalcArray.Unk10_base) <= MotionParams.radius_22)
                {
                    if (MotionCalcArray.MotionArrayObjectIndexA_base == 1)
                    {//the player
                        if (MotionParams.unk_a_pitch == 0)
                        {
                            MotionParams.z_4 = (short)GetTileZOffset_seg028_2941_1AF(MotionParams.x_0, MotionParams.y_2);
                        }
                    }
                }
            }
            MotionParams.heading_1E = MotionCalcArray.Heading6_base;
        }

        static int ProcessCollisions_seg031_2CFA_12B8(UWMotionParamArray MotionParams, int arg0)
        {
            var var1 = 0;
            var var2 = 0;
            if (arg0 == -1)
            {
                //seg031_2CFA_12CD:
                ScanForCollisions(0, 0);
                SetCollisionTarget_seg031_2CFA_10E(MotionParams, 0);
                MotionParams.tilestate25 = UWMotionParamArray.MotionParam0x25_dseg_67d6_26A9;//stores value
                if (UWMotionParamArray.dseg_67d6_26A5 != 0)
                {
                    var1 = 1;
                }
            }
            else
            {
                //seg031_2CFA_12F6
                //UWMotionParamArray.CalculateMotionGlobal_dseg_67d6_25DB--;
                MotionCalcArray.Unk17_base--;
            }
            //seg031_2CFA_12FE

            if (MotionParams.unk_a_pitch == 0)
            {
                var2 = LikelyTranslateXY_seg031_2CFA_8A6(MotionParams, 0, arg0);
            }
            else
            {
                var2 = MAYBEGRAVITYZ_seg031_2CFA_1138(MotionParams, 0, arg0);
            }

            if (var1 != 0)
            {
                var si = GetCollisionHeightState_seg031_2CFA_13B2(MotionParams);
                UWMotionParamArray.MotionParam0x25_dseg_67d6_26A9 = MotionParams.tilestate25;
                MotionParams.tilestate25 = GetTileState(si);
            }

            return var2;
        }



        static void DoTileCollisionMaybe_seg031_2CFA_179C(uwObject projectile, UWMotionParamArray MotionParams)
        {
            var var3 = 0;
            var var2 = GetCollisionHeightState_seg031_2CFA_13B2(MotionParams);
            UWMotionParamArray.MotionParam0x25_dseg_67d6_26A9 = MotionParams.tilestate25;
            MotionParams.tilestate25 = GetTileState(var2);
            if ((var2 & 0xC000) == 0)//var2 getting screwed up here?
            {//seg031_2CFA_17ED:
                var tmp = UWMotionParamArray.PtrTo267D2_dseg_67d6_26B8_table0;
                var2 = var2 & ~tmp;
                var result = false;
                if (var2 != 0)
                {
                    if ((UWMotionParamArray.dseg_67d6_26BA_MotionHandler2 & var2) != 0)
                    {
                        //seg031_2CFA_180F:
                        //Debug.Print($"Call motion code at {UWMotionParamArray.dseg_67d6_26c2_LikeMagicProjectile8} with param {var2}");
                        result = NPCMotionCollision_seg006_1413_ABF(critter: projectile, arg0: var2, motionparams: MotionParams);
                        if (result)
                        {
                            //seg031_2CFA_1818:
                            ProcessCollisions_seg031_2CFA_12B8(MotionParams, -1);
                            UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414 = (short)(UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E + 1);
                            return;
                        }
                    }
                    //seg031_2CFA_1829:   
                    if ((var2 & 0x700) != 0)
                    {//when var2>0 then 0, when var2==0 then 1,
                        //seg031_2CFA_1830: 
                        seg031_2CFA_C5C(MotionParams, SBB(var2 & 0x400));

                        var3 = 1;
                        if (
                            ((var2 & 0x1000) != 0)
                            &&
                            (MotionParams.unk_10_Z == 0)
                            )
                        {
                            MotionParams.unk_10_Z = -4;
                            seg031_2CFA_78A(MotionParams, var3);
                        }

                    }
                }
            }
            else
            {//seg031_2CFA_17CE:
                ProcessCollisions_seg031_2CFA_12B8(MotionParams, -1);
                if ((var2 & 0x4000) == 0)
                {
                    //seg031_2CFA_17E4:           
                    UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414 = (short)(UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E + 1);
                }
                else
                {
                    //seg031_2CFA_17DE:
                    ZeroiseMotionValues_seg031_2CFA_7BF(MotionParams);
                }
            }
        }

        /// <summary>
        /// Looks like this updates the x and y values
        /// </summary>
        /// <param name="MotionParams"></param>
        /// <param name="arg0"></param>
        /// <param name="si_arg2"></param>
        /// <returns></returns>
        static int LikelyTranslateXY_seg031_2CFA_8A6(UWMotionParamArray MotionParams, int arg0, int si_arg2)
        {
            var cx = arg0;
            var ax = 0;
            if (si_arg2 == -1)
            {
                ax = 1;
            }
            if (UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E + ax > UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414)
            {//seg031_2CFA_8CB:
                if (si_arg2 == 1)
                {//seg031_2CFA_8D4:
                    UWMotionParamArray.SetMotionXY3FE(
                        index: UWMotionParamArray.dseg_67d6_40C_indexer,
                        value: (short)(UWMotionParamArray.GetMotionXY_3FE(UWMotionParamArray.dseg_67d6_40C_indexer) + UWMotionParamArray.GetMotionXY_404(UWMotionParamArray.dseg_67d6_40C_indexer))
                        );
                }
                else
                {//seg031_2CFA_8E6
                    UWMotionParamArray.SetMotionXY3FE(
                        index: UWMotionParamArray.dseg_67d6_40C_indexer,
                        value: (short)(UWMotionParamArray.GetMotionXY_3FE(UWMotionParamArray.dseg_67d6_40C_indexer) - UWMotionParamArray.GetMotionXY_404(UWMotionParamArray.dseg_67d6_40C_indexer))
                        );
                }

                //seg031_2CFA_8FA:
                if (UWMotionParamArray.GetMotionXY_404(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer) * si_arg2 <= 0)
                {
                    //seg031_2CFA_919:
                    var tmp = DataLoader.getAt(MotionCalcArray.PtrToMotionCalc, UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer * 2, 16);
                    tmp--;
                    DataLoader.setAt(MotionCalcArray.PtrToMotionCalc, UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer * 2, 16, (int)tmp);
                }
                else
                {
                    //seg031_2CFA_90F:
                    var tmp = DataLoader.getAt(MotionCalcArray.PtrToMotionCalc, UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer * 2, 16);
                    tmp++;
                    DataLoader.setAt(MotionCalcArray.PtrToMotionCalc, UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer * 2, 16, (int)tmp);
                }

                //seg031_2CFA_926:
                if ((UWMotionParamArray.GetMotionXY_3FE(UWMotionParamArray.dseg_67d6_40C_indexer) & 0xE000) != 0)
                {
                    cx = 1;
                    if (UWMotionParamArray.GetMotionXY_3FE(UWMotionParamArray.dseg_67d6_40C_indexer) > 0)
                    {//seg031_2CFA_944
                        var tmp = DataLoader.getAt(MotionCalcArray.PtrToMotionCalc, UWMotionParamArray.dseg_67d6_40C_indexer * 2, 16);
                        tmp++;
                        DataLoader.setAt(MotionCalcArray.PtrToMotionCalc, UWMotionParamArray.dseg_67d6_40C_indexer * 2, 16, (int)tmp);
                    }
                    else
                    {//seg031_2CFA_953
                        var tmp = DataLoader.getAt(MotionCalcArray.PtrToMotionCalc, UWMotionParamArray.dseg_67d6_40C_indexer * 2, 16);
                        tmp--;
                        DataLoader.setAt(MotionCalcArray.PtrToMotionCalc, UWMotionParamArray.dseg_67d6_40C_indexer * 2, 16, (int)tmp);
                    }

                    //seg031_2CFA_960:
                    UWMotionParamArray.SetMotionXY3FE(
                        index: UWMotionParamArray.dseg_67d6_40C_indexer,
                        value: (short)(UWMotionParamArray.GetMotionXY_3FE(UWMotionParamArray.dseg_67d6_40C_indexer) & 0x1FFF));
                }
                //seg031_2CFA_96C:
                UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414 += (short)si_arg2;
                return 1;
            }
            else
            {//seg031_2CFA_975:
                if (si_arg2 == 1)
                {
                    var temp = (UWMotionParamArray.GetMotionXY_404(UWMotionParamArray.dseg_67d6_40C_indexer) >> 5) * (UWMotionParamArray.dseg_67d6_410 >> 8);
                    UWMotionParamArray.SetMotionXY3FE(
                        index: UWMotionParamArray.dseg_67d6_40C_indexer,
                        value: (short)(UWMotionParamArray.GetMotionXY_3FE(UWMotionParamArray.dseg_67d6_40C_indexer) + temp));
                }
                else
                {//seg031_2CFA_99C:
                    var temp = (UWMotionParamArray.GetMotionXY_404(UWMotionParamArray.dseg_67d6_40C_indexer) >> 5) * (UWMotionParamArray.dseg_67d6_410 >> 8);
                    UWMotionParamArray.SetMotionXY3FE(
                        index: UWMotionParamArray.dseg_67d6_40C_indexer,
                        value: (short)(UWMotionParamArray.GetMotionXY_3FE(UWMotionParamArray.dseg_67d6_40C_indexer) - temp));
                }

                //seg031_2CFA_9BC:
                if (si_arg2 * UWMotionParamArray.GetMotionXY_404(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer) <= 0)
                {//seg031_2CFA_9DB:
                    UWMotionParamArray.SetMotionXY3FE(
                        index: UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer,
                        value: (short)(UWMotionParamArray.GetMotionXY_3FE(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer) - UWMotionParamArray.dseg_67d6_410));
                }
                else
                {//seg031_2CFA_9D0:
                    UWMotionParamArray.SetMotionXY3FE(
                        index: UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer,
                        value: (short)(UWMotionParamArray.GetMotionXY_3FE(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer) + UWMotionParamArray.dseg_67d6_410));
                }
                //seg031_2CFA_9E8:
                if ((UWMotionParamArray.GetMotionXY_3FE(0) & 0xE000) != 0)
                {
                    cx = 1;
                    if (UWMotionParamArray.GetMotionXY_3FE(0) <= 0)
                    {
                        //seg031_2CFA_A02:
                        MotionCalcArray.x0--;
                    }
                    else
                    {//seg031_2CFA_A00:
                        MotionCalcArray.x0++;
                    }
                    //seg031_2CFA_A08:
                    UWMotionParamArray.SetMotionXY3FE(index: 0, value: (short)(UWMotionParamArray.GetMotionXY_3FE(0) & 0x1fff));
                }

                if ((UWMotionParamArray.GetMotionXY_3FE(1) & 0xE000) != 0) //or _400
                {
                    //seg031_2CFA_A16:
                    cx = 1;
                    if (UWMotionParamArray.GetMotionXY_3FE(1) > 0)
                    {
                        //seg031_2CFA_A24:
                        MotionCalcArray.y2++;
                    }
                    else
                    {
                        //seg031_2CFA_A29: 
                        MotionCalcArray.y2--;
                    }
                    UWMotionParamArray.SetMotionXY3FE(index: 1, value: (short)(UWMotionParamArray.GetMotionXY_3FE(1) & 0x1fff));
                }
                UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414 += (short)si_arg2; //this is a loop control value
                return cx;
            }
        }

        static int MAYBEGRAVITYZ_seg031_2CFA_1138(UWMotionParamArray MotionParams, int arg0, int arg2)
        {
            // Debug.Print($"Step {iteration} Gravity");
            var di = arg2;
            short ax;
            int var2;
            if (di == -1)
            {
                ax = 1;
            }
            else
            {
                ax = 0;
            }
            //seg031_2CFA_114F
            if (UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E + ax <= UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414)
            {
                //seg031_2CFA_117A: 
                if (UWMotionParamArray.dseg_67d6_410 == 0)
                {//seg031_2CFA_11C6
                    //toconfirm is this 0x40 is correct.
                    var2 = 0x40 * (UWMotionParamArray.Gravity_related_dseg_67d6_408 / 0x800) * di * UWMotionParamArray.Gravity_Related_dseg_67d6_41F;
                }
                else
                {//Seg031_2CFA_1181
                    var2 = UWMotionParamArray.dseg_67d6_410 * (UWMotionParamArray.Gravity_related_dseg_67d6_408 / 0x800) * di * UWMotionParamArray.Gravity_Related_dseg_67d6_41F;
                    var2 = var2 / 0x100;
                }
            }
            else
            {//2CFA:115B
                if (UWMotionParamArray.Gravity_related_dseg_67d6_408 * di > 0)
                {
                    var2 = UWMotionParamArray.Gravity_Related_dseg_67d6_41F << 5;
                }
                else
                {
                    //seg031_2CFA_116F
                    var2 = -(UWMotionParamArray.Gravity_Related_dseg_67d6_41F << 5);
                }
            }

            //seg031_2CFA_11F3

            var2 = var2 + UWMotionParamArray.RelatedToMotionZ_dseg_67d6_402;
            short si;
            if (var2 >= 0)
            {//seg031_2CFA_1211:
                si = (short)(var2 / 0x800);
            }
            else
            {//seg031_2CFA_1223
                si = (short)(Math.Abs(var2) / 0x800);
                si++;
                si = (short)-si;
            }

            //seg031_2CFA_1240:
            UWMotionParamArray.RelatedToMotionZ_dseg_67d6_402 = (short)(var2 & 0x7FF);

            //seg031_2CFA_124B:
            if (di == -1)
            {//seg031_2CFA_1254:
                MotionCalcArray.z4_base = (ushort)(MotionCalcArray.z4_base + si);
                //seg031_2CFA_12A9
                return LikelyTranslateXY_seg031_2CFA_8A6(MotionParams, arg0, di);
            }
            else
            {//seg031_2CFA_1259
                if (si <= 0)
                {
                    if (si < 0)
                    {//2CFA:1285
                        MotionParams.tilestate25 = 0x10;
                        if (MotionCalcArray.z4_base + si >= UWMotionParamArray.CollisionZposHeightRelated_dseg_67d6_419)
                        {
                            MotionCalcArray.z4_base = (ushort)(MotionCalcArray.z4_base + si);
                            //seg031_2CFA_12A9
                            return LikelyTranslateXY_seg031_2CFA_8A6(MotionParams, arg0, di);
                        }
                        else
                        {
                            //seg031_2CFA_12A1:
                            DoCollision_seg031_2CFA_D1F(MotionParams);
                            return 0;
                        }
                    }
                    else
                    {//si==0
                        return LikelyTranslateXY_seg031_2CFA_8A6(MotionParams, arg0, di);
                    }
                }
                else
                {//si>0
                    MotionParams.tilestate25 = 0x10;
                    if (MotionCalcArray.z4_base + si > UWMotionParamArray.CollisionZposHeightRelated_dseg_67d6_419)
                    {
                        DoCollision_seg031_2CFA_D1F(MotionParams);
                        return 0;
                    }
                    else
                    {
                        MotionCalcArray.z4_base = (ushort)(MotionCalcArray.z4_base + si);
                        return LikelyTranslateXY_seg031_2CFA_8A6(MotionParams, arg0, di);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a value indicating the state of the tile the player/object has steped on or landed on.
        /// </summary>
        /// <param name="arg0"></param>
        /// <returns></returns>
        public static byte GetTileState(int arg0)
        {
            if ((arg0 & 0x1000) == 0)
            {
                if ((arg0 & 0x4) == 0)
                {
                    if ((arg0 & 0x88) == 0)
                    {
                        if ((arg0 & 0x10) == 0)
                        {
                            if ((arg0 & 0x20) == 0)
                            {
                                return 8;// on snow/ice. (Todo check what happens here in UW1)
                            }
                            else
                            {
                                return 4;//on lava
                            }
                        }
                        else
                        {
                            return 2;//in water?
                        }
                    }
                    else
                    {
                        return 1;//bits 3,7 is grounded
                    }
                }
                else
                {
                    if (
                        (MotionCalcArray.MotionArrayObjectIndexA_base == 1)
                        &&
                        ((arg0 & 3) == 1)
                        &&
                        ((arg0 & 0xF8) != (arg0 & 0x90))
                        )
                    {
                        return 0x20;//player about to enter water?
                    }
                    else
                    {
                        return (byte)(1 << (arg0 & 0x3));
                    }
                }
            }
            else
            {
                return 0x10;//bit 12, is jumping
            }
        }


        /// <summary>
        /// Sets motion params to zero (as if object has landed on the ground.)
        /// </summary>
        /// <param name="MotionParams"></param>
        static void ZeroiseMotionValues_seg031_2CFA_7BF(UWMotionParamArray MotionParams)
        {
            MotionParams.unk_8_y = 0;
            MotionParams.unk_e_Y = 0;
            MotionParams.unk_6_x = 0;
            MotionParams.unk_c_X = 0;
            MotionParams.unk_a_pitch = 0;
            MotionParams.unk_10_Z = 0;
            MotionParams.unk_14 = 0;
            UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E++;//Stops loop in Calculate Motion.
            UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414 = 0;
        }

        static void seg031_2CFA_C5C(UWMotionParamArray MotionParams, int arg0)
        {
            //Debug.Print("seg031_2CFA_C5C");
            if (MotionCalcArray.Unk17_base <= 0)
            {
                int si;
                //seg031_2CFA_C72:
                if (arg0 != 0)
                {
                    seg028_2941_803(MotionParams);//Unk12_base gets set in here.
                    si = MotionCalcArray.Unk12_base;

                    if (si == 9)
                    {
                        //seg031_2CFA_C8B: for arg0 !=0
                        si = UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer << 1;
                    }
                }
                else
                {
                    //seg031_2CFA_C8B: for arg0 == 0
                    si = UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer << 1;
                }
                //seg031_2CFA_C92:
                ProcessCollisions_seg031_2CFA_12B8(MotionParams, -1);

                var result = HeadingRelated_seg031_2CFA_A49(MotionParams, (short)dseg_67d6_421[si]);

                if (result == 0)
                {//seg31_2cfa:cbc
                    UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414 = (short)(UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E + 1);
                }
                else
                {//seg31_2cfa:cad
                    seg031_2CFA_78A(MotionParams, 1);
                    MotionCalcArray.Unk17_base = 2;
                }


            }
            else
            {
                //seg031_2CFA_C69:
                ProcessCollisions_seg031_2CFA_12B8(MotionParams, -1);
                UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414 = (short)(UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E + 1);
            }
        }


        static void seg028_2941_803(UWMotionParamArray MotionParams)
        {
            var si = 0;
            var di = 0;
            var var3 = 0;
            var var4 = 0;
            var var1 = 0;
            var var2 = 0;
            var cl = 0;
            var var8 = 0;
            var var9 = 0;


            while (cl < 4)
            {
                //seg028_2941_821:
                var temp = DataLoader.getAt(UWMotionParamArray.SubArray.dseg_2562, 5 + cl * 5, 16);
                if ((temp & 0xF8) == 0)
                {
                    var2 += dseg_67d6_3C4_Lookup[cl];
                    var1 += dseg_67d6_3C4_Lookup[(cl + 3) & 3];
                    di++;
                }

                if ((temp & 0x300) != 0)
                {
                    var4 += dseg_67d6_3C4_Lookup[cl];
                    var3 += dseg_67d6_3C4_Lookup[(cl + 3) & 3];
                    si++;
                }
                cl++;
            }

            if (si != 0)
            {
                //seg028_2941_891:
                var temp = 3 * (var4 / si) + (var3 / si);
                MotionCalcArray.Unk12 = dseg_67d6_3C4_Lookup[8 + temp];

                if (si == 1)
                {//untested here
                    //seg028_2941_8BB:
                    if (MotionCalcArray.Unk12 / 2 != 0)
                    {
                        if (UWMotionParamArray.dseg_67d6_2584 != 0)
                        {
                            var var5 = (9 - MotionCalcArray.Unk12) & 0x7;
                            var var6 = MotionCalcArray.Heading6 >> 0xD;
                            switch ((var6 - var5) & 0x7)
                            {
                                case 0:
                                case 1:
                                    {//seg028_2941_90F
                                        MotionCalcArray.Unk12 = 9;
                                        break;
                                    }
                                case 2:
                                case 3:
                                    {//seg028_2941_912
                                        MotionCalcArray.Unk12 = (sbyte)((MotionCalcArray.Unk12 + 1) & 0x7);
                                        break;
                                    }
                                case 4:
                                case 5:
                                    {//seg028_2941_923:
                                        var var7 = (MotionCalcArray.Unk12 - 1) >> 1;
                                        MotionCalcArray.Unk12--;
                                        switch (var7)
                                        {
                                            case 0: //seg028_2941_948:
                                                {
                                                    var8 = (int)DataLoader.getAt(UWMotionParamArray.SubArray.dseg_2562, 3 + (5 * var7), 8);
                                                    var9 = (int)DataLoader.getAt(UWMotionParamArray.SubArray.dseg_2562, 4 + (5 * var7), 8);
                                                    break;
                                                }
                                            case 1: //seg028_2941_95D:
                                                {
                                                    var8 = (int)DataLoader.getAt(UWMotionParamArray.SubArray.dseg_2562, 3 + (5 * var7), 8);
                                                    var9 = 8 - (int)DataLoader.getAt(UWMotionParamArray.SubArray.dseg_2562, 4 + (5 * var7), 8);
                                                    break;
                                                }
                                            case 2: //seg028_2941_987
                                                {
                                                    var8 = (int)DataLoader.getAt(UWMotionParamArray.SubArray.dseg_2562, 4 + (5 * var7), 8);
                                                    var9 = (int)DataLoader.getAt(UWMotionParamArray.SubArray.dseg_2562, 3 + (5 * var7), 8);
                                                    break;
                                                }
                                            case 3: //seg028_2941_9AC:
                                                {
                                                    var8 = (int)DataLoader.getAt(UWMotionParamArray.SubArray.dseg_2562, 3 + (5 * var7), 8);
                                                    var9 = (int)DataLoader.getAt(UWMotionParamArray.SubArray.dseg_2562, 4 + (5 * var7), 8);
                                                    break;
                                                }
                                        }

                                        //seg028_2941_9D4:
                                        if (var8 < var9)
                                        {
                                            MotionCalcArray.Unk12 = (sbyte)((MotionCalcArray.Unk12 + 2) & 0x7);
                                        }
                                        if (var8 == var9)
                                        {
                                            MotionCalcArray.Unk12++;
                                        }
                                        break;
                                    }

                                case 6:
                                case 7:
                                    {
                                        MotionCalcArray.Unk12 = (sbyte)((MotionCalcArray.Unk12 + 7) & 7);
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            else
            {
                //seg028_2941_A0B:
                MotionCalcArray.Unk12 = 9;
            }

            //seg028_2941_A13:
            if ((di == 1) || (di == 2))
            {
                var temp = 3 * (var2 / -di) + (var2 / -di);
                MotionCalcArray.Unk13 = dseg_67d6_3C4_Lookup[8 + temp];
            }
            else
            {
                MotionCalcArray.Unk13 = 9;
            }
        }

        static int HeadingRelated_seg031_2CFA_A49(UWMotionParamArray MotionParams, short arg0)
        {
            var di_arg0 = arg0;
            short si = 0;
            var var3 = 0;
            var var4 = 0;
            if ((MotionParams.unk_17 & 0x40) == 0)
            {
                //seg031_2CFA_A63:
                if (MotionParams.unk_a_pitch != 0)
                {
                    si = (short)(MotionParams.unk_a_pitch / 0x10);
                    si = (short)(si * (MotionParams.unk_16_relatedtoPitch + 1));
                    MotionParams.unk_a_pitch = (short)si;
                }
                si = (short)(di_arg0 - MotionCalcArray.Heading6_base);

                if ((si > 0x4000) || (si < -16384))
                {
                    di_arg0 += -32768;//  0x8000;
                    si += -32768;
                }

                if ((MotionParams.unk_17 & 0x80) != 0)
                {
                    //seg031_2CFA_AB9
                    if ((Math.Abs(si) > 0x3000) && (Math.Abs(si) < 0x5000))
                    {
                        MotionParams.unk_26_falldamage += MotionParams.unk_14;
                        return 0;
                    }
                    else
                    {
                        //seg031_2CFA_ADC: 
                        if (MotionCalcArray.Heading6_base != di_arg0)
                        {
                            //seg031_2CFA_AE5:
                            MotionCalcArray.Heading6_base = di_arg0;
                            if ((di_arg0 & 0x2000) != 0)
                            {
                                //seg031_2CFA_AF2:
                                var var5 = GetTileAttribute8_seg028_2941_A6D();
                                if ((var5 >= 2) && (var5 <= 5))
                                {
                                    var4 = 0;
                                    //seg031_2CFA_B06: 
                                    if ((di_arg0 & 0x4000) == 0)
                                    {
                                        var3 = 1;
                                    }
                                    else
                                    {
                                        var3 = 0;
                                    }
                                    int dx; int ax;
                                    //seg031_2CFA_B19
                                    if (si >= 0)
                                    {
                                        dx = 0;
                                    }
                                    else
                                    {
                                        dx = 1;
                                    }

                                    //seg031_2CFA_B24:
                                    if ((di_arg0 & 0x8000) == 0)
                                    {
                                        ax = 0;
                                    }
                                    else
                                    {
                                        ax = 1;
                                    }

                                    //seg031_2CFA_B35:
                                    if (ax != dx)
                                    {
                                        var3 = var3 ^ 1;  //XOR 1
                                        var4 = var4 ^ 1;
                                    }

                                    //seg031_2CFA_B47:
                                    UWMotionParamArray.RelatedToMotionX_dseg_67d6_3FE = (short)(var3 * 0x1F00);
                                    UWMotionParamArray.RelatedToMotionY_dseg_67d6_400 = (short)(var4 * 0x1F00);
                                }
                                else
                                {
                                    //seg031_2CFA_B64:
                                    UWMotionParamArray.RelatedToMotionX_dseg_67d6_3FE = 0x1000;
                                    UWMotionParamArray.RelatedToMotionY_dseg_67d6_400 = 0x1000;
                                }
                            }
                        }
                        else
                        {
                            //seg031_2CFA_AE2: 
                            var bl = 0;
                            var cl = 0;
                            if ((di_arg0 & 0x4000) == 0)
                            {
                                cl = 1;
                            }
                            var dx = 0;
                            var ax = 0;
                            if (si > 0)
                            {
                                dx = 1;
                            }

                            //seg031_2CFA_B8F: 
                            if ((di_arg0 & 0x8000) != 0)
                            {
                                ax = 1;
                            }

                            //seg031_2CFA_B9F:
                            if (ax != dx)
                            {
                                cl = cl ^ 1;
                                bl = bl ^ 1;
                            }

                            UWMotionParamArray.RelatedToMotionX_dseg_67d6_3FE = (short)(cl * 0x1F00);
                            UWMotionParamArray.RelatedToMotionY_dseg_67d6_400 = (short)(bl * 0x1F00);

                        }
                    }
                }
                else
                {
                    //seg031_2CFA_BC7:
                    if ((Math.Abs(si) > 0x3000) && (Math.Abs(si) < 0x5000))
                    {//seg31_2cfa:BDF
                        MotionCalcArray.Heading6_base = (short)(di_arg0 + si);
                    }
                    else
                    {//seg31_2cfa:BDF
                        var var2 = si / 0xF;
                        MotionCalcArray.Heading6_base = (short)(di_arg0 + (var2 * MotionParams.unk_16_relatedtoPitch));
                    }
                }

                //seg031_2CFA_C0B:
                if ((MotionParams.unk_17 & 0x80) == 0)
                {
                    MotionParams.unk_26_falldamage += (short)((MotionParams.unk_14 * (0xf - MotionParams.unk_16_relatedtoPitch)) / 0xF);
                    MotionParams.unk_14 = (short)((MotionParams.unk_14 * MotionParams.unk_16_relatedtoPitch) / 0xF);
                }
                //seg031_2CFA_C4C:
                MotionParams.heading_1E = MotionCalcArray.Heading6_base;
                return 1;
            }
            else
            {
                return 0;
            }
        }


        static int GetTileAttribute8_seg028_2941_A6D()
        {
            return UWMotionParamArray.TileAttributesArray[8] & 0xF;
        }


        /// <summary>
        /// Offsets z for sloped tiles based on x/y pos within the tile.
        /// </summary>
        /// <param name="xArg0"></param>
        /// <param name="yArg0"></param>
        /// <returns></returns>
        static int GetTileZOffset_seg028_2941_1AF(int xArg0, int yArg0)
        {
            var di_y = yArg0 & 0xFF;
            var si = 0;
            var dx = xArg0 & 0xFF;

            switch (UWMotionParamArray.TileAttributesArray[4] & 0xF)
            {
                case UWTileMap.TILE_SLOPE_N:
                    si = di_y;
                    break;
                case UWTileMap.TILE_SLOPE_S:
                    si = 0xFF - di_y;
                    break;
                case UWTileMap.TILE_SLOPE_E:
                    si = dx;
                    break;
                case UWTileMap.TILE_SLOPE_W:
                    si = 0xFF - dx;
                    break;
            }
            si = si >> 2;
            si = si + ((UWMotionParamArray.TileAttributesArray[4] & 0xF0) << 2);
            return si;
        }

    }//end class
}//end namespace