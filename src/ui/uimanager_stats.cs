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

		public static int StatsOffset = 0;

		static string AttributesFontColor
		{
			get
			{
				if (UWClass._RES == UWClass.GAME_UW2)
				{
					return PaletteLoader.ToBBCode(0, 0xC4);
				}
				else
				{
					return PaletteLoader.ToBBCode(0, 0xF1);
				}
			}
		}

		static string SkillsFontColor
		{
			get
			{
				if (UWClass._RES == UWClass.GAME_UW2)
				{
					return PaletteLoader.ToBBCode(0, 0xC9);
				}
				else
				{
					return PaletteLoader.ToBBCode(0, 0x68);
				}
			}
		}

		private void InitStats()
		{
			if (UWClass._RES == UWClass.GAME_UW2)
			{
				var adjustment = new Godot.Vector2(-2f, -14f);
				instance.Charname.Position += adjustment;
				instance.CharClass.Position += adjustment;
				instance.CharLevel.Position += adjustment;
				instance.STR.Position += adjustment;
				instance.DEX.Position += adjustment;
				instance.INT.Position += adjustment;
				instance.VIT.Position += adjustment;
				instance.MANA.Position += adjustment;
				instance.EXP.Position += adjustment;
				adjustment = new Godot.Vector2(-2f, -18f);
				instance.StatsName.Position += adjustment;
				instance.StatsValue.Position += adjustment;
			}
		}

		/// <summary>
		/// Updates the stats display panel
		/// </summary>
		public static void RefreshStatsDisplay()
		{
			instance.Charname.Text = $"[center][color={AttributesFontColor}]{playerdat.CharName.ToUpper()}[/color][/center]";
			instance.CharClass.Text = $"[color={AttributesFontColor}]{playerdat.CharClassName.ToUpper()}[/color]";
			instance.CharLevel.Text = $"[right][color={AttributesFontColor}]{playerdat.play_level}{GameStrings.GetOrdinal(playerdat.play_level).ToUpper()}[/color][/right]";
			instance.STR.Text = $"[right][color={AttributesFontColor}]{playerdat.STR}[/color][/right]";
			instance.DEX.Text = $"[right][color={AttributesFontColor}]{playerdat.DEX}[/color][/right]";
			instance.INT.Text = $"[right][color={AttributesFontColor}]{playerdat.INT}[/color][/right]";
			instance.VIT.Text = $"[right][color={AttributesFontColor}]{playerdat.play_hp}/{playerdat.max_hp}[/color][/right]";
			instance.MANA.Text = $"[right][color={AttributesFontColor}]{playerdat.play_mana}/{playerdat.max_mana}[/color][/right]";
			instance.EXP.Text = $"[right][color={AttributesFontColor}]{playerdat.Exp / 10}[/color][/right]";
			instance.StatsName.Text = "";
			instance.StatsValue.Text = "";
			for (int s = 0; s < 6; s++)
			{
				if ((StatsOffset == 0) && (s == 0))
				{
					//display training points
					instance.StatsName.Text = $"[color={SkillsFontColor}]Skill Pt[/color]\n";
					instance.StatsValue.Text = $"[right][color={SkillsFontColor}]{playerdat.SkillPoints}[/color][/right]\n";
				}
				else
				{//display stat
					instance.StatsName.Text += $"[color={SkillsFontColor}]{GameStrings.GetString(2, 30 + StatsOffset + s).ToUpper()}[/color]\n";
					instance.StatsValue.Text += $"[right][color={SkillsFontColor}]{playerdat.GetSkillValue(StatsOffset + s - 1)}[/color][/right]\n";
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