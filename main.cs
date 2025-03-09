using Godot;
using System;
using System.Diagnostics;
using Underworld;


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

		// var exe = System.IO.File.ReadAllBytes("C:\\Games\\UW2\\uw2.exe");
		// int addr_ptr=0x690c0;
		// for (long x = 0; x<=320;x++)
		// {
		// 	Debug.Print($"{x}={(short)Loader.getAt(exe,addr_ptr,16)}");
		// 	addr_ptr+=2;
		// }
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
			Node3D worldobjects = instance.GetNode<Node3D>("/root/Underworld/worldobjects");
			worldobjects.AddChild(a_sprite);
			a_sprite.Position = gamecam.Position;
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
			tileX = Math.Max(Math.Min(tileX,63),0);
			tileY = Math.Max(Math.Min(tileY,63),0);
			int xposvecto = -(int)(((cam.Position.X % 1.2f) / 1.2f) * 8);
			int yposvecto = (int)(((cam.Position.Z % 1.2f) / 1.2f) * 8);
			int newzpos =(int)(((((cam.Position.Y) * 100)/32f)/15f)*128f)  - commonObjDat.height(127);
			
			newzpos = Math.Max(Math.Min(newzpos,127),0);
			var tmp = cam.Rotation;
			tmp.Y = (float)(tmp.Y - Math.PI);
			playerdat.heading = (int)Math.Round(-(tmp.Y * 127) / Math.PI);//placeholder track these values for projectile calcs.
			// playerdat.playerObject.heading = (short)((playerdat.headingMinor >> 0xD) & 0x7);
			// playerdat.playerObject.npc_heading = (short)((playerdat.headingMinor>>8) & 0x1F);
			uimanager.UpdateCompass();
			combat.CombatInputHandler(delta);
			playerdat.PlayerTimedLoop(delta);
			if (EnablePositionDebug)
			{
				var fps = Engine.GetFramesPerSecond();
				lblPositionDebug.Text = $"FPS:{fps} Time:{playerdat.game_time}\nL:{playerdat.dungeon_level} X:{tileX} Y:{tileY}\n{uimanager.instance.uwsubviewport.GetMousePosition()}\n{cam.Rotation} {playerdat.heading} {(playerdat.heading >> 4) % 4} {xposvecto} {yposvecto} {newzpos}";
			}


			if (UWTileMap.ValidTile(tileX, tileY))//((tileX < 64) && (tileX >= 0) && (tileY < 64) && (tileY >= 0))
			{				
				if ((playerdat.tileX != tileX) || (playerdat.tileY != tileY))
				{
					
					var tileExited = UWTileMap.current_tilemap.Tiles[playerdat.tileX, playerdat.tileY];
					if (UWClass._RES == UWClass.GAME_UW2)
					{
						//find exit triggers.
						if (tileExited.indexObjectList != 0)
						{
							var next = tileExited.indexObjectList;
							while (next != 0)
							{
								var nextObj = UWTileMap.current_tilemap.LevelObjects[next];
								trigger.RunTrigger(character: 0,
										ObjectUsed: nextObj,
										TriggerObject: nextObj,
										triggerType: (int)triggerObjectDat.triggertypes.EXIT,
										objList: UWTileMap.current_tilemap.LevelObjects);
								next = nextObj.next;
							}
						}
					}
					//player has changed tiles. move them to their new tile
					var oldTileX = playerdat.tileX; var oldTileY = playerdat.tileY;

					playerdat.tileX = Math.Min(Math.Max(tileX,0),63);
					playerdat.tileY = Math.Min(Math.Max(tileY,0),63);
					playerdat.PlacePlayerInTile(playerdat.tileX, playerdat.tileY, oldTileX, oldTileY);
					playerdat.xpos = Math.Min(Math.Max(0,xposvecto),8);
					playerdat.ypos = Math.Min(Math.Max(0,yposvecto),8);
					playerdat.zpos = newzpos;

					//tmp update the player object to keep in sync with other values
					playerdat.playerObject.item_id = 127;
					playerdat.playerObject.xpos = (short)playerdat.xpos;
					playerdat.playerObject.ypos = (short)playerdat.ypos;
					playerdat.playerObject.tileX = playerdat.tileX;
					playerdat.playerObject.npc_xhome = (short)tileX;
					playerdat.playerObject.tileY = playerdat.tileY;
					playerdat.playerObject.npc_yhome = (short)tileY;
					var tileEntered = UWTileMap.current_tilemap.Tiles[playerdat.tileX, playerdat.tileY]; 
					playerdat.PlayerStatusUpdate();
					if (UWClass._RES == UWClass.GAME_UW2)
					{
						//find enter triggers.
						//find exit triggers.
						if (tileEntered.indexObjectList != 0)
						{
							var next = tileEntered.indexObjectList;
							while (next != 0)
							{
								var nextObj = UWTileMap.current_tilemap.LevelObjects[next];
								trigger.RunTrigger(character: 0,
										ObjectUsed: nextObj,
										TriggerObject: nextObj,
										triggerType: (int)triggerObjectDat.triggertypes.ENTER,
										objList: UWTileMap.current_tilemap.LevelObjects);
								next = nextObj.next;
							}
						}
						//Debug.Print($"{playerdat.zpos} vs {(tileEntered.floorHeight << 3)}");
						// If grounded try and find pressure triggers. for the moment ground is just zpos less than floorheight.
						if (playerdat.zpos <= (tileEntered.floorHeight << 3))//Janky temp implementation. player must be on/below the height before changing tiles.
						{
							if (tileEntered.indexObjectList != 0)
							{
								var next = tileEntered.indexObjectList;
								while (next != 0)
								{
									var nextObj = UWTileMap.current_tilemap.LevelObjects[next];
									trigger.RunTrigger(character: 0,
											ObjectUsed: nextObj,
											TriggerObject: nextObj,
											triggerType: (int)triggerObjectDat.triggertypes.PRESSURE,
											objList: UWTileMap.current_tilemap.LevelObjects);
									next = nextObj.next;
								}
							}
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
					for (int i = 0; i < UWTileMap.current_tilemap.NoOfActiveMobiles; i++)
					{
						var index = UWTileMap.current_tilemap.GetActiveMobileAtIndex(i);
						if ((index != 0) && (index < 256))
						{
							var obj = UWTileMap.current_tilemap.LevelObjects[index];
							if (obj.majorclass == 1)
							{
								//This is an NPC						
								npc.NPCInitialProcess(obj);
							}
							else
							{
								if (motion.MotionSingleStepEnabled)
								{
									//This is a projectile
									motion.MotionProcessing(obj);
								}
							}
						}
					}
					//motion.MotionSingleStepEnabled = false;

					AnimationOverlay.UpdateAnimationOverlays();
					timers.RunTimerTriggers(1);
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
							tracking.DetectMonsters(8, playerdat.Track); break;
						case Key.F10: // make camp
							{
								Debug.Print("Make camp"); 
								//Try and find a bedroll in player inventory.
								var bedroll = objectsearch.FindMatchInFullObjectList(
									majorclass: 4, minorclass: 2, classindex: 1, 
									objList: playerdat.InventoryObjects);
								if (bedroll!=null)
								{
									sleep.Sleep(1);
								}
								else
								{
									sleep.Sleep(0);
								}
								break;							
							} 
							

						case Key.F11://toggle position label
							{
								EnablePositionDebug = !EnablePositionDebug;
								uimanager.EnableDisable(lblPositionDebug, EnablePositionDebug);
								break;
							}
						case Key.F12://debug
							{
								//cutsplayer.PlayCutscene(0);//test  
								//trigger.RunTimerTriggers();
								if (UWClass._RES==UWClass.GAME_UW2)
								{
									scd.ProcessSCDArk(1);
								}								
								//trigger.RunNextScheduledTrigger();
								break;
							}
						case Key.Pagedown:
							{
								motion.MotionSingleStepEnabled = true;//for stepping through motion processing.
								break;
							}
						case Key.Apostrophe:
							{
								//give full mage abilities
								playerdat.max_mana = 60;
								playerdat.play_mana = 60;
								playerdat.Casting = 30;
								playerdat.ManaSkill = 30;
								playerdat.play_level = 16;
								for (int r=0;r<24;r++)
								{
									playerdat.SetRune(r, true);
								}
								playerdat.PlayerStatusUpdate();
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
			switch (@event)
			{
				//Click to select options in conversation. Ensure we only allow left &right click to adhere to the original UW implementation. 
				case InputEventMouseButton mouseEvent:
					if (uimanager.CursorOverMessageScroll)
					{
						if (mouseEvent.Pressed && (mouseEvent.ButtonIndex == MouseButton.Left || mouseEvent.ButtonIndex == MouseButton.Right))
						{
							int result = uimanager.instance.HandleMessageScrollClick(mouseEvent);
							if ((result > 0) && (result <= ConversationVM.MaxAnswer))
							{
								ConversationVM.PlayerNumericAnswer = result;
								ConversationVM.WaitingForInput = false;
							}
							//GD.Print("Mouse Clicked in conversation");
						}
					}
	
					break;
				
				//Using keyboard numbers to select options in conversation
				case InputEventKey keyinput:
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

					break;
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

		if ((MessageDisplay.WaitingForTypedInput) && (!chargen.ChargenWaitForInput))
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

		if ((MessageDisplay.WaitingForTypedInput) && (chargen.ChargenWaitForInput))
		{//handles character name input
			if (@event is InputEventKey keyinput)
			{
				if (keyinput.Pressed)
				{
					bool stop = false;
					//var keyin = keyinput.GetKeycodeWithModifiers();
					switch (keyinput.Keycode)
					{
						case Key.Enter:
							stop = true;
							break;
						case Key.Backspace:
							{
								var text = uimanager.instance.ChargenNameInput.Text;
								if (text.Length>0)
								{
									text = text.Remove(text.Length-1);
									uimanager.instance.ChargenNameInput.Text = text;
								}
								break;
							}
						case >= Key.Space and <=Key.Z :
							{
								string inputed;// = (char)keyinput.Unicode;
								if (Input.IsPhysicalKeyPressed(Key.Shift))
								{
									inputed = ((char)keyinput.Unicode).ToString().ToUpper();
								}
								else
								{
									inputed = ((char)keyinput.Unicode).ToString().ToLower();
								}
								var text = uimanager.instance.ChargenNameInput.Text;
								if (text.Length<16)
								{
									text += inputed;
									uimanager.instance.ChargenNameInput.Text = text;
								}
								break;
							}
					}
					if (stop)
					{//end typed input						
						MessageDisplay.WaitingForTypedInput = false;
						chargen.ChargenWaitForInput = false;						
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
			UWTileMap.current_tilemap.CleanUp();
			//Handle tile changes after all else is done
			foreach (var t in UWTileMap.current_tilemap.Tiles)
			{
				if (t.Redraw)
				{
					UWTileMap.RemoveTile(
						tileX: t.tileX,
						tileY: t.tileY,
						removeWall: (t.tileType >= 2 && t.tileType <= 5));
					tileMapRender.RenderTile(tileMapRender.worldnode, t.tileX, t.tileY, t);
					t.Redraw = false;
				}
			}
		}

		//Handle level transitions now since it's possible for further traps to be called after the teleport trap
		Teleportation.HandleTeleportation();
	}    

}//end class
