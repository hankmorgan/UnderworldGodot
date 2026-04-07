using System;
using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Stats")]
		//Stats Display
		[Export] public RichTextLabel Charname;
		[Export] public RichTextLabel CharLevel;
		[Export] public RichTextLabel CharClass;
		[Export] public RichTextLabel STR;
		[Export] public RichTextLabel DEX;
		[Export] public RichTextLabel INT;
		[Export] public RichTextLabel VIT;
		[Export] public RichTextLabel MANA;
		[Export] public RichTextLabel EXP;
		[Export] public RichTextLabel StatsName;
		[Export] public RichTextLabel StatsValue;

		public static int StatsOffset=0;

		static string AttributesFontColor
		{
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    var col = PaletteLoader.Palettes[0].ColorAtIndex(0xC4,false,false);
                    return $"#{col.R8.ToString("X2")}{col.G8.ToString("X2")}{col.B8.ToString("X2")}";
                }
                else
                {
                    var col = PaletteLoader.Palettes[0].ColorAtIndex(0xF1,false,false);
                    return $"#{col.R8.ToString("X2")}{col.G8.ToString("X2")}{col.B8.ToString("X2")}";
                }
            }
		}

		static string SkillsFontColor
		{
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    var col = PaletteLoader.Palettes[0].ColorAtIndex(0xC9,false,false);
                    return $"#{col.R8.ToString("X2")}{col.G8.ToString("X2")}{col.B8.ToString("X2")}";
                }
                else
                {
                    var col = PaletteLoader.Palettes[0].ColorAtIndex(0x68,false,false);
                    return $"#{col.R8.ToString("X2")}{col.G8.ToString("X2")}{col.B8.ToString("X2")}";
                }
            }
		}

        private void InitStats()
        {

        }

        /// <summary>
		/// Updates the stats display panel
		/// </summary>
		public static void RefreshStatsDisplay()
		{
			instance.Charname.Text = $"[color={AttributesFontColor}]{playerdat.CharName.ToUpper()}[/color]";
			instance.CharClass.Text = $"[color={AttributesFontColor}]{GameStrings.GetString(2, 23 + playerdat.CharClass).ToUpper()}[/color]";
			instance.CharLevel.Text = $"[color={AttributesFontColor}]{playerdat.play_level}{GameStrings.GetOrdinal(playerdat.play_level).ToUpper()}[/color]";
			instance.STR.Text = $"[color={AttributesFontColor}]{playerdat.STR}[/color]";
			instance.DEX.Text = $"[color={AttributesFontColor}]{playerdat.DEX}[/color]";
			instance.INT.Text = $"[color={AttributesFontColor}]{playerdat.INT}[/color]";
			instance.VIT.Text = $"[color={AttributesFontColor}]{playerdat.play_hp}/{playerdat.max_hp}[/color]";
			instance.MANA.Text =  $"[color={AttributesFontColor}]{playerdat.play_mana}/{playerdat.max_mana}[/color]";
			instance.EXP.Text = $"[color={AttributesFontColor}]{playerdat.Exp/10}[/color]";
			instance.StatsName.Text="";
			instance.StatsValue.Text="";
			for (int s = 0; s<6; s++)
			{
				if ((StatsOffset==0) && (s==0))
				{
					//display training points
					instance.StatsName.Text= $"[color={SkillsFontColor}]Skill Pt[/color]\n";
					instance.StatsValue.Text = $"[color={SkillsFontColor}]{playerdat.SkillPoints}[/color]\n";
				}
				else
				{//display stat
					instance.StatsName.Text += $"[color={SkillsFontColor}]{GameStrings.GetString(2,30+StatsOffset+s).ToUpper()}[/color]\n";
					instance.StatsValue.Text += $"[color={SkillsFontColor}]{playerdat.GetSkillValue(StatsOffset+s-1)}[/color]\n";
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