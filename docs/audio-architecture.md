# Audio architecture

How music and sound effects work, end to end. The two systems are independent
producers that share the Godot audio bus; they do not interact.

- **[Music](#music)** — XMI theme playback via one of four synth engines.
- **[Sound effects](#sound-effects)** — UW1 TVFX state-machine rendering through a bare OPL2 chip.

---

# Music

## Overview

Music in UnderworldGodot is synthesised in real time from the game's original XMI
files. There's no pre-rendering, no WAV cache, no startup conversion delay. Four
synth engines are supported, selected by the `synth` setting in `uwsettings.json`:

| Setting | Engine | Notes |
|---------|--------|-------|
| `cm32l` | [mt32emu](https://github.com/munt/munt) via [Munt.NET](https://github.com/abedegno/Munt.NET) | Circuit-level Roland CM-32L emulation. Requires ROM files. Authentic Ultima Underworld sound. |
| `mt32`  | mt32emu via Munt.NET | Same engine, MT-32 ROMs instead of CM-32L. |
| `soundfont` (default) | [MeltySynth](https://github.com/sinshu/meltysynth) | Pure C# SoundFont synth. Bundled Phoenix MT-32 soundfont works out of the box. |
| `opl`   | [AdlMidi.NET](https://github.com/csinkers/AdlMidi.NET) | OPL3 FM synthesis. AdLib/SoundBlaster-era sound. |

## Data flow

```
 XMI file (UWA01.XMI etc.)                          [game data on disk]
      │
      ▼
 XmiSequencer.ParseXmi()                            [Munt.NET — one-shot parse]
      │  List<MidiEvent> (tick, bytes)
      ▼
 XmiPlayer (stateful, re-renders on each call)      [Munt.NET]
      │  MIDI events fired at sample boundaries
      ▼
 ISynthEngine.PlayMsg / PlaySysex / Render          [Munt.NET / UnderworldGodot]
      │  short[] stereo PCM
      ▼
 MusicStreamPlayer.AudioThreadLoop (producer thread) [UnderworldGodot]
      │  Vector2[] frames
      ▼
 AudioStreamGeneratorPlayback.PushBuffer            [Godot]
      │  ring buffer, lock-free between threads
      ▼
 Godot audio thread (consumer)                      [Godot engine internals]
      │
      ▼
 Speakers / AudioBus
```

## Components

### `Munt.NET` (NuGet, separate repo)

Contains the cross-platform, Godot-independent pieces:

- **`ISynthEngine`** — interface: `PlayMsg`, `PlaySysex`, `Render`, `Reset`, `Dispose`.
  Packed MIDI message format: `status | (data1 << 8) | (data2 << 16)`. Render writes
  interleaved 16-bit stereo PCM.
- **`Mt32EmuSynth`** — existing P/Invoke wrapper around libmt32emu.
- **`Mt32EmuEngine`** — `ISynthEngine` impl wrapping `Mt32EmuSynth`. Handles ROM
  discovery (standard + MAME-style filenames).
- **`XmiSequencer`** — XMI parser. Produces a `List<MidiEvent>` of `(tick, byte[])`
  pairs. Understands XMI's note-duration encoding (Note On with inline duration
  generates a deferred Note Off at `tick + duration`).
- **`XmiPlayer`** — stateful real-time player. Owns an `ISynthEngine`, advances
  a current-tick cursor in step with audio rendering, fires MIDI events as the
  cursor crosses their tick, loops when the event list is exhausted.

### `UnderworldGodot` (this repo)

Godot-specific audio plumbing:

- **`src/audio/MusicStreamPlayer.cs`** — the Godot `Node`. Owns the synth engine,
  the `XmiPlayer`, and an `AudioStreamGenerator`. Runs a dedicated producer thread
  that fills the ring buffer.
- **`src/audio/MeltySynthEngine.cs`** — `ISynthEngine` over MeltySynth. Soundfont
  discovery order: explicit `synthpath` → `{BasePath}/SOUND/default.sf2` →
  `res://soundfonts/default.sf2` (bundled).
- **`src/audio/AdlMidiEngine.cs`** — `ISynthEngine` over AdlMidi.NET's real-time
  API. Loads the game's proprietary OPL bank (`UW.OPL` for UW2, `UW.AD` for UW1)
  and converts it to WOPL format for AdlMidi.
- **`src/loaders/xmimusic.cs`** — static `XMIMusic` class, now a thin facade:
  resolves theme numbers to XMI filenames (octal-encoded per original engine)
  and delegates playback to `MusicStreamPlayer.Instance.PlayXmi`.
- **`soundfonts/default.sf2`** — bundled Phoenix MT-32 soundfont (CC BY 3.0),
  used by MeltySynthEngine when no override is given.

## Why a dedicated producer thread

Godot 4's C# API does not expose a pull-based audio callback equivalent to Unity's
`OnAudioFilterRead` — that would require a GDExtension in C++. The pragmatic
alternative is the push model: the consumer calls `GetFramesAvailable()` and pushes
PCM via `PushBuffer`.

The naive approach is to push from `_Process()`, which runs on the main thread at
the game's frame rate. This breaks down during main-thread hitches — cutscene
panorama scrolls, texture loads, anything that stalls the main thread for longer
than the audio buffer. The ring buffer drains, the audio thread mixes silence,
playback pauses audibly.

A dedicated `System.Threading.Thread` that polls `GetFramesAvailable()` at a high
rate decouples audio production from main-thread frame pacing. The thread sleeps
5ms when the buffer is full (cheap poll, no tight spin). This is the pattern used
by FluidSynthGodot and similar Godot 4 C# audio projects.

### Thread safety

One shared piece of state: the `XmiPlayer`. Main thread calls `PlayXmi` / `Stop`;
producer thread calls `Render`. Protected by a single `_playerLock` mutex. The
lock holds only for the duration of one `Render` call (microseconds at 44.1 kHz),
so contention is negligible. `XmiPlayer` and the synth engines themselves remain
single-threaded internally — the lock ensures they're only accessed from one
thread at a time.

## Why real-time synthesis

The earlier pipeline rendered every XMI to a WAV file on startup and played the
WAV via `AudioStreamWav`. That approach had three problems:

1. **Startup delay** — first run rendered every track before the game could start,
   noticeable with CM-32L's reverb tail rendering.
2. **Loop gap** — pre-rendered WAVs baked in the reverb-decay silence at the end,
   producing an audible gap between loop iterations. The original DOS engine
   loops MIDI events with no gap: notes from the end of the track blend naturally
   into the start via the synth's own release envelopes.
3. **Pipeline complexity** — separate code paths for OPL and CM-32L rendering,
   WAV cache invalidation on settings changes, tail trimming heuristics, etc.

Real-time synthesis matches the original DOS engine architecture: MIDI events
stream to the synth, the synth produces audio on demand. No startup delay, no
loop gap, one unified code path across all four backends.

### Design inspiration

[Kweepa](https://github.com/Kweepa)'s Unity-based Ultima Underworld port
([repo](https://github.com/Kweepa/Underground)) demonstrated that real-time XMI
synthesis via MeltySynth is practical. That project uses Unity's
`OnAudioFilterRead` callback (runs on the audio thread automatically, no
threading concerns) and a soundfont for MT-32-ish sound. Our implementation
differs in two key ways: the threading model (managed producer thread instead of
engine callback) required by Godot's C# audio API, and the choice to support
mt32emu for authentic CM-32L/MT-32 emulation alongside the soundfont fallback.

## Theme selection and looping

Ultima Underworld's music is state-driven, not track-driven:

- Theme changes are triggered by game state events (enter combat, change level,
  enter menu, cutscene command 25).
- Tracks loop indefinitely once started — there is no "pick a new theme when the
  current one ends" logic in the original engine (confirmed from disassembly).

`XmiPlayer.Loop` is set to `true` by default in `XMIMusic.ChangeTheme`. The
`UW2WorldThemes` table provides three variants per world; one is picked at random
by `PickLevelThemeMusic()` when the world changes, not when a track ends.

## Settings

```json
{
    "synth": "soundfont",
    "synthpath": ""
}
```

- `synth`: one of `cm32l`, `mt32`, `soundfont`, `opl`. Default `soundfont`.
- `synthpath`: path to ROM directory (for cm32l/mt32) or `.sf2` file (for
  soundfont). Empty = use defaults (bundled soundfont, or no ROMs available).
- Legacy `rompath` is still honoured — if set and `synthpath` is empty, it's
  promoted to `synthpath` with a deprecation warning.

Changing the `synth` setting requires a game restart. The synth engine is
instantiated once in `MusicStreamPlayer._Ready` and lives for the lifetime of
the node.

## Fallback behaviour

If the primary synth engine fails to initialise (missing ROMs, corrupt soundfont,
missing native library) the system falls back to `AdlMidiEngine`. If that also
fails, music is silently disabled — the game continues to run. All failures are
logged to the Godot console.

## Native library resolution

`libmt32emu` (for mt32emu) and `libADLMIDI` (for OPL) ship as platform-specific
native libraries inside their respective NuGet packages under
`runtimes/{rid}/native/`. .NET's default native library loader doesn't look
there, so `MusicStreamPlayer.SetupMuntDllLoader` and
`AdlMidiEngine.SetupDllLoader` register `NativeLibrary.SetDllImportResolver`
callbacks that probe the runtime-specific paths.

Munt.NET is netstandard2.0 and can't register the resolver itself (no
`NativeLibrary` API), so `MusicStreamPlayer` does it on Munt.NET's behalf
before constructing `Mt32EmuEngine`.

## File index

| File | Purpose |
|------|---------|
| `src/audio/MusicStreamPlayer.cs` | Godot node, producer thread, synth selection, fallback |
| `src/audio/MeltySynthEngine.cs` | SoundFont backend |
| `src/audio/AdlMidiEngine.cs` | OPL/AdLib backend (includes OPL→WOPL bank conversion) |
| `src/loaders/xmimusic.cs` | Theme-number facade (thin) |
| `soundfonts/default.sf2` | Bundled Phoenix MT-32 soundfont (CC BY 3.0) |
| `soundfonts/LICENSE.txt` | Phoenix MT-32 attribution and license |

External dependencies:

- `Munt.NET` (NuGet or ProjectReference) — XmiPlayer, ISynthEngine, Mt32EmuEngine, Mt32EmuSynth
- `MeltySynth` (NuGet) — SoundFont synth engine
- `AdlMidi.NET` (ProjectReference, abedegno fork) — OPL synth engine

---

# Sound effects

UW1 ships SFX as a set of 24 "TVFX" (Time-Varying Effects) patches in
`SOUND/UW.AD` — not MIDI patches, but proprietary byte-code state machines that
animate 8 OPL2 parameters per voice at 60 Hz. Each per-game-event trigger
kicks off one voice; up to 9 voices (one per OPL2 channel) play concurrently.
Implementation lives in `src/audio/sfx/`.

UW2's SFX use pre-recorded `.voc` files for most sounds, falling back to the
same TVFX path when a `.voc` is absent. The UW2 VOC path is fully implemented:
mono 8-bit samples are stereo-baked at emit time using Miles AIL 2.0's
`pan_graph` × V / 16129 curve (see [Positional audio](#positional-audio)
below), then played through `AudioStreamPlayer`. Missing-VOC ids fall back
to the TVFX sink.

## Data flow

```
 SOUNDS.DAT (UW1: 24 × 5-byte, UW2: 31 × 8-byte) ──┐
                                                   ├─► SoundEntry[]   [Godot main thread]
 UW.AD (TVFX patch bank) ──────────────────────────┤
                                                   └─► TvfxPatch[]

 Game event (trap / object / combat / etc.)
   │
   ├── UWsoundeffects.PlaySoundEffectAtAvatar(id, pan, velOffset)       — non-positional
   ├── UWsoundeffects.PlaySoundEffectAtObject(id, uwObject, volDelta)   — positional, object-based
   └── UWsoundeffects.PlaySoundEffectAtCoordinate(id, packedX, packedY, volDelta)
            │
            ▼
       PositionalAudio.Sample(src, player, heading, baseVel, volDelta)
            │                                    [pure math, no Godot/game deps]
            ▼  SoundFalloff { Vol, Pan, Culled }
       (if Culled → drop)
            │
            ▼
       PlaySoundEffectAtAvatar(id, pan, velocityOffset = Vol - baseVel)
            │
  ┌─────────┴───────────────┐
  │                         │
  ▼ UW1                     ▼ UW2
 Sfx.SoundEffects.Play   vocLoader.Load → mono int16
  → TvfxSfxBackend        → StereoPanBake.Apply(vol, pan)  [Miles AIL2 pan_graph × V / 16129]
  → SfxCommand via SPSC   → AudioStreamWav (16-bit stereo)
  → producer thread       → main.instance.DigitalAudioPlayer
  → OplChip → frames       → Godot audio thread
  → AudioStreamGenerator
            │
            ▼
 Godot audio thread → Speakers / AudioBus
```

## Positional audio

UW1 and UW2 compute a `(vol, pan)` pair per SFX trigger from source position,
player position, player heading, and SOUNDS.DAT's per-sound base velocity.
Both games use byte-for-byte identical math — traced from
`UW1_asm.asm:64454-64921` (seg014_8AE) and `uw2_asm.asm:79351-79706`
(Maybe3DAudioSource).

**Math** lives in `src/audio/sfx/PositionalAudio.cs` as a pure function:

```csharp
SoundFalloff Sample(int srcX, int srcY, int playerX, int playerY,
                    byte heading8, int baseVelocity, sbyte volDelta)
    → { byte Vol; byte Pan; bool Culled; }
```

- **Coordinates** are packed `(tile << 3) | fine` — 8 units per tile.
- **Distance** is Euclidean `sqrt(dx² + dy²)` via Newton-Raphson isqrt.
- **Volume** — `raw = baseVel + volDelta`; if `dist < 8` unattenuated,
  if `dist > 48` culled (sound dropped), else `raw × (48 - dist) / 40`,
  clamped 0..0x7F.
- **Pan** — cross-product `(dxNorm×cosθ - dyNorm×sinθ) >> 8` applied to
  a 0x40-centred byte, where θ is the heading rotated 90° (matches the
  asm's `0x4000 - (heading << 8)` trick). Operand order was resolved
  audibly: see the code comment in `PositionalAudio.ComputePan` for the
  asm-authoritative expression.

**Stereo bake (UW2 VOC only).** `src/audio/sfx/StereoPanBake.cs` applies
Miles AIL 2.0's `pan_graph × V / 16129` L/R split to a mono int16 buffer,
producing stereo interleaved int16. Byte-accurate to the driver source at
`external/AIL2/DMASOUND.ASM:287-294` (pan_graph LUT) and `:993-1006`
(set_volume gain compute). The LUT is piecewise-linear with saturation at
index 63 — both channels run at full 127 in the "centre dead zone"
(`pan ∈ [63, 64]`), so only clearly off-centre pans produce audible
stereo separation.

**Miles native polarity:** pan byte 0 → hard right, 127 → hard left. This
is the AIL 2.0 convention, opposite to MIDI CC 10 but matches the driver
source this port targets.

**UW1 OPL path:** the TVFX backend is mono hardware — pan is silently
dropped (authentic AdLib behaviour). Volume attenuation flows through
`velocityOffset` on `Sfx.SoundEffects.Play`.

**Round-trip encoding:** `PlaySoundEffectAtCoordinate` computes an absolute
vol from `PositionalAudio.Sample`, converts back to `velocityOffset =
Vol - baseVel`, and forwards to `PlaySoundEffectAtAvatar`. The sink then
recomputes `baseVel + velocityOffset = Vol`. This keeps a single internal
API (`PlaySoundEffectAtAvatar(effectno, pan, velocityOffset)`) for both
positional and non-positional callers without changing 22 existing call
sites.

**Test-time resolutions** — three ambiguities the RE traces couldn't pin
down from the asm alone, all resolved audibly in-game:

| Ambiguity | Resolution |
|---|---|
| Heading bit-packing | `heading8 = (octant << 5) \| subAngle`, UW2 form |
| Sin/cos operand order | `(dxNorm × fwdX - dyNorm × fwdY) >> 8`, matches asm & upstream |
| L/R polarity | Miles native (pan=0 → right) |

## Backend selection

v1 ships the OPL/TVFX backend only. Other `synth` settings log a one-time
warning and SFX are silent for those users (no regression — they had no SFX
before). Note that the OPL backend is UW1-specific; UW2 uses the VOC path
regardless of `synth`, so UW2 SFX are audible on all synth choices.

| `synth` | SFX backend | Status |
|---|---|---|
| `opl` | TVFX over bare OPL2 chip (this doc) | shipping |
| `cm32l` / `mt32` / `soundfont` | MT-32 over the shared music `ISynthEngine` | deferred follow-up |

## Components

### `ADLMidi.NET` upstream addition

- **`OplChip`** — bare-chip wrapper around libadlmidi's embedded nuked-opl3.
  Exposes `Create(sampleRateHz)`, `WriteReg(addr, val)`, `GenerateFrames(buf, frames)`,
  `Reset`, `Dispose`. No MIDI / bank / sequencer layer. Pinned via local
  `feat/bare-opl-chip` branch until merged upstream ([PR #3](https://github.com/csinkers/AdlMidi.NET/pull/3)).

### TVFX engine (`src/audio/sfx/`, Godot-independent)

- **`SoundsDatLoader`** — record parser for both games. UW1 uses 5-byte records
  (little-endian DurationWord); UW2 uses 8-byte records (big-endian DurationWord,
  per `uw2_asm.asm:83683-83688`). Block size and endianness are selected from
  `UWClass._RES`. Returns `SoundEntry[]` (PatchNum, Note, Velocity, DurationWord).
- **`TvfxPatch`** — header parser. Fixed 54-byte header + optional 8-byte ADSR
  block (present iff `keyon_f_offset != 0x34`). Reads 8 `(InitVal, KeyonOffset,
  ReleaseOffset)` param triples.
- **`TvfxPatchBank`** — UW.AD index walker. Dispatches patches by size:
  14→OPL2 melodic (skipped), 248→MT-32 (skipped), else→TVFX.
- **`TvfxVoice`** — the per-voice state machine. Holds 8 parameter accumulators,
  counters, increments, stream cursors; aux OPL register bases; ADSR bytes;
  phase (Idle/Keyon/Release) and tick counters. Methods: `StartKeyon`,
  `ServiceTick`, `EmitRegisters(IOplRegisterSink)`, `EnterRelease` (private).
- **`TvfxVoiceAllocator`** — 9-slot voice pool (one per OPL2 channel).
  Allocate-first-free; returns null when saturated (drops the trigger, matching
  authentic UW behavior rather than voice-stealing).
- **`IOplRegisterSink`** — `WriteReg(addr, val)`. Decouples the TVFX engine
  from the specific OPL chip so tests can substitute a capturing/counting sink.
- **`SfxCommand`** + **`SpscQueue<T>`** — SPSC lock-free ring for crossing the
  game-thread → audio-thread boundary on trigger.

### Godot-facing layer (`src/audio/sfx/godot/`)

- **`SoundEffects`** — static façade. `Initialize(synth, soundDir)` loads
  SOUNDS.DAT once at boot; `Play(soundId, pan, velOffset)` dispatches to the
  active backend.
- **`TvfxSfxBackend`** — wraps `TvfxPatchBank` + `SfxStreamPlayer`. Translates
  SOUNDS.DAT `DurationWord` into service-tick lifetime (see asm derivation
  below). Pan is accepted but ignored — OPL2 hardware is mono.
- **`SfxStreamPlayer`** — Godot `Node`. Owns the `OplChip` instance + voice
  allocator + producer thread + `AudioStreamGenerator`. Mirrors
  `MusicStreamPlayer`'s threading pattern. Also hosts the dev-menu **backtick**
  trigger (`Shift+backtick` backward) that cycles through SOUNDS.DAT ids 0..23.

## TVFX format (on disk)

Every value reconciled against the original Miles driver's assembly source
(`audio/kail/ALE.INC` in [khedoros/uw-engine](https://github.com/khedoros/uw-engine)).

### SOUNDS.DAT (121 bytes)

```
offset 0   : uint8 count                         // 0x18 = 24
offset 1   : SoundEntry[count]                   // 5 bytes each

SoundEntry {
    uint8  patchNum         // TVFX bank-1 patch number
    uint8  note             // MIDI note — unused by TVFX
    uint8  velocity         // base velocity 0..127
    uint16 durationWord     // bytes 3..4 LE; lifetime encoding (see below)
}
```

### UW.AD index

Variable-length. Each entry is 6 bytes: `uint8 patch, uint8 bank, uint32 offset`.
Terminated by `(0xFF, 0xFF)`. UW1 SFX are all `bank=1`.

### TVFX patch

```
0x00 uint16  size                    // total patch bytes incl. header
0x02 uint8   transpose
0x03 uint8   type                    // 1=TV_INST, 2=TV_EFFECT (UW SFX are all 2)
0x04 uint16  duration                // 60 Hz ticks before EnterRelease
0x06 .. 0x35   8 × 6-byte param triples:
                uint16 initVal, uint16 keyonOffset, uint16 releaseOffset
[optional, present iff keyonOffset != 0x34:]
0x36 uint8   keyon_sr_car            // ALE.INC field labels are backwards;
0x37 uint8   keyon_ad_car            //   the HIGH byte of each WORD is AD,
0x38 uint8   keyon_sr_mod            //   the LOW byte is SR
0x39 uint8   keyon_ad_mod
0x3A uint8   release_sr_car
0x3B uint8   release_ad_car
0x3C uint8   release_sr_mod
0x3D uint8   release_ad_mod
[stream data follows, starting at keyonOffset + 2 (the +2 skips a size-padding
 word that ALE.INC unconditionally jumps over at init via `add ax, 2`)]
```

### Stream VM opcodes

Each entry is 4 bytes = two u16 little-endian words `(w0, w1)`.

| `w0` | opcode | action |
|---|---|---|
| `0x0000` | JUMP | `cursor += (int16)w1 / 2` words (signed — matches ALE.INC's modular `add di, dx`) |
| `0xFFFF` | SET_VAL | `acc = w1`; continue reading |
| `0xFFFE` | SET_BASE | update aux OPL register per param (freq→B0, level0→KSL mod, ...); continue |
| else | STEP | `counter = w0`, `increment = (int16)w1`; **return** |

## SOUNDS.DAT lifetime derivation

Traced from `UW1_asm.asm`:

- **Storage:** SOUNDS.DAT's byte3-4 word is signed-divided by 16
  (`cwd; idiv bx` with `bx=0x10`) at the trigger site (`seg014_F69`,
  `UW1_asm.asm:65827-65834`) and stashed at `dseg+0x262C` per-voice.
- **Decrement:** `seg014_D15` runs as an **AIL PIT-timer callback at 16 Hz**
  (derivation: `UW1_asm.asm:65608-65620` pushes `0x10` as the Hz argument to
  `seg020_94D`; internal math `1,000,000 µs/s ÷ 16 Hz = 62,500 µs period`).
  Each callback decrements the counter; when it hits 0, the driver emits MIDI
  All-Notes-Off on the voice's channel.
- **Conversion to our 60 Hz service ticks:** `service_ticks = raw × 60 / 256 =
  raw × 15 / 64`. Applied in `TvfxSfxBackend.Play`.
- **Infinite sentinel:** `0xFFFF` is an explicit no-expiry flag in the driver.
  Values with bit-15 set (`0x8000`, `0x8001`, etc.) produce a large negative
  signed quotient that takes ~66 minutes to decrement to zero — effectively
  infinite. Both map to `-1` internally; patches with this value rely on
  game-side logic (trap handlers etc.) to terminate voices.

## Why nuked-opl3

libadlmidi embeds [nuked-opl3](https://github.com/nukeykt/Nuked-OPL3), a
bit-accurate OPL3 emulator validated against real silicon. We use it in
OPL2-compat mode (the OPL3 "new" bit stays 0 at reset). The sister project
[pyopl](https://pypi.org/project/pyopl/) uses DOSBox's **dbopl** — an older,
faster, less-accurate emulator. Both implement the OPL2 register spec, but
their internal waveform and envelope tables differ enough to produce
audibly different harmonic content on identical register sequences. This is
a known difference in the retro-audio community; neither is "wrong".

## Reverse-engineering notes

The TVFX engine went through 13 asm-verified bug fixes during development.
Notable ones (each cites the ALE.INC line that resolved it):

1. **AD/SR byte order** (ALE.INC:414-424) — struct field names like
   `T_play_ad_1` at offset 0x36 are misleading. The asm reads a WORD into AX/DX
   and stores DH→S_AD, DL→S_SR: HIGH byte is AD, LOW byte is SR. khedoros's
   field names match the asm; psmitty7373's writeup swapped them.
2. **+2 stream padding** (ALE.INC:442-485) — every keyon/release offset is
   incremented by 2 bytes at init. psmitty's Python is explicit about it;
   khedoros bakes the +2 into `update_data[]`'s memcpy base so it's invisible
   in the VM code.
3. **Voice-lifetime rate** — 16 Hz PIT callback, not 60 Hz.
   Derivation in "SOUNDS.DAT lifetime" above.
4. **OPL2 waveform-select enable** — `reg 0x01 = 0x20`. Without it the chip
   forces all operators to sine regardless of the `0xE0+op` waveform register.
5. **HasAdsrBlock sentinel** — `!= 0x34`, not `== 0x3C` (ALE.INC:410-412).
   Same for real UW.AD patches (which only use 0x34 or 0x3C) but diverges
   from the asm's actual test.
6. **TV_INST freq SET_BASE mask** (ALE.INC:__TV_inst branch) — `_b0Base &= 0xE0`
   only for `Type == TV_INST`. khedoros omits this mask.
7. **TV_INST duration** (ALE.INC:428-434) — S_duration overrides to `0xFFFF`
   for `TV_INST`; patches of that type are held until externally note-off'd.
8. **Release-to-Idle unsigned compare** (ALE.INC:371-376) — `cmp ax, 400h; jnb`
   is an unsigned-not-below branch. We were using a signed cast.
9. **EnterRelease aux-reg reset** (ALE.INC:393-407) — `TV_phase` runs the full
   aux-register reset (S_FBC=0, S_KSLTL_*=0, S_AVEKM_*=0x20, S_BLOCK=0x20 or
   0x28) before **both** keyon and release branches.
10. **Mark-dirty-always after VM** (ALE.INC:242) — `or S_update, U_FREQ`
    **unconditionally** after `call TVFX_increment_stage`. khedoros overwrites
    `changed` with the VM's bool return, dropping the flag when the VM ran
    STEP-only.
11. **Duration off-by-one** (ALE.INC:428-436, 365) — `S_duration = T_duration + 1`,
    then `dec; jnz` fires KEYOFF on the `(T_duration + 1)`-th tick. Our
    `>` should be `>=`.
12. **Level clamp bidirectional** (ALE.INC:287-298) — clamp fires when the top
    bit of the accumulator flipped AND new+increment have the same top bit.
    Catches positive-increment-wrap-downward as well as the more common
    negative-increment-wrap-upward. Our simplified check missed the first case.
13. **KeyOff on voice termination** (khedoros `tvfx_note_free`, behavior implied
    by ALE.INC's release-voice path) — clear bit 0x20 of B0 when a voice
    transitions to Idle. Without it the OPL envelope keeps the note keyed
    forever.

The bug-finding pattern was consistent: **khedoros's C++ port often hides
structural details inside data construction that aren't visible in the
runtime code**. Porting from khedoros's VM was the mistake that made several
of these bugs latent; comparing against ALE.INC's asm directly is the
authoritative path.

## Tooling

Under `tools/` — not shipped to end users, but useful for verification:

- **`tools/SfxWav`** — CLI that runs the full TVFX engine + `OplChip` and dumps
  one WAV per SOUNDS.DAT entry. `--trace` also writes per-sound register-write
  traces for diffing. `--ignore-lifetime` plays the full patch duration
  (matches psmitty's reference render for A/B comparison).
- **`tools/psmitty_reference`** — vendored Python TVFX renderer from
  [hankmorgan/UnderworldGodot#28](https://github.com/hankmorgan/UnderworldGodot/issues/28)
  (uses DOSBox's dbopl via pyopl). Authored by @psmitty7373 as an independent
  reference oracle. README in-tree documents its known inaccuracies.
- **`tools/sfx-compare/index.html`** — browser-based A/B page. Plays our
  render vs psmitty's for each of the 24 sounds, with optional full-duration
  mode and cache-busting so regenerated WAVs are always fresh.

## Unit tests

`tests/Underworld.Sfx.Tests/` — xUnit project targeting `net10.0`. Compiles the
pure-logic SFX source files (everything not under `godot/`) via `<Compile Include>`
so tests run on plain .NET without a Godot runtime. 68 tests covering:

- `SoundsDatLoader` — golden UW1 fixture, field ranges, UW1 LE decode,
  UW2 BE decode regression (per `uw2_asm.asm:83683-83688`).
- `TvfxPatchBank` / `TvfxPatch` — index parsing, opt-block sentinel, per-field
  parsing, coverage check (all 24 SFX resolve to valid TVFX patches).
- `TvfxVoice` — scaffold / StartKeyon init / counter priming.
- Stream VM — each opcode (JUMP, SET_ACC, SET_BASE, STEP) + JUMP budget +
  out-of-bounds halt.
- Per-tick register writes — 13-write count, channel-to-operator mapping on
  channels 0 and 5.
- Phase transitions — Keyon→Release timing, lifetime expiry to Idle,
  release-phase level clamp.
- Voice allocator — 9-slot saturation, reallocation of Idle voices.
- SPSC queue — FIFO, full-detection, concurrent-producer-consumer smoke.
- `PositionalAudio` — 12 tests: distance bands (0/7/8/28/48/49), cull, volume
  clamp edges, pan symmetry under mirror sources, pan behaviour under heading
  rotation, pan in-range clamping.
- `StereoPanBake` — 8 tests: pan centre equality, hard-left/right Miles native,
  saturation edge at pan 63/64, volume attenuation scaling, interleaved output
  length, pan_graph LUT spot-checks (0, 1, 62, 63, 64, 127).

## File index (SFX)

| File | Purpose |
|------|---------|
| `src/audio/sfx/SoundEntry.cs` | SOUNDS.DAT record (pure data) |
| `src/audio/sfx/SoundsDatLoader.cs` | 5-byte (UW1 LE) / 8-byte (UW2 BE) record parser |
| `src/audio/sfx/PositionalAudio.cs` | Pure falloff math (vol, pan, cull) |
| `src/audio/sfx/StereoPanBake.cs` | Miles AIL2 pan_graph × V / 16129 stereo bake |
| `src/audio/sfx/TvfxPatch.cs` | Header parser, opt-block detect |
| `src/audio/sfx/TvfxPatchBank.cs` | UW.AD index walker + lazy patch load |
| `src/audio/sfx/TvfxVoice.cs` | State machine, stream VM, register emitter |
| `src/audio/sfx/TvfxVoiceAllocator.cs` | 9-voice pool |
| `src/audio/sfx/IOplRegisterSink.cs` | OPL write abstraction |
| `src/audio/sfx/SfxCommand.cs` | Trigger command value type |
| `src/audio/sfx/SpscQueue.cs` | Lock-free SPSC ring |
| `src/audio/sfx/ISfxBackend.cs` | Backend interface |
| `src/audio/sfx/godot/SoundEffects.cs` | Static façade + backend selection |
| `src/audio/sfx/godot/SfxStreamPlayer.cs` | Godot node, producer thread, OplChip owner, dev-menu |
| `src/audio/sfx/godot/TvfxSfxBackend.cs` | Bank + player wiring + lifetime conversion |
| `src/audio/UWSoundEffects.cs` | Public `PlaySoundEffect*` façade; UW1/UW2 dispatch; UW2 VOC stereo bake |

External additions:

- `AdlMidi.NET` `feat/bare-opl-chip` — `OplChip` bare-chip wrapper (upstream PR open).
