using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {

        [ExportGroup("StatusDisplays")]
        //Health and manaflask
        [Export] public Panel HealthFlaskPanel;
        [Export] public TextureRect[] HealthFlask = new TextureRect[13];
        [Export] public TextureRect HealthFlaskBG;
        [Export] public Panel ManaFlaskPanel;
        [Export] public TextureRect[] ManaFlask = new TextureRect[13];
        [Export] public TextureRect ManaFlaskBG;

        [Export] public TextureRect HealthBubbles;

        public static int CurrentHealthFlaskLevel;
        public static int CurrentManaFlaskLevel;
        static int HealthBubbleCounter = 0;

        public static int TargetHealthFlaskLevel
        {
            get
            {
                return (int)((float)((float)playerdat.play_hp / (float)playerdat.max_hp) * 12f);
            }
        }
        public static int TargetManaFlaskLevel
        {
            get
            {
                return (int)((float)((float)playerdat.play_mana / (float)playerdat.max_mana) * 12f);
            }
        }


        private void InitFlasks()
        {
            grFlasks = new GRLoader(GRLoader.FLASKS_GR, GRLoader.GRShaderMode.UIShader);

            HealthFlaskBG.Texture = grFlasks.LoadImageAt(76);
            ManaFlaskBG.Texture = grFlasks.LoadImageAt(76);

            if (UWClass._RES == UWClass.GAME_UW2)
            {
                var offset = new Vector2(0, 24);
                HealthFlaskPanel.Position += offset;
                offset = new Vector2(16, 24);
                ManaFlaskPanel.Position += offset;
            }


            EnableDisable(HealthFlaskPanel, true);
            EnableDisable(ManaFlaskPanel, true);
        }


        /// <summary>
        /// Updates the hp/mana flask animations.
        /// </summary>
        public static void AnimateFlasks()
        {
            if ((TargetHealthFlaskLevel > CurrentHealthFlaskLevel) && (main.GlobalPITTimer % 32 == 0))
            {
                CurrentHealthFlaskLevel++;
                RedrawHealthFlask();
            }
            else if ((TargetHealthFlaskLevel < CurrentHealthFlaskLevel) && (main.GlobalPITTimer % 32 == 0))
            {
                CurrentHealthFlaskLevel--;
                RedrawHealthFlask();
            }
            else
            {
                bool doBubbling = false;
                if (main.GlobalPITTimer % 32 == 0)
                {
                    if (HealthBubbleCounter == 0)
                    {
                        doBubbling = Rng.r.Next(32) == 0;
                    }
                    else
                    {
                        doBubbling = true;
                    }
                }
                //doBubbling = false;// the bubbling happens at multiple flask levels but uses a specific graphic of size 24x4  that I need to overlay at the 1/4, 1/2 and 3/4 levels.
                if ((doBubbling) && (((CurrentHealthFlaskLevel >= 3) && (CurrentHealthFlaskLevel <= 9)) && (TargetHealthFlaskLevel == CurrentHealthFlaskLevel) && (main.GlobalPITTimer % 32 == 0)))
                {
                    RedrawHealthFlask(doBubbling, HealthBubbleCounter++); //false for now.
                    if (HealthBubbleCounter > 11)
                    {
                        HealthBubbleCounter = 0;
                    }
                }
                else
                {
                    if (!(((CurrentHealthFlaskLevel >= 3) && (CurrentHealthFlaskLevel <= 9)) && (TargetHealthFlaskLevel == CurrentHealthFlaskLevel)))
                    {
                        HealthBubbleCounter = 0;
                    } 
                    if (main.GlobalPITTimer % 32 == 0)
                    {
                        RedrawHealthFlask(false,0);
                    }
                }
            }

            if (TargetManaFlaskLevel > CurrentManaFlaskLevel)
            {
                CurrentManaFlaskLevel++;
                RedrawManaFlask();
            }
            else if (TargetManaFlaskLevel < CurrentManaFlaskLevel)
            {
                CurrentManaFlaskLevel--;
                RedrawManaFlask();
            }
            else
            {
                //flask is at right level. consider doing the bubbling animation..
            }

        }

        public static void RedrawHealthFlask(bool doBubbling = false, int bubblingOffset = 0)
        {
            if (instance == null) return; // reachable from InitEmptyPlayer in tests, before the Godot scene wires up the singleton
            //int level = (int)((float)((float)playerdat.play_hp / (float)playerdat.max_hp) * 12f);
            int startOffset = 0;
            if (playerdat.play_poison > 0)
            {
                startOffset = 50;
            }
            if ((doBubbling) && (TargetHealthFlaskLevel == CurrentHealthFlaskLevel) && ((CurrentHealthFlaskLevel >= 3) && (CurrentHealthFlaskLevel <= 9)))
            {
                instance.HealthBubbles.Texture = grFlasks.LoadImageAt(startOffset + 13 + bubblingOffset);
            }
            else
            {
                instance.HealthBubbles.Texture = null;
            }
            for (int i = 0; i < 13; i++)
            {
                if (i <= CurrentHealthFlaskLevel)
                {
                    if (doBubbling && ((i >= 3) || (i <= 9)) && (i == TargetHealthFlaskLevel))
                    {
                        instance.HealthFlask[i].Texture = grFlasks.LoadImageAt(startOffset + i);//null;//grFlasks.LoadImageAt(startOffset + i + 10 + bubblingOffset);
                        //move the bubbles to the level.
                        instance.HealthBubbles.Position = new Vector2(instance.HealthFlask[i].Position.X, instance.HealthFlask[i].Position.Y);
                    }
                    else
                    {
                        instance.HealthFlask[i].Texture = grFlasks.LoadImageAt(startOffset + i);
                    }
                }
                else
                {
                    instance.HealthFlask[i].Texture = null;
                }
            }
        }

        public static void RedrawManaFlask()
        {
            if (instance == null) return; // see RefreshHealthFlask
                                          // int level = (int)((float)((float)playerdat.play_mana / (float)playerdat.max_mana) * 12f);
            int startOffset = 25;
            for (int i = 0; i < 13; i++)
            {
                if (i <= CurrentManaFlaskLevel)
                {
                    instance.ManaFlask[i].Texture = grFlasks.LoadImageAt(startOffset + i);
                }
                else
                {
                    instance.ManaFlask[i].Texture = null;
                }
            }
        }

        private void _on_healthflask_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                if (playerdat.play_poison > 0)
                {//you are POISON LEVEL \n Your current vitality is vit out of maxvit

                    /*
                        0 barely
                        1 mildly
                        2 badly
                        3 seriously
                        4 critically
                        */

                    var poisonlevel = (playerdat.play_poison - 1) % 5;
                    if (uimanager.InConversation)
                    {
                        AddToMessageScroll(
                            stringToAdd: $"{GameStrings.GetString(1, GameStrings.str_you_are_)}{GameStrings.GetString(1, GameStrings.str_barely + poisonlevel)}{GameStrings.GetString(1, GameStrings.str__poisoned_)}\n{GameStrings.GetString(1, GameStrings.str_your_current_vitality_is_)}{playerdat.play_hp} out of {playerdat.max_hp}"
                            , mode: MessageDisplay.MessageDisplayMode.TemporaryMessage);
                    }
                    else
                    {
                        AddToMessageScroll(
                            stringToAdd: $"{GameStrings.GetString(1, GameStrings.str_you_are_)}{GameStrings.GetString(1, GameStrings.str_barely + poisonlevel)}{GameStrings.GetString(1, GameStrings.str__poisoned_)}\n{GameStrings.GetString(1, GameStrings.str_your_current_vitality_is_)}{playerdat.play_hp} out of {playerdat.max_hp}");
                    }

                }
                else
                {
                    if (uimanager.InConversation)
                    {
                        AddToMessageScroll(
                            stringToAdd: $"{GameStrings.GetString(1, GameStrings.str_your_current_vitality_is_)}{playerdat.play_hp} out of {playerdat.max_hp}",
                            mode: MessageDisplay.MessageDisplayMode.TemporaryMessage);
                    }
                    else
                    {
                        AddToMessageScroll($"{GameStrings.GetString(1, GameStrings.str_your_current_vitality_is_)}{playerdat.play_hp} out of {playerdat.max_hp}");
                    }

                }
            }
        }


        private void _on_manaflask_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                if (uimanager.InConversation)
                {
                    AddToMessageScroll(
                        stringToAdd: $"{GameStrings.GetString(1, GameStrings.str_your_current_mana_points_are_)}{playerdat.play_mana} out of {playerdat.max_mana}",
                        mode: MessageDisplay.MessageDisplayMode.TemporaryMessage
                        );
                }
                else
                {
                    AddToMessageScroll(
                        stringToAdd: $"{GameStrings.GetString(1, GameStrings.str_your_current_mana_points_are_)}{playerdat.play_mana} out of {playerdat.max_mana}"
                        );
                }
            }
        }
    }//end class
}//end namespace