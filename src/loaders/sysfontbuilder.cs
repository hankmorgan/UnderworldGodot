using Godot;

namespace Underworld
{
    /// <summary>
    /// Turns a parsed SysFont into a Godot FontFile.
    ///
    /// Kept apart from SysFont so the parser stays Godot-free and testable in the
    /// headless suite. Everything here needs the engine, so it is covered by the
    /// render test in tests/godot instead.
    /// </summary>
    public static class SysFontBuilder
    {
        /// <summary>
        /// The design size the committed TTFs behave as: 1024 units per em at 64
        /// units per source pixel. A font size of S therefore renders S/16 source
        /// pixels. Using the .SYS glyph height here instead makes every size wrong.
        /// </summary>
        public const int DesignSize = 16;

        public static FontFile Build(SysFont font, float ascent, float descent)
        {
            var f = new FontFile();
            Populate(f, font, ascent, descent);
            return f;
        }

        /// <summary>
        /// Fills an existing FontFile. The scene holds references to four placeholder
        /// instances, and the switch-over fills those rather than replacing them, because
        /// every theme override already points at them.
        /// </summary>
        /// <param name="ascent">
        /// Rows from the top of the line box to the baseline, in design-size units. NOT
        /// derived from the font: the .SYS header has no baseline field, so the caller
        /// supplies it. See SysFontProvider.LegacyMetricsFor for why the values are what
        /// they are.
        /// </param>
        /// <param name="descent">Rows below the baseline, same units.</param>
        public static void Populate(FontFile f, SysFont font, float ascent, float descent)
        {
            int cell = font.CellWidth;
            var img = Image.CreateEmpty(cell * font.GlyphCount, font.Height, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            // Vertical metrics come from the caller, not from the glyph height. Deriving
            // them from Height was wrong and shipped a real regression: it gave FONT5X6I a
            // line box of 28 screen pixels at font size 64 where the converted TTF gave 47,
            // moving every baseline up by 19 pixels and making multi-line labels drift.
            f.SetCacheAscent(0, DesignSize, ascent);
            f.SetCacheDescent(0, DesignSize, descent);

            var key = new Vector2I(DesignSize, 0);

            for (int c = 0; c < font.GlyphCount; c++)
            {
                for (int r = 0; r < font.Height; r++)
                {
                    for (int x = 0; x < cell; x++)
                    {
                        if (font.PixelAt(c, r, x))
                        {
                            img.SetPixel(c * cell + x, r, new Color(1, 1, 1, 1));
                        }
                    }
                }

                // Advance from the declared width byte, never from the ink.
                f.SetGlyphAdvance(0, DesignSize, c, new Vector2(font.AdvanceOf(c), 0));
                // The glyph's bottom row sits `descent` rows below the baseline, so its top
                // belongs Height - descent above it. The old -(Height - 1) assumed exactly
                // one descender row: true for the three small fonts, wrong for FONTBIG,
                // which has four and rendered 3 source pixels (12 at size 64) too high on
                // save slot labels, chargen, subtitles and the automap number.
                f.SetGlyphOffset(0, key, c, new Vector2(0, -(font.Height - descent)));
                f.SetGlyphSize(0, key, c, new Vector2(cell, font.Height));
                f.SetGlyphUVRect(0, key, c, new Rect2(c * cell, 0, cell, font.Height));
                f.SetGlyphTextureIdx(0, key, c, 0);
            }

            f.SetTextureImage(0, key, 0, img);
            f.FixedSize = DesignSize;
            f.FixedSizeScaleMode = TextServer.FixedSizeScaleMode.Enabled;

            // A gap must be a visible gap, not a silent substitution from a system
            // font, or both the validation and the render test's oracle are useless.
            f.AllowSystemFallback = false;
        }
    }
}
