# Fonts

The game builds its four UI fonts at run time from the player's own `.SYS` files instead of
shipping converted TTFs.

## Why

The converted TTFs took each glyph's advance from its bitmap ink rather than from the width
byte the format declares, and six glyphs were clipped where their ink exceeded their own
header's `maxwidth` (issue #72). Both are properties of the conversion, not of the game data,
so the fix is to stop converting.

## The pieces

| file | responsibility |
| --- | --- |
| `src/loaders/sysfontloader.cs` | parses `.SYS`. No Godot types, so the headless suite covers it. |
| `src/loaders/sysfontbuilder.cs` | fills a Godot `FontFile` from a parsed font. |
| `src/loaders/sysfontprovider.cs` | loads all four as a set, or fails whole. Owns the metrics. |
| `src/loaders/installationvalidator.cs` | one place that decides whether an install is usable. |
| `src/ui/UnderworldRoot.cs` | publishes the fonts in `_EnterTree`. |

`scenes/Underworld.tscn` holds four empty placeholder `FontFile` resources and all 51 theme
overrides point at those instances, so filling them fills every label without touching an
override.

## Why `_EnterTree`

Godot enters the tree parent-first and readies child-first, so the root's `_EnterTree` runs
before any descendant's `_EnterTree`, theme notification, `_Ready`, deferred call or draw.
`_Ready` would be far too late.

That is necessary but was not obviously sufficient: Godot instantiates the whole `PackedScene`
before adding the root, so a Control could in principle shape its text against an empty font
and keep stale metrics. `FontBootTest` exists to answer that. It measures two real labels in
the real scene against the widths their declared width bytes predict, and it has a negative
control that withholds the fonts and requires the same assertions to fail.

## The compatibility contract

`SysFontProvider.Metrics` holds an ascent and descent per font. **These are not derived from
the game data and cannot be.** The `.SYS` header is six `Int16`s — unknown, charsize,
blankwidth, height, rowbytes, maxwidth — and carries no baseline at all. The values were
measured from the TTFs being replaced, because every node position in a 3200-line scene was
hand-tuned against them.

Deriving them from glyph height instead was tried and shipped a visible regression: FONT5X6I's
line box went from 47 screen pixels to 28 at font size 64, moving every baseline 19 pixels and
making multi-line labels drift.

`FixedSize` is 16 because the TTFs are 1024 units per em at 64 units per source pixel, so font
size S renders S/16 source pixels.

Correcting the line box to what a bitmap font would actually want is separate work. It needs a
DOS baseline the `.SYS` files cannot supply, and it would re-lay-out both games.

## Two settings that are load-bearing

`gui/fonts/dynamic_fonts/use_oversampling=false`. With `canvas_items` stretch, Godot
recalculates font oversampling when the window-to-content scale changes and calls TextServer's
global oversampling setter, which invalidates font caches without exempting a manually
populated bitmap `FontFile`. Measured: resizing to a non-integer scale took the glyph cache
from 127 glyphs to 0 and every character became a missing-glyph box. There is no repair path,
only prevention: repopulating afterwards restores the glyphs but not the rendering, because
the shaped text stays cached. Oversampling exists to re-rasterise vector fonts, so a bitmap
font loses nothing.

`theme_override_constants/line_separation` on six multi-line labels was tuned against the
TTFs. The runtime fonts are one pixel taller per line at font size 64, so those constants moved
by one to keep the same pitch. Without it the character panel lost a row off the bottom.

## Known residuals

- **`EndGameStats` renders at font size 40**, which is 2.5 times the source pixel grid. A
  scaled bitmap places the same glyph differently depending on where it lands, which is why
  `PositionLabel` moved from 47 to 48. It is also multi-line at run time with no
  `line_separation` compensation. Choosing between 48 and 32 needs a rendered end-game screen
  with representative content, so it is deliberately left alone.
- **`PositionLabel` is multi-line at size 48 with no compensation**, so it drifts about a pixel
  per line. It is the debug position readout and `EnablePositionDebug` is false.
- **`MapNo` and `CutsSubtitle`** are single-line at size 48, so they sit about a pixel lower
  than they did. `line_separation` does not apply to a single line.
- The four converted `.ttf` files are still committed and referenced by nothing. They are kept
  deliberately as a fallback until the runtime path has seen real use.

## Testing

The headless suite covers the parser, the provider and the validator. Anything needing the
engine lives in `tests/godot` and prints `GODOT-TEST-VERDICT PASS`, `FAIL` or `SKIP`; the exit
status is not trusted, because Godot 4.3 mono can fault on shutdown after `Quit()` and hang.
Three of the scenes need the shipped `.SYS` files and report `SKIP` without them.

`FontPlacementTest` deserves a note. It pins where each font's ink sits relative to the
baseline, measured from the TTFs. Nothing else did, and the consequence was that FONTBIG
rendered three source pixels too high across every save slot label, the chargen screens,
cutscene subtitles and the automap level number, with every test green: `FontRenderTest`
derived its sample origin from the same expression as the builder, so its oracle moved with
the bug.
