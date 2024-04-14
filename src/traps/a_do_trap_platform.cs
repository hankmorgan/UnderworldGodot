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
        public static void Activate(uwObject trapObj, uwObject triggerObj, uwObject[] objList)
        {
            var tileX = triggerObj.quality;
            var tileY = triggerObj.owner;
            var startObject = objList[ObjectThatStartedChain];
            if (startObject != null)
            {
                //Find Remove existing tile
                UWTileMap.RemoveTile(tileX, tileY);

                //Set the new height
                var newHeight = heights[startObject.flags];
                Debug.Print($"Flags is {startObject.flags} new height is {newHeight}");
                var t = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                t.floorHeight = (short)newHeight;

                //Render new tile
                trigger.DoRedraw = true;
                t.Redraw = true;
                //tileMapRender.RenderTile(tileMapRender.worldnode, tileX, tileY, t);

                //TODO find objects in the tile that may need to be moved.
            }
        }
    } //end class
}//end namespace