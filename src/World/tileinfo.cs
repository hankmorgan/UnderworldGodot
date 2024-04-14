using Godot;
namespace Underworld
{
    public class TileInfo : Loader
    {
        public bool Redraw;

        /// <summary>
        /// PTR to the file data in the UW Block for this tile.
        /// </summary>
        public long Ptr
        {
            get
            {
                return tileX * 4 + tileY * 256;
            }
        }

        /// <summary>
        /// Reference to the tilemap containing this tile
        /// </summary>
        public UWTileMap map;

        /// <summary>
        /// What type of tile this is
        /// </summary>
        public short tileType
        {
            get
            {
                return (short)(map.lev_ark_block.Data[Ptr] & 0x0F);
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr];
                val &= 0xF0;
                val |= (value & 0xF);
                map.lev_ark_block.Data[Ptr] = (byte)val;
            }
        }

        /// <summary>
        /// How high is the floor.
        /// Value is multipled by 2 for rendering purposes.
        /// </summary>
        public short floorHeight
        {
            get
            {
                return (short)(2 * (map.lev_ark_block.Data[Ptr] & 0xF0) >> 4);
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr];
                val &= 0x0F;
                val |= ((value / 2) & 0xF) << 4;
                map.lev_ark_block.Data[Ptr] = (byte)val;
            }
        }

        /// <summary>
        /// How low is the ceiling
        /// </summary>
        /// Constant in UW. Variable in shock
        public short ceilingHeight;

        //Index into the texture map table for the actual floor texture of this tile.
        //private short _floorTexture;
        public short floorTexture
        {
            get
            {
                return (short)((map.lev_ark_block.Data[Ptr + 1] >> 2) & 0x0F);
                //return _floorTexture;
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr + 1];
                val &= 0xC3;
                val |= ((value & 0xF) << 2);
                map.lev_ark_block.Data[Ptr + 1] = (byte)val;
            }
        }

        /// <summary>
        /// Index into texture map for the wall texture presented to other tiles by this tile.
        /// </summary>
        public short wallTexture
        {
            get
            {
                return (short)(map.lev_ark_block.Data[Ptr + 2] & 0x3F);
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr + 2];
                val &= 0xC0;
                val |= ((value & 0x3F));
                map.lev_ark_block.Data[Ptr + 2] = (byte)val;
            }
        }

        /// <summary>
        /// /Points to a linked list of objects in the objects block
        /// </summary>
        public short indexObjectList
        {
            get
            {
                return (short)(getAt(map.lev_ark_block.Data, Ptr + 2, 16) >> 6);
            }
            set
            {
                int val = ((value & 0x3FF) << 6) | (wallTexture & 0x3F);
                map.lev_ark_block.Data[Ptr + 2] = (byte)(val & 0xFF);
                map.lev_ark_block.Data[Ptr + 3] = (byte)((val >> 8) & 0xFF);
            }
        }

        /// <summary>
        /// Does this tile contain a door per uw-formats.txt
        /// </summary>    /
        public short doorBit
        {
            get
            {
                return (short)((map.lev_ark_block.Data[Ptr + 1] >> 7) & 0x01);
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr + 1];
                val &= 0x7F;
                val |= ((value & 0x1) << 7);
                map.lev_ark_block.Data[Ptr + 1] = (byte)val;
            }
        }
        /// <summary>
        /// Set when ever a tile does contain a door regardless of the door bit above.
        /// </summary>
        public bool HasDoor
        {
            get
            {
                return DoorIndex!=0;
            }
        }
        /// <summary>
        /// Index of the door at this tile.
        /// </summary>
        public int DoorIndex;

        /// <summary>
        /// If set then we output this tile. Is off when it is a subpart of a group or is hidden from sight.
        /// </summary>
        public bool Render = true;
       
        /// <summary>
        ///  The dimensions on the x-axis of this tile. 1 for a regular tile.
        /// </summary>
        public short DimX = 1;
        /// <summary>
        /// The dimensions on the y-axis of this tile. 1 for a regular tile.
        /// </summary>
        public short DimY = 1;
        /// <summary>
        /// indicates the tile is a child of a group pareted by a tile of DimX>1 or DimY>1
        /// </summary>
        public bool Grouped = false;
        /// <summary>
        /// Which faces are visible on a tile. Used to reduce mesh complexity.
        /// </summary>
        public bool[] VisibleFaces = { true, true, true, true, true, true };
        /// <summary>
        /// The texture to display on the north face of the tile
        /// </summary>
        /// Based on the actual wall texture value of the tile located in that direction 
        public short North;
        /// <summary>
        /// The texture to display on the south face of the tile
        /// </summary>
        /// Based on the actual wall texture value of the tile located in that direction 
        public short South;
        /// <summary>
        /// The texture to display on the east face of the tile
        /// </summary>
        /// Based on the actual wall texture value of the tile located in that direction 
        public short East;
        /// <summary>
        /// The texture to display on the west face of the tile
        /// </summary>
        /// Based on the actual wall texture value of the tile located in that direction 
        public short West;

        /// <summary>
        /// Is the terrain land?
        /// </summary>
        public bool isLand
        {
            get
            {
                return !((isWater) || (isLava) || (isNothing));
            }
        }
        public bool isStair;

        /// <summary>
        /// Checks if the tile is water.
        /// </summary>
        public bool isWater
        {
            get
            {
                return UWTileMap.isTerrainWater(terrain);
            }
        }
        /// <summary>
        /// Checks if the tile is icy
        /// </summary>
        public bool isIce
        {
            get
            {
                return UWTileMap.isTerrainIce(terrain);
            }
        }

        /// <summary>
        /// Check if the tile on on lava
        /// </summary>
        public bool isLava
        {
            get
            {
                return UWTileMap.isTerrainLava(terrain);
            }
        }
        /// <summary>
        /// Set when the tile contains a bridge.
        /// </summary>
        public bool hasBridge;
        /// <summary>
        /// Set when the tile has the nothing textures
        /// </summary>
        public bool isNothing;
        /// <summary>
        /// Index to the contigous room area that the tile is part of.
        /// </summary>
        /// Used for AI decision making
        public short roomRegion;
        /// <summary>
        /// The x position of this tile
        /// </summary>
        public short tileX;
        /// <summary>
        /// The y position of this tile.
        /// </summary>
        public short tileY;


        /// <summary>
        /// Index of trigger to fire when entering this tile
        /// </summary>
        public int EnterTrigger =0;
        
        /// <summary>
        /// Index of trigger to fire when exiting this tile
        /// </summary>
        public int ExitTrigger =0;

        /// <summary>
        /// UW Tile flags - Unknown purpose
        /// </summary>
        public short flags
        {
            get
            {
                return (short)(map.lev_ark_block.Data[Ptr + 1] & 0x03);
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr + 1];
                val &= 0xFC;
                val |= (value & 0x3);
                map.lev_ark_block.Data[Ptr + 1] = (byte)val;
            }
        }

        public short lightFlag
        {
            get
            {
                return (short)(map.lev_ark_block.Data[Ptr + 1] & 0x1);
            }
        }


        /// <summary>
        /// Only seems to matter on Level 9 and possibly where there is water? 
        /// UPDATE>Possible bug in reading data. Retest this. TODO:
        /// </summary>
        public short noMagic
        {
            get
            {
                return (short)((map.lev_ark_block.Data[Ptr + 1] >> 6) & 0x01);
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr + 1];
                val &= 0xBF;
                val |= ((value & 0x1) << 6);
                map.lev_ark_block.Data[Ptr + 1] = (byte)val;
            }
        }

        //Shock Specific Stuff
        //public short shockSlopeFlag = TileMap.SLOPE_FLOOR_ONLY;    //For controlling ceiling slopes for shock.
        //public short shockCeilingTexture;

        //public short _shockTileSlopeSteepness;
        public short TileSlopeSteepness
        {
            get
            {
                switch (_RES)
                {
                    default:
                        if (tileType >= 2)
                        {
                            return 2;
                        }
                        else
                        {
                            return 0;
                        }
                }
            }
            set
            {
                switch (_RES)
                {
                    default:
                        //do nothing read only.
                        break;
                }
            }
        }

        /// <summary>
        /// Indicates that the tile can change into another type of tile or moves in someway. Eg because change terrain trap.
        /// </summary>
        /// Used to ensure this tile is rendered as a single tile.
        public bool TerrainChange;  //

        public int terrain
        {
            get
            {
                //Set the terrain type for the tile when the texture changes
                switch (_RES)
                {
                    case GAME_UWDEMO:
                    case GAME_UW1:
                        return TerrainDatLoader.Terrain[46 + map.texture_map[floorTexture + 48]];
                    case GAME_UW2:
                        return TerrainDatLoader.Terrain[map.texture_map[floorTexture]];
                    default:
                        return 0;
                }
            }
        }

        public string DescriptionFloor
        {
            get
            {
                return TextureName(floorTexture, true);
            }
        }

        public string DescriptionWall
        {
            get
            {
                return TextureName(wallTexture, false);
            }
        }

        public string DescriptionNorth
        {
            get
            {
                return TextureName(North, false);
            }
        }

        public string DescriptionSouth
        {
            get
            {
                return TextureName(South, false);
            }
        }

        public string DescriptionEast
        {
            get
            {
                return TextureName(East, false);
            }
        }

        public string DescriptionWest
        {
            get
            {
                return TextureName(West, false);
            }
        }


        /// <summary>
        /// Gets the texture name for the specified index (into texturemap) based on the surface type
        /// </summary>
        /// <param name="index"></param>
        /// <param name="floor"></param>
        /// <returns></returns>
        static string TextureName(int index, bool floor = true)
        {
            int offset = 0;
            if ((_RES != GAME_UW2) && (floor)) { offset = 48; }
            var textureNo = UWTileMap.current_tilemap.texture_map[index + offset];
            if (_RES == GAME_UW2)
            {
                if (floor)
                {
                    return GameStrings.GetString(10, 510 - textureNo);
                }
                else
                {
                    return GameStrings.GetString(10, textureNo);
                }                
            }
            else
            {
                if (textureNo < 210)
                {//Return a wall texture.
                    return GameStrings.GetString(10, textureNo);
                }
                else
                {//return a floor texture in reverse order.
                    return GameStrings.GetString(10, 510 - textureNo + 210);
                }
            }
        }

        public static string GetTileSurfaceDescription(Vector3 normal, int tileX, int tileY)
        {
            var t = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            if (t == null)
            {
                return "";
            }
            //look at tile
            //uimanager.AddToMessageScroll($"{tileX},{tileY}");
            //parse the normal into a tile surface.
            if (normal.Y > 0)
            {
                //this is a floor
                return t.DescriptionFloor;
            }
            else
            {
                if (normal == Vector3.Forward)
                {
                    return t.DescriptionSouth;
                }
                if (normal == Vector3.Back)
                {
                    return t.DescriptionNorth;
                }
                if (normal == Vector3.Left)
                {
                    return t.DescriptionEast;
                }
                if (normal == Vector3.Right)
                {
                    return t.DescriptionWest;
                }
            }
            return t.DescriptionWall; //default self wall
        }

        /// <summary>
        /// Initialise a tile with parameters for source data and X,Y offset into data.
        /// </summary>
        /// <param name="tm"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public TileInfo(UWTileMap tm, short X, short Y)
        {
            map = tm;
            tileX = X;
            tileY = Y;

            //Init default render textures.
            North = wallTexture;
            South = wallTexture;
            East = wallTexture;
            West = wallTexture;
        }
    }//end class

}//end namespace