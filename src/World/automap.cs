using System.Diagnostics;

namespace Underworld
{
    public class automap : Loader
    {

        public static int currentautomap;
        public static int currentworld = 0;

        /// <summary>
        /// Array of all cached automaps
        /// </summary>
        public static automap[] automaps;


        //The raw data for this automap.
        public byte[] buffer;

        public automaptileinfo[,] tiles = new automaptileinfo[64, 64];


        /// <summary>
        /// Initialises an automap for the specified level no and loads the automap data from the lev.ark file
        /// </summary>
        /// <param name="LevelNo"></param>
        public automap(int LevelNo, int gameNo)
        {
            //load buffer. then init tiles with their offsets
            int blockno;
            if (gameNo == GAME_UW2) //this is weird. I had to pass gameno as a parm or otherwise this if-else would not work??
            {
                Debug.Print("UW2");
                blockno = 160 + LevelNo;
            }
            else
            {
                blockno = LevelNo + 27;
            }
            DataLoader.LoadUWBlock(LevArkLoader.lev_ark_file_data, blockno, 64 * 64, out UWBlock block);
            if (block.Data == null)
            { //init a blank map
                buffer = new byte[64 * 64];
            }
            else
            {
                buffer = block.Data;
            }

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    tiles[x, y] = new automaptileinfo((y * 64) + x, ref buffer);
                }
            }
        }

        public static void MarkTileVisited(int level, int tileX, int tileY, int tiletype, int displaytype = automaptileinfo.DisplayTypeClear)
        {
            automaps[level].tiles[tileX, tileY].tileType = (short)tiletype;
            automaps[level].tiles[tileX, tileY].DisplayType = (short)displaytype;
        }


        /// <summary>
        /// Checks if automapping is allowed in this map
        /// </summary>
        /// <param name="dungeon"></param>
        /// <returns></returns>
        public static bool CanMap(int dungeon)
        {
            if (_RES == GAME_UW2)
            {
                if (worlds.GetWorldNo(dungeon) == 8)
                {
                    return false;
                }
            }
            else
            {
                return dungeon != 9;
            }
            return true;
        }

        /// <summary>
        /// Marks a range of tiles around the center X,Y as visited.
        /// </summary>
        /// <param name="cX"></param>
        /// <param name="cY"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static int MarkRangeOfTilesVisited(int cX, int cY, int range, int dungeon_level)
        {

            var FillArea = FloodFillTile.FloodFillSubArea(cX, cY, range + 1);
            for (var aX = 0; aX <= FillArea.GetUpperBound(0); aX++)
            {
                for (var aY = 0; aY <= FillArea.GetUpperBound(1); aY++)
                {
                    bool literaledgecase =false;
                    if ((aY==0) || (aX == 0) || (aX== FillArea.GetUpperBound(0)) || (aY== FillArea.GetUpperBound(1)))
                    {
                        literaledgecase = true;
                    }
                    var x = FillArea[aX, aY].ActualX;
                    var y = FillArea[aX, aY].ActualY;
                    if ((FillArea[aX, aY].Tested) && (FillArea[aX, aY].Accessible) && (!literaledgecase))
                    {
                        //mark visited fileArea[aX,aY].ActualX/Y based on existing rules
                        MarkTileVisited(
                            level: dungeon_level - 1,
                            tileX: x, tileY: y,
                            tiletype: UWTileMap.current_tilemap.Tiles[x, y].tileType,
                            displaytype: automaptileinfo.GetDisplayType(UWTileMap.current_tilemap.Tiles[x, y]));
                    }
                    else
                    {
                        if (UWTileMap.ValidTile(x, y))
                        {//mark the open tiles outside of vision range as undiscovered open tile if not already visited
                            var tile = UWTileMap.current_tilemap.Tiles[x, y];
                            if (automaps[dungeon_level - 1].tiles[x, y].visited == false)
                            {
                                if (UWTileMap.IsOpen(UWTileMap.current_tilemap.Tiles[x, y].tileType))
                                {
                                    var displaytype = automaptileinfo.GetDisplayType(tile);
                                    //automap.automaps[dungeon_level - 1].tiles[northaxis, aY].DisplayType;
                                    //mark as undiscovered, open tile. 
                                    MarkTileVisited(
                                        level: dungeon_level - 1,
                                        tileX: x, tileY: y,
                                        tiletype: 11,
                                        displaytype: displaytype);
                                }
                            }
                        }
                    }
                }
            }
            return range;
        }

        // for (int aX = cX - range; aX <= cX + range; aX++)
        // {
        //     for (int aY = cY - range; aY <= cY + range; aY++)
        //     {
        //         if (UWTileMap.ValidTile(aX, aY))
        //         {//TODO figure out how to do line of sight checking of solid walls.
        //             automap.MarkTileVisited(
        //                     level: dungeon_level - 1,
        //                     tileX: aX, tileY: aY,
        //                     tiletype: UWTileMap.current_tilemap.Tiles[aX, aY].tileType,
        //                     displaytype: automaptileinfo.GetDisplayType(UWTileMap.current_tilemap.Tiles[aX, aY]));
        //         }
        //     }
        // }
        // range++;
        // //make outside range as unvisited open tiles if not already visited
        // //along the north
        // var northaxis = cX + range;
        // var southaxis = cX - range;
        // var westaxis = cY - range;
        // var eastaxis = cY + range;
        // for (int aY = cY - range; aY <= cY + range; aY++)
        // {
        //     if (UWTileMap.ValidTile(northaxis, aY))
        //     {
        //         var tile = UWTileMap.current_tilemap.Tiles[northaxis, aY];
        //         if (automap.automaps[dungeon_level - 1].tiles[northaxis, aY].visited == false)
        //         {
        //             if (UWTileMap.IsOpen(UWTileMap.current_tilemap.Tiles[northaxis, aY].tileType))
        //             {
        //                 var displaytype = automaptileinfo.GetDisplayType(tile);
        //                 //automap.automaps[dungeon_level - 1].tiles[northaxis, aY].DisplayType;
        //                 //mark as undiscovered, open tile. (there probably needs to be other rules for diagonals)
        //                 automap.MarkTileVisited(
        //                     level: dungeon_level - 1,
        //                     tileX: northaxis, tileY: aY,
        //                     tiletype: 11,
        //                     displaytype: displaytype);
        //             }
        //         }
        //     }
        //     if (UWTileMap.ValidTile(southaxis, aY))
        //     {
        //         var tile = UWTileMap.current_tilemap.Tiles[southaxis, aY];
        //         if (automap.automaps[dungeon_level - 1].tiles[southaxis, aY].visited == false)
        //         {
        //             if (UWTileMap.IsOpen(UWTileMap.current_tilemap.Tiles[southaxis, aY].tileType))
        //             {
        //                 var displaytype = automaptileinfo.GetDisplayType(tile);
        //                 automap.MarkTileVisited(
        //                     level: dungeon_level - 1,
        //                     tileX: southaxis, tileY: aY,
        //                     tiletype: 11,
        //                     displaytype: displaytype);
        //             }
        //         }
        //     }
        // }

        // for (int aX = cX - range; aX <= cX + range; aX++)
        // {
        //     if (UWTileMap.ValidTile(aX, westaxis))
        //     {
        //         var tile = UWTileMap.current_tilemap.Tiles[aX, westaxis];
        //         if (automap.automaps[dungeon_level - 1].tiles[aX, westaxis].visited == false)
        //         {
        //             if (UWTileMap.IsOpen(UWTileMap.current_tilemap.Tiles[aX, westaxis].tileType))
        //             {
        //                 var displaytype = automaptileinfo.GetDisplayType(tile);
        //                 automap.MarkTileVisited(
        //                     level: dungeon_level - 1,
        //                     tileX: aX, tileY: westaxis,
        //                     tiletype: 11,
        //                     displaytype: displaytype);
        //             }
        //         }
        //     }
        //     if (UWTileMap.ValidTile(aX, eastaxis))
        //     {
        //         var tile = UWTileMap.current_tilemap.Tiles[aX, eastaxis];
        //         if (automap.automaps[dungeon_level - 1].tiles[aX, eastaxis].visited == false)
        //         {
        //             if (UWTileMap.IsOpen(UWTileMap.current_tilemap.Tiles[aX, eastaxis].tileType))
        //             {
        //                 var displaytype = automaptileinfo.GetDisplayType(tile);
        //                 automap.MarkTileVisited(
        //                     level: dungeon_level - 1,
        //                     tileX: aX, tileY: eastaxis,
        //                     tiletype: 11,
        //                     displaytype: displaytype);
        //             }
        //         }
        //     }
        // }
        // return range;
        //}


    }//end class
}//end namespace
