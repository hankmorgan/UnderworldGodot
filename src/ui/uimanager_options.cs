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
                                            AddToMessageScroll(GameStringFormat.StripDisplayCodes(GameStrings.GetString(1, GameStrings.str_save_game_failed_)));
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

        /// <summary>
        /// The cursor is an image because DOS does not draw it from the font either. Its
        /// input routine fills a rectangle straight into video memory
        /// (UW1_asm.asm:139815-139838, calling the fill at seg003_54EB), which is why no UW
        /// font carries a block glyph: FONT5X6P holds ASCII 0x20 to 0x7E and nothing else.
        ///
        /// Measured off a DOS screenshot: 5 by 6 pixels, exactly one character cell. The
        /// converted font has 1024 units per em with an advance of 64 per original pixel,
        /// so at the scroll's font size of 64 one original pixel is four here.
        ///
        /// bgcolor was tried first and cannot work: the scroll sets line_separation to -24
        /// against a 64 font, so the box reaches up into the line above at any size.
        /// </summary>
        /// <summary>
        /// The image carries transparent padding above the block, because an inline image
        /// has no vertical alignment control and sat 14 too high measured against DOS. The
        /// drawn size therefore includes that padding.
        /// </summary>
        private const string CursorImage = "res://resources/textcursor.png";
        private const int CursorWidth = 5 * 4;
        private const int CursorHeight = 6 * 4 + 14;

        /// <summary>
        /// What an empty slot shows in the list, and what its prompt starts with.
        ///
        /// DOS keeps it as a real name if the player just presses Enter: a slot saved that
        /// way holds the 14 bytes "&lt;not used yet&gt;". Odd, but checked, so it is left alone
        /// rather than treated as an empty description.
        /// </summary>
        public const string UnusedSlotPlaceholder = "<not used yet>";

        private static readonly SaveDescriptionPrompt SaveDescription_Prompt = new();
        private static LineEdit SaveDescriptionField;

        public static bool SaveDescriptionPromptActive => SaveDescription_Prompt.Active;

        /// <summary>
        /// What the scroll shows in place of {TYPEDINPUT} while the prompt is open: the
        /// text with a block cursor sitting on the character at the caret, which is what
        /// DOS draws. A bare "|" cannot show which character the caret is on, and it never
        /// moved because arrow keys change the caret without changing the text.
        /// </summary>
        public static string SaveDescriptionText
        {
            get
            {
                if (SaveDescriptionField == null || !GodotObject.IsInstanceValid(SaveDescriptionField))
                {
                    return SaveDescription_Prompt.Buffer;
                }

                string text = SaveDescriptionField.Text;

                // While the prefill is still selected DOS parks the cursor at the end of it,
                // not over its first character. The selection is what makes typing replace
                // the name, so it stays; only where the cursor is drawn changes.
                int caret = SaveDescription_Prompt.SelectionPending
                    ? text.Length
                    : System.Math.Clamp(SaveDescriptionField.CaretColumn, 0, text.Length);

                string before = text.Substring(0, caret);
                string after = caret < text.Length ? text.Substring(caret + 1) : "";

                // The character it covers is dropped rather than drawn under the block,
                // matching DOS, and the block's fixed size keeps the text from shifting as
                // the caret crosses letters of different widths.
                return $"{before}[img={CursorWidth}x{CursorHeight}]{CursorImage}[/img]{after}";
            }
        }

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
            var descPath = System.IO.Path.Combine(UWClass.BasePath, $"SAVE{slot}", "DESC");
            string existing = SaveDescription.TryReadSlot(descPath, out string stored)
                ? stored
                : UnusedSlotPlaceholder;

            SaveDescription_Prompt.Open(slot, existing);

            var field = EnsureSaveDescriptionField();
            field.MaxLength = SaveDescription.MaxLength;
            field.Text = SaveDescription_Prompt.Buffer;
            EnableDisable(field, true);

            instance.scroll.Clear();
            AddToMessageScroll(
                GameStringFormat.StripDisplayCodes(
                    GameStrings.GetString(1, GameStrings.str_please_enter_a_save_file_description_)));
            // Only the slot being named. The whole list is what the menu showed a moment
            // ago; repeating it here just buries the line being edited.
            AddToMessageScroll(">{TYPEDINPUT}", colour: 2,
                               mode: MessageDisplay.MessageDisplayMode.TypedInput);

            // Focus and selection are deferred together and carry the generation they were
            // queued with, so an Escape or a keystroke in between cannot leave them acting
            // on a prompt that has gone.
            int generation = SaveDescription_Prompt.Generation;
            Callable.From(() => FocusSaveDescriptionField(generation)).CallDeferred();
        }

        private static void FocusSaveDescriptionField(int generation)
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
            SaveDescriptionField.FocusExited += OnSaveDescriptionFocusExited;

            // Sits where the existing typed-input proxy sits and is the same size. What the
            // player reads is the {TYPEDINPUT} substitution in the message scroll, exactly as
            // for the quantity and conversation prompts; the field only collects the keys.
            var parent = instance.TypedInput?.GetParent() ?? (Node)instance;
            parent.AddChild(SaveDescriptionField);
            if (instance.TypedInput != null)
            {
                SaveDescriptionField.Position = instance.TypedInput.Position;
                SaveDescriptionField.Size = instance.TypedInput.Size;
            }
            return SaveDescriptionField;
        }

        private static void RefreshSaveDescriptionLine()
        {
            if (SaveDescription_Prompt.Active) instance.scroll.UpdateMessageDisplay();
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
            instance.scroll.UpdateMessageDisplay();
        }

        /// <summary>
        /// Keys that edit or move within the text rather than adding to it. While the
        /// prefilled name is still selected these end the selection instead of replacing it.
        /// </summary>
        private static bool IsEditingKey(Key code) =>
            code == Key.Backspace || code == Key.Delete ||
            code == Key.Left || code == Key.Right ||
            code == Key.Home || code == Key.End;

        private static void OnSaveDescriptionGuiInput(InputEvent @event)
        {
            if (!SaveDescription_Prompt.Active) return;

            // Only something the player did on purpose ends the initial selection. Pointer
            // motion and key releases arrive here too and must not count, or moving the
            // mouse across the field would cancel the select-all.
            bool keyPress = @event is InputEventKey k && k.Pressed;
            bool actionable =
                keyPress ||
                (@event is InputEventMouseButton m && m.Pressed) ||
                @event is InputEventScreenTouch;

            if (keyPress && IsEditingKey(((InputEventKey)@event).Keycode)
                && SaveDescription_Prompt.BeginEditingPrefill())
            {
                // A key that edits or moves rather than types means the player wants the
                // existing name, not a replacement. Collapse the selection to the END and
                // let the same event through, so Backspace deletes the last character and
                // Left steps back from the end.
                //
                // This has to be done here: Godot collapses a selection to its START, so
                // Left would otherwise jump the caret to position 0 rather than move it.
                SaveDescriptionField.Deselect();
                SaveDescriptionField.CaretColumn = SaveDescriptionField.Text.Length;
            }
            else if (actionable)
            {
                SaveDescription_Prompt.NoteInteraction();
            }

            // Redraw after the LineEdit has acted on the event. Arrows, Home, End and a
            // mouse click move the caret without changing the text, so TextChanged never
            // fires and the drawn cursor would sit still. This must not sit behind an early
            // return: doing so left exactly those keys unable to move the cursor.
            if (@event is InputEventKey || @event is InputEventMouseButton)
            {
                Callable.From(RefreshSaveDescriptionLine).CallDeferred();
            }
        }

        /// <summary>
        /// The prompt is modal, so the field keeps focus until it closes. Without this,
        /// anything that steals focus (Tab, a click elsewhere) leaves the player unable to
        /// type while the option buttons are still blocked, with only Escape to get out.
        /// </summary>
        private static void OnSaveDescriptionFocusExited()
        {
            if (!SaveDescription_Prompt.Active) return;
            if (SaveDescriptionField == null || !GodotObject.IsInstanceValid(SaveDescriptionField)) return;
            Callable.From(() =>
            {
                if (SaveDescription_Prompt.Active
                    && SaveDescriptionField != null
                    && GodotObject.IsInstanceValid(SaveDescriptionField)
                    && !SaveDescriptionField.HasFocus())
                {
                    SaveDescriptionField.GrabFocus();
                }
            }).CallDeferred();
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
            AddToMessageScroll(GameStringFormat.StripDisplayCodes(GameStrings.GetString(1, stringId)));
            ReturnToGameFromOptions();
        }

        /// <summary>
        /// Escape. DOS abandons the whole save, reports failure, and returns to the game
        /// rather than staying on the save menu. Checked in UW1.
        /// </summary>
        public static void CancelSaveDescription()
        {
            if (!SaveDescription_Prompt.Active) return;

            SaveDescription_Prompt.Cancel();
            CloseSaveDescriptionField();

            listsaves();
            instance.scroll.Clear();
            AddToMessageScroll(GameStringFormat.StripDisplayCodes(GameStrings.GetString(1, GameStrings.str_save_game_failed_)));
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
                    AddToMessageScroll($"{romannumerals[i - 1]}- {UnusedSlotPlaceholder}", colour: 2);
                }
            }
        }

    }//end class
}//end namespace