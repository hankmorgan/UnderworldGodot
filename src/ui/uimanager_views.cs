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

        public static float ViewPortMouseXPos;
        public static float ViewPortMouseYPos;

        public static float Window3DLeftBorder
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return 16f;
                }
                else
                {
                    return 52f;
                }
            }
        }

        public static int Window3DMaxX
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return 208;
                }
                else
                {
                    return 172;
                }
            }
        }

        public static int Window3DMaxY
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return 128;
                }
                else
                {
                    return 113;
                }
            }
        }

        public static int Window3DHeadingAdjust
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return 104;
                }
                else
                {
                    return 86;
                }
            }
        }

        public static int Window3DPitchAdjust
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return 64;
                }
                else
                {
                    return 56;
                }
            }
        }

        public static int Window3DDropThreshold
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return 42;
                }
                else
                {
                    return 37;
                }
            }
        }

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
            if (!InGame) { return; }
            bool LeftClick = (eventMouseButton.ButtonIndex == MouseButton.Left);
            Debug.Print($"{eventMouseButton.Position.X},{eventMouseButton.Position.Y}");
            Dictionary result = DoRayCast(eventMouseButton.Position, RayDistance, out Vector3 rayOrigin);

            //store these values for use in throw and spell casting events
            ViewPortMouseXPos = eventMouseButton.Position.X;
            ViewPortMouseYPos = eventMouseButton.Position.Y;

            if (result != null)
            {
                if (result.ContainsKey("collider") && result.ContainsKey("normal") && result.ContainsKey("position"))
                {
                    var obj = (StaticBody3D)result["collider"];
                    var normal = (Vector3)result["normal"];
                    var hitCoordinateEnd = (Vector3)result["position"];
                    var hitCoordinate = rayOrigin.Lerp(hitCoordinateEnd, 0.9f);
                    Debug.Print(obj.Name);
                    string[] vals = obj.Name.ToString().Split("_");

                    switch (vals[0].ToUpper())
                    {
                        case "TILE":
                        case "WALL":
                            {
                                if (SpellCasting.currentSpell != null)
                                {
                                    if (SpellCasting.currentSpell.SpellMajorClass == 5)
                                    {
                                        SpellCasting.CastCurrentSpellOnRayCastTarget(
                                            index: 0,
                                            objList: null,
                                            WorldObject: true);//not enough room to cast
                                        return;
                                    }
                                }
                                switch (InteractionMode)
                                {
                                    case InteractionModes.ModePickup:
                                        {
                                            //interaction removed from here to support new drop/throw methods.
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
                                if (SpellCasting.currentSpell != null)
                                {
                                    if (SpellCasting.currentSpell.SpellMajorClass == 5)
                                    {
                                        SpellCasting.CastCurrentSpellOnRayCastTarget(
                                            index: 0,
                                            objList: null,
                                            WorldObject: true);//not enough room to cast
                                        return;
                                    }
                                }
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
                                    if (SpellCasting.currentSpell == null)
                                    {
                                        if (!(InteractionMode == InteractionModes.ModePickup && playerdat.ObjectInHand != -1))//temp to allow drop/throw below to work
                                        {
                                            InteractWithObjectCollider(
                                                index: index, LeftClick: LeftClick);
                                            return;
                                        }

                                    }
                                    else
                                    {
                                        SpellCasting.CastCurrentSpellOnRayCastTarget(
                                            index: index,
                                            objList: UWTileMap.current_tilemap.LevelObjects,
                                            WorldObject: true);
                                    }
                                }
                            }
                            break;
                    }
                }
                else
                {
                    //along the ray
                    if (SpellCasting.currentSpell != null)
                    {
                        //try can cast the current spell if class 5
                        if (SpellCasting.currentSpell.SpellMajorClass == 5)
                        {
                            SpellCasting.CastMagicProjectile(playerdat.playerObject, SpellCasting.currentSpell.SpellMinorClass);
                            return;
                        }
                    }
                    else
                    {
                        //no match on the raycast. if holding an object in pickup mode this should try and throw the object
                    }
                }
            }


            //New methods that ignore the raycast
            switch (InteractionMode)
            {
                case InteractionModes.ModePickup:
                    {
                        if (playerdat.ObjectInHand != -1)
                        {
                            //something is held. try and drop or throw it
                            var objToThrow = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];
                            var itemid = objToThrow.item_id;
                            if (pickup.DropObjectByPlayer(objToThrow, true))
                            {
                                playerdat.ObjectInHand = -1;
                                instance.mousecursor.SetCursorToCursor();
                                pickup.DropSpecialCases(itemid);//primarily handle moonstones
                            }
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Drops an object at the clicked on tile position.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        private static void DropToTileAtPosition_OLD(Vector3 pos, int tileX, int tileY)
        {
            Debug.Print("To Remove DropToTileAtPosition_OLD()");
            if (
                pickup.Drop_old(
                index: playerdat.ObjectInHand,
                objList: UWTileMap.current_tilemap.LevelObjects,
                dropPosition: pos,
                tileX: tileX, tileY: tileY)
            )
            {
                playerdat.ObjectInHand = -1;
                instance.mousecursor.SetCursorToCursor();
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