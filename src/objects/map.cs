namespace Underworld
{

    /// <summary>
    /// The map scroll
    /// </summary>
    public class map : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {            
            if (WorldObject){return false;}
            uimanager.DrawAutoMap(playerdat.dungeon_level-1);
            return false;
        }
    }//end class
}//end namespace