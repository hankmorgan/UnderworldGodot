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
        // Floor of one zero slot is enforced by SerializeUw1Canonical to avoid the
        // DOS UW.EXE Journey-Onward hang when PLAYER.DAT ends exactly at InventoryPtr.
        int slotsExpected = Math.Max(Underworld.PlayerDatWriter.LastPopulatedInventorySlot(), 1);
        int expectedLen = Underworld.playerdat.InventoryPtr + slotsExpected * 8;
        Assert.Equal(expectedLen, decrypted.Length);
        // Compare only the populated header region; padded trailing slot is
        // zeros and is not present in the in-memory pdat at the same offset.
        for (int i = 0; i < Underworld.playerdat.InventoryPtr; i++)
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
    public void Uw1InitEmptyPlayer_PdatD3IsShadesDatLightIndex()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;
        Underworld.playerdat.InitEmptyPlayer("TestGronk");

        // pdat[0xD3] = ShadeCutOff (per Hank/RE on PR #33). Valid range
        // 0..7 indexes shades.dat (96 bytes / 12 bytes per entry).
        // Chargen default is 3 = Light. See docs/save-architecture.md §5.
        Assert.Equal(0x03, Underworld.playerdat.pdat[0xD3]);
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

    // -------------------------------------------------------------------------
    // PlayerDatWriter behavioural regressions (Hank's PR #33 review)
    //
    // These tests synthesise specific in-memory inventory states and assert
    // the writer produces correct output. They cover the three concrete
    // failure modes Hank reported / the code-review identified:
    //   - is_quant link follow-as-slot (Alfred's letter at link=514)
    //   - sack-class bit-0 toggle corrupting nested item 143 (runebag)
    //   - out-of-range slot reference throwing IndexOutOfRange
    // -------------------------------------------------------------------------

    private static void SetupUw1WithPdat()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;
        Underworld.playerdat.InitEmptyPlayer("TestGronk");
    }

    // Write a single 8-byte inventory slot record at the given slot index
    // (1-based, per the load loop convention).
    private static void WriteSlot(int slot, int item_id, bool is_quant, int link, int next, int qual = 0)
    {
        int o = Underworld.playerdat.InventoryPtr + (slot - 1) * 8;
        int word0 = (item_id & 0x1FF) | (is_quant ? 0x8000 : 0);
        Underworld.playerdat.pdat[o]     = (byte)(word0 & 0xFF);
        Underworld.playerdat.pdat[o + 1] = (byte)((word0 >> 8) & 0xFF);
        // pos word at +2 — leave at 0 (we don't care for chain tests).
        Underworld.playerdat.pdat[o + 2] = 0;
        Underworld.playerdat.pdat[o + 3] = 0;
        // word2: bits 0-5 quality, bits 6-15 next
        int word2 = (qual & 0x3F) | ((next & 0x3FF) << 6);
        Underworld.playerdat.pdat[o + 4] = (byte)(word2 & 0xFF);
        Underworld.playerdat.pdat[o + 5] = (byte)((word2 >> 8) & 0xFF);
        // word3: bits 0-5 owner=0, bits 6-15 link
        int word3 = ((link & 0x3FF) << 6);
        Underworld.playerdat.pdat[o + 6] = (byte)(word3 & 0xFF);
        Underworld.playerdat.pdat[o + 7] = (byte)((word3 >> 8) & 0xFF);
    }

    private static void SetBp0(int slot)
    {
        // BP0 pointer at pdat[0x10E], 10-bit slot in bits 6-15
        int w = (slot & 0x3FF) << 6;
        Underworld.playerdat.pdat[0x10E] = (byte)(w & 0xFF);
        Underworld.playerdat.pdat[0x10F] = (byte)((w >> 8) & 0xFF);
    }

    private static (int itemId, bool isQuant, int next, int link) ReadSlotFromDecrypted(byte[] pdat, int slot)
    {
        int o = Underworld.playerdat.InventoryPtr + (slot - 1) * 8;
        int w0 = pdat[o] | (pdat[o + 1] << 8);
        int item_id = w0 & 0x1FF;
        bool isq = (w0 & 0x8000) != 0;
        int next = (pdat[o + 4] | (pdat[o + 5] << 8)) >> 6;
        int link = (pdat[o + 6] | (pdat[o + 7] << 8)) >> 6;
        return (item_id, isq, next & 0x3FF, link & 0x3FF);
    }

    [Fact]
    public void Uw1Serialize_IsQuantLinkAtTopLevel_LeavesLinkVerbatim()
    {
        // Alfred's letter case: top-level item with is_quant=1 and link=514
        // (= property 2). The DFS must NOT follow link as a slot reference;
        // the emit pass must preserve the literal link value.
        SetupUw1WithPdat();
        // BP0 → slot 1 = Alfred's letter (id 312, is_quant=1, link 514)
        SetBp0(1);
        WriteSlot(slot: 1, item_id: 312, is_quant: true, link: 514, next: 0);

        byte[] encrypted = Underworld.PlayerDatWriter.Serialize();
        byte[] decrypted = Underworld.playerdat.EncryptDecryptUW1(encrypted, encrypted[0]);

        var s1 = ReadSlotFromDecrypted(decrypted, 1);
        Assert.Equal(312, s1.itemId);
        Assert.True(s1.isQuant);
        Assert.Equal(514, s1.link);  // preserved verbatim, not walked-as-slot
    }

    [Fact]
    public void Uw1Serialize_NestedSackInsidePack_KeepsItem143()
    {
        // Hank's Carrying-Backpack scenario: BP0 = pack 130, with a runebag
        // (item 143, classindex 0xF) inside. The writer must NOT clear bit 0
        // on the runebag (it's not an open/closed pair — items 140-143 are
        // distinct items per src/objects/container.cs:26 classindex≤0xB rule).
        SetupUw1WithPdat();
        SetBp0(1);
        // slot 1 = pack (id 130, link → slot 2)
        WriteSlot(slot: 1, item_id: 130, is_quant: false, link: 2, next: 0);
        // slot 2 = runebag inside pack (id 143)
        WriteSlot(slot: 2, item_id: 143, is_quant: false, link: 0, next: 0);

        byte[] encrypted = Underworld.PlayerDatWriter.Serialize();
        byte[] decrypted = Underworld.playerdat.EncryptDecryptUW1(encrypted, encrypted[0]);

        var s1 = ReadSlotFromDecrypted(decrypted, 1);
        var s2 = ReadSlotFromDecrypted(decrypted, 2);
        Assert.Equal(130, s1.itemId);  // pack kept as-is (already even, no toggle)
        Assert.Equal(143, s2.itemId);  // runebag NOT corrupted to 142
    }

    [Fact]
    public void Uw1Serialize_TopLevelOpenSack_ClosedOnSerialise()
    {
        // Counter-test: the close-bit toggle SHOULD fire for top-level
        // sack-class items 128-139 (classindex 0..0xB). An open sack (129)
        // at BP0 must be saved as closed (128).
        SetupUw1WithPdat();
        SetBp0(1);
        WriteSlot(slot: 1, item_id: 129, is_quant: false, link: 0, next: 0);

        byte[] encrypted = Underworld.PlayerDatWriter.Serialize();
        byte[] decrypted = Underworld.playerdat.EncryptDecryptUW1(encrypted, encrypted[0]);

        var s1 = ReadSlotFromDecrypted(decrypted, 1);
        Assert.Equal(128, s1.itemId);  // closed
    }

    [Fact]
    public void Uw1Serialize_OutOfRangeLinkSlot_DoesNotThrow()
    {
        // A top-level non-quant container with link pointing at slot 600
        // (out of port pdat range) must not throw IndexOutOfRange. Either
        // the bounds check returns 0 silently or the result is well-defined
        // and exception-free.
        SetupUw1WithPdat();
        SetBp0(1);
        // slot 1 = pack with link → slot 600 (well past pdat 512-slot limit)
        WriteSlot(slot: 1, item_id: 130, is_quant: false, link: 600, next: 0);

        // Should NOT throw.
        byte[] encrypted = Underworld.PlayerDatWriter.Serialize();
        Assert.NotNull(encrypted);
        Assert.True(encrypted.Length >= Underworld.playerdat.InventoryPtr);

        byte[] decrypted = Underworld.playerdat.EncryptDecryptUW1(encrypted, encrypted[0]);
        var s1 = ReadSlotFromDecrypted(decrypted, 1);
        Assert.Equal(130, s1.itemId);
        Assert.Equal(0, s1.link);  // out-of-range slot resolves to 0 (no link)
    }

    [Fact]
    public void Uw1Serialize_EmptyInventory_PadsToAtLeastOneSlot()
    {
        // DOS UW.EXE hangs at "You reenter the Abyss..." when PLAYER.DAT
        // ends exactly at InventoryPtr (0x138) with zero inventory slots.
        // A fresh port chargen with no items in BP0..BP7 + paperdoll used
        // to produce a 312-byte file. Verified empirically against UW.EXE
        // under js-dos that one zero slot (file >= 0x140 = 320 bytes) is
        // sufficient to unstick the load path.
        SetupUw1WithPdat();
        // Leave BP0..BP7 + paperdoll all zero — no items anywhere.

        byte[] encrypted = Underworld.PlayerDatWriter.Serialize();
        Assert.NotNull(encrypted);
        Assert.True(
            encrypted.Length >= Underworld.playerdat.InventoryPtr + 8,
            $"empty-inventory PLAYER.DAT must be at least {Underworld.playerdat.InventoryPtr + 8} bytes " +
            $"to unstick DOS UW.EXE Journey-Onward; got {encrypted.Length}");
    }
}
