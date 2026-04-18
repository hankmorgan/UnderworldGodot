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

        // Cull path — UW1_asm.asm:64850 (JG, strict >) / uw2_asm.asm:79639.
        if (dist > CullDistance) return SoundFalloff.CulledResult;

        // Raw volume before distance attenuation:
        //   raw = SOUNDS.DAT[+2] + volDelta   (signed; UW1_asm.asm:64836-64849).
        int raw = baseVelocity + volDelta;

        int vol;
        if (dist < FullVolumeRadius)
        {
            vol = raw;                                           // no attenuation
        }
        else
        {
            // Attenuation — UW1_asm.asm:64866-64871 (imul bx=(0x30-dist); idiv 0x28).
            vol = raw * (CullDistance - dist) / FalloffDenominator;
        }
        vol = Clamp(vol, 0, PanMax);

        // Pan calc deferred to Task 3 — centre for now.
        byte pan = (byte)PanCentre;

        return new SoundFalloff((byte)vol, pan, culled: false);
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
