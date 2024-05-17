using Godot;

namespace Underworld
{
    /// <summary>
    /// Functions related to thievery and illegal actions.
    /// </summary>
    public class pathfind : UWClass
    {

        static int[] tilewallflags = new int[] { 30, 0, 19, 21, 11, 13, 32, 32, 32, 32 };



        /// <summary>
        /// Looks like this tests if two points on the map are pathable? to each other.
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
            var di_xdiff = dstX-srcX;
            var si_ydiff = dstY-srcY;
            if ((di_xdiff == 0) && (si_ydiff == 0))
            {
                return true;
            }

            bool var17;
            int var16;
            int var15;
            int var14;
            int var13;
            int var12;
            int var11;
            //int var10;
            bool var10_axis;
            //int varE; //in uw this was a ptr to either srcX or srcY. this needs to be worked around
            bool varE_axis; // true if point to srcX, false to srcY
            int varB = srcY>>3;
            int varA = srcX>>3;
            int var9 = srcY>>3;
            int var8 = srcX>>3;
            int var7 = dstY>>3;
            int var6 = dstX>>3;
            int var5 = dstZ>>3;
            int var4 = srcY>>3;
            int var3 = srcX>>3;
            int var2 = srcZ;

            if (di_xdiff>=si_ydiff)
            {
                if (-si_ydiff<=di_xdiff)
                {//seg006_1413_2370
                    //varE = srcX;
                    varE_axis = true;
                    var11 = di_xdiff>>3;
                    //var10 = srcY;
                    var10_axis = false;
                    var13 = 1;
                    if (si_ydiff<=0)
                    {
                        var14 = -1;
                        var15 = (-si_ydiff<<7)/di_xdiff;
                        var ax = var15;
                        var bx = 7 - (srcX & 7);
                        ax = (ax * bx) / 8;
                        var16 = ((7-(srcY & 7))<<4) + ax;
                    }
                    else
                    {
                        var14 = 1;
                        var15 = (si_ydiff<<7)/di_xdiff;
                        var ax = var15;
                        var bx = 7 - (srcX & 7);
                        ax = (ax * bx) / 8;
                        var16 = ((srcY & 7)<<4) + ax;
                    }
                }
                else
                {//seg006_1413_240D
                    //varE = srcY;
                    varE_axis = false;
                    var11 = -si_ydiff>>3;
                    //var10 = srcX;
                    var10_axis = true;
                    var13 = -1;
                    if(di_xdiff<=0)
                    {
                        var14 = -1;
                        //seg006_1413_247B:
                        var15 = (-di_xdiff<<7) / -si_ydiff;
                        var ax = var15;
                        var dx = (srcY & 7);
                        ax = (ax * dx) / 8;
                        var16 = ((srcX & 7) << 4) + ax;
                    }
                    else
                    {
                        var14 = 1;  
                        var15 = (di_xdiff<<7) / -si_ydiff;
                        var ax = var15;     
                        var dx = (srcY & 7);  
                        ax = (ax * dx) / 8;
                        dx = srcX & 7;
                        var bx = 7 - dx;
                        var16 = (bx<<4) + ax;          
                    }
                }                
            }
            else
            {//when di<si
                if (-si_ydiff <= di_xdiff)
                {//seg006_1413_24BA
                    //varE = srcY;
                    varE_axis = false;
                    var11 = si_ydiff >> 3;
                    //var10 = srcX;
                    var10_axis = true;
                    var13 = 1;
                    if (di_xdiff<=0)
                    {//seg006_1413_251A
                        var14 = -1;
                        var15 = (-di_xdiff<<7)/si_ydiff;
                        var ax = var15;
                        var dx = srcY & 7;
                        var bx = 7 - dx;
                        ax = (ax * bx)/8;
                        dx = srcX & 7;
                        bx = 7 - dx;
                        var16 = (bx<<4) + ax;
                    }
                    else
                    {
                        var14 = 1;
                        var15 = (di_xdiff<<7)/si_ydiff;
                        var ax = var15;
                        var dx = srcY & 7;
                        var bx = 7 - dx;
                        ax = (ax * bx)/8;
                        dx = srcX & 7;
                        var16 = (dx<<4)+ax;
                    }
                }
                else
                {//seg006_1413_2560
                    //varE = srcX;
                    varE_axis = true;
                    var11 = -di_xdiff >> 3;
                    //var10 = srcY;
                    var10_axis = false;
                    var13 = -1;
                    if (si_ydiff<=0)
                    {
                        var14 = -1;
                        var15 = (-si_ydiff<<7)/-di_xdiff;
                        var ax = var15;
                        var dx = srcX & 7;
                        ax = (ax * dx) / 8;
                        var16 = ((srcY & 7)<<4) + ax;
                    }
                    else
                    {
                        var14 = 1;
                        var15 = (si_ydiff<<7)/-di_xdiff;
                        var ax = var15;
                        var dx = srcX & 7;
                        ax = (ax * dx) / 8;
                        dx = srcY & 7;
                        var bx = 7 - dx;
                        var16 = (bx<<4) + ax;
                    }
                }
            }

            //seg006_1413_2609:
            
            var5 = srcZ;
            var12 = 0;     

            Loop_seg006_1413_2613:

            if ((var16 & 0x80) !=0 )
            { //seg006_1413_261C
                var16 = var16 & 0x7f;
                if (var10_axis)
                {//srcX
                    srcX += var14;
                }
                else
                {//srcY
                    srcY += var14;
                }

                var17 = TestTileTraversal(var8,var9,var4,var4,varA,varB,var5);
                if (var17)
                {//seg006_1413_265D:
                    if ((srcX==dstX) && (srcX==dstY))
                    {
                        var17 = TestTileTraversal(var3,var4,varA,varB,0,0,var5);
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
                srcX += var13;
            }
            else
            {
                srcY += var13;
            }
            var12++;
            if (var12<=0xA)
            {
                if (var11!=0)
                {
                    var5 = (var12*var2)/var11;
                }

                var17 = TestTileTraversal(var8,var9,var3,var4,varA,varB,var5);
                if (var17)
                {
                    if ((srcX==dstX)&&(srcY==dstY))
                    {
                        var17 = TestTileTraversal(var3,var4,varA,varB,0,0,var5);
                        return var17;
                    }
                    else
                    {
                        var8 = var3;
                        var9 = var4;
                        var3 = srcX;
                        var4 = srcY;
                        var16 = var15;
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
        public static bool TestTileTraversal(int arg0, int arg2, int Tile1_X_arg4, int Tile1_Y_arg6, int Tile2_X_arg8, int Tile2_Y_argA, int Height_argC)
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
                                                    return (tile2.floorHeight>>3)>=Height_argC;
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
    }//end class
}//end namespace