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
            if (_RES==GAME_UW2)
            {
                var worldno = worlds.GetWorldNo(playerdat.dungeon_level);
                uimanager.DrawAutoMap(playerdat.dungeon_level-1, worldno);
            }
            else
            {
                uimanager.DrawAutoMap(playerdat.dungeon_level-1, 0);
            }
            
            return false;
        }
    }//end class
}//end namespace