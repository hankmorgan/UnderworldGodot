using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Trap that oscillates the height of a single tile. 
    /// (Note, not the same as the oscillator hack trap which operates over a range of tiles in a wave pattern)
    /// </summary>
    public class an_oscillator_trap : trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {            
            int var22;
            int var38 = 0;
            var var36 = 0x3F;
            var var3A_newheight = 0xF;

            var tile = UWTileMap.current_tilemap.Tiles[triggerX, triggerY];
            var var23 = (trapObj.xpos & 0x6)>>1;

            var var30_floortexture = 0xF;

            var var32 = 0x3F;
            var var34_newheight = 0x3F;

            var si = trapObj.xpos & 0x1;
            if (si == 0)
            {
                si --;
            }

            switch (var23)
            {
                case 0://ovr166_CEB
                    {
                        var22 = tile.floorHeight;
                        var36 = 0xF;
                        var34_newheight = var22 + si;
                        var38 = var34_newheight;
                        if ((tile.tileType != UWTileMap.TILE_SOLID) || (tile.tileType == UWTileMap.TILE_SOLID && si>=0))
                        {
                            if (tile.tileType == UWTileMap.TILE_OPEN)
                            {
                                if (var34_newheight == 0x10)
                                {
                                    var3A_newheight = 0;
                                }
                            }
                        }
                        else
                        {
                            if (tile.tileType == UWTileMap.TILE_SOLID && si < 0)
                            {//ovr166_D15
                                var3A_newheight = 1;
                                var34_newheight = 0xF;
                            }
                        }
                        
                        if (trapObj.owner==0x10)
                        {
                            var36 = 0x3F;
                        }
                        break;    
                    }                    
                case 1://ovr166_D50
                    Debug.Print("THIS IS UNTESTED OscillateTrap!");
                    var22 = tile.floorTexture;
                    var30_floortexture = var22 + si;
                    var38 = var30_floortexture;
                    break;
                case 2://ovr166_D67
                    Debug.Print("THIS IS UNTESTED OscillateTrap!");
                    var22 = tile.wallTexture;
                    var32 = var22 + si;
                    var38 = var32;
                    break;
                case 3:
                    break;
            }//end switch

            if ((trapObj.quality <= var38) && (trapObj.owner>=var38))//check for range.
            {
                Debug.Print($"updating {triggerX},{triggerY}");
                TileInfo.ChangeTile(
                    StartTileX: triggerX,
                    StartTileY: triggerY, 
                    newHeight: var34_newheight,   
                    newFloorTexture: var30_floortexture,                  
                    HeightAdjustFlag: 4,
                    newType: var3A_newheight);
            }

            if (si==1)
            {
                if (trapObj.owner == var38)
                {
                    if (trapObj.quality < var36)
                    {
                        trapObj.xpos &= 0x6;
                    }
                }
            }
            else
            {
                if (trapObj.quality == var38)
                {
                    if (trapObj.owner < var36)
                    {
                        var tmp = trapObj.xpos & 0x6;
                        tmp++;
                        trapObj.xpos = (short)tmp;
                        if (playerdat.dungeon_level == 0x44)
                        {
                            SpecificOscillation();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Specal case for the oscillator trap for dungeon_level 68d (ethereal void)
        /// Appears to be targeted at 2 tiles in the golden maze
        /// Tile 24,2 is at the entrance to the final area with the 2 brain creatures.
        /// Tile 2,21 is where a moongate is located.
        /// This appears to be cut content as the conditions for this case to run will not occur.
        /// </summary>
        static void SpecificOscillation()
        {            
            var tile = UWTileMap.current_tilemap.Tiles[24,2];
            if (tile.floorHeight!=0)
            {
                Debug.Print("THIS IS UNTESTED Special Case for OscillateTrap!");
                TileInfo.ChangeTile(
                    StartTileX: 24, 
                    StartTileY:2, 
                    newWallTexture:0x17, 
                    newType: UWTileMap.TILE_OPEN, 
                    newFloorTexture: 4,
                    newHeight: 0,
                    DimX: 6, DimY:5);

                TileInfo.ChangeTile(
                    StartTileX: 2, 
                    StartTileY: 21, 
                    newWallTexture:0x14, 
                    newFloorTexture: 4, 
                    newHeight: 4,
                    newType: UWTileMap.TILE_OPEN);
            }
        }
    }//end class
}//end namespace