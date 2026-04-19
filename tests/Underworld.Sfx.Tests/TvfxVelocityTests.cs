using Underworld.Sfx;

namespace Underworld.Sfx.Tests;

public class TvfxVelocityTests
{
    // Reference: Miles AIL 2.0 YAMAHA.INC:240 (vel_graph) + :2376-2387 (note_on
    // velocity capture) + :1491-1502 (update_voice composite with CC 7 / CC 11).
    // UW1 doesn't separately track CC 7 / CC 11 per effect; we use the Miles
    // defaults (C = E = 127), under which the composite simplifies to
    // vol = vel_graph[effectiveVel >> 3].

    [Fact]
    public void VelGraph_matches_YAMAHA_INC_line_240()
    {
        // Exact 16-byte dump. Prevents future edits from drifting the curve.
        byte[] expected = { 82, 85, 88, 91, 94, 97, 100, 103, 106, 109, 112, 115, 118, 121, 124, 127 };
        for (int i = 0; i < 16; i++)
            Assert.Equal(expected[i], TvfxVelocity.VelGraph[i]);
    }

    [Fact]
    public void Max_effective_velocity_returns_127()
    {
        // baseVel=0x7F, no offset → effectiveVel=0x7F → V>>3=15 → vel_graph[15]=127.
        byte vol = TvfxVelocity.ComputeVolScale(baseVelocity: 0x7F, velocityOffset: 0);
        Assert.Equal((byte)127, vol);
    }

    [Fact]
    public void Zero_effective_velocity_floors_at_82()
    {
        // baseVel=0x7F, offset=-0x7F → effectiveVel=0 → V>>3=0 → vel_graph[0]=82.
        // Miles loudness floor — see YAMAHA.INC:240 comment in TvfxVelocity.
        byte vol = TvfxVelocity.ComputeVolScale(
            baseVelocity: 0x7F, velocityOffset: unchecked((byte)(-0x7F)));
        Assert.Equal((byte)82, vol);
    }

    [Fact]
    public void Mid_effective_velocity_picks_correct_bucket()
    {
        // baseVel=0x40 (64), no offset → 64>>3=8 → vel_graph[8]=106.
        byte vol = TvfxVelocity.ComputeVolScale(baseVelocity: 0x40, velocityOffset: 0);
        Assert.Equal((byte)106, vol);
    }

    [Fact]
    public void Negative_effective_velocity_clamps_to_zero()
    {
        // baseVel=0x10, offset=-0x40 → effectiveVel=-0x30 → clamp to 0 → vel_graph[0]=82.
        byte vol = TvfxVelocity.ComputeVolScale(
            baseVelocity: 0x10, velocityOffset: unchecked((byte)(-0x40)));
        Assert.Equal((byte)82, vol);
    }

    [Fact]
    public void Over_max_effective_velocity_clamps_to_0x7F()
    {
        // baseVel=0x70, offset=+0x20 → effectiveVel=0x90 → clamp 0x7F → vel_graph[15]=127.
        byte vol = TvfxVelocity.ComputeVolScale(baseVelocity: 0x70, velocityOffset: 0x20);
        Assert.Equal((byte)127, vol);
    }

    [Fact]
    public void Velocity_bucket_boundaries_use_right_shift_3()
    {
        // effectiveVel=7 → V>>3=0 → vel_graph[0]=82 (still floor).
        // effectiveVel=8 → V>>3=1 → vel_graph[1]=85 (first step up).
        Assert.Equal((byte)82, TvfxVelocity.ComputeVolScale(0x07, 0));
        Assert.Equal((byte)85, TvfxVelocity.ComputeVolScale(0x08, 0));
    }
}
