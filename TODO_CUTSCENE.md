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
- [ ] Fine pixel panning: original engine uses VGA register 0x3C0 index 0x33 for
  sub-pixel horizontal scroll. Our implementation rounds to whole pixels.
- [ ] Verify sprite alignment on all scroll scenes (CS000 horizontal/vertical tested)

### TODO — Palette Interpolation (func 19)

The engine interpolates the LPF's embedded palette toward a target palette from PALS.DAT.
This creates effects like sunset-to-night transitions and is NOT palette rotation.

- [ ] Load target palette from `PaletteLoader.Palettes[targetIndex]`
- [ ] Each step: move each color channel by 4 (VGA DAC granularity: 6-bit mapped to 8-bit)
- [ ] Only interpolate palette entries within the LPF's color cycling ranges
  (from the 128-byte block at LPF offset 0x80, 16 entries of 8 bytes, bytes 6-7 = low/high)
- [ ] Speed parameter: interpolate every N frames
- [ ] Need to re-render the current frame with the updated palette each step
- [ ] Color cycling ranges need to be exposed from CutsLoader (currently parsed but not stored)

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

- [ ] CS002: Guardian's appearance (triggered in-game, has panorama scroll)
- [ ] CS004-007: Stub cutscenes (have .N00 but no .N01 — empty?)
- [ ] CS011: Title screen animation ("Ultima Underworld II / Labyrinth of Worlds")
- [ ] CS012: Credits/acknowledgements
- [ ] CS030-036: Dream/vision sequences (from sleep?)
- [ ] CS040: Unknown
- [ ] CS403: Death animation (has alpha channel sprites)

Note: CS000 bytecode references N11 which contains unfinished art not in the final game.
The bytecode for CS000 may contain stale references from an earlier build.

### TODO — BYT.ARK Screens

- [ ] Entry 0 (BLNKMAP): palette 1 — uses current game palette, no explicit set in assembly
- [ ] Entry 1 (CHARGEN): palette 3 — assembly confirms OpenPalsData(3) called after load
- [ ] Entries 4, 5, 9: assembly passes 0xFFFF (keep current palette).
  The Godot code uses palette 0 as fallback — may need dynamic palette selection.
- [ ] ALLPALS.DAT (513 bytes, 32 x 16-byte entries): auxiliary palette mapping data used by
  the rendering pipeline. Purpose not fully understood — each entry appears to contain
  a list of palette indices for remapping. Accessed via `shl ax, 4; add ax, AllPals_dseg_6742`.

## Reference Files

- `UW2/extract_intro_frames.py` — Python reference extractor with full rendering pipeline
- `UW2/IntroFramesGenerated/player.html` — HTML frame player with DOSBox comparison
- `UW2/DOSBoxCapture/frames/` — 8351 pixel-perfect reference frames at 70fps
- `UW2/RawFrames/` — All decoded LPF frames and LBACK/BYT.ARK images
- `UW2/cutscene0_uw2_commands.md` — CS000 command dump with annotations
- `UWReverseEngineering/Cutscene Engine RE Notes.md` — Full RE documentation
- `UWReverseEngineering/uw2_asm.asm` — Complete disassembly with named functions
