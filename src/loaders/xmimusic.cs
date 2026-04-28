using System;
using System.IO;
using System.Reflection.Emit;
using Godot;
using Munt.NET;

namespace Underworld
{
    /// <summary>
    /// Static facade over MusicStreamPlayer. Resolves theme numbers to XMI filenames
    /// and delegates playback to the node. All synthesis is real-time — no WAV caching.
    /// </summary>
    public class XMIMusic : UWClass
    {
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
            if (_RES == GAME_UW2)
            {
                if (playerdat.MusicEnabled)
                {
                    if (CurrentlyPlayingThemeNo == Fanfare)//fanfare
                    {
                        if (MusicStreamPlayer.Instance.IsPlaying)
                        {
                            return; //Do not interupt the fanfare until finished.
                        }
                    }
                    if ((CurrentlyPlayingThemeNo >= 2) && (CurrentlyPlayingThemeNo < 4))
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
                        if (MusicStreamPlayer.Instance.IsPlaying)
                        {
                            if (CurrentlyPlayingThemeNo < 8)
                            {
                                goto seg_2CF6;
                            }
                            else
                            {
                                if (CurrentlyPlayingThemeNo <= 0xF)
                                {
                                    goto Seg_2D0B;
                                }
                            }
                        seg_2CF6:
                            if (CurrentlyPlayingThemeNo < 2)
                            {
                                goto Seg_2D04;
                            }
                            if (CurrentlyPlayingThemeNo <= 4)
                            {
                                goto Seg_2D0B;
                            }

                        Seg_2D04:
                            if (CurrentlyPlayingThemeNo != 0x18)
                            {
                                goto Seg_2D19;
                            }

                        Seg_2D0B:
                            if (CurrentlyPlayingThemeNo < 8)
                            {
                                goto WeapDrawn_2D28;
                            }

                            //seg2d12
                            if (CurrentlyPlayingThemeNo > 0xF)
                            {
                                goto WeapDrawn_2D28;
                            }
                        Seg_2D19:
                            if (!uimanager.InGame)
                            {
                                goto WeapDrawn_2D28;
                            }

                            //PickTheme_2D20:
                            if (!MusicStreamPlayer.Instance.IsPlaying)//BUG. When a theme has stopped playing isPlaying still returns true;
                            {
                                PickLevelThemeMusic(-1);
                            }
                            


                        WeapDrawn_2D28:
                            if (
                                (playerdat.play_drawn != 0) && (CurrentlyPlayingThemeNo < 2)
                                ||
                                (playerdat.play_drawn != 0) && (CurrentlyPlayingThemeNo >= 5)
                                )
                            {
                                NewThemeToPlay = 5;
                            }
                            else
                            {
                                //SEG16_2D4B
                                if (CurrentlyPlayingThemeNo < 8)
                                {
                                    goto seg_2D59;
                                }
                                if (CurrentlyPlayingThemeNo <= 0xF)
                                {
                                    goto seg_2D6E;
                                }

                            seg_2D59:
                                if (CurrentlyPlayingThemeNo < 2)
                                {
                                    goto seg_2D67;
                                }
                                //seg 22d60:
                                if (CurrentlyPlayingThemeNo <= 4)
                                {
                                    goto seg_2D6E;
                                }
                            seg_2D67:
                                if (CurrentlyPlayingThemeNo!= 0x18)
                                {
                                    goto seg_2D7B;
                                }
                            seg_2D6E:
                            if (NewThemeToPlay == 0)
                                {
                                    NewThemeToPlay = CurrentlyPlayingThemeNo;
                                }
                            }
                        
                        seg_2D7B:
                        LoadXMI(NewThemeToPlay);
                        if ((NewThemeToPlay>=2) && (NewThemeToPlay<=4))
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
            }
            else
            {
                //tmp 
                CurrentlyPlayingThemeNo = NewThemeToPlay;
                LoadXMI(NewThemeToPlay);
            }


        }
        
        public static void ChangeThemeMusic(byte newTheme)
        {
            if (CurrentlyPlayingThemeNo == Fanfare)
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
