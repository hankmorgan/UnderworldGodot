using Godot;

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
			Texture = uimanager.grCursors.LoadImageAt(0);
			Input.MouseMode = Input.MouseModeEnum.Hidden;
		}

		public void SetCursorArt(int index)
		{
			Texture = uimanager.grObjects.LoadImageAt(index);
			Material= uimanager.grObjects.GetMaterial(index);
		}

		public void ResetCursor()
		{
			Texture = uimanager.grCursors.LoadImageAt(0);
			Material= uimanager.grCursors.GetMaterial(0);
		}
	}

}//end namespace