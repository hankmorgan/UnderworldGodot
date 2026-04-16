using Underworld.Sfx;

namespace Underworld.Sfx.Tests;

public class TvfxStreamVmTests
{
    private static TvfxVoice Run(TvfxPatch p, int ticks)
    {
        var v = new TvfxVoice(0);
        v.StartKeyon(p, lifetimeTicks: -1);
        for (int i = 0; i < ticks; i++) v.ServiceTick();
        return v;
    }

    [Fact]
    public void STEP_sets_counter_and_increment_then_accumulates()
    {
        // Stream: STEP(count=4, delta=0x0100).
        // Ordering: accumulate, then counter--, then maybe advance.
        // Tick 1: acc+=0 (still 0), counter 1→0, advance reads STEP → counter=4,
        //         delta=0x0100.
        // Tick 2: acc+=0x0100 → 0x0100, counter 4→3.
        // Tick 3: acc+=0x0100 → 0x0200.
        // Tick 4: acc+=0x0100 → 0x0300.
        var p = SyntheticPatch.Build(0x0004, 0x0100);
        var v = Run(p, ticks: 4);
        Assert.Equal(0x0300, v.Acc(0));
    }

    [Fact]
    public void SET_ACC_writes_accumulator_then_continues()
    {
        // SET_ACC(0xABCD), STEP(count=1, delta=0).
        var p = SyntheticPatch.Build(0xFFFF, 0xABCD, 0x0001, 0x0000);
        var v = Run(p, ticks: 1);
        Assert.Equal(0xABCD, v.Acc(0));
    }

    [Fact]
    public void SET_BASE_on_freq_updates_b0_base()
    {
        // SET_BASE(0x2000), STEP(1, 0). _b0Base = (0x2000 >> 8) = 0x20.
        // StartKeyon pre-sets _b0Base to 0x28 for TvEffect patches, so 0x20
        // unambiguously means SET_BASE executed.
        var p = SyntheticPatch.Build(0xFFFE, 0x2000, 0x0001, 0x0000);
        var v = Run(p, ticks: 1);
        Assert.Equal(0x20, v.B0Base);
    }

    [Fact]
    public void JUMP_moves_cursor_relative_to_current_position()
    {
        // 0x00: JUMP +8  (4 words ahead, skipping past SET_ACC(0xDEAD))
        // 0x04: SET_ACC(0xDEAD)   (skipped)
        // 0x08: STEP(1, 0)
        var p = SyntheticPatch.Build(
            0x0000, 0x0008,  // JUMP +8
            0xFFFF, 0xDEAD,  // SET_ACC (should be skipped)
            0x0001, 0x0000); // STEP
        var v = Run(p, ticks: 1);
        Assert.Equal(0x0000, v.Acc(0));
    }

    [Fact]
    public void JUMP_budget_stops_infinite_loop_safely()
    {
        // Infinite self-jump — budget exit should halt.
        var p = SyntheticPatch.Build(0x0000, 0x0000);
        var v = Run(p, ticks: 1);
        Assert.Equal(0x0000, v.Acc(0));
        Assert.Equal(0xFFFF, v.Counter(0));
    }

    [Fact]
    public void Out_of_bounds_cursor_halts_safely()
    {
        // Craft a patch whose freq keyon cursor points past the end of Raw.
        // VM must set counter=0xFFFF, increment=0 without throwing.
        // freq-keyon offset MUST be 0x34 so the ctor doesn't try to read the
        // optional ADSR block at 0x36..0x3D (which we haven't allocated).
        // Size = 0x36 fits the 54-byte header but leaves zero bytes for any
        // stream data — the VM's first read should safely OOB-halt.
        int freqStart = 0x34;
        int size = 0x36;
        var raw = new byte[size];
        raw[0] = (byte)(size & 0xFF); raw[1] = (byte)(size >> 8);
        raw[3] = (byte)TvfxType.TvEffect;
        raw[4] = 60; raw[5] = 0;

        void WriteParam(int idx, ushort off)
        {
            int o = 6 + idx * 6;
            raw[o + 2] = (byte)(off & 0xFF); raw[o + 3] = (byte)(off >> 8);
            raw[o + 4] = (byte)(off & 0xFF); raw[o + 5] = (byte)(off >> 8);
        }
        // Point every param at freqStart — but there are no bytes there, so
        // the first read is out-of-bounds.
        for (int i = 0; i < 8; i++) WriteParam(i, (ushort)freqStart);

        var p = TvfxPatch.ForTesting(raw);
        var v = new TvfxVoice(0);
        v.StartKeyon(p, lifetimeTicks: -1);
        v.ServiceTick();   // must not throw

        Assert.Equal(0xFFFF, v.Counter(0));
    }
}
