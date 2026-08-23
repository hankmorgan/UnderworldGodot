using System;
using System.IO;

namespace Underworld.Save.Tests;

/// <summary>
/// Parser tests over synthetic fixtures, so they run in CI with no game data.
/// Real-file behaviour is covered separately in SysFontRealDataTests.
/// </summary>
public class SysFontTests
{
    /// <summary>Builds a .SYS image: 6 UInt16 header, then (bitmap, width) per glyph.</summary>
    private static byte[] MakeSys(int charSize, int blankWidth, int height, int rowBytes,
                                  int maxWidth, params (byte[] bitmap, byte width)[] glyphs)
    {
        var buf = new byte[12 + glyphs.Length * (charSize + 1)];
        void W(int off, int v) { buf[off] = (byte)(v & 0xFF); buf[off + 1] = (byte)((v >> 8) & 0xFF); }
        W(0, 1); W(2, charSize); W(4, blankWidth); W(6, height); W(8, rowBytes); W(10, maxWidth);
        for (int i = 0; i < glyphs.Length; i++)
        {
            int off = 12 + i * (charSize + 1);
            Array.Copy(glyphs[i].bitmap, 0, buf, off, charSize);
            buf[off + charSize] = glyphs[i].width;
        }
        return buf;
    }

    /// <summary>A FONT5X6P-shaped glyph: 6 rows of one byte.</summary>
    private static (byte[], byte) Glyph6(byte width, params byte[] rows)
    {
        var bm = new byte[6];
        Array.Copy(rows, bm, rows.Length);
        return (bm, width);
    }

    [Fact]
    public void Parse_ReadsHeaderFields()
    {
        var f = Underworld.SysFont.Parse(MakeSys(6, 4, 6, 1, 5, Glyph6(3, 0x80)));
        Assert.Equal(6, f.CharSize);
        Assert.Equal(4, f.BlankWidth);
        Assert.Equal(6, f.Height);
        Assert.Equal(1, f.RowBytes);
        Assert.Equal(5, f.MaxWidth);
        Assert.Equal(8, f.CellWidth);
    }

    [Fact]
    public void Parse_DerivesGlyphCountFromLength()
    {
        var f = Underworld.SysFont.Parse(MakeSys(6, 4, 6, 1, 5,
            Glyph6(3, 0x80), Glyph6(4, 0x40), Glyph6(5, 0x20)));
        Assert.Equal(3, f.GlyphCount);
        Assert.True(f.Covers(2));
        Assert.False(f.Covers(3));
        Assert.False(f.Covers(-1));
    }

    [Fact]
    public void AdvanceOf_ReturnsTheDeclaredWidthByte_NotTheInk()
    {
        // One ink column, but a declared advance of 5. The advance must win:
        // deriving it from the ink is the defect behind issue #72.
        var f = Underworld.SysFont.Parse(MakeSys(6, 4, 6, 1, 5, Glyph6(5, 0x80, 0x80, 0x80)));
        Assert.Equal(5, f.AdvanceOf(0));
    }

    [Fact]
    public void AdvanceOf_AllowsAWidthWiderThanMaxWidth()
    {
        // uw-formats: glyphs wider than the header's maxwidth are legal and use the
        // remaining bits of the row. Clipping them is exactly issue #72.
        var f = Underworld.SysFont.Parse(MakeSys(6, 4, 6, 1, 5, Glyph6(7, 0xFE)));
        Assert.Equal(7, f.AdvanceOf(0));
        Assert.True(f.PixelAt(0, 0, 6), "the 7th column must survive; maxWidth is not a clip bound");
    }

    [Fact]
    public void PixelAt_DecodesMostSignificantBitFirst()
    {
        var f = Underworld.SysFont.Parse(MakeSys(6, 4, 6, 1, 5, Glyph6(5, 0xA0)));  // 1010 0000
        Assert.True(f.PixelAt(0, 0, 0));
        Assert.False(f.PixelAt(0, 0, 1));
        Assert.True(f.PixelAt(0, 0, 2));
        Assert.False(f.PixelAt(0, 0, 3));
    }

    [Fact]
    public void PixelAt_UsesRowBytesAsTheRowStride()
    {
        // FONTBIG-shaped: 2 bytes per row, so column 8 lives in the second byte.
        var bm = new byte[4];
        bm[0] = 0x00; bm[1] = 0x80;   // row 0: column 8 set
        bm[2] = 0x80; bm[3] = 0x00;   // row 1: column 0 set
        var f = Underworld.SysFont.Parse(MakeSys(4, 5, 2, 2, 11, (bm, (byte)11)));
        Assert.Equal(16, f.CellWidth);
        Assert.True(f.PixelAt(0, 0, 8));
        Assert.False(f.PixelAt(0, 0, 0));
        Assert.True(f.PixelAt(0, 1, 0));
        Assert.False(f.PixelAt(0, 1, 8));
    }

    [Fact]
    public void PixelAt_OutsideTheGlyphBox_IsBlankNotAnError()
    {
        var f = Underworld.SysFont.Parse(MakeSys(6, 4, 6, 1, 5, Glyph6(5, 0xFF)));
        Assert.False(f.PixelAt(0, 6, 0));
        Assert.False(f.PixelAt(0, 0, 8));
        Assert.False(f.PixelAt(0, -1, 0));
    }

    [Fact]
    public void AdvanceOf_UncoveredCodepoint_Throws()
    {
        var f = Underworld.SysFont.Parse(MakeSys(6, 4, 6, 1, 5, Glyph6(3, 0x80)));
        Assert.Throws<ArgumentOutOfRangeException>(() => f.AdvanceOf(1));
    }

    [Fact]
    public void Parse_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Underworld.SysFont.Parse(null));
    }

    [Fact]
    public void Parse_ShorterThanTheHeader_Throws()
    {
        Assert.Throws<InvalidDataException>(() => Underworld.SysFont.Parse(new byte[11]));
    }

    [Fact]
    public void Parse_PayloadNotAWholeNumberOfRecords_Throws()
    {
        var good = MakeSys(6, 4, 6, 1, 5, Glyph6(3, 0x80), Glyph6(3, 0x80));
        Array.Resize(ref good, good.Length - 2);          // truncate mid-record
        Assert.Throws<InvalidDataException>(() => Underworld.SysFont.Parse(good));
    }

    [Fact]
    public void Parse_CharSizeInconsistentWithHeightTimesRowBytes_Throws()
    {
        // charsize must equal height * rowbytes; it does in all four shipped fonts.
        Assert.Throws<InvalidDataException>(() =>
            Underworld.SysFont.Parse(MakeSys(5, 4, 6, 1, 5, (new byte[5], (byte)3))));
    }

    [Fact]
    public void Parse_ZeroHeight_Throws()
    {
        // charsize and rowbytes are otherwise valid; only height is zero.
        Assert.Throws<InvalidDataException>(() =>
            Underworld.SysFont.Parse(MakeSys(6, 4, 0, 1, 5, Glyph6(3, 0x80))));
    }

    [Fact]
    public void Parse_ZeroRowBytes_Throws()
    {
        // charsize and height are otherwise valid; only rowbytes is zero.
        Assert.Throws<InvalidDataException>(() =>
            Underworld.SysFont.Parse(MakeSys(6, 4, 6, 0, 5, Glyph6(3, 0x80))));
    }

    [Fact]
    public void Parse_NoGlyphRecords_Throws()
    {
        // A valid 12-byte header with no glyph records at all: the payload is empty.
        Assert.Throws<InvalidDataException>(() =>
            Underworld.SysFont.Parse(MakeSys(6, 4, 6, 1, 5)));
    }

    [Fact]
    public void Parse_WidthWiderThanTheRow_Throws()
    {
        // A declared width of 9 cannot be represented in a 1-byte row.
        Assert.Throws<InvalidDataException>(() =>
            Underworld.SysFont.Parse(MakeSys(6, 4, 6, 1, 5, Glyph6(9, 0xFF))));
    }
}
