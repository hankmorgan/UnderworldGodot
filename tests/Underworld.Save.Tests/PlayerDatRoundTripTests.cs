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
        // PTR = InventoryPtr + (i-1)*8, so N emitted slots occupy N*8 bytes past
        // InventoryPtr. No padding floor: an empty inventory is exactly InventoryPtr
        // bytes, which is what DOS itself writes.
        int slotsExpected = Underworld.PlayerDatWriter.LastPopulatedInventorySlot();
        int expectedLen = Underworld.playerdat.InventoryPtr + slotsExpected * 8;
        Assert.Equal(expectedLen, decrypted.Length);

        // The header is copied verbatim EXCEPT the inventory count at 0xD3..0xD4,
        // which is derived from the number of emitted records rather than carried
        // over from memory. Skipping it here rather than loosening the comparison,
        // so any other header drift still fails.
        const int countOffset = 0xD3;
        for (int i = 0; i < Underworld.playerdat.InventoryPtr; i++)
        {
            if (i == countOffset || i == countOffset + 1) continue;
            Assert.True(originalPdat[i] == decrypted[i],
                $"Byte mismatch at 0x{i:X4}: expected 0x{originalPdat[i]:X2}, got 0x{decrypted[i]:X2}");
        }
        Assert.Equal(slotsExpected + 1, decrypted[countOffset] | (decrypted[countOffset + 1] << 8));
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
    public void Uw1InitEmptyPlayer_WritesThreeAtD3()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW1");
        Underworld.UWClass._RES = Underworld.UWClass.GAME_UW1;
        Underworld.playerdat.InitEmptyPlayer("TestGronk");

        // Records what chargen puts at 0xD3, not what the byte means. It was named
        // ShadeCutOff and given the value 3, but DOS reads the shade index from the high
        // nibble of player data +0x63, and every DOS-created save inspected so far holds
        // the inventory record count plus one at 0xD3. PlayerDatWriter now derives that
        // value on save, so this in-memory 3 no longer reaches the file.
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

    [Fact]
    public void Uw1Serialize_NestedContainers_CountCoversEveryEmittedRecord()
    {
        // The count DOS reads must cover records emitted recursively, not just the
        // top-level chain, or DOS reads too few and follows a link into memory it
        // never read. A container inside a container inside the backpack.
        SetupUw1WithPdat();
        WriteSlot(slot: 1, item_id: 128, is_quant: false, link: 2, next: 0); // sack
        WriteSlot(slot: 2, item_id: 128, is_quant: false, link: 3, next: 0); // sack inside it
        WriteSlot(slot: 3, item_id: 143, is_quant: false, link: 0, next: 0); // item inside that
        SetBp0(1);

        byte[] encrypted = Underworld.PlayerDatWriter.Serialize();
        byte[] plain = Underworld.playerdat.EncryptDecryptUW1(encrypted, encrypted[0]);

        int records = (encrypted.Length - Underworld.playerdat.InventoryPtr) / 8;
        Assert.Equal(3, records);
        Assert.Equal(records + 1, plain[0xD3] | (plain[0xD4] << 8));

        // And the emitted chain must be walkable within the count: every next/link
        // that is non-zero has to name a slot the file actually contains.
        for (int slot = 1; slot <= records; slot++)
        {
            int o = Underworld.playerdat.InventoryPtr + (slot - 1) * 8;
            int next = ((plain[o + 4] | (plain[o + 5] << 8)) >> 6) & 0x3FF;
            int link = ((plain[o + 6] | (plain[o + 7] << 8)) >> 6) & 0x3FF;
            bool isQuant = ((plain[o] | (plain[o + 1] << 8)) & 0x8000) != 0;
            Assert.True(next <= records, $"slot {slot} next={next} beyond {records} records");
            if (!isQuant)
            {
                Assert.True(link <= records, $"slot {slot} link={link} beyond {records} records");
            }
        }
    }

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
    public void Uw1Serialize_EmptyInventory_IsExactlyInventoryPtrBytes()
    {
        // An empty inventory is InventoryPtr bytes, 312, which is byte for byte what
        // DOS writes. This used to be padded to 320 to stop DOS hanging at
        // "You reenter the Abyss...", but the hang was really the head at
        // PlayerObjectStoragePTR+6 being written as 1 with nothing to point at; the
        // spare record only gave that bad head somewhere harmless to land. Verified
        // in real UW.EXE under js-dos while investigating issue #44.
        SetupUw1WithPdat();
        // Leave BP0..BP7 + paperdoll all zero — no items anywhere.

        byte[] encrypted = Underworld.PlayerDatWriter.Serialize();
        Assert.NotNull(encrypted);
        Assert.Equal(Underworld.playerdat.InventoryPtr, encrypted.Length);

        byte[] plain = Underworld.playerdat.EncryptDecryptUW1(encrypted, encrypted[0]);

        // Count is records + 1, so 1 for an empty inventory. DOS uses it as the number
        // of records to read; too small and it walks a chain into memory it never read.
        Assert.Equal(1, plain[0xD3] | (plain[0xD4] << 8));

        // Head must be 0 with no records. 0xDB..0xDC is the word holding it in bits
        // 6..15; the low six bits are the owner field and are not asserted here.
        int head = (plain[0xDB] | (plain[0xDC] << 8)) >> 6;
        Assert.Equal(0, head);
    }
}
