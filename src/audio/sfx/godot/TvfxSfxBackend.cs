using System;
using Godot;

namespace Underworld.Sfx;

/// <summary>
/// SFX backend that drives a TVFX state machine over an OPL2 chip via
/// <see cref="SfxStreamPlayer"/>. The pan parameter is accepted but ignored —
/// real AdLib hardware is mono and the per-voice mix happens inside the OPL
/// chip; per-voice pan would require either a dedicated chip per voice or
/// post-mix tricks neither of which is in v1 scope.
/// </summary>
public sealed class TvfxSfxBackend : ISfxBackend
{
    private readonly TvfxPatchBank _bank;
    private readonly SfxStreamPlayer _player;

    public TvfxSfxBackend(TvfxPatchBank bank, SfxStreamPlayer player)
    {
        _bank = bank ?? throw new ArgumentNullException(nameof(bank));
        _player = player ?? throw new ArgumentNullException(nameof(player));
    }

    public void Play(SoundEntry entry, byte pan, byte velocityOffset)
    {
        // TODO(Task 8): velocityOffset is accepted here but not forwarded to
        // the voice pipeline — SfxCommand carries only (patch, lifetime).
        // As a result UW1 positional-audio volume attenuation is currently
        // inaudible on the OPL backend. Wiring this to per-voice velocity
        // scaling is a follow-up. See docs/audio-architecture.md §Positional
        // audio → UW1 OPL path. `pan` is authentic no-op (OPL is mono).
        var patch = _bank.GetTvfx(entry.PatchNum);
        if (patch == null)
        {
            GD.PushWarning($"TvfxSfxBackend: no patch for sound id (patchNum={entry.PatchNum})");
            return;
        }

        // SOUNDS.DAT byte3-4 → voice lifetime. Conversion traced in asm:
        //   - UW1_asm.asm 65827-65834: raw word → SIGNED divide by 16 (cwd;
        //     idiv bx with bx=16) → stored at dseg+262Ch as a countdown.
        //   - seg014_D15 decrements at 16 Hz (UW1_asm.asm 65608-65620 pushes
        //     0x10=16 Hz; seg020_94D + seg020_5D5 compute 1,000,000/16 = 62500
        //     µs period). Sends All-Notes-Off when the counter hits 0.
        //
        // The signed divide is load-bearing: raw values with bit 15 set
        // (0x8000, 0x8001 — 14 of UW1's 24 sounds) become large negative
        // quotients (e.g. 0x8000 / 16 signed = -2048 = 0xF800 unsigned). The
        // decrement loop from 0xF800 at 16 Hz takes ~66 minutes to reach 0 —
        // effectively infinite. Those sounds rely on game-side logic (trap
        // handlers etc.) to terminate, not the voice-lifetime timer.
        //
        // Our ServiceTick runs at 60 Hz. For positive quotients:
        //   game_lifetime_s = (raw/16) / 16 = raw / 256
        //   our_service_ticks = raw × 60 / 256 = raw × 15 / 64
        //
        // Sentinels:
        //   0xFFFF       : explicit "infinite" in the original loader
        //   bit 15 set   : signed divide wraps, decrement-to-zero takes ~66min
        //   both collapse to our -1 (don't enforce an external cutoff; let the
        //   TVFX engine's intrinsic Duration + release-decay terminate).
        int lifetime;
        if (entry.DurationWord == 0xFFFF || (entry.DurationWord & 0x8000) != 0)
            lifetime = -1;
        else
            lifetime = entry.DurationWord * 15 / 64;

        _player.Enqueue(new SfxCommand(patch, lifetime));
    }

    public void Dispose() { /* SfxStreamPlayer is owned by the Godot scene tree */ }
}
