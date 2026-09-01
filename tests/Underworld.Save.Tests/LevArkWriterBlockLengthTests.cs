using System.Text;

namespace Underworld.Save.Tests;

/// <summary>
/// Drives <see cref="LevArkWriter"/> over a synthetic UW1 archive whose blocks are out of index
/// order, and checks the blocks it copies come out the size they went in.
///
/// Reported on PR #71. The unit tests around LevArkLoader.UW1BlockLength cover the measurement itself,
/// but they do not prove the writer calls it: putting the old inline scan back at the call site
/// leaves every one of them green while the archive is written wrong again. This covers the
/// wiring.
/// </summary>
[Collection("UWClassState")]
public class LevArkWriterBlockLengthTests : System.IDisposable
{
    private const int Blocks = 135;
    private const int HeaderSize = 2 + Blocks * 4;

    private readonly byte _origRes;
    private readonly byte[] _origArk;
    private readonly UWTileMap[] _origDungeons;
    private readonly automap[] _origAutomaps;
    private readonly automapnote[] _origNotes;

    public LevArkWriterBlockLengthTests()
    {
        _origRes = UWClass._RES;
        _origArk = LevArkLoader.lev_ark_file_data;

        // Serialize() replaces blocks from these caches when they hold anything. Left populated
        // by an earlier test they would overwrite the very blocks being measured here, and the
        // test would pass whatever the writer did with the source lengths. Cleared for the run
        // and put back afterwards.
        _origDungeons = UWTileMap.dungeons;
        _origAutomaps = automap.automaps;
        _origNotes = automapnote.automapsnotes;
        UWTileMap.dungeons = null;
        automap.automaps = null;
        automapnote.automapsnotes = null;
    }

    public void Dispose()
    {
        UWClass._RES = _origRes;
        LevArkLoader.lev_ark_file_data = _origArk;
        UWTileMap.dungeons = _origDungeons;
        automap.automaps = _origAutomaps;
        automapnote.automapsnotes = _origNotes;
    }

    private static void Put(byte[] d, int block, int off)
    {
        d[2 + block * 4 + 0] = (byte)(off & 0xFF);
        d[2 + block * 4 + 1] = (byte)((off >> 8) & 0xFF);
        d[2 + block * 4 + 2] = (byte)((off >> 16) & 0xFF);
        d[2 + block * 4 + 3] = (byte)((off >> 24) & 0xFF);
    }

    /// <summary>
    /// Note block 36, then automap block 29, then note block 39. That is the shape from the
    /// save on #71: the block following 36 has a lower index, so a forward-by-index search
    /// runs past it and swallows the automap block.
    /// </summary>
    private static byte[] Archive()
    {
        var body = new System.Collections.Generic.List<byte>();

        int off36 = HeaderSize;
        for (int i = 0; i < 16; i++)
        {
            var rec = new byte[54];
            Encoding.ASCII.GetBytes("NOTE" + i, 0, ("NOTE" + i).Length, rec, 0);
            body.AddRange(rec);
        }

        int off29 = off36 + body.Count;
        var automap = new byte[4096];
        for (int i = 0; i < automap.Length; i++) { automap[i] = (byte)(i % 16); }
        body.AddRange(automap);

        int off39 = off36 + body.Count;
        for (int i = 0; i < 20; i++)
        {
            var rec = new byte[54];
            Encoding.ASCII.GetBytes("OTHER" + i, 0, ("OTHER" + i).Length, rec, 0);
            body.AddRange(rec);
        }

        var d = new byte[HeaderSize + body.Count];
        d[0] = Blocks & 0xFF; d[1] = Blocks >> 8;
        Put(d, 36, off36);
        Put(d, 29, off29);
        Put(d, 39, off39);
        body.CopyTo(d, HeaderSize);
        return d;
    }

    [Fact]
    public void RewritingAnArchiveKeepsEveryBlockTheSizeItWas()
    {
        UWClass._RES = UWClass.GAME_UW1;
        LevArkLoader.lev_ark_file_data = Archive();

        byte[] rewritten = LevArkWriter.Serialize();
        Assert.NotNull(rewritten);

        // Measure the result with the same rule, which is sound here because the writer lays
        // its blocks out in index order.
        int len36 = LevArkLoader.UW1BlockLength(rewritten, 36, Blocks);
        int len29 = LevArkLoader.UW1BlockLength(rewritten, 29, Blocks);
        int len39 = LevArkLoader.UW1BlockLength(rewritten, 39, Blocks);

        Assert.Equal(16 * 54, len36);
        Assert.Equal(4096, len29);
        Assert.Equal(20 * 54, len39);
    }

    [Fact]
    public void TheNotesInARewrittenArchiveStillReadBackAsThemselves()
    {
        UWClass._RES = UWClass.GAME_UW1;
        LevArkLoader.lev_ark_file_data = Archive();

        LevArkLoader.lev_ark_file_data = LevArkWriter.Serialize();
        var level0 = new automapnote(0);

        // 16 notes, and none of the automap block or the other level's notes among them.
        Assert.Equal(16, level0.notes.Count);
        Assert.Equal("NOTE0", level0.notes[0].notetext);
        Assert.Equal("NOTE15", level0.notes[15].notetext);
        Assert.DoesNotContain(level0.notes, n => n.notetext.StartsWith("OTHER"));
    }
}
