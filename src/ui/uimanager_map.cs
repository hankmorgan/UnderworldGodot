using System;
using System.Diagnostics;
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

        [Export] public TextureRect[] AutomapWorldGem = new TextureRect[9];


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

        public static void InitAutomap()
        {
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                instance.AutomapNumberLabel.Position += new Vector2(-28, 12);
                for (int i = 0; i <= instance.AutomapWorldGem.GetUpperBound(0); i++)
                {
                    instance.AutomapWorldGem[i].Texture = null; //clear until later.
                }
            }
            else
            {
                for (int i = 0; i <= instance.AutomapWorldGem.GetUpperBound(0); i++)
                {
                    EnableDisable(instance.AutomapWorldGem[i],false);
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

            if (UWClass._RES == UWClass.GAME_UW2)
            {                
                int[] worldmappingSelected = new int[] { 16, 8, 9, 10, 11, 12, 13, 14, 15 }; //the order of worlds is not the same as the order of images. this maps the world number to the on version of the image
                int[] worldmappingVisited = new int[] { 16, 0, 1, 2, 3, 4, 5, 6, 7 };
                for (int i = 0; i <= instance.AutomapWorldGem.GetUpperBound(0); i++)
                {
                    if (i == worldno)
                    {
                        instance.AutomapWorldGem[i].Texture = grGempt.LoadImageAt(worldmappingSelected[worldno]);
                    }
                    else
                    {
                        if (i != 0)
                        {
                            if (playerdat.HasWorldBeenVisited(i-1))
                            {
                                instance.AutomapWorldGem[i].Texture = grGempt.LoadImageAt(worldmappingVisited[i]);
                            }
                            else
                            {
                                instance.AutomapWorldGem[i].Texture  = null; //clear, unselected and unvisited.
                            }
                        }
                        else
                        {
                            instance.AutomapWorldGem[i].Texture = null; //clear britannia
                        }
                    }
                }
            }

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
                    mapnote.Position = new Vector2(-4 + n.posX * 4, 780 - (n.posY * 4));
                    mapnote.FitContent = true;
                    mapnote.BbcodeEnabled = true;
                    mapnote.AutowrapMode = TextServer.AutowrapMode.Off;
                    mapnote.Theme = MessageScroll.Theme;
                    mapnote.AddThemeFontOverride("normal_font", instance.Font4X5P);
                    mapnote.AddThemeFontSizeOverride("normal_font_size", 48);
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
                var newlevel = Math.Min(MapsPerWorld, automap.currentautomap + 1);
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
                var newlevel = Math.Max(0, automap.currentautomap - 1);
                if (newlevel != automap.currentautomap)
                {
                    DrawAutoMap(newlevel, automap.currentworld);
                }
            }
        }

        private void _on_world_gui_input(InputEvent @event, long extra_arg_0)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                Debug.Print ($"World {extra_arg_0}");
                if (extra_arg_0 != automap.currentworld)
                {
                    DrawAutoMap(automap.currentautomap, (int)extra_arg_0);
                }
            }
        }

    }//end class
}//end namespace