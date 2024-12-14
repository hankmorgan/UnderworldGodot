using System;

namespace Underworld
{
    /// <summary>
    /// Trap which controls oscillating rows of tiles (example blue zone in ethereal void)
    /// </summary>
    public class a_hack_trap_oscillator : hack_trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY, uwObject[] objList)
        {
            Oscillate(trapObj.tileX, trapObj.tileY, trapObj.owner);
            trapObj.owner = (short)((trapObj.owner + 1) & 0xF);
        }

        /// <summary>
        /// oscillates the tile in a wave pattern.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="owner"></param>
        static void Oscillate(int X, int Y, int owner)
        {
            int[] oscillatordata = new int[] { 1, 2, 1, 0, -1, -2, -1 };
            var tile = UWTileMap.current_tilemap.Tiles[X, Y];

            var height = tile.floorHeight;
            var adjustment = 0;
            if (owner >= 8)
            {
                adjustment = Math.Abs(12 - owner) - 4;
            }
            else
            {
                adjustment = 4 - Math.Abs(4 - owner);
            }

            for (int si = 1; si < 8; si++)
            {
                //var t = UWTileMap.current_tilemap.Tiles[X, Y - si];
                var finalheight = (short)((adjustment * oscillatordata[si - 1]) / 2);
                finalheight += height;
                TileInfo.ChangeTile(
                    StartTileX: X, 
                    StartTileY: Y - si, 
                    newHeight: finalheight);
            }
        }
    }//end class
}//end namespace