using System.Text;

namespace Underworld.Save.Tests;

/// <summary>
/// Reads notes out of a synthetic UW1 LEV.ARK laid out the way DOS lays one out, with a block
/// of another type sitting between two note blocks.
///
/// Reported on PR #71. The reader measured a note block by looking for the nearest of the nine note
/// blocks that started later, and ran straight through whatever sat in between. In the save
/// attached to that pull request, block 36 is followed by automap block 29, so the reader took 9056
/// bytes where the block holds 864. That is 167 record slots rather than 16, and 17 notes were
/// drawn rather than 16: a slot is only drawn if its first byte is non-zero and its string
/// terminates within 0x31 bytes, so most of the surplus is discarded and the rest is drawn as
/// text on the automap.
/// </summary>
[Collection("UWClassState")]
public class AutomapNoteBlockSizingTests : System.IDisposable
{
    private readonly byte _origRes;
    private readonly byte[] _origArk;

    public AutomapNoteBlockSizingTests()
    {
        // The collection serialises these tests against the other stateful ones, but it does
        // not undo what they write. Both of these are process-wide.
        _origRes = UWClass._RES;
        _origArk = LevArkLoader.lev_ark_file_data;
    }

    public void Dispose()
    {
        UWClass._RES = _origRes;
        LevArkLoader.lev_ark_file_data = _origArk;
    }

    private const int Blocks = 135;
    private const int HeaderSize = 2 + Blocks * 4;

    private static byte[] Note(string text, short x, short y)
    {
        var rec = new byte[54];
        Encoding.ASCII.GetBytes(text, 0, text.Length, rec, 0);
        rec[0x32] = (byte)(x & 0xFF); rec[0x33] = (byte)(x >> 8);
        rec[0x34] = (byte)(y & 0xFF); rec[0x35] = (byte)(y >> 8);
        return rec;
    }

    /// <summary>
    /// Note block 36 with <paramref name="noteCount"/> notes, then an automap block, then note
    /// block 39. Block 29 between the two is what a note-blocks-only search misses.
    /// </summary>
    private static byte[] Archive(int noteCount)
    {
        var body = new System.Collections.Generic.List<byte>();
        int off36 = HeaderSize;
        for (int i = 0; i < noteCount; i++) { body.AddRange(Note("NOTE" + i, (short)(10 + i), 20)); }

        // An automap block, physically between the two note blocks. Its contents matter. A real
        // automap is a buffer of small per-tile values, so it holds a mixture of zero and
        // non-zero bytes, and the note reader only accepts a record whose string terminates
        // within 0x31 bytes. Filling this with zeros, or with unbroken text, would make every
        // bogus record fail that check and the test would pass no matter what the reader did.
        int off29 = off36 + body.Count;
        var automap = new byte[4096];
        for (int i = 0; i < automap.Length; i++) { automap[i] = (byte)(i % 16); }
        body.AddRange(automap);

        int off39 = off36 + body.Count;
        body.AddRange(Note("OTHER LEVEL", 5, 5));

        var d = new byte[HeaderSize + body.Count];
        d[0] = Blocks & 0xFF; d[1] = Blocks >> 8;
        void Put(int block, int off)
        {
            d[2 + block * 4 + 0] = (byte)(off & 0xFF);
            d[2 + block * 4 + 1] = (byte)((off >> 8) & 0xFF);
            d[2 + block * 4 + 2] = (byte)((off >> 16) & 0xFF);
            d[2 + block * 4 + 3] = (byte)((off >> 24) & 0xFF);
        }
        Put(36, off36);
        Put(29, off29);
        Put(39, off39);
        body.CopyTo(d, HeaderSize);
        return d;
    }

    [Fact]
    public void ANoteBlockStopsAtTheNextBlockOfAnyType()
    {
        UWClass._RES = UWClass.GAME_UW1;
        LevArkLoader.lev_ark_file_data = Archive(noteCount: 16);

        var level0 = new automapnote(0);

        // 16. Reading on through the automap block to block 39 examines 91 record slots
        // and adds 81 of them, because most of the filler happens to terminate like a string.
        Assert.Equal(16, level0.notes.Count);
        Assert.Equal("NOTE0", level0.notes[0].notetext);
        Assert.Equal("NOTE15", level0.notes[15].notetext);
    }

    [Fact]
    public void TheFollowingBlocksAreNotReadAsNotes()
    {
        UWClass._RES = UWClass.GAME_UW1;
        LevArkLoader.lev_ark_file_data = Archive(noteCount: 16);

        var level0 = new automapnote(0);

        // The note that belongs to the other level must not appear on this one.
        Assert.DoesNotContain(level0.notes, n => n.notetext == "OTHER LEVEL");
    }
}
