using System;
using System.Diagnostics;

namespace Underworld
{

    /// <summary>
    /// Functions related to pathfinding
    /// </summary>
    public class Pathfind : UWClass
    {
        public static int MaybeMaxTravelDistance_dseg_67d6_2272;
        public static int MaybePathIndexOrLength_dseg_67d6_225A;

        static int TraverseRelated_dseg_67d6_224B;


        static int[] tilewallflags = new int[] { 30, 0, 19, 21, 11, 13, 32, 32, 32, 32 };

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

            while (index_var1< MaybePathIndexOrLength_dseg_67d6_225A)
            {//seg006_1413_2A42
                var accumualted_var3 = 0;
                var var2 = 0;

                while (var2<4)
                {
                    //seg006_1413_2981
                    var Seg56Record = FinalPath56.finalpath[index_var1+var2];
                    var NextSeg56Record = Seg56Record.Next();
                    var ax = ((NextSeg56Record.X0 - Seg56Record.X0) * 3) - (NextSeg56Record.Y1 - Seg56Record.Y1);

                    accumualted_var3 += (GetTilePathingRelated_dseg_67d6_BA(ax) & 0x3) << (var2<<1);

                    //seg006_1413_2A14: 
                    var2++;
                }


                //seg006_1413_2A23:
                index_var1 += 4;
            }
            
            //seg006_1413_2A4E:
            index_var1 = 0;
            while (index_var1<MaybePathIndexOrLength_dseg_67d6_225A)
            {
                var var4 = 0;
                var var2 = 0;

                //seg006_1413_2A8B
                while (var2<8)
                {
                    //seg006_1413_2A5E:
                    var Seg56Record =FinalPath56.finalpath[index_var1+var2];
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

        /// <summary>
        /// Placeholder to look up a table using possibly negative values
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        static int  GetTilePathingRelated_dseg_67d6_BA(int index)
        {
            Debug.Print($"Todo TilePathingRelated_dseg_67d6_BA({index})");
            return 0;
        }

        public static bool PathFindBetweenTiles(int currTileX_arg0, int currTileY_arg2, int CurrFloorHeight_arg4, int TargetTileX_arg6, int TargetTileY_arg8, int TargetFloorHeight_argA, int LikelyRangeArgC)
        {
            var MaybePathPathLength_var1D = 0;
            MaybeMaxTravelDistance_dseg_67d6_2272 = LikelyRangeArgC;
            var NoOfStepsIndex_var13 = 0;
            var Counter_var14 = 0;
            var ProbablyPathDistance_var1E = 0;

            var varC = Path5859.Path5859_Records[0];
            var var12 = Path5859.Path5859_Records[0x80];

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

            if (currTileX_arg0 < TargetTileX_arg6)
            {
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
            else
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

            //For some reason I'm not storing the Y tile here. Byt that is what the code is doing?
            PathFindingData49.pathfindtiles[currTileX_arg0, currTileY_arg2].X0 = currTileX_arg0;
            PathFindingData49.pathfindtiles[currTileX_arg0, currTileY_arg2].Z2 = CurrFloorHeight_arg4;
            PathFindingData49.pathfindtiles[currTileX_arg0, currTileY_arg2].unk4 = 0;

            var si = 0;
            while (si < 4)
            {
                var NeighbourTileX = currTileX_arg0 + PathXOffsetTable[si];
                var NeighbourTileY = currTileY_arg2 + PathYOffsetTable[si];
                if (UWTileMap.ValidTile(NeighbourTileX, NeighbourTileY))
                {
                    var NeighbourPathTile = PathFindingData49.pathfindtiles[NeighbourTileX, NeighbourTileY];
                    ProbablyPathDistance_var1E = 0;
                    var tmp = NeighbourPathTile.Z2;//out value from travers
                    //seg006_1413_1BC8:
                    var res = TraverseMultipleTiles(
                        tile1_X_arg0: 0, tile1_Y_arg2: 0,
                        tile2_X_arg4: currTileX_arg0, tile2_Y_arg6: currTileY_arg2,
                        tile3_X_arg8: NeighbourTileX, tile3_Y_argA: NeighbourTileY,
                        argC: npc.SpecialMotionHandler[4], argE: npc.SpecialMotionHandler[6],
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
                }
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
                    while (si < 4)
                    {
                        //seg006_1413_1CE1:
                        var NeighbourTileX = Tile2X_var1 + PathXOffsetTable[si];
                        var NeighbourTileY = Tile2Y_var2 + PathYOffsetTable[si];
                        if (
                                (NeighbourTileX >= X1_var6)
                                &&
                                (NeighbourTileX <= X2_var_7)
                                &&
                                (NeighbourTileY >= Y1_var8)
                                &&
                                (NeighbourTileY >= Y2_var_9)
                        )
                        {
                            //seg006_1413_1D23: 
                            var NeighbourPathTile = PathFindingData49.pathfindtiles[NeighbourTileX, NeighbourTileY];
                            ProbablyPathDistance_var1E = TileRecord_var1C.unk3_bit1_7;
                            if ((NeighbourTileX != TileRecord_var1C.X0) || (NeighbourTileY != TileRecord_var1C.Y1))
                            {
                                var Height_var1F = 0;
                                //seg006_1413_1D68:
                                var res_var20 = TraverseMultipleTiles(
                                    tile1_X_arg0: TileRecord_var1C.X0, tile1_Y_arg2: TileRecord_var1C.Y1, 
                                    tile2_X_arg4: Tile2X_var1, tile2_Y_arg6: Tile2Y_var2, 
                                    tile3_X_arg8: NeighbourTileX, tile3_Y_argA: NeighbourTileY, 
                                    argC: npc.SpecialMotionHandler[4], argE: npc.SpecialMotionHandler[6], 
                                    ProbablyHeight_arg10: TileRecord_var1C.Z2, arg12: ref Height_var1F, 
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
                                        if (TileRecord_var1C.unk4< NeighbourPathTile.unk4)
                                        {
                                            if(Math.Abs(Height_var1F - TargetFloorHeight_argA) < Math.Abs(NeighbourPathTile.Z2-TargetFloorHeight_argA))
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
                                        TileRecord_var1C.unk3_bit0 = TraverseRelated_dseg_67d6_224B & 0x1;

                                        if (!var21)
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
                                                tile1_X_arg0: Tile2X_var1, tile1_Y_arg2: Tile2Y_var2, 
                                                tile2_X_arg4: NeighbourTileX, tile2_Y_arg6: NeighbourTileY, 
                                                tile3_X_arg8: 0, tile3_Y_argA: 0, 
                                                argC: npc.SpecialMotionHandler[4], argE: npc.SpecialMotionHandler[6], 
                                                ProbablyHeight_arg10: NeighbourPathTile.Z2, 
                                                arg12: ref tmp, PathDistanceResult_arg16: ref ProbablyPathDistance_var1E);
                                            NeighbourPathTile.Z2 = tmp;

                                            if (res)
                                            {
                                                //a valid path has been found. save it and return true
                                                StorePath(
                                                    arg0: MaybePathPathLength_var1D, 
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
                    varC = Path5859.Path5859_Records[0x80];
                    var12 = Path5859.Path5859_Records[0];
                }
                else
                {
                    //seg006_1413_1F30:
                    varC = Path5859.Path5859_Records[0];
                    var12 = Path5859.Path5859_Records[0x80];
                }

                //seg006_1413_1F44:
                NoOfStepsIndex_var13 = Counter_var14;
                MaybePathPathLength_var1D++;
            }

            return false;//while loop at 1c83 got nothing.
        }


        public static bool TraverseMultipleTiles(int tile1_X_arg0, int tile1_Y_arg2, int tile2_X_arg4, int tile2_Y_arg6, int tile3_X_arg8, int tile3_Y_argA, int argC, int argE, int ProbablyHeight_arg10, ref int arg12, ref int PathDistanceResult_arg16)
        {

            return false;
        }

        static void StorePath(int arg0, int targetXarg2, int targetYArg4)
        {

        }


        public static int seg006_1413_205B(int CurrTileX, int CurrTileY, int targetX, int targetY)
        {
            var var8 = 0x40;
            var xVector_var1 = targetX - CurrTileX;
            var yVector_var2 = targetY - CurrTileY;
            var xVar3 = CurrTileX;
            var yVar4 = CurrTileY;
            var var6 = 0;
            bool UseVar3_si = false;//reference var3 of var4 in later calcs.
            bool UseVar3_di = false;//reference var3 of var4 in later calcs.
            var divisionVar7 = 0;
            var directionVar5 = 0;


            var TileVarC = UWTileMap.current_tilemap.Tiles[CurrTileX, CurrTileY];
            Pathfind.MaybeMaxTravelDistance_dseg_67d6_2272 = 0;

            if ((xVector_var1 == 0) && (yVector_var2 == 0))
            {
                return -1;
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
                        UseVar3_di = false;
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
                        UseVar3_di = true;
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
                        UseVar3_di = false;
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
                        UseVar3_di = true;
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


                //seg006_1413_21C0:
                FinalPath56.finalpath[0].X0 = CurrTileX;
                FinalPath56.finalpath[0].Y1 = CurrTileY;
                Pathfind.MaybePathIndexOrLength_dseg_67d6_225A = 1;
                FinalPath56.finalpath[0].Z2 = TileVarC.floorHeight;

                do
                {
                    if (UseVar3_di)
                    {
                        xVar3 += directionVar5;
                    }
                    else
                    {
                        yVar4 += directionVar5;
                    }

                    if (Pathfind.seg006_1413_2769(xVar3, yVar4))
                    {
                        //seg006_1413_2214    
                        var8 += divisionVar7;
                        if ((var8 & 0x80) != 0)
                        {
                            if (UseVar3_si)
                            {
                                xVar3 += var6;
                            }
                            else
                            {
                                yVar4 += var6;
                            }
                            if (Pathfind.seg006_1413_2769(xVar3, yVar4)==false)
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
                var tmp = Step2.Z2; //this may be wrong.
                var res = Pathfind.TraverseMultipleTiles(
                    tile1_X_arg0: Step3.X0, tile1_Y_arg2: Step3.Y1, 
                    tile2_X_arg4: Step2.X0, tile2_Y_arg6: Step2.Y1, 
                    tile3_X_arg8: 0, tile3_Y_argA: 0, 
                    argC: npc.SpecialMotionHandler[4], argE: npc.SpecialMotionHandler[6], 
                    ProbablyHeight_arg10: Step3.Z2, 
                    arg12: ref xVar3, 
                    PathDistanceResult_arg16: ref varE);
                Step2.Z2 = tmp;
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


        static bool seg006_1413_2769(int x, int y)
        {
            //TODO
            return false;
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
            //int var10;
            bool var10_axis;
            //int varE; //in uw this was a ptr to either srcX or srcY. this needs to be worked around
            bool varE_axis; // true if point to srcX, false to srcY
            int varB = srcY >> 3;
            int varA = srcX >> 3;
            int var9 = srcY >> 3;
            int var8 = srcX >> 3;
            int var7 = dstY >> 3;
            int var6 = dstX >> 3;
            int var5 = dstZ >> 3;
            int var4 = srcY >> 3;
            int var3 = srcX >> 3;
            int var2_zdiff = dstZ - srcZ;

            if (di_xdiff >= si_ydiff)
            {
                if (-si_ydiff <= di_xdiff)
                {//seg006_1413_2370
                    //varE = srcX;
                    varE_axis = true;
                    var11 = di_xdiff >> 3;
                    //var10 = srcY;
                    var10_axis = false;
                    var13_axismodifier = 1;
                    if (si_ydiff <= 0)
                    {
                        var14_axismodifier = -1;
                        var15 = (-si_ydiff << 7) / di_xdiff;
                        var ax = var15;
                        var bx = 7 - (srcX & 7);
                        ax = (ax * bx) / 8;
                        var16 = ((7 - (srcY & 7)) << 4) + ax;
                    }
                    else
                    {
                        var14_axismodifier = 1;
                        var15 = (si_ydiff << 7) / di_xdiff;
                        var ax = var15;
                        var bx = 7 - (srcX & 7);
                        ax = (ax * bx) / 8;
                        var16 = ((srcY & 7) << 4) + ax;
                    }
                }
                else
                {//seg006_1413_240D
                    //varE = srcY;
                    varE_axis = false;
                    var11 = -si_ydiff >> 3;
                    //var10 = srcX;
                    var10_axis = true;
                    var13_axismodifier = -1;
                    if (di_xdiff <= 0)
                    {
                        var14_axismodifier = -1;
                        //seg006_1413_247B:
                        var15 = (-di_xdiff << 7) / -si_ydiff;
                        var ax = var15;
                        var dx = (srcY & 7);
                        ax = (ax * dx) / 8;
                        var16 = ((srcX & 7) << 4) + ax;
                    }
                    else
                    {
                        var14_axismodifier = 1;
                        var15 = (di_xdiff << 7) / -si_ydiff;
                        var ax = var15;
                        var dx = (srcY & 7);
                        ax = (ax * dx) / 8;
                        dx = srcX & 7;
                        var bx = 7 - dx;
                        var16 = (bx << 4) + ax;
                    }
                }
            }
            else
            {//when di<si
                if (-si_ydiff <= di_xdiff)
                {//seg006_1413_24BA
                    //varE = srcY;
                    varE_axis = false; //changes y-axis
                    var11 = si_ydiff >> 3;
                    //var10 = srcX;
                    var10_axis = true;
                    var13_axismodifier = 1;
                    if (di_xdiff <= 0)
                    {//seg006_1413_251A
                        var14_axismodifier = -1;
                        var15 = (-di_xdiff << 7) / si_ydiff;
                        var ax = var15;
                        var dx = srcY & 7;
                        var bx = 7 - dx;
                        ax = (ax * bx) / 8;
                        dx = srcX & 7;
                        bx = 7 - dx;
                        var16 = (bx << 4) + ax;
                    }
                    else
                    {
                        var14_axismodifier = 1;
                        var15 = (di_xdiff << 7) / si_ydiff;
                        var ax = var15;
                        var dx = srcY & 7;
                        var bx = 7 - dx;
                        ax = (ax * bx) / 8;
                        dx = srcX & 7;
                        var16 = (dx << 4) + ax;
                    }
                }
                else
                {//seg006_1413_2560
                    //varE = srcX;
                    varE_axis = true;
                    var11 = -di_xdiff >> 3;
                    //var10 = srcY;
                    var10_axis = false;
                    var13_axismodifier = -1;
                    if (si_ydiff <= 0)
                    {
                        var14_axismodifier = -1;
                        var15 = (-si_ydiff << 7) / -di_xdiff;
                        var ax = var15;
                        var dx = srcX & 7;
                        ax = (ax * dx) / 8;
                        var16 = ((srcY & 7) << 4) + ax;
                    }
                    else
                    {
                        var14_axismodifier = 1;
                        var15 = (si_ydiff << 7) / -di_xdiff;
                        var ax = var15;
                        var dx = srcX & 7;
                        ax = (ax * dx) / 8;
                        dx = srcY & 7;
                        var bx = 7 - dx;
                        var16 = (bx << 4) + ax;
                    }
                }
            }

            //seg006_1413_2609:

            var5 = srcZ;
            var12 = 0;

        Loop_seg006_1413_2613:

            if ((var16 & 0x80) != 0)
            { //seg006_1413_261C
                var16 = var16 & 0x7f;
                if (var10_axis)
                {
                    varB += var14_axismodifier;
                }
                else
                {
                    varA += var14_axismodifier;
                }

                var17 = TestBasicTileTraversal(
                    arg0: var8, arg2: var9,
                    Tile1_X_arg4: var3, Tile1_Y_arg6: var4,
                    Tile2_X_arg8: varA, Tile2_Y_argA: varB,
                    Height_argC: var5);

                if (var17)
                {//seg006_1413_265D:
                    if ((varA == var6) && (varB == var7))
                    {
                        var17 = TestBasicTileTraversal(
                            arg0: var3, arg2: var4,
                            Tile1_X_arg4: varA, Tile1_Y_arg6: varB,
                            Tile2_X_arg8: 0, Tile2_Y_argA: 0,
                            Height_argC: var5);
                        return var17;
                    }
                    else
                    {
                        var8 = var3;
                        var9 = var4;
                        var3 = varA;
                        var4 = varB;
                    }
                }
                else
                {
                    return false;
                }
            }

            //seg006_1413_26AA
            if (varE_axis)
            {
                varA += var13_axismodifier;
            }
            else
            {
                varB += var13_axismodifier;
            }
            var12++;
            if (var12 <= 0xA)
            {
                if (var11 != 0)
                {
                    var5 = srcZ + (var12 * var2_zdiff) / var11;
                }

                var17 = TestBasicTileTraversal(
                    arg0: var8, arg2: var9,
                    Tile1_X_arg4: var3, Tile1_Y_arg6: var4,
                    Tile2_X_arg8: varA, Tile2_Y_argA: varB,
                    Height_argC: var5);
                if (var17)
                {
                    if ((varA == var6) && (varB == var7))
                    {
                        var17 = TestBasicTileTraversal(
                            arg0: var3, arg2: var4,
                            Tile1_X_arg4: varA, Tile1_Y_arg6: varB,
                            Tile2_X_arg8: 0, Tile2_Y_argA: 0,
                            Height_argC: var5);
                        return var17;
                    }
                    else
                    {//seg006_1413_2744:
                        var8 = var3;
                        var9 = var4;
                        var3 = varA;
                        var4 = varB;
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
            var bl_y = yVector << 2;

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