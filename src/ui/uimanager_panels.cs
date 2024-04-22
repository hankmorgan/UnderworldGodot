using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Panels")]
        /// <summary>
        /// Panels
        /// </summary>
        [Export] public Panel PanelInventory;
        [Export] public TextureRect PanelInventoryArt;
        [Export] public Panel PanelRuneBag;
        [Export] public TextureRect PanelRuneBagArt;
        [Export] public Panel PanelStats;
        [Export] public TextureRect PanelStatsArt;

        Panel PanelToTurnOff;
        Panel PanelToTurnOn;

        public static int PanelMode;

        //Panel Timers
        static double PanelMoveTimer = 0;

        //Rotation states
        static bool RotatingOff = false;
        static bool RotatingOn = false;

        private void InitPanels()
        {
            var grPanels = new GRLoader(GRLoader.PANELS_GR, GRLoader.GRShaderMode.UIShader);
            if (grPanels != null)
            {
                PanelInventoryArt.Texture = grPanels.LoadImageAt(0);
                PanelRuneBagArt.Texture = grPanels.LoadImageAt(1);
                PanelStatsArt.Texture = grPanels.LoadImageAt(2);
            }
        }

        private void _ProcessPanels(double delta)
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
                    PanelToTurnOff.Scale = new Vector2(0, 1);

                    RotatingOn = true;
                    EnableDisable(PanelToTurnOn, true);
                    PanelToTurnOn.Scale = PanelToTurnOff.Scale;
                }
                else
                {
                    PanelToTurnOff.Scale = PanelToTurnOff.Scale.Lerp(new Vector2(0, 1f), (float)delta * 2);
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
                    PanelToTurnOn.Scale = new Vector2(1, 1);
                }
                else
                {
                    PanelToTurnOn.Scale = PanelToTurnOn.Scale.Lerp(new Vector2(1, 1f), (float)delta * 2);
                }
            }
        }


        public static void SetPanelMode(int NewMode)
        {//0 = inv, 1=runes, 2=stats
            if (PanelMode==NewMode)
            {
                return;//already at that panel
            }
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
                        {   //turn off runes
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
                        PanelMode = 0;
                        break;
                    }
                case 1: //switch to runes
                        //TODO enable rune panel at scale 0,0 to rotate in the oppsite direction.
                    for (int i = 0; i<24;i++)
                    {
                        SetRuneInBag(i, playerdat.GetRune(i));
                    }
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
        
		/// <summary>
		/// Handles clicking the chain to change the paperdoll panels
		/// </summary>
		/// <param name="event"></param>
		private void ChainPull(InputEvent @event)
		{
			if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                if (main.blockmouseinput)
                {
                    return;
                }
                ChangePanels();
            }
        }

        public static void ChangePanels()
        {
            switch (PanelMode)
            {
                case 0:
                    SetPanelMode(2); //go to stats from inventory
                    RefreshStatsDisplay();
                    break;
                case 1:
                    SetPanelMode(0); //runes from inventory
                    break;
                case 2:
                    SetPanelMode(0); // inventory from stats						
                    break;
            }
        }
    }//end class
}//end namespace