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
            uimanager.instance.AutomapImage.Texture = AutomapRender.MapImage(playerdat.dungeon_level-1);
            uimanager.EnableDisable(uimanager.instance.AutomapPanel,true);
            return false;
        }
    }//end class
}//end namespace