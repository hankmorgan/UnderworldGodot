# Audio architecture

How the music system works, end to end.

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
