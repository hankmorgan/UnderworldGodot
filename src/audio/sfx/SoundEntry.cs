namespace Underworld.Sfx;

/// <summary>
/// One record from UW1's <c>SOUND/SOUNDS.DAT</c>. 24 records per file,
/// indexed by the sound id the game passes to <c>SoundEffects.Play</c>.
///
/// Pan is not in the record — it is always supplied per-trigger by the caller
/// (matches UW1.EXE behavior in <c>seg014_F69</c>, which forwards a caller-side
/// pan byte as MIDI CC 0x0A).
/// </summary>
public readonly record struct SoundEntry(
    byte PatchNum,      // TVFX patch number (bank=1 for SFX).
    byte Note,          // MIDI note — used by MT-32 backend; TVFX engine ignores it.
    byte Velocity,      // 0..127, clamped on trigger after any per-call offset.
    ushort DurationWord // Raw duration word from SOUNDS.DAT bytes 3..4.
                        // UW1: little-endian. UW2: big-endian (uw2_asm.asm:83683-83688).
                        // UW1 voice-lifetime: divide by 16 (seg014_F69 etc.).
                        // 0xFFFF = infinite (caller stops externally).
);
