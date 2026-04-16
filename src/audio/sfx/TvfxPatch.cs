namespace Underworld.Sfx;

public enum TvfxType : byte { BnkInst = 0, TvInst = 1, TvEffect = 2, Opl3Inst = 3 }

/// <summary>
/// One of the 8 TVFX parameter triples (InitVal, KeyonOffset, ReleaseOffset).
/// The two offsets are absolute byte offsets from the start of the patch data.
/// </summary>
public readonly record struct TvfxParam(ushort InitVal, ushort KeyonOffset, ushort ReleaseOffset);

/// <summary>
/// Parsed UW TVFX patch (bank=1 in UW.AD). Fixed 54-byte header with 8×6-byte
/// param triples, then an optional 8-byte ADSR override block (present iff
/// <c>Params[0].KeyonOffset == 0x3C</c>), then the variable-length segment
/// streams indexed by each param's KeyonOffset / ReleaseOffset.
///
/// Reference: ALE.INC:44-85, uw_patch.cpp:22-71 (khedoros/uw-engine).
/// </summary>
public sealed class TvfxPatch
{
    public byte[] Raw { get; }
    public ushort Size { get; }
    public byte Transpose { get; }
    public TvfxType Type { get; }
    public ushort Duration { get; }
    public TvfxParam[] Params { get; }
    public bool HasAdsrBlock { get; }

    public byte KeyonAdCar { get; }   public byte KeyonSrCar { get; }
    public byte KeyonAdMod { get; }   public byte KeyonSrMod { get; }
    public byte ReleaseAdCar { get; } public byte ReleaseSrCar { get; }
    public byte ReleaseAdMod { get; } public byte ReleaseSrMod { get; }

    public static TvfxPatch ForTesting(byte[] raw) => new TvfxPatch(raw);

    internal TvfxPatch(byte[] raw)
    {
        Raw = raw;
        Size = U16(raw, 0x00);
        Transpose = raw[0x02];
        Type = (TvfxType)raw[0x03];
        Duration = U16(raw, 0x04);

        Params = new TvfxParam[8];
        for (int i = 0; i < 8; i++)
        {
            int o = 0x06 + i * 6;
            Params[i] = new TvfxParam(
                InitVal:       U16(raw, o + 0),
                KeyonOffset:   U16(raw, o + 2),
                ReleaseOffset: U16(raw, o + 4));
        }

        // ALE.INC:410-412 reads T_init_f_offset, adds 2, and jumps to
        // __use_default iff the result == 0x36 (i.e. keyon_f_offset == 0x34).
        // ANY other value means the 8-byte optional ADSR block is present at
        // 0x36..0x3D. khedoros (and an earlier version of this code) uses
        // `== 0x3C` instead — functionally equivalent for UW.AD patches (which
        // only use 0x34 or 0x3C) but diverges from ALE.INC's actual test.
        HasAdsrBlock = Params[0].KeyonOffset != 0x34;
        if (HasAdsrBlock)
        {
            // ALE.INC field labels are misleading: the asm at lines 414-424 reads
            // a WORD into AX/DX (low=byte at offset N, high=byte at offset N+1)
            // then stores DH→S_AD_0 and DL→S_SR_0 — i.e. the HIGH byte of each
            // pair is the AD register and the LOW byte is the SR register.
            // khedoros's struct field naming (uw_patch.h:123-133) reflects this
            // correctly: bytes 0x36/0x38/0x3A/0x3C are SR, 0x37/0x39/0x3B/0x3D
            // are AD. An earlier version of this code had the two swapped,
            // which produced wrong envelope timing on every patch with the
            // optional ADSR block.
            KeyonSrCar   = raw[0x36]; KeyonAdCar   = raw[0x37];
            KeyonSrMod   = raw[0x38]; KeyonAdMod   = raw[0x39];
            ReleaseSrCar = raw[0x3A]; ReleaseAdCar = raw[0x3B];
            ReleaseSrMod = raw[0x3C]; ReleaseAdMod = raw[0x3D];
        }
        else
        {
            // Defaults from oplSequencer.cpp:500-504.
            KeyonAdCar = KeyonAdMod = ReleaseAdCar = ReleaseAdMod = 0xFF;
            KeyonSrCar = KeyonSrMod = ReleaseSrCar = ReleaseSrMod = 0x0F;
        }
    }

    private static ushort U16(byte[] b, int o) => (ushort)(b[o] | (b[o + 1] << 8));
}
