using System;
using System.IO;

namespace Underworld
{

    /// <summary>
    /// Functions related to pathfinding
    /// </summary>
    public class Pathfind : UWClass
    {
        public static int MaybeMaxTravelDistance_dseg_67d6_2272;
        public static int MaybePathIndexOrLength_dseg_67d6_225A;

        static bool TraverseRelated_dseg_67d6_224B;


        static int[] tilewallflags = new int[] { 30, 0, 19, 21, 11, 13, 32, 32, 32, 32 };

        static int[] TilePathingFlags = new int[] { 0xFF, 3, 0xFF, 2, 0xFF, 0, 0xFF, 1, 0xFF };

        static int[] SlopePathingTypes = new int[] { 6, 8, 7, 9 };

        public static int[] PathXOffsetTable = new int[] { 0, 1, 0, -1 };
        public static int[] PathYOffsetTable = new int[] { 1, 0, -1, 0 };

        /// <summary>
        /// Accumulates some values into Segment 57 from Seg56.
        /// </summary>
        /// <param name="Seg57Record"></param>
        public static void UpdateSeg57Values(PathFind57 Seg57Record)
        {
            //Seg006_1413_2934
            //does some path finding work
            Seg57Record.unk2_0_6_maybeZ = 0;
            Seg57Record.X0 = FinalPath56.finalpath[0].X0;
            Seg57Record.Y1 = FinalPath56.finalpath[0].Y1;
            Seg57Record.UNK3 = MaybePathIndexOrLength_dseg_67d6_225A;
            var index_var1 = 0;

            while (index_var1 < MaybePathIndexOrLength_dseg_67d6_225A)
            {//seg006_1413_2A42
                var accumualted_var3 = 0;
                var var2 = 0;

                while (var2 < 4)
                {
                    //seg006_1413_2981
                    var Seg56Record = FinalPath56.finalpath[index_var1 + var2];
                    var NextSeg56Record = Seg56Record.Next();

                    var ax = ((NextSeg56Record.X0 - Seg56Record.X0) * 3) + (NextSeg56Record.Y1 - Seg56Record.Y1);

                    accumualted_var3 += (TilePathingFlags[4 + ax] & 0x3) << (var2 << 1);

                    //seg006_1413_2A14: 
                    var2++;
                }


                //seg006_1413_2A23:
                index_var1 += 4;
            }

            //seg006_1413_2A4E:
            index_var1 = 0;
            while (index_var1 < MaybePathIndexOrLength_dseg_67d6_225A)
            {
                var var4 = 0;
                var var2 = 0;

                //seg006_1413_2A8B
                while (var2 < 8)
                {
                    //seg006_1413_2A5E:
                    var Seg56Record = FinalPath56.finalpath[index_var1 + var2];
                    var NextSeg56Record = Seg56Record.Next();
                    var4 += (NextSeg56Record.flag3 & 0x1) << var2;

                    var2++;
                }
                //This needs to be tested.
                var offset = Seg57Record.PTR + (index_var1 / 8);
                DataLoader.setAt(PathFind57.pathfind57data, offset + 0x14, 8, var4);

                index_var1 += 8;
            }
        }

        public static bool PathFindBetweenTiles(uwObject critter, int currTileX_arg0, int currTileY_arg2, int CurrFloorHeight_arg4, int TargetTileX_arg6, int TargetTileY_arg8, int TargetFloorHeight_argA, int LikelyRangeArgC)
        {
            var MaybePathPathLength_var1D = 0;
            MaybeMaxTravelDistance_dseg_67d6_2272 = LikelyRangeArgC;
            var NoOfStepsIndex_var13 = 0;
            var Counter_var14 = 0;
            var ProbablyPathDistance_var1E = 0;

            var varC = Path5859.Path5859_Records[0];
            var var12 = Path5859.Path5859_Records[0x40];

            PathFindingData49.pathfindmap49 = new byte[0x5000];
            var LikelyPathFindRange_var22 = 5;
            if (_RES == GAME_UW2)
            {
                if (worlds.GetWorldNo(playerdat.dungeon_level) == 0)
                {
                    LikelyPathFindRange_var22 = 0xA;
                }
            }

            int X1_var6;
            int Y1_var8;
            int X2_var_7;
            int Y2_var_9;

            //seg006_1413_19F6:
            if (currTileX_arg0 >= TargetTileX_arg6)
            {
                //seg006_1413_1A17:
                if (TargetTileX_arg6 - LikelyPathFindRange_var22 <= 1)
                {
                    //seg006_1413_1A2F:
                    X1_var6 = 1;
                }
                else
                {
                    X1_var6 = TargetTileX_arg6 - LikelyPathFindRange_var22;
                }
            }
            else
            {
                //seg006_1413:19FE
                if (currTileX_arg0 - LikelyPathFindRange_var22 <= 1)
                {
                    //seg006_1413_1A13
                    X1_var6 = 1;
                }
                else
                {
                    X1_var6 = currTileX_arg0 - LikelyPathFindRange_var22;
                }
            }

            //seg006_1413_1A31:
            if (currTileY_arg2 >= TargetTileY_arg8)
            {
                //seg006_1413_1A55:
                if (TargetTileY_arg8 - LikelyPathFindRange_var22 <= 1)
                {
                    Y1_var8 = 1;
                }
                else
                {
                    Y1_var8 = TargetTileY_arg8 - LikelyPathFindRange_var22;
                }
            }
            else
            {
                if (currTileY_arg2 - LikelyPathFindRange_var22 <= 1)
                {
                    Y1_var8 = 1;
                }
                else
                {
                    Y1_var8 = currTileY_arg2 - LikelyPathFindRange_var22;
                }
            }

            //seg006_1413_1A6F:
            if (currTileX_arg0 <= TargetTileX_arg6)
            {
                //seg006_1413_1A93:
                if (TargetTileX_arg6 + LikelyPathFindRange_var22 >= 0x40)
                {
                    X2_var_7 = 0x40;
                }
                else
                {
                    X2_var_7 = TargetTileX_arg6 + LikelyPathFindRange_var22;
                }
            }
            else
            {
                if (currTileX_arg0 + LikelyPathFindRange_var22 >= 0x40)
                {
                    X2_var_7 = 0x40;
                }
                else
                {
                    X2_var_7 = currTileX_arg0 + LikelyPathFindRange_var22;
                }
            }

            //seg006_1413_1AAD:
            if (currTileY_arg2 <= TargetTileY_arg8)
            {
                //seg006_1413_1AD1:
                if (TargetTileY_arg8 + LikelyPathFindRange_var22 >= 0x40)
                {
                    Y2_var_9 = 0x40;
                }
                else
                {
                    Y2_var_9 = TargetTileY_arg8 + LikelyPathFindRange_var22;
                }
            }
            else
            {
                if (currTileY_arg2 + LikelyPathFindRange_var22 >= 0x40)
                {
                    Y2_var_9 = 0x40;
                }
                else
                {
                    Y2_var_9 = currTileY_arg2 + LikelyPathFindRange_var22;
                }
            }


            //seg006_1413_1AEE:

            //For some reason I'm not storing the Y tile here. But that is what the code is doing?
            PathFindingData49.pathfindtiles[currTileX_arg0, currTileY_arg2].X0 = currTileX_arg0;
            PathFindingData49.pathfindtiles[currTileX_arg0, currTileY_arg2].Z2 = CurrFloorHeight_arg4;
            PathFindingData49.pathfindtiles[currTileX_arg0, currTileY_arg2].unk4 = 0;

            var si = 0;
            while (si < 4)
            {
                var NeighbourTileX = currTileX_arg0 + PathXOffsetTable[si];
                var NeighbourTileY = currTileY_arg2 + PathYOffsetTable[si];
                // if (UWTileMap.ValidTile(NeighbourTileX, NeighbourTileY))
                // {
                var NeighbourPathTile = PathFindingData49.pathfindtiles[NeighbourTileX, NeighbourTileY];
                ProbablyPathDistance_var1E = 0;
                var tmp = NeighbourPathTile.Z2;//out value from traverse
                                               //seg006_1413_1BC8:
                var res = TraverseMultipleTiles(
                    critter: critter,
                    tile1_X_arg0: 0, tile1_Y_arg2: 0,
                    tile2_X_arg4: currTileX_arg0, tile2_Y_arg6: currTileY_arg2,
                    tile3_X_arg8: NeighbourTileX, tile3_Y_argA: NeighbourTileY,
                    argC: (int)Loader.getAt(npc.SpecialMotionHandler, 4, 16), argE: (int)Loader.getAt(npc.SpecialMotionHandler, 6, 16),
                    ProbablyHeight_arg10: CurrFloorHeight_arg4,
                    arg12: ref tmp,
                    PathDistanceResult_arg16: ref ProbablyPathDistance_var1E);
                NeighbourPathTile.Z2 = tmp;

                if (res)
                {
                    //seg006_1413_1BDA:
                    if ((NeighbourTileX == TargetTileX_arg6) && (NeighbourTileY == TargetTileY_arg8))
                    {
                        //seg006_1413_1BEA:
                        //Trivial case where the neighbour tile is the target.
                        MaybePathIndexOrLength_dseg_67d6_225A = 1;
                        FinalPath56.finalpath[0].X0 = currTileX_arg0;
                        FinalPath56.finalpath[0].Y1 = currTileY_arg2;
                        FinalPath56.finalpath[0].flag3 = 0;

                        FinalPath56.finalpath[1].X0 = NeighbourTileX;
                        FinalPath56.finalpath[1].Y1 = NeighbourTileY;
                        return true;
                    }
                    else
                    {
                        //seg006_1413_1C27
                        //traversable case
                        NeighbourPathTile.X0 = currTileX_arg0;
                        NeighbourPathTile.Y1 = currTileY_arg2;
                        NeighbourPathTile.unk4 = 1; //mark as traversable?
                        NeighbourPathTile.unk3_bit1_7 = ProbablyPathDistance_var1E;

                        Path5859.Path5859_Records[NoOfStepsIndex_var13].X0 = NeighbourTileX;
                        Path5859.Path5859_Records[NoOfStepsIndex_var13].Y1 = NeighbourTileY;

                        NoOfStepsIndex_var13++;
                    }
                }
                //}
                //seg006_1413_1C7A:
                si++;
            }

            //seg006_1413_1C83:
            MaybePathPathLength_var1D = 1;
            //seg006_1413_1F52: 
            while ((MaybePathPathLength_var1D < 0x28) && (NoOfStepsIndex_var13 > 0))
            {
                //seg006_1413_1C8A:
                Counter_var14 = 0;
                var var5 = 0;
                //seg006_1413_1EFB  
                while ((var5 < NoOfStepsIndex_var13) && (Counter_var14 < 0x40))
                {
                    //seg006_1413_1C95:
                    var Tile2X_var1 = Path5859.Path5859_Records[varC.index + var5].X0;
                    var Tile2Y_var2 = Path5859.Path5859_Records[varC.index + var5].Y1;
                    var TileRecord_var1C = PathFindingData49.pathfindtiles[Tile2X_var1, Tile2Y_var2];
                    si = 0;
                    while (si < 4)//1413:1EEB
                    {
                        //seg006_1413_1CE1:
                        var NeighbourTileX = Tile2X_var1 + PathXOffsetTable[si];
                        var NeighbourTileY = Tile2Y_var2 + PathYOffsetTable[si];
                        if (//check if tile is within the search bounds for pathfinding
                                (NeighbourTileX >= X1_var6)
                                &&
                                (NeighbourTileX <= X2_var_7)
                                &&
                                (NeighbourTileY >= Y1_var8)
                                &&
                                (NeighbourTileY <= Y2_var_9)
                        )
                        {
                            //seg006_1413_1D23: 
                            var NeighbourPathTile = PathFindingData49.pathfindtiles[NeighbourTileX, NeighbourTileY];
                            ProbablyPathDistance_var1E = TileRecord_var1C.unk3_bit1_7;
                            if ((NeighbourTileX != TileRecord_var1C.X0) || (NeighbourTileY != TileRecord_var1C.Y1))
                            {
                                var Height_var1F = 0;
                                //seg006_1413_1D68:    //This traverse is coming back wrong
                                var res_var20 = TraverseMultipleTiles(
                                    critter: critter,
                                    tile1_X_arg0: TileRecord_var1C.X0, tile1_Y_arg2: TileRecord_var1C.Y1,
                                    tile2_X_arg4: Tile2X_var1, tile2_Y_arg6: Tile2Y_var2,
                                    tile3_X_arg8: NeighbourTileX, tile3_Y_argA: NeighbourTileY,
                                    argC: (int)Loader.getAt(npc.SpecialMotionHandler, 4, 16), argE: (int)Loader.getAt(npc.SpecialMotionHandler, 6, 16),
                                    ProbablyHeight_arg10: TileRecord_var1C.Z2,
                                    arg12: ref Height_var1F,
                                    PathDistanceResult_arg16: ref ProbablyPathDistance_var1E);

                                //seg006_1413_1DBA
                                var var21 = false;
                                if (NeighbourPathTile.X0 == 0)
                                {
                                    var21 = true;
                                }

                                if (res_var20)
                                {
                                    bool Run1E0D = false;
                                    if (!var21)
                                    {
                                        //seg006_1413:1DCC 
                                        if (TileRecord_var1C.unk4 < NeighbourPathTile.unk4)
                                        {
                                            if (Math.Abs(Height_var1F - TargetFloorHeight_argA) < Math.Abs(NeighbourPathTile.Z2 - TargetFloorHeight_argA))
                                            {
                                                Run1E0D = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Run1E0D = true;
                                    }
                                    if (Run1E0D)
                                    {
                                        //seg006_1413_1E0D:
                                        NeighbourPathTile.X0 = Tile2X_var1;
                                        NeighbourPathTile.Y1 = Tile2Y_var2;
                                        NeighbourPathTile.Z2 = Height_var1F;
                                        NeighbourPathTile.unk4 = MaybePathPathLength_var1D + 1;
                                        NeighbourPathTile.unk3_bit1_7 = ProbablyPathDistance_var1E & 0x7F;
                                        if (TraverseRelated_dseg_67d6_224B)
                                        {
                                            TileRecord_var1C.unk3_bit0 = 1;
                                        }
                                        else
                                        {
                                            TileRecord_var1C.unk3_bit0 = 0;
                                        }

                                        if (var21)
                                        {
                                            //seg006_1413_1E56:
                                            Path5859.Path5859_Records[var12.index + Counter_var14].X0 = NeighbourTileX;
                                            Path5859.Path5859_Records[var12.index + Counter_var14].Y1 = NeighbourTileY;
                                            Counter_var14++;
                                        }

                                        if ((NeighbourTileX == TargetTileX_arg6) && (NeighbourTileY == TargetTileY_arg8))
                                        {
                                            var tmp = NeighbourPathTile.Z2;
                                            var res = TraverseMultipleTiles(
                                                critter: critter,
                                                tile1_X_arg0: Tile2X_var1, tile1_Y_arg2: Tile2Y_var2,
                                                tile2_X_arg4: NeighbourTileX, tile2_Y_arg6: NeighbourTileY,
                                                tile3_X_arg8: 0, tile3_Y_argA: 0,
                                                argC: (int)Loader.getAt(npc.SpecialMotionHandler, 4, 16), argE: (int)Loader.getAt(npc.SpecialMotionHandler, 6, 16),
                                                ProbablyHeight_arg10: NeighbourPathTile.Z2,
                                                arg12: ref tmp, PathDistanceResult_arg16: ref ProbablyPathDistance_var1E);
                                            NeighbourPathTile.Z2 = tmp;

                                            if (res)
                                            {
                                                //a valid path has been found. save it and return true
                                                StorePath(
                                                    MaybeLength_Arg0: MaybePathPathLength_var1D,
                                                    targetXarg2: TargetTileX_arg6,
                                                    targetYArg4: TargetTileY_arg8);
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        si++;// to Seg006_1413_1EEA
                    }
                    //seg006_1413_1EF3:
                    var5++;
                }
                //seg006_1413_1F0C

                //fill this gap with ptr changes to varC/Var12
                if (varC.index == 0)
                {
                    //seg006_1413:1F1A
                    varC = Path5859.Path5859_Records[0x40];
                    var12 = Path5859.Path5859_Records[0];
                }
                else
                {
                    //seg006_1413_1F30:
                    varC = Path5859.Path5859_Records[0];
                    var12 = Path5859.Path5859_Records[0x40];
                }

                //seg006_1413_1F44:
                NoOfStepsIndex_var13 = Counter_var14;
                MaybePathPathLength_var1D++;
            }

            return false;//while loop at 1c83 got nothing.
        }


        public static bool TraverseMultipleTiles(uwObject critter, int tile1_X_arg0, int tile1_Y_arg2, int tile2_X_arg4, int tile2_Y_arg6, int tile3_X_arg8, int tile3_Y_argA, int argC, int argE, int ProbablyHeight_arg10, ref int arg12, ref int PathDistanceResult_arg16)
        {
            var TallestHeight_Tile1_2_var13 = 0;
            var TallestHeight_Tile2_3_var14 = 0;
            var TallestHeight_Tile2_3_var15 = 0;
            var ObjectHeight_var16 = 0;
            var var17 = false;
            var var18 = false;
            var TestedDoors_var21 = false;
            TraverseRelated_dseg_67d6_224B = false;
            var tile1 = UWTileMap.current_tilemap.Tiles[tile1_X_arg0, tile1_Y_arg2];
            var tile2 = UWTileMap.current_tilemap.Tiles[tile2_X_arg4, tile2_Y_arg6];
            var tile3 = UWTileMap.current_tilemap.Tiles[tile3_X_arg8, tile3_Y_argA];
            var tile2type = tile2.tileType;
            var tile3type = tile3.tileType;
            var tile2terrain = TerrainDatLoader.GetTerrainTypeNo(tile2);
            var tile3terrain = TerrainDatLoader.GetTerrainTypeNo(tile3);
            var si = 0;

            if (tile1_X_arg0 == 0)
            {
                //seg006_1413_E57:
                arg12 = ProbablyHeight_arg10;
                TallestHeight_Tile2_3_var14 = tile3.floorHeight;

                if ((tile3_X_arg8 > tile2_X_arg4) && ((tilewallflags[tile3type] & 2) != 0))
                {
                    return false;
                }

                if ((tile3_X_arg8 < tile2_X_arg4) && ((tilewallflags[tile3type] & 4) != 0))
                {
                    return false;
                }

                if ((tile3_Y_argA > tile2_Y_arg6) && ((tilewallflags[tile3type] & 8) != 0))
                {
                    return false;
                }

                if ((tile3_Y_argA < tile2_Y_arg6) && ((tilewallflags[tile3type] & 0x10) != 0))
                {
                    return false;
                }

                if ((tile3_X_arg8 > tile2_X_arg4) && ((tilewallflags[tile2type] & 0x4) != 0))
                {
                    return false;
                }

                if ((tile3_X_arg8 < tile2_X_arg4) && ((tilewallflags[tile2type] & 0x2) != 0))
                {
                    return false;
                }

                if ((tile3_Y_argA > tile2_Y_arg6) && ((tilewallflags[tile2type] & 0x10) != 0))
                {
                    return false;
                }

                if ((tile3_Y_argA < tile2_Y_arg6) && ((tilewallflags[tile2type] & 0x8) != 0))
                {
                    return false;
                }
                //seg006_1413_F47
                if ((argC & 0x1000) == 0)
                {
                    return true;
                }
                else
                {
                    if ((tile3type >= 6) && (tile3type <= 9))
                    {
                        var idx = ((tile3_X_arg8 - tile2_X_arg4) * 3) - (tile3_Y_argA - tile2_Y_arg6); // should only return 1,-1 (yaxis), 3,-3(xaxis) in a cross pattern
                        var bx = TilePathingFlags[4 + idx];//This may be wrong!
                        if (SlopePathingTypes[bx] != tile3type)
                        {
                            TallestHeight_Tile2_3_var14++;
                        }
                    }
                    if (TallestHeight_Tile2_3_var14 <= ProbablyHeight_arg10 + 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {

                //seg006_1413_FAA:
                if (tile3_X_arg8 != 0)
                {
                    //seg006_1413_10E2:
                    arg12 = ProbablyHeight_arg10;
                    if (tile3_X_arg8 <= tile2_X_arg4)
                    {
                        //seg006_1413_1117: 
                        if (tile3_X_arg8 >= tile2_X_arg4)
                        {
                            //seg006_1413_1140
                            if (tile3_Y_argA <= tile2_Y_arg6)
                            {
                                //seg006_1413_1169: 
                                if (tile3_Y_argA <= tile2_Y_arg6)
                                {
                                    //seg006_1413_1169:
                                    if (tile3_Y_argA < tile2_Y_arg6)
                                    {
                                        if ((tilewallflags[tile3type] & 0x10) != 0)
                                        {
                                            return false;
                                        }
                                        if ((tilewallflags[tile2type] & 0x8) != 0)
                                        {
                                            return false;
                                        }
                                    }
                                    //jmp     NoBlockingTiles_seg006_1413_1192
                                }
                                else
                                {
                                    //seg006_1413:1148
                                    if ((tilewallflags[tile3type] & 0x8) != 0)
                                    {
                                        return false;
                                    }
                                    if ((tilewallflags[tile2type] & 0x10) != 0)
                                    {
                                        return false;
                                    }
                                    //jmp     NoBlockingTiles_seg006_1413_1192
                                }
                            }
                            else
                            {
                                //seg006_1413:1148
                                if ((tilewallflags[tile3type] & 8) != 0)
                                {
                                    return false;
                                }
                                if ((tilewallflags[tile2type] & 0x10) != 0)
                                {
                                    return false;
                                }
                                //jmp     NoBlockingTiles_seg006_1413_1192
                            }
                        }
                        else
                        {
                            //seg006_1413:111F
                            if ((tilewallflags[tile3type] & 0x4) != 0)
                            {
                                return false;
                            }
                            if ((tilewallflags[tile2type] & 0x2) != 0)
                            {
                                return false;
                            }
                            //jmp     NoBlockingTiles_seg006_1413_1192
                        }
                    }
                    else
                    {
                        //seg006_1413:10F3
                        if ((tilewallflags[tile3type] & 0x2) != 0)
                        {
                            return false;
                        }
                        if ((tilewallflags[tile2type] & 0x4) != 0)
                        {
                            return false;
                        }
                        //jmp     NoBlockingTiles_seg006_1413_1192
                    }


                    //NoBlockingTiles_seg006_1413_1192:?
                    ObjectHeight_var16 = 0;
                    var next = tile2.indexObjectList;
                    //jmp     LoopObjectsNoBlockingTiles_seg006_1413_140B
                    while ((next != 0) && (ObjectHeight_var16 == 0))
                    {
                        var obj = UWTileMap.current_tilemap.LevelObjects[next];
                        if ((obj.majorclass == 5) && (obj.minorclass == 0) && (obj.classindex < 8))
                        {
                            // is a closed door
                            if (npc.CharacterDoorLockAndKeyInteraction(critter, obj, 0) == 0)
                            {
                                //seg006_1413_1216:
                                var DoorHeading_var22 = obj.heading;
                                var DoorXPos_var23 = obj.xpos;
                                var DoorYPos_var24 = obj.ypos;
                                if (TestedDoors_var21 == false)
                                {
                                    if (tile1_X_arg0 >= tile2_X_arg4)
                                    {
                                        //seg006_1413_1274:
                                        if (tile1_X_arg0 <= tile2_X_arg4)
                                        {
                                            //seg006_1413_129B
                                            if (tile1_Y_arg2 >= tile2_Y_arg6)
                                            {
                                                //seg006_1413_12C2:
                                                if (tile2_X_arg4 >= tile3_X_arg8)
                                                {
                                                    //seg006_1413_12CF:
                                                    if (tile2_X_arg4 <= tile3_X_arg8)
                                                    {
                                                        //seg006_1413_12DC:
                                                        si = 0xA;
                                                    }
                                                    else
                                                    {
                                                        si = 9;
                                                    }
                                                }
                                                else
                                                {
                                                    si = 0xB;
                                                }
                                            }
                                            else
                                            {
                                                //seg006_1413:12A3
                                                if (tile2_X_arg4 >= tile3_X_arg8)
                                                {
                                                    //seg006_1413_12B0:
                                                    if (tile2_X_arg4 <= tile3_X_arg8)
                                                    {
                                                        //seg006_1413_12BD:
                                                        si = 7;
                                                    }
                                                    else
                                                    {
                                                        si = 6;
                                                    }
                                                }
                                                else
                                                {
                                                    si = 8;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //seg006_1413:127C
                                            if (tile2_Y_arg6 >= tile3_Y_argA)
                                            {
                                                //seg006_1413_1289
                                                if (tile2_Y_arg6 <= tile3_Y_argA)
                                                {
                                                    //seg006_1413_1296: 
                                                    si = 4;
                                                }
                                                else
                                                {
                                                    si = 5;
                                                }
                                            }
                                            else
                                            {
                                                si = 3;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //seg006_1413:1256
                                        if (tile2_Y_arg6 >= tile3_Y_argA)
                                        {
                                            //seg006_1413_1262:
                                            if (tile2_Y_arg6 <= tile3_Y_argA)
                                            {
                                                //seg006_1413_126F:
                                                si = 1;
                                            }
                                            else
                                            {
                                                si = 2;
                                            }
                                        }
                                        else
                                        {
                                            si = 0;
                                        }
                                    }
                                    TestedDoors_var21 = true;
                                }

                                //seg006_1413_12E3:
                                switch (si)
                                {

                                    case 0:
                                    case 9:
                                        {
                                            if (DoorHeading_var22 == 0)
                                            {
                                                //seg006_1413_1326:
                                                if (DoorYPos_var24 > 3)
                                                {
                                                    return false;
                                                }
                                            }
                                            else
                                            {
                                                if (DoorHeading_var22 == 1)
                                                {
                                                    return false;
                                                }
                                                if (DoorHeading_var22 == 2)
                                                {
                                                    //seg006_1413_1339: 
                                                    if (DoorXPos_var23 < 4)
                                                    {
                                                        return false;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case 1:
                                    case 4:
                                        {
                                            if (DoorHeading_var22 != 0)
                                            {
                                                return false;
                                            }
                                        }
                                        break;
                                    case 2:
                                    case 6:
                                        {
                                            if (DoorHeading_var22 != 0)
                                            {
                                                if (DoorHeading_var22 == 2)
                                                {
                                                    //seg006_1413_136B:
                                                    if (DoorXPos_var23 < 4)
                                                    {
                                                        return false;
                                                    }
                                                }
                                                else
                                                {
                                                    if (DoorHeading_var22 == 3)
                                                    {
                                                        return false;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //seg006_1413_135D: 
                                                if (DoorYPos_var24 < 4)
                                                {
                                                    return false;
                                                }
                                            }
                                        }
                                        break;
                                    case 3:
                                    case 11:
                                        {
                                            if (DoorHeading_var22 == 0)
                                            {
                                                //seg006_1413_1393:
                                                if (DoorYPos_var24 > 3)
                                                {
                                                    return false;
                                                }
                                            }
                                            else
                                            {
                                                if (DoorHeading_var22 == 2)
                                                {
                                                    //seg006_1413_139E:
                                                    if (DoorXPos_var23 > 3)
                                                    {
                                                        return false;
                                                    }
                                                }
                                                else
                                                {
                                                    if (DoorHeading_var22 == 3)
                                                    {
                                                        return false;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case 5:
                                    case 8:
                                        {
                                            if (DoorHeading_var22 == 0)
                                            {
                                                if (DoorYPos_var24 < 4)
                                                {
                                                    return false;
                                                }
                                            }
                                            else
                                            {
                                                if (DoorXPos_var23 == 1)
                                                {
                                                    return false;
                                                }
                                                if (DoorHeading_var22 == 2)
                                                {
                                                    if (DoorXPos_var23 > 3)
                                                    {
                                                        return false;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case 7:
                                    case 10:
                                        {
                                            if (DoorHeading_var22 != 2)
                                            {
                                                return false;
                                            }
                                        }
                                        break;
                                }

                            }
                        }
                        else
                        {//seg006_1413_13DE
                            //not a closed door
                            if (commonObjDat.UnknownFlag3_1(obj.item_id))
                            {//seg006_1413:13EA
                                ObjectHeight_var16 = (commonObjDat.height(obj.item_id) + obj.zpos) >> 3;
                            }
                        }

                        //NEXT_seg006_1413_1402
                        next = obj.next;
                    }
                    //AfterLoop_seg006_1413_142A:
                    if ((argC & 0x1000) != 0)
                    {
                        // seg006_1413_144A
                        TallestHeight_Tile1_2_var13 = Math.Max(tile1.floorHeight, tile2.floorHeight);
                        TallestHeight_Tile2_3_var14 = Math.Max(tile2.floorHeight, tile3.floorHeight);
                        TallestHeight_Tile2_3_var15 = TallestHeight_Tile2_3_var14;
                        TallestHeight_Tile1_2_var13 = Math.Max(ProbablyHeight_arg10, TallestHeight_Tile1_2_var13);


                        //seg006_1413_14CC:
                        if ((tile3type >= 6) && (tile3type <= 9))
                        {
                            var idx = ((tile3_X_arg8 - tile2_X_arg4) * 3) - (tile3_Y_argA - tile2_Y_arg6);
                            var bx = TilePathingFlags[4 + idx];//This may be wrong!
                            if (SlopePathingTypes[bx] != tile3type)
                            {
                                TallestHeight_Tile2_3_var14++;
                            }
                        }

                        //seg006_1413_151A:
                        var tallest = Math.Max(TallestHeight_Tile1_2_var13, TallestHeight_Tile2_3_var14);

                        if (npc.currObjHeight + (tallest << 3) <= 0x7F)
                        {
                            //seg006_1413_1531:
                            if (TallestHeight_Tile1_2_var13 <= TallestHeight_Tile2_3_var14 + 1)
                            {
                                //seg006_1413_1590:
                                if ((ObjectHeight_var16 != 0) && (TallestHeight_Tile1_2_var13 >= ObjectHeight_var16))
                                {//seg006_1413:159E 
                                    if (TallestHeight_Tile1_2_var13 <= ObjectHeight_var16 + 1)
                                    {
                                        var18 = true;
                                    }
                                }
                            }
                            else
                            {
                                //seg006_1413:1540
                                if (TallestHeight_Tile1_2_var13 <= ObjectHeight_var16 + 1)
                                {
                                    //seg006_1413_1585:
                                    TallestHeight_Tile2_3_var14 = ObjectHeight_var16;
                                    TallestHeight_Tile2_3_var15 = ObjectHeight_var16;
                                    var18 = true;
                                    //jump seg006_1413_15AE:
                                }
                                else
                                {
                                    //seg006_1413:154F
                                    var17 = true;
                                    if (TallestHeight_Tile2_3_var14 <= ObjectHeight_var16)
                                    {
                                        //seg006_1413_155D
                                        arg12 = ObjectHeight_var16;
                                    }
                                    else
                                    {
                                        arg12 = TallestHeight_Tile2_3_var14;
                                    }
                                    PathDistanceResult_arg16 = TallestHeight_Tile1_2_var13 - arg12 - 1;
                                    if (PathDistanceResult_arg16 > MaybeMaxTravelDistance_dseg_67d6_2272)
                                    {
                                        return false;
                                    }
                                    //else jump seg006_1413_15AE:
                                }
                            }

                            //seg006_1413_15AE:
                            if (TallestHeight_Tile2_3_var14 > TallestHeight_Tile1_2_var13 + 1)
                            {
                                return false;
                            }

                            //seg006_1413_15C2:
                            if (
                                (TallestHeight_Tile1_2_var13 > tile2.floorHeight + 1)
                                &&
                                (var17 == false)
                                &&
                                (var18 == false)
                            )
                            {
                                //seg006_1413_15F4:
                                if ((argC & (8 << tile2terrain)) != 0)
                                {
                                    return false;
                                }
                                //seg006_1413_1606:
                                if (
                                    ((argE & (8 << tile3terrain)) != 0)
                                    &&
                                    (PathDistanceResult_arg16 + 2 >= MaybeMaxTravelDistance_dseg_67d6_2272)
                                    )
                                {
                                    return false;
                                }

                                //seg006_1413_162C:
                                if (TallestHeight_Tile1_2_var13 > ObjectHeight_var16 + 1)
                                {
                                    arg12 = TallestHeight_Tile1_2_var13;
                                    if (TallestHeight_Tile1_2_var13 < TallestHeight_Tile2_3_var14)
                                    {
                                        return false;
                                    }
                                    if (critterObjectDat.unk0xA_5(critter.item_id) == false)
                                    {
                                        return false;
                                    }
                                    PathDistanceResult_arg16++;
                                    if (PathDistanceResult_arg16 < MaybeMaxTravelDistance_dseg_67d6_2272)
                                    {
                                        TraverseRelated_dseg_67d6_224B = true;
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                //seg006_1413:1688
                                if (
                                    (((argC) & (8 << tile2terrain)) != 0)
                                    &&
                                    var18 == false
                                    )
                                {
                                    //seg006_1413:169E
                                    if ((argC & (8 << tile3terrain)) == 0)
                                    {
                                        return false;
                                    }
                                    //seg006_1413_16AF
                                    if ((argE & (8 << tile3terrain)) == 0)
                                    {
                                        PathDistanceResult_arg16 += 2;
                                        if (PathDistanceResult_arg16 > MaybeMaxTravelDistance_dseg_67d6_2272)
                                        {
                                            return false;
                                        }
                                    }
                                    //seg006_1413_16D4:
                                    if (critterObjectDat.unk0xA_5(critter.item_id) == false)
                                    {
                                        return false;
                                    }
                                    //seg006_1413_16E8:
                                    PathDistanceResult_arg16++;
                                    if (PathDistanceResult_arg16 < MaybeMaxTravelDistance_dseg_67d6_2272)
                                    {
                                        TraverseRelated_dseg_67d6_224B = true;
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }

                            }

                            //seg006_1413_1708:
                            arg12 = TallestHeight_Tile2_3_var15;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //seg006_1413:1432
                        arg12 = 0x10 - ((npc.currObjHeight + 3) >> 2);
                        return true;
                    }
                }
                else
                {
                    //seg006_1413_FB3
                    arg12 = ProbablyHeight_arg10;
                    if ((argC & 0x1000) == 0)
                    {
                        return true;
                    }
                    else
                    {
                        //LoopObjectsArgC_seg006_1413_1026:
                        var next = tile2.indexObjectList;
                        while ((next != 0) && (ObjectHeight_var16 == 0))
                        {
                            var obj = UWTileMap.current_tilemap.LevelObjects[next];
                            if (commonObjDat.UnknownFlag3_1(obj.item_id))
                            {
                                ObjectHeight_var16 = (obj.zpos + commonObjDat.height(obj.item_id)) >> 3;
                            }
                            next = obj.next;
                        }

                        //seg006_1413_1042:
                        TallestHeight_Tile2_3_var14 = tile2.floorHeight;
                        if (ObjectHeight_var16 > TallestHeight_Tile2_3_var14)
                        {
                            TallestHeight_Tile2_3_var14 = ObjectHeight_var16;
                            var18 = true;
                        }

                        //seg006_1413_1060:                        
                        if (tile1.floorHeight > TallestHeight_Tile2_3_var14 + 1)
                        {
                            //seg006_1413_1076:
                            arg12 = TallestHeight_Tile2_3_var14;
                            PathDistanceResult_arg16 = ProbablyHeight_arg10 - arg12 - 1;
                            if (PathDistanceResult_arg16 > MaybeMaxTravelDistance_dseg_67d6_2272)
                            {
                                return false;
                            }
                        }
                        //seg006_1413_109E:
                        if (var18 != false)
                        {
                            return true;
                        }
                        else
                        {
                            //seg006_1413:10A7
                            if ((argC & (8 << tile2terrain)) == 0)
                            {
                                //seg006_1413_10B9:
                                if ((argE & (8 << tile2terrain)) == 0)
                                {
                                    return true;
                                }
                                else
                                {
                                    PathDistanceResult_arg16 += 2;
                                    if (PathDistanceResult_arg16 <= MaybeMaxTravelDistance_dseg_67d6_2272)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }

        static void StorePath(int MaybeLength_Arg0, int targetXarg2, int targetYArg4)
        {
            MaybePathIndexOrLength_dseg_67d6_225A = MaybeLength_Arg0 + 1;
            var EndPath56 = FinalPath56.finalpath[MaybeLength_Arg0].Next();
            EndPath56.X0 = targetXarg2;
            EndPath56.Y1 = targetYArg4;
            FinalPath56.finalpath[0].flag3 = 0;

            var cl = MaybeLength_Arg0 + 1;
            while (cl > 0)
            {
                var clPath56 = FinalPath56.finalpath[cl];
                var pathtile = PathFindingData49.pathfindtiles[clPath56.X0, clPath56.Y1];
                var PreviousClPath = clPath56.Previous();
                PreviousClPath.X0 = pathtile.X0;
                PreviousClPath.Y1 = pathtile.Y1;
                clPath56.flag3 = pathtile.unk3_bit0;
                cl--;
            }
            //File.WriteAllBytes("c:\\temp\\finalpath", FinalPath56.data056);
        }


        /// <summary>
        /// Tries to find a path in a straight line to the target.
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="CurrTileX"></param>
        /// <param name="CurrTileY"></param>
        /// <param name="targetX"></param>
        /// <param name="targetY"></param>
        /// <returns></returns>
        public static int PathAlongStraightLine_seg006_1413_205B(uwObject critter, int CurrTileX, int CurrTileY, int targetX, int targetY)
        {
            var var8 = 0x40;//is a byte
            var xVector_var1 = targetX - CurrTileX;
            var yVector_var2 = targetY - CurrTileY;
            var xVar3 = CurrTileX;
            var yVar4 = CurrTileY;
            var var6 = 0;
            bool UseVar3_si = false;//reference var3 or var4 in later calcs.
            bool UseVar3_diForX = false;//reference var3 or var4 in later calcs.
            var divisionVar7 = 0;
            var directionVar5 = 0;


            var TileVarC = UWTileMap.current_tilemap.Tiles[CurrTileX, CurrTileY];
            Pathfind.MaybeMaxTravelDistance_dseg_67d6_2272 = 0;

            if ((xVector_var1 == 0) && (yVector_var2 == 0))
            {
                return -1; //already at the target
            }
            else
            {
                //seg006_1413_20B0:
                if (xVector_var1 >= yVector_var2)
                {
                    //seg006_1413_20BB:
                    if (xVector_var1 < -yVector_var2)
                    {
                        //seg006_1413_2104
                        UseVar3_diForX = false;
                        UseVar3_si = true;
                        divisionVar7 = (xVector_var1 << 7) / yVector_var2;
                        directionVar5 = -1;
                        if (xVector_var1 <= 0)
                        {
                            //seg006_1413_213A:
                            var6 = -1;
                        }
                        else
                        {
                            var6 = 1;
                        }
                    }
                    else
                    {
                        //seg006_1413:20C8
                        UseVar3_diForX = true;
                        UseVar3_si = false;
                        divisionVar7 = (yVector_var2 << 7) / xVector_var1;
                        directionVar5 = 1;
                        if (yVector_var2 <= 0)
                        {
                            var6 = -1;
                        }
                        else
                        {
                            var6 = 1;
                        }
                    }
                }
                else
                {
                    //seg006_1413_2140:
                    if (xVector_var1 >= -yVector_var2)
                    {
                        UseVar3_diForX = false;
                        UseVar3_si = true;
                        divisionVar7 = (xVector_var1 << 7) / yVector_var2;
                        directionVar5 = 1;
                        if (xVector_var1 <= 0)
                        {
                            var6 = -1;
                        }
                        else
                        {
                            var6 = 1;
                        }
                    }
                    else
                    {
                        //seg006_1413_2189:
                        UseVar3_diForX = true;
                        UseVar3_si = false;
                        divisionVar7 = (yVector_var2 << 7) / xVector_var1;
                        directionVar5 = -1;
                        if (yVector_var2 <= 0)
                        {
                            var6 = -1;
                        }
                        else
                        {
                            var6 = 1;
                        }
                    }
                }


                //seg006_1413_21C0:
                FinalPath56.finalpath[0].X0 = CurrTileX;
                FinalPath56.finalpath[0].Y1 = CurrTileY;
                Pathfind.MaybePathIndexOrLength_dseg_67d6_225A = 1;
                FinalPath56.finalpath[0].Z2 = TileVarC.floorHeight;
                //seg006_1413_21F4:
                do
                {
                    if (UseVar3_diForX)
                    {
                        xVar3 += directionVar5;
                    }
                    else
                    {
                        yVar4 += directionVar5;
                    }

                    if (Pathfind.TestStraightPathTraversal_seg006_1413_2769(critter, xVar3, yVar4))
                    {
                        //seg006_1413_2214                           
                        var8 = ((var8 + (byte)(divisionVar7 & 0xFF)) & 0xFF);
                        if ((var8 & 0x80) != 0)
                        {
                            //seg006_1413:2220
                            var8 = var8 & 0x7F;
                            if (UseVar3_si)
                            {
                                xVar3 += var6;
                            }
                            else
                            {
                                yVar4 += var6;
                            }
                            if (Pathfind.TestStraightPathTraversal_seg006_1413_2769(critter, xVar3, yVar4) == false)
                            {
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        return 0;
                    }
                } while ((xVar3 != targetX) && (yVar4 != targetY));

                //seg006_1413:225C
                var Step1 = FinalPath56.finalpath[Pathfind.MaybePathIndexOrLength_dseg_67d6_225A];
                var Step2 = Step1.Previous();
                var Step3 = Step2.Previous();

                int varE = 0;
                var tmp = Step3.Z2; //this may be wrong.
                var res = Pathfind.TraverseMultipleTiles(
                    critter: critter,
                    tile1_X_arg0: Step3.X0, tile1_Y_arg2: Step3.Y1,
                    tile2_X_arg4: Step2.X0, tile2_Y_arg6: Step2.Y1,
                    tile3_X_arg8: 0, tile3_Y_argA: 0,
                    argC: (int)Loader.getAt(npc.SpecialMotionHandler, 4, 16), argE: (int)Loader.getAt(npc.SpecialMotionHandler, 6, 16),
                    ProbablyHeight_arg10: Step3.Z2,
                    arg12: ref xVar3,
                    PathDistanceResult_arg16: ref varE);
                Step3.Z2 = tmp;
                if (res)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }


        /// <summary>
        /// Checks if the proposed next step in the path will allow continued traversal following the last 2 tiles in the stored path
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static bool TestStraightPathTraversal_seg006_1413_2769(uwObject critter, int x, int y)
        {
            MaybeMaxTravelDistance_dseg_67d6_2272 = 0;
            var path56 = FinalPath56.finalpath[MaybePathIndexOrLength_dseg_67d6_225A];
            path56.X0 = x;
            path56.Y1 = y;
            MaybePathIndexOrLength_dseg_67d6_225A++;

            if (MaybePathIndexOrLength_dseg_67d6_225A <= 0x3F)
            {
                //seg006_1413_27B1:
                if (MaybePathIndexOrLength_dseg_67d6_225A == 2)
                {
                    //"trivial" case!
                    var height1 = FinalPath56.finalpath[1].Z2;
                    var var2 = 0;
                    var CanTraverse_var1 = TraverseMultipleTiles(
                        critter: critter,
                        tile1_X_arg0: 0, tile1_Y_arg2: 0,
                        tile2_X_arg4: FinalPath56.finalpath[0].X0, tile2_Y_arg6: FinalPath56.finalpath[0].Y1,
                        tile3_X_arg8: FinalPath56.finalpath[1].X0, tile3_Y_argA: FinalPath56.finalpath[1].Y1,
                        argC: (int)Loader.getAt(npc.SpecialMotionHandler, 4, 16), argE: (int)Loader.getAt(npc.SpecialMotionHandler, 6, 16),
                        ProbablyHeight_arg10: FinalPath56.finalpath[0].Z2, arg12: ref height1,
                        PathDistanceResult_arg16: ref var2);
                    FinalPath56.finalpath[1].Z2 = height1;

                    if (CanTraverse_var1)
                    {
                        return !TraverseRelated_dseg_67d6_224B;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    //seg006_1413_2828:
                    path56 = FinalPath56.finalpath[MaybePathIndexOrLength_dseg_67d6_225A];
                    var Prev1 = path56.Previous(); //record -1
                    var Prev2 = Prev1.Previous(); //record -2
                    var Prev3 = Prev2.Previous(); //record -3
                    var var2 = 0;
                    var height = Prev2.Z2;
                    //I will need to check if these params are all correct!
                    var CanTraverse_var1 = TraverseMultipleTiles(
                        critter: critter,
                        tile1_X_arg0: Prev3.X0, tile1_Y_arg2: Prev3.Y1,
                        tile2_X_arg4: Prev2.X0, tile2_Y_arg6: Prev2.Y1,
                        tile3_X_arg8: Prev1.X0, tile3_Y_argA: Prev1.Y1,
                        argC: (int)Loader.getAt(npc.SpecialMotionHandler, 4, 16), argE: (int)Loader.getAt(npc.SpecialMotionHandler, 6, 16),
                        ProbablyHeight_arg10: Prev3.Z2, arg12: ref height,
                        PathDistanceResult_arg16: ref var2);
                    Prev2.Z2 = height;

                    if (CanTraverse_var1 == true)
                    {
                        return !TraverseRelated_dseg_67d6_224B;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;//path too long.
            }
        }


        /// <summary>
        /// Looks like this tests if two points on the map are pathable? to each other.
        /// TestBetweenCoordinates_seg006_1413_22F7
        /// </summary>
        /// <param name="srcX"></param>
        /// <param name="srcY"></param>
        /// <param name="srcZ"></param>
        /// <param name="dstX"></param>
        /// <param name="dstY"></param>
        /// <param name="dstZ"></param>
        /// <returns></returns>
        public static bool TestBetweenPoints(int srcX, int srcY, int srcZ, int dstX, int dstY, int dstZ)
        {
            var di_xdiff = dstX - srcX;
            var si_ydiff = dstY - srcY;
            if ((di_xdiff == 0) && (si_ydiff == 0))
            {
                return true;
            }

            bool var17;
            int var16;
            int var15;
            int var14_axismodifier;
            int var13_axismodifier;
            int var12;
            int var11;

            bool var10_axisIsX;
            //int varE; //in uw this was a ptr to either srcX or srcY. this needs to be worked around
            bool varE_axisIsX; // true if VarE points to srcX, false to srcY



            int X_var3 = srcX >> 3;
            int X_var8 = srcX >> 3;
            int X_varA = srcX >> 3;

            int Y_var4 = srcY >> 3;
            int Y_var9 = srcY >> 3;
            int Y_varB = srcY >> 3;

            int var2_zdiff = dstZ - srcZ;

            int Xvar6 = dstX >> 3;
            int Yvar7 = dstY >> 3;
            int Height_var5 = dstZ >> 3;

            int srcXpos = srcX & 0x7;
            int srcYpos = srcY & 0x7;

            //seg006_1413_235E
            if (di_xdiff >= si_ydiff)
            {
                //seg006_1413_2365:
                if (-si_ydiff <= di_xdiff)
                {
                    //seg006_1413_2370
                    //varE = srcX;
                    varE_axisIsX = true;
                    var11 = di_xdiff >> 3;
                    //var10 = srcY;
                    var10_axisIsX = false;
                    var13_axismodifier = 1;
                    if (si_ydiff <= 0)
                    {
                        var14_axismodifier = -1;
                    }
                    else
                    {
                        var14_axismodifier = 1;
                    }
                    //seg006_1413_2392:
                    if (var14_axismodifier == 1)
                    {
                        //seg006_1413_23A5:
                        //seg006_1413_25F5:
                        //seg006_1413_25FE:
                        var15 = ((si_ydiff << 7) / di_xdiff) & 0xFF;
                        var16 = (srcYpos << 4) + ((var15 * (7 - srcXpos)) / 8);
                    }
                    else
                    {
                        //seg006_1413_23C7
                        var15 = ((-si_ydiff << 7) / di_xdiff) & 0xFF;
                        var16 = ((7 - srcYpos) << 4) + (var15 * (7 - srcXpos) / 8);
                    }
                }
                else
                {
                    //seg006_1413_240D
                    //varE = srcY;
                    varE_axisIsX = false;
                    var11 = -si_ydiff >> 3;
                    //var10 = srcX;
                    var10_axisIsX = true;
                    var13_axismodifier = -1;
                    if (di_xdiff <= 0)
                    {
                        //seg006_1413_242F:
                        var14_axismodifier = -1;
                    }
                    else
                    {
                        var14_axismodifier = 1;
                    }
                    //seg006_1413_2431:
                    if (var14_axismodifier != 1)
                    {
                        //seg006_1413_247B
                        var15 = ((-di_xdiff << 7) / si_ydiff) & 0xFF;
                        var16 = (srcXpos << 4) + ((var15 * srcYpos) / 8);
                    }
                    else
                    {
                        //seg006_1413:243A
                        var15 = ((di_xdiff << 7) / -si_ydiff) & 0xFF;
                        var16 = ((7 - srcXpos) << 4) + ((var15 * srcYpos) / 8);
                    }
                }
            }
            else
            {//when di<si
                if (-si_ydiff <= di_xdiff)
                {//seg006_1413_24BA
                    //varE = srcY;
                    varE_axisIsX = false; //changes y-axis
                    var11 = si_ydiff >> 3;
                    //var10 = srcX;
                    var10_axisIsX = true;
                    var13_axismodifier = 1;
                    if (di_xdiff <= 0)
                    {
                        //seg006_1413_251A
                        var14_axismodifier = -1;
                    }
                    else
                    {
                        var14_axismodifier = 1;
                    }
                    //seg006_1413_24DF:
                    if (var14_axismodifier != 1)
                    {
                        //seg006_1413_251A
                        var15 = ((-di_xdiff << 7) / si_ydiff) & 0xFF;
                        var16 = ((7 - srcXpos) << 4) + ((var15 * (7 - srcYpos)) / 8);
                    }
                    else
                    {
                        //seg006_1413:24E5
                        var15 = ((di_xdiff << 7) / si_ydiff) & 0xFF;
                        var16 = (srcXpos << 4) + ((var15) * (7 - srcYpos) / 8);
                    }

                }
                else
                {//seg006_1413_2560
                    //varE = srcX;
                    varE_axisIsX = true;
                    var11 = -di_xdiff >> 3;
                    //var10 = srcY;
                    var10_axisIsX = false;
                    var13_axismodifier = -1;
                    if (si_ydiff <= 0)
                    {
                        var14_axismodifier = -1;
                    }
                    else
                    {
                        var14_axismodifier = 1;
                    }
                    //seg006_1413_2584
                    if (var14_axismodifier != 1)
                    {
                        //seg006_1413_25CD
                        var15 = ((-si_ydiff << 7) / -di_xdiff) & 0xFF;
                        var16 = (srcYpos << 4) + ((srcXpos * var15) / 8);
                    }
                    else
                    {
                        var15 = ((si_ydiff << 7) / di_xdiff) & 0xFF;
                        var16 = ((7 - srcYpos) << 4) + ((var15 * srcXpos) / 8);
                    }
                }
            }

            //seg006_1413_2609:
            Height_var5 = srcZ;
            var12 = 0;

        Loop_seg006_1413_2613:

            if ((var16 & 0x80) != 0)
            { //seg006_1413_261C
                var16 = var16 & 0x7f;
                if (var10_axisIsX)
                {
                    X_varA += var14_axismodifier;
                }
                else
                {
                    Y_varB += var14_axismodifier;
                }

                var17 = TestBasicTileTraversal(
                    arg0: X_var8, arg2: Y_var9,
                    Tile1_X_arg4: X_var3, Tile1_Y_arg6: Y_var4,
                    Tile2_X_arg8: X_varA, Tile2_Y_argA: Y_varB,
                    Height_argC: Height_var5);

                if (var17)
                {//seg006_1413_265D:
                    if ((X_varA == Xvar6) && (Y_varB == Yvar7))
                    {
                        var17 = TestBasicTileTraversal(
                            arg0: X_var3, arg2: Y_var4,
                            Tile1_X_arg4: X_varA, Tile1_Y_arg6: Y_varB,
                            Tile2_X_arg8: 0, Tile2_Y_argA: 0,
                            Height_argC: Height_var5);
                        return var17;
                    }
                    else
                    {
                        X_var8 = X_var3;
                        Y_var9 = Y_var4;
                        X_var3 = X_varA;
                        Y_var4 = Y_varB;
                    }
                }
                else
                {
                    return false;
                }
            }

            //seg006_1413_26AA
            if (varE_axisIsX)
            {
                X_varA += var13_axismodifier;
            }
            else
            {
                Y_varB += var13_axismodifier;
            }
            var12++;
            if (var12 <= 0xA)
            {
                //seg006_1413_26C0:
                if (var11 != 0)
                {
                    Height_var5 = srcZ + ((var12 * var2_zdiff) / var11);
                }

                var17 = TestBasicTileTraversal(
                    arg0: X_var8, arg2: Y_var9,
                    Tile1_X_arg4: X_var3, Tile1_Y_arg6: Y_var4,
                    Tile2_X_arg8: X_varA, Tile2_Y_argA: Y_varB,
                    Height_argC: Height_var5);
                if (var17)
                {
                    //seg006_1413_2710:
                    if ((X_varA == Xvar6) && (Y_varB == Yvar7))
                    {
                        //seg006_1413_2728:
                        var17 = TestBasicTileTraversal(
                            arg0: X_var3, arg2: Y_var4,
                            Tile1_X_arg4: X_varA, Tile1_Y_arg6: Y_varB,
                            Tile2_X_arg8: 0, Tile2_Y_argA: 0,
                            Height_argC: Height_var5);
                        return var17;
                    }
                    else
                    {
                        //seg006_1413_2744:
                        X_var8 = X_var3;
                        Y_var9 = Y_var4;
                        X_var3 = X_varA;
                        Y_var4 = Y_varB;
                        var16 += var15;
                        goto Loop_seg006_1413_2613; //loop around and try again
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Port of a function from disassembly. Tests if movement from tile1 and tile2 is allowed based on the walls of the tiles and their relative positioning
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="arg2"></param>
        /// <param name="Tile1_X_arg4"></param>
        /// <param name="Tile1_Y_arg6"></param>
        /// <param name="Tile2_X_arg8"></param>
        /// <param name="Tile2_Y_argA"></param>
        /// <param name="Height_argC"></param>
        /// <returns></returns>
        public static bool TestBasicTileTraversal(int arg0, int arg2, int Tile1_X_arg4, int Tile1_Y_arg6, int Tile2_X_arg8, int Tile2_Y_argA, int Height_argC)
        {
            var tile1 = UWTileMap.current_tilemap.Tiles[Tile1_X_arg4, Tile1_Y_arg6];
            var tile2 = UWTileMap.current_tilemap.Tiles[Tile2_X_arg8, Tile2_Y_argA];

            if ((arg0 == 0) || ((arg0 == Tile1_X_arg4) && (arg2 == Tile1_Y_arg6)))
            {
                if ((Tile2_X_arg8 <= Tile1_X_arg4) || ((tilewallflags[tile2.tileType] & 2) == 0))
                {
                    if ((Tile2_X_arg8 >= Tile1_X_arg4) || ((tilewallflags[tile2.tileType] & 4) == 0))
                    {
                        if ((Tile2_Y_argA <= Tile1_Y_arg6) || ((tilewallflags[tile2.tileType] & 8) == 0))
                        {
                            if ((Tile2_Y_argA >= Tile1_Y_arg6) || ((tilewallflags[tile2.tileType] & 0x10) == 0))
                            {
                                if ((Tile2_X_arg8 <= Tile1_X_arg4) || ((tilewallflags[tile1.tileType] & 4) == 0))
                                {
                                    if ((Tile2_X_arg8 >= Tile1_X_arg4) || ((tilewallflags[tile1.tileType] & 2) == 0))
                                    {
                                        if ((Tile2_Y_argA <= Tile1_Y_arg6) || ((tilewallflags[tile1.tileType] & 0x10) == 0))
                                        {
                                            if ((Tile2_Y_argA >= Tile1_Y_arg6) || ((tilewallflags[tile1.tileType] & 8) == 0))
                                            {
                                                return (Height_argC >> 3) >= tile2.floorHeight;
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {//otherside
                if (Tile2_X_arg8 == 0)
                {
                    return true;
                }
                else
                {
                    if ((Tile2_X_arg8 <= Tile1_X_arg4) || ((tilewallflags[tile2.tileType] & 2) == 0))
                    {
                        if ((Tile2_X_arg8 >= Tile1_X_arg4) || ((tilewallflags[tile2.tileType] & 4) == 0))
                        {
                            if ((Tile2_Y_argA <= Tile1_Y_arg6) || ((tilewallflags[tile2.tileType] & 8) == 0))
                            {
                                if ((Tile2_Y_argA >= Tile1_Y_arg6) || ((tilewallflags[tile2.tileType] & 0x10) == 0))
                                {
                                    if ((Tile2_X_arg8 <= Tile1_X_arg4) || ((tilewallflags[tile1.tileType] & 2) == 0))
                                    {
                                        if ((Tile2_X_arg8 >= Tile1_X_arg4) || ((tilewallflags[tile1.tileType] & 4) == 0))
                                        {
                                            if ((Tile2_Y_argA <= Tile1_Y_arg6) || ((tilewallflags[tile1.tileType] & 8) == 0))
                                            {
                                                if ((Tile2_Y_argA >= Tile1_Y_arg6) || ((tilewallflags[tile1.tileType] & 0x10) == 0))
                                                {
                                                    return (Height_argC >> 3) >= tile2.floorHeight;
                                                }
                                                else
                                                {
                                                    return false;
                                                }
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }


        /// <summary>
        /// Converts x and y vectors into a 0-7 heading value.
        /// </summary>
        /// <param name="xVector"></param>
        /// <param name="yVector"></param>
        /// <returns></returns>
        public static int GetVectorHeading(int xVector, int yVector)
        {
            var cl_x = xVector << 1;
            var bl_y = yVector << 1;

            if (yVector <= cl_x)
            {
                if (xVector <= -bl_y)
                {
                    if (yVector <= -cl_x)
                    {
                        return 4;
                    }
                    else
                    {
                        return 3;
                    }
                }
                else
                {
                    if (xVector <= bl_y)
                    {
                        return 1;
                    }
                    else
                    {
                        return 2;
                    }
                }
            }
            else
            {
                if (xVector <= -bl_y)
                {
                    if (xVector <= bl_y)
                    {
                        return 6;
                    }
                    else
                    {
                        return 5;
                    }
                }
                else
                {
                    if (yVector <= -cl_x)
                    {
                        return 7;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }


        public static int ConvertTilePointsToHeading(int srcX, int srcY, int dstX, int dstY)
        {
            return ConvertVectorToHeading(srcX - dstX, srcY - dstY);
        }


        /// <summary>
        /// Takes a relative vector between two tiles and calculates the ordinal heading between them.
        /// </summary>
        /// <param name="xVector"></param>
        /// <param name="yVector"></param>
        /// <returns></returns>
        public static int ConvertVectorToHeading(int xVector, int yVector)
        {
            if (Math.Abs(xVector) <= Math.Abs(yVector))
            {
                if (Math.Abs(xVector) <= Math.Abs(yVector) / 2)
                {
                    if (xVector >= 0)
                    {
                        if (yVector >= 0)
                        {
                            return 1;
                        }
                        else
                        {
                            return 3;
                        }
                    }
                    else
                    {
                        if (yVector <= 0)
                        {
                            return 5;
                        }
                        else
                        {
                            return 7;
                        }
                    }
                }
                else
                {
                    if (yVector <= 0)
                    {
                        return 4;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            else
            {
                if (xVector <= 0)
                {
                    return 6;
                }
                else
                {
                    return 2;
                }
            }
        }
    }//end class
}//end namespace