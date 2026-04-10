using System.Data.Common;
using System.Diagnostics;
using Godot;
namespace Underworld
{
    /// <summary>
    /// Placeholder for handling the victory screen.
    /// </summary>
    public partial class uimanager : Node2D
    {

        [ExportGroup("EndGame")]
        [Export]
        public Panel PanelEndGame;
        [Export]
        public TextureRect EndGameBackground;

        [Export]
        public RichTextLabel EndGameStats;

        enum EndGameStages
        {
            NotStarted = 0,
            OnBitmap = 1,
            OnStats = 2,
            OnCredits = 3
        };

        static EndGameStages EndGameStage = 0;
        public static void VictoryScreen()
        {
            Debug.Print("You have won the game!");
            EnableDisable(instance.PanelEndGame, true);
            ProcessNextEndGameStage();
        }

        static void ProcessNextEndGameStage()
        {
            switch (EndGameStage)
            {
                case EndGameStages.NotStarted:
                    {
                        //Begin process
                        instance.EndGameStats.Text = "";
                        //display congratulations bitmap
                        if (UWClass._RES == UWClass.GAME_UW2)
                        {
                            instance.EndGameBackground.Texture = bitmaps.LoadImageAt(BytLoader.UW2WIN1_BYT, false);
                        }
                        else
                        {
                            instance.EndGameBackground.Texture = bitmaps.LoadImageAt(BytLoader.WIN1_BYT, false);
                        }
                        EndGameStage = EndGameStages.OnBitmap;
                        break;
                    }
                case EndGameStages.OnBitmap:
                    {
                        //Debug.Print("Display end game stats");
                        if (UWClass._RES == UWClass.GAME_UW2)
                        {
                            instance.EndGameBackground.Texture = bitmaps.LoadImageAt(BytLoader.UW2WIN2_BYT, false);
                        }
                        else
                        {
                            instance.EndGameBackground.Texture = bitmaps.LoadImageAt(BytLoader.WIN2_BYT, false);
                        }
                        DisplayEndGameStats();
                        EndGameStage = EndGameStages.OnStats;
                        break;
                    }
                case EndGameStages.OnStats:
                    {
                        EnableDisable(instance.PanelEndGame, false);
                        // Debug.Print("Play credits");
                        EndGameStage = EndGameStages.NotStarted;
                        cutsplayer.PlayCutscene(0xA, ReturnToMainMenu);
                        break;
                    }
            }
        }

        /// <summary>
        /// Displays the end game stats screen
        /// </summary>
        public static void DisplayEndGameStats()
        {
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                instance.EndGameBackground.Texture = bitmaps.LoadImageAt(BytLoader.UW2WIN2_BYT, false);
            }
            else
            {
                instance.EndGameBackground.Texture = bitmaps.LoadImageAt(BytLoader.WIN2_BYT, false);
            }

            string fontcolour;
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                fontcolour = PaletteLoader.ToBBCode(0, 0x52);
            }
            else
            {
                fontcolour = PaletteLoader.ToBBCode(0, 0x5C);
            }

            var gamedays = (playerdat.ClockValue / 0x1C2000) / 0xC;
            bool IsCheater = (playerdat.STR + playerdat.INT + playerdat.DEX) > 0x40;
            var stringBase = GameStrings.str_a_level_; //strings numbers for stats start at different offsets in UW1/UW2
            var statstring = $"{playerdat.CharName}\n{GameStrings.GetString(1, stringBase)}{playerdat.play_level} {playerdat.CharClassName} {GameStrings.GetString(1, stringBase + 1)} {GameStrings.GetString(1, stringBase + 2)}{gamedays}{GameStrings.GetString(1, stringBase + 3)}\n";

            //display attributes vit, mana and exp
            //TODO figure out a clear way to align these (hopefully without adding more text controls)
            statstring += $"{GameStrings.GetString(2, 0x11).PadRight(8)}{playerdat.STR.ToString().PadRight(6)}  {GameStrings.GetString(2, 0x14).PadRight(8)}{playerdat.max_hp.ToString().PadRight(6)}";
            statstring += "\n";
            statstring += $"{GameStrings.GetString(2, 0x12).PadRight(8)}{playerdat.DEX.ToString().PadRight(6)}  {GameStrings.GetString(2, 0x15).PadRight(8)}{playerdat.max_mana.ToString().PadRight(6)}";
            statstring += "\n";
            statstring += $"{GameStrings.GetString(2, 0x13).PadRight(8)}{playerdat.INT.ToString().PadRight(6)}  {GameStrings.GetString(2, 0x16).PadRight(8)}{(playerdat.Exp / 10).ToString().PadRight(6)}";
            statstring += "\n";

            //display skills. use a loop for simplicity
            for (int skillno = 0; skillno < 20; skillno++)
            {
                if (skillno != 0)
                {
                    if ((skillno) % 3 == 0)
                    {
                        statstring += "\n";
                    }
                    else
                    {
                       // statstring += "\t";
                    }
                }
                statstring += $"{GameStrings.GetString(2, 0x1F + skillno).PadRight(10)}{playerdat.GetSkillValue(skillno).ToString().PadRight(4)}";
            }


            if ((IsCheater) && (UWClass._RES == UWClass.GAME_UW2))
            {
                //and cheated on their stats.
                statstring += $"\n{GameStrings.GetString(1, stringBase + 4)}";
            }
            Debug.Print(statstring);
            instance.EndGameStats.Text = $"[center][color={fontcolour}]{statstring}[/color][/center]";
            uimanager.EnableDisable(instance.PanelEndGame, true);
        }

        private void _on_endgame_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                ProcessNextEndGameStage();
            }
        }
    }
}