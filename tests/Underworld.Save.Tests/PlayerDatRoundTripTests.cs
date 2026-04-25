using System;
using System.IO;
using Xunit;

namespace Underworld.Save.Tests;

// Share a collection with other UWClass-state tests to serialise them and avoid static-state races.
[Collection("UWClassState")]
public class PlayerDatRoundTripTests : IDisposable
{
    // Save/restore UWClass static state so tests don't leak to other collections.
    private readonly string _origBasePath;
    private readonly byte _origRes;

    public PlayerDatRoundTripTests()
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
    public void Uw1InitEmptyPlayer_SerializeDecrypt_ByteIdenticalInPopulatedRegion()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;

        Underworld.playerdat.InitEmptyPlayer("TestGronk");

        byte[] originalPdat = (byte[])Underworld.playerdat.pdat.Clone();
        byte[] encrypted = Underworld.PlayerDatWriter.Serialize();
        byte[] decrypted = Underworld.playerdat.EncryptDecryptUW1(encrypted, encrypted[0]);

        // File length mirrors the load loop in playerdatutil.cs:Load: slot i lives at
        // PTR = InventoryPtr + (i-1)*8, so N populated slots occupy N*8 bytes past InventoryPtr.
        int expectedLen = Underworld.playerdat.InventoryPtr
            + Underworld.PlayerDatWriter.LastPopulatedInventorySlot() * 8;
        Assert.Equal(expectedLen, decrypted.Length);
        for (int i = 0; i < expectedLen; i++)
        {
            Assert.True(originalPdat[i] == decrypted[i],
                $"Byte mismatch at 0x{i:X4}: expected 0x{originalPdat[i]:X2}, got 0x{decrypted[i]:X2}");
        }
    }

    [Fact]
    public void Uw2InitEmptyPlayer_SerializeDecrypt_ByteIdenticalInPopulatedRegion()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW2");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW2;

        Underworld.playerdat.InitEmptyPlayer("TestGronk");

        byte[] originalPdat = (byte[])Underworld.playerdat.pdat.Clone();
        byte[] encrypted = Underworld.PlayerDatWriter.Serialize();
        byte[] decrypted = Underworld.playerdat.EncryptDecryptUW2(encrypted, encrypted[0]);

        // File length mirrors the load loop in playerdatutil.cs:Load: slot i lives at
        // PTR = InventoryPtr + (i-1)*8, so N populated slots occupy N*8 bytes past InventoryPtr.
        int expectedLen = Underworld.playerdat.InventoryPtr
            + Underworld.PlayerDatWriter.LastPopulatedInventorySlot() * 8;
        Assert.Equal(expectedLen, decrypted.Length);
        for (int i = 0; i < expectedLen; i++)
        {
            Assert.True(originalPdat[i] == decrypted[i],
                $"Byte mismatch at 0x{i:X4}: expected 0x{originalPdat[i]:X2}, got 0x{decrypted[i]:X2}");
        }
    }

    // -------------------------------------------------------------------------
    // DOS round-trip marker guards
    //
    // Regression guards for the UW1 DOS round-trip byte-level fixes
    // (see docs/save-architecture.md "UW1 DOS round-trip"). If a future
    // refactor zeroes these markers, DOS UW.EXE will refuse to load
    // port-written saves but the in-port loader won't notice.
    // -------------------------------------------------------------------------

    [Fact]
    public void Uw1InitEmptyPlayer_DetailLevelDefaultsToVeryHigh()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;
        Underworld.playerdat.InitEmptyPlayer("TestGronk");

        // pdat[0xB6] bits 4-5 = UW1 graphics detail level. DOS chargen sets
        // it to Very High (3); 0 (Low) renders DOS-loaded saves flat-shaded.
        Assert.Equal(3, Underworld.playerdat.DetailLevel);
        Assert.Equal(0x30, Underworld.playerdat.pdat[0xB6] & 0x30);
    }

    [Fact]
    public void Uw1InitEmptyPlayer_PdatD3IsDosChargenValue()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;
        Underworld.playerdat.InitEmptyPlayer("TestGronk");

        // DOS chargen writes 0x08 at pdat[0xD3]; leaving 0 hangs DOS load.
        // Semantics TBD — see docs/save-architecture.md §5.
        Assert.Equal(0x08, Underworld.playerdat.pdat[0xD3]);
    }

    [Fact]
    public void Uw2InitEmptyPlayer_DoesNotTouchUw1Markers()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW2");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW2;
        Underworld.playerdat.InitEmptyPlayer("TestGronk");

        // UW2 chargen must not touch UW1-specific marker bytes; the gates
        // in InitEmptyPlayer leave pdat[0xD3] zero on UW2.
        Assert.Equal(0x00, Underworld.playerdat.pdat[0xD3]);
    }
}
