using System;


namespace Underworld
{
    /// <summary>
    /// Range of Vision Variables
    /// </summary>
    public class VisionParams : Loader
    {
        static byte[] dseg_523 = new byte[] { 10, 0x00, 0x00, 0x00, 0x02, 0x00, 0x04, 0x00, 0x02, 0x00, 0x03, 0x00, 0xFF, 0xFF, 0x01, 0x00 };
        static byte[] dseg_527 = new byte[] { 0x2, 0x4 };
        static short[] dseg_52B = new short[] { 02, 00, 03, 00 };//, FF FF 01 00 00 04 00 07 00 01 00 01}
        static short[] dseg_52F = new short[] { -1, 1 };//i think the indexer for this can only be 0 or 1.
        static byte[] dseg_452 = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0x0, 0x0, 0x0, 0x0, 0x0 };

        static byte[] dseg_493 = new byte[] { 00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xB8, 0x98, 0xB0, 0x98, 0xB0, 0x98, 0xB0, 0xE4, 0xC4, 0xE4, 0xC4, 0xE0, 0xC4, 0xE4, 0xCD, 0xCD, 0xC5, 0xC9, 0xC5, 0xCD, 0xC5, 0xD6, 0xD2, 0x00, 0xD6, 0x00, 0xD6, 0x00, 0xD7, 0x00, 0xD3, 0x00, 0xD7, 0x00, 0xD7, 0xBC, 0x9C, 0xB4, 0x9C, 0xB4, 0x9C, 0xB4, 0xBD, 0x9D, 0xB5, 0x9D, 0xB5, 0x9D, 0xB5, 0xBE, 0x9E, 0xB6, 0x9E, 0xB6, 0x9E, 0xB6, 0xBF, 0x9F, 0xB7, 0x9F, 0xB7, 0x9F, 0xB7 };

        static byte[] dseg_432 = new byte[] { 01, 0x00, 0x40, 0x00, 0xFF, 0xFF, 0xC0, 0xFF, 0x01, 0x00, 0x40, 0x00, 0xFF, 0xFF, 0xC0, 0xFF, 0x01, 0x00, 0x40, 0x00, 0xFF, 0xFF, 0xC0, 0xFF };

        public static VisionParams[] visionparams = new VisionParams[0xF];
        byte[] _rawvisiondata; //= new byte[0xF * 0x11];
        static byte[] _defaultrawdata = new byte[0xF * 0x11];
        static short RelatedToFov_2C60 = 0;
        public static short LikelyDistanceToWallOrDarkness = -1;

        //array of data starting at dseg:2b5e
        public byte dseg_2B5E
        {
            get
            {
                return (byte)getAt8(_rawvisiondata, ptr + 0);
            }
            set
            {
                setAt8(_rawvisiondata, ptr + 0, value);
            }
        }
        public short FovYawX //= 0 //2B5F
        {
            get
            {
                return (short)getAt16(_rawvisiondata, ptr + 1);
            }
            set
            {
                setAt16(_rawvisiondata, ptr + 1, value);
            }
        }
        public short FovYawY// = 0; //2B61
        {
            get
            {
                return (short)getAt16(_rawvisiondata, ptr + 3);
            }
            set
            {
                setAt16(_rawvisiondata, ptr + 3, value);
            }
        }

        public short[] UNKARRAY_dseg_2B6B;

        public byte dseg_2B63//;
        {
            get
            {
                return (byte)getAt8(_rawvisiondata, ptr + 5);
            }
            set
            {
                setAt8(_rawvisiondata, ptr + 5, value);
            }
        }
        public byte CameraX_2b64//;//2B64
        {
            get
            {
                return (byte)getAt8(_rawvisiondata, ptr + 6);
            }
            set
            {
                setAt8(_rawvisiondata, ptr + 6, value);
            }
        }

        public short dseg_2B65
        {
            get
            {
                return (short)getAt16(_rawvisiondata, ptr + 7);
            }
            set
            {
                setAt16(_rawvisiondata, ptr + 7, value);
            }
        }
        public byte CameraY_2b66//;//2B66
        {
            get
            {
                return (byte)getAt8(_rawvisiondata, ptr + 8);
            }
            set
            {
                setAt8(_rawvisiondata, ptr + 8, value);
            }
        }

        public TileInfo playerTileCopy_2B67; //2B67

        public byte dseg_2B6B//This might actually be a ptr to data and not a value??
        {
            get
            {
                return (byte)getAt8(_rawvisiondata, ptr + 0XD);
            }
            set
            {
                setAt8(_rawvisiondata, ptr + 0xD, value);
            }
        }
        public byte dseg_2B6C
        {
            get
            {
                return (byte)getAt8(_rawvisiondata, ptr + 0XE);
            }
            set
            {
                setAt8(_rawvisiondata, ptr + 0xE, value);
            }
        }
        public short dseg_2B6D
        {
            get
            {
                return (short)getAt16(_rawvisiondata, ptr + 0XF);
            }
            set
            {
                setAt16(_rawvisiondata, ptr + 0xF, value);
            }
        }
        // public static short dseg_2B6F;
        // public static short FovYawXRIGHT = 0;//2b70        
        // public static short FovYawYRIGHT = 0;//2b72

        // public static short dseg_2B74;
        // public static byte CameraX_2B75;
        // public static byte dseg_2B76;
        // public static byte CameraY_2B77;

        // public static TileInfo playerTileCopy_2B78;
        // public static byte dseg_2B7C;

        int ptr;
        static VisionParams()
        {
            for (int i = 0; i <= visionparams.GetUpperBound(0); i++)
            {
                visionparams[i] = new VisionParams(index: i, rawdata: _defaultrawdata);
            }
        }

        VisionParams(int index, byte[] rawdata)
        {
            ptr = index * 0x11;
            _rawvisiondata = rawdata;
        }


        /// <summary>
        /// Port of a vanilla function which is used to calcuate what objects are inview. Implemented here to help support vanilla implementation of automapping
        /// </summary>
        public static void SetRangeOfVisionParams(short camerax, short cameray, short camerayaw)
        {
            var tileX = camerax >> 8; var tileY = cameray >> 8;
            if (!UWTileMap.ValidTile(tileX, tileY))
            {
                return;
            }
            var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            if (tile.tileType == 0)
            {
                RelatedToFov_2C60 = 0xF;
            }
            else
            {
                RelatedToFov_2C60 = 0;

                visionparams[0].dseg_2B5E = 0x81;
                visionparams[0].dseg_2B63 = 0;
                visionparams[0].dseg_2B65 = 0;

                visionparams[0].CameraX_2b64 = (byte)camerax;
                visionparams[0].CameraY_2b66 = (byte)cameray;

                visionparams[0].playerTileCopy_2B67 = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                //dseg_2B6B = reference to an array;
                visionparams[1].dseg_2B5E = 0;//.dseg_2B6F = 0;
                visionparams[1].dseg_2B63 = 0;//dseg_2B74 = 0;
                visionparams[1].dseg_2B65 = 0;  //dseg_2B76 = 0;


                visionparams[1].CameraX_2b64 = (byte)camerax; //CameraX_2B75 = (byte)camerax;
                visionparams[1].CameraY_2b66 = (byte)cameray;//CameraY_2B77 = (byte)cameray;
                visionparams[1].playerTileCopy_2B67 = UWTileMap.current_tilemap.Tiles[tileX, tileY];////playerTileCopy_2B78 = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                //dseg_2B7C =reference to an array that is set up in Seg032_6CF
                short x = 0; short y = 0;
                motion.SomethingProjectileHeading_seg021_22FD_EAE(heading: (ushort)(camerayaw + 0x2040), Result_arg2: ref x, Result_arg4: ref y);
                visionparams[1].FovYawX = x; visionparams[1].FovYawY = y;
                motion.SomethingProjectileHeading_seg021_22FD_EAE(heading: (ushort)(camerayaw - 0x2040), Result_arg2: ref x, Result_arg4: ref y);
                visionparams[0].FovYawX = x; visionparams[0].FovYawY = y;

                visionparams[1].FovYawX = (short)(visionparams[1].FovYawX >> 4);
                visionparams[1].FovYawY = (short)(visionparams[1].FovYawY >> 4);
                visionparams[0].FovYawX = (short)(visionparams[0].FovYawX >> 4);
                visionparams[0].FovYawY = (short)(visionparams[0].FovYawY >> 4);
            }
        }

        public static void LikelyGetViewDistance()
        {
            var di = 0;
            LikelyDistanceToWallOrDarkness = -1;
            var currentshade = shade.shadesdata[playerdat.lightlevel].ShadingArray_26EF;
            var var2currentshadePtr = 0;

        seg032_1175:
            LikelyDistanceToWallOrDarkness++;
            //var var4 = RelatedToFov_2C60; //var4 appears to be a pointer to this value
        seg032_119D:

            if ((RelatedToFov_2C60 & 0xF) == 0xF)
            {
                //seg032_11B0
                di = currentshade[66 + var2currentshadePtr];

                //loop
            seg032_11ED:
                if ((RelatedToFov_2C60 & 0xF) == 0xF)
                {
                seg032_120B:
                    if (currentshade[var2currentshadePtr] < di)
                    {
                        //seg032_11FD
                        currentshade[var2currentshadePtr] = 0;
                        var2currentshadePtr += 2;

                        //Loop back to 120B
                        goto seg032_120B;
                    }
                    else
                    {
                        //seg032_1210
                        if (RelatedToFov_2C60 == 0xF)
                        {
                            return;
                        }
                        else
                        {
                            goto seg032_1175;
                        }
                    }
                }
                else
                {
                seg032_11C8:
                    if (visionparams[RelatedToFov_2C60].dseg_2B6D > currentshade[var2currentshadePtr])
                    {
                        //seg32_11BA
                        currentshade[var2currentshadePtr] = 0;
                        var2currentshadePtr += 2;
                        goto seg032_11C8;
                    }
                    else
                    {
                        //seg32_11DE
                        seg32_1014(ref RelatedToFov_2C60, currentshade, var2currentshadePtr);
                        goto seg032_11ED;
                    }
                }
            }
            else
            {
                //seg032_1180
                //lookup a dseg and call 
                seg032_C9D(visionparams[0]);

                goto seg032_119D;
            }

        }

        static void seg032_C9D(VisionParams vision)
        {
            var di = 0;
            short var2;
            short var4 = 0;
            if (vision.FovYawX < 0)
            {
                di = 0;
            }
            else
            {
                di = 1;
            }
            if (di != 1)
            {
                var2 = vision.CameraX_2b64;
            }
            else
            {
                var2 = (short)(0x100 - vision.CameraX_2b64);
            }

            if ((di << 7) == vision.dseg_2B5E)
            {
                vision.dseg_2B6D = vision.dseg_2B6B;
            }

            if (vision.FovYawY != 0)
            {
                if (vision.FovYawX != 0)
                {
                    //Seg032_D08
                    goto seg032_D08;
                }
                else
                {
                    var4 = 0;
                    goto seg032_F9D;
                }
            }
            else
            {
                var4 = 1;
                goto seg032_F9D;
            }

        seg032_D08:
            if ((dseg_52F[di] * vision.FovYawX) * (0x100 - vision.CameraY_2b66) <= (vision.CameraY_2b66 * vision.FovYawY))
            {
                var4 = 0;
            }
            else
            {
                var4 = 1;
            }
            goto seg032_F9D;


        seg032_F9D:  //while var4
            if (var4 == 0)
            {
                //seg032_FA6
                vision.CameraX_2b64 = (byte)(vision.CameraX_2b64 + (((0xFF - vision.CameraY_2b66) * (vision.FovYawY * dseg_52F[di])) / vision.FovYawY) * dseg_52F[di]);
                vision.CameraY_2b66 = 0xFF;
                if ((vision.dseg_2B5E & 0x80) != (di << 7))
                {
                    //seg032_100A
                    vision.dseg_2B6D = vision.dseg_2B6B;
                }
                return;
            }
            else
            {
                //seg032_D5D
                var var3 = dseg_452[vision.playerTileCopy_2B67.tileType];
                //var al = Pathfind.tilewallflags[var3];
                if ((dseg_527[di] & Pathfind.tilewallflags[var3]) == 0)
                {
                    //seg032_D8F
                    var offset = vision.playerTileCopy_2B67.Ptr + motion.CameraYawHeadingRelated_2B52 * 6 * dseg_452[di];
                    var othertile = UWTileMap.GetTileByPTR((int)offset);
                    var al = dseg_452[othertile.tileType + dseg_52F[di]];//could this go out of bounds if other tile is solid and 52F[di] is -1?
                    var ax = Pathfind.tilewallflags[al];
                    if ((dseg_527[(di + 1) % 2] & Pathfind.tilewallflags[al]) == 0)
                    {
                        //seg032_DE9

                        vision.CameraY_2b66 += (byte)(var2 * (dseg_52F[di] * vision.FovYawX));

                        vision.CameraX_2b64 = (byte)(0xFF * ((di + 1) % 2));

                        var2 = 0x100;
                        if ((vision.dseg_2B5E & 0x80) != (di << 7))
                        {
                            //seg032_E50
                            MaybeTestVisibilityNextTile_seg032_6CF(vision: vision, arg2: 0, arg4: 0);
                        }
                        //seg032_E56
                        if (dseg_52F[di] != 1)
                        {
                            //seg032_E68
                            GetNextVisionTileNegative_seg032_6A9(vision);
                        }
                        else
                        {
                            //seg032_E62
                            GetNextVisionTilePositive_seg032_683(vision);
                        }
                        //seg032_E6D
                        if ((vision.dseg_2B6C != 0xF) || (Math.Abs(vision.dseg_2B63) > 0x10))
                        {
                            //seg032_E90
                            if (dseg_52F[(di + 1) % 2] != 1)
                            {
                                //seg032_EAB
                                GetNextVisionTileNegative_seg032_6A9(vision);
                            }
                            else
                            {
                                //seg032_EA6
                                GetNextVisionTilePositive_seg032_683(vision);
                            }
                            //seg032_EB0
                            vision.CameraX_2b64 = (byte)(0xFF * di);
                            vision.CameraY_2b66 = 0xFF;
                            if ((vision.dseg_2B5E * 0x80) == (di << 7))
                            {
                                return;
                            }
                            else
                            {
                                //seg032_Ed4->seg032_100A
                                vision.dseg_2B6D = vision.dseg_2B6B;
                                return;
                            }
                        }
                        else
                        {
                            //seg032_F3E
                            if (vision.FovYawY == 0)
                            {
                                var4 = 1;
                                goto seg032_F9D;
                            }
                            else
                            {
                                if ((0x100 - vision.CameraY_2b66) * (dseg_52F[di]) <= (var2 * vision.FovYawY))
                                {
                                    var4 = 0;
                                }
                                else
                                {
                                    var4 = 1;
                                }
                                goto seg032_F9D;
                            }
                            //loopback to Seg032_F9D
                        }
                    }
                    // else
                    // {
                    //goto ED7
                    // }
                }
                //Seg032_ED7
                vision.CameraY_2b66 = 0xFF;
                if ((Pathfind.tilewallflags[var3] & dseg_527[di]) != dseg_527[di])
                {
                    //Seg032_F1D
                    vision.CameraX_2b64 = (byte)(di * 0xFF);
                }
                else
                {
                    //seg032_EFB
                    if (dseg_52B[di] != var3)
                    {
                        vision.CameraX_2b64 = (byte)(di * 0xFF);
                    }
                    else
                    {
                        vision.CameraX_2b64 = (byte)(((di + 1) % 2) * 0xFF);
                    }
                }
                //seg032_F24
                if ((vision.dseg_2B5E & 0x80) != (di << 7))
                {
                    //Seg032_F3B -> seg032_100A
                    vision.dseg_2B6D = vision.dseg_2B6B;
                }
            }

            return;
        }

        static void seg32_1014(ref short ptrdseg_2c60, byte[] currentshade, int currentshade_ptr)
        {

        }

        static bool MaybeTestVisibilityNextTile_seg032_6CF(VisionParams vision, short arg2, short arg4)
        {
            var var4 = vision.dseg_2B6C;
            TileInfo AnotherTile;
            short PlayerFloorheightoffset;
            short OtherFloorheightoffset;
            short var6;
            short var8;

            var di = dseg_452[vision.playerTileCopy_2B67.tileType + (motion.CameraYawHeadingRelated_2B52 << 4)];
            if (vision.dseg_2B63 != 0)
            {
                //seg032_712
                if (vision.dseg_2B63 > 0)
                {
                    var6 = 2;
                }
                else
                {
                    var6 = 1;
                }
                //seg032_71F
                if (Math.Abs(vision.dseg_2B63) > Math.Abs(vision.dseg_2B65))
                {
                    var6 += 2;
                }
                if (Math.Abs(vision.dseg_2B63) == Math.Abs(vision.dseg_2B65))
                {
                    var6 += 4;
                }
            }
            else
            {
                var6 = 0;
            }
            //seg032_75B

            var var2 = dseg_493[var6 + di * 7];
            if (var2 == 0)
            {
                vision.dseg_2B6B = 0;
                return false;
            }
            else
            {
                //seg032_77D
                if ((var2 & 0x10) != 0)
                {
                    AnotherTile = UWTileMap.GetTileByPTR((int)(vision.playerTileCopy_2B67.Ptr + dseg_432[2 + motion.CameraYawHeadingRelated_2B52 * 6])); //array referenced is at dseg434
                    PlayerFloorheightoffset = vision.playerTileCopy_2B67.floorHeight;
                    var8 = dseg_452[(motion.CameraYawHeadingRelated_2B52 << 4) + AnotherTile.tileType];

                    if ((Pathfind.tilewallflags[var8] & 0x8) == 0)
                    {
                        //seg032_7CA
                        OtherFloorheightoffset = AnotherTile.floorHeight;
                        if ((Pathfind.tilewallflags[var8] & 0x20) == 0x20)
                        {
                            OtherFloorheightoffset++;
                        }
                        //seg032_818
                        if (var8 == 6)
                        {
                            OtherFloorheightoffset--;
                        }
                        if (di == 6)
                        {
                            PlayerFloorheightoffset++;
                        }
                        //seg032_835
                        if ((di == var8) && (di != 1))
                        {
                            OtherFloorheightoffset++;
                        }
                        //seg032_84A
                        if (OtherFloorheightoffset > PlayerFloorheightoffset)
                        {
                            var4 += 0x20;
                        }
                        else
                        {
                            var2 -= 0x10;
                        }
                    }
                }

                //rejoin at seg032_85C

                if ((var2 & 0x20) != 0)
                {
                    //Seg032_8A9
                    AnotherTile = UWTileMap.GetTileByPTR((int)(vision.playerTileCopy_2B67.Ptr + dseg_432[motion.CameraYawHeadingRelated_2B52 * 6]));

                    var8 = dseg_452[AnotherTile.tileType + (motion.CameraYawHeadingRelated_2B52 << 4)];

                    if ((Pathfind.tilewallflags[var8] & 0x2) == 0)
                    {
                        //seg032_8A9
                        OtherFloorheightoffset = AnotherTile.floorHeight;
                        PlayerFloorheightoffset = vision.playerTileCopy_2B67.floorHeight;

                        if ((Pathfind.tilewallflags[var8] & 0x20) == 0x20)
                        {
                            OtherFloorheightoffset++;
                        }
                        //seg032_8E6
                        if (var8 == 8)
                        {
                            OtherFloorheightoffset--;
                        }
                        //seg032_8F7
                        if (di == 8)
                        {
                            PlayerFloorheightoffset++;
                        }
                        //seg32_918
                        if ((var8 == di) && (di != 1))
                        {
                            PlayerFloorheightoffset++;
                        }
                        if (OtherFloorheightoffset > PlayerFloorheightoffset)
                        {
                            var4 += 0x20;
                        }
                        else
                        {
                            var2 -= 0x10;
                        }
                    }
                }

                //rejoin at Seg032_93B
                if ((var2 & 0x8) != 0)
                {
                    //seg032_945
                    AnotherTile = UWTileMap.GetTileByPTR((int)(vision.playerTileCopy_2B67.Ptr + dseg_432[motion.CameraYawHeadingRelated_2B52 * 6]));
                    var8 = dseg_452[AnotherTile.tileType + (motion.CameraYawHeadingRelated_2B52 << 4)];
                    if ((Pathfind.tilewallflags[var8] & 4) == 0)
                    {
                        //seg032_988
                        OtherFloorheightoffset = AnotherTile.floorHeight;
                        PlayerFloorheightoffset = vision.playerTileCopy_2B67.floorHeight;

                        if ((Pathfind.tilewallflags[var8] & 0x20) == 0x20)
                        {
                            OtherFloorheightoffset++;
                        }
                        //seg032_9C5
                        if (var8 == 9)
                        {
                            OtherFloorheightoffset--;
                        }
                        //seg032_9D9
                        if (di == 9)
                        {
                            PlayerFloorheightoffset++;
                        }

                        //seg032_9F6
                        if ((var8 == di) && (di != 1))
                        {
                            PlayerFloorheightoffset++;
                        }

                        if (OtherFloorheightoffset > PlayerFloorheightoffset)
                        {
                            //seg032_A16
                            var4 += 0x10;
                        }
                        else
                        {
                            var2 -= 8;
                        }
                    }
                }

                //rejoin at seg032_A1A
                vision.dseg_2B6B = var2;
                vision.dseg_2B6C = var4;

                if (arg2 == 0)
                {
                    return false;
                }
                else
                {
                    //possibly looking behind player?
                    //seg032_A34
                    AnotherTile = UWTileMap.GetTileByPTR((int)(vision.playerTileCopy_2B67.Ptr + dseg_432[2 + motion.CameraYawHeadingRelated_2B52 * 6]));   //array was dseg_434
                    var tmp = dseg_452[AnotherTile.tileType + (motion.CameraYawHeadingRelated_2B52 << 4)];

                    if ((Pathfind.tilewallflags[tmp] & 0x8) == arg4)
                    {
                        short ax;
                        if (arg4 == 8)
                        {
                            ax = 1;
                        }
                        else
                        {
                            ax = 0;
                        }

                        if (dseg_523[ax << 1] != (Pathfind.tilewallflags[di] & 0x10))
                        {
                            goto seg032_ACD;
                        }
                    }
                    //seg032_A9E
                    if ((Pathfind.tilewallflags[di] & 1) == 1)
                    {
                        var ax = 0;
                        if (arg4 != 0)
                        {
                            ax = 0;
                        }
                        else
                        {
                            ax = 1;
                        }

                        if ( dseg_523[ax<<1]  != (Pathfind.tilewallflags[di] & 0x10))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }


                seg032_ACD:
                    if (arg2 != 1)
                    {
                        GetNextVisionTileNegative_seg032_6A9(vision);
                    }
                    else
                    {
                        GetNextVisionTilePositive_seg032_683(vision);
                    }
                    return true;
                }
            }

        }

        /// <summary>
        /// Updates some values on the array and gets a new tile PTR
        /// </summary>
        /// <param name="vision"></param>
        static void GetNextVisionTilePositive_seg032_683(VisionParams vision)
        {
            vision.dseg_2B63++;
            vision.playerTileCopy_2B67 = UWTileMap.GetTileByPTR((int)(vision.playerTileCopy_2B67.Ptr + (dseg_432[dseg_432[motion.CameraYawHeadingRelated_2B52 * 6]] << 2)));
            vision.dseg_2B6B += 2;
        }

        static void GetNextVisionTileNegative_seg032_6A9(VisionParams vision)
        {
            vision.dseg_2B63--;
            vision.playerTileCopy_2B67 = UWTileMap.GetTileByPTR((int)(vision.playerTileCopy_2B67.Ptr - (dseg_432[dseg_432[motion.CameraYawHeadingRelated_2B52 * 6]] << 2)));
            vision.dseg_2B6B -= 2;
        }

    }//end class
}