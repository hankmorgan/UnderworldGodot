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
}
