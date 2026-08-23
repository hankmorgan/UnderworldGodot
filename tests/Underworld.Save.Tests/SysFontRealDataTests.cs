using System;
using System.IO;

namespace Underworld.Save.Tests;

/// <summary>
/// Tests against the shipped .SYS files. Skipped when the game data is absent, so
/// CI stays green without proprietary content, but run locally these are the only
/// thing that proves the header interpretation, row orientation and bit order.
/// </summary>
public class SysFontRealDataTests
{
    private static bool Have(string path) => File.Exists(path);

    /// <summary>
    /// The provider's expected-header table against the actual shipped files.
    ///
    /// That table exists in three places: the provider, and the fixtures in two test files
    /// which are generated FROM the test copies. So a typo in the provider's copy would
    /// refuse every installation on earth and leave every synthetic test green, because
    /// nothing else compares it to a real .SYS file. This does.
    /// </summary>
    [GameDataTheory]
    [InlineData("UW1")]
    [InlineData("UW2")]
    public void TheShippedFontsPassTheProvidersOwnHeaderCheck(string game)
    {
        string dir = Path.Combine(TestData.UW2GogRoot, game, "DATA");
        if (!Directory.Exists(dir)) return;   // the other game may not be installed
        bool ok = SysFontProvider.TryLoadAll(dir, out var fonts, out string error);
        Assert.True(ok, $"{game}: the provider rejected its own shipped fonts: {error}");
        Assert.NotNull(fonts);
        Assert.Equal(SysFontProvider.FontNames.Length, fonts.Count);
    }


    /// <summary>Record counts measured from both shipped games.</summary>
    public static TheoryData<string, int> ExpectedCounts => new()
    {
        { "FONT4X5P.SYS",  97 },   // stops at 0x60: no lower case
        { "FONT5X6I.SYS", 127 },
        { "FONT5X6P.SYS", 127 },
        { "FONTBIG.SYS",  127 },
    };

    [GameDataTheory]
    [MemberData(nameof(ExpectedCounts))]
    public void Uw1_GlyphCountMatchesTheShippedFile(string name, int expected)
    {
        string p = TestData.Uw1Data(name);
        Assert.True(Have(p), $"expected game data at {p}; the attribute should have skipped this");
        Assert.Equal(expected, Underworld.SysFont.Parse(File.ReadAllBytes(p)).GlyphCount);
    }

    [GameDataTheory]
    [MemberData(nameof(ExpectedCounts))]
    public void Uw2_GlyphCountMatchesTheShippedFile(string name, int expected)
    {
        string p = TestData.Uw2Data(name);
        Assert.True(Have(p), $"expected game data at {p}; the attribute should have skipped this");
        Assert.Equal(expected, Underworld.SysFont.Parse(File.ReadAllBytes(p)).GlyphCount);
    }

    [GameDataFact]
    public void BlankWidthEqualsTheSpaceGlyphsDeclaredWidth()
    {
        foreach (string name in new[] { "FONT4X5P.SYS", "FONT5X6I.SYS", "FONT5X6P.SYS", "FONTBIG.SYS" })
        {
            string p = TestData.Uw1Data(name);
            if (!Have(p)) continue;
            var f = Underworld.SysFont.Parse(File.ReadAllBytes(p));
            Assert.Equal(f.BlankWidth, f.AdvanceOf(0x20));
        }
    }

    [GameDataFact]
    public void SpaceHasNoInk()
    {
        string p = TestData.Uw1Data("FONT5X6P.SYS");
        Assert.True(Have(p), $"expected game data at {p}; the attribute should have skipped this");
        var f = Underworld.SysFont.Parse(File.ReadAllBytes(p));
        for (int r = 0; r < f.Height; r++)
            for (int c = 0; c < f.CellWidth; c++)
                Assert.False(f.PixelAt(0x20, r, c), $"space has ink at row {r}, column {c}");
    }

    [GameDataFact]
    public void SomeGlyphsAreWiderThanTheHeadersMaxWidth()
    {
        // The premise of issue #72. FONTBIG's M and X are 13 ink columns against a
        // declared maxwidth of 11. A parser that clips at maxwidth loses them.
        string p = TestData.Uw1Data("FONTBIG.SYS");
        Assert.True(Have(p), $"expected game data at {p}; the attribute should have skipped this");
        var f = Underworld.SysFont.Parse(File.ReadAllBytes(p));
        Assert.Equal(11, f.MaxWidth);
        Assert.Equal(14, f.AdvanceOf('M'));
        Assert.Equal(14, f.AdvanceOf('X'));
        Assert.True(f.PixelAt('M', 0, 11) || f.PixelAt('M', 1, 11) || f.PixelAt('M', 2, 11)
                 || f.PixelAt('M', 3, 11) || f.PixelAt('M', 4, 11),
            "M must have ink beyond column 11, past the header's maxwidth");
    }

    [GameDataFact]
    public void Font4X5P_HasNoLowerCase()
    {
        // Why issue #75 exists. Coverage stops at 0x60.
        string p = TestData.Uw1Data("FONT4X5P.SYS");
        Assert.True(Have(p), $"expected game data at {p}; the attribute should have skipped this");
        var f = Underworld.SysFont.Parse(File.ReadAllBytes(p));
        Assert.True(f.Covers('`'));
        Assert.False(f.Covers('a'));
        Assert.False(f.Covers('z'));
    }

    [GameDataFact]
    public void PinnedGlyphCoordinates_ProveBitOrderAndRowStride()
    {
        // Read off the shipped files independently and written down here, because
        // every other test in this class decodes through the same parser: a
        // consistently reversed bit order or a wrong stride would compare equal to
        // itself and pass. These coordinates do not.

        // FONT5X6P 'F' is asymmetric, so MSB-first and LSB-first disagree.
        //   row 0: ####....   row 1: #.......   row 2: ###.....
        string p1 = TestData.Uw1Data("FONT5X6P.SYS");
        if (Have(p1))
        {
            var f = Underworld.SysFont.Parse(File.ReadAllBytes(p1));
            Assert.Equal(5, f.AdvanceOf('F'));
            foreach (int c in new[] { 0, 1, 2, 3 }) Assert.True(f.PixelAt('F', 0, c), $"F row 0 col {c}");
            foreach (int c in new[] { 4, 5, 6, 7 }) Assert.False(f.PixelAt('F', 0, c), $"F row 0 col {c}");
            Assert.True(f.PixelAt('F', 1, 0));
            foreach (int c in new[] { 1, 2, 3 }) Assert.False(f.PixelAt('F', 1, c), $"F row 1 col {c}");
            foreach (int c in new[] { 0, 1, 2 }) Assert.True(f.PixelAt('F', 2, c), $"F row 2 col {c}");
            Assert.False(f.PixelAt('F', 2, 3));
            for (int c = 0; c < f.CellWidth; c++) Assert.False(f.PixelAt('F', 5, c), $"F row 5 col {c}");
        }

        // FONTBIG 'M' carries ink past column 7, so it can only be read correctly
        // with a 2-byte row stride. A stride of 1 puts these in the wrong row.
        // Note: bytes 2 and 3 of 'M' are both 0x70, so the (1,*) coordinates below
        // happen to read the same bit whether the stride is 2 or 1 - they alone do
        // not prove the stride. The (2,0)/(2,1) pair below is what actually does:
        // under a stride of 1 they read byte 2 (0x70) instead of byte 4 (0xb0) and
        // both flip.
        string p2 = TestData.Uw1Data("FONTBIG.SYS");
        if (Have(p2))
        {
            var f = Underworld.SysFont.Parse(File.ReadAllBytes(p2));
            Assert.Equal(2, f.RowBytes);
            Assert.Equal(16, f.CellWidth);
            Assert.True(f.PixelAt('M', 1, 9));
            Assert.True(f.PixelAt('M', 1, 10));
            Assert.True(f.PixelAt('M', 1, 11));
            Assert.True(f.PixelAt('M', 2, 9));
            Assert.True(f.PixelAt('M', 3, 8));
            Assert.False(f.PixelAt('M', 1, 8));
            Assert.False(f.PixelAt('M', 1, 12));
            // Row 2 of 'M' is #.##.....##....., so column 0 is inked and column 1 is
            // not. Under a stride of 1, row 2 reads byte 2 (0x70) instead of byte 4
            // (0xb0), which inverts both of these.
            Assert.True(f.PixelAt('M', 2, 0));
            Assert.False(f.PixelAt('M', 2, 1));
            for (int c = 0; c < f.CellWidth; c++) Assert.False(f.PixelAt('M', 0, c), $"M row 0 col {c}");
        }
    }

    [GameDataFact]
    public void Uw1AndUw2DifferOnlyInTheAmpersand()
    {
        // The one divergence between the games, in FONT4X5P and FONT5X6P.
        foreach (string name in new[] { "FONT4X5P.SYS", "FONT5X6P.SYS" })
        {
            string p1 = TestData.Uw1Data(name), p2 = TestData.Uw2Data(name);
            if (!Have(p1) || !Have(p2)) continue;
            var a = Underworld.SysFont.Parse(File.ReadAllBytes(p1));
            var b = Underworld.SysFont.Parse(File.ReadAllBytes(p2));
            Assert.Equal(a.GlyphCount, b.GlyphCount);
            for (int c = 0x20; c < a.GlyphCount; c++)
            {
                if (c == '&') continue;
                Assert.True(a.AdvanceOf(c) == b.AdvanceOf(c), $"{name}: advance differs at 0x{c:X2}");
                for (int r = 0; r < a.Height; r++)
                    for (int x = 0; x < a.CellWidth; x++)
                        Assert.True(a.PixelAt(c, r, x) == b.PixelAt(c, r, x),
                            $"{name}: bitmap differs at 0x{c:X2} row {r} col {x}");
            }
            Assert.Equal(5, a.AdvanceOf('&'));
            Assert.Equal(4, b.AdvanceOf('&'));
        }
    }
}
