using Underworld.Sfx;

namespace Underworld.Sfx.Tests;

public class StereoPanBakeTests
{
    // Constants and table source: external/AIL2/DMASOUND.ASM:287-294 (pan_graph),
    //                             external/AIL2/DMASOUND.ASM:993-1006 (mixer).
    // Miles native polarity (spec §StereoPanBake): P=0 → hard right, P=127 → hard left.

    private static short[] Mono(int n, short s)
    {
        var b = new short[n]; for (int i = 0; i < n; i++) b[i] = s; return b;
    }

    [Fact]
    public void Pan_centre_equal_both_channels()
    {
        // P=0x40 → pan_graph[64]=127, pan_graph[63]=127 (saturate at i>=63).
        // Both gains = 127 * V. At V=0x7F: gain = 127*127/16129 = 1.0 → output = input.
        var stereo = StereoPanBake.Apply(Mono(1, 1000), vol: 0x7F, pan: 0x40);
        Assert.Equal(2, stereo.Length);          // L, R
        Assert.Equal((short)1000, stereo[0]);
        Assert.Equal((short)1000, stereo[1]);
    }

    [Fact]
    public void Pan_zero_hard_right_miles_native()
    {
        // P=0x00 → pan_graph[0]=0, pan_graph[127]=127 (saturated).
        // Left = 0 * V = 0; Right = 127 * V = full at V=0x7F.
        // Interleaved [L, R] per sample.
        var stereo = StereoPanBake.Apply(Mono(1, 1000), vol: 0x7F, pan: 0x00);
        Assert.Equal((short)0, stereo[0]);       // L
        Assert.Equal((short)1000, stereo[1]);    // R
    }

    [Fact]
    public void Pan_max_hard_left_miles_native()
    {
        // P=0x7F → pan_graph[127]=127, pan_graph[0]=0.
        // Left full, Right silent.
        var stereo = StereoPanBake.Apply(Mono(1, 1000), vol: 0x7F, pan: 0x7F);
        Assert.Equal((short)1000, stereo[0]);    // L
        Assert.Equal((short)0, stereo[1]);       // R
    }

    [Fact]
    public void Pan_saturation_edge_p63_equals_p64()
    {
        // pan_graph saturates at i>=63 (external/AIL2/DMASOUND.ASM:287-294).
        // So P=63 and P=64 must produce identical output on the "saturated side".
        var at63 = StereoPanBake.Apply(Mono(1, 1000), vol: 0x7F, pan: 63);
        var at64 = StereoPanBake.Apply(Mono(1, 1000), vol: 0x7F, pan: 64);
        // At P=63: pan_graph[63]=127 (L side), pan_graph[64]=127 (R side) → both full.
        // At P=64: pan_graph[64]=127 (L side), pan_graph[63]=127 (R side) → both full.
        // Both should be identical.
        Assert.Equal(at63[0], at64[0]);
        Assert.Equal(at63[1], at64[1]);
    }

    [Fact]
    public void Vol_zero_silences_both_channels()
    {
        var stereo = StereoPanBake.Apply(Mono(1, 1000), vol: 0, pan: 0x40);
        Assert.Equal((short)0, stereo[0]);
        Assert.Equal((short)0, stereo[1]);
    }

    [Fact]
    public void Vol_half_attenuates_both_channels()
    {
        // V=0x40 (64). pan_graph[0x40] saturates at 127. Gain = 127*64/16129.
        // For sample=10000: output = 10000 * 127 * 64 / 16129 = ~5037.
        // Allow ±1 for integer truncation.
        var stereo = StereoPanBake.Apply(Mono(1, 10000), vol: 0x40, pan: 0x40);
        Assert.InRange(stereo[0], (short)5035, (short)5039);
        Assert.InRange(stereo[1], (short)5035, (short)5039);
    }

    [Fact]
    public void Output_length_is_twice_input()
    {
        // Interleaved stereo — N mono samples produce 2N stereo samples.
        var stereo = StereoPanBake.Apply(Mono(100, 500), vol: 0x7F, pan: 0x40);
        Assert.Equal(200, stereo.Length);
    }

    [Fact]
    public void PanGraph_lookup_spot_checks()
    {
        // External/AIL2/DMASOUND.ASM:287-294:
        //   pan_graph[i]      = 2*i    for i in 0..62
        //   pan_graph[63..127] = 127
        Assert.Equal((byte)0,   StereoPanBake.PanGraph(0));
        Assert.Equal((byte)2,   StereoPanBake.PanGraph(1));
        Assert.Equal((byte)124, StereoPanBake.PanGraph(62));
        Assert.Equal((byte)127, StereoPanBake.PanGraph(63));
        Assert.Equal((byte)127, StereoPanBake.PanGraph(64));
        Assert.Equal((byte)127, StereoPanBake.PanGraph(127));
    }
}
