namespace Underworld.Sfx;

/// <summary>
/// Applies Miles AIL 2.0's per-sample volume and pan curves to a mono int16
/// buffer, producing stereo-interleaved int16 output. Byte-accurate to the
/// driver source at:
///   external/AIL2/DMASOUND.ASM:287-294   pan_graph LUT
///   external/AIL2/DMASOUND.ASM:993-1006  set_volume gain compute
///
/// Pan polarity: Miles native. P=0 → hard right, P=127 → hard left.
/// Audibly confirmed in-game on UW2 — no swap needed. This polarity matches
/// the AIL 2.0 driver convention that the reverse-engineered pan bytes from
/// <see cref="PositionalAudio.Sample"/> target.
/// </summary>
public static class StereoPanBake
{
    // pan_graph: 128-byte LUT, piecewise linear with saturation at i=63.
    // external/AIL2/DMASOUND.ASM:287-294.
    private static readonly byte[] _panGraph = BuildPanGraph();
    private static byte[] BuildPanGraph()
    {
        var g = new byte[128];
        for (int i = 0; i < 128; i++) g[i] = i <= 62 ? (byte)(2 * i) : (byte)127;
        return g;
    }
    public static byte PanGraph(int i) => _panGraph[i & 0x7F];

    // Gain denominator: pan_graph[max] * vol[max] = 127 * 127.
    // external/AIL2/DMASOUND.ASM:993-1006.
    private const int GainDenominator = 127 * 127; // 16129

    /// <summary>
    /// Produce stereo-interleaved int16 (L, R, L, R, ...) from a mono int16
    /// buffer using Miles AIL2's pan/volume curves.
    /// </summary>
    public static short[] Apply(short[] mono, byte vol, byte pan)
    {
        pan &= 0x7F;
        vol &= 0x7F;

        int gainL = _panGraph[pan]           * vol;   // Miles native: P=0 → right
        int gainR = _panGraph[(0x7F) - pan]  * vol;

        var stereo = new short[mono.Length * 2];
        for (int i = 0, j = 0; i < mono.Length; i++)
        {
            int s = mono[i];
            stereo[j++] = (short)((s * gainL) / GainDenominator);
            stereo[j++] = (short)((s * gainR) / GainDenominator);
        }
        return stereo;
    }
}
