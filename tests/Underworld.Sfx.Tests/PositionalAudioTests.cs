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
}
