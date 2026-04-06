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

### TODO — Panorama Scrolling

The original engine uses a **640-pixel-wide VGA offscreen buffer** for hardware scrolling
(confirmed from disassembly: SetViewportFar writes to VGA CRT registers 0x0C/0x0D).

#### LBACK Background Loading (func 22 — map-file)
- [ ] Load `CUTS/LBACK{idx:03d}.BYT` files (raw 320x200 pixel data, no header)
- [ ] Render LBACK using the current LPF file's palette
- [ ] Build panorama composite:
  - Horizontal: width = 320 + vp_start_x. First LBACK at x=0, second at x=320.
    Clip second to `vp_start_x` pixels wide.
  - Vertical: stack LBACKs sequentially (LBACK at y=0, next at y=200, etc.)
- [ ] Composite should be built when `set-start` (func 21) fires (before scroll starts)
  so that the pre-scroll segment can display it

#### Scroll Viewport (func 23 — start-scroll)
- [ ] Each frame, crop 320-pixel viewport from the composite at the current scroll position
- [ ] Horizontal: `scroll_pos = vp_start_x + dx * frame` (dx from direction table)
- [ ] Vertical: `scroll_pos = dy * frame` (start at 0, scroll down for "Up" direction)
- [ ] Panorama content at top of 200px display, black below for subtitles
- [ ] Pre-scroll segment (before start-scroll fires): display current LPF animation frames directly,
  NOT the composite — so the pre-scroll animation (cart, flags) plays correctly

#### Sprite Overlay During Scroll
- [ ] The current auto-advanced LPF file (e.g. N03, N06) provides the animated overlay
- [ ] Original engine applies LPF RLE directly to the VGA screen (which has LBACK background).
  RLE skip operations preserve screen pixels. Our approach options:
  1. **Standalone sprite** (is_sprite mode): zero buffer before each keyframe, overlay non-zero
     pixels onto the viewport. Works but doesn't handle the LBACK000 frozen cart correctly.
  2. **VGA screen simulation**: apply each frame's RLE to a buffer initialized with LBACK content.
     More accurate but requires per-frame LBACK reset.
  3. **Accumulated buffer at fixed position**: write the accumulated N02→N03 buffer into the
     composite at vp_start_x. The LPF frame overwrites the LBACK in the viewport area.
     Simpler but has cart smearing from accumulated deltas.
- [ ] The file role pattern is consistent across all scroll scenes:
  - N02/N05: pre-scroll animation (contains animated element)
  - N03/N06: sprite overlay during scroll (mostly empty, index 0 = transparent)
  - N04/N07: post-scroll clean scene (no animated element)
  - LBACK000/002: panorama background (has animated element frozen at last pre-scroll position)

#### Cart Rendering — Open Issue
- [ ] LBACK000 has the cart frozen at N02 frame-23 position (x=158-225, y=92-115)
- [ ] DOSBox pixel analysis shows N04 values at the cart position — meaning the engine masks it
- [ ] The original engine applies RLE to the VGA screen (LBACK background). Skip operations
  preserve LBACK pixels, writes draw the cart. Since the engine renders the full LPF frame
  at the fixed position (vp_start_x), it overwrites LBACK's frozen cart.
- [ ] Our accumulated buffer has N02 background pixels (different from LBACK), causing visual
  differences. The VGA screen simulation approach would fix this.
- [ ] This is a subtle issue — at 320x200 resolution the difference is barely noticeable

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
- [ ] Consider adding a mode for decoding with an external base image (for sprite overlays
  where the base is LBACK pixel data instead of the persistent buffer)

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
