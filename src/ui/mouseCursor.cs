using Godot;

namespace Underworld
{
	/// <summary>
	/// The mouse cursor in UW
	/// </summary>
	public partial class mouseCursor : TextureRect
	{
		[Export] public TextureRect cross;
		int directionalCursorIndex = -1;
		bool directionalOwnsCursor = false;
		// private static Vector2 UW1CursorPosition = new Vector2(552, 296);
		// private static Vector2 UW2CursorPosition = new Vector2(474, 314);
		// private static Vector2 UW1SubCursorPosition = new Vector2(366, 238);
		// private static Vector2 UW2SubCursorPosition = new Vector2(420, 250);

		/// <summary>
		/// Display position of the cursor when in mouselook
		/// </summary>
		// public static Vector2 CursorPosition
		// {
		// 	get
		// 	{
		// 		if (UWClass._RES == UWClass.GAME_UW2)
		// 		{
		// 			return UW2CursorPosition;
		// 		}
		// 		else
		// 		{
		// 			return UW1CursorPosition;
		// 		}
		// 	}
		// }


		/// <summary>
		/// Position of the mouse cursor for raycasts when in mouselook
		/// </summary>
		// public static Vector2 CursorPositionSub
		// {
		// 	get
		// 	{
		// 		if (UWClass._RES == UWClass.GAME_UW2)
		// 		{
		// 			return UW2SubCursorPosition;
		// 		}
		// 		else
		// 		{
		// 			return UW1SubCursorPosition;
		// 		}
		// 	}
		// }

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (main.cameraPitchGimbal_world != null)
			{
				if (Texture != null)
				{
					if (uimanager.CurrentGameMode == uimanager.GameModes.AUTOMAP)
					{
						if (uimanager.CurrentAutomapAction == uimanager.automapactions.WRITING)
						{
							return; //don't move mouse while writing.
						}
					}
					UpdateDirectionalCursor();
					var offset = new Vector2(-Texture.GetWidth(), -Texture.GetHeight());
					if (main.MouseLookActive && uimanager.instance != null)
					{
						var viewCentre = uimanager.instance.uwviewport.GetGlobalRect().GetCenter();
						cross.Position = viewCentre - (cross.Texture.GetSize() * 0.5f);
						cross.Visible = true;
						Visible = false;
					}
					else
					{
						cross.Visible = false;
						Visible = true;
						var target = GetViewport().GetMousePosition();
						cross.Position = target;
						this.Position = target + offset;
					}
				}
			}
		}

		/// <summary>
		/// Shows the vanilla UW directional arrow / crosshair for the current
		/// position in the 3D view. Special cursors (spells, held objects) win.
		/// </summary>
		void UpdateDirectionalCursor()
		{
			if (!uimanager.InGame || uimanager.blockinput
				|| SpellCasting.currentSpell != null
				|| playerdat.ObjectInHand != -1)
			{
				ReleaseDirectionalCursor();
				return;
			}
			var desired = main.GetMouseCursorIndex();
			if (desired != directionalCursorIndex || !directionalOwnsCursor)
			{
				SetCursorToCursor(desired);
				directionalCursorIndex = desired;
				directionalOwnsCursor = true;
			}
		}

		/// <summary>
		/// Restores the default cursor when the 3D view no longer owns it, so a
		/// directional arrow never survives into menus, options or conversations.
		/// </summary>
		void ReleaseDirectionalCursor()
		{
			if (!directionalOwnsCursor) { return; }
			directionalOwnsCursor = false;
			directionalCursorIndex = -1;
			SetCursorToCursor(0);
		}

		public void InitCursor()
		{
			Texture = uimanager.grCursors.LoadImageAt(0);
			Input.MouseMode = Input.MouseModeEnum.Hidden;
			this.Size = Texture.GetSize() * 4;
		}

		public void SetCursorToObject(int index)
		{
			Texture = uimanager.grObjects.LoadImageAt(index);
			Material = uimanager.grObjects.GetMaterial(index);
			this.Size = Texture.GetSize() * 4;
			directionalOwnsCursor = false;
			directionalCursorIndex = -1;
		}

		public void SetCursorToCursor(int index = 0)
		{
			if (((index == 14) || (index == 12) || (index==13)) && (UWClass._RES == UWClass.GAME_UW2))
			{
				//Special case for the quill and other map cursors
				uimanager.grCursors.PaletteNo = 1;
			}
			else
			{
				uimanager.grCursors.PaletteNo = 0;
			}
			Texture = uimanager.grCursors.LoadImageAt(index);
			//Material= uimanager.grCursors.GetMaterial(index);//no shader is applied here
			Material = null;
			this.Size = Texture.GetSize() * 4;
			directionalOwnsCursor = false;
			directionalCursorIndex = -1;
		}
	}

}//end namespace