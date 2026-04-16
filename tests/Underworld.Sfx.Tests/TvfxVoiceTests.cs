using Underworld.Sfx;

namespace Underworld.Sfx.Tests;

public class TvfxVoiceTests
{
    private static TvfxPatch LoadFixturePatch(byte p)
        => TvfxPatchBank.Load(Fixtures.UwAd).GetTvfx(p)!;

    // Fresh_voice_is_idle doesn't need fixtures (it uses no patch at all).
    [Fact]
    public void Fresh_voice_is_idle()
    {
        var v = new TvfxVoice(channel: 0);
        Assert.Equal(TvfxPhase.Idle, v.Phase);
        Assert.Null(v.Patch);
    }

    [Fact]
    public void StartKeyon_sets_phase_and_loads_init_vals()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        var v = new TvfxVoice(channel: 0);
        var p = LoadFixturePatch(1);              // "Step"
        v.StartKeyon(p, lifetimeTicks: 16);

        Assert.Equal(TvfxPhase.Keyon, v.Phase);
        Assert.Same(p, v.Patch);
        for (int i = 0; i < 8; i++)
            Assert.Equal(p.Params[i].InitVal, v.Acc(i));
    }

    [Fact]
    public void StartKeyon_accepts_infinite_lifetime()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        var v = new TvfxVoice(0);
        v.StartKeyon(LoadFixturePatch(1), lifetimeTicks: -1);
        Assert.Equal(TvfxPhase.Keyon, v.Phase);       // no exception
    }

    [Fact]
    public void StartKeyon_primes_counters_to_one_so_first_tick_reads_stream()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        var v = new TvfxVoice(0);
        v.StartKeyon(LoadFixturePatch(1), lifetimeTicks: -1);
        for (int i = 0; i < 8; i++)
            Assert.Equal(1, v.Counter(i));      // first ServiceTick will decrement to 0 -> advance_segment
    }

    [Fact]
    public void StartKeyon_on_active_voice_resets_state()
    {
        if (!Fixtures.SkipIfUnavailable()) return;
        var v = new TvfxVoice(0);
        var p1 = LoadFixturePatch(1);     // Step
        var p2 = LoadFixturePatch(8);     // Water — different init vals
        v.StartKeyon(p1, lifetimeTicks: 10);
        v.StartKeyon(p2, lifetimeTicks: 20);
        Assert.Same(p2, v.Patch);
        Assert.Equal(TvfxPhase.Keyon, v.Phase);
        for (int i = 0; i < 8; i++)
            Assert.Equal(p2.Params[i].InitVal, v.Acc(i));
    }
}
