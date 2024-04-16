namespace Underworld
{
    public class key_of_infinity : objectInstance
    {

        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (!WorldObject)
            {
                //flag we are using the key of infinity
                useon.CurrentItemBeingUsed = new useon(obj, WorldObject);
                //print use message
                uimanager.AddToMessageScroll(useon.CurrentItemBeingUsed.GeneralUseOnString);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool UseOn(uwObject keyobject, uwObject itemTargeted, bool WorldObject)
        {
            if (!WorldObject){return false;}
            if (itemTargeted.link != 0)
            {
                trigger.TriggerObjectLink(character:0, 
                    ObjectUsed: itemTargeted, 
                    triggerType: (int)triggerObjectDat.triggertypes.USE,
                    triggerX: itemTargeted.tileX,
                    triggerY: itemTargeted.tileY, 
                    objList: UWTileMap.current_tilemap.LevelObjects);
                //trigger.OpenTrigger(itemTargeted, itemTargeted.link, UWTileMap.current_tilemap.LevelObjects);
            }
            return true;
        }
    }//end class
}//end namespace