using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Underworld.Save.Tests;

/// <summary>
/// Integration tests for SaveGame.Save — verifies that all five files are written
/// correctly, SCD.ARK is skipped for UW1, and invalid slot throws.
/// </summary>
[Collection("UWClassState")]
public class SaveGameOrchestratorTests : IDisposable
{
    // Saved UWClass static state
    private readonly string _origBasePath;
    private readonly byte _origRes;
    private readonly string _origCurrentFolder;
    private readonly byte[] _origLevArkFileData;
    private readonly UWTileMap[] _origDungeons;
    private readonly UWBlock[] _origScdData;
    private readonly bglobal.BablGlobal[] _origBGlobals;

    // Temp directory created per-test-class, deleted in Dispose
    private readonly string _tempRoot;

    public SaveGameOrchestratorTests()
    {
        _origBasePath       = UWClass.BasePath;
        _origRes            = UWClass._RES;
        _origCurrentFolder  = playerdat.currentfolder;
        _origLevArkFileData = LevArkLoader.lev_ark_file_data;
        _origDungeons       = UWTileMap.dungeons;
        _origScdData        = scd.scd_data;
        _origBGlobals       = bglobal.bGlobals;

        _tempRoot = Path.Combine(Path.GetTempPath(), "uw-save-test-" + Guid.NewGuid());
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        UWClass.BasePath               = _origBasePath;
        UWClass._RES                   = _origRes;
        playerdat.currentfolder        = _origCurrentFolder;
        LevArkLoader.lev_ark_file_data = _origLevArkFileData;
        UWTileMap.dungeons             = _origDungeons;
        scd.scd_data                   = _origScdData;
        bglobal.bGlobals               = _origBGlobals;

        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Set up minimal UW1 state using real game data (LEV.ARK from DATA).
    /// BasePath is set to the real UW1 path; caller must redirect it to _tempRoot
    /// only AFTER calling this.
    /// </summary>
    private void SetupUw1State()
    {
        UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        UWClass._RES     = UWClass.GAME_UW1;
        playerdat.InitEmptyPlayer("TestSave");
        bglobal.bGlobals = Array.Empty<bglobal.BablGlobal>();
        // Load real UW1 lev_ark_file_data so AssembleUW1Ark doesn't crash.
        LevArkLoader.LoadLevArkFileData(folder: "DATA");
        UWTileMap.dungeons = null;
        scd.scd_data = null;
        playerdat.currentfolder = "DATA";
    }

    /// <summary>
    /// Set up minimal UW2 state using real SAVE0 data.
    /// Uses InitEmptyPlayer (Godot-free) + direct bglobal/lev loading.
    /// Copies SCD.ARK from SAVE0 into _tempRoot/SAVE0/ so ScdArkWriter can find it
    /// when BasePath is redirected.
    /// </summary>
    private void SetupUw2State()
    {
        UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW2");
        UWClass._RES     = UWClass.GAME_UW2;
        playerdat.InitEmptyPlayer("TestSaveUW2");
        bglobal.LoadGlobals("SAVE0");
        LevArkLoader.LoadLevArkFileData(folder: "SAVE0");
        UWTileMap.dungeons = null;
        scd.scd_data = null;
        playerdat.currentfolder = "SAVE0";

        // Copy SCD.ARK from the real SAVE0 into the temp SAVE0 folder so
        // ScdArkWriter (which reads from BasePath/folder/SCD.ARK) can find it
        // after we redirect BasePath to _tempRoot.
        string srcScd = Path.Combine(TestData.UW2GogRoot, "UW2", "SAVE0", "SCD.ARK");
        string dstDir  = Path.Combine(_tempRoot, "SAVE0");
        Directory.CreateDirectory(dstDir);
        File.Copy(srcScd, Path.Combine(dstDir, "SCD.ARK"), overwrite: true);
    }

    // -------------------------------------------------------------------------
    // Invalid-slot tests — no game state required
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-1)]
    public void Save_InvalidSlot_ThrowsArgumentOutOfRangeException(int slot)
    {
        UWClass.BasePath = _tempRoot;
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => SaveGame.Save(slot, "test"));
        Assert.Equal("slot", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void Save_InvalidSlot_DoesNotCreateSaveDirectory(int slot)
    {
        UWClass.BasePath = _tempRoot;
        try { SaveGame.Save(slot, "test"); } catch (ArgumentOutOfRangeException) { }
        Assert.False(Directory.Exists(Path.Combine(_tempRoot, $"SAVE{slot}")));
    }

    // -------------------------------------------------------------------------
    // UW1 tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// The two fields DOS derives from the inventory, checked on the file the
    /// orchestrator actually writes. Serialize() alone is not enough: the head is
    /// patched afterwards in SaveGame, so a test that only calls the writer passes
    /// whatever was already in memory and never exercises the patch.
    /// </summary>
    [Fact]
    public void Save_Uw1_EmptyInventory_WritesCountOneAndHeadZero()
    {
        SetupUw1State();
        UWClass.BasePath = _tempRoot;

        SaveGame.Save(1, "inventory fields");

        byte[] raw = File.ReadAllBytes(Path.Combine(_tempRoot, "SAVE1", "PLAYER.DAT"));
        byte[] plain = playerdat.EncryptDecryptUW1(raw, raw[0]);

        // No records emitted, so the file ends at InventoryPtr, exactly as DOS writes it.
        Assert.Equal(playerdat.InventoryPtr, raw.Length);

        // DOS reads this as the number of records to read: records + 1.
        Assert.Equal(1, plain[0xD3] | (plain[0xD4] << 8));

        // Head lives in bits 6..15 of the word at PlayerObjectStoragePTR+6. It must be
        // 0 with no records; writing 1 sent DOS chasing a chain into a file that ends
        // immediately, which is the hang in issue #43.
        int linkOff = playerdat.PlayerObjectStoragePTR + 6;
        int head = (plain[linkOff] | (plain[linkOff + 1] << 8)) >> 6;
        Assert.Equal(0, head);
    }

    [Fact]
    public void Save_Uw1_WritesExpectedFilesExceptScdArk()
    {
        SetupUw1State();
        UWClass.BasePath = _tempRoot;  // redirect output to temp dir

        SaveGame.Save(1, "uw1 save test");

        string saveDir = Path.Combine(_tempRoot, "SAVE1");
        Assert.True(File.Exists(Path.Combine(saveDir, "DESC")),         "DESC missing");
        Assert.True(File.Exists(Path.Combine(saveDir, "PLAYER.DAT")),  "PLAYER.DAT missing");
        Assert.True(File.Exists(Path.Combine(saveDir, "BGLOBALS.DAT")),"BGLOBALS.DAT missing");
        Assert.True(File.Exists(Path.Combine(saveDir, "LEV.ARK")),      "LEV.ARK missing");
        // SCD.ARK must NOT be written for UW1
        Assert.False(File.Exists(Path.Combine(saveDir, "SCD.ARK")),    "SCD.ARK must not exist for UW1");
    }

    [Fact]
    public void Save_Uw1_FilesHaveNonZeroSize()
    {
        SetupUw1State();
        UWClass.BasePath = _tempRoot;

        SaveGame.Save(2, "uw1 size check");

        string saveDir = Path.Combine(_tempRoot, "SAVE2");
        Assert.True(new FileInfo(Path.Combine(saveDir, "PLAYER.DAT")).Length > 0, "PLAYER.DAT empty");
        Assert.True(new FileInfo(Path.Combine(saveDir, "LEV.ARK")).Length > 0,    "LEV.ARK empty");
    }

    // -------------------------------------------------------------------------
    // DESC tests
    // -------------------------------------------------------------------------

    [Fact]
    public void Save_DescHoldsTheWholeDescription()
    {
        // These two tests previously asserted a single byte, justified by a comment saying
        // DOS used DESC as an in-use flag and never showed the string. A save written by
        // real DOS carries the typed name, and the port's own save and load lists display
        // it, so every used slot showed one character.
        SetupUw1State();
        UWClass.BasePath = _tempRoot;

        SaveGame.Save(3, "test desc");

        byte[] raw = File.ReadAllBytes(Path.Combine(_tempRoot, "SAVE3", "DESC"));
        Assert.Equal(Encoding.ASCII.GetBytes("test desc"), raw);
    }

    [Fact]
    public void Save_DescPreservesCaseAndHasNoTerminator()
    {
        SetupUw1State();
        UWClass.BasePath = _tempRoot;

        SaveGame.Save(4, "Testing123");

        byte[] raw = File.ReadAllBytes(Path.Combine(_tempRoot, "SAVE4", "DESC"));
        Assert.Equal(Encoding.ASCII.GetBytes("Testing123"), raw);
        Assert.DoesNotContain((byte)0, raw);
    }

    [Fact]
    public void Save_EmptyDescription_WritesAZeroLengthDescRatherThanNoFile()
    {
        // DOS accepts an empty description and leaves the file behind at zero length. The
        // slot is still occupied, which is what stops the menu offering it as free.
        SetupUw1State();
        UWClass.BasePath = _tempRoot;

        SaveGame.Save(2, "");

        string descPath = Path.Combine(_tempRoot, "SAVE2", "DESC");
        Assert.True(File.Exists(descPath), "DESC missing");
        Assert.Empty(File.ReadAllBytes(descPath));
    }

    [Fact]
    public void Save_DescriptionThatCannotBeStored_FailsBeforeTouchingTheSlot()
    {
        // Everything after the encode either creates the directory or mutates live game
        // state, so an impossible description has to be refused while that is all untouched.
        SetupUw1State();
        UWClass.BasePath = _tempRoot;
        string saveDir = Path.Combine(_tempRoot, "SAVE1");

        Assert.Throws<ArgumentException>(() => SaveGame.Save(1, new string('A', 31)));
        Assert.False(Directory.Exists(saveDir), "the slot directory was created anyway");

        Assert.Throws<ArgumentException>(() => SaveGame.Save(1, "caf\u00e9"));
        Assert.False(Directory.Exists(saveDir), "the slot directory was created anyway");
    }

    // -------------------------------------------------------------------------
    // UW2 tests — use real SAVE0 fixture
    // -------------------------------------------------------------------------

    /// <summary>
    /// UW2 must keep the head it had before the UW1 inventory fix. Its writer still
    /// uses the legacy straight-copy path and its DOS invariants are unverified, so the
    /// records-derived rule is deliberately UW1-only. Without this, the UW1 fix silently
    /// rewrote UW2's avatar link, because InventoryPtr differs between the formats
    /// (0x3E3 versus 0x138) and the derived record count would be computed against the
    /// wrong base.
    /// </summary>
    [Fact]
    public void Save_Uw2_PlayerObjectLinkUnchangedByUw1InventoryRule()
    {
        SetupUw2State();
        UWClass.BasePath = _tempRoot;

        SaveGame.Save(1, "uw2 head");

        byte[] raw = File.ReadAllBytes(Path.Combine(_tempRoot, "SAVE1", "PLAYER.DAT"));
        int linkOff = playerdat.PlayerObjectStoragePTR + 6;
        Assert.True(linkOff + 1 < raw.Length, "UW2 PLAYER.DAT too short to hold the link");

        // UW2 pdat is encrypted differently, so read the raw bytes: the rule under test
        // writes them directly either way.
        int head = (raw[linkOff] | (raw[linkOff + 1] << 8)) >> 6;
        Assert.Equal(1, head);
    }

    [Fact]
    public void Save_Uw2_WritesAllFiveFiles()
    {
        SetupUw2State();
        UWClass.BasePath = _tempRoot;

        SaveGame.Save(1, "uw2 save test");

        string saveDir = Path.Combine(_tempRoot, "SAVE1");
        Assert.True(File.Exists(Path.Combine(saveDir, "DESC")),         "DESC missing");
        Assert.True(File.Exists(Path.Combine(saveDir, "PLAYER.DAT")),  "PLAYER.DAT missing");
        Assert.True(File.Exists(Path.Combine(saveDir, "BGLOBALS.DAT")),"BGLOBALS.DAT missing");
        Assert.True(File.Exists(Path.Combine(saveDir, "LEV.ARK")),      "LEV.ARK missing");
        Assert.True(File.Exists(Path.Combine(saveDir, "SCD.ARK")),      "SCD.ARK missing for UW2");
    }

    [Fact]
    public void Save_Uw2_AllFilesNonZeroSize()
    {
        SetupUw2State();
        UWClass.BasePath = _tempRoot;

        SaveGame.Save(2, "uw2 size check");

        string saveDir = Path.Combine(_tempRoot, "SAVE2");
        foreach (var fname in new[] { "DESC", "PLAYER.DAT", "BGLOBALS.DAT", "LEV.ARK", "SCD.ARK" })
        {
            Assert.True(new FileInfo(Path.Combine(saveDir, fname)).Length > 0, $"{fname} is empty");
        }
    }

    [Fact]
    public void Save_Uw2_OverwritesExistingFiles()
    {
        // UW2 writes the same file set as UW1, DESC included, so it takes the same rule.
        // A shorter description must replace the longer one rather than leave its tail.
        SetupUw2State();
        UWClass.BasePath = _tempRoot;
        SaveGame.Save(3, "a longer first name");

        string descPath = Path.Combine(_tempRoot, "SAVE3", "DESC");
        Assert.Equal(Encoding.ASCII.GetBytes("a longer first name"), File.ReadAllBytes(descPath));

        SetupUw2State();
        UWClass.BasePath = _tempRoot;
        SaveGame.Save(3, "second");
        Assert.Equal(Encoding.ASCII.GetBytes("second"), File.ReadAllBytes(descPath));
    }
    // ---- issue #74: the slot is replaced as a unit ---------------------------------------

    /// <summary>Working files left beside a slot, which a finished save must not leave.</summary>
    private string[] LeftoversBeside(int slot)
    {
        return System.IO.Directory.GetFileSystemEntries(_tempRoot)
            .Select(System.IO.Path.GetFileName)
            .Where(x => x.StartsWith($"SAVE{slot}.", StringComparison.Ordinal))
            .ToArray();
    }

    [Fact]
    public void Save_LeavesNoWorkingFilesBehind()
    {
        SetupUw1State();
        UWClass.BasePath = _tempRoot;

        SaveGame.Save(1, "first");

        // No journal, no staging directory, no backup.
        Assert.Empty(LeftoversBeside(1));
        Assert.True(File.Exists(Path.Combine(_tempRoot, "SAVE1", "LEV.ARK")));
    }

    [Fact]
    public void Save_OverAnOccupiedSlot_KeepsAFileTheGameDidNotWrite()
    {
        SetupUw1State();
        UWClass.BasePath = _tempRoot;
        SaveGame.Save(1, "first");
        File.WriteAllText(Path.Combine(_tempRoot, "SAVE1", "NOTES.TXT"), "mine");

        SaveGame.Save(1, "second");

        // DOS leaves such files alone, measured by driving UW.EXE, so swapping a fresh
        // directory in without carrying them across would be a regression.
        Assert.Equal("mine", File.ReadAllText(Path.Combine(_tempRoot, "SAVE1", "NOTES.TXT")));
        Assert.Equal("second", File.ReadAllText(Path.Combine(_tempRoot, "SAVE1", "DESC")));
        Assert.Empty(LeftoversBeside(1));
    }

    [Fact]
    public void Save_TwiceToTheSameSlot_LeavesOnlyTheSecond()
    {
        SetupUw1State();
        UWClass.BasePath = _tempRoot;

        SaveGame.Save(1, "first");
        long firstLen = new FileInfo(Path.Combine(_tempRoot, "SAVE1", "LEV.ARK")).Length;
        SaveGame.Save(1, "second");

        // The second save runs recovery first, then replaces the slot again.
        Assert.Equal("second", File.ReadAllText(Path.Combine(_tempRoot, "SAVE1", "DESC")));
        Assert.Equal(firstLen, new FileInfo(Path.Combine(_tempRoot, "SAVE1", "LEV.ARK")).Length);
        Assert.Empty(LeftoversBeside(1));
    }

    [Fact]
    public void Save_DoesNotDisturbAnotherSlot()
    {
        SetupUw1State();
        UWClass.BasePath = _tempRoot;
        SaveGame.Save(2, "two");

        SaveGame.Save(1, "one");

        Assert.Equal("two", File.ReadAllText(Path.Combine(_tempRoot, "SAVE2", "DESC")));
        Assert.Empty(LeftoversBeside(2));
    }

    [Fact]
    public void Save_ThatFailsPartWay_LeavesThePreviousSaveIntact()
    {
        SetupUw1State();
        UWClass.BasePath = _tempRoot;
        SaveGame.Save(1, "good save");
        var before = Directory.GetFiles(Path.Combine(_tempRoot, "SAVE1"))
            .ToDictionary(Path.GetFileName, File.ReadAllBytes);

        // Make the third file fail. BGlobalWriter iterates the globals it is given, so a null
        // array throws after DESC and PLAYER.DAT have been written. This is issue #74 itself:
        // writing straight into the slot got that far and left the slot holding the new DESC
        // and the new PLAYER.DAT beside the old LEV.ARK, listed under a name that would not
        // load.
        bglobal.bGlobals = null;

        Assert.ThrowsAny<Exception>(() => SaveGame.Save(1, "doomed"));

        // The previous save is untouched, byte for byte across every file. This is the whole
        // point of the issue.
        foreach (var kv in before)
        {
            Assert.Equal(kv.Value, File.ReadAllBytes(Path.Combine(_tempRoot, "SAVE1", kv.Key)));
        }
        Assert.Equal(before.Count,
            Directory.GetFiles(Path.Combine(_tempRoot, "SAVE1")).Length);

        // The journal and the abandoned staging directory are still there, because they are
        // what tells the next run there is something to tidy.
        Assert.NotEmpty(LeftoversBeside(1));

        // And the next read clears them, since every reader resolves the slot through recovery.
        string dir = SlotTransaction.SlotDirectory(_tempRoot, 1);
        Assert.Equal("good save", File.ReadAllText(Path.Combine(dir, "DESC")));
        Assert.Empty(LeftoversBeside(1));
    }

}
