using Underworld.Sfx;

namespace Underworld.Sfx.Tests;

public class PositionalAudioTests
{
    // All tests use packed coords (tile<<3 | fine). 8 units per tile.
    // Constants under test trace to:
    //   UW1 seg014_8AE  (UW1_asm.asm:64454-64921)
    //   UW2 Maybe3DAudioSource  (uw2_asm.asm:79351-79706)

    [Fact]
    public void Dist_zero_returns_base_volume_and_centre_pan()
    {
        // UW1 seg014_8AE short-circuit at UW1_asm.asm:64614-64645:
        //   if dist == 0: vol = SOUNDS.DAT[+2] + volDelta; pan = 0x40.
        var r = PositionalAudio.Sample(
            srcX: 80, srcY: 80, playerX: 80, playerY: 80,
            heading8: 0, baseVelocity: 0x50, volDelta: 0);
        Assert.False(r.Culled);
        Assert.Equal((byte)0x50, r.Vol);
        Assert.Equal((byte)0x40, r.Pan);
    }

    [Fact]
    public void Dist_seven_is_full_volume()
    {
        // dist < 8 branch: no attenuation (UW1_asm.asm:64858 / uw2_asm.asm:79649).
        var r = PositionalAudio.Sample(
            srcX: 87, srcY: 80, playerX: 80, playerY: 80,
            heading8: 0, baseVelocity: 0x50, volDelta: 0);
        Assert.False(r.Culled);
        Assert.Equal((byte)0x50, r.Vol);
    }

    [Fact]
    public void Dist_eight_begins_attenuation()
    {
        // dist == 8 → vol = raw * (0x30 - 8) / 0x28 = raw * 40/40 = raw.
        // (First point of the attenuation branch; still unattenuated at the edge.)
        // Constants: UW1_asm.asm:64850, 64858, 64866-64871.
        var r = PositionalAudio.Sample(
            srcX: 88, srcY: 80, playerX: 80, playerY: 80,
            heading8: 0, baseVelocity: 0x50, volDelta: 0);
        Assert.False(r.Culled);
        Assert.Equal((byte)0x50, r.Vol);
    }

    [Fact]
    public void Dist_twenty_eight_is_attenuated_halfway()
    {
        // dist = 28, raw = 0x50 (80): vol = 80 * (48-28) / 40 = 80 * 20/40 = 40 = 0x28.
        var r = PositionalAudio.Sample(
            srcX: 80 + 28, srcY: 80, playerX: 80, playerY: 80,
            heading8: 0, baseVelocity: 0x50, volDelta: 0);
        Assert.False(r.Culled);
        Assert.Equal((byte)0x28, r.Vol);
    }

    [Fact]
    public void Dist_fortyeight_still_audible()
    {
        // UW1_asm.asm:64850 uses JG (>), i.e. STRICT greater-than → 48 passes.
        // raw * (48-48) / 40 = 0 → clamped to 0.
        var r = PositionalAudio.Sample(
            srcX: 80 + 48, srcY: 80, playerX: 80, playerY: 80,
            heading8: 0, baseVelocity: 0x50, volDelta: 0);
        Assert.False(r.Culled);
        Assert.Equal((byte)0x00, r.Vol);
    }

    [Fact]
    public void Dist_fortynine_is_culled()
    {
        // dist > 48 → return 0xFF ("culled") per UW1_asm.asm:64850-64856.
        var r = PositionalAudio.Sample(
            srcX: 80 + 49, srcY: 80, playerX: 80, playerY: 80,
            heading8: 0, baseVelocity: 0x50, volDelta: 0);
        Assert.True(r.Culled);
    }

    [Fact]
    public void Volume_clamps_to_upper_bound()
    {
        // raw = 0x7F + 10 = 137 → clamp to 0x7F at UW1_asm.asm:64874-64888.
        var r = PositionalAudio.Sample(
            srcX: 80, srcY: 80, playerX: 80, playerY: 80,
            heading8: 0, baseVelocity: 0x7F, volDelta: 10);
        Assert.False(r.Culled);
        Assert.Equal((byte)0x7F, r.Vol);
    }

    [Fact]
    public void Volume_clamps_to_lower_bound()
    {
        // raw = 5 + (-20) = -15 → clamp to 0.
        var r = PositionalAudio.Sample(
            srcX: 80, srcY: 80, playerX: 80, playerY: 80,
            heading8: 0, baseVelocity: 5, volDelta: -20);
        Assert.False(r.Culled);
        Assert.Equal((byte)0, r.Vol);
    }

    // Heading convention in these tests — per UW2 uw2_asm.asm:79427-79441:
    //   heading8 is 8-bit, 0..255 full circle.
    //   The pan formula applies 0x4000-(heading<<8) as a 90° rotation
    //   before the sin/cos lookup (UW1_asm.asm:64777, uw2_asm.asm:79445).
    //
    // For facing heading=0 (the +X axis in game coords), a source at
    // positive dY should pan to one side and negative dY to the other.
    // The exact polarity is subject to the RE's "sin vs cos table
    // ambiguity" (spec §Testing → sin/cos operand order). The tests
    // assert SYMMETRY and THAT PAN MOVES OFF CENTRE, not the sign.
    // In-game audible test resolves the sign.

    [Fact]
    public void Pan_symmetric_about_source_on_axis()
    {
        // Source in front of the player — pan should be near centre.
        var r = PositionalAudio.Sample(
            srcX: 80 + 16, srcY: 80, playerX: 80, playerY: 80,
            heading8: 0, baseVelocity: 0x50, volDelta: 0);
        Assert.InRange(r.Pan, (byte)(0x40 - 4), (byte)(0x40 + 4));
    }

    [Fact]
    public void Pan_moves_off_centre_for_side_sources()
    {
        // Source to one side: pan shifts distinctly off centre.
        var left  = PositionalAudio.Sample(
            srcX: 80, srcY: 80 + 16, playerX: 80, playerY: 80,
            heading8: 0, baseVelocity: 0x50, volDelta: 0);
        var right = PositionalAudio.Sample(
            srcX: 80, srcY: 80 - 16, playerX: 80, playerY: 80,
            heading8: 0, baseVelocity: 0x50, volDelta: 0);
        Assert.NotEqual((byte)0x40, left.Pan);
        Assert.NotEqual((byte)0x40, right.Pan);
        // Mirror sources produce mirrored pans about 0x40.
        Assert.Equal(left.Pan, (byte)(0x80 - right.Pan));
    }

    [Fact]
    public void Pan_behaviour_under_heading_rotation()
    {
        // Rotating the player 180° should flip which side a fixed source pans to.
        var at0   = PositionalAudio.Sample(
            srcX: 80, srcY: 80 + 16, playerX: 80, playerY: 80,
            heading8: 0x00, baseVelocity: 0x50, volDelta: 0);
        var at180 = PositionalAudio.Sample(
            srcX: 80, srcY: 80 + 16, playerX: 80, playerY: 80,
            heading8: 0x80, baseVelocity: 0x50, volDelta: 0);
        Assert.NotEqual(at0.Pan, at180.Pan);
        // Values should mirror about 0x40.
        Assert.Equal(at0.Pan, (byte)(0x80 - at180.Pan));
    }

    [Fact]
    public void Pan_clamps_to_valid_range()
    {
        // Source at extreme dy with small dx — normalised ratio hits saturation
        // at ±0x7F (UW1_asm.asm:64657, 64678). Result must be in [0, 0x7F].
        var r = PositionalAudio.Sample(
            srcX: 80, srcY: 80 + 40, playerX: 80, playerY: 80,
            heading8: 0x40, baseVelocity: 0x50, volDelta: 0);
        Assert.InRange(r.Pan, (byte)0, (byte)0x7F);
    }
}
