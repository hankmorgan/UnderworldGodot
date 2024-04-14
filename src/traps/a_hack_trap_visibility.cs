namespace Underworld
{

    /// <summary>
    /// Trap that creates an item in Britannia that suits the player skillset
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
                //hide object
                if (ObjToChange.instance!=null)
                {
                    ObjToChange.instance.uwnode.QueueFree();
                }
            }
            else
            {
                //show object
                if (ObjToChange.instance==null)
                {
                    ObjectCreator.RenderObject(ObjToChange, UWTileMap.current_tilemap);
                }
            }
        }
    }//end class
}//end namespace
