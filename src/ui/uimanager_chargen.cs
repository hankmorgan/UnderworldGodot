using System.Diagnostics;
using Godot;
namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Chargen")]
        [Export] public CanvasLayer PanelChargen;
        [Export] public TextureRect ChargenBG;
        [Export] public RichTextLabel ChargenQuestion;

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
            chargen.PresentChargenOptions(0);//ask for gender
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
            newButton.Texture = grCHRBTNS.LoadImageAt(0);

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
            if ((chargenCols == 1) || (index ==-1))
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

            if ((chargenCols>1) && (index ==-1))
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