namespace Underworld
{
    /// <summary>
    /// Recycles glass bottles into gold coins
    /// </summary>
    public class a_hack_trap_recycler : hack_trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY, uwObject[] objList)
        {
            var tile = UWTileMap.current_tilemap.Tiles[triggerX, triggerY];
            if (tile.indexObjectList != 0)
            {
                //Recharge all light spheres in the tile.
                var obj = UWTileMap.current_tilemap.LevelObjects[tile.indexObjectList];
                CallBacks.CallFunctionOnObjectsInChain(
                    methodToCall: Recycle,
                    obj: obj,
                    objList: UWTileMap.current_tilemap.LevelObjects);
            }
        }

        /// <summary>
        /// Turns a bottle into a coin
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool Recycle(uwObject obj)
        {
            if (obj.item_id == 0x13D)
            {
                obj.item_id = 0xA0;
                objectInstance.RedrawFull(obj);
            }
            return false;
        }


    }//end class
}//end namespace