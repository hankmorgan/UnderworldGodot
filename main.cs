using Godot;
using System;
using System.Diagnostics;
using Underworld;
using Peaky.Coroutines;
using System.Collections;

/// <summary>
/// Node to initialise the game
/// </summary>
public partial class main : Node3D
{

	static bool EnablePositionDebug = false;
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
	//[Export] public uimanager uwUI;

	[Export] public SubViewport secondarycameras;

	double gameRefreshTimer = 0f;
	double cycletime = 0;

	/// <summary>
	/// To prevent teleporting again when the teleport destination in inside a teleport trap
	/// </summary>
	public static bool JustTeleported;
	public static int TeleportLevel = -1;
	public static int TeleportTileX = -1;
	public static int TeleportTileY = -1;

	public static bool DoRedraw = false;

	public override void _Ready()
	{
		instance = this;
		gamecam = cam;

		//uimanager.instance = uwUI;	
		if (uwsettings.instance != null)
		{
			GetTree().DebugCollisionsHint = uwsettings.instance.showcolliders;
		}

	}

	public static void StartGame()
	{
		if (gamecam == null)
		{
			if (instance.cam == null)
			{
				Debug.Print("Main Cam instance is null. trying to find it's node");
				instance.cam = (Camera3D)instance.GetNode("/root/Underworld/WorldViewContainer/SubViewport/Camera3D");
			}
			gamecam = instance.cam;
			if (gamecam == null)
			{
				Debug.Print("Gamecam is still null!");
			}
		}
		if (uimanager.instance == null)
		{
			Debug.Print("UI Manager is null");
			//UI/uiManager
			uimanager.instance = (uimanager)instance.GetNode("/root/Underworld/UI/uiManager");
			if (uimanager.instance == null)
			{
				Debug.Print("UIManager is still null!!");
			}
		}
		gamecam.Fov = Math.Max(50, uwsettings.instance.FOV);
		uimanager.EnableDisable(instance.lblPositionDebug, EnablePositionDebug);
		uimanager.EnableDisable(uimanager.instance.StartMenuPanel, false);
		ObjectCreator.grObjects = new GRLoader(GRLoader.OBJECTS_GR, GRLoader.GRShaderMode.BillboardSpriteShader);
		ObjectCreator.grObjects.UseRedChannel = true;
		ObjectCreator.grObjects.UseCropping = true;
		Palette.CurrentPalette = 0;
		uimanager.instance.InitUI();
		uimanager.AddToMessageScroll(GameStrings.GetString(1, 13));//welcome message
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
		if ((uimanager.InGame) || (uimanager.AtMainMenu))
		{
			cycletime += delta;
			if (cycletime > 0.2)
			{
				cycletime = 0;
				PaletteLoader.UpdatePaletteCycles();
			}
		}

		if (uimanager.InGame)
		{
			RefreshWorldState();//handles teleports, tile redraws

			int tileX = -(int)(cam.Position.X / 1.2f);
			int tileY = (int)(cam.Position.Z / 1.2f);
			int xposvecto = -(int)(((cam.Position.X % 1.2f) / 1.2f) * 8);
			int yposvecto = (int)(((cam.Position.Z % 1.2f) / 1.2f) * 8);
			var tmp = cam.Rotation;
			tmp.Y = (float)(tmp.Y - Math.PI);
			playerdat.heading = (int)Math.Round(-(tmp.Y * 127) / Math.PI);
			uimanager.UpdateCompass();
			combat.CombatInputHandler(delta);
			playerdat.PlayerTimedLoop(delta);
			if (EnablePositionDebug)
			{
				var fps = Engine.GetFramesPerSecond();
				lblPositionDebug.Text = $"FPS:{fps} Time:{playerdat.game_time}\nL:{playerdat.dungeon_level} X:{tileX} Y:{tileY}\n{uimanager.instance.uwsubviewport.GetMousePosition()}\n{cam.Rotation} {playerdat.heading} {(playerdat.heading >> 4) % 4} {xposvecto} {yposvecto}";
			}



			if ((tileX < 64) && (tileX >= 0) && (tileY < 64) && (tileY >= 0))
			{
				if ((playerdat.tileX != tileX) || (playerdat.tileY != tileY))
				{
					if (UWClass._RES == UWClass.GAME_UW2)
					{
						//find exit triggers.
						var exittrigger = UWTileMap.current_tilemap.Tiles[playerdat.tileX, playerdat.tileY].ExitTrigger;
						if (exittrigger != 0)
						{
							var exittriggerobj = UWTileMap.current_tilemap.LevelObjects[exittrigger];
							//trigger.ExitTrigger(null, entertrigger, UWTileMap.current_tilemap.LevelObjects);
							trigger.RunTrigger(character: 0,
										ObjectUsed: exittriggerobj,
										TriggerObject: exittriggerobj,
										triggerType: (int)triggerObjectDat.triggertypes.EXIT,
										objList: UWTileMap.current_tilemap.LevelObjects);
						}
					}
					playerdat.tileX = tileX;
					playerdat.tileY = tileY;
					playerdat.xpos = xposvecto;
					playerdat.ypos = yposvecto;

					//tmp update the player object to keep in sync with other values
					playerdat.playerObject.xpos = (short)playerdat.xpos;
					playerdat.playerObject.ypos = (short)playerdat.ypos;
					playerdat.playerObject.tileX = tileX;
					playerdat.playerObject.tileY = tileY;

					playerdat.PlayerStatusUpdate();
					if (UWClass._RES == UWClass.GAME_UW2)
					{
						//find enter triggers.
						var entertrigger = UWTileMap.current_tilemap.Tiles[playerdat.tileX, playerdat.tileY].EnterTrigger;
						if (entertrigger != 0)
						{
							var entertriggerobj = UWTileMap.current_tilemap.LevelObjects[entertrigger];
							//trigger.EnterTrigger(null, entertrigger, UWTileMap.current_tilemap.LevelObjects);
							trigger.RunTrigger(character: 0,
										ObjectUsed: entertriggerobj,
										TriggerObject: entertriggerobj,
										triggerType: (int)triggerObjectDat.triggertypes.ENTER,
										objList: UWTileMap.current_tilemap.LevelObjects);
						}
					}
				}
			}

			gameRefreshTimer += delta;
			if (gameRefreshTimer >= 0.3)
			{
				gameRefreshTimer = 0;
				if (!blockmouseinput)
				{
					for (int i=0;i<UWTileMap.current_tilemap.NoOfActiveMobiles;i++)
					{
						var index = UWTileMap.current_tilemap.GetActiveMobileAtIndex(i);
						if ((index!=0) && (index<256))
						{
							var obj = UWTileMap.current_tilemap.LevelObjects[index];
							if (obj.majorclass==1)
							{
								npc.NPCInitialProcess(obj);
							}
							else
							{
								//TODO this is a projectile
							}
						}
					}					
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
		if ((@event is InputEventMouseButton eventMouseButton)
			&&
			((eventMouseButton.ButtonIndex == MouseButton.Left) || (eventMouseButton.ButtonIndex == MouseButton.Right)))
		{
			if (eventMouseButton.Pressed)
			{
				if (MessageDisplay.WaitingForMore)
				{
					//Debug.Print("End wait due to click");
					MessageDisplay.WaitingForMore = false;
					return; //don't process any more clicks here.
				}
				if (!blockmouseinput)
				{
					if (uimanager.IsMouseInViewPort())
					{
						uimanager.ClickOnViewPort(eventMouseButton);
					}

				}
			}
		}


		if ((!blockmouseinput) && (uimanager.InGame))
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
							{//toggle to free curso
								Input.MouseMode = Input.MouseModeEnum.Hidden;
							}
							else
							{//toogle to mouselook
								Input.MouseMode = Input.MouseModeEnum.Captured;
							}
							gamecam.Set("MOUSELOOK", !mouselook);
							break;
						case Key.F1: //open options menu
							uimanager.InteractionModeToggle(0); break;
						case Key.F2: //talk
							uimanager.InteractionModeToggle((uimanager.InteractionModes)1); break;
						case Key.F3://pickup
							uimanager.InteractionModeToggle((uimanager.InteractionModes)2); break;
						case Key.F4://look
							uimanager.InteractionModeToggle((uimanager.InteractionModes)3); break;
						case Key.F5://attack
							uimanager.InteractionModeToggle((uimanager.InteractionModes)4); break;
						case Key.F6://use
							uimanager.InteractionModeToggle((uimanager.InteractionModes)5); break;
						case Key.F7://toggle panel
							uimanager.ChangePanels(); break;
						case Key.F8: //cast spell
							RunicMagic.CastRunicSpell(); break;
						case Key.F9://track skill
							Debug.Print("Track"); break;
						case Key.F10: // make camp 
							Debug.Print("Make camp"); break;
						case Key.F11://toggle position label
							{
								EnablePositionDebug = !EnablePositionDebug;
								uimanager.EnableDisable(lblPositionDebug, EnablePositionDebug);
								break;
							}
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

	/// <summary>
	/// Handles the end of chain events.
	/// </summary>
	public static void RefreshWorldState()
	{
		if (DoRedraw)
		{
			//update tile faces
			UWTileMap.SetTileMapWallFacesUW();
			//Handle tile changes after all else is done
			foreach (var t in UWTileMap.current_tilemap.Tiles)
			{
				if (t.Redraw)
				{
					UWTileMap.RemoveTile(t.tileX, t.tileY);
					tileMapRender.RenderTile(tileMapRender.worldnode, t.tileX, t.tileY, t);
					t.Redraw = false;
				}
			}
		}

		//Handle level transitions now since it's possible for further traps to be called after the teleport trap
		if (TeleportLevel != -1)
		{
			int itemToTransfer = -1;
			if (playerdat.ObjectInHand != -1)
			{//handle moving an object in hand through levels. Temporarily add to inventory data.
				itemToTransfer = playerdat.AddObjectToPlayerInventory(playerdat.ObjectInHand, false);
			}
			playerdat.dungeon_level = TeleportLevel;
			//switch level
			UWTileMap.LoadTileMap(
					newLevelNo: playerdat.dungeon_level - 1,
					datafolder: playerdat.currentfolder,
					newGameSession: false);

			if (itemToTransfer != -1)
			{//takes object back out of inventory.
				uimanager.DoPickup(itemToTransfer);
			}
		}
		if ((TeleportTileX != -1) && (TeleportTileY != -1))
		{
			//move to new tile
			var targetTile = UWTileMap.current_tilemap.Tiles[TeleportTileX, TeleportTileY];
			playerdat.zpos = targetTile.floorHeight << 3;
			playerdat.xpos = 3; playerdat.ypos = 3;
			playerdat.tileX = TeleportTileX; playerdat.tileY = TeleportTileY;
			main.gamecam.Position = uwObject.GetCoordinate(
				tileX: playerdat.tileX,
				tileY: playerdat.tileY,
				_xpos: playerdat.xpos,
				_ypos: playerdat.ypos,
				_zpos: playerdat.camerazpos);
		}

		if ((TeleportTileX != -1) || (TeleportTileY != -1) || (TeleportLevel != -1))
		{
			JustTeleported = true;
			_ = Peaky.Coroutines.Coroutine.Run(
			PauseTeleport(),
			main.instance
			);
		}
		TeleportLevel = -1;
		TeleportTileX = -1;
		TeleportTileY = -1;
	}


	/// <summary>
	/// Puts a block on sucessive level transitions due to teleport placing player in a new move trigger
	/// </summary>
	/// <returns></returns>
	public static IEnumerator PauseTeleport()
	{
		JustTeleported = true;
		yield return new WaitForSeconds(1);
		JustTeleported = false;
		yield return 0;
	}

}//end class
