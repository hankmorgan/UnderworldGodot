using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Trap which changes the owner of the linked object.
    /// </summary>
    public class a_hack_trap_owner : hack_trap
    {
        public static void Activate(uwObject trapObj)
        {
            if ((trapObj.link!=0) && (trapObj.link<=1024))
            {
                var obj = UWTileMap.current_tilemap.LevelObjects[trapObj.link];
                if (obj!=null)
                {
                    Debug.Print($"Setting {obj.index} {obj.a_name} owner to {trapObj.owner}");
                    obj.owner = trapObj.owner;
                    //Force a redraw now since this is usually used on TMAPS
                    objectInstance.RedrawFull(obj);
                }
            }
        }
    }//end class
    
}//end namespace