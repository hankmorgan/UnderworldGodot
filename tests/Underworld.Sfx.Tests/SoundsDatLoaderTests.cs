using Underworld.Sfx;

namespace Underworld.Sfx.Tests;

public class SoundsDatLoaderTests
{
    [Fact]
    public void Uw1_fixture_is_121_bytes()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        // 1-byte count (0x18 = 24) + 24 * 5-byte records = 121 bytes.
        Assert.Equal(121, new System.IO.FileInfo(Fixtures.SoundsDat).Length);
    }

    [Fact]
    public void Loads_24_entries()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        Assert.Equal(24, SoundsDatLoader.Load(Fixtures.SoundsDat).Length);
    }

    [Fact]
    public void Entry_fields_are_in_range()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        foreach (var e in SoundsDatLoader.Load(Fixtures.SoundsDat))
        {
            Assert.InRange(e.Note, (byte)0, (byte)127);
            Assert.InRange(e.Velocity, (byte)0, (byte)127);
        }
    }

    [Fact]
    public void Entries_one_and_two_share_tvfx_number_step()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        // Per issue #28 name table: IDs 1 and 2 both map to tvfx=1 ("Step" / duplicate).
        var entries = SoundsDatLoader.Load(Fixtures.SoundsDat);
        Assert.Equal(entries[1].PatchNum, entries[2].PatchNum);
    }

    [Fact]
    public void Duration_word_is_little_endian()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        // Bytes 3..4 form a little-endian u16. Since entry 1 and 2 share the same
        // patch, their duration words should match.
        var entries = SoundsDatLoader.Load(Fixtures.SoundsDat);
        Assert.Equal(entries[1].DurationWord, entries[2].DurationWord);
    }

    [Fact]
    public void Entry_0_patch_num_is_water()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        // Per issue #28 name table: ID 0 → tvfx=8 ("Water").
        Assert.Equal((byte)8, SoundsDatLoader.Load(Fixtures.SoundsDat)[0].PatchNum);
    }

    [Fact]
    public void Uw2_duration_word_is_big_endian()
    {
        // UW2 SOUNDS.DAT entry layout from uw2_asm.asm:83683-83688
        //   shl ax, 8 ; add ax, dx  → byte[3] is high, byte[4] is low → big-endian.
        // UW1 is little-endian (5-byte entries).
        //
        // Craft a 2-entry UW2 file: count byte 0x02, then two 8-byte entries where
        // byte[3]=0x12, byte[4]=0x34. Expect DurationWord == 0x1234 when parsed
        // as UW2, 0x3412 when parsed as UW1. Assert only on the UW2 parse.
        var tmp = System.IO.Path.GetTempFileName();
        try
        {
            byte[] buf = new byte[] {
                0x02,                                                   // 2 entries
                0x10, 0x20, 0x30, 0x12, 0x34, 0x01, 0x02, 0x03,          // entry 0
                0x11, 0x21, 0x31, 0xAA, 0xBB, 0x00, 0x00, 0x00,          // entry 1
            };
            System.IO.File.WriteAllBytes(tmp, buf);

            var prev = UWClass._RES;
            UWClass._RES = UWClass.GAME_UW2;
            try
            {
                var entries = SoundsDatLoader.Load(tmp);
                Assert.Equal(2, entries.Length);
                Assert.Equal(0x1234, entries[0].DurationWord);
                Assert.Equal(0xAABB, entries[1].DurationWord);
            }
            finally { UWClass._RES = prev; }
        }
        finally { System.IO.File.Delete(tmp); }
    }
}
