using System;

namespace Underworld
{



    /// <summary>
    /// Functions related to pathfinding
    /// </summary>
    public class Pathfind : UWClass
    {
        static int MaybeMaxTravelDistance_dseg_67d6_2272;
        static bool MaybePathIndexOrLength_dseg_67d6_225A;

        static int TraverseRelated_dseg_67d6_224B;


        static int[] tilewallflags = new int[] { 30, 0, 19, 21, 11, 13, 32, 32, 32, 32 };

        static int[] PathXOffsetTable = new int[] { 0, 1, 0, -1 };
        static int[] PathYOffsetTable = new int[] { 1, 0, -1, 0 };

        /// <summary>
        /// Accumulates some values into Segment 57 from Seg56.
        /// </summary>
        /// <param name="Seg57Record"></param>
        public static void UpdateSeg57Values(PathFind57 Seg57Record)
        {
            //Seg006_1413_2934
            //does some path finding work
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
                        ProbablyHeight_arg12: CurrFloorHeight_arg4,
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
                            MaybePathIndexOrLength_dseg_67d6_225A = true;
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
                                    ProbablyHeight_arg12: TileRecord_var1C.Z2, arg12: ref Height_var1F, 
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
                                                ProbablyHeight_arg12: NeighbourPathTile.Z2, 
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


        static bool TraverseMultipleTiles(int tile1_X_arg0, int tile1_Y_arg2, int tile2_X_arg4, int tile2_Y_arg6, int tile3_X_arg8, int tile3_Y_argA, int argC, int argE, int ProbablyHeight_arg12, ref int arg12, ref int PathDistanceResult_arg16)
        {

            return false;
        }

        static void StorePath(int arg0, int targetXarg2, int targetYArg4)
        {

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