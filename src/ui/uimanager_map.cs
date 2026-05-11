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
        [Export] public Panel NotesPanel;

        [Export] public RichTextLabel AutomapNumberLabel;

        [Export] public TextureRect[] AutomapWorldGem = new TextureRect[9];
        [Export] public TextureRect Eraser;
        
        public enum automapactions
        {
            NONE,
            WRITING,
            DELETING
        }

        public static automapnote.mapnotetext currentmapnote;

        public static automapactions CurrentAutomapAction = automapactions.NONE;

        static int writingX = 0; static int writingY = 0;

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
                    EnableDisable(instance.AutomapWorldGem[i], false);
                }
            }
        }

        public static void DrawAutoMap(int level, int worldno)
        {
            uimanager.CurrentGameMode = GameModes.AUTOMAP;
            int blockno = GetAutomapBlock(ref level, worldno);
            Debug.Print($"Displaying automap {blockno}");
            if (automap.automaps[blockno] == null)
            {
                automap.automaps[blockno] = new automap(blockno);
            }
            if (automap.automaps[blockno] == null)
            {
                return;
            }
            instance.AutomapImage.Texture = AutomapRender.MapImage(blockno);
            instance.AutomapNumberLabel.Text = $"{level + 1}";
            EnableDisable(instance.AutomapPanel, true);


            if (UWClass._RES == UWClass.GAME_UW2)
            {
                var tmpworld = worldno;
                //Annoyingly Loths tomb and pits of carnage are swapped.
                if (tmpworld == 6)
                {
                    tmpworld = 7;
                }
                else if (tmpworld == 7)
                {
                    tmpworld = 6;
                }
                int[] worldmappingSelected = new int[] { 16, 8, 9, 10, 11, 12, 14, 13, 15 }; //the order of worlds is not the same as the order of images. this maps the world number to the on version of the image
                int[] worldmappingVisited = new int[] { 16, 0, 1, 2, 3, 4, 6, 5, 7 };
                for (int i = 0; i <= instance.AutomapWorldGem.GetUpperBound(0); i++)
                {
                    if (i == tmpworld)
                    {

                        instance.AutomapWorldGem[i].Texture = grGempt.LoadImageAt(worldmappingSelected[tmpworld]);
                    }
                    else
                    {
                        if (i != 0)
                        {
                            if (playerdat.HasWorldBeenVisited(i - 1))
                            {
                                instance.AutomapWorldGem[i].Texture = grGempt.LoadImageAt(worldmappingVisited[i]);
                            }
                            else
                            {
                                instance.AutomapWorldGem[i].Texture = null; //clear, unselected and unvisited.
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
                automapnote.automapsnotes[blockno] = new automapnote(blockno);
            }
            if (automapnote.automapsnotes[blockno] != null)
            {
                foreach (var n in automapnote.automapsnotes[blockno].notes)
                {
                    n.textlabel =  RichTextLabelMapNote.AddAutoMapNoteToScreen(n);
                }
            }

            automap.currentautomap = level;
            automap.currentworld = worldno;
            uimanager.CurrentGameMode = GameModes.AUTOMAP;//to block input and other game motion.

            //change the cursor to the quill
            uimanager.CurrentAutomapAction = automapactions.NONE;
            uimanager.instance.mousecursor.SetCursorToCursor(14);
        }

        public static int GetAutomapBlock(ref int level, int worldno)
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

            return blockno;
        }





        /// <summary>
        /// Closes the automap window
        /// </summary>
        /// <param name="event"></param>
        private void CloseAutomap(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                if (CurrentAutomapAction != automapactions.NONE)
                {
                    return;//don't do anything while writing.
                }
                CurrentAutomapAction = automapactions.NONE;
                EnableDisable(AutomapPanel, false);
                uimanager.CurrentGameMode = GameModes.GAME;
                if (UWClass._RES != UWClass.GAME_UW2)
                {
                    if (playerdat.play_drawn == 1)
                    {
                        XMIMusic.ChangeThemeMusic(XMIMusic.Armed);
                    }
                    else
                    {
                        XMIMusic.PickLevelThemeMusic(-1);
                    }
                }
                else
                {
                    if (playerdat.play_drawn == 1)
                    {
                        XMIMusic.ChangeThemeMusic(XMIMusic.Armed);
                    }
                }
                uimanager.instance.mousecursor.SetCursorToCursor();
            }
        }

        private void EraseMapNote(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                uimanager.CurrentAutomapAction = automapactions.DELETING;
                uimanager.instance.mousecursor.SetCursorToCursor(13);//this is probably the wrong cursor but I can't find the right one in the art files..
            }
        }


        private void _on_map_down_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                if (CurrentAutomapAction != automapactions.NONE)
                {
                    return;//don't do anything while writing.
                }
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
                if (CurrentAutomapAction != automapactions.NONE)
                {
                    return;//don't do anything while writing.
                }
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
                Debug.Print($"World {extra_arg_0}");
                if (extra_arg_0 != automap.currentworld)
                {
                    DrawAutoMap(automap.currentautomap, (int)extra_arg_0);
                }
            }
        }
        
        private void _on_map_background_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                switch (CurrentAutomapAction)
                {
                    case automapactions.NONE:
                        if (eventMouseButton.Position.X >=1000)
                        {
                            return;//don't allow writing on the right hand side near ui elements.
                        }
                        //start writing
                        CurrentAutomapAction = automapactions.WRITING;
                        uimanager.instance.mousecursor.SetCursorToCursor(12);
                                                
                        Debug.Print($"Begin writing at {eventMouseButton.Position.X / 4},{eventMouseButton.Position.Y / 4}");
                        writingX = (int)(-1 + (eventMouseButton.Position.X / 4));
                        writingY = (int)(195 - (eventMouseButton.Position.Y / 4));                        
                        currentmapnote = new automapnote.mapnotetext("", writingX, writingY);                                          
                        currentmapnote.textlabel =  RichTextLabelMapNote.AddAutoMapNoteToScreen(currentmapnote);
                        break;

                    case automapactions.WRITING:
                        //stop writing
                        //StopWritingAutomapNote();
                        break;

                    case automapactions.DELETING: //to be implemented.
                        CurrentAutomapAction = automapactions.NONE;
                        instance.mousecursor.SetCursorToCursor(14);
                        break;
                }

            }
        }

        public static void StopWritingAutomapNote(bool cancelled)
        {            
            CurrentAutomapAction = automapactions.NONE;
            instance.mousecursor.SetCursorToCursor(14);

            if (!cancelled)
            {
                //Add note to memory
                var level = automap.currentautomap;
                int blockno = GetAutomapBlock(ref level, automap.currentworld);
                if (automapnote.automapsnotes[blockno] == null)
                {//load data if not ready.
                    automapnote.automapsnotes[blockno] = new automapnote(blockno);
                }
                automapnote.automapsnotes[blockno].notes.Add(currentmapnote);

                //TODO fill out remainer of string with nulls.
            }
            else
            {
                currentmapnote.textlabel.QueueFree();
                currentmapnote = null;
            }
        }



    }//end class
}//end namespace