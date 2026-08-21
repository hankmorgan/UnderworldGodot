using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("GameOptions")]
        [Export]
        public TextureRect[] GameOptionButtonsUW1 = new TextureRect[7];
        [Export]
        public TextureRect[] GameOptionButtonsUW2 = new TextureRect[7];
        [Export]
        public TextureRect GameOptionsBackgroundUW1 = new TextureRect();
        [Export]
        public TextureRect GameOptionsBackgroundUW2 = new TextureRect();

        static InteractionModes PreviousInteractionMode;

        public enum OptionMenus
        {
            MainOptionMenu = 0,
            SaveMenu = 1,
            RestoreMenu = 2,
            MusicMenu = 3,
            SoundMenu = 4,
            DetailMenu = 5,
            Return = 6,
            Quit = 7,
            ConfirmQuit = 8
        }

        enum OptionButtonIndices
        {
            AllInteractionButtons = 0,
            AllOptionButtons = 1,
            AllSaveButtons = 2,
            QuitGame = 3,
            MusicButtons = 4,
            DetailButtons = 5,
            SaveGameOff = 6,
            SaveGameOn = 7,
            RestoreGameOff = 8,
            RestoreGameOn = 9,
            MusicOff = 10,
            MusicOn = 11,
            SoundOff = 12,
            SoundOn = 13,
            DetailOff = 14,
            DetailOn = 15,
            ReturnToGameOff = 16,
            ReturnToGameOn = 17,
            QuitGameOff = 18,
            QuitGameOn = 19,
            OnButtonOff = 20,
            OnButtonOn = 21,
            OffButtonOff = 22,
            OffButtonOn = 23,
            CancelOff = 24,
            CancelOn = 25,
            DoneOff = 26,
            DoneOn = 27,
            Save1Off = 30,
            Save1On = 31,
            Save2Off = 32,
            Save2On = 33,
            Save3Off = 34,
            Save3On = 35,
            Save4Off = 36,
            Save4On = 37,
            LowDetailOff = 38,
            LowDetailOn = 39,
            MediumDetailOff = 40,
            MediumDetailOn = 41,
            HighDetailOff = 42,
            HighDetailOn = 43,
            VHighDetailOff = 44,
            VHighDetailOn = 45,
            RestoreGameLabel = 46,
            MusicIsOnLabel = 47,
            MusicIsOffLabel = 48,
            SoundIsOnLabel = 49,
            SoundIsOffLabel = 50,
            TurnMusicLabel = 51,
            TurnSoundLabel = 52,
            DetailLowLabel = 53,
            DetailMedLabel = 54,
            DetailHighLabel = 55,
            DetailVHighLabel = 56,
            YesOff = 57,
            YesOn = 58,
            NoOff = 59,
            NoOn = 60
        }

        public static OptionMenus CurrentGameOptionMenu = OptionMenus.MainOptionMenu;

        private static ImageTexture[] UW2OptionButtons = new ImageTexture[61];


        public static TextureRect[] GameOptionButtons
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return instance.GameOptionButtonsUW2;
                }
                else
                {
                    return instance.GameOptionButtonsUW1;
                }
            }
        }

        public static TextureRect GameOptionBackground
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return instance.GameOptionsBackgroundUW2;
                }
                else
                {
                    return instance.GameOptionsBackgroundUW1;
                }
            }
        }

        public static void InitGameOptions()
        {
            if (UWClass._RES == UWClass.GAME_UW2)
            {

                Rect2I[] croppingareas = new Rect2I[11];

                //cropping areas for 7 button images
                croppingareas[0] = new Rect2I(1, 3, 76, 15);
                croppingareas[1] = new Rect2I(1, 19, 76, 15);
                croppingareas[2] = new Rect2I(1, 35, 76, 15);
                croppingareas[3] = new Rect2I(1, 51, 76, 15);
                croppingareas[4] = new Rect2I(1, 67, 76, 15);
                croppingareas[5] = new Rect2I(1, 83, 76, 15);
                croppingareas[6] = new Rect2I(1, 99, 76, 15);

                //croping areas for 4 button images
                croppingareas[7] = new Rect2I(1, 1, 76, 15);
                croppingareas[8] = new Rect2I(1, 17, 76, 15);
                croppingareas[9] = new Rect2I(1, 33, 76, 15);
                croppingareas[10] = new Rect2I(1, 49, 76, 15);

                //crop uw2 art
                UW2OptionButtons[(int)OptionButtonIndices.AllInteractionButtons] = grOptBtns.LoadImageAt(1);
                UW2OptionButtons[(int)OptionButtonIndices.AllOptionButtons] = grOptBtns.LoadImageAt(3);
                UW2OptionButtons[(int)OptionButtonIndices.AllSaveButtons] = grOptBtns.LoadImageAt(6);
                UW2OptionButtons[(int)OptionButtonIndices.QuitGame] = grOptBtns.LoadImageAt(5);
                UW2OptionButtons[(int)OptionButtonIndices.MusicButtons] = grOptBtns.LoadImageAt(7);
                UW2OptionButtons[(int)OptionButtonIndices.DetailButtons] = grOptBtns.LoadImageAt(4);
                UW2OptionButtons[(int)OptionButtonIndices.SaveGameOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(3).GetImage(), croppingareas[0]);
                UW2OptionButtons[(int)OptionButtonIndices.SaveGameOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(8).GetImage(), croppingareas[0]);
                UW2OptionButtons[(int)OptionButtonIndices.RestoreGameOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(3).GetImage(), croppingareas[1]);
                UW2OptionButtons[(int)OptionButtonIndices.RestoreGameOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(8).GetImage(), croppingareas[1]);
                UW2OptionButtons[(int)OptionButtonIndices.MusicOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(3).GetImage(), croppingareas[2]);
                UW2OptionButtons[(int)OptionButtonIndices.MusicOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(8).GetImage(), croppingareas[2]);
                UW2OptionButtons[(int)OptionButtonIndices.SoundOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(3).GetImage(), croppingareas[3]);
                UW2OptionButtons[(int)OptionButtonIndices.SoundOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(8).GetImage(), croppingareas[3]);
                UW2OptionButtons[(int)OptionButtonIndices.DetailOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(3).GetImage(), croppingareas[4]);
                UW2OptionButtons[(int)OptionButtonIndices.DetailOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(8).GetImage(), croppingareas[4]);
                UW2OptionButtons[(int)OptionButtonIndices.ReturnToGameOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(3).GetImage(), croppingareas[6]);
                UW2OptionButtons[(int)OptionButtonIndices.ReturnToGameOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(8).GetImage(), croppingareas[6]);
                UW2OptionButtons[(int)OptionButtonIndices.QuitGameOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(3).GetImage(), croppingareas[5]);
                UW2OptionButtons[(int)OptionButtonIndices.QuitGameOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(8).GetImage(), croppingareas[5]);
                UW2OptionButtons[(int)OptionButtonIndices.OnButtonOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(7).GetImage(), croppingareas[2]);
                UW2OptionButtons[(int)OptionButtonIndices.OnButtonOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(12).GetImage(), croppingareas[2]);
                UW2OptionButtons[(int)OptionButtonIndices.OffButtonOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(7).GetImage(), croppingareas[3]);
                UW2OptionButtons[(int)OptionButtonIndices.OffButtonOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(12).GetImage(), croppingareas[3]);
                UW2OptionButtons[(int)OptionButtonIndices.CancelOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(6).GetImage(), croppingareas[5]);
                UW2OptionButtons[(int)OptionButtonIndices.CancelOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(11).GetImage(), croppingareas[5]);
                UW2OptionButtons[(int)OptionButtonIndices.DoneOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(7).GetImage(), croppingareas[4]);
                UW2OptionButtons[(int)OptionButtonIndices.DoneOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(12).GetImage(), croppingareas[4]);
                UW2OptionButtons[(int)OptionButtonIndices.Save1Off] = ArtLoader.CropImage(grOptBtns.LoadImageAt(6).GetImage(), croppingareas[1]);
                UW2OptionButtons[(int)OptionButtonIndices.Save1On] = ArtLoader.CropImage(grOptBtns.LoadImageAt(11).GetImage(), croppingareas[1]);
                UW2OptionButtons[(int)OptionButtonIndices.Save2Off] = ArtLoader.CropImage(grOptBtns.LoadImageAt(6).GetImage(), croppingareas[2]);
                UW2OptionButtons[(int)OptionButtonIndices.Save2On] = ArtLoader.CropImage(grOptBtns.LoadImageAt(11).GetImage(), croppingareas[2]);
                UW2OptionButtons[(int)OptionButtonIndices.Save3Off] = ArtLoader.CropImage(grOptBtns.LoadImageAt(6).GetImage(), croppingareas[3]);
                UW2OptionButtons[(int)OptionButtonIndices.Save3On] = ArtLoader.CropImage(grOptBtns.LoadImageAt(11).GetImage(), croppingareas[3]);
                UW2OptionButtons[(int)OptionButtonIndices.Save4Off] = ArtLoader.CropImage(grOptBtns.LoadImageAt(6).GetImage(), croppingareas[4]);
                UW2OptionButtons[(int)OptionButtonIndices.Save4On] = ArtLoader.CropImage(grOptBtns.LoadImageAt(11).GetImage(), croppingareas[4]);
                UW2OptionButtons[(int)OptionButtonIndices.LowDetailOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(4).GetImage(), croppingareas[2]);
                UW2OptionButtons[(int)OptionButtonIndices.LowDetailOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(9).GetImage(), croppingareas[2]);
                UW2OptionButtons[(int)OptionButtonIndices.MediumDetailOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(4).GetImage(), croppingareas[3]);
                UW2OptionButtons[(int)OptionButtonIndices.MediumDetailOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(9).GetImage(), croppingareas[3]);
                UW2OptionButtons[(int)OptionButtonIndices.HighDetailOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(4).GetImage(), croppingareas[4]);
                UW2OptionButtons[(int)OptionButtonIndices.HighDetailOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(9).GetImage(), croppingareas[4]);
                UW2OptionButtons[(int)OptionButtonIndices.VHighDetailOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(4).GetImage(), croppingareas[5]);
                UW2OptionButtons[(int)OptionButtonIndices.VHighDetailOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(9).GetImage(), croppingareas[5]);
                UW2OptionButtons[(int)OptionButtonIndices.RestoreGameLabel] = ArtLoader.CropImage(grOptBtns.LoadImageAt(14).GetImage(), croppingareas[8]);
                UW2OptionButtons[(int)OptionButtonIndices.MusicIsOnLabel] = ArtLoader.CropImage(grOptBtns.LoadImageAt(15).GetImage(), croppingareas[8]);
                UW2OptionButtons[(int)OptionButtonIndices.MusicIsOffLabel] = ArtLoader.CropImage(grOptBtns.LoadImageAt(15).GetImage(), croppingareas[7]);
                UW2OptionButtons[(int)OptionButtonIndices.SoundIsOnLabel] = ArtLoader.CropImage(grOptBtns.LoadImageAt(15).GetImage(), croppingareas[10]);
                UW2OptionButtons[(int)OptionButtonIndices.SoundIsOffLabel] = ArtLoader.CropImage(grOptBtns.LoadImageAt(15).GetImage(), croppingareas[9]);
                UW2OptionButtons[(int)OptionButtonIndices.TurnMusicLabel] = ArtLoader.CropImage(grOptBtns.LoadImageAt(7).GetImage(), croppingareas[1]);
                UW2OptionButtons[(int)OptionButtonIndices.TurnSoundLabel] = ArtLoader.CropImage(grOptBtns.LoadImageAt(14).GetImage(), croppingareas[9]);
                UW2OptionButtons[(int)OptionButtonIndices.DetailLowLabel] = ArtLoader.CropImage(grOptBtns.LoadImageAt(13).GetImage(), croppingareas[7]);
                UW2OptionButtons[(int)OptionButtonIndices.DetailMedLabel] = ArtLoader.CropImage(grOptBtns.LoadImageAt(13).GetImage(), croppingareas[8]);
                UW2OptionButtons[(int)OptionButtonIndices.DetailHighLabel] = ArtLoader.CropImage(grOptBtns.LoadImageAt(13).GetImage(), croppingareas[9]);
                UW2OptionButtons[(int)OptionButtonIndices.DetailVHighLabel] = ArtLoader.CropImage(grOptBtns.LoadImageAt(13).GetImage(), croppingareas[10]);
                UW2OptionButtons[(int)OptionButtonIndices.YesOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(5).GetImage(), croppingareas[2]);
                UW2OptionButtons[(int)OptionButtonIndices.YesOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(10).GetImage(), croppingareas[2]);
                UW2OptionButtons[(int)OptionButtonIndices.NoOff] = ArtLoader.CropImage(grOptBtns.LoadImageAt(5).GetImage(), croppingareas[3]);
                UW2OptionButtons[(int)OptionButtonIndices.NoOn] = ArtLoader.CropImage(grOptBtns.LoadImageAt(10).GetImage(), croppingareas[3]);


            }
            EnableDisable(GameOptionBackground, false);
            for (int i = 0; i <= GameOptionButtons.GetUpperBound(0); i++)
            {
                EnableDisable(GameOptionButtons[i], false);
            }
        }

        /// <summary>
        /// Shows or hides the buttons.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="index">-1 do all buttons</param>
        public static void ToggleOptionButtons(bool state, int index = -1)
        {
            if (index == -1)
            {
                for (int i = 0; i <= GameOptionButtons.GetUpperBound(0); i++)
                {
                    EnableDisable(GameOptionButtons[i], state);
                }
            }
            else
            {
                EnableDisable(GameOptionButtons[index], state);
            }
        }



        public static void SetGameOptionButtons(int[] buttonindices)
        {
            for (int i = 0; i <= GameOptionButtons.GetUpperBound(0); i++)
            {
                if (buttonindices[i] != -1)
                {
                    GameOptionButtons[i].Texture = GetOptionButtonImage(buttonindices[i]);
                    EnableDisable(GameOptionButtons[i], true);
                }
                else
                {
                    EnableDisable(GameOptionButtons[i], false);
                }
            }
        }

        static void SetGameOptionsBackground(int index)
        {
            if (index != -1)
            {
                GameOptionBackground.Texture = GetOptionButtonImage(index);
                EnableDisable(GameOptionBackground, true);
            }
            else
            {
                EnableDisable(GameOptionBackground, false);
            }
        }

        static ImageTexture GetOptionButtonImage(int index)
        {
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                return UW2OptionButtons[index];
            }
            else
            {
                return grOptBtns.LoadImageAt(index);
            }
        }


        private void _on_game_options_input(InputEvent @event, long extra_arg_0)
        {
            // These buttons get gui_input straight from the scene, so the usual input block
            // does not reach them. Without this you can click another slot, or Cancel, while
            // still typing a description.
            if (SaveDescriptionPromptActive) return;

            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
            {
                switch (CurrentGameOptionMenu)
                {
                    case OptionMenus.MainOptionMenu:
                        {   //at main menu. will switch to menu specified by arg0
                            switch (extra_arg_0)
                            {
                                case 0://switch to save menu
                                    {
                                        CurrentGameOptionMenu = OptionMenus.SaveMenu;
                                        SetGameOptionsBackground((int)OptionButtonIndices.AllSaveButtons);
                                        SetGameOptionButtons(
                                            new int[]{
                                                (int)OptionButtonIndices.SaveGameOff,
                                                (int)OptionButtonIndices.Save1Off,
                                                (int)OptionButtonIndices.Save2Off,
                                                (int)OptionButtonIndices.Save3Off,
                                                (int)OptionButtonIndices.Save4Off,
                                                (int)OptionButtonIndices.CancelOff,
                                                -1});
                                        listsaves();
                                        break;
                                    }
                                case 1://switch to restore menu
                                    {
                                        CurrentGameOptionMenu = OptionMenus.RestoreMenu;
                                        SetGameOptionsBackground((int)OptionButtonIndices.AllSaveButtons);
                                        SetGameOptionButtons(
                                            new int[]{
                                                (int)OptionButtonIndices.RestoreGameLabel,
                                                (int)OptionButtonIndices.Save1Off,
                                                (int)OptionButtonIndices.Save2Off,
                                                (int)OptionButtonIndices.Save3Off,
                                                (int)OptionButtonIndices.Save4Off,
                                                (int)OptionButtonIndices.CancelOff,
                                                -1});
                                        listsaves();
                                        break;
                                    }
                                case 2: // switch to music options
                                    {
                                        CurrentGameOptionMenu = OptionMenus.MusicMenu;
                                        SetGameOptionsBackground((int)OptionButtonIndices.MusicButtons);
                                        if (playerdat.MusicEnabled)
                                        {
                                            SetGameOptionButtons(new int[]{
                                                (int)OptionButtonIndices.MusicIsOnLabel,
                                                (int)OptionButtonIndices.TurnMusicLabel,
                                                (int)OptionButtonIndices.OnButtonOn,
                                                (int)OptionButtonIndices.OffButtonOff,
                                                (int)OptionButtonIndices.DoneOff,
                                                -1,-1 });
                                        }
                                        else
                                        {
                                            SetGameOptionButtons(new int[]{
                                                (int)OptionButtonIndices.MusicIsOffLabel,
                                                (int)OptionButtonIndices.TurnMusicLabel,
                                                (int)OptionButtonIndices.OnButtonOff,
                                                (int)OptionButtonIndices.OffButtonOn,
                                                (int)OptionButtonIndices.DoneOff,
                                                -1,-1 });
                                        }

                                        break;
                                    }
                                case 3: // switch to sound options
                                    {
                                        CurrentGameOptionMenu = OptionMenus.SoundMenu;
                                        SetGameOptionsBackground((int)OptionButtonIndices.MusicButtons);
                                        if (playerdat.SoundEffectsEnabled)
                                        {
                                            SetGameOptionButtons(new int[]{
                                                (int)OptionButtonIndices.SoundIsOnLabel,
                                                (int)OptionButtonIndices.TurnSoundLabel,
                                                (int)OptionButtonIndices.OnButtonOn,
                                                (int)OptionButtonIndices.OffButtonOff,
                                                (int)OptionButtonIndices.DoneOff,
                                                -1,-1 });
                                        }
                                        else
                                        {
                                            SetGameOptionButtons(new int[]{
                                                (int)OptionButtonIndices.SoundIsOffLabel,
                                                (int)OptionButtonIndices.TurnSoundLabel,
                                                (int)OptionButtonIndices.OnButtonOff,
                                                (int)OptionButtonIndices.OffButtonOn,
                                                (int)OptionButtonIndices.DoneOff,
                                                -1,-1 });
                                        }

                                        break;
                                    }
                                case 4: //detail options.
                                    {
                                        SetupDetailMenu();
                                        break;
                                    }
                                case 5: // return to game
                                    {
                                        ReturnToGameFromOptions();
                                        break;
                                    }
                                case 6://quit game
                                    {
                                        CurrentGameOptionMenu = OptionMenus.ConfirmQuit;
                                        SetGameOptionsBackground((int)OptionButtonIndices.QuitGame);
                                        SetGameOptionButtons(
                                            new int[]{
                                                -1,
                                                -1,
                                                (int)OptionButtonIndices.YesOff,
                                                (int)OptionButtonIndices.NoOff,
                                                -1,
                                                -1,
                                                -1});
                                        break;
                                    }
                            }

                            break;
                        }
                    case OptionMenus.SaveMenu:
                        {
                            switch (extra_arg_0)
                            {
                                case 1:
                                case 2:
                                case 3:
                                case 4://save to chosen slot
                                    {
                                        if (UWClass._RES != UWClass.GAME_UW1)
                                        {
                                            // UW2 save is unsupported pending an upstream UW2 lev.ark compressor.
                                            // Writing uncompressed UW2 blocks would fail DOS load (>80 uncompressed
                                            // blocks crash vanilla UW2.EXE). Until the compressor is ported, refuse.
                                            //
                                            // Refused before the prompt opens: asking for a name and then failing
                                            // on purpose would be worse than failing straight away.
                                            GD.PrintErr("UW2 save pending upstream compressor — not yet supported");
                                            listsaves();
                                            instance.scroll.Clear();
                                            AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_save_game_failed_));
                                            ReturnToGameFromOptions();
                                            break;
                                        }

                                        // Ask for the description first. Saving happens on Enter, in
                                        // OnSaveDescriptionSubmitted; Escape abandons it entirely.
                                        BeginSaveDescription((int)extra_arg_0);
                                        break;
                                    }
                                case 5://cancel and return to top
                                    {
                                        ReturnToTopOptionsMenu();
                                        break;
                                    }
                            }
                            break;
                        }
                    case OptionMenus.RestoreMenu:
                        {
                            switch (extra_arg_0)
                            {
                                case 1:
                                case 2:
                                case 3:
                                case 4://try and restore game
                                    {
                                        var path = System.IO.Path.Combine(UWClass.BasePath, $"SAVE{extra_arg_0}", "LEV.ARK");
                                        if (System.IO.File.Exists(path))
                                        {
                                            JourneyOnwards($"SAVE{extra_arg_0}");
                                            instance.scroll.Clear();
                                        }
                                        else
                                        {
                                            instance.scroll.Clear();
                                            AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_no_save_game_there_));
                                        }
                                        ReturnToGameFromOptions();
                                        if (UWClass._RES != UWClass.GAME_UW2)
                                        {//UW1 will only change theme on in-game save load. Not when loading from main menu. See JourneyOnwards() for change of theme handling in UW2
                                            //XMIMusic.ChangeTheme(XMIMusic.PickLevelThemeMusic(0));
                                            XMIMusic.PickLevelThemeMusic(0);
                                        }
                                        break;
                                    }
                                case 5://cancel and return to top
                                    {
                                        ReturnToTopOptionsMenu();
                                        break;
                                    }
                            }
                            break;
                        }
                    case OptionMenus.MusicMenu:
                        {
                            switch (extra_arg_0)
                            {
                                case 2://turn on
                                    {
                                        playerdat.MusicEnabled = true;
                                        if (MusicStreamPlayer.Instance != null)
                                        {//restart music if not already playing.
                                            //XMIMusic.ChangeTheme(XMIMusic.PickLevelThemeMusic());
                                            XMIMusic.PickLevelThemeMusic(0);
                                        }
                                        SetGameOptionButtons(new int[]{
                                                (int)OptionButtonIndices.MusicIsOnLabel,
                                                (int)OptionButtonIndices.TurnMusicLabel,
                                                (int)OptionButtonIndices.OnButtonOn,
                                                (int)OptionButtonIndices.OffButtonOff,
                                                (int)OptionButtonIndices.DoneOff,
                                                -1,-1 });
                                        break;
                                    }
                                case 3: //turn off
                                    {
                                        playerdat.MusicEnabled = false;
                                        MusicStreamPlayer.Instance?.Stop();
                                        SetGameOptionButtons(new int[]{
                                            (int)OptionButtonIndices.MusicIsOffLabel,
                                            (int)OptionButtonIndices.TurnMusicLabel,
                                            (int)OptionButtonIndices.OnButtonOff,
                                            (int)OptionButtonIndices.OffButtonOn,
                                            (int)OptionButtonIndices.DoneOff,
                                            -1,-1 });
                                        break;
                                    }
                                case 4: // done
                                    {
                                        ReturnToTopOptionsMenu();
                                        break;
                                    }
                            }
                            break;
                        }
                    case OptionMenus.SoundMenu:
                        {
                            switch (extra_arg_0)
                            {
                                case 2://turn on
                                    {
                                        playerdat.SoundEffectsEnabled = true;
                                        SetGameOptionButtons(new int[]{
                                                (int)OptionButtonIndices.SoundIsOnLabel,
                                                (int)OptionButtonIndices.TurnSoundLabel,
                                                (int)OptionButtonIndices.OnButtonOn,
                                                (int)OptionButtonIndices.OffButtonOff,
                                                (int)OptionButtonIndices.DoneOff,
                                                -1,-1 });
                                        break;
                                    }
                                case 3: //turn off
                                    {
                                        playerdat.SoundEffectsEnabled = false;
                                        SetGameOptionButtons(new int[]{
                                            (int)OptionButtonIndices.SoundIsOffLabel,
                                            (int)OptionButtonIndices.TurnSoundLabel,
                                            (int)OptionButtonIndices.OnButtonOff,
                                            (int)OptionButtonIndices.OffButtonOn,
                                            (int)OptionButtonIndices.DoneOff,
                                            -1,-1 });
                                        break;
                                    }
                                case 4: // done
                                    {
                                        ReturnToTopOptionsMenu();
                                        break;
                                    }
                            }
                            break;
                        }
                    case OptionMenus.DetailMenu:
                        {
                            switch (extra_arg_0)
                            {
                                case 2://low detail.
                                case 3://med
                                case 4://high
                                case 5://vhigh
                                    if (playerdat.DetailLevel != (byte)(extra_arg_0 - 2))
                                    {
                                        playerdat.DetailLevel = (byte)(extra_arg_0 - 2);                                         
                                        SetupDetailMenu();  
                                    }     
                                    break;
                                case 6://done
                                    ReturnToTopOptionsMenu(); break;                                
                            }
                            break;
                        }

                    case OptionMenus.ConfirmQuit:
                        {
                            switch (extra_arg_0)
                            {
                                case 2://confirm quit yes
                                    GetTree().Quit();
                                    break;
                                case 3://cancel quit
                                    ReturnToTopOptionsMenu();
                                    break;
                            }
                            break;
                        }

                }
            }
        }

        private static void SetupDetailMenu()
        {
            CurrentGameOptionMenu = OptionMenus.DetailMenu;
            var lowbutton = (int)OptionButtonIndices.LowDetailOff;
            var medbutton = (int)OptionButtonIndices.MediumDetailOff;
            var highbutton = (int)OptionButtonIndices.HighDetailOff;
            var vhighbutton = (int)OptionButtonIndices.VHighDetailOff;
            switch (playerdat.DetailLevel)
            {
                case 0: lowbutton = (int)OptionButtonIndices.LowDetailOn; break;
                case 1: medbutton = (int)OptionButtonIndices.MediumDetailOn; break;
                case 2: highbutton = (int)OptionButtonIndices.HighDetailOn; break;
                case 3: vhighbutton = (int)OptionButtonIndices.VHighDetailOn; break;
            }
            SetGameOptionsBackground((int)OptionButtonIndices.DetailButtons);
            SetGameOptionButtons(new int[]{
                                            (int)OptionButtonIndices.DetailLowLabel + playerdat.DetailLevel,
                                            -1,  //no turn detail label unless i crop image 5
                                            lowbutton,
                                            medbutton,
                                            highbutton,
                                            vhighbutton,
                                            (int)OptionButtonIndices.DoneOff});
        }


        public static void ReturnToGameFromOptions()
        {
            uimanager.CurrentGameMode = GameModes.GAME;
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                EnableDisable(instance.PanelInventory, true);
                PanelMode = 0;
            }
            if (PreviousInteractionMode != InteractionMode)
            {
                if (PreviousInteractionMode != InteractionModes.ModeAttack)
                {
                    InteractionModeToggle(PreviousInteractionMode);      
                }
                else
                {
                    InteractionMode = InteractionModes.ModeAttack;
                    //Turn on attack interaction button.
                    if (UWClass._RES == UWClass.GAME_UW2)
                    {
                        instance.InteractionButtonsUW2[(int)(InteractionModes.ModeAttack)].Texture = instance.UW2InteractionBtnsOff[(int)(InteractionModes.ModeAttack)];
                    }
                    else
                    {
                        instance.InteractionButtonsUW1[(int)(InteractionModes.ModeAttack)].Texture = grLfti.LoadImageAt((int)(InteractionModes.ModeAttack) * 2, false);
                    }
                }
               
            } 
            else
            {
                ToggleInteractionButtonDisplay(InteractionMode);//only visualy change.
            }       
            InteractionModeShowHide(true);
            SetGameOptionsBackground(-1);
            SetGameOptionButtons(
                new int[]{
                                                -1,
                                                -1,
                                                -1,
                                                -1,
                                                -1,
                                                -1,
                                                -1});

        }


        private static void ReturnToTopOptionsMenu()
        {
            uimanager.CurrentGameMode = GameModes.OPTIONS;
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                EnableDisable(instance.PanelInventory, false);
                EnableDisable(instance.PanelRuneBag, false);
                EnableDisable(instance.PanelStats, false);
            }
            CurrentGameOptionMenu = OptionMenus.MainOptionMenu;
            SetGameOptionsBackground((int)OptionButtonIndices.AllOptionButtons);
            SetGameOptionButtons(
                new int[]{
                    (int)OptionButtonIndices.SaveGameOff,
                    (int)OptionButtonIndices.RestoreGameOff,
                    (int)OptionButtonIndices.MusicOff,
                    (int)OptionButtonIndices.SoundOff,
                    (int)OptionButtonIndices.DetailOff,
                    (int)OptionButtonIndices.ReturnToGameOff,
                    (int)OptionButtonIndices.QuitGameOff});
        }

        // ---- save description prompt ------------------------------------------------

        private static readonly SaveDescriptionPrompt SaveDescription_Prompt = new();
        private static LineEdit SaveDescriptionField;

        public static bool SaveDescriptionPromptActive => SaveDescription_Prompt.Active;

        /// <summary>
        /// Forgets any prompt belonging to a previous scene. Called as the UI comes up.
        /// </summary>
        public static void ResetSaveDescriptionPrompt()
        {
            if (SaveDescription_Prompt.Active) SaveDescription_Prompt.Cancel();
            SaveDescriptionField = null;
            RestoringSaveDescriptionText = false;
        }

        /// <summary>
        /// Asks for the description before saving, the way DOS does. Nothing is written
        /// until the player presses Enter, because SaveGame.Save mutates live state and
        /// creates the slot directory as soon as it is called.
        /// </summary>
        private static void BeginSaveDescription(int slot)
        {
            string existing = "";
            var descPath = System.IO.Path.Combine(UWClass.BasePath, $"SAVE{slot}", "DESC");
            SaveDescription.TryReadSlot(descPath, out existing);

            SaveDescription_Prompt.Open(slot, existing);

            var field = EnsureSaveDescriptionField();
            field.MaxLength = SaveDescription.MaxLength;
            field.Text = SaveDescription_Prompt.Buffer;
            EnableDisable(field, true);

            instance.scroll.Clear();
            AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_please_enter_a_save_file_description_));

            // Focus and selection are deferred together and carry the generation they were
            // queued with, so an Escape or a keystroke in between cannot leave them acting
            // on a prompt that has gone.
            int generation = SaveDescription_Prompt.Generation;
            instance.CallDeferred(nameof(FocusSaveDescriptionField), generation);
        }

        private void FocusSaveDescriptionField(int generation)
        {
            if (!SaveDescription_Prompt.MayRunDeferred(generation)) return;
            if (SaveDescriptionField == null || !GodotObject.IsInstanceValid(SaveDescriptionField)) return;

            SaveDescriptionField.GrabFocus();
            SaveDescriptionField.SelectAll();
        }

        private static LineEdit EnsureSaveDescriptionField()
        {
            // The scene can be loaded more than once, from the launcher, so a field built
            // for a previous one is a freed node by now. Rebuild rather than touch it.
            if (SaveDescriptionField != null && !GodotObject.IsInstanceValid(SaveDescriptionField))
            {
                SaveDescriptionField = null;
            }
            if (SaveDescriptionField != null) return SaveDescriptionField;

            // Built here rather than in the scene: the shared TypedInput has
            // selecting_enabled off, and borrowing it would mean lending conversations and
            // chargen a 30 character limit.
            SaveDescriptionField = new LineEdit
            {
                Name = "SaveDescriptionInput",
                MaxLength = SaveDescription.MaxLength,
                ContextMenuEnabled = false,
                ShortcutKeysEnabled = false,
                MiddleMousePasteEnabled = false,
                SelectingEnabled = true,
                Theme = instance.TypedInput?.Theme,
                Visible = false,
            };
            SaveDescriptionField.TextChanged += OnSaveDescriptionTextChanged;
            SaveDescriptionField.TextSubmitted += OnSaveDescriptionSubmitted;
            SaveDescriptionField.GuiInput += OnSaveDescriptionGuiInput;

            var parent = instance.TypedInput?.GetParent() ?? (Node)instance;
            parent.AddChild(SaveDescriptionField);
            SaveDescriptionField.Position = instance.TypedInput?.Position ?? Vector2.Zero;
            SaveDescriptionField.Size = new Vector2(600, 31);
            return SaveDescriptionField;
        }

        private static bool RestoringSaveDescriptionText = false;

        private static void OnSaveDescriptionTextChanged(string newText)
        {
            if (RestoringSaveDescriptionText) return;

            // Godot has already applied the text, so anything unusable is undone rather than
            // prevented. MaxLength has trimmed an over-long paste before we get here.
            if (SaveDescription_Prompt.TryAccept(newText))
            {
                RestoringSaveDescriptionText = true;
                SaveDescriptionField.Text = SaveDescription_Prompt.Buffer;
                SaveDescriptionField.CaretColumn = SaveDescription_Prompt.Buffer.Length;
                SaveDescriptionField.Deselect();
                RestoringSaveDescriptionText = false;
            }
        }

        private static void OnSaveDescriptionGuiInput(InputEvent @event)
        {
            if (!SaveDescription_Prompt.Active) return;

            // Only something the player did on purpose ends the initial selection. Pointer
            // motion and key releases arrive here too and must not count, or moving the
            // mouse across the field would cancel the select-all.
            bool actionable =
                (@event is InputEventKey k && k.Pressed) ||
                (@event is InputEventMouseButton m && m.Pressed) ||
                @event is InputEventScreenTouch;

            if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Backspace)
            {
                // DOS treats Backspace on a freshly opened slot as editing the existing name
                // rather than replacing it. Drop the selection and let the same event through
                // so it deletes one character, not the lot.
                if (SaveDescription_Prompt.BeginEditingPrefill())
                {
                    SaveDescriptionField.Deselect();
                    SaveDescriptionField.CaretColumn = SaveDescriptionField.Text.Length;
                }
                return;
            }

            if (actionable) SaveDescription_Prompt.NoteInteraction();
        }

        private static void OnSaveDescriptionSubmitted(string text)
        {
            if (!SaveDescription_Prompt.Active) return;

            int slot = SaveDescription_Prompt.Slot;
            string description = SaveDescription_Prompt.Commit();
            CloseSaveDescriptionField();

            int stringId;
            try
            {
                SaveGame.Save(slot, description);
                stringId = GameStrings.str_save_game_succeeded_;
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"SaveGame.Save failed: {ex}");
                stringId = GameStrings.str_save_game_failed_;
            }

            listsaves();
            instance.scroll.Clear();
            AddToMessageScroll(GameStrings.GetString(1, stringId));
            ReturnToGameFromOptions();
        }

        /// <summary>Escape. DOS abandons the whole save and reports failure.</summary>
        public static void CancelSaveDescription()
        {
            if (!SaveDescription_Prompt.Active) return;

            SaveDescription_Prompt.Cancel();
            CloseSaveDescriptionField();

            listsaves();
            instance.scroll.Clear();
            AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_save_game_failed_));
            ReturnToGameFromOptions();
        }

        private static void CloseSaveDescriptionField()
        {
            if (SaveDescriptionField == null || !GodotObject.IsInstanceValid(SaveDescriptionField)) return;
            SaveDescriptionField.ReleaseFocus();
            SaveDescriptionField.Deselect();
            SaveDescriptionField.Text = "";
            EnableDisable(SaveDescriptionField, false);
        }

        static void listsaves()
        {
            string[] romannumerals = new string[] { "I", "II", "III", "IV" };
            instance.scroll.Clear();
            for (int i = 1; i <= 4; i++)
            {
                var path = System.IO.Path.Combine(UWClass.BasePath, $"SAVE{i}", "DESC");
                if (SaveDescription.TryReadSlot(path, out string savename))
                {
                    AddToMessageScroll($"{romannumerals[i - 1]}- {savename}", colour: 2);
                }
                else
                {
                    AddToMessageScroll($"{romannumerals[i - 1]}- <not used yet>", colour: 2);
                }
            }
        }

    }//end class
}//end namespace