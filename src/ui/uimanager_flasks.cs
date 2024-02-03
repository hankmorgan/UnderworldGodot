using System;
using System.Diagnostics;
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


        private void InitFlasks()
        {
            grFlasks = new GRLoader(GRLoader.FLASKS_GR, GRLoader.GRShaderMode.UIShader);
            HealthFlaskBG.Texture = grFlasks.LoadImageAt(75);
            ManaFlaskBG.Texture = grFlasks.LoadImageAt(75);

            if (UWClass._RES == UWClass.GAME_UW2)
                {
                var offset = new Vector2(0, 24);
                HealthFlaskPanel.Position += offset;
                offset = new Vector2(16, 24);
                ManaFlaskPanel.Position += offset;
                }
        }

        public static void RefreshHealthFlask()
        {
            int level = (int)((float)((float)playerdat.play_hp / (float)playerdat.max_hp) * 12f);
            int startOffset = 0;
            if (playerdat.play_poison > 0)
            {
                startOffset = 50;
            }
            for (int i = 0; i < 13; i++)
            {
                if (i <= level)
                {
                    instance.HealthFlask[i].Texture = grFlasks.LoadImageAt(startOffset + i);
                }
                else
                {
                    instance.HealthFlask[i].Texture = null;
                }
            }
        }

        public static void RefreshManaFlask()
        {
            int level = (int)((float)((float)playerdat.play_mana / (float)playerdat.max_mana) * 12f);
            int startOffset = 25;
            for (int i = 0; i < 13; i++)
            {
                if (i <= level)
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
                    uimanager.AddToMessageScroll($"{GameStrings.GetString(1, GameStrings.str_you_are_)}{GameStrings.GetString(1, GameStrings.str_barely + poisonlevel)}{GameStrings.GetString(1, GameStrings.str__poisoned_)}\n{GameStrings.GetString(1, GameStrings.str_your_current_vitality_is_)}{playerdat.play_hp} out of {playerdat.max_hp}");
                }
                else
                {
                    uimanager.AddToMessageScroll($"{GameStrings.GetString(1, GameStrings.str_your_current_vitality_is_)}{playerdat.play_hp} out of {playerdat.max_hp}");
                }
            }
        }


        private void _on_manaflask_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                uimanager.AddToMessageScroll($"{GameStrings.GetString(1, GameStrings.str_your_current_mana_points_are_)}{playerdat.play_mana} out of {playerdat.max_mana}");
            }
        }
    }//end class
}//end namespace