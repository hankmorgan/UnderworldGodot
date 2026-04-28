using System.Collections;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using Godot;
using Munt.NET;
using Peaky.Coroutines;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        byte PaletteIndexSaveGameSelected
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return 7;
                }
                else
                {
                    return 0xA2;
                }

            }
        }

        byte PaletteIndexSaveGameUnSelected
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return 0xCD;
                }
                else
                {
                    return 0xAA;
                }

            }
        }

        [ExportGroup("MainMenu")]
        [Export]
        public Panel PanelMainMenu;
        [Export]
        public TextureRect MainMenuBG;

        [Export]
        public TextureRect[] MainMenuButtons = new TextureRect[4];

        [Export] public Label LoadingLabel;

        [Export] public Label[] SaveGamesNames = new Label[4];

        public static bool AtMainMenu;
        private void InitMainMenu()
        {
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                var bgTex = bitmaps.LoadImageAt(5);
                Debug.Print($"MainMenu BG texture: {(bgTex != null ? $"{bgTex.GetWidth()}x{bgTex.GetHeight()}" : "NULL")}");
                MainMenuBG.Texture = bgTex;
                //move main menu buttons
                MainMenuButtons[0].Size = new Vector2(436, 68);
                MainMenuButtons[0].Position = new Vector2(420, 308);
                MainMenuButtons[1].Position = new Vector2(324, 392);

                MainMenuButtons[2].Size = new Vector2(668, 96);
                MainMenuButtons[2].Position = new Vector2(300, 484);

                MainMenuButtons[3].Size = new Vector2(580, 100);
                MainMenuButtons[3].Position = new Vector2(348, 584);
            }
            else
            {
                bitmaps.UseRedChannel = true;
                MainMenuBG.Texture = bitmaps.LoadImageAt(BytLoader.OPSCR_BYT);
                MainMenuBG.Material = bitmaps.GetMaterial(BytLoader.OPSCR_BYT);
                var img = bitmaps.LoadImageAt(BytLoader.OPSCR_BYT);
                Palette.CurrentPalette = 6;
                bitmaps.UseRedChannel = false;             
            }

            //Set the font for the save games.
            foreach (var sgn in SaveGamesNames)
            {
                sgn.Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameUnSelected, false, false));
            }   

            //MainMenuBG.Material = bitmaps.GetMaterial(BytLoader.OPSCR_BYT);
            LoadingLabel.Text = "";
            TurnButtonsOff();
            ToggleMainMenuButtons(true);
            HideSaves();

            AtMainMenu = true;
        }

        /// <summary>
        /// loads the off graphics for the main menu buttons
        /// </summary>
        private void TurnButtonsOff()
        {
            if (grOptbtn == null) return;
            for (int i = 0; i < 4; i++)
            {
                if (MainMenuButtons[i] != null)
                    MainMenuButtons[i].Texture = grOptbtn.LoadImageAt(i * 2);
            }
        }


        /// <summary>
        /// Shows or hides the 4 main menu options
        /// </summary>
        /// <param name="show"></param>
        private void ToggleMainMenuButtons(bool show)
        {
            for (int i = 0; i < 4; i++)
            {
                EnableDisable(MainMenuButtons[i], show);
            }
        }


        /// <summary>
        /// Hides the list of save games
        /// </summary>
        private void HideSaves()
        {
            for (int i = 0; i < 4; i++)
            {
                EnableDisable(SaveGamesNames[i], false);
            }
        }

        private void _on_introduction_mouse_entered()
        {
            TurnButtonsOff();
            MainMenuButtons[0].Texture = grOptbtn.LoadImageAt(1);
        }


        private void _on_introduction_mouse_exited()
        {
            TurnButtonsOff();
            MainMenuButtons[0].Texture = grOptbtn.LoadImageAt(0);
        }


        private void _on_create_character_mouse_entered()
        {
            TurnButtonsOff();
            MainMenuButtons[1].Texture = grOptbtn.LoadImageAt(3);
        }


        private void _on_create_character_mouse_exited()
        {
            TurnButtonsOff();
            MainMenuButtons[1].Texture = grOptbtn.LoadImageAt(2);
        }


        private void _on_acknowledgements_mouse_entered()
        {
            TurnButtonsOff();
            MainMenuButtons[2].Texture = grOptbtn.LoadImageAt(5);
        }


        private void _on_acknowledgements_mouse_exited()
        {
            TurnButtonsOff();
            MainMenuButtons[2].Texture = grOptbtn.LoadImageAt(4);
        }

        //
        private void _on_acknowledgements_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                // Introduction button plays CS000 → CS001 only.
                // Splash screens (Origin, LGS, title) play on game startup.
                cutsplayer.PlayCutscene(0xA, ReturnToMainMenu);
            }
        }

        private void _on_journey_onwards_mouse_entered()
        {
            TurnButtonsOff();
            MainMenuButtons[3].Texture = grOptbtn.LoadImageAt(7);
        }


        private void _on_journey_onwards_mouse_exited()
        {
            TurnButtonsOff();
            MainMenuButtons[3].Texture = grOptbtn.LoadImageAt(6);
        }


        /// <summary>
        /// Load the save game specified in the config file
        /// </summary>
        /// <param name="event"></param>
        private void _on_journey_onwards_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                ToggleMainMenuButtons(false);
                ToggleSaves();
            }
        }

        private void ToggleSaves(bool show = true)
        {
            if (show)
            {
                for (int i = 1; i <= 4; i++)
                {
                    var path = Path.Combine(UWClass.BasePath, $"SAVE{i}", "DESC");
                    if (File.Exists(path))
                    {
                        var savename = File.ReadAllText(path);
                        EnableDisable(SaveGamesNames[i - 1], true);
                        SaveGamesNames[i - 1].Text = savename;
                    }
                    else
                    {
                        EnableDisable(SaveGamesNames[i - 1], false);
                    }
                }
            }
            else
            {
                for (int i = 1; i <= 4; i++)
                {
                    EnableDisable(SaveGamesNames[i - 1], false);
                }
            }
        }

        private IEnumerator ClearMainMenu()
        {
            ToggleMainMenuButtons(false);
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                LoadingLabel.Text = GameStrings.GetString(1, 273);
            }
            else
            {
                LoadingLabel.Text = GameStrings.GetString(1, 257);
            }
            yield return 0;
        }

        /// <summary>
        /// Loads the save game and map from the selected SAVE game folder ".\SAVEx"
        /// </summary>
        /// <param name="folder"></param>
        public void JourneyOnwards(string folder)
        {
            playerdat.previousLightLevel = -1;
            playerdat.currentfolder = folder;
            playerdat.LoadPlayerDat(datafolder: folder);
            
            // //Common launch actions            
            UWTileMap.LoadTileMap(
                    newLevelNo: playerdat.dungeon_level - 1,
                    datafolder: folder,
                    newGameSession: true);


            //add player object data to the map
            for (int i = 0; i <= 0x1A; i++)
            {
                playerdat.playerObject.DataBuffer[playerdat.playerObject.PTR + i] = playerdat.pdat[playerdat.PlayerObjectStoragePTR + i];
            }
            playerdat.playerObject.item_id = 127;//make sure the object is an adventurer.
            playerdat.playerObject.link = 0;//prevents infinite loops.
            playerdat.playerObject.tileX = playerdat.playerObject.npc_xhome;
            playerdat.playerObject.tileY = playerdat.playerObject.npc_yhome;

            if (folder.ToUpper() == "DATA")
            {
                //default start locations.                
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        //main.gamecam.Position = new Vector3(-23f, 4.3f, 58.2f);
                        Teleportation.InitialisePlayerOnLevelOrPositionChange(0x13, 0x30);
                        break;
                    default:
                        //main.gamecam.Position = new Vector3(-38f, 4.2f, 2.2f);
                        Teleportation.InitialisePlayerOnLevelOrPositionChange(32, 1);
                        break;
                }
            }
            else
            {
                playerdat.PlacePlayerInTile(playerdat.playerObject.tileX, playerdat.playerObject.tileY);
            }

            //Update weight display
            uimanager.RefreshWeightDisplay();
            playerdat.PlayerStatusUpdate();

            //restore some UI elements that have been previously hidden as part of the splash intro
            uimanager.EnableDisable(uimanager.instance.uw1UI, UWClass._RES != UWClass.GAME_UW2);
            uimanager.EnableDisable(uimanager.instance.uw2UI, UWClass._RES == UWClass.GAME_UW2);
            uimanager.EnableDisable(uimanager.instance.PanelInventory,true);
            uimanager.EnableDisable(uimanager.instance.ManaFlaskPanel,true);
            uimanager.EnableDisable(uimanager.instance.HealthFlaskPanel,true);
            

            uimanager.OpenedContainerIndex = -1;//clear slot graphics
            uimanager.SetOpenedContainer(0, -1); 
            uimanager.BackPackStart = 0;
            uimanager.EnableDisable(uimanager.instance.ArrowUp, false);
            uimanager.EnableDisable(uimanager.instance.ArrowDown, false);

            if (!playerdat.MusicEnabled)
            {
                MusicStreamPlayer.Instance.Stop(); 
            }
            instance.InitViews();
            SetPanelMode(0);
            
            //Apply player motion on game load.
            motion.PlayerMotion(0x40);
            if (UWClass._RES == UWClass.GAME_UW2)
            {//In UW2 the theme music will always change on game load or if coming from main menu. In UW1 it will only happen when loading a game from the options menu. In that case the call to change themes is in the ui code for the options menu
                XMIMusic.ChangeTheme(XMIMusic.PickLevelThemeMusic(0));
            }
               
            //Set up the weapon animations.
            ToggleWeaponAnimationState(playerdat.play_drawn == 1);
            if (playerdat.play_drawn == 1)
            {
                InteractionMode = InteractionModes.ModeAttack;
                PreviousInteractionMode = InteractionModes.ModeAttack;            
            }
            else
            {
                InteractionMode = InteractionModes.ModeUse;
                PreviousInteractionMode = InteractionModes.ModeUse; 
            }
            uimanager.UpdateCompass();
        }

        private void _on_create_character_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                InitChargenUI();
            }
        }



        private void _on_save_1_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouse eventMouse)
            {
                //Set the font for the save games.
                SaveGamesNames[0].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameSelected, false, false)); 
                SaveGamesNames[1].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameUnSelected, false, false)); 
                SaveGamesNames[2].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameUnSelected, false, false)); 
                SaveGamesNames[3].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameUnSelected, false, false)); 
            }
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                _ = Coroutine.Run(
                   ClearMainMenu()
                   , main.instance);
                JourneyOnwards("SAVE1");
            }
        }

        private void _on_save_2_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouse eventMouse)
            {
                //Set the font for the save games.
                SaveGamesNames[0].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameUnSelected, false, false)); 
                SaveGamesNames[1].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameSelected, false, false)); 
                SaveGamesNames[2].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameUnSelected, false, false)); 
                SaveGamesNames[3].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameUnSelected, false, false)); 
            }
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                _ = Coroutine.Run(
                   ClearMainMenu()
                   , main.instance);

                JourneyOnwards("SAVE2");
            }
        }


        private void _on_save_3_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouse eventMouse)
            {
                //Set the font for the save games.
                SaveGamesNames[0].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameUnSelected, false, false)); 
                SaveGamesNames[1].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameUnSelected, false, false)); 
                SaveGamesNames[2].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameSelected, false, false)); 
                SaveGamesNames[3].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameUnSelected, false, false)); 
            }
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                _ = Coroutine.Run(
                   ClearMainMenu()
                   , main.instance);

                JourneyOnwards("SAVE3");
            }
        }


        private void _on_save_4_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouse eventMouse)
            {
                //Set the font for the save games.
                SaveGamesNames[0].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameUnSelected, false, false)); 
                SaveGamesNames[1].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameUnSelected, false, false)); 
                SaveGamesNames[2].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameUnSelected, false, false)); 
                SaveGamesNames[3].Set("theme_override_colors/font_color", PaletteLoader.Palettes[0].ColorAtIndex(PaletteIndexSaveGameSelected, false, false)); 
            }

            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                _ = Coroutine.Run(
                   ClearMainMenu()
                   , main.instance);

                JourneyOnwards("SAVE4");
            }
        }

        private void _on_introduction_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    // Introduction button plays CS000 → CS001 only.
                    cutsplayer.PlayCutscene(0, () =>
                        cutsplayer.PlayCutscene(1, ReturnToMainMenu));
                }
                else
                {
                    // Introduction button plays CS000 only.
                    cutsplayer.PlayCutscene(0, ReturnToMainMenu);
                }
            }
        }


        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyinput)
            {
                if (keyinput.Pressed & AtMainMenu)
                {
                    if (keyinput.Keycode == Key.Escape)
                    {//return to main menu
                        if (cutsplayer.IsPlaying)
                        {
                            cutsplayer.StopCutscene();
                        }
                        ToggleMainMenuButtons(true);
                        ToggleSaves(false);
                        EnableDisable(PanelChargen, false);
                        EnableDisable(PanelMainMenu, true);
                    }
                }
            }
        }


        /// <summary>
        /// Returns to main menu from the game (eg on player death)
        /// </summary>
        public static void ReturnToMainMenu()
        {
            Debug.Print("Return to main menu");
            //Still some weirdness with enabling the main menu again. eg palette switch in UW1
            if (MusicStreamPlayer.Instance?.IsPlaying != true || XMIMusic.CurrentThemeNo != 1)
            {
                XMIMusic.ChangeTheme(1);
            }
            EnableDisable(instance.PanelMainMenu, true);    
            instance.ToggleMainMenuButtons(true);            
            instance.ToggleSaves(false);
            AtMainMenu = true;
            InGame = false;
            Node3D the_tiles = main.instance.GetNode<Node3D>("/root/Underworld/tilemap");
            if (the_tiles != null)
            {
                UWTileMap.DestroyTileMapAndContents(the_tiles);
            }
        }        

    }//end class
}//end namespace