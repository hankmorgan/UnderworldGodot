using System;
using System.IO;
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
}
