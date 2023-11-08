using System.Diagnostics;
using Godot;

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
        public static void activate(uwObject trapObj, uwObject triggerObj, uwObject[] objList)
        {
            var tileX = triggerObj.quality;
            var tileY = triggerObj.owner;
            var startObject = objList[trap.ObjectThatStartedChain];
            if (startObject != null)
            {                                
                //Find Remove existing tile
                string TileName = "Tile_" + tileX.ToString("D2") + "_" + tileY.ToString("D2");
                //var existingTile = tileMapRender.worldnode.FindChild(TileName);
                Node3D existingTile = tileMapRender.worldnode.GetNode<Node3D>($"/root/Node3D/tilemap/{TileName}");
                existingTile.Name = $"{TileName}_todestroy";
                existingTile.QueueFree();

                //Set the new height
                var newHeight = heights[startObject.flags];
                Debug.Print($"Flags is {startObject.flags} new height is {newHeight}");
                var t = TileMap.current_tilemap.Tiles[tileX, tileY];
                t.floorHeight = (short)newHeight;
                
                //Render new tile
                tileMapRender.RenderTile(tileMapRender.worldnode, tileX, tileY, t);
            }
        }
    } //end class


}//end namespace