using System;
using System.IO;
using Underworld;
using Xunit;

public class InstallationValidatorTests : IDisposable
{
    private readonly string _dir;

    public InstallationValidatorTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "uwinst-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_dir, "DATA"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    /// <summary>Minimal well-formed .SYS: 6 header Int16s then count*(charsize+1) bytes.</summary>
    private static byte[] MakeSys(int charsize, int blankwidth, int height, int rowbytes, int maxwidth, int count)
    {
        var b = new byte[12 + count * (charsize + 1)];
        void W(int at, int v) { b[at] = (byte)(v & 0xFF); b[at + 1] = (byte)(v >> 8); }
        W(0, 0); W(2, charsize); W(4, blankwidth); W(6, height); W(8, rowbytes); W(10, maxwidth);
        return b;
    }

    /// <summary>
    /// The real header of each shipped font: charsize, blankwidth, height, rowbytes,
    /// maxwidth, records. The provider checks the whole header, not just the count, so a
    /// fixture that reuses one header for all four is rejected.
    /// </summary>
    private static readonly System.Collections.Generic.Dictionary<string, int[]> Headers = new()
    {
        { "FONT4X5P", new[] {  4, 2,  4, 1,  5,  97 } },
        { "FONT5X6I", new[] {  7, 3,  7, 1,  6, 127 } },
        { "FONT5X6P", new[] {  6, 4,  6, 1,  5, 127 } },
        { "FONTBIG",  new[] { 30, 5, 15, 2, 11, 127 } },
    };

    private void WriteGoodFonts()
    {
        foreach (string name in SysFontProvider.FontNames)
        {
            int[] h = Headers[name];
            File.WriteAllBytes(Path.Combine(_dir, "DATA", name + ".SYS"),
                               MakeSys(h[0], h[1], h[2], h[3], h[4], h[5]));
        }
    }

    [Fact]
    public void TryValidate_WithAGoodRetailInstall_Succeeds()
    {
        WriteGoodFonts();
        Assert.True(InstallationValidator.TryValidate(UWClass.GAME_UW1, _dir, out string error));
        Assert.Null(error);
    }

    [Fact]
    public void TryValidate_JudgesTheDemoOnItsOwnFonts()
    {
        // The demo used to be refused on sight. It is now judged like any other install:
        // the provider identifies a font by charsize, height and rowbytes, which does
        // constrain the bitmap layout, and it checks whatever the player actually has.
        // Refusing outright removed a configuration this port supports elsewhere and
        // stranded players who never chose the demo, since the launcher classifies any UW1
        // folder holding UWDEMO.EXE as demo by itself.
        WriteGoodFonts();
        Assert.True(InstallationValidator.TryValidate(UWClass.GAME_UWDEMO, _dir, out string error));
        Assert.Null(error);
    }

    [Fact]
    public void TryValidate_RefusesADemoWithUnusableFonts_ForTheFontReason()
    {
        // And when a demo's fonts are not ones we can build, the player is told which file
        // is wrong rather than that their whole edition is unsupported.
        WriteGoodFonts();
        File.WriteAllBytes(Path.Combine(_dir, "DATA", "FONTBIG.SYS"), MakeSys(6, 4, 6, 1, 5, 127));
        Assert.False(InstallationValidator.TryValidate(UWClass.GAME_UWDEMO, _dir, out string error));
        Assert.Contains("FONTBIG", error);
    }

    [Fact]
    public void TryValidate_AcceptsUW2()
    {
        // Without this, nothing pins the demo check to the demo. Rewriting it as
        // `res != GAME_UW1` or `res == GAME_UWDEMO || res == GAME_UW2` passes every other
        // test in this file while rejecting every real UW2 installation with a message
        // about the UW1 demo.
        WriteGoodFonts();
        Assert.True(InstallationValidator.TryValidate(UWClass.GAME_UW2, _dir, out string error));
        Assert.Null(error);
    }

    [Fact]
    public void TryValidate_WithAMissingFont_FailsAndNamesIt()
    {
        WriteGoodFonts();
        File.Delete(Path.Combine(_dir, "DATA", "FONTBIG.SYS"));
        Assert.False(InstallationValidator.TryValidate(UWClass.GAME_UW1, _dir, out string error));
        Assert.Contains("FONTBIG.SYS", error);
    }

    [Fact]
    public void TryValidate_WithNoPath_FailsRatherThanThrowing()
    {
        Assert.False(InstallationValidator.TryValidate(UWClass.GAME_UW1, null, out string e1));
        Assert.Contains("configured", e1);
        Assert.False(InstallationValidator.TryValidate(UWClass.GAME_UW1, "", out string e2));
        Assert.Contains("configured", e2);
    }

    [Fact]
    public void TryValidate_WithNoPath_ReportsThePathWhateverTheGame()
    {
        // A demo selection with no path is a path problem like any other, now that the demo
        // is not refused on sight.
        Assert.False(InstallationValidator.TryValidate(UWClass.GAME_UWDEMO, null, out string error));
        Assert.Contains("configured", error);
    }

    [Fact]
    public void TryValidate_LooksInsideDATA()
    {
        // Fonts live in <basePath>/DATA, not in basePath. Callers pass the install root.
        WriteGoodFonts();
        Assert.False(InstallationValidator.TryValidate(UWClass.GAME_UW1, Path.Combine(_dir, "DATA"), out _));
    }
}
