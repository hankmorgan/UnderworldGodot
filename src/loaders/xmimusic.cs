using System.IO;

namespace Underworld
{
    /// <summary>
    /// Static facade over MusicStreamPlayer. Resolves theme numbers to XMI filenames
    /// and delegates playback to the node. All synthesis is real-time — no WAV caching.
    /// </summary>
    public class XMIMusic : UWClass
    {
        public static bool DEBUG_MUSIC_HASSTOPPED = false;//for simulating stopping of music themes, until a reliable way of detecting that the music player is not playing music is set up to toggle value click on the compass ui.
        public static byte CurrentlyPlayingThemeNo;
        public static byte NewThemeToPlay;
        static readonly byte[] UW2WorldThemes = [0xA, 0xC, 0xE, 0x9, 0xA, 0xF, 0xB, 0xD, 0xA, 0xC, 0xD, 0x9, 0x8, 0xB, 0xE, 0xD, 0x8, 0xF, 0xE, 0x9, 0x8, 0xF, 0xB, 0xA, 0x8, 0xC, 0x9];
        static int CurrentWorldTheme;

        public static double LastCombatMusicThemeChange;
        public static double CombatMusicTimer;

        public const byte IntroTheme = 1;
        public const byte MapsAndLegends = 0xD;

        public static byte Armed => _RES == GAME_UW2 ? (byte)5 : (byte)8;
        public static byte Fanfare => _RES == GAME_UW2 ? (byte)6 : (byte)9;

        /// <summary>
        /// Processes the playing and requested music tracks and decides based on game state what music to play.
        /// </summary>
        public static void RefreshMusic()
        {
            if (!playerdat.MusicEnabled)
            {
                return;
            }
            if (_RES == GAME_UW2)
            {
                if (CurrentlyPlayingThemeNo == Fanfare)//fanfare
                {
                    if (MusicStreamPlayer.Instance.IsPlaying && !DEBUG_MUSIC_HASSTOPPED)//this is a guess.
                    {
                        DEBUG_MUSIC_HASSTOPPED = false;
                        return; //Do not interupt the fanfare until finished.
                    }
                }
                if ((CurrentlyPlayingThemeNo >= 2) && (CurrentlyPlayingThemeNo <= 4))
                {
                    //combat themes.
                    if (main.GlobalPITTimer > CombatMusicTimer + 0xA00)
                    {
                        //timer has expired
                        if (playerdat.play_drawn == 1)
                        {
                            NewThemeToPlay = Armed;
                        }
                        else
                        {
                            PickLevelThemeMusic(-1);
                        }
                    }
                }

                if (
                    (NewThemeToPlay == 0)
                    ||
                    ((NewThemeToPlay != 0) && (NewThemeToPlay == CurrentlyPlayingThemeNo))
                    )
                {
                    //2cdc
                    //the source logic in the disassembly from here on is hard to parse but I think this is what it means.

                    // currently bugged and not returning correct value, 
                    // /there is ambiguity as to what the call to SEG16_2DB8 is doing.  
                    // current assumption based on usage above with fanfare is it is a test to see if a track is playing
                    // if that case the usage here must be a check to see if the music is not playing. 
                    if (!MusicStreamPlayer.Instance.IsPlaying | DEBUG_MUSIC_HASSTOPPED)
                    {
                        DEBUG_MUSIC_HASSTOPPED = false;
                        switch (CurrentlyPlayingThemeNo)
                        {
                            case >= 8 and <= 0xF://map themes.
                                {
                                    if (uimanager.InGame)
                                    {
                                        PickLevelThemeMusic(-1);
                                    }
                                    break;
                                }
                            case >= 2 and <= 4://combat themes
                            case > 0xF: //cutscene themes
                            default:
                                {
                                    //do nothing.
                                    break;
                                }
                        }

                        // WeapDrawn_2D28:
                        if (
                            (playerdat.play_drawn != 0) && (CurrentlyPlayingThemeNo < 2)
                            ||
                            (playerdat.play_drawn != 0) && (CurrentlyPlayingThemeNo >= 5)
                            )
                        {
                            NewThemeToPlay = Armed;//set to armed theme if not already in a combat theme.
                        }
                        else
                        {
                            switch (CurrentlyPlayingThemeNo)
                            {
                                case >= 2 and <= 4: //combat
                                case >= 8 and <= 0xF: //level themes
                                case 0x18:  // cutscene
                                    if (NewThemeToPlay == 0)
                                    {
                                        NewThemeToPlay = CurrentlyPlayingThemeNo;//repeat theme.
                                    }
                                    break;
                            }
                        }

                        //seg_2D7B:
                        LoadXMI(NewThemeToPlay);
                        if ((NewThemeToPlay >= 2) && (NewThemeToPlay <= 4))
                        {
                            LastCombatMusicThemeChange = main.GlobalPITTimer;
                        }
                        else
                        {
                            LastCombatMusicThemeChange = 0;
                        }
                    }
                }
                else
                {
                    //2c42
                    if (
                        (CurrentlyPlayingThemeNo >= 2) && (CurrentlyPlayingThemeNo <= 4)
                        &&
                        (NewThemeToPlay >= 2) && (NewThemeToPlay <= 4)
                        )
                    {
                        if (main.GlobalPITTimer <= LastCombatMusicThemeChange + 0x800)
                        {
                            //not enough time has elapsed to change theme.
                            NewThemeToPlay = CurrentlyPlayingThemeNo;
                        }
                        else
                        {
                            LastCombatMusicThemeChange = main.GlobalPITTimer;
                            LoadXMI(NewThemeToPlay);
                        }
                    }
                    else
                    {
                        //Seg16_2CA6
                        //not combat themes
                        //CurrentlyPlayingThemeNo = NewThemeToPlay;
                        LoadXMI(NewThemeToPlay);
                    }

                    //seg16_2CB3
                    if ((NewThemeToPlay >= 2) && (NewThemeToPlay <= 4))
                    {
                        LastCombatMusicThemeChange = main.GlobalPITTimer;
                    }
                }
            }
            else
            {
                //uw1 refresh logic
                if (CurrentlyPlayingThemeNo == Fanfare || CurrentlyPlayingThemeNo == 0xB)
                {
                    //fanfare or death theme
                    if (MusicStreamPlayer.Instance.IsPlaying && !DEBUG_MUSIC_HASSTOPPED)//this is a guess.
                    {
                        DEBUG_MUSIC_HASSTOPPED = false;
                        return; //Do not interupt the fanfare until finished.
                    }
                }

                if ((CurrentlyPlayingThemeNo >= 5) && (CurrentlyPlayingThemeNo <= 7))
                {
                    //combat themes.
                    if (main.GlobalPITTimer > CombatMusicTimer + 0xA00)
                    {
                        //timer has expired
                        if (playerdat.play_drawn == 1)
                        {
                            NewThemeToPlay = Armed;
                        }
                        else
                        {
                            PickLevelThemeMusic(-1);
                        }
                    }
                }

                if (
                    (NewThemeToPlay == 0)
                    ||
                    ((NewThemeToPlay != 0) && (NewThemeToPlay == CurrentlyPlayingThemeNo))
                    )
                {
                    if (!MusicStreamPlayer.Instance.IsPlaying | DEBUG_MUSIC_HASSTOPPED)
                    {
                        DEBUG_MUSIC_HASSTOPPED = false;
                        switch (CurrentlyPlayingThemeNo)
                        {
                            case >= 2 and <= 4://map themes.
                                {
                                    if (uimanager.InGame)
                                    {
                                        PickLevelThemeMusic(-1);
                                    }
                                    break;
                                }
                            case >= 5 and <= 7://combat themes
                            default:
                                {
                                    //do nothing.
                                    break;
                                }
                        }

                        // WeapDrawn_2D28:
                        if (
                            (playerdat.play_drawn != 0) && (CurrentlyPlayingThemeNo < 5)
                            ||
                            (playerdat.play_drawn != 0) && (CurrentlyPlayingThemeNo >= 8)
                            )
                        {
                            NewThemeToPlay = Armed;//set to armed theme if not already in a combat theme.
                        }
                        else
                        {
                            switch (CurrentlyPlayingThemeNo)
                            {
                                case >= 2 and <= 4: //level themes                                
                                case >= 5 and <= 7: //combat
                                    if (NewThemeToPlay == 0)
                                    {
                                        NewThemeToPlay = CurrentlyPlayingThemeNo;//repeat theme.
                                    }
                                    break;
                            }
                        }

                        LoadXMI(NewThemeToPlay);
                        if ((NewThemeToPlay >= 2) && (NewThemeToPlay <= 4))
                        {
                            LastCombatMusicThemeChange = main.GlobalPITTimer;
                        }
                        else
                        {
                            LastCombatMusicThemeChange = 0;
                        }
                    }
                }
                else
                {
                    //2c42
                    if (
                        (CurrentlyPlayingThemeNo >= 5) && (CurrentlyPlayingThemeNo <= 7)
                        &&
                        (NewThemeToPlay >= 5) && (NewThemeToPlay <= 7)
                        )
                    {
                        if (main.GlobalPITTimer <= LastCombatMusicThemeChange + 0x800)
                        {
                            //not enough time has elapsed to change theme.
                            NewThemeToPlay = CurrentlyPlayingThemeNo;
                        }
                        else
                        {
                            LastCombatMusicThemeChange = main.GlobalPITTimer;
                            LoadXMI(NewThemeToPlay);
                        }
                    }
                    else
                    {
                        //Seg16_2CA6
                        //not combat themes
                        //CurrentlyPlayingThemeNo = NewThemeToPlay;
                        LoadXMI(NewThemeToPlay);
                    }

                    //seg16_2CB3
                    if ((NewThemeToPlay >= 5) && (NewThemeToPlay <= 7))
                    {
                        LastCombatMusicThemeChange = main.GlobalPITTimer;
                    }
                }
            }//end uw1 logic
        }

        public static void ChangeThemeMusic(byte newTheme)
        {
            if (CurrentlyPlayingThemeNo != Fanfare)
            {
                NewThemeToPlay = newTheme;
            }
        }

        public static void LoadXMI(byte themeNo, bool Loop = false)
        {
            Loop = false;//temporary force looping off in all cases.
            if (CurrentlyPlayingThemeNo != themeNo)
            {
                // Theme numbers are octal-encoded: upper 5 bits = first digit, lower 3 bits = second digit.
                // Matches original engine behaviour (see commit 9beb7e6 upstream).
                var digit1 = (char)(0x30 + (themeNo >> 3));
                var digit2 = (char)(0x30 + (themeNo & 0x7));
                //

                CurrentlyPlayingThemeNo = themeNo;
                NewThemeToPlay = 0;

                string filename = _RES == GAME_UW2
                    ? $"UWA{digit1}{digit2}.XMI"
                    : $"UW{digit1}{digit2}.XMI";
                LoadXMIByFileName(filename, Loop);
            }
        }

        static void LoadXMIByFileName(string filename, bool Loop = false)
        {
            if (!playerdat.MusicEnabled) return;
            if (MusicStreamPlayer.Instance == null) return;

            string xmiPath = Path.Combine(BasePath, "SOUND", filename);
            if (!File.Exists(xmiPath))
            {
                System.Diagnostics.Debug.Print($"Theme {filename} not found at {xmiPath}");
                return;
            }

            MusicStreamPlayer.Instance.PlayXmi(xmiPath, Loop);
        }

        public static void PickLevelThemeMusic(int ThemeOffset)
        {
            switch (_RES)
            {
                case GAME_UW2:
                    if (CurrentWorldTheme != playerdat.CurrentWorld)
                    {
                        CurrentWorldTheme = playerdat.CurrentWorld;
                        ThemeOffset = 0;
                    }
                    else if (ThemeOffset == -2)
                    {
                        return;// make no change to NewThemeToPlay  CurrentlyPlayingThemeNo;
                    }
                    if (ThemeOffset == -1) ThemeOffset = Rng.r.Next(3);
                    NewThemeToPlay = UW2WorldThemes[(playerdat.CurrentWorld * 3) + ThemeOffset];
                    break;
                default:
                    NewThemeToPlay = (byte)(2 + Rng.r.Next(3));
                    break;
            }
        }
    }
}
