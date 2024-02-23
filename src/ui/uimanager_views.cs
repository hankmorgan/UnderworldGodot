using System.Diagnostics;
using Godot;
using Godot.Collections;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("View")]
        [Export] public Camera3D cam;
        //[Export] public Node3D freelook;

        [Export] public SubViewportContainer uwviewport;
        [Export] public SubViewport uwsubviewport;

        [Export] public mouseCursor mousecursor;
        [Export] public CanvasLayer uw1UI;
        [Export] public CanvasLayer uw2UI;

        [Export] public TextureRect mainwindowUW1;
        [Export] public TextureRect mainwindowUW2;


        public static bool Fullscreen = false; //someday

        private void InitViews()
        {
            switch (UWClass._RES)
            {
                case UWClass.GAME_UW2:
                    mainwindowUW2.Texture = bitmaps.LoadImageAt(BytLoader.UW2ThreeDWin_BYT, true);
                    if (!Fullscreen)
                    {
                        uwviewport.SetSize(new Vector2(840f, 512f));
                        uwviewport.Position = new Vector2(62f, 62f);
                        uwsubviewport.Size = new Vector2I(840, 512);
                    }
                    break;
                default:
                    mainwindowUW1.Texture = bitmaps.LoadImageAt(BytLoader.MAIN_BYT, true);
                    if (!Fullscreen)
                    {
                        uwviewport.SetSize(new Vector2(700f, 456f));
                        uwviewport.Position = new Vector2(200f, 72f);
                        uwsubviewport.Size = new Vector2I(700, 456);
                    }
                    break;
            }
        }


        /// <summary>
		/// Checks if the mouse cursor is over the viewport
		/// </summary>
		/// <returns></returns>
		public static bool IsMouseInViewPort()
        {
            var viewportmouspos = instance.uwsubviewport.GetMousePosition();
            if (
                (viewportmouspos.X >= 0) && (viewportmouspos.Y >= 0)
                &&
                (viewportmouspos.X <= instance.uwsubviewport.Size.X) && (viewportmouspos.Y <= instance.uwsubviewport.Size.Y)
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public static void ClickOnViewPort(InputEventMouseButton eventMouseButton)
        {
            //bool LeftClick = (eventMouseButton.ButtonIndex == MouseButton.Left); //else treat it as a right click.
                                                                                 //float RayLength = 3.0f;
            Dictionary result = DoRayCast(eventMouseButton, RayDistance);
            if (result != null)
            {
                if (result.ContainsKey("collider") && result.ContainsKey("normal") && result.ContainsKey("position"))
                {
                    var obj = (StaticBody3D)result["collider"];
                    var normal = (Vector3)result["normal"];
                    var pos = (Vector3)result["position"];
                    Debug.Print(obj.Name);
                    string[] vals = obj.Name.ToString().Split("_");

                    switch (vals[0].ToUpper())
                    {
                        case "TILE":
                        case "WALL":
                            {
                                switch (InteractionMode)
                                {
                                    case InteractionModes.ModePickup:
                                        {
                                            if (playerdat.ObjectInHand != -1)
                                            {//something is held. try and drop it on this tile.
                                             //int tileX = int.Parse(vals[1]); int tileY = int.Parse(vals[2]);

                                                int tileX = (int)(-pos.X / 1.2f);
                                                int tileY = (int)(pos.Z / 1.2f);
                                                //move object to this tile if possble
                                                DropToTileAtPosition(pos, tileX, tileY);
                                            }
                                            break;
                                        }
                                    case InteractionModes.ModeLook:
                                        {//Look at tile
                                            int tileX = int.Parse(vals[1]); int tileY = int.Parse(vals[2]);
                                            LookAtTile(normal, tileX, tileY);
                                            break;
                                        }
                                }
                                break;
                            }
                        case "CEILING":
                            {
                                switch (InteractionMode)
                                {
                                    case InteractionModes.ModeLook:
                                        {
                                            LookAtCeiling();
                                            break;
                                        }
                                }
                                break;
                            }

                        default: //check for regular collider with an obj.
                            {
                                if (int.TryParse(vals[0], out int index))
                                {
                                    InteractWithObjectCollider(
                                        index: index);
                                }
                            }
                            break;
                    }
                }
            }
            else
            {
                //no match on the raycast. if holding an object in pickup mode this should try and throw the object
                //along the ray
            }
        }

        /// <summary>
        /// Drops an object at the clicked on tile position.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        private static void DropToTileAtPosition(Vector3 pos, int tileX, int tileY)
        {
            if(
                pickup.Drop(
                index: playerdat.ObjectInHand,
                objList: UWTileMap.current_tilemap.LevelObjects,
                dropPosition: pos,
                tileX: tileX, tileY: tileY)
            )
            {
                playerdat.ObjectInHand=-1;
                uimanager.instance.mousecursor.ResetCursor();
            }
        }


        /// <summary>
        /// Handles looking at the ceiling
        /// </summary>
        private static void LookAtCeiling()
        {
            AddToMessageScroll(
                GameStrings.GetString(1, GameStrings.str_you_see_)
                +
                GameStrings.GetString(10, 511)
                );
        }


        /// <summary>
        /// Handles looking at a tile on a surface determined from the surface normal;
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        private static void LookAtTile(Vector3 normal, int tileX, int tileY)
        {
            AddToMessageScroll(
                GameStrings.GetString(1, GameStrings.str_you_see_)
                +
                TileInfo.GetTileSurfaceDescription(normal, tileX, tileY));
        }

    }//end class
}//end namespace