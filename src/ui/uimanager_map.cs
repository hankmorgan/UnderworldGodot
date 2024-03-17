using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {

        [ExportGroup("Automap")]
        //Automap
        [Export] public Panel AutomapPanel;
        [Export] public TextureRect AutomapBG;
        [Export] public TextureRect AutomapImage;
        public static bool InAutomap = false;
        [Export] public Panel NotesPanel;
    


        public static void DrawAutoMap(int level)
        {
            if (automap.automaps[level] == null)
            {
                automap.automaps[level] = new automap(level, UWClass._RES);
            }
            if (automap.automaps[level] == null)
            {
                return;
            }
            instance.AutomapImage.Texture = AutomapRender.MapImage(level);
            EnableDisable(instance.AutomapPanel,true);

            //delete and replace notes
            foreach (var child in instance.NotesPanel.GetChildren())
            {
                child.QueueFree();
            }
            if (automapnote.automapsnotes[level] == null)
            {//load data if not ready.
                automapnote.automapsnotes[level] = new automapnote(level, UWClass._RES);
            }
            if (automapnote.automapsnotes[level] != null)
            {
                foreach (var n in automapnote.automapsnotes[level].notes)
                {
                    RichTextLabel mapnote = new();
                    mapnote.Position = new Vector2(-10 + n.posX * 4, 770 - (n.posY * 4) );
                    mapnote.FitContent = true;
                    mapnote.BbcodeEnabled = true;
                    mapnote.AutowrapMode = TextServer.AutowrapMode.Off;
                    mapnote.Theme = MessageScroll.Theme; 
                    mapnote.AddThemeFontOverride("normal_font",uimanager.instance.Font4X5P);
                    mapnote.AddThemeFontSizeOverride("normal_font_size", 64);                    
                    mapnote.Text = $"[color=#331C13]{n.notetext}[/color]";
                    instance.NotesPanel.AddChild(mapnote);
                }
            }            

            InAutomap=true;//to block input
        }

        /// <summary>
        /// Closes the automap window
        /// </summary>
        /// <param name="event"></param>
        private void CloseAutomap(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                EnableDisable(AutomapPanel, false);
                InAutomap = false;
            }
        }
    }//end class
}//end namespace