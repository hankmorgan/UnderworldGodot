namespace Underworld.Sfx;

public enum TvfxPhase { Idle, Keyon, Release }

/// <summary>
/// Per-voice TVFX state machine. Holds the 8 parameter accumulators, aux OPL
/// register bases, ADSR bytes, and phase/lifetime trackers driven at 60 Hz by
/// the (forthcoming) stream VM.
///
/// Reference: oplSequencer.cpp:472-565 switch_tvfx_phase (khedoros/uw-engine).
/// </summary>
public sealed class TvfxVoice
{
    public int Channel { get; }
    public TvfxPhase Phase { get; private set; } = TvfxPhase.Idle;
    public TvfxPatch? Patch { get; private set; }

    private readonly ushort[] _acc       = new ushort[8];
    private readonly ushort[] _counter   = new ushort[8];
    private readonly short[]  _increment = new short[8];
    private readonly int[]    _cursorWord = new int[8];

    private byte _b0Base;
    private byte _kslMod, _kslCar;
    private byte _fconBase;
    private byte _avekmModBase, _avekmCarBase;

    private byte _adMod, _srMod, _adCar, _srCar;

    // Per-element dirty bits: bit i = element i's contributing OPL register(s)
    // need to be re-emitted next EmitRegisters. Set when increment is non-zero
    // (acc changing this tick) or when SET_VAL/SET_BASE/STEP fires. Cleared
    // when emitted. Matches khedoros tvfx_update[voice] (oplSequencer.cpp:723).
    // ADSR registers are tracked separately because they only change at phase
    // boundaries, not per-tick.
    private byte _updateMask;
    private bool _adsrDirty;

    private int _phaseTicks;
    private int _lifetimeTicks;        // -1 == infinite (game's 0xFFFF sentinel)

    // Composite volume scale applied to the carrier's linear volIn before
    // TL inversion. Valid range is 0..127; the upstream pipeline (TvfxVelocity
    // .ComputeVolScale → VelGraph) guarantees values in 82..127. 127 is the
    // identity (byte-identical to the pre-scaling carrier TL). No clamp here
    // — callers are responsible for the 0..127 invariant. Source: YAMAHA.INC:
    // 1491-1502 (composite compute) + :1748-1756 (carrier TL write scaling).
    private byte _volScale = 127;

    public TvfxVoice(int channel) { Channel = channel; }

    public ushort Acc(int paramIndex) => _acc[paramIndex];
    public ushort Counter(int paramIndex) => _counter[paramIndex];
    public ushort B0Base => _b0Base;

    /// <summary>
    /// Advance all 8 parameter streams by one 60 Hz tick. For each param:
    /// accumulate (u16 wrapping), decrement counter, and when counter hits 0
    /// read the next byte-code entries until a STEP (or safety exit) is reached.
    /// Reference: oplSequencer.cpp:627-685 iterateTvfxCommandList.
    /// </summary>
    public void ServiceTick()
    {
        if (Phase == TvfxPhase.Idle || Patch is null) return;

        for (int i = 0; i < 8; i++)
        {
            // Per ALE.INC lines 234-242 (and equivalent for each element):
            //   if increment != 0: acc += increment; OR S_update, U_bit
            //   counter--
            //   if counter == 0: call TVFX_increment_stage; OR S_update, U_bit
            // Critically, both flag-sets are ORs, not overwrites. khedoros's
            // port OVERWRITES with the VM's bool return (oplSequencer.cpp:735),
            // dropping the flag when an increment accumulated this tick AND
            // the VM ran a STEP-only transition. This mis-renders any patch
            // whose animation transitions between STEPs mid-ramp (audibly: a
            // jagged volume curve). We follow ALE.INC: OR, not overwrite.
            bool changed = false;
            if (_increment[i] != 0)
            {
                ushort prev = _acc[i];
                ushort next = (ushort)(prev + (ushort)_increment[i]);
                // Level clamp during Release: if the top bit of the acc
                // changed (sign flipped = wrapped past 0x8000) AND the new
                // value shares a top bit with the increment (i.e. they're on
                // the same side of zero), clamp to 0. Matches ALE.INC:287-298
                // exactly — fires for both negative-increment-wrap-upward
                // AND positive-increment-wrap-downward. Applies only to
                // acc[1]/acc[2] (the level accumulators).
                if (Phase == TvfxPhase.Release && (i == 1 || i == 2))
                {
                    ushort flipped = (ushort)(prev ^ next);
                    ushort sameSide = (ushort)(next ^ (ushort)_increment[i]);
                    if ((flipped & 0x8000) != 0 && (sameSide & 0x8000) == 0)
                        next = 0;
                }
                _acc[i] = next;
                changed = true;
            }

            if (_counter[i] > 0) _counter[i]--;
            if (_counter[i] == 0)
            {
                AdvanceSegment(i);
                changed = true;      // ALE.INC:242 — always mark dirty after VM ran
            }

            if (changed) _updateMask |= (byte)(1 << i);
        }

        _phaseTicks++;

        // ALE.INC:428-436 stores S_duration = T_duration + 1 at keyon entry,
        // then `dec S_duration; jnz` each tick (line 365). KEYOFF fires when
        // S_duration hits 0 — i.e. on the (T_duration + 1)-th tick after
        // entry. TV_INST patches override S_duration to 0xFFFF (effectively
        // infinite).
        //
        // We count _phaseTicks UP (tick 1 is the first ServiceTick), so the
        // correct trigger is `_phaseTicks >= Patch.Duration + 1`. Using `>`
        // was an off-by-one that delayed release by 1 tick (~16.7 ms) —
        // audible on single-tick percussive SFX.
        int triggerTick = Patch!.Type == TvfxType.TvInst ? 0x10000 : Patch.Duration + 1;
        if (Phase == TvfxPhase.Keyon && _phaseTicks >= triggerTick)
            EnterRelease();

        // Release → Idle once both level accumulators have decayed below the
        // 0x400 threshold (unsigned). Matches ALE.INC:371-376 (`cmp ax,400h;
        // jnb` = unsigned jump-if-not-below). Our level clamp during release
        // prevents negative wraps so high-bit-set values shouldn't occur here
        // in practice — but matching ALE.INC's unsigned cmp is the safe
        // default. The _phaseTicks > 0 guard prevents a spurious Idle on the
        // release-entry tick for sfx patches whose level0/1 init at 0.
        if (Phase == TvfxPhase.Release && _phaseTicks > 0 &&
            _acc[1] < 0x400 && _acc[2] < 0x400)
            Phase = TvfxPhase.Idle;

        if (_lifetimeTicks > 0)
        {
            _lifetimeTicks--;
            if (_lifetimeTicks == 0) Phase = TvfxPhase.Idle;
        }
    }

    /// <summary>
    /// Transition Keyon → Release: swap in the patch's release-ADSR bytes and
    /// re-prime each param's stream cursor to its release-stream offset.
    /// Accumulators are intentionally preserved — release continues the current
    /// waveform/volume from where keyon left off, then decays.
    /// Reference: oplSequencer.cpp:472-565 switch_tvfx_phase.
    /// </summary>
    private void EnterRelease()
    {
        var patch = Patch!;
        Phase = TvfxPhase.Release;
        _phaseTicks = 0;

        for (int i = 0; i < 8; i++)
        {
            // +1 word: same "size value" padding ALE.INC skips at keyon init
            // applies to release streams too. See StartKeyon cursor comment.
            _cursorWord[i] = patch.Params[i].ReleaseOffset / 2 + 1;
            _counter[i]    = 1;
            _increment[i]  = 0;
        }

        // ALE.INC:393-407 runs the same aux-register reset block (TV_phase)
        // before both KEYON and KEYOFF branches. EnterRelease must apply the
        // same defaults as StartKeyon, otherwise any SET_BASE writes from the
        // keyon stream leak into the release phase.
        _b0Base       = patch.Type == TvfxType.TvInst ? (byte)0x20 : (byte)0x28;
        _kslMod = _kslCar = 0;
        _fconBase     = 0;
        _avekmModBase = _avekmCarBase = 0x20;

        _adMod = patch.ReleaseAdMod; _srMod = patch.ReleaseSrMod;
        _adCar = patch.ReleaseAdCar; _srCar = patch.ReleaseSrCar;
        _adsrDirty = true;          // ADSR registers need re-emit at phase boundary
        _updateMask = 0xFF;         // and all element regs to reflect the new phase
    }

    /// <summary>
    /// Walk the param's byte-code stream forward by up to 10 entries. Returns
    /// true if any SET_VAL or SET_BASE was applied (caller marks the element
    /// dirty for emission). STEP termination returns false — the new STEP starts
    /// a fresh animation; no register write is needed for the act of starting.
    /// JUMP / iteration-budget exhaustion return false (no state change visible
    /// to OPL). Matches khedoros iterateTvfxCommandList return semantics.
    /// </summary>
    private bool AdvanceSegment(int i)
    {
        var raw = Patch!.Raw;
        bool valChanged = false;
        for (int iter = 0; iter < 10; iter++)
        {
            int bytePos = _cursorWord[i] * 2;
            if (bytePos < 0 || bytePos + 4 > raw.Length)
            {
                _counter[i] = 0xFFFF;
                _increment[i] = 0;
                return valChanged;
            }
            ushort w0 = (ushort)(raw[bytePos]     | (raw[bytePos + 1] << 8));
            ushort w1 = (ushort)(raw[bytePos + 2] | (raw[bytePos + 3] << 8));

            if (w0 == 0x0000)
            {
                // JUMP: w1 is a signed byte delta. ALE.INC:134-136 describes
                // 0xFFFC = -4 bytes as the canonical "loop back" pattern, using
                // x86 modular add. khedoros oplSequencer.cpp:641 divides w1 as
                // unsigned, which works for forward jumps but breaks negative
                // loops (0xFFFC/2u = 0x7FFE, not -2). We follow ALE.INC here.
                _cursorWord[i] += (short)w1 / 2;
                continue;
            }

            _cursorWord[i] += 2;

            if (w0 == 0xFFFF)
            {
                _acc[i] = w1;
                valChanged = true;
                continue;
            }
            if (w0 == 0xFFFE)
            {
                ApplySetBase(i, w1);
                valChanged = true;
                continue;
            }

            // STEP — sets up the next animation phase. Does not itself dirty the
            // OPL register; the next tick's increment-driven accumulate will.
            _counter[i] = w0;
            _increment[i] = (short)w1;
            return valChanged;
        }

        // Iteration budget exhausted — halt this stream safely.
        _counter[i] = 0xFFFF;
        _increment[i] = 0;
        return valChanged;
    }

    private void ApplySetBase(int paramIndex, ushort data)
    {
        switch (paramIndex)
        {
            case 0:
                // ALE.INC:__TV_inst branch ANDs S_BLOCK with 0xE0 for TV_INST
                // patches — keeps only the block bits, strips any FNum high
                // bits a SET_BASE might try to set. khedoros misses this.
                byte b = (byte)((data >> 8) & 0xFF);
                if (Patch!.Type == TvfxType.TvInst) b &= 0xE0;
                _b0Base = b;
                break;
            case 1: _kslMod       = (byte)(data & 0xFF);        break; // level0 → S_ksltl_0
            case 2: _kslCar       = (byte)(data & 0xFF);        break; // level1 → S_ksltl_1
            case 3: /* priority: no-op */                       break;
            case 4: _fconBase     = (byte)((data >> 8) & 0xFF); break; // feedback → S_fbc
            case 5: _avekmModBase = (byte)(data & 0xFF);        break; // mult0 → S_avekm_0
            case 6: _avekmCarBase = (byte)(data & 0xFF);        break; // mult1 → S_avekm_1
            case 7: /* waveform: no-op */                       break;
        }
    }

    /// <summary>
    /// Begin the Keyon phase for this voice.
    /// <para>
    /// Precondition: voice is <see cref="TvfxPhase.Idle"/>; caller
    /// (<c>TvfxVoiceAllocator</c>) ensures this by allocating a free voice first
    /// (mirrors khedoros <c>find_unused_voice</c> in <c>playSfx</c>). Calling on
    /// an active voice restarts it — state is fully re-initialised, no smooth
    /// handover. No runtime assertion is raised; restart is safe.
    /// </para>
    /// </summary>
    public void StartKeyon(TvfxPatch patch, int lifetimeTicks, byte volScale = 127)
    {
        Patch = patch;
        Phase = TvfxPhase.Keyon;
        _phaseTicks = 0;
        _lifetimeTicks = lifetimeTicks;
        _volScale = volScale;

        for (int i = 0; i < 8; i++)
        {
            _acc[i]        = patch.Params[i].InitVal;
            _counter[i]    = 1;                                  // first tick decrements to 0 → VM reads stream
            _increment[i]  = 0;
            // (keyon_offset + 2) / 2 as word index. ALE.INC at lines 442-485
            // adds 2 bytes to every one of the 8 params' keyon offsets when
            // initialising S_*_offset — "adds 2 because of the size value of
            // the timbre, I think" (comment in the original asm). psmitty's
            // Python does the same (+2 byte offset). Missing this +2 was
            // making the VM read stream entries one half-entry too early,
            // producing wrong audio on every patch.
            _cursorWord[i] = patch.Params[i].KeyonOffset / 2 + 1;
        }

        // S_block: 0x20 for TV_INST, 0x28 otherwise (oplSequencer.cpp:490-497).
        _b0Base       = patch.Type == TvfxType.TvInst ? (byte)0x20 : (byte)0x28;
        _kslMod = _kslCar = 0;                                   // oplSequencer.cpp:485-486
        _fconBase     = 0;                                       // oplSequencer.cpp:484
        _avekmModBase = _avekmCarBase = 0x20;                    // oplSequencer.cpp:487-488

        _adMod = patch.KeyonAdMod; _srMod = patch.KeyonSrMod;
        _adCar = patch.KeyonAdCar; _srCar = patch.KeyonSrCar;
        _adsrDirty = true;          // emit ADSR once at phase entry
        _updateMask = 0xFF;         // and prime all element regs for the first emit
    }

    private static readonly int[] MOD_OFFSETS = { 0x00, 0x01, 0x02, 0x08, 0x09, 0x0A, 0x10, 0x11, 0x12 };
    private static readonly int[] CAR_OFFSETS = { 0x03, 0x04, 0x05, 0x0B, 0x0C, 0x0D, 0x13, 0x14, 0x15 };

    /// <summary>
    /// Emit OPL2 register writes for any state that changed since the previous
    /// emit. Mirrors khedoros tvfx_update_voice (oplSequencer.cpp:568-624) which
    /// consults a per-element dirty mask and only writes the registers that
    /// actually changed — typical sounds emit 1-3 writes per tick, not 13.
    ///
    /// ADSR (0x60/0x80) is special-cased: it only changes at phase boundaries
    /// and is tracked by _adsrDirty separately from _updateMask.
    ///
    /// On Idle transition, B0 is force-emitted with bit 0x20 (KeyOn) cleared
    /// even if its mask bit is clean — without this the OPL envelope stays in
    /// sustain and the note rings forever (oplSequencer.cpp:688-695 tvfx_note_free).
    /// </summary>
    public void EmitRegisters(IOplRegisterSink sink)
    {
        int mod = MOD_OFFSETS[Channel];
        int car = CAR_OFFSETS[Channel];

        if (_adsrDirty)
        {
            sink.WriteReg(0x60 + mod, _adMod);
            sink.WriteReg(0x60 + car, _adCar);
            sink.WriteReg(0x80 + mod, _srMod);
            sink.WriteReg(0x80 + car, _srCar);
            _adsrDirty = false;
        }

        if ((_updateMask & (1 << 5)) != 0)   // mult0 → AVEKM mod
            sink.WriteReg(0x20 + mod, (byte)((_acc[5] >> 12) | _avekmModBase));
        if ((_updateMask & (1 << 6)) != 0)   // mult1 → AVEKM car
            sink.WriteReg(0x20 + car, (byte)((_acc[6] >> 12) | _avekmCarBase));
        if ((_updateMask & (1 << 1)) != 0)   // level0 → KSL/TL mod
            sink.WriteReg(0x40 + mod, (byte)(((~(_acc[1] >> 10)) & 0x3F) | _kslMod));
        if ((_updateMask & (1 << 2)) != 0)   // level1 → KSL/TL car
        {
            // Miles AIL 2.0 carrier TL scaling (YAMAHA.INC:1748-1756):
            //   volIn  = (~patch_TL) & 0x3F  — but TVFX stores volume-form
            //                                 directly in _acc[2] >> 10.
            //   scaled = volIn * vol / 127   — multiplication in linear space.
            //   TL     = (~scaled) & 0x3F    — invert back to attenuation.
            int volIn = (_acc[2] >> 10) & 0x3F;
            int scaled = (volIn * _volScale) / 127;
            int tl = (~scaled) & 0x3F;
            sink.WriteReg(0x40 + car, (byte)(tl | _kslCar));
        }
        if ((_updateMask & (1 << 4)) != 0)   // feedback → FBC
            sink.WriteReg(0xC0 + Channel, (byte)(((_acc[4] >> 12) & 0x0E) | (_fconBase & 1)));
        if ((_updateMask & (1 << 7)) != 0)   // waveform → both ops
        {
            sink.WriteReg(0xE0 + mod, (byte)((_acc[7] >> 8) & 0x07));
            sink.WriteReg(0xE0 + car, (byte)(_acc[7] & 0x07));
        }

        bool emitFreq = (_updateMask & (1 << 0)) != 0 || Phase == TvfxPhase.Idle;
        if (emitFreq)
        {
            sink.WriteReg(0xA0 + Channel, (byte)((_acc[0] >> 6) & 0xFF));
            byte b0 = (byte)(((_acc[0] >> 6) >> 8) | _b0Base);
            if (Phase == TvfxPhase.Idle) b0 = (byte)(b0 & ~0x20);
            sink.WriteReg(0xB0 + Channel, b0);
        }

        _updateMask = 0;
    }
}
