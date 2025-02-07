using System;
namespace Underworld
{
    public partial class motion : Loader
    {

        static int LikelyIsMagicProjectile_dseg_67d6_26B8;
        static int MotionParam0x25_dseg_67d6_26A9;
        static int CalculateMotionGlobal_dseg_67d6_25DB;

        static int CalculateMotionGlobal_dseg_67d6_26B6;

        public static bool CalculateMotion_TopLevel(uwObject projectile, UWMotionParamArray MotionParams, int MaybeMagicObjectFlag)
        {//seg006_1413_D6A
            MotionParams.speed_12 = projectile.Projectile_Speed << 4;
            CalculateMotion(projectile, MotionParams, MaybeMagicObjectFlag);
            return true;
        }


        static void CalculateMotion(uwObject projectile, UWMotionParamArray MotionParams, int MaybeMagicObjectFlag)
        {
            //TODO
            LikelyIsMagicProjectile_dseg_67d6_26B8 = MaybeMagicObjectFlag;
            MotionParam0x25_dseg_67d6_26A9 = MotionParams.unk_25;
            CalculateMotionGlobal_dseg_67d6_25DB = 0;
            CalculateMotionGlobal_dseg_67d6_26B6 = 0;

            if (seg031_2CFA_412(projectile, MotionParams, 1, 1))
            {
                //do more processing at seg031_2CFA_69
            }
        }

        static bool seg031_2CFA_412(uwObject projectile, UWMotionParamArray MotionParams, int arg0, int arg2)
        {
            short var2 = 0; short var4 = 0;
            short var8 = 0;
            SomethingProjectileHeading_seg021_22FD_EAE(MotionParams.heading_1E, ref var2, ref var4);
            MotionParams.unk_6 = (var2 * MotionParams.unk_14) >> 0xF;
            MotionParams.unk_8 = (var4 * MotionParams.unk_14) >> 0xF;

            //possibly the following are translation vectors
            MotionParams.unk_6 = MotionParams.unk_6 + (MotionParams.unk_c * MotionParams.speed_12);
            MotionParams.unk_8 = MotionParams.unk_6 + (MotionParams.unk_d * MotionParams.speed_12);
            MotionParams.unk_a = MotionParams.unk_a + (MotionParams.unk_10 * MotionParams.speed_12);

            if ((MotionParams.unk_6 | MotionParams.unk_8 | MotionParams.unk_a) == 0)
            {
                return false;
            }
            else
            {
                //seg031_2CFA_4D2
                if (arg0 != 0)
                {
                    CopyMotionValsToAnotherArray_seg031_2CFA_93(MotionParams);
                }
                if (Math.Abs(MotionParams.unk_6) <= Math.Abs(MotionParams.unk_8))
                {
                    UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer = 1;
                }
                else
                {
                    UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer = 0;
                }
                UWMotionParamArray.dseg_67d6_40C_indexer = (UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer + 1) / 2;

                var Param6Lookup = Loader.getAt(MotionParams.data, 6 + UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer * 2, 16);
                if (Param6Lookup <= 0)
                {
                    UWMotionParamArray.dseg_67d6_404[UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer] = -8192;
                }
                else
                {
                    UWMotionParamArray.dseg_67d6_404[UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer] = +8192;
                }
                //tmp = DataLoader.getAt(MotionParams.data, 6 + UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer*2, 16);
                if (Param6Lookup == 0)
                {
                    //seg031_2CFA_5D8:
                    UWMotionParamArray.dseg_67d6_404[UWMotionParamArray.dseg_67d6_40C_indexer] = 1;
                    var8 = 0;
                    UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E = 0;
                    UWMotionParamArray.dseg_67d6_410 = 0;
                    UWMotionParamArray.dseg_67d6_412 = MotionParams.speed_12;
                }
                else
                {
                    //seg031_2CFA_54F
                    var tmp = (UWMotionParamArray.dseg_67d6_404[UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer] / 0x100) * Param6Lookup;
                    tmp = tmp / Param6Lookup;
                    tmp = tmp << 8;

                    UWMotionParamArray.dseg_67d6_404[UWMotionParamArray.dseg_67d6_40C_indexer] = (int)tmp;

                    var8 = (short)(Param6Lookup * MotionParams.speed_12);

                    UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E = Math.Abs(var8) >> 0xD;

                    UWMotionParamArray.dseg_67d6_410 = Math.Abs(var8) & 0x1FFF;

                    UWMotionParamArray.dseg_67d6_412 = (int)Math.Abs(0x2000 / Param6Lookup);
                }

                UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414 = 0;

                if (
                    (UWMotionParamArray.MotionArrayObjectIndex_dseg_67d6_25CE != 1)
                    ||
                    ((UWMotionParamArray.MotionArrayObjectIndex_dseg_67d6_25CE == 1) && MotionParams.unk_a != 0)
                    )
                {
                    if (arg2 != 0)
                    {
                        RelatedToTileAndMotion_seg028_2941_385(MotionParams.SubArray, MotionParams.unk_24);//unk24 is 0 in normal projectile processing
                        seg028_2941_C0E(0, 0);
                    }
                }

                //resume at seg031_2CFA_648:

            }
            return false;//temp      
        }


        static void SomethingProjectileHeading_seg021_22FD_EAE(int heading, ref short Result_arg2, ref short Result_arg4)
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
        static int HeadingLookupCalc(int bx, out short Result_AX, out short Result_BX)
        {
            var cx = bx & 0xff;
            bx = bx >> 8;
            var bp = HeadingLookupTable[bx];
            var ax = HeadingLookupTable[bx + 1];
            ax = (short)(ax - bp);
            ax = (short)(ax * cx);
            if (ax < 0)
            {
                ax = (short)(ax >> 8);
                ax = (short)-Math.Abs(ax);
            }
            else
            {
                ax = (short)(ax >> 8);
            }
            ax = (short)(ax + bp);
            Result_AX = ax;

            bp = HeadingLookupTable[64 + bx];
            ax = HeadingLookupTable[65 + bx];
            ax = (short)(ax - bp);
            ax = (short)(ax * cx);

            bx = ax >> 8;
            if (ax < 0)
            {
                bx = -Math.Abs(bx);
            }
            Result_BX = (short)bx;
            return Result_AX;
        }

        static void CopyMotionValsToAnotherArray_seg031_2CFA_93(UWMotionParamArray MotionParams)
        {
            //store some globals
            UWMotionParamArray.relatedtoheadinginMotion_dseg_67d6_25CA = MotionParams.heading_1E;
            UWMotionParamArray.Likely_RadiusInMotion_dseg_67d6_25CC = MotionParams.radius_22;
            UWMotionParamArray.Likely_HeightInMotion_dseg_67d6_25CD = MotionParams.height_23;
            UWMotionParamArray.MotionArrayObjectIndex_dseg_67d6_25CE = MotionParams.index_20;

            MotionParams.SubArray.Unk0_x = MotionParams.x_0 >> 5;
            MotionParams.SubArray.Unk2_y = MotionParams.y_2 >> 5;
            MotionParams.SubArray.Unk3_z = MotionParams.z_4 >> 3;

            UWMotionParamArray.RelatedToMotionX_dseg_67d6_3FE = (MotionParams.x_0 & 0x1F) << 8;
            UWMotionParamArray.RelatedToMotionY_dseg_67d6_400 = (MotionParams.y_2 & 0x1F) << 8;
            UWMotionParamArray.RelatedToMotionZ_dseg_67d6_402 = (MotionParams.z_4 & 7) << 8;

        }


        /// <summary>
        /// Appears to do stuff with the tile the motion is happening in.
        /// </summary>
        /// <param name="arg0"></param>
        static void RelatedToTileAndMotion_seg028_2941_385(OtherMotionArray SubArray, int arg0)
        {
            //?
            UWMotionParamArray.TileAttributesArray = new int[0x9]; // 9 * 0x1111 or 18 * 0x11?   0-8 entries
            for (int i = 0; i <= UWMotionParamArray.TileAttributesArray.GetUpperBound(0); i++)
            {
                UWMotionParamArray.TileAttributesArray[i] = 0x1111;//default values for the tiles in a 3x3 grid
            }
            var tileX = SubArray.Unk0_x >> 3;
            var tileY = SubArray.Unk2_y >> 3;
            UWMotionParamArray.TileRelatedToMotion_dseg_67d6_257E = UWTileMap.current_tilemap.Tiles[tileX, tileY];

            var xposVar8 = SubArray.Unk0_x & 0x7;
            var yposVarA = SubArray.Unk2_y & 0x7;
            var XYOffsetVar1 = 4;//4 means the center tile in a 3x3 grid of tiles
            var YPosVarA = 0;
            var XPosVarB = 0;
            SubArray.Unk16 = 4;
            SubArray.Unk17_xpos = xposVar8;
            SubArray.Unk18_ypos = yposVarA;
            if (UWMotionParamArray.TileAttributesArray[4] == 0x1111)
            {
                //set value for this tile.
                var tile = UWMotionParamArray.TileRelatedToMotion_dseg_67d6_257E;
                //int terrain = TerrainDatLoader.GetTerrainTypeNo(tile);
                UWMotionParamArray.TileAttributesArray[4] = (int)(tile.tileType) | ((int)tile.floorHeight << 4) | (TerrainDatLoader.GetTerrainTypeNo(tile) << 8);
            }

            seg028_2941_2CF(SubArray, arg0);
            SubArray.UnkE = SubArray.Unkc_terrain;
            SubArray.Unk11 = SubArray.Unk10;

            //seg028_2941_449
            if (SubArray.Unk8 != 0)
            {
                yposVarA -= SubArray.Unk8;
                while (yposVarA < 0)
                {
                    XYOffsetVar1 -= -3;
                    yposVarA += 8;
                }

                xposVar8 -= SubArray.Unk8;
                while (xposVar8 < 0)
                {
                    XYOffsetVar1--;
                    xposVar8 += 8;
                }

                SubArray.Unk2_y = XYOffsetVar1;
                SubArray.Unk3_z = xposVar8;
                SubArray.Unk4 = yposVarA;

                xposVar8 += (SubArray.Unk8 << 1);
                while (xposVar8 > 7)
                {
                    XYOffsetVar1++;
                    xposVar8 -= 8;
                }

                SubArray.Unk7 = XYOffsetVar1;
                SubArray.Unk8 = xposVar8;
                SubArray.Unk9 = yposVarA;
                yposVarA += (SubArray.Unk8 << 1);

                while (yposVarA > 7)
                {
                    XYOffsetVar1 += 3;
                    yposVarA -= 8;
                }

                SubArray.Unkc_terrain = XYOffsetVar1;

                SubArray.UnkD = xposVar8;
                SubArray.UnkE = yposVarA;

                xposVar8 -= (SubArray.Unk8 << 1);
                while (xposVar8 < 0)
                {//again?
                    XYOffsetVar1--;
                    XYOffsetVar1 += 8;
                }

                SubArray.Unk11 = XYOffsetVar1;
                SubArray.Unk12 = xposVar8;
                SubArray.Unk13 = yposVarA;

                //2941:543
                var searchPTR = (int)UWMotionParamArray.TileRelatedToMotion_dseg_67d6_257E.Ptr + (TileOffsetArray[SubArray.Unk2_y] * 4);
                var Tile_Var6 = UWTileMap.GetTileByPTR(searchPTR);
                if (UWMotionParamArray.TileAttributesArray[SubArray.Unk2_y] == 0x1111)
                {//seg028_2941_572:
                    UWMotionParamArray.TileAttributesArray[SubArray.Unk2_y] = (int)(Tile_Var6.tileType) | ((int)Tile_Var6.floorHeight << 4) | (TerrainDatLoader.GetTerrainTypeNo(Tile_Var6) << 8);
                }


                //seg028_2941_5AA:
                searchPTR = (int)UWMotionParamArray.TileRelatedToMotion_dseg_67d6_257E.Ptr + (TileOffsetArray[SubArray.Unk7] * 4);
                Tile_Var6 = UWTileMap.GetTileByPTR(searchPTR);
                if (UWMotionParamArray.TileAttributesArray[SubArray.Unk7] == 0x1111)
                {//seg028_2941_5DA
                    UWMotionParamArray.TileAttributesArray[SubArray.Unk7] = (int)(Tile_Var6.tileType) | ((int)Tile_Var6.floorHeight << 4) | (TerrainDatLoader.GetTerrainTypeNo(Tile_Var6) << 8);
                }

                //seg028_2941_618:
                searchPTR = (int)UWMotionParamArray.TileRelatedToMotion_dseg_67d6_257E.Ptr + (TileOffsetArray[SubArray.Unkc_terrain] * 4);
                Tile_Var6 = UWTileMap.GetTileByPTR(searchPTR);
                if (UWMotionParamArray.TileAttributesArray[SubArray.Unkc_terrain] == 0x1111)
                {//seg028_2941_648
                    UWMotionParamArray.TileAttributesArray[SubArray.Unkc_terrain] = (int)(Tile_Var6.tileType) | ((int)Tile_Var6.floorHeight << 4) | (TerrainDatLoader.GetTerrainTypeNo(Tile_Var6) << 8);
                }

                //seg028_2941_686:
                searchPTR = (int)UWMotionParamArray.TileRelatedToMotion_dseg_67d6_257E.Ptr + (TileOffsetArray[SubArray.Unk11] * 4);
                Tile_Var6 = UWTileMap.GetTileByPTR(searchPTR);
                if (UWMotionParamArray.TileAttributesArray[SubArray.Unk11] == 0x1111)
                {//seg028_2941_6B9
                    UWMotionParamArray.TileAttributesArray[SubArray.Unk11] = (int)(Tile_Var6.tileType) | ((int)Tile_Var6.floorHeight << 4) | (TerrainDatLoader.GetTerrainTypeNo(Tile_Var6) << 8);
                }


                //seg028_2941_6F4
                UWMotionParamArray.dseg_67d6_2584 = 1;
                var var2 = 0;
                while (var2 < 4)
                {
                    if (seg028_2941_217(SubArray, var2, arg0) == 0)
                    {//seg028_2941_718:
                        var temp = getAt(SubArray.data, 5 + var2 * 5, 16);
                        if ((temp & 0x300) == 0)
                        {
                            var var10 = new byte[] { 0x4, 0x10, 0x2, 0x8, 0x4, 0x1 };   //dseg_67d6_3BF
                            var varB = 0;

                            while (varB <= 2)
                            {
                                //seg028_2941_75D
                                var tmp = getAt(SubArray.data, 2 + (5 * ((1 + var2 + (varB << 1)) & 0x3)), 8);

                                if (tmp != getAt(SubArray.data, var2 * 5, 8))
                                {
                                    var tile_index = getAt(SubArray.data, 2 + (var2 * 5), 8);
                                    var tiletype = UWMotionParamArray.TileAttributesArray[tile_index] & 0xF;
                                    //var10[varB+var2]
                                    //test tile attributes
                                    if ((TileTraversalFlags_dseg_67d6_1BA6[tiletype] & var10[varB + var2]) == 0)
                                    {
                                        //seg028_2941_7B2:
                                        DataLoader.setAt(SubArray.data, 5 + (var2 * 5), 16, 0x200);
                                        SubArray.Unk11 = -128;
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
                SubArray.UnkE = SubArray.UnkE | SubArray.Unk5 | SubArray.UnkA | SubArray.UnkF | SubArray.Unk14;
            }
        }


        static int seg028_2941_2CF(OtherMotionArray SubArray, int arg0)
        {//this may behave differently in UW1?
            SubArray.Unkc_terrain = (UWMotionParamArray.TileAttributesArray[4] & 0x300) >> 8;
            int var1;

            SubArray.Unk10 = SomethingWithTileTypes_seg028_2941_E(SubArray, 4, out var1);

            if (SubArray.Unk10 != 0x80)
            {
                if (SubArray.Unk4 + arg0 >= SubArray.Unk10)
                {
                    if (SubArray.Unk4 - arg0 <= SubArray.Unk10)
                    {
                        SubArray.Unkc_terrain = SubArray.Unkc_terrain | 0x4;

                        var tmp = 8 << ((UWMotionParamArray.TileAttributesArray[4] & 0x300) >> 8);

                        SubArray.Unkc_terrain = SubArray.Unkc_terrain | tmp;

                    }
                    else
                    {
                        SubArray.Unkc_terrain = SubArray.Unkc_terrain | 0x800;
                    }
                }
                else
                {
                    SubArray.Unkc_terrain = SubArray.Unkc_terrain | 0x100;
                }
            }
            else
            {
                SubArray.Unkc_terrain = SubArray.Unkc_terrain | 0x200;
            }

            if ((UWMotionParamArray.TileAttributesArray[4] & 0xF) >= 6)
            {
                SubArray.Unk0_x = SubArray.Unk0_x | 0x2000;
            }
            if (var1 < 0)
            {
                return 0;
            }
            else
            {
                if (var1 > 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
        }

        static void seg028_2941_C0E(int arg0, int arg2)
        {
            //?
        }

        static sbyte SomethingWithTileTypes_seg028_2941_E(OtherMotionArray SubArray, int TileArrayOffset_arg0, out int arg2)
        {
            arg2 = 0;
            var temp_index = Loader.getAt(SubArray.data, 2 + TileArrayOffset_arg0 * 5, 8);     // MotionParams.SubArray_dseg_67d6_3FC_ptr_to_25C4_maybemotion.GetParam2_BlockSize5(TileArrayOffset_arg0);

            sbyte cl = (sbyte)((UWMotionParamArray.TileAttributesArray[temp_index] & 0xF0) >> 1);
            var tiletype = UWMotionParamArray.TileAttributesArray[temp_index] & 0xF;

            switch (tiletype)
            {
                case UWTileMap.TILE_SOLID://0
                    {
                        return -128;
                    }
                case UWTileMap.TILE_DIAG_SE://2
                    {
                        if (Loader.getAt(SubArray.data, 4 + TileArrayOffset_arg0 * 5, 8) < Loader.getAt(SubArray.data, 3 + TileArrayOffset_arg0 * 5, 8))
                        {
                            cl = -128;
                        }
                        arg2 = 1;
                        return cl;
                    }
                case UWTileMap.TILE_DIAG_SW://3
                    {
                        if (Loader.getAt(SubArray.data, 3 + TileArrayOffset_arg0 * 5, 8) + Loader.getAt(SubArray.data, 4 + TileArrayOffset_arg0 * 5, 8) >= 7)
                        {
                            cl = -128;
                        }
                        arg2 = 1;
                        return (sbyte)cl;
                    }
                case UWTileMap.TILE_DIAG_NE://4
                    {
                        if (Loader.getAt(SubArray.data, 3 + TileArrayOffset_arg0 * 5, 8) + Loader.getAt(SubArray.data, 4 + TileArrayOffset_arg0 * 5, 8) <= 7)
                        {
                            cl = -128;
                        }
                        arg2 = 1;
                        return cl;
                    }
                case UWTileMap.TILE_DIAG_NW://5
                    {
                        if (Loader.getAt(SubArray.data, 3 + TileArrayOffset_arg0 * 5, 8) <= Loader.getAt(SubArray.data, 4 + TileArrayOffset_arg0 * 5, 8))
                        {
                            cl = -128;
                        }
                        arg2 = 1;
                        return cl;
                    }
                case UWTileMap.TILE_SLOPE_N://6
                    {
                        return (sbyte)(cl + (Loader.getAt(SubArray.data, 4 + TileArrayOffset_arg0 * 5, 8) & 0x7));
                    }
                case UWTileMap.TILE_SLOPE_S://7
                    {
                        return (sbyte)(cl + 7 - (Loader.getAt(SubArray.data, 4 + TileArrayOffset_arg0 * 5, 8) & 0x7));
                    }
                case UWTileMap.TILE_SLOPE_E://TILE_SLOPE_E
                    {
                        return (sbyte)(cl + (Loader.getAt(SubArray.data, 3 + TileArrayOffset_arg0 * 5, 8) & 0x7));
                    }
                case UWTileMap.TILE_SLOPE_W:
                    {
                        return (sbyte)(cl + 7 - (Loader.getAt(SubArray.data, 3 + TileArrayOffset_arg0 * 5, 8) & 0x7));
                    }
                case UWTileMap.TILE_OPEN://1
                default:
                    {
                        return cl;
                    }
            }
        }

        static int seg028_2941_217(OtherMotionArray SubArray, int arg0, int arg2)
        {
            var si = 0;
            var var2 = SomethingWithTileTypes_seg028_2941_E(SubArray, arg0, out int var1);

            if (var2!= -128)
            {
                if (SubArray.Unk4 + arg2 >= var2)
                {
                    if (SubArray.Unk4 - arg2 >= var2)
                    {
                        var index = getAt(SubArray.data, 2 + arg0 * 5, 8);
                        //8 << (UWMotionParamArray.TileAttributesArray[index] & 0x300) >> 8;
                        si = si | 8 << ((UWMotionParamArray.TileAttributesArray[index] & 0x300) >> 8);

                    }
                    else
                    {
                        si = si |0x800;
                    }
                }
                else
                {
                    si = si |0x100;
                }
            }
            else
            {
                si = si |0x200;
            }

            setAt(SubArray.data, 5 + arg0*5, 16, si);

            if (SubArray.Unk11 < var2)
            {
                SubArray.Unk11 = var2;
            }

            if (var1 < 0)
            {
                return 0;
            }
            else
            {
                if (var1 > 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }

        }

    }//end class
}//end namespace