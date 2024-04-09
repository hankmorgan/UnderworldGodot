using System;
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
		public static void RefreshStatsDisplay()
		{
			instance.Charname.Text = playerdat.CharName.ToUpper();
			instance.CharClass.Text = GameStrings.GetString(2, 23 + playerdat.CharClass).ToUpper();
			instance.CharLevel.Text = $"{playerdat.play_level}{GameStrings.GetOrdinal(playerdat.play_level).ToUpper()}";
			instance.STR.Text = $"{playerdat.STR}";
			instance.DEX.Text = $"{playerdat.DEX}";
			instance.INT.Text = $"{playerdat.INT}";
			instance.VIT.Text = $"{playerdat.play_hp}/{playerdat.max_hp}";
			instance.MANA.Text =  $"{playerdat.play_mana}/{playerdat.max_mana}";
			instance.EXP.Text = $"{playerdat.Exp/10}";
			instance.StatsName.Text="";
			instance.StatsValue.Text="";
			for (int s = 0; s<6; s++)
			{
				if ((StatsOffset==0) && (s==0))
				{
					//display training points
					instance.StatsName.Text= "Skill Pt\n";
					instance.StatsValue.Text = $"{playerdat.SkillPoints}\n";
				}
				else
				{//display stat
					instance.StatsName.Text += $"{GameStrings.GetString(2,30+StatsOffset+s).ToUpper()}\n";
					instance.StatsValue.Text += $"{playerdat.GetSkillValue(StatsOffset+s-1)}\n";
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
                        RefreshStatsDisplay();
                        break;
                    case 1:
                        StatsOffset = Math.Min(15, StatsOffset + 1);
                        RefreshStatsDisplay();
                        break;
                }
            }
        }
        
    }//end class
}//end namespace