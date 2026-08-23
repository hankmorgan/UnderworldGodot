using System;
using System.IO;

namespace Underworld
{
    /// <summary>
    /// A parsed .SYS bitmap font.
    ///
    /// Deliberately free of Godot types: the headless test suite in
    /// tests/Underworld.Save.Tests never loads the engine, and this is the half of
    /// the font pipeline worth testing exhaustively. Turning one of these into a
    /// Godot FontFile is SysFontBuilder's job.
    ///
    /// Layout (uw-formats 3.5): six little-endian UInt16s — unknown, charsize,
    /// blankwidth, height, rowbytes, maxwidth — then per glyph `charsize` bytes of
    /// bitmap followed by one width byte. The glyph index is the codepoint, and
    /// coverage varies per font: FONT4X5P stops at 0x60, the rest at 0x7E.
    /// </summary>
    public sealed class SysFont
    {
        public int CharSize { get; private set; }
        public int BlankWidth { get; private set; }
        public int Height { get; private set; }
        public int RowBytes { get; private set; }

        /// <summary>
        /// The header's declared maximum width. NOT a clip bound: uw-formats notes
        /// that some fonts carry glyphs wider than this and use the remaining bits
        /// of the row. Clipping at this value is the root cause of issue #72.
        /// </summary>
        public int MaxWidth { get; private set; }

        public int GlyphCount { get; private set; }

        /// <summary>Pixels addressable per row, which is the row stride in bits.</summary>
        public int CellWidth => RowBytes * 8;

        private byte[] _data;

        private SysFont() { }

        private int RecordSize => CharSize + 1;
        private int RecordOffset(int codepoint) => 12 + codepoint * RecordSize;

        public bool Covers(int codepoint) => codepoint >= 0 && codepoint < GlyphCount;

        /// <summary>The glyph's declared advance, in source pixels.</summary>
        public int AdvanceOf(int codepoint)
        {
            if (!Covers(codepoint))
            {
                throw new ArgumentOutOfRangeException(nameof(codepoint),
                    $"SysFont covers 0..{GlyphCount - 1}; asked for {codepoint}.");
            }
            return _data[RecordOffset(codepoint) + CharSize];
        }

        /// <summary>Is this pixel inked? Anything outside the glyph box is blank.</summary>
        public bool PixelAt(int codepoint, int row, int col)
        {
            if (!Covers(codepoint))
            {
                throw new ArgumentOutOfRangeException(nameof(codepoint),
                    $"SysFont covers 0..{GlyphCount - 1}; asked for {codepoint}.");
            }
            if (row < 0 || row >= Height || col < 0 || col >= CellWidth) return false;
            int at = RecordOffset(codepoint) + row * RowBytes + (col >> 3);
            return (_data[at] & (0x80 >> (col & 7))) != 0;
        }

        public static SysFont Parse(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length < 12)
            {
                throw new InvalidDataException(
                    $"SysFont: {data.Length} bytes is shorter than the 12-byte header.");
            }

            var f = new SysFont
            {
                CharSize   = data[2] | (data[3] << 8),
                BlankWidth = data[4] | (data[5] << 8),
                Height     = data[6] | (data[7] << 8),
                RowBytes   = data[8] | (data[9] << 8),
                MaxWidth   = data[10] | (data[11] << 8),
                _data      = data,
            };

            if (f.CharSize <= 0 || f.Height <= 0 || f.RowBytes <= 0)
            {
                throw new InvalidDataException(
                    $"SysFont: bad header (charsize={f.CharSize}, height={f.Height}, rowbytes={f.RowBytes}).");
            }
            if (f.CharSize != f.Height * f.RowBytes)
            {
                throw new InvalidDataException(
                    $"SysFont: charsize {f.CharSize} != height {f.Height} * rowbytes {f.RowBytes}.");
            }

            int payload = data.Length - 12;
            if (payload <= 0 || payload % f.RecordSize != 0)
            {
                throw new InvalidDataException(
                    $"SysFont: {payload} bytes of payload is not a whole number of {f.RecordSize}-byte records.");
            }
            f.GlyphCount = payload / f.RecordSize;

            for (int c = 0; c < f.GlyphCount; c++)
            {
                int w = data[f.RecordOffset(c) + f.CharSize];
                if (w > f.CellWidth)
                {
                    throw new InvalidDataException(
                        $"SysFont: glyph {c} declares width {w}, wider than the {f.CellWidth}-pixel row.");
                }
            }

            return f;
        }
    }
}
