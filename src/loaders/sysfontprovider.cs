using System;
using System.Collections.Generic;
using System.IO;

namespace Underworld
{
    /// <summary>
    /// Loads the game's four .SYS fonts as a set.
    ///
    /// All or nothing on purpose. The four are one presentation set, so handing a caller
    /// three good fonts would give a UI in mixed typefaces and bury the reason. Failing
    /// once, with the offending filename in the message, is more useful than degrading.
    ///
    /// Godot-free, like SysFont, so the headless suite can cover it. Turning these into
    /// FontFiles belongs to the caller.
    /// </summary>
    public static class SysFontProvider
    {
        /// <summary>The four fonts the UI binds to, without extension.</summary>
        public static readonly string[] FontNames =
        {
            "FONT4X5P", "FONT5X6I", "FONT5X6P", "FONTBIG",
        };

        /// <summary>
        /// Enough of each font's header to identify it, measured from both shipped games,
        /// which carry byte-identical files.
        ///
        /// A record count alone is not enough: FONT5X6I, FONT5X6P and FONTBIG all hold 127
        /// records, so a count check lets any one be copied over another and published with
        /// the wrong bitmap size, advances and layout metrics.
        ///
        /// CharSize alone separates all four (4, 7, 6, 30), and CharSize == Height *
        /// RowBytes, so these three give identity and also bound what reaches the builder:
        /// the parser otherwise accepts a row 65535 bytes wide, which would drive an
        /// enormous atlas allocation.
        ///
        /// BlankWidth and MaxWidth are deliberately NOT checked. Neither adds any
        /// discrimination the other fields lack, and both are rendering details a
        /// translated or otherwise legitimate release could reasonably differ on. Refusing
        /// to launch over them would trade a wrong-looking font for an unplayable game.
        ///
        /// GlyphCount is a minimum, not an equality, for the same reason: a font with MORE
        /// glyphs than the GOG release is better, not broken. FONT4X5P's 97 is a real
        /// limitation of the shipped file, not a requirement.
        /// </summary>
        private readonly record struct Header(int CharSize, int Height, int RowBytes, int MinGlyphs);

        private static readonly Dictionary<string, Header> Expected = new()
        {
            { "FONT4X5P", new Header( 4,  4, 1,  97) },
            { "FONT5X6I", new Header( 7,  7, 1, 127) },
            { "FONT5X6P", new Header( 6,  6, 1, 127) },
            { "FONTBIG",  new Header(30, 15, 2, 127) },
        };

        /// <summary>The first field of <paramref name="font"/> that differs, or null.</summary>
        private static string HeaderMismatch(string name, SysFont font)
        {
            var e = Expected[name];
            if (font.CharSize != e.CharSize)  return $"charsize {font.CharSize}, expected {e.CharSize}";
            if (font.Height   != e.Height)    return $"height {font.Height}, expected {e.Height}";
            if (font.RowBytes != e.RowBytes)  return $"rowbytes {font.RowBytes}, expected {e.RowBytes}";
            if (font.GlyphCount < e.MinGlyphs) return $"{font.GlyphCount} glyphs, expected at least {e.MinGlyphs}";
            return null;
        }

        /// <summary>
        /// Vertical metrics in design-size units, per font, measured from the converted TTFs
        /// this loader replaces.
        ///
        /// These are a COMPATIBILITY CONTRACT, not a property of the .SYS format. The header
        /// is six Int16s (unknown, charsize, blankwidth, height, rowbytes, maxwidth) and
        /// carries no baseline at all, so nothing here can be derived from the game data.
        /// The value 11 is whatever the original TTF conversion chose, and every node
        /// position in the 3200-line Underworld.tscn was then hand-tuned against it.
        ///
        /// Changing them re-lays-out both games. Correcting the line box to something a
        /// bitmap font would want is separate work and needs a DOS baseline the .SYS files
        /// cannot supply.
        /// </summary>
        private static readonly Dictionary<string, (float Ascent, float Descent)> Metrics = new()
        {
            { "FONT4X5P", (11f, 1f) },
            { "FONT5X6I", (11f, 1f) },
            { "FONT5X6P", (11f, 1f) },
            { "FONTBIG",  (11f, 4f) },
        };

        /// <summary>The ascent and descent to build <paramref name="fontName"/> with.</summary>
        public static (float Ascent, float Descent) LegacyMetricsFor(string fontName)
        {
            if (!Metrics.TryGetValue(fontName, out var m))
            {
                throw new ArgumentOutOfRangeException(nameof(fontName),
                    $"No metrics recorded for '{fontName}'. The four UI fonts are the only "
                  + "ones with a layout contract; anything else needs its own measurement.");
            }
            return m;
        }

        /// <summary>
        /// Parses all four fonts from a DATA directory. On failure returns false, leaves
        /// `fonts` null, and puts a reason naming the file in `error`.
        /// </summary>
        public static bool TryLoadAll(string dataDir, out Dictionary<string, SysFont> fonts, out string error)
        {
            fonts = null;
            error = null;

            if (string.IsNullOrWhiteSpace(dataDir))
            {
                error = "No game data directory is configured.";
                return false;
            }
            if (!Directory.Exists(dataDir))
            {
                error = $"Game data directory not found: {dataDir}";
                return false;
            }

            // Parse into a local first, so a late failure cannot publish a partial set.
            var loaded = new Dictionary<string, SysFont>();
            foreach (string name in FontNames)
            {
                string path = Path.Combine(dataDir, name + ".SYS");
                if (!File.Exists(path))
                {
                    error = $"Font file missing: {path}";
                    return false;
                }
                SysFont font;
                try
                {
                    font = SysFont.Parse(File.ReadAllBytes(path));
                }
                catch (Exception ex)
                {
                    error = $"Font file unreadable: {path} ({ex.Message})";
                    return false;
                }

                string mismatch = HeaderMismatch(name, font);
                if (mismatch != null)
                {
                    error = $"Font file does not match {name}: {path} has {mismatch}";
                    return false;
                }

                loaded[name] = font;
            }

            fonts = loaded;
            return true;
        }
    }
}
