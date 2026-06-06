using System;

namespace Underworld
{
    /// <summary>
    /// Range of Vision Variables
    /// </summary>
    public class VisionParams : Loader
    {
        static byte[] dseg_527 = new byte[] { 0x2, 0x4 };
        static short[] dseg_52F = new short[] { -1, 1 };//i think the indexer for this can only be 0 or 1.
        static byte[] dseg_452 = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0x0, 0x0, 0x0, 0x0, 0x0 };

        public static VisionParams[] visionparams = new VisionParams[0xF];
        static byte[] _rawvisiondata = new byte[0xF * 0x11];
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

        public short dseg_2B6B
        {
            get
            {
                return (short)getAt16(_rawvisiondata, ptr + 0XD);
            }
            set
            {
                setAt16(_rawvisiondata, ptr + 0xD, value);
            }
        }
        public byte dseg_2B6C
        {
            get
            {
                return (byte)getAt8(_rawvisiondata, ptr + 0xE);
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
                visionparams[i] = new VisionParams(i);
            }
        }

        VisionParams(int index)
        {
            ptr = index * 0x11;
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
                        if ((vision.dseg_2B5E & 0x80) != (di<<7))
                        {
                            //seg032_E50
                            seg032_6CF(vision: vision, arg2: 0, arg4: 0);
                        }
                        //seg032_E56
                        if (dseg_52F[di] != 1)
                        {
                            //seg032_E68
                            seg032_6A9(vision);
                        }
                        else
                        {
                            //seg032_E62
                            seg032_683(vision);
                        }
                        //seg032_E6D
                        if ((vision.dseg_2B6C != 0xF) || (Math.Abs(vision.dseg_2B63)>0x10))
                        {
                            //seg032_E90
                        }
                        else
                        {
                            //seg032_F3E
                        }

                    }
                    // else
                    // {
                    //goto ED7
                    // }
                }
                //Seg032_ED7

            }

            return;
        }

        static void seg32_1014(ref short ptrdseg_2c60, byte[] currentshade, int currentshade_ptr)
        {

        }

        static void seg032_6CF(VisionParams vision, short arg2, short arg4)
        {
            
        }

        static void seg032_683(VisionParams vision)
        {
            
        }

        static void seg032_6A9(VisionParams vision)
        {
            
        }

    }//end class
}