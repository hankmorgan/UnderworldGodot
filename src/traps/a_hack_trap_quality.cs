namespace Underworld
{
    /// <summary>
    /// Trap which changes the quality of the linked object.
    /// </summary>
    public class a_hack_trap_quality : hack_trap
    {
        public static void Activate(uwObject trapObj)
        {
            if ((trapObj.link!=0) && (trapObj.link<=1024))
            {
                var obj = UWTileMap.current_tilemap.LevelObjects[trapObj.link];
                if (obj!=null)
                {
                    obj.quality = trapObj.owner;
                }
            }
        }
    }//end class
    
}//end namespace