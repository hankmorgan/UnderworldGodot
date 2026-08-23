using System;
using System.Collections.Generic;
using System.IO;

namespace Underworld.Save.Tests;

/// <summary>
/// The four fonts are one presentation set. Loading three of them and carrying on would
/// give a UI in mixed fonts and bury the reason, so the provider is all or nothing.
/// </summary>
public class SysFontProviderTests : IDisposable
{
    private readonly string _dir;

    public SysFontProviderTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "sysfontprovider-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    /// <summary>A minimal valid .SYS: 6 UInt16 header, then (bitmap, width) per glyph.</summary>
    private static byte[] MakeSys(int charSize, int blankWidth, int height, int rowBytes,
                                  int maxWidth, int glyphs)
    {
        var buf = new byte[12 + glyphs * (charSize + 1)];
        void W(int off, int v) { buf[off] = (byte)(v & 0xFF); buf[off + 1] = (byte)((v >> 8) & 0xFF); }
        W(0, 1); W(2, charSize); W(4, blankWidth); W(6, height); W(8, rowBytes); W(10, maxWidth);
        for (int i = 0; i < glyphs; i++) buf[12 + i * (charSize + 1) + charSize] = (byte)blankWidth;
        return buf;
    }

    /// <summary>
    /// The real header of each shipped font: charsize, blankwidth, height, rowbytes,
    /// maxwidth, records. Measured from both games, which ship byte-identical files.
    /// </summary>
    private static readonly System.Collections.Generic.Dictionary<string, int[]> Headers = new()
    {
        { "FONT4X5P", new[] {  4, 2,  4, 1,  5,  97 } },
        { "FONT5X6I", new[] {  7, 3,  7, 1,  6, 127 } },
        { "FONT5X6P", new[] {  6, 4,  6, 1,  5, 127 } },
        { "FONTBIG",  new[] { 30, 5, 15, 2, 11, 127 } },
    };

    private static byte[] Real(string name)
    {
        int[] h = Headers[name];
        return MakeSys(h[0], h[1], h[2], h[3], h[4], h[5]);
    }

    private void WriteAllFour()
    {
        foreach (string name in Underworld.SysFontProvider.FontNames)
        {
            File.WriteAllBytes(Path.Combine(_dir, name + ".SYS"), Real(name));
        }
    }

    [Fact]
    public void TryLoadAll_WithAllFourPresent_Succeeds()
    {
        WriteAllFour();
        bool ok = Underworld.SysFontProvider.TryLoadAll(_dir, out var fonts, out string error);
        Assert.True(ok, error);
        Assert.Null(error);
        Assert.Equal(4, fonts.Count);
        foreach (string name in Underworld.SysFontProvider.FontNames)
        {
            Assert.True(fonts.ContainsKey(name), name + " missing from the result");
        }
    }

    [Fact]
    public void TryLoadAll_WithOneMissing_FailsAndNamesTheFile()
    {
        WriteAllFour();
        File.Delete(Path.Combine(_dir, "FONTBIG.SYS"));
        bool ok = Underworld.SysFontProvider.TryLoadAll(_dir, out var fonts, out string error);
        Assert.False(ok);
        Assert.Null(fonts);
        Assert.Contains("FONTBIG.SYS", error);
        // Naming the file is not enough. Without the File.Exists guard the read still fails
        // and still names the file, just with a worse message, so pin the guard's own wording.
        Assert.Contains("missing", error);
    }

    [Fact]
    public void TryLoadAll_WithOneCorrupt_FailsAndNamesTheFile()
    {
        WriteAllFour();
        File.WriteAllBytes(Path.Combine(_dir, "FONT5X6I.SYS"), new byte[] { 1, 2, 3 });
        bool ok = Underworld.SysFontProvider.TryLoadAll(_dir, out var fonts, out string error);
        Assert.False(ok);
        Assert.Null(fonts);
        Assert.Contains("FONT5X6I.SYS", error);
    }

    [Fact]
    public void TryLoadAll_PublishesNothingWhenAnyFontFails()
    {
        // The point of atomicity: a caller must not be handed three good fonts.
        WriteAllFour();
        File.Delete(Path.Combine(_dir, "FONT4X5P.SYS"));
        Underworld.SysFontProvider.TryLoadAll(_dir, out var fonts, out _);
        Assert.Null(fonts);
    }

    [Fact]
    public void TryLoadAll_WithMissingDirectory_FailsAndNamesThePath()
    {
        string missing = Path.Combine(_dir, "nope");
        bool ok = Underworld.SysFontProvider.TryLoadAll(missing, out var fonts, out string error);
        Assert.False(ok);
        Assert.Null(fonts);
        Assert.Contains(missing, error);
        // Without the Directory.Exists guard the first File.Exists check fails instead and
        // reports "Font file missing", which sends someone with a wrong game path looking
        // for a font rather than for the directory. Pin the classification, not just failure.
        Assert.Contains("directory not found", error);
    }

    [Fact]
    public void TryLoadAll_WithNullOrEmptyPath_FailsRatherThanThrowing()
    {
        // config.cs leaves BasePath unset for the demo, so this is reachable.
        // Directory.Exists(null) returns false rather than throwing, so without pinning the
        // wording this passes with or without the guard, and the user gets "not found: "
        // with an empty path instead of being told nothing is configured.
        Assert.False(Underworld.SysFontProvider.TryLoadAll(null, out _, out string e1));
        Assert.Contains("configured", e1);
        Assert.False(Underworld.SysFontProvider.TryLoadAll("", out _, out string e2));
        Assert.Contains("configured", e2);
    }

    [Fact]
    public void TryLoadAll_WithAWholeRecordTruncated_FailsAndNamesTheFile()
    {
        // Losing whole records leaves the file structurally valid, so only a count check
        // catches it. Without one it would publish and render as missing glyphs.
        WriteAllFour();
        string path = Path.Combine(_dir, "FONT5X6P.SYS");
        File.WriteAllBytes(path, MakeSys(6, 4, 6, 1, 5, 120));  // FONT5X6P header, records truncated
        bool ok = Underworld.SysFontProvider.TryLoadAll(_dir, out var fonts, out string error);
        Assert.False(ok);
        Assert.Null(fonts);
        Assert.Contains("FONT5X6P.SYS", error);
        Assert.Contains("127", error);
    }

    [Theory]
    [InlineData("FONT4X5P", "FONT5X6P")]
    [InlineData("FONT5X6I", "FONT5X6P")]   // both 127 records: a count check cannot tell them apart
    [InlineData("FONT5X6P", "FONT5X6I")]   // ditto
    [InlineData("FONTBIG",  "FONT5X6I")]   // ditto
    [InlineData("FONT5X6P", "FONTBIG")]    // ditto
    public void TryLoadAll_WithOneFontCopiedOverAnother_Fails(string victim, string impostor)
    {
        // Three of the four hold 127 records, so they are freely interchangeable under a
        // count check and would publish the wrong bitmap size, advances and metrics under
        // the wrong name. Only the full header separates them.
        WriteAllFour();
        File.WriteAllBytes(Path.Combine(_dir, victim + ".SYS"), Real(impostor));
        bool ok = Underworld.SysFontProvider.TryLoadAll(_dir, out var fonts, out string error);
        Assert.False(ok);
        Assert.Null(fonts);
        Assert.Contains(victim, error);
    }

    [Fact]
    public void TryLoadAll_WithAbsurdDimensions_FailsBeforeAnythingAllocates()
    {
        // The parser accepts any positive height and rowbytes whose product is charsize, so
        // without a header check a crafted file could declare a row 65535 bytes wide and
        // drive an enormous atlas allocation in the builder.
        WriteAllFour();
        File.WriteAllBytes(Path.Combine(_dir, "FONT5X6P.SYS"), MakeSys(65535, 4, 1, 65535, 5, 127));
        bool ok = Underworld.SysFontProvider.TryLoadAll(_dir, out var fonts, out string error);
        Assert.False(ok);
        Assert.Null(fonts);
        Assert.Contains("FONT5X6P", error);
    }

    [Fact]
    public void FontNames_AreTheFourTheUiUses()
    {
        Assert.Equal(
            new[] { "FONT4X5P", "FONT5X6I", "FONT5X6P", "FONTBIG" },
            Underworld.SysFontProvider.FontNames);
    }
}
