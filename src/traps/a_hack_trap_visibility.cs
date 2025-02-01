using System.Diagnostics;

namespace Underworld
{

    /// <summary>
    /// Trap that shows or hides a target object.
    /// </summary>
    public class a_hack_trap_visibility : trap
    {
        public static void Activate(uwObject trapObj)
        {
            var ObjToChange = UWTileMap.current_tilemap.LevelObjects[trapObj.link];
            var newVis = (short)(trapObj.owner & 0x1);

            ObjToChange.invis = newVis;
            if (ObjToChange.invis==1)
            {
                Debug.Print($"Hiding {ObjToChange.a_name} {ObjToChange.index}");
                //hide object
                if (ObjToChange.instance!=null)
                {
                    ObjToChange.instance.uwnode.QueueFree();
                }
            }
            else
            {
                //show object
                Debug.Print($"Showing {ObjToChange.a_name} {ObjToChange.index}");
                objectInstance.RedrawFull(ObjToChange);
            }
        }
    }//end class
}//end namespace
