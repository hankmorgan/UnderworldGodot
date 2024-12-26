namespace Underworld
{
    /// <summary>
    /// Trap that toggles a tile between height=0 and a specified height.
    /// </summary>
    public class a_pit_trap : trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            var tile = UWTileMap.current_tilemap.Tiles[triggerX, triggerY];
            int newTileType;// = tile.tileType;
            int newFloorTexture;
            int newHeight;
            if (tile.floorHeight == 0)
            {
                newHeight = (trapObj.ypos<<3) + trapObj.xpos;
                newFloorTexture = trapObj.owner;
                
            }
            else
            {
                newHeight = 0;
                newFloorTexture = trapObj.quality;
            }
            if (newHeight<15)
            {
                newTileType = UWTileMap.TILE_OPEN;
            }
            else
            {
                newTileType = UWTileMap.TILE_SOLID;
            }

            TileInfo.ChangeTile(
                StartTileX: triggerX, 
                StartTileY: triggerY, 
                newFloorTexture: newFloorTexture, 
                newType: newTileType,
                newHeight: newHeight,
                HeightAdjustFlag:4);

        }
    }//end class
}//end namespace