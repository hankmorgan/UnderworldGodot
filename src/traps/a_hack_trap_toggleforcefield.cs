namespace Underworld
{
    /// <summary>
    /// Trap which toggles a forcefield by changing it's zpos when activated
    /// </summary>
    public class a_hack_trap_toggleforcefield : hack_trap
    {
        public static void Activate(int triggerX, int triggerY)
        {
            var tile = UWTileMap.current_tilemap.Tiles[triggerX, triggerY];
            if (tile.indexObjectList!=0)
            {
                var obj = UWTileMap.current_tilemap.LevelObjects[tile.indexObjectList];
                CallBacks.RunCodeOnObjectsInChain(
                    methodToCall: ToggleForceField, 
                    obj: obj, 
                    objList: UWTileMap.current_tilemap.LevelObjects);
            }           
        }

        public static bool ToggleForceField(uwObject obj)
        {
            if (obj.item_id==365)
            {
                if (obj.zpos>=0x7F)
                {
                    obj.zpos = 0;
                }
                else
                {
                    obj.zpos = 0x7F;
                }
                objectInstance.Reposition(obj);
                return true;
            }
            else
            {
                return false;
            }
        }
    }//end class
}//end namespace