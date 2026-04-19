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
}
