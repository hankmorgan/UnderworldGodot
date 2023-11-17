using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Godot;
using Godot.NativeInterop;

namespace Underworld
{
	public partial class uimanager : Node2D
	{
		public static uimanager instance;

		public enum InteractionModes
		{
			ModeOptions = 0,
			ModeTalk = 1,
			ModePickup = 2,
			ModeLook = 3,
			ModeAttack = 4,
			ModeUse = 5
		};

		//UI Timers
		static double PanelMoveTimer = 0;

		static bool RotatingOff = false;
		static bool RotatingOn = false;

		[Export] public Font Font4X5P;
		[Export] public Font Font5X6I;
		[Export] public Font Font5X6P;
		[Export] public Font FontBig;

		public static InteractionModes InteractionMode = InteractionModes.ModeUse;

		[Export] public Camera3D cam;
		[Export] public Node3D freelook;

		// [Export] public SubViewportContainer uwviewport;
		// [Export] public SubViewport uwsubviewport;

		[Export] public mouseCursor mousecursor;
		[Export] public CanvasLayer uw1UI;
		[Export] public CanvasLayer uw2UI;

		[Export] public TextureRect mainwindowUW1;
		[Export] public TextureRect mainwindowUW2;

		[Export] public Label messageScrollUW1;
		[Export] public Label messageScrollUW2;

		[Export] public TextureRect placeholderuw1;
		[Export] public TextureRect placeholderuw2;

		/// <summary>
		/// Panels
		/// </summary>
		[Export] public Panel PanelInventory;
		[Export] public Panel PanelRuneBag;
		[Export] public Panel PanelStats;

		Panel PanelToTurnOff;
		Panel PanelToTurnOn;

		public static int PanelMode;

		//Array to store the interaction mode mo
		[Export] public Godot.TextureButton[] InteractionButtonsUW1 = new Godot.TextureButton[6];
		[Export] public Godot.TextureButton[] InteractionButtonsUW2 = new Godot.TextureButton[6];

		public static bool Fullscreen = false;
		public static GRLoader grCursors;
		public static GRLoader grObjects;
		public static GRLoader grLfti;
		public static GRLoader grOptBtns;
		private ImageTexture[] UW2OptBtnsOff;
		private ImageTexture[] UW2OptBtnsOn;

		public static GRLoader grBody;
		public static GRLoader grArmour_F;
		public static GRLoader grArmour_M;

		[Export] public TextureRect Body;
		[Export] public TextureRect Helm;
		[Export] public TextureRect Armour;
		[Export] public TextureRect ArmourInput;
		[Export] public TextureRect Leggings;
		[Export] public TextureRect LeggingsInput;
		[Export] public TextureRect Boots;
		[Export] public TextureRect Gloves;
		[Export] public TextureRect GlovesInput1;
		[Export] public TextureRect GlovesInput2;
		[Export] public TextureRect RightShoulder;
		[Export] public TextureRect LeftShoulder;
		[Export] public TextureRect RightHand;
		[Export] public TextureRect LeftHand;
		[Export] public TextureRect RightRing;
		[Export] public TextureRect RightRingInput;
		[Export] public TextureRect LeftRing;
		[Export] public TextureRect LeftRingInput;
		[Export] public TextureRect[] Backpack = new TextureRect[8];
		[Export] public TextureRect OpenedContainer;
		public static int CurrentSlot;

		[Export] public TextureRect[] Runes = new TextureRect[24];
		[Export] public TextureRect[] SelectedRunes = new TextureRect[3];

		[Export] public Panel AutomapPanel;
		[Export] public TextureRect AutomapImage;

		public static BytLoader byt;

		static uimanager()
		{

		}

		public void InitUI()
		{
			instance = this;
			grCursors = new GRLoader(GRLoader.CURSORS_GR, GRLoader.GRShaderMode.UIShader);
			grObjects = new GRLoader(GRLoader.OBJECTS_GR, GRLoader.GRShaderMode.UIShader);
			grObjects.UseRedChannel = true;
			grLfti = new GRLoader(GRLoader.LFTI_GR, GRLoader.GRShaderMode.UIShader);
			grArmour_F = new GRLoader(GRLoader.ARMOR_F_GR, GRLoader.GRShaderMode.UIShader);
			grArmour_M = new GRLoader(GRLoader.ARMOR_M_GR, GRLoader.GRShaderMode.UIShader);

			// for (int i=0; i<grArmour_F.ImageCache.GetUpperBound(0);i++)
			// {
			//    grArmour_F.LoadImageAt(i).GetImage().SavePng(System.IO.Path.Combine("c:\\temp",$"armourf_{i}.png"));
			// }
			if (UWClass._RES == UWClass.GAME_UW2)
			{
				UW2OptBtnsOff = new ImageTexture[6];
				UW2OptBtnsOn = new ImageTexture[6];
				grOptBtns = new GRLoader(GRLoader.OPTBTNS_GR, GRLoader.GRShaderMode.UIShader);
				var Off = grOptBtns.LoadImageAt(0).GetImage();
				var On = grOptBtns.LoadImageAt(1).GetImage();
				UW2OptBtnsOff[4] = ArtLoader.CropImage(Off, new Rect2I(0, 0, 25, 14)); //attack button off
				UW2OptBtnsOn[4] = ArtLoader.CropImage(On, new Rect2I(0, 0, 25, 14)); //attack button on

				UW2OptBtnsOff[5] = ArtLoader.CropImage(Off, new Rect2I(26, 0, 25, 14)); //use button off
				UW2OptBtnsOn[5] = ArtLoader.CropImage(On, new Rect2I(26, 0, 25, 14)); //use button on

				UW2OptBtnsOff[2] = ArtLoader.CropImage(Off, new Rect2I(52, 0, 25, 14)); //pickup button off
				UW2OptBtnsOn[2] = ArtLoader.CropImage(On, new Rect2I(52, 0, 25, 14)); //pickup button on

				UW2OptBtnsOff[1] = ArtLoader.CropImage(Off, new Rect2I(0, 15, 25, 14)); //talk button off
				UW2OptBtnsOn[1] = ArtLoader.CropImage(On, new Rect2I(0, 15, 25, 14)); //talk button on

				UW2OptBtnsOff[3] = ArtLoader.CropImage(Off, new Rect2I(26, 15, 25, 14)); //look button off
				UW2OptBtnsOn[3] = ArtLoader.CropImage(On, new Rect2I(26, 15, 25, 14)); //look button on

				UW2OptBtnsOff[0] = ArtLoader.CropImage(Off, new Rect2I(52, 15, 25, 14)); //options button off
				UW2OptBtnsOn[0] = ArtLoader.CropImage(On, new Rect2I(52, 15, 25, 14)); //option button on

				//Move paperdoll
				var offset = new Vector2(-8, -13);
				Body.Position += offset;
				Helm.Position += offset;
				Boots.Position += offset;
				Gloves.Position += offset;
				Leggings.Position += offset;
				Armour.Position += offset;
				RightRing.Position += offset;
				LeftRing.Position += offset;
				RightRingInput.Position += offset;
				LeftRingInput.Position += offset;
				GlovesInput1.Position += offset;
				GlovesInput2.Position += offset;
				ArmourInput.Position += offset;
				LeggingsInput.Position += offset;

				for (int i = 0; i < 8; i++)
				{
					Backpack[i].Position += offset;
				}


			}
			byt = new BytLoader();

			mousecursor.InitCursor();
			EnableDisable(placeholderuw1, false);
			EnableDisable(placeholderuw2, false);

			EnableDisable(uw1UI, UWClass._RES == UWClass.GAME_UW1);
			EnableDisable(uw2UI, UWClass._RES != UWClass.GAME_UW1);

			//EnableDisable(mainwindowUW1, UWClass._RES == UWClass.GAME_UW1);
			//EnableDisable(mainwindowUW2, UWClass._RES != UWClass.GAME_UW1);  

			//EnableDisable(messageScrollUW1, UWClass._RES == UWClass.GAME_UW1);
			//EnableDisable(messageScrollUW2, UWClass._RES != UWClass.GAME_UW1);  

			switch (UWClass._RES)
			{
				case UWClass.GAME_UW2:
					mainwindowUW2.Texture = byt.LoadImageAt(BytLoader.UW2ThreeDWin_BYT, true);
					if (!Fullscreen)
					{
						// uwviewport.SetSize(new Vector2(840f,512f));
						// uwviewport.Position = new Vector2(62f,62f);
						// uwsubviewport.Size = new Vector2I(840,512);
					}

					for (int i = 0; i <= InteractionButtonsUW2.GetUpperBound(0); i++)
					{
						InteractionButtonsUW2[i].TexturePressed = UW2OptBtnsOn[i]; // grLfti.LoadImageAt(i*2 + 1,false);
						InteractionButtonsUW2[i].TextureNormal = UW2OptBtnsOff[i]; //grLfti.LoadImageAt(i*2,false);  
						InteractionButtonsUW2[i].SetPressedNoSignal((i == (int)InteractionMode));
					}

					break;
				default:
					mainwindowUW1.Texture = byt.LoadImageAt(BytLoader.MAIN_BYT, true);
					if (!Fullscreen)
					{
						// uwviewport.SetSize(new Vector2(700f,456f));
						// uwviewport.Position = new Vector2(200f,72f);
						// uwsubviewport.Size = new Vector2I(700,456);
					}
					//grLfti.ExportImages("c:\\temp\\lfti\\");
					for (int i = 0; i <= InteractionButtonsUW1.GetUpperBound(0); i++)
					{
						InteractionButtonsUW1[i].TexturePressed = grLfti.LoadImageAt(i * 2 + 1, false);
						InteractionButtonsUW1[i].TextureNormal = grLfti.LoadImageAt(i * 2, false);
						InteractionButtonsUW1[i].SetPressedNoSignal((i == (int)InteractionMode));
					}
					break;
			}
		}

		public override void _Process(double delta)
		{
			if (RotatingOff)
			{
				PanelMoveTimer += delta;
				if (PanelMoveTimer >= 0.5)
				{ //after 0.5 seconds inventory panel goes off and next pane goes on.
					//Disable inventory panel
					EnableDisable(PanelToTurnOff, false);
					RotatingOff = false;
					PanelMoveTimer = 0;
					PanelToTurnOff.Scale = new Vector2(0,1);

					RotatingOn = true; 
					EnableDisable(PanelToTurnOn, true);					
					PanelToTurnOn.Scale = PanelToTurnOff.Scale;					
				}
				else
				{
					PanelToTurnOff.Scale = PanelToTurnOff.Scale.Lerp(new Vector2(0,1f), (float)delta * 2);
				}
			}

			if (RotatingOn)
			{
				PanelMoveTimer += delta;
				if (PanelMoveTimer >= 0.5)
				{
					//Panel Runebag has arrived
					RotatingOn = false;
					PanelMoveTimer = 0;
					PanelToTurnOn.Scale = new Vector2(1,1);
				}
				else
				{
					PanelToTurnOn.Scale = PanelToTurnOn.Scale.Lerp(new Vector2(1,1f), (float)delta *2);
				}
			}
		}

		public static void EnableDisable(Control ctrl, bool state)
		{
			if (ctrl != null)
			{
				ctrl.Visible = state;
			}
		}

		public static void EnableDisable(CanvasLayer ctrl, bool state)
		{
			if (ctrl != null)
			{
				ctrl.Visible = state;
			}
		}

		public static void SetPanelMode(int NewMode)
		{//0 = inv, 1=runes, 2=stats
			if (RotatingOff || RotatingOn)
			{
				return; // a rotation is already in progress. block this until complete.
			}
			switch (NewMode)
			{
				case 0:
					{
						//stats or runes to inventory.
						if (PanelMode == 1)
						{	//turn off runes
							instance.PanelToTurnOff = instance.PanelRuneBag;
							instance.PanelToTurnOn = instance.PanelInventory;							
						}
						else
						{
							instance.PanelToTurnOff = instance.PanelStats;
							instance.PanelToTurnOn = instance.PanelInventory;
						}
						RotatingOff = true;
						RotatingOn = false;
						PanelMode=0;
						break;
					}
				case 1: //switch to runes
					//TODO enable rune panel at scale 0,0 to rotate in the oppsite direction.
					PanelMode = 1;
					RotatingOff = true;
					RotatingOn = false;
					instance.PanelToTurnOff = instance.PanelInventory;
					instance.PanelToTurnOn = instance.PanelRuneBag;
					PanelMoveTimer = 0;
					break;
				case 2:
				{//inventory to stats
					PanelMode = 2;
					RotatingOff = true;
					RotatingOn = false;
					instance.PanelToTurnOff = instance.PanelInventory;
					instance.PanelToTurnOn = instance.PanelStats;
					PanelMoveTimer = 0;

					break;
				}
			}
		}

		public void InteractionModeToggle(InteractionModes index)
		{
			Debug.Print($"Press {index}");

			if (UWClass._RES == UWClass.GAME_UW2)
			{

				for (int i = 0; i <= instance.InteractionButtonsUW2.GetUpperBound(0); i++)
				{
					InteractionButtonsUW2[i].SetPressedNoSignal(i == (int)(index));
					if (i == (int)(index))
					{
						InteractionMode = index;
					}
				}
			}
			else
			{
				for (int i = 0; i <= instance.InteractionButtonsUW1.GetUpperBound(0); i++)
				{
					InteractionButtonsUW1[i].SetPressedNoSignal(i == (int)(index));
					if (i == (int)(index))
					{
						InteractionMode = index;
					}
				}
			}
		}

		/// <summary>
		/// Sets the body image for the pc
		/// </summary>
		/// <param name="body"></param>
		/// <param name="isFemale"></param>
		public static void SetBody(int body, bool isFemale)
		{
			int MaleOrFemale = 0;
			if (isFemale)
			{
				MaleOrFemale = 1;
			}
			if (grBody == null)
			{
				grBody = new GRLoader(GRLoader.BODIES_GR, GRLoader.GRShaderMode.UIShader);
			}
			instance.Body.Texture = grBody.LoadImageAt(body + (5 * MaleOrFemale));
		}


		/// <summary>
		/// Redraws the specified slot
		/// </summary>
		/// <param name="slotno"></param>
		/// <param name="isFemale"></param>
		public static void RefreshSlot(int slotno, bool isFemale)
		{
			switch (slotno)
			{
				case 0: uimanager.SetHelm(playerdat.isFemale, helm.GetSpriteIndex(playerdat.HelmObject)); break;
				case 1: uimanager.SetArmour(playerdat.isFemale, chestarmour.GetSpriteIndex(playerdat.ChestArmourObject)); break;
				case 2: uimanager.SetGloves(playerdat.isFemale, gloves.GetSpriteIndex(playerdat.GlovesObject)); break;
				case 3: uimanager.SetLeggings(playerdat.isFemale, gloves.GetSpriteIndex(playerdat.LeggingsObject)); break;
				case 4: uimanager.SetBoots(playerdat.isFemale, gloves.GetSpriteIndex(playerdat.BootsObject)); break;
				//Set arms and shoulders
				case 5: uimanager.SetRightShoulder(uwObject.GetObjectSprite(playerdat.RightShoulderObject)); break;
				case 6: uimanager.SetLeftShoulder(uwObject.GetObjectSprite(playerdat.LeftShoulderObject)); break;
				case 7: uimanager.SetRightHand(uwObject.GetObjectSprite(playerdat.RightHandObject)); break;
				case 8: uimanager.SetLeftHand(uwObject.GetObjectSprite(playerdat.LeftHandObject)); break;
				//set rings
				case 9: uimanager.SetRightRing(ring.GetSpriteIndex(playerdat.RightRingObject)); break;
				case 10: uimanager.SetLeftRing(ring.GetSpriteIndex(playerdat.LeftRingObject)); break;
				default:
					if ((slotno >= 11) && (slotno <= 18))
					{
						uimanager.SetBackPack(slotno - 11, uwObject.GetObjectSprite(playerdat.BackPackObject(slotno - 11)));
					}
					break;
			}
		}


		/// <summary>
		/// Sets the sprite for the helmet
		/// </summary>
		/// <param name="isFemale"></param>
		/// <param name="SpriteNo"></param>
		public static void SetHelm(bool isFemale, int SpriteNo = -1)
		{
			if (SpriteNo == -1)
			{ //clear the slot
				instance.Helm.Texture = null;
			}
			else
			{
				instance.Helm.Texture = grArmour(isFemale).LoadImageAt(SpriteNo);
			}
		}


		/// <summary>
		/// Sets the sprite for the armour
		/// </summary>
		/// <param name="isFemale"></param>
		/// <param name="SpriteNo"></param>
		public static void SetArmour(bool isFemale, int SpriteNo = -1)
		{
			if (SpriteNo == -1)
			{ //clear the slot
				instance.Armour.Texture = null;
			}
			else
			{
				instance.Armour.Texture = grArmour(isFemale).LoadImageAt(SpriteNo);
			}
		}


		/// <summary>
		/// Sets the sprite for the leggings
		/// </summary>
		/// <param name="isFemale"></param>
		/// <param name="SpriteNo"></param>
		public static void SetLeggings(bool isFemale, int SpriteNo = -1)
		{
			if (SpriteNo == -1)
			{ //clear the slot
				instance.Leggings.Texture = null;
			}
			else
			{
				instance.Leggings.Texture = grArmour(isFemale).LoadImageAt(SpriteNo);
			}
		}


		/// <summary>
		/// Sets the sprite for the boots
		/// </summary>
		/// <param name="isFemale"></param>
		/// <param name="SpriteNo"></param>
		public static void SetBoots(bool isFemale, int SpriteNo = -1)
		{
			if (SpriteNo == -1)
			{ //clear the slot
				instance.Boots.Texture = null;
			}
			else
			{
				instance.Boots.Texture = grArmour(isFemale).LoadImageAt(SpriteNo);
			}
		}


		/// <summary>
		/// Sets the sprite for the gloves
		/// </summary>
		/// <param name="isFemale"></param>
		/// <param name="SpriteNo"></param>
		public static void SetGloves(bool isFemale, int SpriteNo = -1)
		{
			if (SpriteNo == -1)
			{ //clear the slot
				instance.Gloves.Texture = null;
			}
			else
			{
				instance.Gloves.Texture = grArmour(isFemale).LoadImageAt(SpriteNo);
			}
		}

		/// <summary>
		/// Sets the sprite in the right shoulder
		/// </summary>
		/// <param name="SpriteNo"></param>
		public static void SetRightShoulder(int SpriteNo = -1)
		{
			if (SpriteNo == -1)
			{ //clear the slot
				instance.RightShoulder.Texture = null;
			}
			else
			{
				instance.RightShoulder.Texture = grObjects.LoadImageAt(SpriteNo);
				instance.RightShoulder.Material = grObjects.GetMaterial(SpriteNo);
			}
		}


		/// <summary>
		/// Sets the sprite in the left shoulder
		/// </summary>
		/// <param name="SpriteNo"></param>
		public static void SetLeftShoulder(int SpriteNo = -1)
		{
			if (SpriteNo == -1)
			{ //clear the slot
				instance.LeftShoulder.Texture = null;
			}
			else
			{
				instance.LeftShoulder.Texture = grObjects.LoadImageAt(SpriteNo);
				instance.LeftShoulder.Material = grObjects.GetMaterial(SpriteNo);
			}
		}


		/// <summary>
		/// Sets the sprite in the right hand
		/// </summary>
		/// <param name="SpriteNo"></param>
		public static void SetRightHand(int SpriteNo = -1)
		{
			if (SpriteNo == -1)
			{ //clear the slot
				instance.RightHand.Texture = null;
			}
			else
			{
				instance.RightHand.Texture = grObjects.LoadImageAt(SpriteNo);
				instance.RightHand.Material = grObjects.GetMaterial(SpriteNo);
			}
		}


		/// <summary>
		/// Sets the sprite in the left hand
		/// </summary>
		/// <param name="SpriteNo"></param>
		public static void SetLeftHand(int SpriteNo = -1)
		{
			if (SpriteNo == -1)
			{ //clear the slot
				instance.LeftHand.Texture = null;
			}
			else
			{
				instance.LeftHand.Texture = grObjects.LoadImageAt(SpriteNo);
				instance.LeftHand.Material = grObjects.GetMaterial(SpriteNo);
			}
		}


		/// <summary>
		/// Sets the sprite in the ring slot
		/// </summary>
		/// <param name="SpriteNo"></param>
		public static void SetRightRing(int SpriteNo = -1)
		{
			if (SpriteNo == -1)
			{ //clear the slot
				instance.RightRing.Texture = null;
			}
			else
			{
				instance.RightRing.Texture = grObjects.LoadImageAt(SpriteNo);
				instance.RightRing.Material = grObjects.GetMaterial(SpriteNo);
			}
		}


		/// <summary>
		/// Sets the sprite in the ring slot
		/// </summary>
		/// <param name="SpriteNo"></param>
		public static void SetLeftRing(int SpriteNo = -1)
		{
			if (SpriteNo == -1)
			{ //clear the slot
				instance.LeftRing.Texture = null;
			}
			else
			{
				instance.LeftRing.Texture = grObjects.LoadImageAt(SpriteNo);
				instance.LeftRing.Material = grObjects.GetMaterial(SpriteNo);
			}
		}

		/// <summary>
		/// Sets the sprite in the backpack slot
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="SpriteNo"></param>
		public static void SetBackPack(int slot, int SpriteNo = -1)
		{
			if (SpriteNo == -1)
			{ //clear the slot
				instance.Backpack[slot].Texture = null;
			}
			else
			{
				instance.Backpack[slot].Texture = grObjects.LoadImageAt(SpriteNo);
				instance.Backpack[slot].Material = grObjects.GetMaterial(SpriteNo);
			}
		}



		/// <summary>
		/// Sets the open container gui image
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="SpriteNo"></param>
		public static void SetOpenedContainer(int slot, int SpriteNo = -1)
		{
			if (SpriteNo == -1)
			{ //clear the slot
				instance.OpenedContainer.Texture = null;
			}
			else
			{
				instance.OpenedContainer.Texture = grObjects.LoadImageAt(SpriteNo);
				instance.OpenedContainer.Material = grObjects.GetMaterial(SpriteNo);
			}
		}

		public static void SetRuneInBag(int slot, bool state)
		{
			if (state)
			{
				instance.Runes[slot].Texture =  grObjects.LoadImageAt(232+slot);
				instance.Runes[slot].Material = grObjects.GetMaterial(232+slot);
			}
			else
			{
				instance.Runes[slot].Texture = null;
			}
			
		}


		/// <summary>
		/// Returns the gender specific grArmour data
		/// </summary>
		/// <param name="isFemale"></param>
		/// <returns></returns>
		public static GRLoader grArmour(bool isFemale)
		{
			if (isFemale)
			{
				if (grArmour_F == null)
				{
					grArmour_F = new GRLoader(GRLoader.ARMOR_F_GR, GRLoader.GRShaderMode.UIShader);
				}
				return grArmour_F;
			}
			else
			{
				if (grArmour_M == null)
				{
					grArmour_M = new GRLoader(GRLoader.ARMOR_M_GR, GRLoader.GRShaderMode.UIShader);
				}
				return grArmour_M;
			}
		}


		/// <summary>
		/// Handles click events on the paperdoll
		/// </summary>
		/// <param name="event"></param>
		/// <param name="extra_arg_0"></param>
		private void _paperdoll_gui_input(InputEvent @event, string extra_arg_0)
		{
			// Replace with function body.
			if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
			{
				var obj = 0;
				Debug.Print($"->{extra_arg_0}");
				switch (extra_arg_0)
				{
					case "Helm": { obj = playerdat.Helm; CurrentSlot = 0; break; }
					case "Armour": { obj = playerdat.ChestArmour; CurrentSlot = 1; break; }
					case "Gloves": { obj = playerdat.Gloves; CurrentSlot = 2; break; }
					case "Leggings": { obj = playerdat.Leggings; CurrentSlot = 3; break; }
					case "Boots": { obj = playerdat.Boots; CurrentSlot = 4; break; }

					case "RightShoulder": { obj = playerdat.RightShoulder; CurrentSlot = 5; break; }
					case "LeftShoulder": { obj = playerdat.LeftShoulder; CurrentSlot = 6; break; }
					case "RightHand": { obj = playerdat.RightHand; CurrentSlot = 7; break; }
					case "LeftHand": { obj = playerdat.LeftHand; CurrentSlot = 8; break; }

					case "RightRing": { obj = playerdat.RightRing; CurrentSlot = 9; break; }
					case "LeftRing": { obj = playerdat.LeftRing; CurrentSlot = 10; break; }

					case "Back0": { obj = playerdat.GetBackPackIndex(0); CurrentSlot = 11; break; }
					case "Back1": { obj = playerdat.GetBackPackIndex(1); CurrentSlot = 12; break; }
					case "Back2": { obj = playerdat.GetBackPackIndex(2); CurrentSlot = 13; break; }
					case "Back3": { obj = playerdat.GetBackPackIndex(3); CurrentSlot = 14; break; }
					case "Back4": { obj = playerdat.GetBackPackIndex(4); CurrentSlot = 15; break; }
					case "Back5": { obj = playerdat.GetBackPackIndex(5); CurrentSlot = 16; break; }
					case "Back6": { obj = playerdat.GetBackPackIndex(6); CurrentSlot = 17; break; }
					case "Back7": { obj = playerdat.GetBackPackIndex(7); CurrentSlot = 18; break; }
					case "OpenedContainer": { obj = playerdat.OpenedContainer; CurrentSlot = -1; break; }
					default:
						CurrentSlot = -1;
						Debug.Print("Unimplemented inventory slot"); break;
				}

				//Do action appropiate to the interaction mode verb. use 
				if (obj != 0)
				{
					switch (InteractionMode)
					{
						case InteractionModes.ModeUse:
							if (extra_arg_0 != "OpenedContainer")
							{
								use.Use(
									index: obj,
									objList: playerdat.InventoryObjects,
									WorldObject: false);
							}
							else
							{
								//close up opened container.
								container.Close(
									index: obj,
									objList: playerdat.InventoryObjects);
							}
							break;
						case InteractionModes.ModeLook:
							look.LookAt(obj, playerdat.InventoryObjects); break;
						default:
							Debug.Print("Unimplemented inventory use verb-object combination"); break;
					}
				}
				CurrentSlot = -1;
			}
		}

		/// <summary>
		/// Closes the automap window
		/// </summary>
		/// <param name="event"></param>
		private void CloseAutomap(InputEvent @event)
		{
			if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
			{
				EnableDisable(AutomapPanel, false);
			}
		}
		
		
		/// <summary>
		/// Handles clicking the chain to change the paperdoll panels
		/// </summary>
		/// <param name="event"></param>
		private void ChainPull(InputEvent @event)
		{
			if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
			{
				switch (PanelMode)
				{
					case 0:
						SetPanelMode(2); //go to stats from inventory
						break;
					case 1:
						SetPanelMode(0); //runes from inventory
						break;
					case 2:
						SetPanelMode(0); // inventory from stats
						break;						
				}
			}
		}
		
		private void RuneClick(InputEvent @event, long extra_arg_0)
		{
			if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
			{
				if (extra_arg_0>=0)
				{
					if (playerdat.GetRune((int)extra_arg_0))
					{						
						if (InteractionMode == InteractionModes.ModeLook)
						{
							look.GeneralLookDescription((int)(232 + extra_arg_0));
						}
						else
						{
							//use action
							Debug.Print($"Rune {extra_arg_0} can be selected");
							SelectRune((int)extra_arg_0);
						}
					}
					else
					{
						//Debug.Print($"Rune {extra_arg_0} is not available");
					}
				}
				else
				{
					//clear runes  
					for (int i=0;i<3;i++)
					{
						playerdat.SetSelectedRune(i,24);
					}
					RedrawSelectedSlots();
				}
			}
		}


		/// <summary>
		/// Selects a rune from the rune bag, triggers update of the selected ui and updates player dat
		/// </summary>
		/// <param name="NewRuneToSelect"></param>
		static void SelectRune(int NewRuneToSelect)
		{			
			//Adds rune to the selected shelf
			if (playerdat.IsSelectedRune(0))
			{
				if (playerdat.IsSelectedRune(1))
				{
					if (playerdat.IsSelectedRune(2))
					{
						//All three slots are filled. Shift values down and fill slot 3
						playerdat.SetSelectedRune(0, playerdat.GetSelectedRune(1));
						playerdat.SetSelectedRune(1, playerdat.GetSelectedRune(2));
						playerdat.SetSelectedRune(2, NewRuneToSelect);						
					}
					else
					{
						//Slot 2 is available.
						playerdat.SetSelectedRune(2, NewRuneToSelect);
					}
				}
				else
				{	//slot 1 is available
					playerdat.SetSelectedRune(1, NewRuneToSelect);
				}
			}
			else
			{//Slot 0 is available.
				playerdat.SetSelectedRune(0, NewRuneToSelect);
			}
			RedrawSelectedSlots();
		}

		/// <summary>
		/// Draws the selected rune slots after a change is made to them.
		/// </summary>
		public static void RedrawSelectedSlots()
		{
			for (int slot=0;slot<3;slot++)
			{
				if (playerdat.IsSelectedRune(slot))
				{
					//display
					instance.SelectedRunes[slot].Texture =  grObjects.LoadImageAt(232+playerdat.GetSelectedRune(slot));
					instance.SelectedRunes[slot].Material = grObjects.GetMaterial(232+playerdat.GetSelectedRune(slot));
				}
				else
				{
					//clear
					instance.SelectedRunes[slot].Texture=null;
				}
			}
		}

	} //end class
}   //end namespace
