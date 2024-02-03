using System;
using System.Diagnostics;
using Godot;

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

        
 
		public static bool Fullscreen = false;

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

    }//end class
}//end namespace