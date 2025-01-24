using System.Diagnostics;
using Godot;
namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Chargen")]
        [Export] public CanvasLayer PanelChargen;
        [Export] public TextureRect ChargenBG;
        [Export] public TextureRect TemplateButton;

        static GRLoader grCHRBTNS;

        static void InitChargenUI()
        {
            EnableDisable(instance.PanelChargen, true);
            EnableDisable(instance.PanelMainMenu, false);
            instance.ChargenBG.Texture = bitmaps.LoadImageAt(BytLoader.CHARGEN_BYT);
            grCHRBTNS = new GRLoader(GRLoader.CHRBTNS_GR, GRLoader.GRShaderMode.None);
            grCHRBTNS.PaletteNo = 9;
            CreateChargenButton(0, "HELLO WORLD", null);
        }

        private static TextureRect CreateChargenButton(int index, string text, Control.GuiInputEventHandler callbackevent)
        {
            var newButton = new TextureRect();
            newButton.GuiInput += instance._on_template_button_gui_input;
            instance.PanelChargen.AddChild(newButton);
            newButton.Position = new Vector2(200, 60);
            newButton.Size = new Vector2(268, 64);
            newButton.Texture = grCHRBTNS.LoadImageAt(0);

            var label = new RichTextLabel();
            newButton.AddChild(label);            
            label.Position = new Vector2(18, 18);
            label.Size = new Vector2(240,40);
            label.Text = text;
            label.AddThemeFontOverride("normal_font",instance.FontBig);
            label.AddThemeFontSizeOverride("normal_font_size", 32);     
            label.MouseFilter = Control.MouseFilterEnum.Ignore;   
            return newButton;    
        }

        private void _on_template_button_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                Debug.Print("CLicked on");
            }
        }


    }
}