using System.Diagnostics;
using System.Text;
using Godot;
namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Chargen")]
        [Export] public CanvasLayer PanelChargen;
        [Export] public TextureRect ChargenBG;
        [Export] public RichTextLabel ChargenQuestion;
        [Export] public RichTextLabel ChargenSkills;
        [Export] public RichTextLabel ChargenSkills_values;
        [Export] public RichTextLabel ChargenStats;
        [Export] public RichTextLabel ChargenStats_values;
        [Export] public RichTextLabel ChargenGender;
        [Export] public RichTextLabel ChargenClass;
        [Export] public RichTextLabel ChargenName;
        [Export] public TextureRect ChargenNameBG;
        [Export] public RichTextLabel ChargenNameInput;
        [Export] public RichTextLabel ChargenNameQuestion;
        [Export] public TextureRect ChargenBody;



        static GRLoader grCHRBTNS;
        public static int chargenRows = 8;
        public static int chargenCols = 1;
        static void InitChargenUI()
        {
            EnableDisable(instance.PanelChargen, true);
            EnableDisable(instance.PanelMainMenu, false);
            instance.ChargenBG.Texture = bitmaps.LoadImageAt(BytLoader.CHARGEN_BYT);
            grCHRBTNS = new GRLoader(GRLoader.CHRBTNS_GR, GRLoader.GRShaderMode.None);            
            grCHRBTNS.PaletteNo = 9;
            instance.ChargenNameBG.Texture = grCHRBTNS.LoadImageAt(6);
            ClearChargenTextAndBody();
            chargen.PresentChargenOptions(0);//ask for gender
        }

        /// <summary>
        /// Clears out text boxes on the chargen screen.
        /// </summary>
        public static void ClearChargenTextAndBody()
        {
            instance.ChargenSkills.Text = "";
            instance.ChargenSkills_values.Text = "";
            instance.ChargenStats.Text = "";
            instance.ChargenStats_values.Text = "";
            instance.ChargenGender.Text = "";
            instance.ChargenClass.Text = "";
            instance.ChargenName.Text = "";
            instance.ChargenBody.Texture = null;
        }
        public static void clearchargenbuttons()
        {
            var nodechildren = instance.PanelChargen.GetChildren();
            foreach (var n in nodechildren)
            {
                if (n.Name.ToString().Substring(0, 4) == "CBTN")
                {
                    instance.PanelChargen.RemoveChild(n);
                    n.QueueFree();
                }
            }
        }


        /// <summary>
        /// Creates and places a chargen answer button
        /// </summary>
        /// <param name="index"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static TextureRect CreateChargenButton(int index, string text)
        {
            Vector2 buttonPos = CalculateChargenButtonPosition(index);

            var newButton = new TextureRect();
            newButton.Name = $"CBTN_{index}_{System.Guid.NewGuid()}";
            switch (index)
            {//I hate this!
                case 0: newButton.GuiInput += instance._on_template_button_gui_input0; break;
                case 1: newButton.GuiInput += instance._on_template_button_gui_input1; break;
                case 2: newButton.GuiInput += instance._on_template_button_gui_input2; break;
                case 3: newButton.GuiInput += instance._on_template_button_gui_input3; break;
                case 4: newButton.GuiInput += instance._on_template_button_gui_input4; break;
                case 5: newButton.GuiInput += instance._on_template_button_gui_input5; break;
                case 6: newButton.GuiInput += instance._on_template_button_gui_input6; break;
                case 7: newButton.GuiInput += instance._on_template_button_gui_input7; break;
                case 8: newButton.GuiInput += instance._on_template_button_gui_input8; break;
                case 9: newButton.GuiInput += instance._on_template_button_gui_input9; break;
                case 10: newButton.GuiInput += instance._on_template_button_gui_input10; break;
            }



            instance.PanelChargen.AddChild(newButton);
            newButton.Position = buttonPos; //new Vector2(200, 60);
            newButton.Size = new Vector2(268, 64);
            newButton.Texture = grCHRBTNS.LoadImageAt(0,false);

            var label = new RichTextLabel();
            newButton.AddChild(label);
            label.Position = new Vector2(18, 18);
            label.Size = new Vector2(240, 40);
            label.Text = text;
            label.AddThemeFontOverride("normal_font", instance.FontBig);
            label.AddThemeFontSizeOverride("normal_font_size", 32);
            label.MouseFilter = Control.MouseFilterEnum.Ignore;
            return newButton;
        }

        public static Vector2 CalculateChargenButtonPosition(int index)
        {
            int midpoint = chargenRows / 2;
            float buttonXaxis;
            if ((chargenCols == 1) || (index == -1))
            {
                buttonXaxis = 850f;
            }
            else
            {
                if (index % chargenCols == 0)
                {
                    buttonXaxis = 700f;
                }
                else
                {
                    buttonXaxis = 1000f;
                }
            }

            if ((chargenCols > 1) && (index == -1))
            {
                index = -2; //hack for positioning for skills question for shepard
            }

            Vector2 buttonPos;

            float tmp = 400f - (midpoint - (index / chargenCols)) * 70f;
            buttonPos = new Vector2(buttonXaxis, tmp);
            return buttonPos;
        }

        // private void _on_template_button_gui_input(InputEvent @event)
        // {
        //     if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
        //     {
        //         Debug.Print("CLicked on");
        //     }
        // }


        /// <summary>
        /// Prints the attributes display box
        /// </summary>
        public static void PrintChargenAttributes()
        {
            StringBuilder sb = new();
            sb.Append($"[p]{GameStrings.GetString(2, 17)}[/p]");
            sb.Append($"[p]{GameStrings.GetString(2, 18)}[/p]");
            sb.Append($"[p]{GameStrings.GetString(2, 19)}[/p]");
            sb.Append($"[p]{GameStrings.GetString(2, 20)}[/p]");
            uimanager.instance.ChargenStats.Text = sb.ToString();

            sb = new();
            sb.Append($"[p]{playerdat.STR}[/p]");
            sb.Append($"[p]{playerdat.DEX}[/p]");
            sb.Append($"[p]{playerdat.INT}[/p]");
            sb.Append($"[p]{playerdat.max_hp}[/p]");
            uimanager.instance.ChargenStats_values.Text = sb.ToString();
        }


        /// <summary>
        /// Prints the skills display box
        /// </summary>
        public static void PrintChargenSkills()
        {
            StringBuilder sb = new();
            StringBuilder sb_v = new();
            for (int i = 0; i < 20; i++)
            {
                if (playerdat.GetSkillValue(i) != 0)
                {
                    sb.Append($"[p]{GameStrings.GetString(2, 0x1F + i)}[/p]");
                    sb_v.Append($"[p]{playerdat.GetSkillValue(i)}[/p]");
                }
            }
            uimanager.instance.ChargenSkills.Text = sb.ToString();
            uimanager.instance.ChargenSkills_values.Text = sb_v.ToString();
        }



        static void HandleChargenClick(int index)
        {
            chargen.SubmitChargenOption(chargen.CurrentStage, index);
        }

        private void _on_template_button_gui_input0(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                HandleChargenClick(0);
            }
        }

        private void _on_template_button_gui_input1(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                HandleChargenClick(1);
            }
        }

        private void _on_template_button_gui_input2(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                HandleChargenClick(2);
            }
        }

        private void _on_template_button_gui_input3(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                HandleChargenClick(3);
            }
        }


        private void _on_template_button_gui_input4(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                HandleChargenClick(4);
            }
        }


        private void _on_template_button_gui_input5(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                HandleChargenClick(5);
            }
        }


        private void _on_template_button_gui_input6(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                HandleChargenClick(6);
            }
        }

        private void _on_template_button_gui_input7(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                HandleChargenClick(7);
            }
        }


        private void _on_template_button_gui_input8(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                HandleChargenClick(8);
            }
        }
        private void _on_template_button_gui_input9(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                HandleChargenClick(9);
            }
        }

        private void _on_template_button_gui_input10(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                HandleChargenClick(10);
            }
        }
    }
}