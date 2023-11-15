using Godot;
namespace Underworld
{
    public class AutomapRender : UWClass
    {
        const int TileSize = 4;

        const int NORTH = 0;
        const int SOUTH = 1;
        const int EAST = 2;
        const int WEST = 3;
        const int NORTHWEST = 4;
        const int NORTHEAST = 5;
        const int SOUTHWEST = 6;
        const int SOUTHEAST = 7;

        public static Color ClearColour = Color.Color8(255, 255, 255, 0);
        //Colours
        public static Color[] BackgroundColour = new Color[1];

        /// <summary>
        /// The open tile colour.
        /// </summary>
        /// RGB as follows
        /// 116,81,56
        /// 102,70,47
        /// 107,75,47
        public static Color[] OpenTileColour =
        {
            Color.Color8(116,81,56,255),
            Color.Color8(102,70,47,255),
            Color.Color8(107,75,47,255)
        };
        //new Color[3];

        /// <summary>
        /// The water tile colour.
        /// </summary>
        /// RGB as follows
        /// 62,61,134
        /// 50,51,115
        public static Color[] WaterTileColour = //new Color[2];
        {
            Color.Color8(62,61,134,255),
            Color.Color8(50,51,115,255)
        };
        /// <summary>
        /// The lava tile colour.
        /// </summary>
        /// RGB is 
        /// 115,23,27
        /// 78,15,14
        public static Color[] LavaTileColour = //new Color[2];
        {
            Color.Color8(115,23,27,255),
            Color.Color8(78,15,14,255)
        };

        /// <summary>
        /// The bridge tile colour.
        /// </summary>
        /// RGB is 
        /// 64,28,0
        /// 59,23,0
        /// 74,28,0
        public static Color[] BridgeTileColour = //new Color[3];
        {
            Color.Color8(64,28,0,255),
            Color.Color8(59,23,0,255),
            Color.Color8(74,28,0,255)
        };

        /// <summary>
        /// The stairs tile colour.
        /// </summary>
        /// RGB as follows
        /// 79,52,27
        /// 70,41,24
        public static Color[] StairsTileColour = //new Color[2];
        {
            Color.Color8(79,52,27,255),
            Color.Color8(70,41,24,255)
        };

        /// <summary>
        /// Wall border colour.
        /// </summary>
        /// RBG is
        /// 66,41,22
        /// 93,60,37
        /// 98,65,42
        /// 88,56,33
        public static Color[] BorderColour = //new Color[4];
        {
            Color.Color8(66,41,22,255),
            Color.Color8(93,60,37,255),
            Color.Color8(98,65,42,255),
            Color.Color8(88,56,33,255)
        };

        /// <summary>
        /// The background colour of the map.
        /// </summary>
        public static Color[] Background = //new Color[1];
        {
            Color.Color8(255,255,255,0)
        };


        /// <summary>
        /// The icy tile colour.
        /// </summary>
        /// RGB as follows
        /// 48,144,182
        /// 25,182,252
        /// 24,116,167
        public static Color[] IceTileColour = //new Color[3];
        {
            Color.Color8(48,144,182,255),
            Color.Color8(25,182,252,255),
            Color.Color8(24,116,167,255)
        };

        static automap mapToRender;

        static System.Random rnd;
        public static ImageTexture MapImage(int levelno)
        {
            rnd = new System.Random();
            var OutputTileMapImage = Image.Create(64 * TileSize, 64 * TileSize, false, Image.Format.Rgba8);
            mapToRender = automap.automaps[levelno];
            //Init the tile map as a blank map first
            // for (int x = 0; x <= TileMap.TileMapSizeX; x++)
            // {
            //     for (int y = 0; y <= TileMap.TileMapSizeY; y++)
            //     {
            //         DrawSolidTile(OutputTileMapImage, mapToRender.tiles[x, y], x, y, BackgroundColour);
            //     }
            // }
            //Fills in the tile background colour first
            for (int x = 0; x <= TileMap.TileMapSizeX; x++)
            {
                for (int y = 0; y <= TileMap.TileMapSizeY; y++)
                {//If the tile has been visited and can be rendered.
                    if (mapToRender.tiles[x, y].visited)
                    {
                    FillTileColour(OutputTileMapImage, mapToRender.tiles[x, y], x, y);
                    }
                }
            }

            for (int x = 0; x <= TileMap.TileMapSizeX; x++)
            {
                for (int y = 0; y <= TileMap.TileMapSizeY; y++)
                {//If the tile has been visited and can be rendered.
                    if (mapToRender.tiles[x, y].visited)
                    {
                        DrawTileBorder(OutputTileMapImage, mapToRender.tiles[x, y], x, y);
                    }
                }
            }

            //Draw door ways
            for (int x = 1; x < TileMap.TileMapSizeX; x++)
            {
                for (int y = 1; y < TileMap.TileMapSizeY; y++)
                {
                    if (mapToRender.tiles[x, y].IsDoor)
                    {
                        if (mapToRender.tiles[x, y].visited)
                        {
                            DrawDoor(OutputTileMapImage, mapToRender.tiles[x, y], x, y,BorderColour);
                        }
                    }
                }
            }

            var tex = new ImageTexture();
            tex.SetImage(OutputTileMapImage);
            return tex;
        }

        /// <summary>
        /// Files a solid tile with no border with the input colour
        /// </summary>
        /// <param name="OutputTileMapImage"></param>
        /// <param name="tile"></param>
        /// <param name="TileX"></param>
        /// <param name="TileY"></param>
        /// <param name="InputColour"></param>
        private static void DrawSolidTile(Image OutputTileMapImage, automaptileinfo tile, int TileX, int TileY, Color[] InputColour)
        {
            for (int x = 0; x < TileSize; x++)
            {
                for (int y = 0; y < TileSize; y++)
                {
                    OutputTileMapImage.SetPixel(x + TileX * TileSize, y + TileY * TileSize, PickColour(InputColour));
                }
            }
        }



        private static void FillTileColour(Image OutputTileMapImage, automaptileinfo tile, int TileX, int TileY)
        {
            Color[] TileColorPrimary;
            Color TileColorSecondary;
            ///Picks which colour to use based on the tile properties.
            if (tile.IsBridge)
            {
                TileColorPrimary = BridgeTileColour;
                TileColorSecondary = ClearColour;
            }
            else if (tile.IsWater)
            {
                TileColorPrimary = WaterTileColour;
                TileColorSecondary = ClearColour;
            }
            else if (tile.IsLava)
            {
                TileColorPrimary = LavaTileColour;
                TileColorSecondary = ClearColour;
            }
            else if (tile.IsStair)
            {
                TileColorPrimary = StairsTileColour;
                TileColorSecondary = ClearColour;
            }
            else if (tile.IsIce)
            {
                TileColorPrimary = IceTileColour;
                TileColorSecondary = ClearColour;
            }
            else
            {
                TileColorPrimary = OpenTileColour;
                TileColorSecondary = ClearColour;
            }
            switch (tile.tileType)
            {
                case automaptileinfo.TILE_SOLID:
                    {
                        break;
                    }
                case automaptileinfo.TILE_DIAG_NE:
                    {//Fills diagonally.
                        for (int i = 0; i <= TileSize; i++)
                        {
                            for (int j = 0; j <= TileSize; j++)
                            {
                                if (i >= TileSize - j)
                                {
                                    OutputTileMapImage.SetPixel(i + TileX * TileSize, j + TileY * TileSize, PickColour(TileColorPrimary));
                                }
                                else
                                {
                                    OutputTileMapImage.SetPixel(i + TileX * TileSize, j + TileY * TileSize, TileColorSecondary);
                                }
                            }
                        }
                        break;
                    }
                case automaptileinfo.TILE_DIAG_NW:
                    {//Fills diagonally.
                        for (int i = 0; i <= TileSize; i++)
                        {
                            for (int j = 0; j <= TileSize; j++)
                            {
                                if (i <= j)
                                {
                                    OutputTileMapImage.SetPixel(i + TileX * TileSize, j + TileY * TileSize, PickColour(TileColorPrimary));
                                }
                                else
                                {
                                    OutputTileMapImage.SetPixel(i + TileX * TileSize, j + TileY * TileSize, TileColorSecondary);
                                }
                            }
                        }
                        break;
                    }
                case automaptileinfo.TILE_DIAG_SE:
                    {//Fills diagonally.	
                        for (int i = 0; i <= TileSize; i++)
                        {
                            for (int j = 0; j <= TileSize; j++)
                            {
                                if (i >= j)
                                {
                                    OutputTileMapImage.SetPixel(i + TileX * TileSize, j + TileY * TileSize, PickColour(TileColorPrimary));
                                }
                                else
                                {
                                    OutputTileMapImage.SetPixel(i + TileX * TileSize, j + TileY * TileSize, TileColorSecondary);
                                }
                            }
                        }
                        break;
                    }
                case automaptileinfo.TILE_DIAG_SW:
                    {//Fills diagonally.
                        for (int i = 0; i <= TileSize; i++)
                        {
                            for (int j = 0; j <= TileSize; j++)
                            {
                                if (TileSize - i >= j)
                                {
                                    OutputTileMapImage.SetPixel(i + TileX * TileSize, j + TileY * TileSize, PickColour(TileColorPrimary));
                                }
                                else
                                {
                                    OutputTileMapImage.SetPixel(i + TileX * TileSize, j + TileY * TileSize, TileColorSecondary);
                                }
                            }
                        }
                        break;
                    }
                case automaptileinfo.TILE_OPEN:
                case automaptileinfo.TILE_SLOPE_E:
                case automaptileinfo.TILE_SLOPE_N:
                case automaptileinfo.TILE_SLOPE_S:
                case automaptileinfo.TILE_SLOPE_W:
                    {//Fills an open tile.
                        DrawSolidTile(OutputTileMapImage, tile, TileX, TileY, TileColorPrimary);
                        break;
                    }
                default:
                    {//Does not draw anything.
                        DrawSolidTile(OutputTileMapImage, tile, TileX, TileY, BackgroundColour);
                        break;
                    }
            }
        }//end  fill colour

        private static void DrawTileBorder(Image OutputTileMapImage, automaptileinfo tile, int TileX, int TileY)
        {
            if (tile.visited)
            {
                switch (tile.tileType)
                {
                    case automaptileinfo.TILE_SOLID://Solid
                        {//no need to draw
                            break;
                        }
                    case automaptileinfo.TILE_OPEN://Open
                    case automaptileinfo.TILE_SLOPE_E:
                    case automaptileinfo.TILE_SLOPE_W:
                    case automaptileinfo.TILE_SLOPE_S:
                    case automaptileinfo.TILE_SLOPE_N:
                        {
                            DrawOpenTileBorder(OutputTileMapImage, tile, TileX, TileY, BorderColour);
                            break;
                        }
                    case automaptileinfo.TILE_DIAG_NE:
                        {
                            DrawDiagNE(OutputTileMapImage, tile, TileX, TileY, BorderColour);
                            break;
                        }
                    case automaptileinfo.TILE_DIAG_SE:
                        {
                            DrawDiagSE(OutputTileMapImage, tile, TileX, TileY, BorderColour);
                            break;
                        }
                    case automaptileinfo.TILE_DIAG_NW:
                        {
                            DrawDiagNW(OutputTileMapImage, tile, TileX, TileY, BorderColour);
                            break;
                        }
                    case automaptileinfo.TILE_DIAG_SW:
                        {
                            DrawDiagSW(OutputTileMapImage, tile, TileX, TileY, BorderColour);
                            break;
                        }
                    default:
                        {
                            //no need to draw
                            break;
                        }
                }
            }
            else
            {
                DrawSolidTile(OutputTileMapImage, tile, TileX, TileY, Background);
            }
        }

        private static void DrawOpenTileBorder(Image OutputTileMapImage, automaptileinfo tile, int TileX, int TileY, Color[] InputColour)
        {
            //Check the tile to the north
            if (TileY < TileMap.TileMapSizeY)
            {
                if (mapToRender.tiles[TileX, TileY + 1].IsSolidWall)
                {//Solid tile to the north.
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, NORTH);
                }
            }
            //Check the tile to the south
            if (TileY > 0)
            {
                if (mapToRender.tiles[TileX, TileY - 1].IsSolidWall)
                {//Solid tile to the south.
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, SOUTH);
                }
            }
            //Check the tile to the east
            if (TileX < TileMap.TileMapSizeX)
            {
                if (mapToRender.tiles[TileX + 1, TileY].IsSolidWall)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, EAST);
                }
            }
            //Check the tile to the west
            if (TileX > 0)
            {
                if (mapToRender.tiles[TileX - 1, TileY].IsSolidWall)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, WEST);
                }
            }
        }


        static void DrawDiagSW(Image OutputTileMapImage, automaptileinfo tile, int TileX, int TileY, Color[] InputColour)
        {
            DrawLine(OutputTileMapImage, TileX, TileY, InputColour, SOUTHWEST);

            //Check the tiles to the north and east of this tile to see what needs to be drawn for borders
            if (TileY < TileMap.TileMapSizeY)
            {//north
                var TileToTest = mapToRender.tiles[TileX, TileY + 1];
                if (TileToTest.IsOpen || TileToTest.tileType == automaptileinfo.TILE_DIAG_SW)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, NORTH);
                }
            }
            if (TileX < TileMap.TileMapSizeX)
            {//east
                var TileToTest = mapToRender.tiles[TileX + 1, TileY];
                if (TileToTest.IsOpen || TileToTest.tileType == automaptileinfo.TILE_DIAG_SW)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, EAST);
                }
            }

            //Check South and East for solids.
            if (TileY > 0)
            {//South
                if (mapToRender.tiles[TileX, TileY - 1].tileType == automaptileinfo.TILE_SOLID)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, SOUTH);
                }
            }

            if (TileX > 0)
            {//West
                if (mapToRender.tiles[TileX - 1, TileY].tileType == automaptileinfo.TILE_SOLID)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, WEST);
                }
            }
        }


        static void DrawDiagNE(Image OutputTileMapImage, automaptileinfo tile, int TileX, int TileY, Color[] InputColour)
        {
            DrawLine(OutputTileMapImage, TileX, TileY, InputColour, NORTHEAST);

            //Check the tiles to the south and west of this tile to see what needs to be drawn.
            if (TileY > 0)
            {//South
                var TileToTest = mapToRender.tiles[TileX, TileY - 1];
                if (TileToTest.IsOpen || TileToTest.tileType == automaptileinfo.TILE_DIAG_NE)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, SOUTH);
                }
            }
            if (TileX > 0)
            {//West
                var TileToTest = mapToRender.tiles[TileX - 1, TileY];
                if (TileToTest.IsOpen || TileToTest.tileType == automaptileinfo.TILE_DIAG_NE)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, WEST);
                }
            }

            //Check North and East for solids.
            if (TileY < TileMap.TileMapSizeY)
            {//North
                if (mapToRender.tiles[TileX, TileY + 1].tileType == automaptileinfo.TILE_SOLID)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, NORTH);
                }
            }

            if (TileX < TileMap.TileMapSizeX)
            {//East
                if (mapToRender.tiles[TileX + 1, TileY].tileType == automaptileinfo.TILE_SOLID)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, EAST);
                }
            }
        }


        static void DrawDiagSE(Image OutputTileMapImage, automaptileinfo tile, int TileX, int TileY, Color[] InputColour)
        {
            DrawLine(OutputTileMapImage, TileX, TileY, InputColour, SOUTHEAST);

            //Check the tiles to the north and west of this tile
            if (TileY < TileMap.TileMapSizeY)
            {//north
                var TileToTest = mapToRender.tiles[TileX, TileY + 1];
                if (TileToTest.IsOpen || TileToTest.tileType == automaptileinfo.TILE_DIAG_SE)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, NORTH);
                }
            }

            if (TileX > 0)
            {//West
                var TileToTest = mapToRender.tiles[TileX - 1, TileY];
                if (TileToTest.IsOpen || TileToTest.tileType == automaptileinfo.TILE_DIAG_SE)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, WEST);
                }
            }

            //Check South and East for solids.
            if (TileY > 0)
            {//South
                if (mapToRender.tiles[TileX, TileY - 1].tileType == automaptileinfo.TILE_SOLID)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, SOUTH);
                }
            }

            if (TileX < TileMap.TileMapSizeX)
            {//East
                if (mapToRender.tiles[TileX + 1, TileY].tileType == automaptileinfo.TILE_SOLID)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, EAST);
                }
            }
        }



        static void DrawDiagNW(Image OutputTileMapImage, automaptileinfo tile, int TileX, int TileY, Color[] InputColour)
        {
            DrawLine(OutputTileMapImage, TileX, TileY, InputColour, NORTHWEST);

            //Check the tiles to the south and east of this tile
            if (TileY > 0)
            {//South
                var TileToTest = mapToRender.tiles[TileX, TileY - 1];
                if (TileToTest.IsOpen || TileToTest.tileType == automaptileinfo.TILE_DIAG_NW)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, SOUTH);
                }
            }
            if (TileX < TileMap.TileMapSizeX)
            {//East
                var TileToTest = mapToRender.tiles[TileX + 1, TileY];
                if (TileToTest.IsOpen || TileToTest.tileType == automaptileinfo.TILE_DIAG_NW)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, EAST);
                }
            }

            //Check North and West for solids.
            if (TileY < TileMap.TileMapSizeY)
            {//North
                if (mapToRender.tiles[TileX, TileY + 1].tileType == automaptileinfo.TILE_SOLID)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, NORTH);
                }
            }

            if (TileX > 0)
            {//West
                if (mapToRender.tiles[TileX - 1, TileY].tileType == automaptileinfo.TILE_SOLID)
                {
                    DrawLine(OutputTileMapImage, TileX, TileY, InputColour, WEST);
                }
            }
        }

        private static void DrawLine(Image OutputTileMapImage, int TileX, int TileY, Color[] InputColour, int Direction)
        {
            switch (Direction)
            {
                case NORTH:
                    {//Border to the north.
                        for (int i = 0; i < TileSize; i++)
                        {
                            OutputTileMapImage.SetPixel(i + TileX * TileSize, TileSize + TileY * TileSize, PickColour(InputColour));
                        }
                        break;
                    }
                case SOUTH:
                    {//Border to the south.
                        for (int i = 0; i < TileSize; i++)
                        {
                            OutputTileMapImage.SetPixel(i + TileX * TileSize, 0 + TileY * TileSize, PickColour(InputColour));
                        }
                        break;
                    }
                case EAST:
                    {//Border to the east.
                        for (int j = 0; j < TileSize; j++)
                        {
                            OutputTileMapImage.SetPixel(TileSize + TileX * TileSize, j + TileY * TileSize, PickColour(InputColour));
                        }
                        break;
                    }
                case WEST:
                    {//Border to the west.
                        for (int j = 0; j < TileSize; j++)
                        {
                            OutputTileMapImage.SetPixel(0 + TileX * TileSize, j + TileY * TileSize, PickColour(InputColour));
                        }
                        break;
                    }
                case NORTHEAST:
                    {//Diagonal
                        for (int k = 0; k <= TileSize; k++)
                        {
                            OutputTileMapImage.SetPixel((TileSize - k) + TileX * TileSize, k + TileY * TileSize, PickColour(InputColour));
                        }
                        break;
                    }
                case SOUTHWEST:
                    {//Diagonal
                        for (int k = 0; k <= TileSize; k++)
                        {
                            OutputTileMapImage.SetPixel(k + TileX * TileSize, (TileSize - k) + TileY * TileSize, PickColour(InputColour));
                        }
                        break;
                    }
                case NORTHWEST:
                    {//Diagonal
                        for (int k = 0; k <= TileSize; k++)
                        {
                            OutputTileMapImage.SetPixel(k + TileX * TileSize, k + TileY * TileSize, PickColour(InputColour));
                        }
                        break;
                    }
                case SOUTHEAST:
                    {//Diagonal
                        for (int k = 0; k <= TileSize; k++)
                        {
                            OutputTileMapImage.SetPixel(k + TileX * TileSize, k + TileY * TileSize, PickColour(InputColour));
                        }
                        break;
                    }
            }
        }


        private static void DrawDoor(Image OutputTileMapImage, automaptileinfo tile, int TileX, int TileY, Color[] InputColour)
        {
            bool TileTypeNorth = mapToRender.tiles[TileX, TileY + 1].IsOpen;
            bool TileTypeSouth = mapToRender.tiles[TileX, TileY - 1].IsOpen;
            bool TileTypeEast = mapToRender.tiles[TileX + 1, TileY].IsOpen;
            bool TileTypeWest = mapToRender.tiles[TileX - 1, TileY].IsOpen;
            
            if (tile.IsOpen)
            {   //Don't display if the door is currently in a solid tile
                if (TileTypeEast || TileTypeWest)
                {
                    for (int j = 0; j < TileSize; j++)
                    {
                        OutputTileMapImage.SetPixel( TileSize / 2 + TileX * ( TileSize), j + TileY * TileSize, PickColour(InputColour));
                    }
                }
                else if (TileTypeNorth || TileTypeSouth)
                {
                    for (int i = 0; i < TileSize; i++)
                    {
                        OutputTileMapImage.SetPixel(i + TileX *  TileSize,  TileSize / 2 + TileY *  TileSize, PickColour(InputColour));
                    }
                }
            }
        }

        /// <summary>
        /// Picks a random colour from the array of colours
        /// </summary>
        /// <returns>The colour selected</returns>
        /// <param name="Selection">Selection.</param>
        static Color PickColour(Color[] Selection)
        {
            return Selection[rnd.Next(0, Selection.GetUpperBound(0) + 1)];
        }
    } //end class
}//end namespace