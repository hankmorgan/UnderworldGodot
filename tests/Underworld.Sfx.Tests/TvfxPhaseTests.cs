using Underworld.Sfx;

namespace Underworld.Sfx.Tests;

public class TvfxPhaseTests
{
    [Fact]
    public void Voice_starts_idle_and_stays_idle_without_keyon()
    {
        var v = new TvfxVoice(0);
        for (int i = 0; i < 100; i++) v.ServiceTick();
        Assert.Equal(TvfxPhase.Idle, v.Phase);
    }

    [Fact]
    public void Keyon_transitions_to_release_after_duration_plus_one_ticks()
    {
        // SyntheticPatch.Build sets duration=60. ALE.INC's S_duration is
        // T_duration+1 = 61; the 61st ServiceTick decrements to 0 and triggers
        // release. 60 ticks keeps us in Keyon.
        var p = SyntheticPatch.Build(0xFFFF, 0x8000);   // benign stream
        var v = new TvfxVoice(0);
        v.StartKeyon(p, lifetimeTicks: -1);

        for (int i = 0; i < 60; i++) v.ServiceTick();
        Assert.Equal(TvfxPhase.Keyon, v.Phase);
        v.ServiceTick();
        Assert.Equal(TvfxPhase.Release, v.Phase);
    }

    [Fact]
    public void Lifetime_expiry_forces_idle()
    {
        var p = SyntheticPatch.Build(0xFFFF, 0x8000);
        var v = new TvfxVoice(0);
        v.StartKeyon(p, lifetimeTicks: 3);

        v.ServiceTick();           // 3 → 2
        v.ServiceTick();           // 2 → 1
        Assert.Equal(TvfxPhase.Keyon, v.Phase);
        v.ServiceTick();           // 1 → 0, forces Idle
        Assert.Equal(TvfxPhase.Idle, v.Phase);
    }

    [Fact]
    public void Infinite_lifetime_does_not_decrement()
    {
        var p = SyntheticPatch.Build(0xFFFF, 0x8000);
        var v = new TvfxVoice(0);
        v.StartKeyon(p, lifetimeTicks: -1);
        for (int i = 0; i < 30; i++) v.ServiceTick();
        Assert.Equal(TvfxPhase.Keyon, v.Phase);
    }

    [Fact]
    public void Level_accumulator_clamps_at_zero_during_release()
    {
        // Hand-build a patch with a release stream on param 1 (level0/modulator
        // volume) whose STEP has a negative delta. Each stream has 2 bytes of
        // "size value" padding at the declared offset (ALE.INC 442-485 skips
        // these via add ax, 2 at init), so the actual STEP lands at offset+2.
        //
        // Layout (D = declared offset, S = STEP body):
        //   0x36 D,pad  freq keyon        padding
        //   0x38 S      freq keyon        STEP(1, 0)
        //   0x3C D,pad  shared dead keyon padding
        //   0x3E S      shared dead keyon STEP(0x7FFF, 0)
        //   0x42 D,pad  level0 release    padding
        //   0x44 S      level0 release    STEP(20, 0xFF00) = delta -256
        //   0x48 D,pad  shared dead rel   padding
        //   0x4A S      shared dead rel   STEP(0x7FFF, 0)
        //
        // duration=1 so we enter release fast.
        var raw = new byte[0x4E];
        raw[0] = 0x4E; raw[3] = (byte)TvfxType.TvEffect;
        raw[4] = 1; raw[5] = 0;     // duration = 1

        void WriteParam(int idx, ushort init, ushort keyon, ushort release)
        {
            int o = 6 + idx * 6;
            raw[o]     = (byte)(init & 0xFF);    raw[o + 1] = (byte)(init >> 8);
            raw[o + 2] = (byte)(keyon & 0xFF);   raw[o + 3] = (byte)(keyon >> 8);
            raw[o + 4] = (byte)(release & 0xFF); raw[o + 5] = (byte)(release >> 8);
        }
        WriteParam(0, 0,      0x36, 0x48);         // freq
        WriteParam(1, 0x0080, 0x3C, 0x42);         // level0 — small init, release points at negative-delta stream
        WriteParam(2, 0x4000, 0x3C, 0x48);         // level1 — init high so Release→Idle < 0x400 doesn't trip
        for (int i = 3; i < 8; i++) WriteParam(i, 0, 0x3C, 0x48);

        // 0x36 padding; 0x38: STEP(1, 0)             freq keyon
        raw[0x38] = 0x01; raw[0x39] = 0x00; raw[0x3A] = 0x00; raw[0x3B] = 0x00;
        // 0x3C padding; 0x3E: STEP(0x7FFF, 0)        shared dead keyon
        raw[0x3E] = 0xFF; raw[0x3F] = 0x7F; raw[0x40] = 0x00; raw[0x41] = 0x00;
        // 0x42 padding; 0x44: STEP(20, 0xFF00)       level0 release
        raw[0x44] = 20;   raw[0x45] = 0x00; raw[0x46] = 0x00; raw[0x47] = 0xFF;
        // 0x48 padding; 0x4A: STEP(0x7FFF, 0)        shared dead release
        raw[0x4A] = 0xFF; raw[0x4B] = 0x7F; raw[0x4C] = 0x00; raw[0x4D] = 0x00;

        var p = TvfxPatch.ForTesting(raw);
        var v = new TvfxVoice(0);
        v.StartKeyon(p, lifetimeTicks: -1);

        // duration=1 → S_duration = 2, so 2 ticks trip EnterRelease.
        v.ServiceTick(); v.ServiceTick();
        Assert.Equal(TvfxPhase.Release, v.Phase);
        Assert.Equal(0x0080, v.Acc(1));

        // After EnterRelease: counter[1]=1, increment[1]=0. Next tick loads
        // the release STEP (counter=20, delta=-256).
        v.ServiceTick();
        Assert.Equal(0x0080, v.Acc(1));

        // Now increment[1] = -256; first subtraction wraps 0x0080 past 0 —
        // the clamp must hold acc[1] at 0, not let it become 0xFF80.
        v.ServiceTick();
        Assert.Equal(0, v.Acc(1));
    }
}
