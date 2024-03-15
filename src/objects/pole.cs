namespace Underworld
{
    public class pole : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (!WorldObject)
            {
                //flag we are using the pole
                useon.CurrentItemBeingUsed = new useon(obj, WorldObject);
                //print use message
                uimanager.AddToMessageScroll(useon.CurrentItemBeingUsed.GeneralUseOnString);

                playerdat.usingpole = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool UseOn(uwObject pole, uwObject itemTargeted, bool WorldObject)
        {
            if (WorldObject)
            {
                playerdat.usingpole = false;
                useon.CurrentItemBeingUsed = null; //stops infinite loop in use function
                if (itemTargeted.OneF0Class == 0x17)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_using_the_pole_you_trigger_the_switch_));
                    return use.Use(itemTargeted.index,UWTileMap.current_tilemap.LevelObjects, WorldObject);
                }     
                else
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_the_pole_cannot_be_used_on_that_));                   
                }           
            }
            return false;
        }
    }//end class
}//end namespace