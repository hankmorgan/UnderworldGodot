namespace Underworld.Sfx;

/// <summary>
/// Miles AIL 2.0 velocity-to-volume scaling for TVFX. Pure logic: no Godot
/// dependencies, suitable for unit testing.
/// </summary>
public static class TvfxVelocity
{
    /// <summary>
    /// Miles AIL 2.0 velocity-curve LUT. 16 bytes at YAMAHA.INC:240. Indexed
    /// by <c>effectiveVelocity &gt;&gt; 3</c> (upper 4 bits of a 7-bit velocity).
    /// Maps to a composite volume value in 82..127. The floor of 82 is
    /// deliberate — in the original driver, even velocity 0 produces
    /// ≈65% loudness; silence only comes from explicit voice termination
    /// or (in our pipeline) the distance-cull in PositionalAudio.
    /// </summary>
    public static readonly byte[] VelGraph =
    {
        82, 85, 88, 91, 94, 97, 100, 103,
        106, 109, 112, 115, 118, 121, 124, 127,
    };

    /// <summary>
    /// Compute the composite volume-scale byte (0..127) applied to the
    /// carrier's linear volume at TL write time. Replicates the Miles
    /// driver's velocity path under the assumption that CC 7 = CC 11 = 127
    /// (defaults; UW1 doesn't track them per sound). Under that assumption
    /// the `update_voice` composite (YAMAHA.INC:1491-1502) reduces to
    /// <c>VelGraph[effectiveVel &gt;&gt; 3]</c>.
    ///
    /// <para>
    /// effectiveVel = clamp(baseVelocity + (sbyte)velocityOffset, 0, 0x7F).
    /// The resulting byte multiplies the carrier's current "volume-in"
    /// (0..63) in <see cref="TvfxVoice.EmitRegisters"/> before inversion to
    /// OPL Total Level. Value 127 is the identity (no extra attenuation).
    /// </para>
    /// </summary>
    public static byte ComputeVolScale(byte baseVelocity, byte velocityOffset)
    {
        int effective = baseVelocity + (sbyte)velocityOffset;
        if (effective < 0) effective = 0;
        if (effective > 0x7F) effective = 0x7F;
        return VelGraph[effective >> 3];
    }
}
