using System.IO;
using System.Diagnostics;

namespace Underworld
{
    public class TerrainDatLoader : Loader
    {

        public enum TerrainTypes
        {
            Normal = 0,
            Ankh = 2,
            Stairsup = 3,
            Stairsdown = 4,
            Pipe = 5,
            Grating = 6,
            Drain = 7,
            ChainedPrincess,
            Window = 9,
            Tapestry = 0xa,
            Textured_door = 0xb,
            Water = 0x10,
            Lava = 0x20,
            Waterfall = 0x40,
            Ice_wall = 0xC0,
            Ice_walls,
            Lavafall = 0x80,
            IceNonSlip = 0xF8,
            WaterFlowSouth = 72,
            WaterFlowNorth = 80,//not yet seen in the wild
            WaterFlowWest = 88,
            WaterFlowEast = 96,
            Unknown = -1
        };

        public const int Normal = 0; // (solid) wall or floor
        public const int Ankh = 2;//mural (shrines)
        public const int Stairsup = 3;
        public const int Stairsdown = 4;
        public const int Pipe = 5;
        public const int Grating = 6;
        public const int Drain = 7;
        public const int ChainedPrincess = 8;
        public const int Window = 9;
        public const int Tapestry = 0xa;
        public const int Textured_door = 0xb;//(used for the lock to the Key of Infinity)
        public const int Water = 0x10;//(not waterfall)
        public const int Lava = 0x20;//(not lavafall)
        public const int Waterfall = 0x40;//     - UW2
        public const int Ice_wall = 0xC0; //     - UW2
                                        //TODO:ID more water types
                                        //00D8
        public const int Ice_walls = 0xE8;// (crumbling?)
        public const int Lavafall = 0x80;   //  - UW2
        public const int IceNonSlip = 0xF8; //   - UW2

        public const int WaterFlowSouth = 72;
        public const int WaterFlowNorth = 80;
        public const int WaterFlowWest = 88;
        public const int WaterFlowEast = 96;

        public static int[] Terrain;

        static TerrainDatLoader()
        {
            string filename = "TERRAIN.DAT";
            if (_RES == GAME_UWDEMO)
            {
                filename = "DTERRAIN.DAT";
            }
            Terrain = new int[256 + 256];//wall and floor
            int add_ptr = 0;
            if (ReadStreamFile(Path.Combine(BasePath, "DATA", filename), out byte[] terrain_dat))
            {
                switch (_RES)
                {
                    case GAME_UW1:
                    case GAME_UWDEMO:
                        for (int i = 0; i < 256; i++)
                        {
                            Terrain[i] = (int)getAt(terrain_dat, add_ptr, 16);
                            add_ptr += 2;
                        }
                        add_ptr = 0x200;
                        for (int i = 256; i < 512; i++)
                        {
                            Terrain[i] = (int)getAt(terrain_dat, add_ptr, 16);
                            add_ptr += 2;
                        }
                        break;
                    case GAME_UW2:
                        {
                            for (int i = 0; i < 256; i++)
                            {
                                Terrain[i] = (int)getAt(terrain_dat, add_ptr, 16);
                                add_ptr += 2;
                            }
                            break;
                        }
                }
            }
        }

        public static TerrainTypes getTerrain(int terrainNo)
        {
            /*
            Normal =0,
            Ankh =2,
            Stairsup = 3,
            Stairsdown=4,
            Pipe=5,
            Grating=6,
            Drain=7,
            ChainedPrincess,
            Window=9,
            Tapestry=0xa,
            Textured_door= 0xb,
            Water = 0x10,
            Lava = 0x20,
            Waterfall =0x40,
            Ice_wall =0xC0,
            Ice_walls,
            Lavafall  = 0x80,
            IceNonSlip  =0xF8  , 
            Unknown = -1
    */

            switch (terrainNo)
            {
                case Normal: return TerrainTypes.Normal;
                case Ankh: return TerrainTypes.Ankh;
                case Stairsup: return TerrainTypes.Stairsup;
                case Stairsdown: return TerrainTypes.Stairsdown;
                case Pipe: return TerrainTypes.Pipe;
                case Grating: return TerrainTypes.Grating;
                case Drain: return TerrainTypes.Drain;
                case ChainedPrincess: return TerrainTypes.ChainedPrincess;
                case Window: return TerrainTypes.Window;
                case Tapestry: return TerrainTypes.Tapestry;
                case Textured_door: return TerrainTypes.Textured_door;
                case Water: return TerrainTypes.Water;
                case Lava: return TerrainTypes.Lava;
                case Waterfall: return TerrainTypes.Waterfall;
                case Ice_wall: return TerrainTypes.Ice_wall;
                case Ice_walls: return TerrainTypes.Ice_walls;
                case Lavafall: return TerrainTypes.Lavafall;
                case IceNonSlip: return TerrainTypes.IceNonSlip;
                case WaterFlowSouth: return TerrainTypes.WaterFlowSouth;
                case WaterFlowWest: return TerrainTypes.WaterFlowWest;
                case WaterFlowEast: return TerrainTypes.WaterFlowEast;
                case WaterFlowNorth: return TerrainTypes.WaterFlowNorth;
                //case Unknown : 
                default:
                    Debug.Print("Terrain unknown:" + terrainNo);
                    return TerrainTypes.Unknown;
            }
        }

        /// <summary>
        /// Gets the terrain type for the specified tile.
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public static int GetTerrainTypeNo(TileInfo tile)
        {
            if (_RES == GAME_UW2)
            {//TOTEST
             //var floorterrain = tileMapRender.FloorTexture(tile);
                var floorterrain = tileMapRender.FloorTexture_MapIndex(tile);
                var ActualTexture = UWTileMap.current_tilemap.texture_map[floorterrain];
                var terrain = TerrainDatLoader.Terrain[ActualTexture];//tile.floorTexture];// floorterrain];
                terrain = (terrain & 0xC0) >> 6;
                return terrain;
            }
            else
            {
                //This is probably wrong as NPCS in UW1 are pathfinding into lava!
                var floorterrain = tileMapRender.FloorTexture_MapIndex(tile);
                var ActualTexture = UWTileMap.current_tilemap.texture_map[floorterrain];
                var terrain = TerrainDatLoader.Terrain[46 + ActualTexture];//floorterrain+46]; //46=256-210
                return terrain >> 4;
            }
        }


    }//end class


}//end namespace