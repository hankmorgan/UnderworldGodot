using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Trap which controls the moving button puzzle in Loths Tomb entrance level
    /// </summary>
    public class a_hack_trap_resetzpos : hack_trap
    {
        public static void Activate(uwObject trapObj, int mode)
        {
            int counter = 0;
            while (true)
            {
                var ax = 0;
                if (mode != 0)
                {
                    ax = 1;
                }
                if (ax<counter)
                {
                    return;
                }
                else
                {
                    //Calculate which 2 of the set of 5 switches will be moved.
                    var tmp = trapObj.link;
                    tmp = (short)((tmp/5) * 5);
                    var link = trapObj.link + counter;
                    link = link % 5;
                    link = tmp + link;


                    var obj = UWTileMap.current_tilemap.LevelObjects[link];
                    
                    var si_zpos = trapObj.zpos;
                    if (obj.zpos <= si_zpos)
                    {
                        si_zpos = (short)(si_zpos + trapObj.owner);
                    }
                    Debug.Print($"Link is {link} {obj.a_name}. It's zpos is {obj.zpos} and is being set to {si_zpos} trap:{trapObj.index} z:{trapObj.zpos} o:{trapObj.owner}");
                    obj.zpos = si_zpos;
                    objectInstance.Reposition(obj);
                    counter++;
                }
            }
        }
    }//end class
}//end namespace