using System;
using System.IO;
using Xunit;

namespace Underworld.Save.Tests;

// Share a collection with other UWClass-state tests to serialise them and avoid static-state races.
[Collection("UWClassState")]
public class LevArkRoundTripTests : IDisposable
{
    private readonly string _origBasePath;
    private readonly byte _origRes;
    private readonly byte[] _origLevArkFileData;
    private readonly UWTileMap[] _origDungeons;

    public LevArkRoundTripTests()
    {
        _origBasePath = Underworld.UWClass.BasePath;
        _origRes = Underworld.UWClass._RES;
        _origLevArkFileData = Underworld.LevArkLoader.lev_ark_file_data;
        _origDungeons = Underworld.UWTileMap.dungeons;
    }

    public void Dispose()
    {
        Underworld.UWClass.BasePath = _origBasePath;
        Underworld.UWClass._RES = _origRes;
        Underworld.LevArkLoader.lev_ark_file_data = _origLevArkFileData;
        Underworld.UWTileMap.dungeons = _origDungeons;
    }

    [Fact]
    public void Uw2Level0_LoadExtractReserialize_TilemapBlockIdentical()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW2");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW2;

        LevArkLoader.LoadLevArkFileData(folder: "SAVE0");
        UWBlock block = LevArkLoader.LoadLevArkBlock(0);

        Assert.NotNull(block);
        Assert.NotNull(block.Data);
        Assert.True(block.DataLen > 0, "Block 0 DataLen should be > 0");

        byte[] rewritten = LevArkWriter.SerializeLevelBlock(block);

        Assert.NotNull(rewritten);
        int compareLen = Math.Min(rewritten.Length, block.Data.Length);
        for (int i = 0; i < compareLen; i++)
        {
            Assert.True(block.Data[i] == rewritten[i],
                $"Byte mismatch at 0x{i:X4}: original=0x{block.Data[i]:X2}, rewritten=0x{rewritten[i]:X2}");
        }
    }

    [Fact]
    public void Uw2FullArk_Reassemble_BlocksAtDocumentedOffsetsReadBackIdentically()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW2");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW2;

        LevArkLoader.LoadLevArkFileData(folder: "SAVE0");
        UWBlock originalBlock0 = LevArkLoader.LoadLevArkBlock(0);

        Assert.NotNull(originalBlock0);
        Assert.NotNull(originalBlock0.Data);

        // Reassemble the full ARK — no dungeons[] are loaded, so all blocks pass through from source.
        byte[] rewritten = LevArkWriter.Serialize();

        Assert.NotNull(rewritten);

        // Replace in-memory ARK data and re-extract block 0.
        LevArkLoader.lev_ark_file_data = rewritten;
        UWBlock reloadedBlock0 = LevArkLoader.LoadLevArkBlock(0);

        Assert.NotNull(reloadedBlock0);
        Assert.NotNull(reloadedBlock0.Data);

        int compareLen = Math.Min(originalBlock0.Data.Length, reloadedBlock0.Data.Length);
        Assert.True(compareLen > 0, "Re-loaded block 0 must have data");

        for (int i = 0; i < compareLen; i++)
        {
            Assert.True(originalBlock0.Data[i] == reloadedBlock0.Data[i],
                $"Byte mismatch at 0x{i:X4}: original=0x{originalBlock0.Data[i]:X2}, reloaded=0x{reloadedBlock0.Data[i]:X2}");
        }
    }

    [Fact]
    public void Uw2VisitedLevel_Serialize_UsesLiveDungeonBuffer()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW2");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW2;

        LevArkLoader.LoadLevArkFileData(folder: "SAVE0");

        // Populate dungeons[0] with a loaded UWTileMap so the writer takes the live-data path.
        UWTileMap.dungeons = new UWTileMap[UWTileMap.NO_OF_LEVELS];
        UWTileMap.dungeons[0] = new UWTileMap(0);

        Assert.NotNull(UWTileMap.dungeons[0].lev_ark_block?.Data);

        // Mutate a known safe byte (0x7BFF — last byte before the "uw" sentinel at 0x7C06-0x7C07).
        const int SafeOffset = 0x7BFF;
        byte originalByte = UWTileMap.dungeons[0].lev_ark_block.Data[SafeOffset];
        byte modifiedByte = (byte)(originalByte ^ 0xFF);
        UWTileMap.dungeons[0].lev_ark_block.Data[SafeOffset] = modifiedByte;

        // Serialize and replace in-memory ARK data.
        byte[] rewritten = LevArkWriter.Serialize();
        Assert.NotNull(rewritten);
        LevArkLoader.lev_ark_file_data = rewritten;

        // Re-extract block 0 and verify the mutation was preserved.
        UWBlock reloadedBlock0 = LevArkLoader.LoadLevArkBlock(0);
        Assert.NotNull(reloadedBlock0?.Data);
        Assert.True(reloadedBlock0.Data.Length > SafeOffset,
            "Re-loaded block 0 must be large enough to contain offset 0x7BFF");
        Assert.Equal(modifiedByte, reloadedBlock0.Data[SafeOffset]);
    }

    [Fact]
    public void Uw1FullArk_Reassemble_Block0ReadBackIdentically()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;

        LevArkLoader.LoadLevArkFileData(folder: "DATA");
        UWBlock originalBlock0 = LevArkLoader.LoadLevArkBlock(0);

        Assert.NotNull(originalBlock0);
        Assert.NotNull(originalBlock0.Data);
        Assert.True(originalBlock0.DataLen > 0, "UW1 block 0 DataLen should be > 0");

        // Reassemble the full ARK — no dungeons[] are loaded, so all blocks pass through from source.
        byte[] rewritten = LevArkWriter.Serialize();

        Assert.NotNull(rewritten);

        // Replace in-memory ARK data and re-extract block 0.
        LevArkLoader.lev_ark_file_data = rewritten;
        UWBlock reloadedBlock0 = LevArkLoader.LoadLevArkBlock(0);

        Assert.NotNull(reloadedBlock0);
        Assert.NotNull(reloadedBlock0.Data);

        int compareLen = Math.Min(originalBlock0.Data.Length, reloadedBlock0.Data.Length);
        Assert.True(compareLen > 0, "Re-loaded UW1 block 0 must have data");

        for (int i = 0; i < compareLen; i++)
        {
            Assert.True(originalBlock0.Data[i] == reloadedBlock0.Data[i],
                $"Byte mismatch at 0x{i:X4}: original=0x{originalBlock0.Data[i]:X2}, reloaded=0x{reloadedBlock0.Data[i]:X2}");
        }
    }

    // Progressive regression test for the "DOS UW.EXE rejects port-saved LEV.ARK"
    // bug (observable symptoms: missing inventory, wrong textures, wrong map
    // location in UW.EXE after restoring a save written by the port).
    //
    // Baseline: load DATA, construct UWTileMap via the same path LoadTileMap
    // does (but stop before BuildTileMapUW / GenerateObjects / PlacePlayerInTile),
    // serialize, compare level 0 block byte-for-byte with original DATA.
    //
    // The constructor just calls LoadLevArkBlock — if this test fails, the
    // problem is earlier than any gameplay code.
    [Fact]
    public void Uw1DungeonCtor_FromData_SerializeBlock0_IdenticalToDataBytes()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;

        LevArkLoader.LoadLevArkFileData(folder: "DATA");
        UWBlock originalBlock0 = LevArkLoader.LoadLevArkBlock(0);
        byte[] originalBytes = (byte[])originalBlock0.Data.Clone();

        // Populate dungeons[0] the way the game does on first level load.
        UWTileMap.dungeons = new UWTileMap[UWTileMap.NO_OF_LEVELS];
        UWTileMap.dungeons[0] = new UWTileMap(0);

        byte[] rewritten = LevArkWriter.Serialize();
        LevArkLoader.lev_ark_file_data = rewritten;
        UWBlock reloadedBlock0 = LevArkLoader.LoadLevArkBlock(0);

        int compareLen = Math.Min(originalBytes.Length, reloadedBlock0.Data.Length);
        int firstDiff = -1;
        int diffCount = 0;
        for (int i = 0; i < compareLen; i++)
        {
            if (originalBytes[i] != reloadedBlock0.Data[i])
            {
                if (firstDiff == -1) firstDiff = i;
                diffCount++;
            }
        }
        Assert.True(diffCount == 0,
            $"UW1 level 0 byte-identity lost after dungeon ctor + Serialize: {diffCount} diffs, first at 0x{firstDiff:X4}");
    }

    // Next step: after UWTileMap ctor, also run BuildTileMapUW — which calls
    // BuildObjectListUW, propagates obj.tileX/tileY from the tile chains, and
    // runs CleanUp. If this step introduces byte mutations, the culprit is
    // inside BuildTileMapUW.
    [Fact]
    public void Uw1DungeonCtor_AndBuildTileMap_SerializeBlock0_IdenticalToDataBytes()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;

        LevArkLoader.LoadLevArkFileData(folder: "DATA");
        UWBlock originalBlock0 = LevArkLoader.LoadLevArkBlock(0);
        byte[] originalBytes = (byte[])originalBlock0.Data.Clone();

        UWTileMap.dungeons = new UWTileMap[UWTileMap.NO_OF_LEVELS];
        UWTileMap.dungeons[0] = new UWTileMap(0);
        UWTileMap.current_tilemap = UWTileMap.dungeons[0];

        // Run the tilemap / object-list / cleanup phases of LoadTileMap.
        UWTileMap.dungeons[0].BuildTileMapUW(
            levelNo: 0,
            tex_ark: UWTileMap.dungeons[0].tex_ark_block,
            ovl_ark: UWTileMap.dungeons[0].ovl_ark_block);

        byte[] rewritten = LevArkWriter.Serialize();
        LevArkLoader.lev_ark_file_data = rewritten;
        UWBlock reloadedBlock0 = LevArkLoader.LoadLevArkBlock(0);

        int compareLen = Math.Min(originalBytes.Length, reloadedBlock0.Data.Length);
        int firstDiff = -1;
        int diffCount = 0;
        for (int i = 0; i < compareLen; i++)
        {
            if (originalBytes[i] != reloadedBlock0.Data[i])
            {
                if (firstDiff == -1) firstDiff = i;
                diffCount++;
            }
        }
        Assert.True(diffCount == 0,
            $"UW1 level 0 byte-identity lost after BuildTileMapUW + Serialize: {diffCount} diffs, first at 0x{firstDiff:X4}");
    }

    // Next step: mimic the player-stash copy that uimanager_mainmenu.cs:277-282
    // performs (copying 27 bytes of pdat back into playerObject's DataBuffer).
    // Uses InitEmptyPlayer so pdat is all zeros (which itself is a hypothetical
    // "empty character" scenario); the copy writes those zeros into slot 1.
    [Fact]
    public void Uw1_AddPlayerStashToSlot1_DiffsLocalisedToMobile1()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;

        LevArkLoader.LoadLevArkFileData(folder: "DATA");
        UWBlock originalBlock0 = LevArkLoader.LoadLevArkBlock(0);
        byte[] originalBytes = (byte[])originalBlock0.Data.Clone();

        UWTileMap.dungeons = new UWTileMap[UWTileMap.NO_OF_LEVELS];
        UWTileMap.dungeons[0] = new UWTileMap(0);
        UWTileMap.current_tilemap = UWTileMap.dungeons[0];
        UWTileMap.dungeons[0].BuildTileMapUW(
            levelNo: 0,
            tex_ark: UWTileMap.dungeons[0].tex_ark_block,
            ovl_ark: UWTileMap.dungeons[0].ovl_ark_block);

        playerdat.InitEmptyPlayer("TestPlayer");

        // Simulate uimanager_mainmenu.cs:277-282 — copy pdat's player-object
        // storage bytes into the live slot 1 buffer.
        for (int i = 0; i <= 0x1A; i++)
        {
            playerdat.playerObject.DataBuffer[playerdat.playerObject.PTR + i] =
                playerdat.pdat[playerdat.PlayerObjectStoragePTR + i];
        }

        byte[] rewritten = LevArkWriter.Serialize();
        LevArkLoader.lev_ark_file_data = rewritten;
        UWBlock reloadedBlock0 = LevArkLoader.LoadLevArkBlock(0);

        int compareLen = Math.Min(originalBytes.Length, reloadedBlock0.Data.Length);
        // Tally diffs per region instead of failing on first diff.
        int mobile1Diffs = 0, otherDiffs = 0;
        int firstOtherDiff = -1;
        int mobile1Start = 0x4000 + 27;
        int mobile1End = mobile1Start + 27;
        for (int i = 0; i < compareLen; i++)
        {
            if (originalBytes[i] == reloadedBlock0.Data[i]) continue;
            if (i >= mobile1Start && i < mobile1End) mobile1Diffs++;
            else
            {
                if (firstOtherDiff == -1) firstOtherDiff = i;
                otherDiffs++;
            }
        }
        // Player stash should only touch mobile#1; any diffs outside that slot
        // indicate a bug elsewhere in this flow.
        Assert.True(otherDiffs == 0,
            $"Player stash leaked byte changes outside slot 1: {otherDiffs} diffs, first at 0x{firstOtherDiff:X4}");
    }

    // Next step: add PlacePlayerInTile. Expect this to modify the target tile's
    // indexObjectList (bytes 2-3 of that tile's entry) and the player slot's
    // .next field — but nothing else. Anything outside those regions is a bug.
    [Fact]
    public void Uw1_AddPlacePlayerInTile_DiffsLocalisedToPlayerAndOneTile()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;

        LevArkLoader.LoadLevArkFileData(folder: "DATA");
        UWBlock originalBlock0 = LevArkLoader.LoadLevArkBlock(0);
        byte[] originalBytes = (byte[])originalBlock0.Data.Clone();

        UWTileMap.dungeons = new UWTileMap[UWTileMap.NO_OF_LEVELS];
        UWTileMap.dungeons[0] = new UWTileMap(0);
        UWTileMap.current_tilemap = UWTileMap.dungeons[0];
        UWTileMap.dungeons[0].BuildTileMapUW(
            levelNo: 0,
            tex_ark: UWTileMap.dungeons[0].tex_ark_block,
            ovl_ark: UWTileMap.dungeons[0].ovl_ark_block);

        playerdat.InitEmptyPlayer("TestPlayer");
        for (int i = 0; i <= 0x1A; i++)
        {
            playerdat.playerObject.DataBuffer[playerdat.playerObject.PTR + i] =
                playerdat.pdat[playerdat.PlayerObjectStoragePTR + i];
        }
        playerdat.playerObject.item_id = 127;
        playerdat.playerObject.link = 0;

        // Default UW1 start tile.
        const int StartX = 32;
        const int StartY = 1;
        playerdat.PlacePlayerInTile(newTileX: StartX, newTileY: StartY, previousTileX: -1, previousTileY: -1);

        byte[] rewritten = LevArkWriter.Serialize();
        LevArkLoader.lev_ark_file_data = rewritten;
        UWBlock reloadedBlock0 = LevArkLoader.LoadLevArkBlock(0);

        // Categorise diffs: slot 1 (mobile#1), target tile's entry, "other".
        int compareLen = Math.Min(originalBytes.Length, reloadedBlock0.Data.Length);
        int mobile1Diffs = 0, targetTileDiffs = 0, otherDiffs = 0;
        int firstOtherDiff = -1;
        int mobile1Start = 0x4000 + 27, mobile1End = mobile1Start + 27;
        int targetTileOff = (StartY * 64 + StartX) * 4;
        for (int i = 0; i < compareLen; i++)
        {
            if (originalBytes[i] == reloadedBlock0.Data[i]) continue;
            if (i >= mobile1Start && i < mobile1End) mobile1Diffs++;
            else if (i >= targetTileOff && i < targetTileOff + 4) targetTileDiffs++;
            else
            {
                if (firstOtherDiff == -1) firstOtherDiff = i;
                otherDiffs++;
            }
        }
        Assert.True(otherDiffs == 0,
            $"PlacePlayerInTile leaked byte changes outside slot 1 and target tile ({StartX},{StartY}): {otherDiffs} diffs, first at 0x{firstOtherDiff:X4}");
    }
}
