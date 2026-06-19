using System;

namespace Underworld
{
    /// <summary>
    /// Range of Vision Variables
    /// </summary>
    public class VisionParams : Loader
    {
        public static short[] cardinallookup_44A = new short[]{0, 16384, - 32768, -16384 };
        public static short TilesDiscoveredForExpGain;
        static byte[] dseg_523 = new byte[] { 0x10, 0x00, 0x00, 0x00, 0x02, 0x00, 0x04, 0x00, 0x02, 0x00, 0x03, 0x00, 0xFF, 0xFF, 0x01, 0x00 };
        static byte[] dseg_527 = new byte[] { 0x2, 0x4 };
        static short[] dseg_52B = new short[] { 02, 00, 03, 00 };//, FF FF 01 00 00 04 00 07 00 01 00 01}
        static short[] dseg_52F = new short[] { -1, 1 };//i think the indexer for this can only be 0 or 1.
        static byte[] dseg_452 = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x04, 0x02, 0x05, 0x03, 0x09, 0x08, 0x06, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x05, 0x04, 0x03, 0x02, 0x07, 0x06, 0x09, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x03, 0x05, 0x02, 0x04, 0x08, 0x09, 0x07, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        static byte[] dseg_493 = new byte[] { 00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xB8, 0x98, 0xB0, 0x98, 0xB0, 0x98, 0xB0, 0xE4, 0xC4, 0xE4, 0xC4, 0xE0, 0xC4, 0xE4, 0xCD, 0xCD, 0xC5, 0xC9, 0xC5, 0xCD, 0xC5, 0xD6, 0xD2, 0x00, 0xD6, 0x00, 0xD6, 0x00, 0xD7, 0x00, 0xD3, 0x00, 0xD7, 0x00, 0xD7, 0xBC, 0x9C, 0xB4, 0x9C, 0xB4, 0x9C, 0xB4, 0xBD, 0x9D, 0xB5, 0x9D, 0xB5, 0x9D, 0xB5, 0xBE, 0x9E, 0xB6, 0x9E, 0xB6, 0x9E, 0xB6, 0xBF, 0x9F, 0xB7, 0x9F, 0xB7, 0x9F, 0xB7 };

        static sbyte[] dseg_432 = new sbyte[] { 0x1, 0x00, 0x40, 0x00, -1, -1, -64, -1, 0x01, 0x00, 0x40, 0x00, -1, -1, -64, -1, 0x01, 0x00, 0x40, 0x00, -1, -1, -64, -1 };//to confirm is the usage of negative values here correct?

        //static sbyte ArgumentForFunction_34_CB_dseg_30F4;

        // static byte[] dseg_30F6 = new byte[0x252];
        // static byte[] dseg_2F9C = new byte[0x12];
        // static byte[] dseg_2FAE = new byte[0x12];
        public short index;

        public static VisionParams[] visionparams = new VisionParams[0xF];
        byte[] _rawvisiondata; //= new byte[0xF * 0x11];
        static byte[] _defaultrawdata = new byte[0xF * 0x11];
        static short RelatedToVisionIndex_2C60 = 0;
        public static short LikelyDistanceToWallOrDarkness = -1;
        public static short LikelyDist_2C6C;

        static short RenderingCounter_2C6A = 0;
        static short ShadeRef_2C74;
        //static TileInfo RenderingTile_2F7C;

        static short MaybeTileShadeLevel_2F7A;

        //array of data starting at dseg:2b5e
        public byte dseg_2B5E_0
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
        public short FovYawX_1 //= 0 //2B5F
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
        public short FovYawY_3// = 0; //2B61
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

        public sbyte dseg_2B63_5//;
        {
            get
            {
                return (sbyte)getAt8(_rawvisiondata, ptr + 5);
            }
            set
            {
                setAt8(_rawvisiondata, ptr + 5, value);
            }
        }
        public byte CameraX_2b64_6//;//2B64
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

        public byte dseg_2B65_7
        {
            get
            {
                return (byte)getAt16(_rawvisiondata, ptr + 7);
            }
            set
            {
                setAt8(_rawvisiondata, ptr + 7, value);
            }
        }
        public byte CameraY_2b66_8//;//2B66
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

        public TileInfo currentTile_2B67_9 //2B67
        {
            get
            {
                return _currenttile;
            }
            set
            {
                _currenttile = value;
                setAt16(_rawvisiondata, ptr + 9, (int)value.dosptr); //temp just to make debugging easier
                setAt16(_rawvisiondata, ptr + 0xB, 0x7CFD);
            }
        }


        private TileInfo _currenttile;

        /// <summary>
        /// //This is a pointer to the shading data.
        /// </summary>
        public short dseg_2B6B_d
        {
            get
            {
                return (short)(getAt16(_rawvisiondata, ptr + 0XD) - 0x26ee);
            }
            set
            {
                setAt16(_rawvisiondata, ptr + 0xD, value + 0x26EE); //addition is a temp to allow for easier debugging.
            }
        }
        // public byte dseg_2B6C_E
        // {
        //     get
        //     {
        //         return (byte)getAt8(_rawvisiondata, ptr + 0XE);
        //     }
        //     set
        //     {
        //         setAt8(_rawvisiondata, ptr + 0xE, value);
        //     }
        // }
        public short dseg_2B6D_F
        {
            get
            {
                return (short)(getAt16(_rawvisiondata, ptr + 0XF) - 0x26ee);
            }
            set
            {
                setAt16(_rawvisiondata, ptr + 0xF, value + 0x26EE); //addition is a temp to allow for easier debugging.
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
            for (short i = 0; i <= visionparams.GetUpperBound(0); i++)
            {
                visionparams[i] = new VisionParams(_index: i, rawdata: _defaultrawdata);
            }
        }

        VisionParams(short _index, byte[] rawdata)
        {
            index = _index;
            ptr = _index * 0x11;
            _rawvisiondata = rawdata;
        }


        /// <summary>
        /// Port of a vanilla function which is used to calcuate what objects are inview. Implemented here to help support vanilla implementation of automapping
        /// </summary>
        public static void SetRangeOfVisionParams(short camerax, short cameray, short camerayaw)
        {
            var tileX = playerdat.playerObject.tileX;//camerax >> 8; var tileY = cameray >> 8;
            var tileY = playerdat.playerObject.tileY;
            if (!UWTileMap.ValidTile(tileX, tileY))
            {
                return;
            }
            var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            if (tile.tileType == 0)
            {
                RelatedToVisionIndex_2C60 = 0xF;
            }
            else
            {
                var4pointsto2C60 = true;
                RelatedToVisionIndex_2C60 = 0;

                visionparams[0].dseg_2B5E_0 = 0x81;
                visionparams[0].dseg_2B63_5 = 0;
                visionparams[0].dseg_2B65_7 = 0;

                visionparams[0].CameraX_2b64_6 = (byte)camerax;
                visionparams[0].CameraY_2b66_8 = (byte)cameray;

                visionparams[0].currentTile_2B67_9 = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                //dseg_2B6B = reference to an array;
                visionparams[0].dseg_2B6B_d = 0x20; // this is an offset into shading data. In disassembly this value is set to 0x270E, the shading data is stored at 0x26EE
                visionparams[1].dseg_2B5E_0 = 0xF;//.dseg_2B6F = 0;
                visionparams[1].dseg_2B63_5 = 0;//dseg_2B74 = 0;
                visionparams[1].dseg_2B65_7 = 0;  //dseg_2B76 = 0;


                visionparams[1].CameraX_2b64_6 = (byte)camerax; //CameraX_2B75 = (byte)camerax;
                visionparams[1].CameraY_2b66_8 = (byte)cameray;//CameraY_2B77 = (byte)cameray;
                visionparams[1].currentTile_2B67_9 = UWTileMap.current_tilemap.Tiles[tileX, tileY];////playerTileCopy_2B78 = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                visionparams[1].dseg_2B6B_d = 0x20; //same ptr as above.
                //dseg_2B7C =reference to an array that is set up in Seg032_6CF
                short x = 0; short y = 0;
                motion.SomethingProjectileHeading_seg021_22FD_EAE(heading: (ushort)(camerayaw + 0x2040), Result_arg2: ref x, Result_arg4: ref y);
                visionparams[1].FovYawX_1 = x; visionparams[1].FovYawY_3 = y;
                motion.SomethingProjectileHeading_seg021_22FD_EAE(heading: (ushort)(camerayaw - 0x2040), Result_arg2: ref x, Result_arg4: ref y);
                visionparams[0].FovYawX_1 = x; visionparams[0].FovYawY_3 = y;

                visionparams[1].FovYawX_1 = (short)(visionparams[1].FovYawX_1 >> 4);
                visionparams[1].FovYawY_3 = (short)(visionparams[1].FovYawY_3 >> 4);
                visionparams[0].FovYawX_1 = (short)(visionparams[0].FovYawX_1 >> 4);
                visionparams[0].FovYawY_3 = (short)(visionparams[0].FovYawY_3 >> 4);
            }
        }

        /// <summary>
        /// Within the rendering functions the final steps of getting LOS on tiles is done. 
        /// This function only implements the elements relating to automap updates and experience gain on undiscovered tiles
        /// </summary>
        public static void FakeRender()
        {
            TilesDiscoveredForExpGain = 0;
            GetTilesToRender();
            if ((playerdat.play_level >= 0) && (playerdat.play_level < 16))
            {
                int finalgain = 0;
                if (_RES == GAME_UW2)
                {
                    finalgain = ((1 + (playerdat.dungeon_level / 8)) * TilesDiscoveredForExpGain) / 0xA;
                }
                else
                {
                    finalgain = (playerdat.dungeon_level * TilesDiscoveredForExpGain) / 0xA;
                }
                //var finalgain = (1 + playerdat.CurrentWorld) * TilesDiscoveredForExpGain;
                if (finalgain > 0)
                {
                    playerdat.ChangeExperience(finalgain);
                }
            }
        }

        /// <summary>
        /// Again only automap and exp related elements are currently implemented.
        /// </summary>
        static void GetTilesToRender()
        {
            int RenderingTile_2F7C_ptr = 0;
            var automapdata = automap.automaps[playerdat.dungeon_level - 1].buffer;
            short si_offsettilemap;
            var playerTile = UWTileMap.current_tilemap.Tiles[playerdat.playerObject.tileX, playerdat.playerObject.tileY];
            var shading = shade.shadesdata[playerdat.lightlevel].ShadingArray_26EE;//System.IO.File.WriteAllBytes("c:\\temp\\shading.dat", shading);
            var di_dseg432 = dseg_432[motion.CameraYawHeadingRelated_2B52 * 6];
            var varC_tileoffset = dseg_432[2 + (motion.CameraYawHeadingRelated_2B52 * 6)];
            var var2_offsetshadeatdistance = LikelyDistanceToWallOrDarkness * 0x42;


            var var4_tile_ptr = (int)(playerTile.Ptr + ((varC_tileoffset * LikelyDistanceToWallOrDarkness) << 2));
            var4_tile_ptr = var4_tile_ptr - (di_dseg432 << 6);


            var tile00 = UWTileMap.current_tilemap.Tiles[0, 0];
            var tile63 = UWTileMap.current_tilemap.Tiles[63, 63];
            short var8_tileindex = (short)((var4_tile_ptr - tile00.Ptr) / 4);
            // if (var8_tileindex > 0x2000)
            // {//not sure if this is needed.
            //     //Seg019_62E
            //     var8_tileindex += 0xC000;
            // }
            //Seg019_634
            //InitSomeArrays(-10);
            LikelyDist_2C6C = LikelyDistanceToWallOrDarkness;

            //seg0139_771
            while (LikelyDist_2C6C >= 0)
            {
                //seg019_646
                //InitSomeArrays(2);
                RenderingCounter_2C6A = 0;
                ShadeRef_2C74 = (short)(var2_offsetshadeatdistance + (RenderingCounter_2C6A << 1));
                RenderingTile_2F7C_ptr = (int)(var4_tile_ptr + ((RenderingCounter_2C6A * di_dseg432) << 2));

                si_offsettilemap = (short)(var8_tileindex + (RenderingCounter_2C6A * di_dseg432));

                //seg019_6B4
                while (RenderingCounter_2C6A < 0x10)
                {
                    //seg19_68D
                    if ((si_offsettilemap & 0xF000) == 0)
                    {
                        StartUpdatingAutomapTile(automapdata, si_offsettilemap, UWTileMap.GetTileByPTR(RenderingTile_2F7C_ptr));
                    }
                    //Seg019_6A0
                    RenderingCounter_2C6A++;
                    ShadeRef_2C74 += 2;
                    RenderingTile_2F7C_ptr = RenderingTile_2F7C_ptr + (di_dseg432 << 2);
                    si_offsettilemap += di_dseg432;
                }
                //seg019:6BB
                //InitSomeArrays(1);
                RenderingCounter_2C6A = 0x20;
                ShadeRef_2C74 = (short)(var2_offsetshadeatdistance + (RenderingCounter_2C6A << 1));
                RenderingTile_2F7C_ptr = var4_tile_ptr + ((RenderingCounter_2C6A * di_dseg432) << 2);
                si_offsettilemap = (short)(var8_tileindex + (RenderingCounter_2C6A * di_dseg432));

                //seg019_729
                while (RenderingCounter_2C6A > 0x10)
                {
                    //seg019_702
                    if ((si_offsettilemap & 0xF000) == 0)
                    {
                        StartUpdatingAutomapTile(automapdata, si_offsettilemap, UWTileMap.GetTileByPTR(RenderingTile_2F7C_ptr));
                    }
                    //seg019_715
                    RenderingCounter_2C6A--;
                    ShadeRef_2C74 -= 2;
                    RenderingTile_2F7C_ptr = RenderingTile_2F7C_ptr - (di_dseg432 << 2);
                    si_offsettilemap -= di_dseg432;
                }
                //seg019_732
                //InitSomeArrays(0);
                if ((si_offsettilemap & 0xF000) == 0)
                {
                    StartUpdatingAutomapTile(automapdata, si_offsettilemap, UWTileMap.GetTileByPTR(RenderingTile_2F7C_ptr));
                }
                //seg019_84C
                //init some object rendering data
                var2_offsetshadeatdistance -= 0x42;
                var4_tile_ptr = (int)(var4_tile_ptr - (varC_tileoffset << 2));
                var8_tileindex -= (short)varC_tileoffset;
                LikelyDist_2C6C--;
            }

            //Seg019_77B
            RenderingTile_2F7C_ptr = var4_tile_ptr;
            RenderingCounter_2C6A = 0;
            si_offsettilemap = (short)var8_tileindex;
            var varA_AutoMapPtr = var8_tileindex;

            while (RenderingCounter_2C6A < 0x21)
            {
                if ((si_offsettilemap & 0xF000) == 0)
                {
                    //Seg019_7A2
                    if (automapdata[varA_AutoMapPtr] == 0)
                    {
                        //automap tile has been unvisited and is an undiscovered tile
                        automapdata[varA_AutoMapPtr] = automaptileinfo.UndiscoveredTiles[UWTileMap.GetTileByPTR(RenderingTile_2F7C_ptr).tileType];
                    }
                }
                //seg019_7BE
                RenderingCounter_2C6A++;
                RenderingTile_2F7C_ptr = RenderingTile_2F7C_ptr + (di_dseg432 << 2);
                si_offsettilemap += di_dseg432;
                varA_AutoMapPtr += di_dseg432;
            }
        }

        static void StartUpdatingAutomapTile(byte[] automapbuffer, short tileindex, TileInfo RenderingTile_2F7C)
        {
            var var8 = shade.shadesdata[playerdat.lightlevel].ShadingArray_26EE[ShadeRef_2C74];
            if ((var8 & 0x80) != 0)
            {
                //seg019_2F7C
                byte var19AutoMapValueToSet = 0;
                MaybeTileShadeLevel_2F7A = (short)(shade.shadesdata[playerdat.lightlevel].ShadingArray_26EE[ShadeRef_2C74 + 1] & 0xF);
                if (MaybeTileShadeLevel_2F7A >= 8)
                {
                    var19AutoMapValueToSet = automapbuffer[tileindex];
                    //seg019_DEF
                    if (automapbuffer[tileindex] == 0)
                    {
                        //seg019_DDD
                        var19AutoMapValueToSet = automaptileinfo.UndiscoveredTiles[RenderingTile_2F7C.tileType];
                    }
                }
                else
                {
                    //Seg019_DA7
                    var19AutoMapValueToSet = (byte)(RenderingTile_2F7C.tileType | TerrainDatLoader.GetTerrainDataBit345(RenderingTile_2F7C));
                }

                //from here on there is a lot of code relating to rendering. 
                // skipping ahead to Seg019_1383 where things like bridge and stairs down tmaps are set
                if (playerdat.AutomapEnabled)
                {
                    if ((RenderingTile_2F7C.indexObjectList != 0) && (MaybeTileShadeLevel_2F7A < 8))
                    {
                        var bridge = objectsearch.FindMatchInTile(tileX: RenderingTile_2F7C.tileX, tileY: RenderingTile_2F7C.tileY, majorclass: 5, minorclass: 2, classindex: 4);
                        if (bridge != null)
                        {
                            if (bridge.invis == 0)
                            {
                                var19AutoMapValueToSet |= (byte)(automaptileinfo.bridgedisplaytype << 4);
                            }
                        }
                        else
                        {
                            //check for tmap
                            var tmap = objectsearch.FindMatchInTile(tileX: RenderingTile_2F7C.tileX, tileY: RenderingTile_2F7C.tileY, majorclass: 5, minorclass: 2, classindex: 0xE);
                            if (tmap == null)
                            {
                                tmap = objectsearch.FindMatchInTile(tileX: RenderingTile_2F7C.tileX, tileY: RenderingTile_2F7C.tileY, majorclass: 5, minorclass: 2, classindex: 0xF);
                            }
                            if (tmap != null)
                            {
                                var terrain = TerrainDatLoader.GetTerrainDataBit012(tmap.owner);
                                if ((terrain == automaptileinfo.stairdisplaytype))
                                {
                                    //placeholder I need to match the texture flag value with something that tells me it is a stairs down
                                    var19AutoMapValueToSet |= (byte)(automaptileinfo.stairdisplaytype << 4);
                                }
                            }
                            else
                            {
                                //check for door
                                var door = objectsearch.FindMatchInTile(tileX: RenderingTile_2F7C.tileX, tileY: RenderingTile_2F7C.tileY, majorclass: 5, minorclass: 0, classindex: -1);
                                if (door != null)
                                {
                                    if (door.classindex != 7) //don't show secret door
                                    {
                                        var19AutoMapValueToSet |= (byte)(automaptileinfo.doordisplaytype << 4);
                                    }
                                }
                                else
                                {
                                    //check for moving door
                                    door = objectsearch.FindMatchInTile(tileX: RenderingTile_2F7C.tileX, tileY: RenderingTile_2F7C.tileY, majorclass: 7, minorclass: 0, classindex: 0xF);
                                    if (door != null)
                                    {
                                        var19AutoMapValueToSet |= (byte)(automaptileinfo.doordisplaytype << 4);
                                    }
                                }
                            }
                        }
                    }
                    //Debug.Print($"Marking tile {RenderingTile_2F7C.tileX} {RenderingTile_2F7C.tileY} as {var19AutoMapValueToSet}");
                    automapbuffer[tileindex] = var19AutoMapValueToSet;
                }
            }
            else
            {
                if (automapbuffer[tileindex] == 0)
                {
                    //Debug.Print($"Marking tile {RenderingTile_2F7C.tileX} {RenderingTile_2F7C.tileY} as {automaptileinfo.UndiscoveredTiles[RenderingTile_2F7C.tileType]}");
                    automapbuffer[tileindex] = automaptileinfo.UndiscoveredTiles[RenderingTile_2F7C.tileType];
                    TilesDiscoveredForExpGain++;
                }
            }
        }
        static bool var4pointsto2C60 = true;

        static short var4_visionindex
        {
            get
            {
                if (var4pointsto2C60)
                {
                    return RelatedToVisionIndex_2C60;
                }
                else
                {
                    return _var4;
                }
            }
            set
            {
                if (var4pointsto2C60)
                {
                    RelatedToVisionIndex_2C60 = value;
                }
                else
                {
                    _var4 = value;
                }
            }
        }
        private static short _var4 = 0;
        public static void GetViewDistance()
        {
            var di = 0;
            LikelyDistanceToWallOrDarkness = -1;
            var currentshade = shade.shadesdata[playerdat.lightlevel].ShadingArray_26EE;
            var var2currentshadePtr = 0;

        seg032_1175:
            LikelyDistanceToWallOrDarkness++;
            //var var4_visionindex = RelatedToVisionIndex_2C60; 
            var4pointsto2C60 = true;

        seg032_119D:

            if ((var4_visionindex & 0xF) == 0xF)
            {
                //seg032_11AB
                var4pointsto2C60 = true;
                //var4_visionindex = RelatedToVisionIndex_2C60;
                di = 0x42 + var2currentshadePtr;

                //loop
            seg032_11ED:
                if ((var4_visionindex & 0xF) == 0xF)
                {
                seg032_120B:
                    if (var2currentshadePtr < di)
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
                        if (RelatedToVisionIndex_2C60 == 0xF)
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
                    if (visionparams[var4_visionindex].dseg_2B6D_F > var2currentshadePtr)
                    {
                        //seg32_11BA
                        currentshade[var2currentshadePtr] = 0; //reset the data.
                        var2currentshadePtr += 2;
                        goto seg032_11C8;
                    }
                    else
                    {
                        //seg32_11DE                        
                        TestVisionAndShade_seg32_1014(currentshade, ref var2currentshadePtr);
                        goto seg032_11ED;
                    }
                }
            }
            else
            {
                //seg032_1180
                //lookup a dseg and call 
                var toProcess = visionparams[var4_visionindex & 0xF];
                MaybeProcessLOSArc_seg032_C9D(toProcess);
                var4pointsto2C60 = false;
                var4_visionindex = toProcess.dseg_2B5E_0;
                goto seg032_119D;
            }

        }

        static void MaybeProcessLOSArc_seg032_C9D(VisionParams vision)
        {
            var shading = shade.shadesdata[playerdat.lightlevel].ShadingArray_26EE;
            var di = 0;
            short var2;
            short var4 = 0;
            if (vision.FovYawX_1 < 0)
            {
                di = 0;
            }
            else
            {
                di = 1;
            }
            if (di != 1)
            {
                var2 = vision.CameraX_2b64_6;
            }
            else
            {
                var2 = (short)(0x100 - vision.CameraX_2b64_6);
            }
            //seg032_CD7
            if ((di << 7) == (vision.dseg_2B5E_0 & 0x80))
            {
                vision.dseg_2B6D_F = vision.dseg_2B6B_d;
            }

            if (vision.FovYawY_3 != 0)
            {
                if (vision.FovYawX_1 != 0)
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
            if ((dseg_52F[di] * vision.FovYawX_1) * (0x100 - vision.CameraY_2b66_8) <= (var2 * vision.FovYawY_3))
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
                vision.CameraX_2b64_6 = (byte)(vision.CameraX_2b64_6 + (((0xFF - vision.CameraY_2b66_8) * (vision.FovYawX_1 * dseg_52F[di])) / vision.FovYawY_3) * dseg_52F[di]);
                vision.CameraY_2b66_8 = 0xFF;
                if ((vision.dseg_2B5E_0 & 0x80) != (di << 7))
                {
                    //seg032_100A
                    vision.dseg_2B6D_F = vision.dseg_2B6B_d;
                }
                return;
            }
            else
            {
                //seg032_D5D
                var var3 = dseg_452[vision.currentTile_2B67_9.tileType + (motion.CameraYawHeadingRelated_2B52 << 4)];
                //var al = Pathfind.tilewallflags[var3];
                if ((dseg_527[di] & Pathfind.tilewallflags[var3]) == 0)
                {
                    //seg032_D8F
                    var offset = vision.currentTile_2B67_9.Ptr + ((dseg_52F[di] * dseg_432[motion.CameraYawHeadingRelated_2B52 * 6]) << 2);
                    var othertile = UWTileMap.GetTileByPTR((int)offset);
                    var al = dseg_452[othertile.tileType + (motion.CameraYawHeadingRelated_2B52 << 4)];//could this go out of bounds if other tile is solid and 52F[di] is -1?
                    //var ax = Pathfind.tilewallflags[al];
                    if ((dseg_527[(di + 1) % 2] & Pathfind.tilewallflags[al]) == 0)
                    {
                        //seg032_DE9

                        vision.CameraY_2b66_8 += (byte)((var2 * vision.FovYawY_3) / (dseg_52F[di] * vision.FovYawX_1));

                        vision.CameraX_2b64_6 = (byte)(0xFF * ((di + 1) % 2));

                        var2 = 0x100;
                        if ((vision.dseg_2B5E_0 & 0x80) == (di << 7))
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
                        if (((shading[vision.dseg_2B6B_d + 1] & 0xF) != 0xF) && (Math.Abs(vision.dseg_2B63_5) <= 0x10))
                        {
                            //seg032_F3E
                            if (vision.FovYawY_3 == 0)
                            {
                                var4 = 1;
                                goto seg032_F9D;
                            }
                            else
                            {
                                if ((dseg_52F[di] * vision.FovYawX_1) * (0x100 - vision.CameraY_2b66_8) <= (var2 * vision.FovYawY_3))
                                //if ((0x100 - vision.CameraY_2b66_8) * (dseg_52F[di]) <= (var2 * vision.FovYawY_3))
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
                        else
                        {
                            //////////////////////
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
                            vision.CameraX_2b64_6 = (byte)(0xFF * di);
                            vision.CameraY_2b66_8 = 0xFF;
                            if ((vision.dseg_2B5E_0 * 0x80) == (di << 7))
                            {
                                return;
                            }
                            else
                            {
                                //seg032_Ed4->seg032_100A
                                vision.dseg_2B6D_F = vision.dseg_2B6B_d;
                                return;
                            }
                        }
                    }
                    // else
                    // {
                    //goto ED7
                    // }
                }
                //Seg032_ED7
                vision.CameraY_2b66_8 = 0xFF;
                if ((Pathfind.tilewallflags[var3] & dseg_527[di]) != dseg_527[di])
                {
                    //Seg032_F1D
                    vision.CameraX_2b64_6 = (byte)(di * 0xFF);
                }
                else
                {
                    //seg032_EFB
                    if (dseg_52B[di] != var3)
                    {
                        vision.CameraX_2b64_6 = (byte)(di * 0xFF);
                    }
                    else
                    {
                        vision.CameraX_2b64_6 = (byte)(((di + 1) % 2) * 0xFF);
                    }
                }
                //seg032_F24
                if ((vision.dseg_2B5E_0 & 0x80) != (di << 7))
                {
                    //Seg032_F3B -> seg032_100A
                    vision.dseg_2B6D_F = vision.dseg_2B6B_d;
                }
            }

            return;
        }

        static void TestVisionAndShade_seg32_1014(byte[] currentshade, ref int currentshade_ptr)
        {
            var di_vision = VisionParams.visionparams[var4_visionindex & 0xF];
            var si_vision = VisionParams.visionparams[di_vision.dseg_2B5E_0 & 0xF];
            currentshade_ptr = si_vision.dseg_2B6D_F + 2;


            //Loop 
        Seg032_1090:
            if (MaybeTestVisibilityNextTile_seg032_6CF(di_vision, 1, 8))
            {
                //Seg032_104E               
                if ((si_vision.dseg_2B63_5 << 8) + si_vision.CameraX_2b64_6 <= (di_vision.dseg_2B63_5 << 8) + di_vision.CameraX_2b64_6)
                {
                    goto Seg032_1070;
                }
                else
                {
                    goto Seg032_1090;
                }
            }
            else
            {//seg032_10A0
                if (di_vision.dseg_2B63_5 < si_vision.dseg_2B63_5)
                {
                    goto Seg032_10A8;
                }
                else
                {
                    goto Seg032_10B8;
                }
            }

        Seg032_1070:
            var4_visionindex = (short)((var4_visionindex & 0xF0) + (si_vision.dseg_2B5E_0 & 0xF));//this may be an issue 2C60 may be incorrectly set up
            di_vision.dseg_2B5E_0 = 0;
            si_vision.dseg_2B5E_0 = 0;
            return;

        Seg032_10A8:
            //seg
            if (MaybeTestVisibilityNextTile_seg032_6CF(si_vision, -1, 8))
            {
                goto Seg032_10A8;
            }
            else
            {
                goto Seg032_10B8;
            }

        Seg032_10B8:
            var tmpVisionArray_var12 = new byte[0x11];
            for (int z = 0; z <= tmpVisionArray_var12.GetUpperBound(0); z++)
            {
                tmpVisionArray_var12[z] = di_vision._rawvisiondata[di_vision.ptr + z];
            }
            var tmpVision = new VisionParams(_index: 0, rawdata: tmpVisionArray_var12);
            tmpVision.currentTile_2B67_9 = di_vision.currentTile_2B67_9;
            if (MaybeTestVisionIntoDarkness_seg032_AF5(di_vision, si_vision))
            {
                //seg032_10F3
                var4pointsto2C60 = false;
                var4_visionindex = si_vision.dseg_2B5E_0;
                if (di_vision.dseg_2B63_5 == si_vision.dseg_2B63_5)
                {
                    return;
                }
                else
                {
                    //seg032_1100
                Seg032_114C:
                    GetNextVisionTilePositive_seg032_683(tmpVision);

                Seg032_1156:
                    if (si_vision.dseg_2B63_5 > tmpVision.dseg_2B63_5)
                    {
                    Seg032_110A:
                        if (MaybeTestVisibilityNextTile_seg032_6CF(tmpVision, 1, 0))
                        {
                            if (si_vision.dseg_2B63_5 > tmpVision.dseg_2B63_5)
                            {
                                goto Seg032_110A;
                            }
                        }
                        //seg032_111D
                        if (si_vision.dseg_2B63_5 <= tmpVision.dseg_2B63_5)
                        {
                            goto Seg032_1156;
                        }
                        //seg032_1125
                        GetNextVisionTilePositive_seg032_683(tmpVision);
                    Seg032_1139:
                        if (MaybeTestVisibilityNextTile_seg032_6CF(tmpVision, 1, 8))
                        {
                            //seg032_1131
                            if (si_vision.dseg_2B63_5 > tmpVision.dseg_2B63_5)
                            {
                                goto Seg032_1139;
                            }
                            else
                            {
                                goto Seg032_114C;
                            }
                        }
                        else
                        {
                            goto Seg032_114C;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                var4_visionindex = (short)((var4_visionindex & 0xF0) + (si_vision.dseg_2B5E_0 & 0xF));//this may be an issue 2C60 may be incorrectly set up
                di_vision.dseg_2B5E_0 = 0;
                si_vision.dseg_2B5E_0 = 0;
                return;
            }
        }

        static bool MaybeTestVisibilityNextTile_seg032_6CF(VisionParams vision, short arg2, short arg4)
        {
            var shading = shade.shadesdata[playerdat.lightlevel].ShadingArray_26EE;
            var var4 = (byte)(shading[vision.dseg_2B6B_d + 1] & 0xF); //(byte)(vision.dseg_2B6C_E & 0xF);
            TileInfo AnotherTile;
            short PlayerFloorheightoffset;
            short OtherFloorheightoffset;
            short var6;
            short var8;

            var di = dseg_452[vision.currentTile_2B67_9.tileType + ((motion.CameraYawHeadingRelated_2B52 & 0xFF) << 4)];
            if (vision.dseg_2B63_5 != 0)
            {
                //seg032_712
                if (vision.dseg_2B63_5 > 0)
                {
                    var6 = 2;
                }
                else
                {
                    var6 = 1;
                }
                //seg032_71F
                if (Math.Abs(vision.dseg_2B63_5) > Math.Abs(vision.dseg_2B65_7))
                {
                    var6 += 2;
                }
                if (Math.Abs(vision.dseg_2B63_5) == Math.Abs(vision.dseg_2B65_7))                //Seg032_73F
                {
                    var6 += 4;
                }
            }
            else
            {
                var6 = 0;
            }
            //seg032_75B

            var var2 = dseg_493[var6 + (di * 7)];
            if (var2 == 0)
            {
                shading[vision.dseg_2B6B_d] = 0;
                return false;
            }
            else
            {
                //seg032_77D
                if ((var2 & 0x10) != 0)
                {//seg032_787
                    AnotherTile = UWTileMap.GetTileByPTR((int)(vision.currentTile_2B67_9.Ptr + (dseg_432[2 + motion.CameraYawHeadingRelated_2B52 * 6] << 2))); //array referenced is at dseg434
                    PlayerFloorheightoffset = vision.currentTile_2B67_9.floorHeight;
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
                    //Seg032_866
                    AnotherTile = UWTileMap.GetTileByPTR((int)(vision.currentTile_2B67_9.Ptr + (dseg_432[motion.CameraYawHeadingRelated_2B52 * 6] << 2)));

                    var8 = dseg_452[AnotherTile.tileType + (motion.CameraYawHeadingRelated_2B52 << 4)];

                    if ((Pathfind.tilewallflags[var8] & 0x2) == 0)
                    {
                        //seg032_8A9
                        OtherFloorheightoffset = AnotherTile.floorHeight;
                        PlayerFloorheightoffset = vision.currentTile_2B67_9.floorHeight;

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
                            var4 += 0x40;
                        }
                        else
                        {
                            var2 -= 0x20;
                        }
                    }
                }

                //rejoin at Seg032_93B
                if ((var2 & 0x8) != 0)
                {
                    //seg032_945
                    AnotherTile = UWTileMap.GetTileByPTR((int)(vision.currentTile_2B67_9.Ptr - (dseg_432[motion.CameraYawHeadingRelated_2B52 * 6] << 2)));
                    var8 = dseg_452[AnotherTile.tileType + (motion.CameraYawHeadingRelated_2B52 << 4)];
                    if ((Pathfind.tilewallflags[var8] & 4) == 0)
                    {
                        //seg032_988
                        OtherFloorheightoffset = AnotherTile.floorHeight;
                        PlayerFloorheightoffset = vision.currentTile_2B67_9.floorHeight;

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
                shading[vision.dseg_2B6B_d] = var2;
                shading[vision.dseg_2B6B_d + 1] = var4;
                //vision.dseg_2B6C_E = var4;

                if (arg2 == 0)
                {
                    return false;
                }
                else
                {
                    //possibly looking behind player?
                    //seg032_A34
                    AnotherTile = UWTileMap.GetTileByPTR((int)(vision.currentTile_2B67_9.Ptr + (dseg_432[2 + motion.CameraYawHeadingRelated_2B52 * 6] << 2)));   //array was dseg_434
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

                        if (dseg_523[ax << 1] == (Pathfind.tilewallflags[di] & 0x10))
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

                        if (dseg_523[ax << 1] != (Pathfind.tilewallflags[di] & 0x10))
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
                        vision.CameraX_2b64_6 = 0xFF;
                    }
                    else
                    {
                        GetNextVisionTilePositive_seg032_683(vision);
                        vision.CameraX_2b64_6 = 0;
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
            vision.dseg_2B63_5++;
            vision.currentTile_2B67_9 = UWTileMap.GetTileByPTR((int)(vision.currentTile_2B67_9.Ptr + (dseg_432[motion.CameraYawHeadingRelated_2B52 * 6] << 2)));
            vision.dseg_2B6B_d += 2;
        }

        static void GetNextVisionTileNegative_seg032_6A9(VisionParams vision)
        {
            vision.dseg_2B63_5--;
            vision.currentTile_2B67_9 = UWTileMap.GetTileByPTR((int)(vision.currentTile_2B67_9.Ptr - (dseg_432[motion.CameraYawHeadingRelated_2B52 * 6] << 2)));
            vision.dseg_2B6B_d -= 2;
        }

        static bool MaybeTestVisionIntoDarkness_seg032_AF5(VisionParams si_vision, VisionParams di_vision)
        {
            var shading = shade.shadesdata[playerdat.lightlevel].ShadingArray_26EE;
            si_vision.dseg_2B65_7++;
            //shade.shadesdata[playerdat.lightlevel].ShadingArray_26EF;
            if (si_vision.dseg_2B65_7 > 0x10)
            {
                return false;
            }
            else
            {
            seg032_B35:
                if (shading[si_vision.dseg_2B6B_d + 0x43] == 0xF)
                {
                    //seg032_B11
                    GetNextVisionTilePositive_seg032_683(si_vision);
                    si_vision.CameraX_2b64_6 = 0;
                    MaybeTestVisibilityNextTile_seg032_6CF(si_vision, 0, 0);
                    if (si_vision.dseg_2B63_5 <= di_vision.dseg_2B63_5)
                    {
                        goto seg032_B35;
                    }
                    return false;
                }
                else
                {
                    //seg032_B45
                    if (
                        (si_vision.dseg_2B65_7 > 1)
                        ||
                        ((Math.Abs(si_vision.CameraX_2b64_6 - playerdat.CAM_x) + Math.Abs(si_vision.CameraY_2b66_8 - playerdat.CAM_y)) > 0x10)
                        )
                    {
                        //seg032_B76
                        si_vision.FovYawX_1 = (short)((si_vision.dseg_2B63_5 << 8) + si_vision.CameraX_2b64_6 - playerdat.CAM_x);
                        si_vision.FovYawX_1 -= (short)(2 + Math.Abs(si_vision.FovYawX_1) / 0x32);
                        si_vision.FovYawY_3 = (short)((si_vision.dseg_2B65_7 << 8) - playerdat.CAM_y);
                    }
                    //seg032_BB4
                    si_vision.currentTile_2B67_9 = UWTileMap.GetTileByPTR((int)(si_vision.currentTile_2B67_9.Ptr + (dseg_432[2 + motion.CameraYawHeadingRelated_2B52 * 6] << 2)));
                    si_vision.dseg_2B6B_d += 0x42;
                    si_vision.CameraY_2b66_8 = 0;
                    di_vision.dseg_2B65_7++;
                seg032_BFA:
                    if ((shading[di_vision.dseg_2B6B_d + 0x43] & 0xF) == 0xF)
                    {
                        GetNextVisionTileNegative_seg032_6A9(di_vision);
                        di_vision.CameraX_2b64_6 = 0xFF;
                        MaybeTestVisibilityNextTile_seg032_6CF(di_vision, 0, 0);
                        if (si_vision.dseg_2B63_5 <= di_vision.dseg_2B63_5)
                        {
                            goto seg032_BFA;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {//seg032_C0A
                        if (di_vision.dseg_2B65_7 <= 1)
                        {
                            //seg032_c10
                            if (Math.Abs(di_vision.CameraX_2b64_6 - playerdat.CAM_x) + Math.Abs(di_vision.CameraY_2b66_8 - playerdat.CAM_y) <= 0x10)
                            {
                                goto Seg032_C7A;
                            }
                        }
                        //Seg032_C3B 
                        di_vision.FovYawX_1 = (short)(((di_vision.dseg_2B63_5 << 8) + di_vision.CameraX_2b64_6) - playerdat.CAM_x);
                        di_vision.FovYawX_1 += (short)(2 + Math.Abs(di_vision.FovYawX_1) / 0x32);
                        di_vision.FovYawY_3 = (short)(((di_vision.dseg_2B65_7 << 8) - playerdat.CAM_y) - 1);
                    Seg032_C7A:
                        di_vision.CameraY_2b66_8 = 0;
                        di_vision.dseg_2B6B_d += 0x42;
                        di_vision.currentTile_2B67_9 = UWTileMap.GetTileByPTR((int)(di_vision.currentTile_2B67_9.Ptr + (dseg_432[2 + motion.CameraYawHeadingRelated_2B52 * 6] << 2)));
                        return true;
                    }
                }
            }
        }

    }//end class
}