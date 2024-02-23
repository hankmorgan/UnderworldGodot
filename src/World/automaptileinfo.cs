namespace Underworld
{
    /// <summary>
    /// Storing and retreval of info about automap tiles
    /// </summary>
    public class automaptileinfo : UWClass
    {
        public const int DisplayTypeClear = 0;  //uw1 + 2
        public const int DisplayTypeWaterUW1 = 1;
        public const int DisplayTypeWaterUW2 = 4;
        public const int DisplayTypeLava = 2;
        public const int DisplayTypeDoorUW1 = 4;
        public const int DisplayTypeDoorUW2 = 1;
        public const int DisplayTypeBridge1UW1 = 9;
        public const int DisplayTypeBridge2UW1 = 10;
        public const int DisplayTypeBridgeUW2 = 6;
        public const int DisplayTypeStairUW1 = 12;
        public const int DisplayTypeStairUW2 = 3;
        public const int DisplayTypeIce = 12;

        public const int TILE_SOLID = 0;
        public const int TILE_OPEN = 1;
        public const int TILE_DIAG_SE = 2;
        public const int TILE_DIAG_SW = 3;
        public const int TILE_DIAG_NE = 4;
        public const int TILE_DIAG_NW = 5;
        public const int TILE_SLOPE_N = 6;
        public const int TILE_SLOPE_S = 7;
        public const int TILE_SLOPE_E = 8;
        public const int TILE_SLOPE_W = 9;

        /// <summary>
        /// Location of data in the buffer
        /// </summary>
        public int PTR;

        /// <summary>
        /// Reference to the data buffer where the tile data is held
        /// </summary>
        public byte[] buffer;
        /// <summary>
        /// How the tile is rendered
        /// </summary>
        public short DisplayType
        {
            get
            {
                return (short)((buffer[PTR] >> 4) & 0xf);
            }
        }

        /// <summary>
        /// The type of tile. When set means tile has been visited
        /// </summary>
        public short tileType
        {
            get
            {
                return (short)(buffer[PTR] & 0xF);
            }
        }

        public automaptileinfo(int _ptr, ref byte[] _buffer)
        {
            PTR = _ptr;
            buffer = _buffer;
        }

        public bool visited
        {
            get
            {
                switch (tileType)
                {
                    case TILE_SOLID:
                        return false;
                    default:
                        return true;
                }
            }
            // set
            // {

            // }
        }

        public bool IsBridge
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return DisplayType == DisplayTypeBridgeUW2;
                    default:
                        return DisplayType == DisplayTypeBridge1UW1 || DisplayType == DisplayTypeBridge2UW1;
                }
            }
        }

        public bool IsWater
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return DisplayType == DisplayTypeWaterUW2;
                    default:
                        return DisplayType == DisplayTypeWaterUW1;
                }
            }
        }

        public bool IsLava
        {
            get
            {
                return DisplayType == DisplayTypeLava;
            }
        }

        public bool IsStair
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return DisplayType == DisplayTypeStairUW2;
                    default:
                        return DisplayType == DisplayTypeStairUW1;
                }
            }
        }

        public bool IsIce
        {
            get
            {
                return DisplayType == DisplayTypeIce;
            }
        }

        public bool IsOpen
        {
            get
            {
                switch (tileType)
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
        }

        public bool IsDoor
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return DisplayType == DisplayTypeDoorUW2;
                    default:
                        return DisplayType == DisplayTypeDoorUW1;
                }
            }
        }

        public bool IsSolidWall
        {
            get
            {
                switch (tileType)
                {
                    case TILE_SOLID:
                    case 10:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        return true;
                    case 11://11 appears to indicate unexplored open tiles
                    default:
                        return false;
                }
            }
        }
    }//end class
}//end namespace