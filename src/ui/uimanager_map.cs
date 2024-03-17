using System;
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

        [Export] public RichTextLabel AutomapNumberLabel;


        public static int MaxLevels
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return 80;
                }
                else
                {
                    return 9;
                }
            }
        }

        public static int MapsPerWorld
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return 7;
                }
                else
                {
                    return 8;
                }
            }
        }
        public static void DrawAutoMap(int level, int worldno)
        {
            int blockno;
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                level = level % 8;                
                blockno = (worldno * 8) + level;
            }
            else
            {
                blockno = level;
            }
            if (automap.automaps[blockno] == null)
            {
                automap.automaps[blockno] = new automap(blockno, UWClass._RES);
            }
            if (automap.automaps[blockno] == null)
            {
                return;
            }
            instance.AutomapImage.Texture = AutomapRender.MapImage(blockno);
            instance.AutomapNumberLabel.Text = $"{level + 1}";
            EnableDisable(instance.AutomapPanel, true);
            //TODO update UW2 Map gem

            //delete and replace notes
            foreach (var child in instance.NotesPanel.GetChildren())
            {
                child.QueueFree();
            }
            if (automapnote.automapsnotes[blockno] == null)
            {//load data if not ready.
                automapnote.automapsnotes[blockno] = new automapnote(blockno, UWClass._RES);
            }
            if (automapnote.automapsnotes[blockno] != null)
            {
                foreach (var n in automapnote.automapsnotes[blockno].notes)
                {
                    RichTextLabel mapnote = new();
                    mapnote.Position = new Vector2(-10 + n.posX * 4, 770 - (n.posY * 4));
                    mapnote.FitContent = true;
                    mapnote.BbcodeEnabled = true;
                    mapnote.AutowrapMode = TextServer.AutowrapMode.Off;
                    mapnote.Theme = MessageScroll.Theme;
                    mapnote.AddThemeFontOverride("normal_font", uimanager.instance.Font4X5P);
                    mapnote.AddThemeFontSizeOverride("normal_font_size", 64);
                    mapnote.Text = $"[color=#331C13]{n.notetext}[/color]";
                    instance.NotesPanel.AddChild(mapnote);
                }
            }

            automap.currentautomap = level;
            automap.currentworld = worldno;
            InAutomap = true;//to block input
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


        private void _on_map_down_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                var newlevel = Math.Max(0, automap.currentautomap - 1);
                if (newlevel != automap.currentautomap)
                {
                    DrawAutoMap(newlevel, automap.currentworld);
                }
            }
        }

        private void _on_map_up_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                var newlevel = Math.Min(MapsPerWorld, automap.currentautomap + 1);
                if (newlevel != automap.currentautomap)
                {
                    DrawAutoMap(newlevel, automap.currentworld);
                }
            }
        }
    }//end class
}//end namespace