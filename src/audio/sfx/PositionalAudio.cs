namespace Underworld.Sfx;

/// <summary>
/// Result of computing positional attenuation for a sound effect.
/// Pan is 0..0x7F, 0x40 = centre. Vol is 0..0x7F. Culled means the sound
/// should not be emitted at all (original returned 0xFF in this case).
/// </summary>
public readonly struct SoundFalloff
{
    public readonly byte Vol;
    public readonly byte Pan;
    public readonly bool Culled;
    public SoundFalloff(byte vol, byte pan, bool culled)
    { Vol = vol; Pan = pan; Culled = culled; }

    public static SoundFalloff CulledResult => new SoundFalloff(0, 0x40, true);
}

/// <summary>
/// Distance/pan falloff for positional sound effects. Implements the formula
/// shared by UW1 seg014_8AE (UW1_asm.asm:64454-64921) and UW2
/// Maybe3DAudioSource (uw2_asm.asm:79351-79706). Both games compute (vol, pan)
/// identically; UW2 adds voice-stealing priority on top (deferred).
///
/// Coordinates are packed (tile &lt;&lt; 3) | fine — 8 units per tile (9-bit).
/// Heading is 8-bit: 0..0xFF full circle.
/// </summary>
public static class PositionalAudio
{
    // Distance thresholds (UW1_asm.asm:64850, 64858, 64866-64871;
    //                     uw2_asm.asm:79639, 79649, 79655-79669).
    private const int CullDistance       = 0x30; // >48 drops the sound
    private const int FullVolumeRadius   = 0x08; // <8 no attenuation
    private const int FalloffDenominator = 0x28; // 48 - 8

    // MIDI-range clamps (UW1_asm.asm:64874-64888; uw2_asm.asm:79680-79695).
    private const int PanCentre = 0x40;
    private const int PanMax    = 0x7F;

    public static SoundFalloff Sample(
        int srcX, int srcY,
        int playerX, int playerY,
        byte heading8,
        int baseVelocity,
        sbyte volDelta)
    {
        int dx = srcX - playerX;
        int dy = srcY - playerY;
        int dist = IntSqrt(dx * dx + dy * dy);

        // Cull — UW1_asm.asm:64850 / uw2_asm.asm:79639.
        if (dist > CullDistance) return SoundFalloff.CulledResult;

        // Raw volume — UW1_asm.asm:64836-64849 (signed add of SOUNDS.DAT[+2]
        // and the caller's volDelta byte).
        int raw = baseVelocity + volDelta;

        int vol;
        if (dist < FullVolumeRadius) vol = raw;
        else vol = raw * (CullDistance - dist) / FalloffDenominator;
        vol = Clamp(vol, 0, PanMax);

        // Pan short-circuit — UW1_asm.asm:64614-64645 (dist==0 path).
        byte pan;
        if (dist == 0)
        {
            pan = (byte)PanCentre;
        }
        else
        {
            pan = ComputePan(dx, dy, dist, heading8);
        }

        return new SoundFalloff((byte)vol, pan, culled: false);
    }

    // Pan cross-product — UW1_asm.asm:64647-64835, uw2_asm.asm:79549-79636.
    //
    // The game normalises (dx, dy) to a Q7 vector on the unit circle by dividing
    // by the isqrt distance, with endpoint saturation to ±0x7F (UW1_asm.asm:
    // 64657, 64678 for dy; 64709, 64728 for dx). It then looks up sin/cos of
    // the rotated heading (angle = 0x4000 - (heading<<8) → 90° rotation;
    // UW1_asm.asm:64777) and forms a cross-product term:
    //   offset = (dyNorm * tB - dxNorm * tA) >> 8     // tB = sin term, tA = cos term
    //   pan    = clamp(0x40 - offset, 0, 0x7F)
    //
    // The sin-vs-cos assignment of the two table outputs was not verified
    // from the seg063 data bytes (see spec §Testing → sin/cos operand order).
    // If in-game test shows pan reversed, swap `tA` and `tB` in the two
    // Math.Round(...) lookups below — NOT the cross-product expression.
    // Using Math.Sin/Cos here instead of the seg063 LUT — drift at ≤1 LSB
    // is inaudible at 7-bit pan resolution.
    private static byte ComputePan(int dx, int dy, int dist, byte heading8)
    {
        // Normalise with endpoint saturation. At exactly ±dist the division
        // would overflow int16 in the original; the asm saturates to ±0x7F
        // (UW1_asm.asm:64657, 64678, 64709, 64728).
        int dxNorm = dx ==  dist ?  0x7F
                   : dx == -dist ? -0x80
                   : (dx << 7) / dist;
        int dyNorm = dy ==  dist ?  0x7F
                   : dy == -dist ? -0x80
                   : (dy << 7) / dist;

        // Rotated angle — UW1_asm.asm:64777. Note: the high byte of the 16-bit
        // parameter is the lookup index, so only the top 8 bits contribute.
        //   rotated8 = (0x40 - heading8) mod 256.
        byte rotated8 = (byte)((0x40 - heading8) & 0xFF);
        double theta = rotated8 * System.Math.PI * 2.0 / 256.0;

        // Table A and Table B outputs — shifted right 8 in the asm
        // (UW1_asm.asm:64798, 64801) to yield a signed byte in [-0x7F..0x7F].
        int tA = (int)System.Math.Round(System.Math.Cos(theta) * 0x7F);
        int tB = (int)System.Math.Round(System.Math.Sin(theta) * 0x7F);

        // Cross product — UW1_asm.asm:64803-64816.
        int offset = (dyNorm * tB - dxNorm * tA) >> 8;

        // Clamp — UW1_asm.asm:64822-64835.
        return (byte)Clamp(PanCentre - offset, 0, PanMax);
    }

    // Integer square root matching UW1 seg019_A78 behaviour
    // (UW1_asm.asm:74772). Newton-Raphson on 32-bit input.
    // For our range (dist² ≤ 2 × 512² = ~500k), a simple loop suffices.
    private static int IntSqrt(int n)
    {
        if (n <= 0) return 0;
        int x = n;
        int y = (x + 1) / 2;
        while (y < x) { x = y; y = (x + n / x) / 2; }
        return x;
    }

    private static int Clamp(int v, int lo, int hi)
        => v < lo ? lo : (v > hi ? hi : v);
}
