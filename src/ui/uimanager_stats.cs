using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Stats")]
		//Stats Display
		[Export] public Label Charname;
		[Export] public Label CharLevel;
		[Export] public Label CharClass;
		[Export] public Label STR;
		[Export] public Label DEX;
		[Export] public Label INT;
		[Export] public Label VIT;
		[Export] public Label MANA;
		[Export] public Label EXP;
		[Export] public Label StatsName;
		[Export] public Label StatsValue;

		public static int StatsOffset=0;

        private void InitStats()
        {
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                Charname.Set("theme_override_colors/font_color", Color.Color8(255, 255, 255));
                CharClass.Set("theme_override_colors/font_color", Color.Color8(255, 255, 255));
                CharLevel.Set("theme_override_colors/font_color", Color.Color8(255, 255, 255));
                StatsName.Set("theme_override_colors/font_color", Color.Color8(255, 255, 255));
                STR.Set("theme_override_colors/font_color", Color.Color8(255, 255, 255));
                DEX.Set("theme_override_colors/font_color", Color.Color8(255, 255, 255));
                INT.Set("theme_override_colors/font_color", Color.Color8(255, 255, 255));
                EXP.Set("theme_override_colors/font_color", Color.Color8(255, 255, 255));
                VIT.Set("theme_override_colors/font_color", Color.Color8(255, 255, 255));
                MANA.Set("theme_override_colors/font_color", Color.Color8(255, 255, 255));
                StatsValue.Set("theme_override_colors/font_color", Color.Color8(255, 255, 255));
            }
        }

        /// <summary>
		/// Updates the stats display panel
		/// </summary>
		private void PrintStatsDisplay()
		{
			Charname.Text = playerdat.CharName.ToUpper();
			CharClass.Text = GameStrings.GetString(2, 23 + playerdat.CharClass).ToUpper();
			CharLevel.Text = $"{playerdat.play_level}{GameStrings.GetOrdinal(playerdat.play_level).ToUpper()}";
			STR.Text = $"{playerdat.STR}";
			DEX.Text = $"{playerdat.DEX}";
			INT.Text = $"{playerdat.INT}";
			VIT.Text = $"{playerdat.play_hp}/{playerdat.max_hp}";
			MANA.Text =  $"{playerdat.play_mana}/{playerdat.max_mana}";
			EXP.Text = $"{playerdat.Exp}";
			StatsName.Text="";
			StatsValue.Text="";
			for (int s = 0; s<6; s++)
			{
				if ((StatsOffset==0) && (s==0))
				{
					//display training points
					StatsName.Text= "Skill Pt\n";
					StatsValue.Text = $"{playerdat.TrainingPoints}\n";
				}
				else
				{//display stat
					StatsName.Text += $"{GameStrings.GetString(2,30+StatsOffset+s).ToUpper()}\n";
					StatsValue.Text += $"{playerdat.GetSkillValue(StatsOffset+s)}\n";
				}
			}
		}

		
        private void _on_stats_updown_gui_input(InputEvent @event, long extra_arg_0)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                switch (extra_arg_0)
                {
                    case -1:
                        StatsOffset = Math.Max(0, StatsOffset - 1);
                        PrintStatsDisplay();
                        break;
                    case 1:
                        StatsOffset = Math.Min(15, StatsOffset + 1);
                        PrintStatsDisplay();
                        break;
                }
            }
        }
        
    }//end class
}//end namespace