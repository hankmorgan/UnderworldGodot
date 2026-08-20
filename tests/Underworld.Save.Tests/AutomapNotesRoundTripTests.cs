using System.IO;
using Xunit;

namespace Underworld.Save.Tests;

[Collection("UWClassState")]
public class AutomapNotesRoundTripTests : System.IDisposable
{
    private readonly string _origBasePath;
    private readonly byte _origRes;

    public AutomapNotesRoundTripTests()
    {
        _origBasePath = Underworld.UWClass.BasePath;
        _origRes = Underworld.UWClass._RES;
    }

    public void Dispose()
    {
        Underworld.UWClass.BasePath = _origBasePath;
        Underworld.UWClass._RES = _origRes;
    }

    [Fact]
    public void Serialize_EmptyNotes_ReturnsEmptyByteArray()
    {
        var note = new Underworld.automapnote();
        byte[] result = note.Serialize();
        Assert.Empty(result);
    }

    [Fact]
    public void Serialize_OneNote_Writes54Bytes_NullTerminatedStringAtOffset0PosAt0x32()
    {
        var note = new Underworld.automapnote();
        note.notes.Add(new Underworld.automapnote.mapnotetext("hello", 42, 7));
        byte[] result = note.Serialize();

        Assert.Equal(54, result.Length);
        Assert.Equal((byte)'h', result[0]);
        Assert.Equal((byte)'e', result[1]);
        Assert.Equal((byte)'l', result[2]);
        Assert.Equal((byte)'l', result[3]);
        Assert.Equal((byte)'o', result[4]);
        Assert.Equal((byte)0, result[5]);
        Assert.Equal((byte)42, result[0x32]);
        Assert.Equal((byte)0, result[0x33]);
        Assert.Equal((byte)7, result[0x34]);
        Assert.Equal((byte)0, result[0x35]);
    }

    [Fact]
    public void Serialize_TwoNotes_EmitsBackToBack108Bytes_SecondNoteAtOffset54()
    {
        var note = new Underworld.automapnote();
        note.notes.Add(new Underworld.automapnote.mapnotetext("A", 1, 2));
        note.notes.Add(new Underworld.automapnote.mapnotetext("B", 3, 4));
        byte[] result = note.Serialize();

        Assert.Equal(108, result.Length);
        Assert.Equal((byte)'A', result[0]);
        Assert.Equal((byte)'B', result[54]);
        Assert.Equal((byte)1, result[0x32]);
        Assert.Equal((byte)2, result[0x34]);
        Assert.Equal((byte)3, result[54 + 0x32]);
        Assert.Equal((byte)4, result[54 + 0x34]);
    }

    [Fact]
    public void LevArkWriter_Uw1_NewNoteInMemory_SurvivesFullSerializeAndReextract()
    {
        Underworld.UWClass.BasePath = System.IO.Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;

        // Load the UW1 DATA/LEV.ARK so the writer has a source ARK to pass through.
        Assert.True(Underworld.LevArkLoader.LoadLevArkFileData(folder: "DATA"));

        // Seed an in-memory note for level 0 that wasn't in the source ARK.
        Underworld.automapnote.automapsnotes = new Underworld.automapnote[Underworld.UWTileMap.NO_OF_LEVELS];
        Underworld.automapnote.automapsnotes[0] = new Underworld.automapnote();
        Underworld.automapnote.automapsnotes[0].notes.Add(
            new Underworld.automapnote.mapnotetext("INTEGRATION TEST NOTE", 10, 20));

        // Run the full writer.
        byte[] rewritten = Underworld.LevArkWriter.Serialize();

        // Swap the source to the rewritten bytes and re-extract block 36 (level 0 notes).
        byte[] originalFile = Underworld.LevArkLoader.lev_ark_file_data;
        Underworld.LevArkLoader.lev_ark_file_data = rewritten;
        try
        {
            var reloaded = new Underworld.automapnote(0);
            Assert.Single(reloaded.notes);
            Assert.Equal("INTEGRATION TEST NOTE", reloaded.notes[0].notetext);
            Assert.Equal(10, reloaded.notes[0].posX);
            Assert.Equal(20, reloaded.notes[0].posY);
        }
        finally
        {
            Underworld.LevArkLoader.lev_ark_file_data = originalFile;
            Underworld.automapnote.automapsnotes = null;
        }
    }

    // ---- issue #63: deleting every note on a level must survive save and reload -------

    private static int Uw1NotesOffset(byte[] ark, int level) =>
        (int)Underworld.Loader.getAt(ark, ((36 + level) * 4) + 2, 32);

    private static (int off, int flag, int len, int res) Uw2BlockHeader(byte[] ark, int block)
    {
        int n = (int)Underworld.Loader.getAt(ark, 0, 32);
        return ((int)Underworld.Loader.getAt(ark, 6 + block * 4, 32),
                (int)Underworld.Loader.getAt(ark, 6 + n * 4 + block * 4, 32),
                (int)Underworld.Loader.getAt(ark, 6 + n * 8 + block * 4, 32),
                (int)Underworld.Loader.getAt(ark, 6 + n * 12 + block * 4, 32));
    }

    /// <summary>
    /// Builds a UW1 ARK carrying notes on the given levels, so deletion has something to
    /// delete. The shipped DATA/LEV.ARK has no notes blocks at all.
    /// </summary>
    private static byte[] Uw1ArkWithNotesOn(params int[] levels)
    {
        Underworld.automapnote.automapsnotes = new Underworld.automapnote[Underworld.UWTileMap.NO_OF_LEVELS];
        foreach (int lvl in levels)
        {
            Underworld.automapnote.automapsnotes[lvl] = new Underworld.automapnote();
            Underworld.automapnote.automapsnotes[lvl].notes.Add(
                new Underworld.automapnote.mapnotetext($"NOTE ON LEVEL {lvl}", 10 + lvl, 20 + lvl));
        }
        byte[] ark = Underworld.LevArkWriter.Serialize();
        Underworld.automapnote.automapsnotes = null;
        return ark;
    }

    [Fact]
    public void LevArkWriter_Uw1_DeletingEveryNote_ClearsTheBlockAndSurvivesReload()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;
        Assert.True(Underworld.LevArkLoader.LoadLevArkFileData(folder: "DATA"));

        byte[] originalFile = Underworld.LevArkLoader.lev_ark_file_data;
        try
        {
            byte[] withNote = Uw1ArkWithNotesOn(0);
            Assert.NotEqual(0, Uw1NotesOffset(withNote, 0));

            // Load that ARK, confirm the note is really there, then delete it.
            Underworld.LevArkLoader.lev_ark_file_data = withNote;
            Underworld.automapnote.automapsnotes = new Underworld.automapnote[Underworld.UWTileMap.NO_OF_LEVELS];
            Underworld.automapnote.automapsnotes[0] = new Underworld.automapnote(0);
            Assert.Single(Underworld.automapnote.automapsnotes[0].notes);
            Underworld.automapnote.automapsnotes[0].notes.Clear();

            byte[] cleared = Underworld.LevArkWriter.Serialize();

            // The block must be gone, the way the shipped archive represents "no notes".
            Assert.Equal(0, Uw1NotesOffset(cleared, 0));

            Underworld.LevArkLoader.lev_ark_file_data = cleared;
            var reloaded = new Underworld.automapnote(0);
            Assert.NotNull(reloaded.notes);
            Assert.Empty(reloaded.notes);
        }
        finally
        {
            Underworld.LevArkLoader.lev_ark_file_data = originalFile;
            Underworld.automapnote.automapsnotes = null;
        }
    }

    [Fact]
    public void LevArkWriter_Uw1_DeletingNotesOnOneLevel_LeavesAnUnloadedLevelIntact()
    {
        // The regression guard for dropping the Count > 0 gate: a level that was never
        // loaded stays null, so its source block must pass through untouched.
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;
        Assert.True(Underworld.LevArkLoader.LoadLevArkFileData(folder: "DATA"));

        byte[] originalFile = Underworld.LevArkLoader.lev_ark_file_data;
        try
        {
            byte[] withNotes = Uw1ArkWithNotesOn(0, 1);
            Underworld.LevArkLoader.lev_ark_file_data = withNotes;

            // Load level 0 only and clear it. Level 1 is left null, as if never visited.
            Underworld.automapnote.automapsnotes = new Underworld.automapnote[Underworld.UWTileMap.NO_OF_LEVELS];
            Underworld.automapnote.automapsnotes[0] = new Underworld.automapnote(0);
            Underworld.automapnote.automapsnotes[0].notes.Clear();
            Assert.Null(Underworld.automapnote.automapsnotes[1]);

            byte[] cleared = Underworld.LevArkWriter.Serialize();

            Underworld.LevArkLoader.lev_ark_file_data = cleared;
            Assert.Equal(0, Uw1NotesOffset(cleared, 0));

            var levelOne = new Underworld.automapnote(1);
            Assert.Single(levelOne.notes);
            Assert.Equal("NOTE ON LEVEL 1", levelOne.notes[0].notetext);
            Assert.Equal(11, levelOne.notes[0].posX);
            Assert.Equal(21, levelOne.notes[0].posY);
        }
        finally
        {
            Underworld.LevArkLoader.lev_ark_file_data = originalFile;
            Underworld.automapnote.automapsnotes = null;
        }
    }

    [Fact]
    public void LevArkWriter_Uw2_DeletingEveryNote_ClearsTheBlockAndItsHeaderFields()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW2");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW2;
        Assert.True(Underworld.LevArkLoader.LoadLevArkFileData(folder: "SAVE0"));

        byte[] originalFile = Underworld.LevArkLoader.lev_ark_file_data;
        try
        {
            // Level 0 is block 240 and carries notes in the shipped save.
            var source = new Underworld.automapnote(0);
            Assert.NotEmpty(source.notes);

            Underworld.automapnote.automapsnotes = new Underworld.automapnote[Underworld.UWTileMap.NO_OF_LEVELS];
            Underworld.automapnote.automapsnotes[0] = source;
            source.notes.Clear();

            byte[] cleared = Underworld.LevArkWriter.Serialize();

            var (off, flag, len, res) = Uw2BlockHeader(cleared, 240);
            Assert.Equal(0, off);
            Assert.Equal(0, flag);
            Assert.Equal(0, len);
            Assert.Equal(0, res);   // an absent block carries no allocation

            Underworld.LevArkLoader.lev_ark_file_data = cleared;
            var reloaded = new Underworld.automapnote(0);
            Assert.Empty(reloaded.notes);
        }
        finally
        {
            Underworld.LevArkLoader.lev_ark_file_data = originalFile;
            Underworld.automapnote.automapsnotes = null;
        }
    }

    [Fact]
    public void LevArkWriter_Uw2_DeletingNotesOnOneLevel_LeavesAnUnloadedLevelIntact()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW2");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW2;
        Assert.True(Underworld.LevArkLoader.LoadLevArkFileData(folder: "SAVE0"));

        byte[] originalFile = Underworld.LevArkLoader.lev_ark_file_data;
        try
        {
            // Block 313 is level 73 and also carries notes in the shipped save.
            var untouched = new Underworld.automapnote(73);
            Assert.NotEmpty(untouched.notes);
            string expectedText = untouched.notes[0].notetext;

            Underworld.automapnote.automapsnotes = new Underworld.automapnote[Underworld.UWTileMap.NO_OF_LEVELS];
            Underworld.automapnote.automapsnotes[0] = new Underworld.automapnote(0);
            Underworld.automapnote.automapsnotes[0].notes.Clear();
            Assert.Null(Underworld.automapnote.automapsnotes[73]);

            byte[] cleared = Underworld.LevArkWriter.Serialize();

            Underworld.LevArkLoader.lev_ark_file_data = cleared;
            var reloaded = new Underworld.automapnote(73);
            Assert.NotEmpty(reloaded.notes);
            Assert.Equal(expectedText, reloaded.notes[0].notetext);
        }
        finally
        {
            Underworld.LevArkLoader.lev_ark_file_data = originalFile;
            Underworld.automapnote.automapsnotes = null;
        }
    }
}
