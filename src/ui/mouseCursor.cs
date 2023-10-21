using Godot;
using System;

namespace Underworld
{
	/// <summary>
	/// The mouse cursor in UW
	/// </summary>
	public partial class mouseCursor : uwTextureRect
	{
		// Called when the node enters the scene tree for the first time.

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			var offset = new Vector2(Texture.GetWidth()/1f, Texture.GetHeight()/1f);
			var pos = GetViewport().GetMousePosition() - offset;
			this.Position = pos;
		}

		public void InitCursor()
		{
			Texture = uwUI.grCursors.LoadImageAt(0);
			Input.MouseMode = Input.MouseModeEnum.Hidden;
			//PivotOffset = new Vector2(Texture.GetWidth()/2f, Texture.GetHeight()/2f);
		}
	}

}//end namespace