namespace Underworld
{
    /// <summary>
    /// For toggling and triggering a switch in the specified tile
    /// </summary>
    public class a_hack_trap_usebutton : hack_trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            var tile = UWTileMap.current_tilemap.Tiles[triggerX, triggerY];
            var foundbutton = objectsearch.FindMatchInObjectListChainNextObjectsOnly(
                ListHeadIndex: tile.indexObjectList, 
                majorclass: 5, 
                minorclass: 3, 
                classindex: -1, 
                objList: UWTileMap.current_tilemap.LevelObjects);
            if (foundbutton!=null)
            {
                //toggle button
                if (button.TryAndUse(foundbutton, trapObj.owner))
                {
                    trigger.TriggerObjectLink(
                        character: 1, 
                        ObjectUsed: foundbutton, 
                        triggerType: (int)triggerObjectDat.triggertypes.USE,
                        triggerX: triggerX, 
                        triggerY: triggerY, 
                        objList: UWTileMap.current_tilemap.LevelObjects);
                }
            }
        }
    }//end class
}//end namespace