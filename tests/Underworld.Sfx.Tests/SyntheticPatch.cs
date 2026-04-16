using Underworld.Sfx;

namespace Underworld.Sfx.Tests;

internal static class SyntheticPatch
{
    // Build a TVFX patch whose freq (param 0) keyon-stream is exactly the supplied
    // words, and all other params have a harmless "hold forever" STEP at a
    // shared dead stream.
    //
    // Layout mirrors real UW.AD patches: each stream's first 2 bytes are
    // padding that ALE.INC skips via "add ax, 2" at init. We include that
    // padding so the VM's cursor (_cursorWord[i] = offset/2 + 1) lands on the
    // first supplied stream word.
    public static TvfxPatch Build(params ushort[] freqStreamWords)
    {
        // Use 0x34 as the declared freq keyon offset so HasAdsrBlock is false
        // (anything != 0x34 triggers opt-block parsing which our tiny synthetic
        // patch doesn't allocate bytes for). Streams then start at 0x34 + 2 =
        // 0x36 per ALE.INC's "add ax, 2".
        int freqStart = 0x34;
        int freqData  = freqStart + 2;
        int deadStart = freqData + freqStreamWords.Length * 2;  // declared offset of dead stream
        int deadData  = deadStart + 2;
        int size      = deadData + 4;
        var raw = new byte[size];
        raw[0] = (byte)(size & 0xFF); raw[1] = (byte)(size >> 8);
        raw[3] = (byte)TvfxType.TvEffect;
        raw[4] = 60; raw[5] = 0;   // duration = 60

        void WriteParam(int idx, ushort keyon, ushort release)
        {
            int o = 6 + idx * 6;
            raw[o + 2] = (byte)(keyon & 0xFF); raw[o + 3] = (byte)(keyon >> 8);
            raw[o + 4] = (byte)(release & 0xFF); raw[o + 5] = (byte)(release >> 8);
        }
        WriteParam(0, (ushort)freqStart, (ushort)freqStart);
        for (int i = 1; i < 8; i++) WriteParam(i, (ushort)deadStart, (ushort)deadStart);

        for (int i = 0; i < freqStreamWords.Length; i++)
        {
            int o = freqData + i * 2;
            raw[o]     = (byte)(freqStreamWords[i] & 0xFF);
            raw[o + 1] = (byte)(freqStreamWords[i] >> 8);
        }
        // Dead stream (at deadData): STEP(counter=0x7FFF, delta=0) — never expires.
        raw[deadData]     = 0xFF; raw[deadData + 1] = 0x7F;
        raw[deadData + 2] = 0x00; raw[deadData + 3] = 0x00;

        return TvfxPatch.ForTesting(raw);
    }
}
