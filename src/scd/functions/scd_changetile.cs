using System.Diagnostics;

namespace Underworld
{
    public partial class scd : UWClass
    {

        /// <summary>
        /// Changes tiles that match a texture to use a new texture and optionally raise the tile tile. Likely usage is to allow collapsing ice floors to refreeze
        /// </summary>
        /// <param name="currentblock"></param>
        /// <param name="eventOffset"></param>
        public static void ChangeTile(byte[] currentblock, int eventOffset)
        {
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    var tile = UWTileMap.current_tilemap.Tiles[x, y];
                    if (tile.floorTexture == currentblock[eventOffset + 6])
                    {
                        if (Rng.r.Next(2) == 0)
                        {//50:50 chance
                            Debug.Print($"SCD Change tile at {x},{y}");
                            TileInfo.ChangeTile(
                                StartTileX: x, StartTileY: y,
                                newFloorTexture: currentblock[eventOffset + 7],
                                newHeight: tile.floorHeight + currentblock[eventOffset + 8]);
                        }
                    }
                }
            }
        }
    }//end class
}//end namespace