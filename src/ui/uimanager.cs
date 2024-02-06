using System;
using System.Diagnostics;
using Godot;


namespace Underworld
{
	public partial class uimanager : Node2D
	{
		public static uimanager instance;


		[Export] public TextureRect placeholderuw1;
		[Export] public TextureRect placeholderuw2;



		static uimanager()
		{

		}


		public void InitUI()
		{
			instance = this;

			InitArt();
			InitFlasks();
			InitCoversation();
			InitPanels();
			InitPaperdoll();
			InitOptions();
			InitInteraction();
			InitViews();
			InitMessageScrolls();


			mousecursor.InitCursor();

			EnableDisable(placeholderuw1, false);
			EnableDisable(placeholderuw2, false);

			EnableDisable(uw1UI, UWClass._RES == UWClass.GAME_UW1);
			EnableDisable(uw2UI, UWClass._RES != UWClass.GAME_UW1);            
		}
		

		public override void _Process(double delta)
		{
			_ProcessPanels(delta);
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
