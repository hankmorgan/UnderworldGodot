using System.Text;

namespace Underworld
{
    /// <summary>
    /// Presentation of game strings. Deliberately standalone: GameStrings has a static
    /// constructor that reads from BasePath, so touching it drags in game data even for
    /// something that only rewrites a string.
    /// </summary>
    public static class GameStringFormat
    {
        /// <summary>
        /// Game strings carry display codes as a backslash and a digit, for instance the
        /// description prompt begins "\6" and the save messages begin "\4" and end "\0".
        /// Nothing in the scroll strips them, so they appear on screen verbatim.
        /// </summary>
        public static string StripDisplayCodes(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            var sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\\' && i + 1 < text.Length && char.IsDigit(text[i + 1]))
                {
                    i++;    // skip the code as well as the backslash
                    continue;
                }
                sb.Append(text[i]);
            }
            return sb.ToString().Trim();
        }
    }
}
