using Godot;

namespace Underworld
{
	public partial class uimanager : Node2D
	{
		public static uimanager instance;


		public enum GameModes
		{ 
			MAIN,
			JOURNEY,
			CHARGEN,
			GAME,
			OPTIONS,
			AUTOMAP,
			CONVERSATION,
			CUTSCENE,
			DEATH,
			ENDGAME
		}

		public static GameModes CurrentGameMode = GameModes.CUTSCENE;


		/// <summary>
		/// Game activity will only current during this mode.
		/// </summary>
		public static bool InGame
		{
			get
			{
				return (uimanager.CurrentGameMode == uimanager.GameModes.GAME);
			}
		}

		public static bool AtMainMenu
		{
			get 
			{
				return 
					(uimanager.CurrentGameMode == uimanager.GameModes.JOURNEY)
					||
					(uimanager.CurrentGameMode == uimanager.GameModes.CHARGEN)
					||
					(uimanager.CurrentGameMode == uimanager.GameModes.MAIN);
			}
		}

		public static bool InConversation
		{
			get
			{
				return (uimanager.CurrentGameMode == uimanager.GameModes.CONVERSATION);
			}
		}

		public static bool InAutomap
		{
			get
			{
				return (uimanager.CurrentGameMode == uimanager.GameModes.AUTOMAP);
			}			
		}

		

	public static bool blockmouseinput
	{
		get
		{
			return
			 uimanager.InConversation
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
			 uimanager.CurrentGameMode == GameModes.OPTIONS
			 ;
		}
	}

		
		
		[ExportGroup("Placeholders")]
		[Export] public TextureRect placeholderuw1;
		[Export] public TextureRect placeholderuw2;


		public override void _Ready()
		{
			//Debug.Print("Uimanager about to set instance to this");
			instance = this;
			main.StartGame();
		}

		public void InitUI()
		{
			InitArt();
			InitMainMenu();
			InitFlasks();
			InitCoversation();
			InitPanels();
			InitPaperdoll();
			InitGameOptions();
			InitInteraction();
			InitViews();
			InitMessageScrolls();
			InitCuts();
			InitSpellIcons();
			InitCompass();
			InitAutomap();
			InitPower();
			InitStats();
			InitWeaponAnimation();
			InitEyes();
			InitDragons();

			AutomapBG.Texture = bitmaps.LoadImageAt(BytLoader.BLNKMAP_BYT);
			EnableDisable(AutomapPanel,false);

			mousecursor.InitCursor();

			EnableDisable(placeholderuw1, false);
			EnableDisable(placeholderuw2, false);

			EnableDisable(uw1UI, UWClass._RES == UWClass.GAME_UW1);
			EnableDisable(uw2UI, UWClass._RES != UWClass.GAME_UW1);  
			EnableDisable(PanelMainMenu,true);          
		}
		

		public override void _Process(double delta)
		{
			_ProcessPanels(delta);
			//_ProcessWeaponAnims(delta);
			_ProcessEyeAnims(delta);
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
	} //end class
}   //end namespace
