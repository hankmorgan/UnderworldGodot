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

	public static main instance;

	// Called when the node enters the scene tree for the first time.
	[Export] public Camera3D cam;
	[Export] public Node3D GimbalYaw;
	[Export] public Node3D GimbalRoll;
	public static Camera3D cameraPitchGimbal; //pitch
	public static Node3D cameraRollGimbal; // up/down
	public static Node3D cameraYawGimbal; // yaw
	[Export] public AudioStreamPlayer DigitalAudioPlayer;
	[Export] public RichTextLabel lblPositionDebug;
	//[Export] public uimanager uwUI;

	[Export] public SubViewport secondarycameras;

	double gameRefreshTimer = 0f;
	static double testclock = 0;

	double cycletime = 0;

	public static bool DoRedraw = false;


	//DOS INT8 (PIT) timer interupt. updates 18.2 times a second.
	public static double Pit = 0f;
	public static uint GlobalPITTimer = 0;
	//static uint PitTimer = 0;
	static uint LastPitTimer = 0;
	static byte EasyMoveFrameIncrement = 0;

	public static byte ThisFrameDelta = 0;
	static byte PreviousFrameDelta = 0;

	public override void _Ready()
	{
		instance = this;
		cameraPitchGimbal = cam;
		cameraRollGimbal = GimbalRoll;
		cameraYawGimbal = GimbalYaw;

		//uimanager.instance = uwUI;	
		if (uwsettings.instance != null)
		{
			GetTree().DebugCollisionsHint = uwsettings.instance.showcolliders;
		}


		_ = Peaky.Coroutines.Coroutine.Run(PITTIMER(), main.instance);

	}

	/// <summary>
	/// Experiment Emulation of an old skol PIT Timer
	/// </summary>
	/// <returns></returns>
	static IEnumerator PITTIMER()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.00391f);  //0.054945f
			GlobalPITTimer++;
		}
		yield return null;
	}


	public static void StartGame()
	{
		if (cameraPitchGimbal == null)
		{
			if (instance.cam == null)
			{
				Debug.Print("Main Cam instance is null. trying to find it's node");
				instance.cam = (Camera3D)instance.GetNode("/root/Underworld/WorldViewContainer/SubViewport/Camera3D");
			}
			cameraPitchGimbal = instance.cam;
			if (cameraPitchGimbal == null)
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
		cameraPitchGimbal.Fov = Math.Max(50, uwsettings.instance.FOV);
		uimanager.EnableDisable(instance.lblPositionDebug, EnablePositionDebug);
		ObjectCreator.grObjects = new GRLoader(GRLoader.OBJECTS_GR, GRLoader.GRShaderMode.BillboardSpriteShader);
		ObjectCreator.grObjects.UseRedChannel = true;
		ObjectCreator.grObjects.UseCropping = true;
		Palette.CurrentPalette = 0;
		uimanager.instance.InitUI();
		uimanager.EnableDisable(uimanager.instance.uw1UI, false);
		uimanager.EnableDisable(uimanager.instance.uw2UI, false);
		uimanager.EnableDisable(uimanager.instance.PanelInventory, false);
		uimanager.EnableDisable(uimanager.instance.ManaFlaskPanel, false);
		uimanager.EnableDisable(uimanager.instance.HealthFlaskPanel, false);
		cutsplayer.PlayCutscene(9, uimanager.ReturnToMainMenu);
		//Play intro theme after cutscene coroutine is queued, so music and graphics start together
		XMIMusic.LoadXMI(XMIMusic.IntroTheme);
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
			a_sprite.Position = cameraPitchGimbal.Position;
		}
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if ((uimanager.InGame) || (uimanager.AtMainMenu) || (uimanager.CurrentGameMode == uimanager.GameModes.CUTSCENE))
		{
			cycletime += delta;
			if (cycletime > 0.2)
			{
				cycletime = 0;
				PaletteLoader.UpdatePaletteCycles();
			}
		}


		// //DOS interupt 8
		// Pit += (delta*5);  //seem smoother		
		//GlobalPITTimer += delta;
		// if (Pit >= 0.054945) // DOS PIT Timer interupt 8 is 18.2 times a second
		// {
		// 	PitTimer++;   // (uint)(Pit / 0.054945);//This is probably all wrong. needs revisiting.
		// 	Pit = 0;
		// }

		//DOSBox seesm to indicate there is a 255hz timer that is incrementing GlobalPit. This would tally with the updating i've seen with that global. 
		// This would indicate that for a typical clock increment of ~ 0x19 (based on breakpoints in dosbox) the game is updating motion about every 0.097659 seconds.

		//Dosbox  PIT:PIT 0 Timer at 255.9927 Hz mode 3 


		//UGH

		testclock += delta;

		if ((uimanager.InGame) && (!uimanager.blockmouseinput) && (testclock >= 0.097659))
		{
			testclock = 0;
			byte AnimationFrameDeltaIncrement = 0;

			var ClockIncrement = GlobalPITTimer - LastPitTimer;
			if ((ClockIncrement < 0) || (ClockIncrement > 0x40))
			{
				ClockIncrement = 0x40;
				AnimationFrameDeltaIncrement = 1;
				EasyMoveFrameIncrement += 4;
			}
			else
			{
				//Debug.Print($"{PitTimer - LastPitTimer}");
				EasyMoveFrameIncrement += (byte)((GlobalPITTimer >> 4) - (LastPitTimer >> 4));  //every 16 pits?
				AnimationFrameDeltaIncrement = (byte)((GlobalPITTimer >> 6) - (LastPitTimer >> 6));//every 63 pits?
																								   // ClockIncrement = 0x5;//i've added this to temporarily control motion frame rate.
																								   // if ((playerdat.MagicalMotionAbilities & 0x4) !=0)
																								   // {
																								   // 	//Hack for levitate
				ClockIncrement = 0x19;//ignore original calc
									  // }

				//HACK the above appears to be what should be happening in vanilla code but is very slow to process, but the below gives the appearance of normal movement but may cause frame rate issues. 
				//Issues caused by this hacking.
				//-Levitation does not work going north ->a higher increment fixes the motionparams.y0 but makes other motion really fast.
				//-A clock increment of 1 will cause strafing to fail when moving to the east!
				//This whole section will need to be fixed in the future.				
				// EasyMoveFrameIncrement = 1;
				// AnimationFrameDeltaIncrement = 1;
				//ClockIncrement = 0xF;
				//ClockIncrement = ClockIncrement * 4;
			}

			if (ClockIncrement != 0)
			{
				//ClockIncrement = Math.Max(ClockIncrement, 2);//TODO: This low value breaks some motion. See above.
				ProcessMotionInputs();
				if (AnimationFrameDeltaIncrement != 0)
				{
					//if animations enabled
					if ((uimanager.InGame) && (!uimanager.blockmouseinput))
					{
						AnimationOverlay.UpdateAnimationOverlays();
						timers.RunTimerTriggers(AnimationFrameDeltaIncrement);
					}
				}
				playerdat.ClockValue += (int)ClockIncrement;
				LastPitTimer = GlobalPITTimer;
				AnimationFrameDeltaIncrement = EasyMoveFrameIncrement;
				if (playerdat.SpeedEnchantment)
				{
					EasyMoveFrameIncrement = (byte)((AnimationFrameDeltaIncrement >> 1) & 0x1);
				}
				else
				{
					EasyMoveFrameIncrement = 0;
				}

				GameObjectLoop(
					ClockIncrement: (byte)ClockIncrement,
					AnimationFrameDelta: AnimationFrameDeltaIncrement,
					EasyMove: false);

				

			}
		}
		
		if (uimanager.InGame)
		{
			combat.CombatInputHandler(delta);//may need to be moved outside this block
			playerdat.PlayerTimedLoop(delta);				
			RefreshWorldState();//handles teleports, tile redraws	
		}

		//Other updates
		if (uimanager.InGame)
		{

			if (EnablePositionDebug)
			{
				int tileX = -(int)(cam.Position.X / 1.2f);
				int tileY = (int)(cam.Position.Z / 1.2f);
				tileX = Math.Max(Math.Min(tileX, 63), 0);
				tileY = Math.Max(Math.Min(tileY, 63), 0);
				int xposvecto = -(int)(((cam.Position.X % 1.2f) / 1.2f) * 8);
				int yposvecto = (int)(((cam.Position.Z % 1.2f) / 1.2f) * 8);
				int newzpos = (int)(((((cam.Position.Y) * 100) / 32f) / 15f) * 128f) - commonObjDat.height(127);
				var fps = Engine.GetFramesPerSecond();
				lblPositionDebug.Text = $"FPS:{fps} Time:{playerdat.game_time}\nL:{playerdat.dungeon_level} X:{tileX} Y:{tileY}\n{uimanager.instance.uwsubviewport.GetMousePosition()}\n {motion.playerMotionParams.x_0} {motion.playerMotionParams.y_2} {motion.playerMotionParams.z_4}";
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


	static void GameObjectLoop(byte ClockIncrement, byte AnimationFrameDelta, bool EasyMove)
	{
		motion.CameraBobZAdjust_dseg_67d6_33CE = 0;
		motion.RelatedToClockIncrement_67d6_742 += ClockIncrement;
		motion.CameraIsBobbing_dseg_67d6_33c6 = false;

		if (motion.MotionInputPressed == 0)
		{
			if (playerdat.RoamingSightEnchantment == false)
			{
				//ProcessMotionInputs for roaming sight.
			}
		}
		if (
			(motion.MotionInputPressed != 0)
			||
			(motion.playerMotionParams.momentum_14 != 0)
			||
			(motion.playerMotionParams.unk_a_pitch != 0)
			||
			(motion.playerMotionParams.gravity_10_Z != 0)
			||
			(motion.playerMotionParams.unk_e_Y != 0)
			||
			(motion.playerMotionParams.unk_c_X != 0)
			||
			(motion.PlayerMotionUpdateRequired_dseg_D3 != false)
		)
		{
			if (EasyMove == false)
			{
				//when any forced movement or player input is not 0
				motion.PlayerMotion(ClockIncrement); //todo confirm increments
			}

		}
		if (playerdat.FreezeTimeEnchantment == false)
		{
			if (AnimationFrameDelta != 0)
			{
				ProcessMobileObjects(AnimationFrameDelta);
			}
		}
		if (playerdat.TileState != 0)
		{
			motion.WalkOnSurfaceType();
		}

		playerdat.ApplyPlayerSneakScore(EasyMove);

		//Footsteps();

		//Position player object now after all possible calcs have been completed
		playerdat.PositionPlayerCamera();
	}

	static void ProcessMotionInputs()
	{
		motion.PlayerMotionWalk_77C = 0;
		motion.PlayerMotionHeading_77E = 0;
		if (Input.IsKeyPressed(Key.W))
		{
			if (Input.IsKeyPressed(Key.Shift))//forwards
			{
				//walk forwards
				motion.PlayerMotionWalk_77C = 0x32;
			}
			else
			{
				//run forwards
				motion.PlayerMotionWalk_77C = 0x70;
			}
			motion.MotionInputPressed = 1;
		}
		//TODO: using this method probably leads to a lot of jank with requiring input on the correct frame?
		if (Input.IsKeyPressed(Key.Q))//turn left
		{
			motion.PlayerMotionHeading_77E = -90;//should this be scaled?
			motion.MotionInputPressed = 1;
		}
		if (Input.IsKeyPressed(Key.E))//turn right
		{
			motion.PlayerMotionHeading_77E = +90;
			motion.MotionInputPressed = 1;
		}
		if (Input.IsKeyPressed(Key.S))//walk backwards
		{
			motion.PlayerMotionWalk_77C = 0;
			motion.PlayerMotionHeading_77E = 0;
			motion.MotionInputPressed = 8;
		}
		if (Input.IsKeyPressed(Key.A))//slide left
		{
			motion.PlayerMotionWalk_77C = 0;
			motion.PlayerMotionHeading_77E = 0;
			motion.MotionInputPressed = 9;
		}
		if (Input.IsKeyPressed(Key.D))//slide right
		{
			motion.PlayerMotionWalk_77C = 0;
			motion.PlayerMotionHeading_77E = 0;
			motion.MotionInputPressed = 0xA;
		}
		if (Input.IsKeyPressed(Key.J))//jump.
		{
			if (playerdat.TileState != 1)//ensure we are not swimming
			{
				if (Input.IsKeyPressed(Key.Shift))
				{
					//long jump
					motion.MotionInputPressed = 6;
				}
				else
				{
					//jump
					//todo: Do a test that the player is grounded.
					motion.MotionInputPressed = 7;
				}
			}

		}
		if (Input.IsKeyPressed(Key.R))//fly up
		{
			if ((playerdat.MagicalMotionAbilities & 0x14) != 0)
			{
				motion.MotionInputPressed = 0xC;
			}
			else
			{
				motion.MotionInputPressed = 0;
			}
		}
		if (Input.IsKeyPressed(Key.F))//fly down
		{
			if ((playerdat.MagicalMotionAbilities & 0x14) != 0)
			{
				motion.MotionInputPressed = 0xD;
			}
			else
			{
				motion.MotionInputPressed = 0;
			}
		}

		//Addition. capture look up and down
		if (Input.IsKeyPressed(Key.Key1))
		{
			//lookdown
			if (motion.PlayerCameraPitch_dseg_67d6_33D6 >= -4096)
			{
				motion.PlayerCameraPitch_dseg_67d6_33D6 = (short)Math.Max(-4096, motion.PlayerCameraPitch_dseg_67d6_33D6 - 0x400);
				Debug.Print($"{motion.PlayerCameraPitch_dseg_67d6_33D6}");
			}
		}
		else if (Input.IsKeyPressed(Key.Key3))
		{
			//lookup
			if (motion.PlayerCameraPitch_dseg_67d6_33D6 <= 4096)
			{
				motion.PlayerCameraPitch_dseg_67d6_33D6 = (short)Math.Min(4096, motion.PlayerCameraPitch_dseg_67d6_33D6 + 0x400);
				Debug.Print($"{motion.PlayerCameraPitch_dseg_67d6_33D6}");
			}
		}
		else if (Input.IsKeyPressed(Key.Key2))
		{
			motion.PlayerCameraPitch_dseg_67d6_33D6 = 0;
			Debug.Print($"{motion.PlayerCameraPitch_dseg_67d6_33D6}");
		}


	}

	static void ProcessMobileObjects(byte AnimationFrameDelta)
	{
		ThisFrameDelta = (byte)((PreviousFrameDelta + AnimationFrameDelta) & 0xF);
		for (int i = 0; i < UWTileMap.current_tilemap.NoOfActiveMobiles; i++)
		{
			var index = UWTileMap.current_tilemap.GetActiveMobileAtIndex(i);
			if ((index > 1) && (index < 256))
			{
				var obj = UWTileMap.current_tilemap.LevelObjects[index];
				//Loop update for a many times as the frame deltas require the object to be updated in this loop.
				var initialnextframe = obj.NextFrame_0XA_Bit0123;
				while (CheckIfUpdateNeeded(nextFrame: obj.NextFrame_0XA_Bit0123))
				{
					if (obj.majorclass == 1)
					{
						if (UWTileMap.ValidTile(obj.tileX, obj.tileY))
						{
							//This is an NPC on the map	
							var n = (npc)obj.instance;
							bool result;
							// if ((obj.item_id==124) && (UWClass._RES == UWClass.GAME_UW1) )
							// {
							// 	result = false;//currently the slasher is bugging out.
							// }
							// else
							// {
							result = npc.NPCInitialProcess(obj);
							//}

							if (n != null)
							{
								if (obj.instance != null)
								{
									var CalcedFacing = npc.CalculateFacingAngleToNPC(obj);
									n.SetAnimSprite(obj.npc_animation, obj.AnimationFrame, CalcedFacing);
								}
							}
							if (result == false)
							{
								break;
							}
						}
						else
						{
							Debug.Print($"{obj.a_name} {obj.index} is off map");
						}
					}
					else
					{
						//if (motion.MotionSingleStepEnabled)
						//{
						//This is a projectile
						if (motion.MotionProcessing(obj) == false)
						{
							break;
						}
						//}
					}
					if (initialnextframe == obj.NextFrame_0XA_Bit0123)
					{
						obj.NextFrame_0XA_Bit0123 += 4; //hack. this will prevent an infinite loop occurring.
														//Debug.Print($"{obj.a_name} {obj.index} has bugged out in ProcessMobileObjects(), probably needs to be made static.");
					}

				}
			}
		}
		PreviousFrameDelta = ThisFrameDelta;
	}

	/// <summary>
	/// Checks if object/npc needs to move based on their nextFrame value and the current Animation Frame Delta
	/// </summary>
	/// <param name="nextFrame"></param>
	/// <param name="AnimationFrameDelta"></param>
	/// <returns></returns>
	static bool CheckIfUpdateNeeded(int nextFrame)
	{
		//if (AnimationFrameDelta)
		if (ThisFrameDelta > nextFrame)
		{
			if (nextFrame + 4 >= ThisFrameDelta)
			{
				return true;
			}
		}
		//seg007_17A2_357A
		if (ThisFrameDelta + 0x10 <= nextFrame)
		{
			return false;
		}
		if (PreviousFrameDelta > nextFrame)
		{
			return false;
		}
		if (PreviousFrameDelta <= ThisFrameDelta)
		{
			return false;
		}
		else
		{
			return true;
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
				if (!uimanager.blockmouseinput)
				{
					if (uimanager.IsMouseInViewPort())
					{
						uimanager.ClickOnViewPort(eventMouseButton);
					}

				}
			}
		}


		if ((!uimanager.blockmouseinput) && (uimanager.InGame))
		{
			if (@event is InputEventKey keyinput)
			{
				if (keyinput.Pressed)
				{
					switch (keyinput.Keycode)
					{
						// case Key.W:
						// 	{
						// 		// if (Input.IsKeyPressed(Key.Shift))
						// 		// {
						// 		// 	//walk forwards
						// 		// 	motion.PlayerMotionWalk_77C = 0x32;
						// 		// 	motion.PlayerMotionHeading_77E = 0;
						// 		// }
						// 		// else
						// 		// {
						// 		// 	//run forwards
						// 		// 	motion.PlayerMotionWalk_77C = 0x70;
						// 		// 	motion.PlayerMotionHeading_77E = 0;
						// 		// }
						// 		// motion.MotionInputPressed = 1;

						// 		break;
						// 	}
						// case Key.Q: //not vanilla key
						// 	{
						// 		// motion.PlayerMotionWalk_77C = 0;
						// 		// //turn left
						// 		// motion.PlayerMotionHeading_77E = -90;
						// 		// motion.MotionInputPressed = 1;
						// 		break;
						// 	}
						// case Key.E: //not vanilla key
						// 	{
						// 		//motion.PlayerMotionWalk_77C = 0;
						// 		//turn right
						// 		// motion.PlayerMotionHeading_77E = +90;
						// 		// motion.MotionInputPressed = 1;
						// 		break;
						// 	}
						// case Key.S: //not vanilla key
						// 	{
						// 		//move backwards
						// 		// motion.PlayerMotionWalk_77C = 0;
						// 		// motion.PlayerMotionHeading_77E = 0;
						// 		// motion.MotionInputPressed = 8;
						// 		break;
						// 	}
						// case Key.A: //not vanilla key
						// 	{
						// 		//slide left
						// 		motion.PlayerMotionWalk_77C = 0;
						// 		motion.PlayerMotionHeading_77E = 0;
						// 		motion.MotionInputPressed = 9;
						// 		break;
						// 	}
						// case Key.D: //not vanilla key
						// 	{
						// 		//slide right
						// 		motion.PlayerMotionWalk_77C = 0;
						// 		motion.PlayerMotionHeading_77E = 0;
						// 		motion.MotionInputPressed = 0xA;
						// 		break;
						// 	}
						// case Key.J://jumps
						// 	{
						// 		motion.PlayerMotionHeading_77E = 0;//cancel turn movement when jumping
						// 		if (Input.IsKeyPressed(Key.Shift))
						// 		{
						// 			//long jump
						// 			motion.MotionInputPressed = 6;

						// 		}
						// 		else
						// 		{
						// 			//jump
						// 			//todo: Do a test that the player is grounded.
						// 			motion.MotionInputPressed = 7;
						// 		}
						// 		break;
						// 	}
						//case Key.T:
						// var mouselook = (bool)cameraPitchGimbal.Get("MOUSELOOK");
						// if (mouselook)
						// {//toggle to free curso
						// 	Input.MouseMode = Input.MouseModeEnum.Hidden;
						// }
						// else
						// {//toogle to mouselook
						// 	Input.MouseMode = Input.MouseModeEnum.Captured;
						// }
						// cameraPitchGimbal.Set("MOUSELOOK", !mouselook);
						//break;
						// case Key.R: //fly up (not vanilla)
						// 	if ((playerdat.MagicalMotionAbilities & 0x14) != 0)
						// 	{
						// 		motion.MotionInputPressed = 0xC;
						// 	}
						// 	else
						// 	{
						// 		motion.MotionInputPressed = 0;
						// 	}
						// 	break;
						// case Key.F: //fly down (not vanilla)
						// 	if ((playerdat.MagicalMotionAbilities & 0x14) != 0)
						// 	{
						// 		motion.MotionInputPressed = 0xD;
						// 	}
						// 	else
						// 	{
						// 		motion.MotionInputPressed = 0;
						// 	}
						// 	break;
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
								if (bedroll != null)
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
								if (UWClass._RES == UWClass.GAME_UW2)
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
								for (int r = 0; r < 24; r++)
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
								if (text.Length > 0)
								{
									text = text.Remove(text.Length - 1);
									uimanager.instance.ChargenNameInput.Text = text;
								}
								break;
							}
						case >= Key.Space and <= Key.Z:
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
								if (text.Length < 16)
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

		if (uimanager.CurrentAutomapAction == uimanager.automapactions.WRITING)
		{
			if (@event is InputEventKey keyinput)
			{
				if (keyinput.Pressed)
				{
					switch (keyinput.Keycode)
					{
						case Key.Enter:
							uimanager.StopWritingAutomapNote(false);
							break;
						case Key.Escape:
							uimanager.StopWritingAutomapNote(true);
							break;
						case Key.Backspace:
							{
								var text = uimanager.currentmapnote.notetext;
								if (text.Length > 0)
								{
									text = text.Remove(text.Length - 1);
									uimanager.currentmapnote.notetext = text;
									uimanager.currentmapnote.textlabel.Text = $"[color=#331C13]{text}[/color]";
								}
								break;
							}

						default://TODO: Default is too broad. must exclude special chars
							{
								var text = uimanager.currentmapnote.notetext;
								if (text.Length < 0x30)//allowing space for \0 ending.
								{
									text += ((char)keyinput.Unicode).ToString();
									uimanager.currentmapnote.notetext = text;
									uimanager.currentmapnote.textlabel.Text = $"[color=#331C13]{text}[/color]";
								}
								break;
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
						cameraPitchGimbal.Set("MOVE", true);//re-enable movement
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
			//UWTileMap.current_tilemap.CleanUp();
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
		if (UWClass._RES == UWClass.GAME_UW2)
		{
			if (playerdat.dungeon_level == 5)
			{
				largeblackrockgem.CycleGemColours();
			}
		}

		//Handle level transitions now since it's possible for further traps to be called after the teleport trap
		Teleportation.HandleTeleportation();
	}



}//end class
