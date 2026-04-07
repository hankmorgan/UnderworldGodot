# Cutscene Engine TODO

Remaining work to complete the UW2 cutscene rendering engine in Godot.
Based on reverse engineering of the original DOS engine (ovr108) and
DPaint LPF format analysis. See `UWReverseEngineering/Cutscene Engine RE Notes.md`
and the Python reference extractor at `UW2/extract_intro_frames.py`.

## Status

### Completed
- [x] All 28 bytecode commands parsed and named (CutsceneCommand.cs)
- [x] Segment-based command processor with frame scheduling (cutsplayer.cs)
- [x] Auto-advance file system across segments
- [x] hasLastDelta handling — DPaint loop delta excluded from display (cutsloader.cs)
- [x] FinalPixelBuffer captured before loop delta for correct delta chaining
- [x] fps field read from LPF header (cutsloader.cs)
- [x] recordSize check to skip empty delta frames
- [x] Scroll direction table lookup (func 23) — 4 directions from disassembly
- [x] BYT.ARK palette indices corrected from disassembly (bytloader.cs)
- [x] Animated fade-in/fade-out with rate control (func 9/10)
- [x] Blocking post-segment commands (frame=999)

### Panorama Scrolling — Mostly Complete

The original engine uses a VGA offscreen buffer for hardware scrolling
(SetViewportFar writes to VGA CRT registers 0x0C/0x0D). See RE Notes
for full rendering pipeline details.

#### LBACK Background Loading (func 22 — map-file) ✅
- [x] Load `CUTS/LBACK{idx:03d}.BYT` files (raw 320x200 pixel data, no header)
- [x] Render LBACK using the current LPF file's palette
- [x] Build panorama composite:
  - Horizontal: width = 320 + vp_start_x. First LBACK at x=0, second at x=320.
  - Vertical: stack LBACKs sequentially (LBACK at y=0, next at y=200, etc.)
- [x] Composite built when `set-start` (func 21) fires

#### Scroll Viewport (func 23 — start-scroll) ✅
- [x] Each frame, crop viewport from the composite at the current scroll position
- [x] Scroll formula: `pos = start + (frame+1) * delta * direction_table[index]`
  (frame+1 from `inc ax` at ovr108_B9E, line 439116)
- [x] Scene height = 200 - vpOffsetY, with black subtitle bar below
- [x] Frame timing from LPF fps header (not hardcoded)

#### Sprite Overlay During Scroll ✅
- [x] Decode sprite LPF with LBACK raw pixels as base, resetting to LBACK before
  each keyframe (is_sprite mode). Prevents accumulation from prior keyframes.
- [x] Generate RLE write masks in the decoder to track which pixels were explicitly
  written (dump/run) vs skipped
- [x] Overlay only RLE-written pixels onto the cropped viewport region at **fixed
  screen positions** (not into the scrolling composite). This matches the original
  engine's DrawArtToScreen (line 444058) which draws at a fixed VGA screen position.
- [x] The LPF animation already compensates for scroll progression in its coordinate
  space — drawing into the composite would cause double-scrolling.

#### Remaining Panorama Issues
- [x] Fine pixel panning: original engine uses VGA register 0x3C0 index 0x33 for
  sub-byte pixel offset within Mode X's 4-pixel byte groups
  (ConfigureDisplayTiming_seg003_0272_2A0D, line 11584). The fine pan value is
  `(scrollX & 3) << 1` (lines 11677-11679). Since our composite is pixel-addressed
  (not byte-addressed like Mode X), we don't need to simulate this — our whole-pixel
  scroll from the composite is equivalent to the combined coarse+fine VGA scroll.
- [ ] Verify sprite alignment on all scroll scenes (CS000 horizontal/vertical tested)

### Palette Interpolation (func 19) ✅

The engine interpolates the LPF's embedded palette toward a target palette from PALS.DAT.
This creates effects like sunset-to-night transitions (CS000 N13) and dawn colour shifts
(CS001 N04). NOT palette rotation.

- [x] Load target palette from PALS.DAT via OpenPalsData (ovr108_1229, line 440668)
- [x] Linear interpolation matching InterpolatePaletteRange_ovr108_32CC (line 446461):
  `result = source + (step * (target - source)) / total`
- [x] Interpolates ALL 256 palette entries ([bx+5974h] = 0x100, line 440656)
  NOT just cycling ranges — the earlier RE notes were wrong about this
- [x] Source palette snapshot stored at [si+55CAh] (lines 440689-440699)
- [x] Speed parameter: interpolate every N frames (timer delay)
- [x] Re-render current frame with updated palette each step
- [x] Params: [0]=PALS.DAT index, [1]=speed, [2]=total frames

### TODO — CutsLoader Enhancements

- [ ] Expose `framesPerSecond` field as a public property for cutsplayer to use
- [ ] Expose color cycling ranges (from LPF header offset 0x80) for palette interpolation
- [ ] Expose `hasLastDelta` flag as public property
- [ ] Add LBACK-base sprite decode mode: reset buffer to LBACK raw pixels before each
  keyframe, with RLE write mask generation to track written vs skipped pixels
- [ ] Expose palette as public property (needed for LBACK rendering)

### Subtitle Rendering — Verified from Disassembly

- [x] Font: FONTBIG.SYS (index 3), hardcoded via OpenFont(3) at ovr108_2E1A (line 445603)
- [x] No inter-character spacing — glyph width used directly (no +1 pixel gap)
- [x] Word wrap at 320px (full display width)
- [x] Line spacing = font.height (from TextPos[bx+6], ovr108_15C4, line 441390)
- [x] Bottom margin = 2px (from `inc dx; inc dx` at ovr108_15CE, lines 441401-441403)
- [x] Subtitle bar height = vpOffsetY from viewport-setup (func 20) when panorama active

### TODO — Other Cutscenes

Only CS000 (intro) and CS001 (dawn/dome) have been tested. Other cutscenes may exercise
different command combinations:

- [ ] CS002: has panorama scroll + palette interp (renders in Python extractor)
- [ ] CS004-007: have .N00 but no .N01 LPF files — stub/empty
- [ ] CS011: title animation (renders in Python extractor)
- [ ] CS012: acknowledgements (renders in Python extractor)
- [ ] CS030-036: triggered from sleep code (sleep.cs line 336)
- [ ] CS040: unknown trigger
- [ ] CS403: has alpha channel sprites (UseAlpha in cutsloader.cs)

Note: CS000 bytecode references N11 which contains unfinished art not in the final game.
The bytecode for CS000 may contain stale references from an earlier build.

### BYT.ARK Screens — Verified from Disassembly ✅

All palette indices confirmed from assembly (`LoadBitMap` calls):

| Entry | Content | Palette | Assembly Location |
|-------|---------|---------|-------------------|
| 0 | BLNKMAP | 1 | Uses game palette, no explicit set |
| 1 | CHARGEN | 3 | OpenPalsData(3) after load |
| 2 | CONV | 0 | Game palette |
| 3 | MAIN | 0 | Game palette |
| 4 | UW2 3D win | 0xFFFF→0 | `LoadBitMap(4, 0xFFFF)` ovr112_3E7, line 461928 |
| 5 | UW2 MAIN | 0xFFFF→0 | `LoadBitMap(5, 0xFFFF)` ovr147_8EB, line 523480 |
| 6 | Origin logo | 5 | `LoadBitMap(6, 5)` SplashPart1, line 461214 |
| 7 | LGS logo | 6 | `LoadBitMap(7, 6)` SplashPart1, line 461263 |
| 8 | Victory 1 | 7 | `LoadBitMap(8, 7)` RunVictorySequence, line 535059 |
| 9 | Victory 2 | 0xFFFF→0 | `LoadBitMap(9, 0xFFFF)` RunVictorySequence, line 535081 |

Entries with 0xFFFF keep current palette — palette 0 fallback is correct since
the game palette is active when these screens load.

- [x] All palette indices match PaletteIndicesUW2 in bytloader.cs
- [x] ALLPALS.DAT (512 bytes, 32 x 16-byte entries): auxiliary palette mapping for
  **sprite rendering** (cursor/object bitmaps), not BYT.ARK screens. Loaded at
  ovr119_A0F (line 473528), accessed via `shl ax, 4; add ax, AllPals_dseg_6742`
  in seg009 (line 67189). Each entry contains 16 palette indices for remapping
  sprite colours in different contexts.

## Reference Files

- `UW2/extract_intro_frames.py` — Python reference extractor with full rendering pipeline
- `UW2/IntroFramesGenerated/player.html` — HTML frame player with DOSBox comparison
- `UW2/DOSBoxCapture/frames/` — 8351 pixel-perfect reference frames at 70fps
- `UW2/RawFrames/` — All decoded LPF frames and LBACK/BYT.ARK images
- `UW2/cutscene0_uw2_commands.md` — CS000 command dump with annotations
- `UWReverseEngineering/Cutscene Engine RE Notes.md` — Full RE documentation
- `UWReverseEngineering/uw2_asm.asm` — Complete disassembly with named functions
