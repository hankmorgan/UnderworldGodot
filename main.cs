using Godot;
using System;
using System.Diagnostics;
using Underworld;

/// <summary>
/// Node to initialise the game
/// </summary>
public partial class main : Node3D
{

	/// <summary>
	/// Blocks input for certain modes
	/// </summary>
	public static bool blockmouseinput
	{
		get
		{
			return
			 ConversationVM.InConversation
			 ||
			 uimanager.InAutomap
			 ||
			 MessageDisplay.WaitingForTypedInput
			 ||
			 MessageDisplay.WaitingForMore
			 ||
			 MessageDisplay.WaitingForYesOrNo
			 ||
			 musicalinstrument.PlayingInstrument
			 ||
			 uimanager.InteractionMode == uimanager.InteractionModes.ModeOptions			 
			 ;

			; //TODO and other menu modes that will stop input
		}
	}
	public static main instance;

	// Called when the node enters the scene tree for the first time.
	[Export] public Camera3D cam;
	public static Camera3D gamecam; //static ref to the above camera
	[Export] public AudioStreamPlayer audioplayer;
	[Export] public RichTextLabel lblPositionDebug;
	[Export] public uimanager uwUI;

	[Export] public SubViewport secondarycameras;


	double gameRefreshTimer = 0f;
	double cycletime = 0;

	public override void _Ready()
	{
		GetTree().DebugCollisionsHint = true;
		instance = this;
		gamecam = cam;
		uwsettings.LoadSettings();

		ObjectCreator.grObjects = new GRLoader(GRLoader.OBJECTS_GR, GRLoader.GRShaderMode.BillboardSpriteShader);
		ObjectCreator.grObjects.UseRedChannel = true;
		ObjectCreator.grObjects.UseCropping = true;
		//ObjectCreator.grObjects.GenerateCollision = false;//not working for now..
		Palette.CurrentPalette = 0;

		uwUI.InitUI();

		uimanager.AddToMessageScroll(GameStrings.GetString(1, 13));//welcome message

		// playerdat.LoadPlayerDat(datafolder: uwsettings.instance.levarkfolder);
	}


	/// <summary>
	/// Draws a debug marker sprite on game load to show where the character is positioned
	/// </summary>
	/// <param name="gr"></param>
	public static void DrawPlayerPositionSprite(GRLoader gr)
	{
		int spriteNo = 127;
		var a_sprite = new MeshInstance3D(); //new Sprite3D();
		a_sprite.Name = "player";
		a_sprite.Mesh = new QuadMesh();
		Vector2 NewSize;
		var img = gr.LoadImageAt(spriteNo);
		if (img != null)
		{
			a_sprite.Mesh.SurfaceSetMaterial(0, gr.GetMaterial(spriteNo));
			NewSize = new Vector2(
					ArtLoader.SpriteScale * img.GetWidth(),
					ArtLoader.SpriteScale * img.GetHeight()
					);
			a_sprite.Mesh.Set("size", NewSize);
			Node3D worldobjects = main.instance.GetNode<Node3D>("/root/Underworld/worldobjects");
			worldobjects.AddChild(a_sprite);
			a_sprite.Position = main.gamecam.Position;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (uimanager.InGame)
		{
			int tileX = -(int)(cam.Position.X / 1.2f);
			int tileY = (int)(cam.Position.Z / 1.2f);
			int xposvecto = -(int)(((cam.Position.X % 1.2f) / 1.2f) * 8);
			int yposvecto = (int)(((cam.Position.Z % 1.2f) / 1.2f) * 8);
			var tmp = cam.Rotation;
			tmp.Y = (float)(tmp.Y - Math.PI);
			playerdat.heading = (int)Math.Round(-(tmp.Y * 127) / Math.PI);
			uimanager.UpdateCompass();

			lblPositionDebug.Text = $"{cam.Position.ToString()}\nL:{playerdat.dungeon_level} X:{tileX} Y:{tileY}\n{uimanager.instance.uwsubviewport.GetMousePosition()}\n{cam.Rotation} {playerdat.heading} {(playerdat.heading >> 4) % 4} {xposvecto} {yposvecto}";

			if ((tileX < 64) && (tileX >= 0) && (tileY < 64) && (tileY >= 0))
			{
				if ((playerdat.tileX != tileX) || (playerdat.tileY != tileY))
				{
					playerdat.tileX = tileX;
					playerdat.tileY = tileY;
					playerdat.xpos = xposvecto;
					playerdat.ypos = yposvecto;
					//playerdat.zpos = 
					//uwsettings.instance.lightlevel = light.BrightestLight();
					//playerdat.lightlevel = light.BrightestLight();
					playerdat.PlayerStatusUpdate();
				}
				
			}
		}

		cycletime += delta;
		if (cycletime > 0.2)
		{
			cycletime = 0;
			PaletteLoader.UpdatePaletteCycles();
		}
		if (uimanager.InGame)
		{
			gameRefreshTimer += delta;
			if (gameRefreshTimer >= 0.3)
			{
				gameRefreshTimer = 0;
				if (!blockmouseinput)
				{
					npc.UpdateNPCs();
					AnimationOverlay.UpdateAnimationOverlays();
				}
			}

			if ((MessageDisplay.WaitingForTypedInput) || (MessageDisplay.WaitingForYesOrNo))
			{
				if (!uimanager.instance.TypedInput.HasFocus())
				{
					uimanager.instance.TypedInput.GrabFocus();
				}
				uimanager.instance.scroll.UpdateMessageDisplay();
			}
		}

	}

	public override void _Input(InputEvent @event)
	{
		if ((@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
			&&
			((eventMouseButton.ButtonIndex == MouseButton.Left) || (eventMouseButton.ButtonIndex == MouseButton.Right)))
		{
			if (MessageDisplay.WaitingForMore)
			{
				Debug.Print("End wait due to click");
				MessageDisplay.WaitingForMore = false;
				return; //don't process any more clicks here.
			}
			if (!blockmouseinput)
			{
				if (uimanager.IsMouseInViewPort())
					uimanager.ClickOnViewPort(eventMouseButton);
			}
		}

		if (!blockmouseinput)
		{
			if (@event is InputEventKey keyinput)
			{
				if (keyinput.Pressed)
				{
					switch (keyinput.Keycode)
					{
						case Key.T:
							var mouselook = (bool)gamecam.Get("MOUSELOOK");
							if (mouselook)
							{
								//Input.set_mouse_mode(Input.MOUSE_MODE_HIDDEN)
								Input.MouseMode = Input.MouseModeEnum.Hidden;
							}
							else
							{
								//Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
								Input.MouseMode = Input.MouseModeEnum.Captured;								
							}
							gamecam.Set("MOUSELOOK",!mouselook);
							break;
					}
				}
			}
		}

		if (ConversationVM.WaitingForInput
			&& !uimanager.MessageScrollIsTemporary
			&& !MessageDisplay.WaitingForTypedInput)
		{
			if (@event is InputEventKey keyinput)
			{
				if (keyinput.Pressed)
				{
					if (int.TryParse(keyinput.AsText(), out int result))
					{
						if ((result > 0) && (result <= ConversationVM.MaxAnswer))
						{
							ConversationVM.PlayerNumericAnswer = result;
							ConversationVM.WaitingForInput = false;
						}
					}
				}
			}
		}

		if (MessageDisplay.WaitingForMore)
		{
			if (@event is InputEventKey keyinput)
			{
				Debug.Print("End wait due to key inputclick");
				MessageDisplay.WaitingForMore = false;
			}
		}

		if (MessageDisplay.WaitingForTypedInput)
		{
			if (@event is InputEventKey keyinput)
			{
				if (keyinput.Pressed)
				{
					bool stop = false;
					switch (keyinput.Keycode)
					{
						case Key.Enter:
							stop = true;
							break;
						case Key.Escape:
							stop = true;
							uimanager.instance.TypedInput.Text = "";
							break;
					}
					if (stop)
					{//end typed input
						uimanager.instance.scroll.Clear();
						MessageDisplay.WaitingForTypedInput = false;
						if (ConversationVM.InConversation == false)
						{
							gamecam.Set("MOVE", true);//re-enable movement
						}
					}
				}

			}
		}

		if (musicalinstrument.PlayingInstrument)
		{
			if (@event is InputEventKey keyinput)
			{
				if (keyinput.Pressed)
				{
					switch (keyinput.Keycode)
					{
						case >= Key.Key0 and <= Key.Key9:
						case >= Key.Kp0 and <= Key.Kp9:
							musicalinstrument.notesplayed += keyinput.AsText();
							Debug.Print($"Imagine musical note {keyinput.AsText()}");
							break;
						case Key.Escape:
							musicalinstrument.StopPlaying();
							break;
					}
				}
			}
		}

		if (MessageDisplay.WaitingForYesOrNo)
		{
			if (@event is InputEventKey keyinput)
			{
				if (keyinput.Pressed)
				{
					bool stop = false;
					switch (keyinput.Keycode)
					{
						case Key.Enter:
							stop = true;
							break;
						case Key.Escape:
							stop = true;
							uimanager.instance.TypedInput.Text = "No";
							break;
						case Key.Y:
							uimanager.instance.TypedInput.Text = "Yes"; break;
						default:
							uimanager.instance.TypedInput.Text = "No"; break;
					}
					if (stop)
					{//end typed input
						uimanager.instance.scroll.Clear();
						MessageDisplay.WaitingForYesOrNo = false;
						gamecam.Set("MOVE", true);//re-enable movement
					}
				}
			}
		}
	}
}//end class
