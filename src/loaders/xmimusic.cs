using System.IO;

namespace Underworld
{
    /// <summary>
    /// Static facade over MusicStreamPlayer. Resolves theme numbers to XMI filenames
    /// and delegates playback to the node. All synthesis is real-time — no WAV caching.
    /// </summary>
    public class XMIMusic : UWClass
    {
        public static byte CurrentThemeNo;
        public static bool LoopTheme = false;
        static readonly byte[] UW2WorldThemes = [0xA, 0xC, 0xE, 0x9, 0xA, 0xF, 0xB, 0xD, 0xA, 0xC, 0xD, 0x9, 0x8, 0xB, 0xE, 0xD, 0x8, 0xF, 0xE, 0x9, 0x8, 0xF, 0xB, 0xA, 0x8, 0xC, 0x9];
        static int CurrentWorldTheme;

        public const byte IntroTheme = 1;
        public const byte MapsAndLegends = 15;

        public static byte Armed => _RES == GAME_UW2 ? (byte)5 : (byte)10;

        public static void ChangeTheme(byte themeNo, bool Loop = false)
        {
            // Theme numbers are octal-encoded: upper 5 bits = first digit, lower 3 bits = second digit.
            // Matches original engine behaviour (see commit 9beb7e6 upstream).
            var digit1 = (char)(0x30 + (themeNo >> 3));
            var digit2 = (char)(0x30 + (themeNo & 0x7));
            CurrentThemeNo = themeNo;
            string filename = _RES == GAME_UW2
                ? $"UWA{digit1}{digit2}.XMI"
                : $"UW{digit1}{digit2}.XMI";
            ChangeTheme(filename, Loop);
        }

        static void ChangeTheme(string filename, bool Loop = false)
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
            LoopTheme = Loop;
        }

        public static byte PickLevelThemeMusic(int arg0 = -1)
        {
            switch (_RES)
            {
                case GAME_UW2:
                    if (CurrentWorldTheme != playerdat.CurrentWorld)
                    {
                        CurrentWorldTheme = playerdat.CurrentWorld;
                        arg0 = 0;
                    }
                    else if (arg0 == -2)
                    {
                        return CurrentThemeNo;
                    }
                    if (arg0 == -1) arg0 = Rng.r.Next(3);
                    return UW2WorldThemes[(playerdat.CurrentWorld * 3) + arg0];
                default:
                    return (byte)(2 + Rng.r.Next(3));
            }
        }
    }
}
