using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Trap which moves a platform up and down
    /// </summary>
    public class a_do_trap_platform : hack_trap
    {
        /// <summary>
        /// The switch flags maps to these tile heights
        /// </summary>
        static int[] heights = new int[] { 4, 6, 8, 10, 12, 14, 16, 18 }; //heights *2
        public static void Activate(uwObject trapObj, uwObject ObjectUsed, int triggerX, int triggerY, uwObject[] objList)
        {
            //var tileX = triggerX;//triggerObj.quality;
            //var tileY = triggerY;//triggerObj.owner;
            //var startObject = objList[ObjectThatStartedChain];
            if (ObjectUsed != null)
            {
                //Find Remove existing tile
                //UWTileMap.RemoveTile(triggerX, triggerY);

                //Set the new height
                var newHeight = heights[ObjectUsed.flags];
                Debug.Print($"Flags is {ObjectUsed.flags} new height is {newHeight}");
                var t = UWTileMap.current_tilemap.Tiles[triggerX, triggerY];
                t.floorHeight = (short)newHeight;

                //Render new tile
                main.DoRedraw = true;
                t.Redraw = true;

                //TODO find objects in the tile that may need to be moved.
            }
        }
    } //end class
}//end namespace