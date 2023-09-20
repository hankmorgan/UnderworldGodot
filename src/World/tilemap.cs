using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    /// <summary>
    /// Tile map class for storing and accessing the tilemap and tile properties..
    /// </summary>
    public class TileMap : Loader
    {
        //Raw Data
        public DataLoader.UWBlock lev_ark_block = new DataLoader.UWBlock();//Data containing tilemap and object data
        public DataLoader.UWBlock tex_ark_block = new DataLoader.UWBlock();//Data containing texture map
        public DataLoader.UWBlock ovl_ark_block = new DataLoader.UWBlock();//Data containing animation overlays


        //Tile Types for UW1 & 2 and SS1. Note the diag tiles are flipped around in SS1.
        public const short TILE_SOLID = 0;
        public const short TILE_OPEN = 1;
        public const short TILE_DIAG_SE = 2;
        public const short TILE_DIAG_SW = 3;
        public const short TILE_DIAG_NE = 4;
        public const short TILE_DIAG_NW = 5;
        public const short TILE_SLOPE_N = 6;
        public const short TILE_SLOPE_S = 7;
        public const short TILE_SLOPE_E = 8;
        public const short TILE_SLOPE_W = 9;
        public const short TILE_VALLEY_NW = 10;
        public const short TILE_VALLEY_NE = 11;
        public const short TILE_VALLEY_SE = 12;
        public const short TILE_VALLEY_SW = 13;
        public const short TILE_RIDGE_SE = 14;
        public const short TILE_RIDGE_SW = 15;
        public const short TILE_RIDGE_NW = 16;
        public const short TILE_RIDGE_NE = 17;

        /// <summary>
        /// The tile map size along the x axis
        /// </summary>
        public const short TileMapSizeX = 63; //0 to 63

        /// <summary>
        /// The tile map size along the y axis.
        /// </summary>
        public const short TileMapSizeY = 63; //0 to 63

        /// <summary>
        /// Locaton X and Y of the object storage tile location where non map objects are kept.
        /// </summary>
        public const short ObjectStorageTile = 99;

        public const short SURFACE_FLOOR = 1;
        public const short SURFACE_CEIL = 2;
        public const short SURFACE_WALL = 3;
        public const short SURFACE_SLOPE = 4;

        public const short SLOPE_BOTH_PARALLEL = 0;
        public const short SLOPE_BOTH_OPPOSITE = 1;
        public const short SLOPE_FLOOR_ONLY = 2;
        public const short SLOPE_CEILING_ONLY = 3;

        //Visible faces indices. Used in sorting tile surface visiblity.
        public const short vTOP = 0;
        public const short vEAST = 1;
        public const short vBOTTOM = 2;
        public const short vWEST = 3;
        public const short vNORTH = 4;
        public const short vSOUTH = 5;


        //BrushFaces
        const short fSELF = 128;
        const short fCEIL = 64;
        const short fNORTH = 32;
        const short fSOUTH = 16;
        const short fEAST = 8;
        const short fWEST = 4;
        const short fTOP = 2;
        const short fBOTTOM = 1;

        public const int UW1_TEXTUREMAPSIZE = 64;
        public const int UW2_TEXTUREMAPSIZE = 70;
        public const int UWDEMO_TEXTUREMAPSIZE = 63;

        public const int UW1_NO_OF_LEVELS = 9;
        public const int UW2_NO_OF_LEVELS = 80;

        /// <summary>
        /// The ceiling texture for this level
        /// </summary>
        public short UWCeilingTexture;

        /// <summary>
        /// Animation overlay. Controls how long an animated effect appears for.
        /// </summary>
        public struct Overlay
        {
            public int header;
            public int link;
            public int duration;
            public int tileX;
            public int tileY;
        };

        /// <summary>
        /// Lists of overlays for controlling animated items.
        /// </summary>
        public Overlay[] Overlays = new Overlay[64];

        public int thisLevelNo; //The number of this level
        public short UW_CEILING_HEIGHT;
        public short CEILING_HEIGHT;
        public short SHOCK_CEILING_HEIGHT;

        /// <summary>
        /// The texture indices for the current map.
        /// </summary>
        public short[] texture_map = new short[272];


        /// <summary>
        /// Tile info storage class
        /// </summary>
        public TileInfo[,] Tiles = new TileInfo[TileMapSizeX + 1, TileMapSizeY + 1];

        /// <summary>
        /// The current tile X that the player is in
        /// </summary>
        public static short visitTileX;
        /// <summary>
        /// The current tile Y that the player is in.
        /// </summary>
        public static short visitTileY;

        /// The tile X that the player was in the previous frame
        /// </summary>
        public static short visitedTileX;
        /// <summary>
        /// The current tile Y that the player was in the previous frame
        /// </summary>
        public static short visitedTileY;

        /// <summary>
        /// Map used for A* path finding tests.
        /// </summary>
        //public List<string> map = new List<string>();

        /// <summary>
        /// Reference to the objects list for this level.
        /// </summary>
        public List<uwObject> LevelObjects;
        // {
        //     get
        //     {
        //         return GameWorldController.instance.objectList[thisLevelNo];
        //     }
        // }


        public TileMap(int NewLevelNo)
        {
            thisLevelNo = NewLevelNo;
        }

        /// <summary>
        /// Checks to see if the tile at a specified location is within the valid game world. (eg is rendered and is not a solid).
        /// Assumes the map is positioned at 0,0,0
        /// </summary>
        /// <returns><c>true</c>, if tile was valided, <c>false</c> otherwise.</returns>
        /// <param name="location">Location.</param>
        public bool ValidTile(Vector3 location)
        {
            int tileX = (int)(location.X / 1.2f);
            int tileY = (int)(location.Y / 1.2f);
            if ((tileX > TileMapSizeX) || (tileX < 0) || (tileY > TileMapSizeY) || (tileY < 0))
            {//Location is outside the map
                return false;
            }
            int tileType = GetTileType(tileX, tileY);
            bool isRendered = GetTileRender(tileX, tileY);

            return ((tileType != TILE_SOLID) && (isRendered));
        }

        /// <summary>
        /// Validates the tile to see if it is within the range of tiles.
        /// </summary>
        /// <returns><c>true</c>, if tile was valided, <c>false</c> otherwise.</returns>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        public static bool ValidTile(int tileX, int tileY)
        {
            return (((tileX >= 0) && (tileX <= TileMapSizeX)) && ((tileY >= 0) && (tileY <= TileMapSizeY)));
        }

        /// <summary>
        /// Tells if the tile is one of the square open types
        /// </summary>
        /// <returns><c>true</c>, if tile open was ised, <c>false</c> otherwise.</returns>
        /// <param name="TileType">Tile type.</param>
        public static bool isTileOpen(int TileType)
        {
            switch (TileType)
            {
                case TILE_OPEN:
                case TILE_SLOPE_N:
                case TILE_SLOPE_S:
                case TILE_SLOPE_E:
                case TILE_SLOPE_W:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        /// <summary>
        /// Gets the height of the floor for the specified tile.
        /// </summary>
        /// <returns>The floor height.</returns>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        public int GetFloorHeight(int tileX, int tileY)
        {
            if (ValidTile(tileX, tileY))
            {
                return Tiles[tileX, tileY].floorHeight;
            }
            else
            {
                // Debug.Log("invalid tile for height at " + tileX + "," + tileY);
                return 0;
            }
        }

        /// <summary>
        /// Gets the height of the ceiling. Will always be the same value in UW1/2 varies in SHOCK.
        /// </summary>
        /// <returns>The ceiling height.</returns>
        /// <param name="LevelNo">Level no.</param>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        public int GetCeilingHeight(int tileX, int tileY)
        {
            return Tiles[tileX, tileY].ceilingHeight;
        }

        /// <summary>
        /// Sets the height of the floor.
        /// </summary>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        /// <param name="newHeight">New height.</param>
        public void SetFloorHeight(int tileX, int tileY, short newHeight)
        {
            Tiles[tileX, tileY].floorHeight = newHeight;
        }

        /// <summary>
        /// Sets the height of the ceiling.
        /// </summary>
        /// <param name="LevelNo">Level no.</param>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        /// <param name="newHeight">New height.</param>
        public void SetCeilingHeight(int tileX, int tileY, short newHeight)
        {
            //Debug.Log ("ceil :" + newHeight + " was " + CeilingHeight[tileX,tileY]);
            Tiles[tileX, tileY].ceilingHeight = newHeight;
        }


        /// <summary>
        /// Gets the type of the tile.
        /// </summary>
        /// <returns>The tile type.</returns>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        public int GetTileType(int tileX, int tileY)
        {
            //if ((tileX>TileMap.TileMapSizeX) || (tileY>TileMap.TileMapSizeY) || (tileX<0) || (tileY<0))
            if (!ValidTile(tileX, tileY))
            {//Assume out of bounds is solid
                return TILE_SOLID;
            }
            else
            {
                return Tiles[tileX, tileY].tileType;
            }
        }

        /// <summary>
        /// Gets the room region at the specified tile
        /// </summary>
        /// <returns>The room.</returns>
        /// <param name="TileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        public int GetRoom(int tileX, int tileY)
        {
            if (ValidTile(tileX, tileY))
            {
                return Tiles[tileX, tileY].roomRegion;
            }
            else
            {
                return 0;
            }
        }


        /// <summary>
        /// Gets the tile render state. 
        /// </summary>
        /// <returns>The tile render.</returns>
        /// <param name="LevelNo">Level no.</param>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        private bool GetTileRender(int tileX, int tileY)
        {
            return Tiles[tileX, tileY].Render == true;
        }

        /// <summary>
        /// Gets the vector3 at the center of the tile specified.
        /// </summary>
        /// <returns>The tile vector.</returns>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        public Vector3 getTileVector(int tileX, int tileY)
        {
            return new Vector3(
                    (tileX * 1.2f) + 0.6f,
                    GetFloorHeight(tileX, tileY) * 0.15f,
                    (tileY * 1.2f) + 0.6f
            );
        }

        /// <summary>
        /// Gets the vector3 at the center of the tile specified.
        /// </summary>
        /// <returns>The tile vector.</returns>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        public Vector3 getTileVector(float tileX, float tileY)
        {
            return getTileVector((int)tileX, (int)tileY);
        }

        /// <summary>
        /// Gets the vector3 at the center of the tile specified.
        /// </summary>
        /// <returns>The tile vector.</returns>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        public Vector3 getTileVector(int tileX, int tileY, float zpos)
        {
            return new Vector3(
                    (tileX * 1.2f) + 0.6f,
                    zpos,
                    (tileY * 1.2f) + 0.6f
            );
        }

        public bool BuildTileMapUW(int levelNo, DataLoader.UWBlock lev_ark, DataLoader.UWBlock tex_ark, DataLoader.UWBlock ovl_ark)
        {
            long address_pointer = 0;
            short CeilingTexture = 0;

            UW_CEILING_HEIGHT = ((128 >> 2) * 8 >> 3);  //Shifts the scale of the level. Idea borrowed from abysmal

            CEILING_HEIGHT = UW_CEILING_HEIGHT;
            BuildTextureMap(tex_ark, ref CeilingTexture, levelNo);
            this.UWCeilingTexture = CeilingTexture;
            for (short y = 0; y <= TileMapSizeY; y++)
            {
                for (short x = 0; x <= TileMapSizeX; x++)
                {
                    Tiles[x, y] = new TileInfo(this, x, y);
                    address_pointer += 4;
                }
            }

            SetTileMapWallFacesUW();

            BuildObjectListUW();

            //if (OverlayAddress!=0)
            switch (_RES)
            {
                case GAME_UW1:
                    {
                        if (ovl_ark.DataLen != 0)
                        {//read in the next 64 entries of length 6 bytes	
                            long OverlayAddress = 0;
                            for (int overlayIndex = 0; overlayIndex < 64; overlayIndex++)
                            {
                                Overlays[overlayIndex].header = (int)DataLoader.getValAtAddress(ovl_ark, OverlayAddress, 16);
                                Overlays[overlayIndex].link = (int)(DataLoader.getValAtAddress(ovl_ark, OverlayAddress, 16) >> 6) & 0x3ff;
                                Overlays[overlayIndex].duration = (int)DataLoader.getValAtAddress(ovl_ark, OverlayAddress + 2, 16);
                                Overlays[overlayIndex].tileX = (int)DataLoader.getValAtAddress(ovl_ark, OverlayAddress + 4, 8);
                                Overlays[overlayIndex].tileY = (int)DataLoader.getValAtAddress(ovl_ark, OverlayAddress + 5, 8);
                                if (Overlays[overlayIndex].link != 0)
                                {
                                    // Debug.Log("Overlay at " + OverlayAddress
                                    //    + " obj " + Overlays[overlayIndex].link
                                    //     + " for " + Overlays[overlayIndex].duration
                                    //     + " tile " + Overlays[overlayIndex].tileX + "," + Overlays[overlayIndex].tileY
                                    //     + " header :" + Overlays[overlayIndex].header);
                                }
                                OverlayAddress += 6;
                            }
                        }
                        break;
                    }
                case GAME_UW2:
                    {
                        long OverlayAddress = 31752;
                        for (int overlayIndex = 0; overlayIndex < 64; overlayIndex++)
                        {
                            if (OverlayAddress + 5 <= lev_ark.Data.GetUpperBound(0))
                            {
                                Overlays[overlayIndex].header = (int)DataLoader.getValAtAddress(lev_ark, OverlayAddress, 16);
                                Overlays[overlayIndex].link = (int)(DataLoader.getValAtAddress(lev_ark, OverlayAddress, 16) >> 6) & 0x3ff;
                                Overlays[overlayIndex].duration = (int)DataLoader.getValAtAddress(lev_ark, OverlayAddress + 2, 16);
                                Overlays[overlayIndex].tileX = (int)DataLoader.getValAtAddress(lev_ark, OverlayAddress + 4, 8);
                                Overlays[overlayIndex].tileY = (int)DataLoader.getValAtAddress(lev_ark, OverlayAddress + 5, 8);
                                if (Overlays[overlayIndex].link != 0)
                                {
                                    // Debug.Log("Overlay at " + OverlayAddress 
                                    //     + " obj " + Overlays[overlayIndex].link 
                                    //     + " for " + Overlays[overlayIndex].duration 
                                    //     + " tile " + Overlays[overlayIndex].tileX + "," + Overlays[overlayIndex].tileY
                                    //    + " header :" + Overlays[overlayIndex].header);
                                }
                            }
                            OverlayAddress += 6;
                        }
                        break;
                    }

            }

            return true;
        }

        void BuildObjectListUW()
        {
            LevelObjects = new();
            int address_pointer = 0;
            int objectsAddress = (64 * 64 * 4);
            for (short x = 0; x < 1024; x++)
            {   //read in master object list
                var uwobj = new uwObject
                {
                    isInventory = false,
                    IsStatic = (x >= 256),
                    index = x,
                    PTR = 0 + objectsAddress + address_pointer,
                    DataBuffer = this.lev_ark_block.Data
                };

                LevelObjects.Add(uwobj);

                Debug.Print(StringLoader.GetObjectNounUW(uwobj.item_id));
                if (uwobj.npc_whoami!=0)
                {
                    Debug.Print (StringLoader.GetString(7, uwobj.npc_whoami + 16));
                }

                if (x<256)
                {
                    address_pointer += 27;
                }
                else
                {
                    address_pointer += 8;
                }

                // objList[x] = new ObjectLoaderInfo(x, map, true)
                // {
                //     map = map,
                //     parentList = this,
                //     index = x,                
                //     address = map.lev_ark_block.Address + objectsAddress + address_pointer
                // };          

                // if ((objList[x].item_id >= 464) && ((_RES == GAME_UW1) || (_RES == GAME_UWDEMO)))//Fixed for bugged out of range items
                // {
                //     objList[x].item_id = 0;
                // }
                // HandleMovingDoors(objList, x);
                // SetObjectTextureValue(objList, map.texture_map, x);
            }
        }

        /// <summary>
        /// Creates the tile map wall textures for each north, south, east and west faces
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public void SetTileMapWallFacesUW()
        {
            short x; short y;
            for (y = 0; y <= TileMapSizeY; y++)
            {
                for (x = 0; x <= TileMapSizeX; x++)
                {
                    SetTileWallFacesUW(x, y);
                }
            }
        }

        /// <summary>
        /// Sets the tile wall faces for the selected tile
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public void SetTileWallFacesUW(short x, short y)
        {
            if (Tiles[x, y].tileType >= 0)//was just solid only. Note: If textures are all wrong it's probably caused here!
            {
                //assign it's north texture
                if (y < TileMapSizeY)
                {
                    Tiles[x, y].North = Tiles[x, y + 1].wallTexture;
                }
                else
                {
                    Tiles[x, y].North = -1;
                }
                //assign it's southern
                if (y > 0)
                {
                    Tiles[x, y].South = Tiles[x, y - 1].wallTexture;
                }
                else
                {
                    Tiles[x, y].South = -1;
                }
                //it's east
                if (x < TileMapSizeX)
                {
                    Tiles[x, y].East = Tiles[x + 1, y].wallTexture;
                }
                else
                {
                    Tiles[x, y].East = -1;
                }
                //assign it's West
                if (x > 0)
                {
                    Tiles[x, y].West = Tiles[x - 1, y].wallTexture;
                }
                else
                {
                    Tiles[x, y].West = -1;
                }
            }
        }

        /// <summary>
        /// Cleans up the tilemap. Splits up the tiles into strips of tiles along the x or y axis and sets tile face visibility as required
        /// </summary>
        /// <param name="game">Game.</param>
        /// Although the tile map renderer supports tiles of size X*Y I'm only smart enought to optimise the tilemap into strips of X*1 or Y*1 !!
        public void CleanUp(string game)
        {
            return; // turn off until i figure out godot and work out the kinks
        }


        public static bool isTerrainWater(int terraintype)
        {
            switch (terraintype)
            {
                case TerrainDatLoader.Water:
                case TerrainDatLoader.Waterfall:
                case TerrainDatLoader.WaterFlowEast:
                case TerrainDatLoader.WaterFlowWest:
                case TerrainDatLoader.WaterFlowNorth:
                case TerrainDatLoader.WaterFlowSouth:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the terrain is lava
        /// </summary>
        /// <param name="terraintype"></param>
        /// <returns>True if it lava</returns>
        public static bool isTerrainLava(int terraintype)
        {
            switch (terraintype)
            {
                case TerrainDatLoader.Lava:
                case TerrainDatLoader.Lavafall:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if Terrain is ice
        /// </summary>
        /// <param name="terraintype"></param>
        /// <returns></returns>
        public static bool isTerrainIce(int terraintype)
        {
            switch (terraintype)
            {
                case TerrainDatLoader.Ice_wall:
                case TerrainDatLoader.IceNonSlip:
                case TerrainDatLoader.Ice_walls:
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Builds a texture map from file data
        /// </summary>
        /// <param name="tex_ark"></param>
        /// <param name="CeilingTexture"></param>
        /// <param name="LevelNo"></param>
        void BuildTextureMap(DataLoader.UWBlock tex_ark, ref short CeilingTexture, int LevelNo)
        {
            short textureMapSize;//=UW1_TEXTUREMAPSIZE;
            switch (_RES)
            {
                case GAME_UW2:
                    textureMapSize = UW2_TEXTUREMAPSIZE;
                    break;
                case GAME_UWDEMO:
                    textureMapSize = UWDEMO_TEXTUREMAPSIZE;
                    break;
                default:
                    textureMapSize = UW1_TEXTUREMAPSIZE;
                    break;
            }
            int offset = 0;
            for (int i = 0; i < textureMapSize; i++)//256
            {
                //TODO: Only use this for texture lookups.
                switch (_RES)
                {
                    case GAME_UWDEMO:
                        {
                            if (i < 48)//Wall textures
                            {
                                texture_map[i] = (short)DataLoader.getValAtAddress(tex_ark, offset, 16);
                                //(i * 2)
                                offset += 2;
                            }
                            else
                                if (i <= 57)//Floor textures are 49 to 56, ceiling is 57
                            {
                                texture_map[i] = (short)(DataLoader.getValAtAddress(tex_ark, offset, 16) + 48);
                                //(i * 2)
                                offset += 2;
                                if (i == 57)
                                {
                                    CeilingTexture = (short)i;
                                }
                            }
                            else
                            {
                                //door textures are int 8s
                                texture_map[i] = (short)DataLoader.getValAtAddress(tex_ark, offset, 8);
                                //+210; //(i * 1)
                                offset++;
                            }
                            break;
                        }
                    case GAME_UW1:
                        {
                            if (i < 48)//Wall textures
                            {
                                texture_map[i] = (short)DataLoader.getValAtAddress(tex_ark, offset, 16);
                                offset += 2;
                            }
                            else
                                if (i <= 57)//Floor textures are 48 to 56, ceiling is 57
                            {
                                texture_map[i] = (short)(DataLoader.getValAtAddress(tex_ark, offset, 16) + 210);
                                offset += 2;
                                if (i == 57)
                                {
                                    CeilingTexture = (short)i;
                                }
                            }
                            else
                            {
                                //door textures are int 8s
                                texture_map[i] = (short)DataLoader.getValAtAddress(tex_ark, offset, 8);
                                //+210; //(i * 1)
                                offset++;
                            }
                            break;
                        }
                    case GAME_UW2://uw2
                        {
                            if (i < 64)
                            {
                                texture_map[i] = (short)DataLoader.getValAtAddress(tex_ark, offset, 16);
                                //tmp //textureAddress+//(i*2)
                                offset += 2;
                            }
                            else
                            {
                                //door textures
                                texture_map[i] = (short)DataLoader.getValAtAddress(tex_ark, offset, 8);
                                //tmp //textureAddress+//(i*2)
                                offset++;
                            }
                        }
                        if (i == 0xf)
                        {
                            CeilingTexture = (short)i;
                            //texture_map[i];
                        }
                        if ((LevelNo == (int)(worlds.UW2_LevelNos.Ethereal4)) && (i == 16))
                        {
                            //Not sure why this is an exceptional case!
                            CeilingTexture = (short)i;
                        }
                        break;
                }
            }
        }
    } //end class

}//end namespace