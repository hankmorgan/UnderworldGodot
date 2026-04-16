using Underworld.Sfx;

namespace Underworld.Sfx.Tests;

public class TvfxPatchBankTests
{
    [Fact]
    public void Loads_tvfx_patches_from_uw1_bank()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        var bank = TvfxPatchBank.Load(Fixtures.UwAd);
        Assert.NotNull(bank.GetTvfx(patch: 1));   // Step
        Assert.NotNull(bank.GetTvfx(patch: 8));   // Water
    }

    [Fact]
    public void Unknown_patch_returns_null()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        Assert.Null(TvfxPatchBank.Load(Fixtures.UwAd).GetTvfx(patch: 200));
    }

    [Fact]
    public void Tvfx_header_fields_parse()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        var p = TvfxPatchBank.Load(Fixtures.UwAd).GetTvfx(patch: 1);
        Assert.NotNull(p);
        Assert.True(p!.Size >= 54, $"size {p.Size} below fixed-header minimum 54");
        Assert.InRange((int)p.Type, 0, 3);
        // keyon_f_offset must be 0x34 (no ADSR block) or 0x3C (block present).
        Assert.Contains(p.Params[0].KeyonOffset, new ushort[] { 0x34, 0x3C });
    }

    [Fact]
    public void Optional_adsr_block_flag_matches_sentinel()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        // HasAdsrBlock is true for any KeyonOffset != 0x34 (matches ALE.INC:410-412);
        // real UW.AD patches only use 0x34 or 0x3C, so `== 0x3C` is an equivalent
        // assertion in practice.
        foreach (var p in TvfxPatchBank.Load(Fixtures.UwAd).AllTvfx())
            Assert.Equal(p.Params[0].KeyonOffset != 0x34, p.HasAdsrBlock);
    }

    [Fact]
    public void Patch_raw_bytes_match_declared_size()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        foreach (var p in TvfxPatchBank.Load(Fixtures.UwAd).AllTvfx())
            Assert.Equal(p.Size, (ushort)p.Raw.Length);
    }

    [Fact]
    public void All_eight_params_are_parsed()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        Assert.Equal(8, TvfxPatchBank.Load(Fixtures.UwAd).GetTvfx(patch: 1)!.Params.Length);
    }

    [Fact]
    public void Bank_covers_all_uw1_sfx_ids()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        var sounds = SoundsDatLoader.Load(Fixtures.SoundsDat);
        var bank = TvfxPatchBank.Load(Fixtures.UwAd);
        foreach (var e in sounds)
            Assert.NotNull(bank.GetTvfx(e.PatchNum));
    }
}
