# Save-game architecture

This document describes the save-game write path added in the `feat/save-game`
branch. It covers scope, the design choice made (Option B), where the code
lives, known limitations, and follow-up work.

## What ships

`SaveGame.Save(int slot, string description)` writes five files to
`{BasePath}/SAVE{slot}/`:

| File | Written for | Produced by |
|---|---|---|
| `DESC` | UW1, UW2 | Inline in `SaveGame.Save` (plain ASCII) |
| `PLAYER.DAT` | UW1, UW2 | `PlayerDatWriter.Serialize()` |
| `BGLOBALS.DAT` | UW1, UW2 | `BGlobalWriter.Serialize()` |
| `LEV.ARK` | UW1, UW2 | `LevArkWriter.Serialize()` |
| `SCD.ARK` | UW2 only | `ScdArkWriter.Serialize(folder)` |

UI entry point is the `Save Game` button in the in-game Options panel
(`uimanager_options.cs` case 0 under `MainOptionMenu`). It reuses the existing
slot-picker UI that was already wired for Restore.

## Design choice: Option B — consolidate at save-time

The DOS game did save as a **two-tier** process:

1. Live code continuously wrote partial state to `DATA/` during play —
   `bglobals.dat` on `ExitConversation`, `scd.ark` every ~20 minutes in
   `PlayerUpdates`, `lev.ark` map-notes on map-close, etc.
2. The "Save" menu then orchestrated `SaveLoadPlayerDat` + `SaveLEVARK` for the
   current level, and finally `CopySaveGameFiles` cloned the whole `DATA/`
   tree into `SAVE{n}/`.

We chose **Option B: consolidate at save-time**. `SaveGame.Save` writes all
files from the in-memory representation in one pass; there is no live
writeback to `DATA/` during gameplay. Rationale:

- The port already keeps every relevant slice of state in RAM (`pdat`,
  `bGlobals[]`, `dungeons[i].lev_ark_block.Data`, `scd.scd_data[]`).
- Mirroring the DOS two-tier flow would require scattering write hooks across
  many unrelated gameplay code paths (conversations, map UI, `PlayerUpdates`
  ticker).
- Success criterion for MVP is **round-trip through the port's own loader**,
  not cross-compatibility with DOS `UW.EXE`.

## Why writing live state is more than "encrypt and dump `pdat`"

The port keeps **two redundant copies** of the player's position at runtime:

- `playerdat.pdat` — the serialised player.dat in memory, loaded at game
  start, read by chargen, never updated during play.
- `motion.playerMotionParams` (live X/Y/Z/heading) and
  `UWTileMap.current_tilemap.LevelObjects[1]` (live player-object record) —
  updated every frame by motion, pickup, teleport, etc.

The write path therefore **stashes live state into `pdat` before encrypting**
(see `SaveGame.StashLiveStateToPdat`). This copies:

- `motion.playerMotionParams.{x_0, y_2, z_4}` → `pdat.{XCoordinate, YCoordinate, Z}`
- `motion.PlayerHeadingMinor_dseg_8294` → `pdat[0x5B]`
- `motion.playerMotionParams.tilestate25` → high 5 bits of
  `pdat.RelatedToMotionState` (swim bits kept from pdat, which motion updates
  directly)
- 27-byte `LevelObjects[1]` record → `pdat[PlayerObjectStoragePTR..]`
  (inverse of the load-time copy in `uimanager_mainmenu.JourneyOnwards`)

Without this stash, a fresh chargen's zero-initialised `pdat` fields end up
on disk, and restoring produces a player at tile (0, 0) that crashes motion
physics with `Invalid PTR in GetTileByPTR -260`.

## Loader quirks the writer mirrors

**`player.dat` file length.** The load loop in `playerdatutil.cs:Load`
starts `oIndex = 1` at `CurrentInventoryPtr = InventoryPtr`, so slot `i`
lives at `PTR = InventoryPtr + (i-1)*8`. `N` populated slots occupy `N*8`
bytes past `InventoryPtr` — file length is `InventoryPtr + N*8`, not
`InventoryPtr + (N+1)*8`. (See commit `cf2c359`.)

**`bglobals.dat` format.** Repeating `{Int16 ConversationNo; Int16 Size;
Size × Int16 Globals}` until EOF. No header, no footer, little-endian.
(`uw-formats (Abysmal).txt` §7.2.)

**`lev.ark` container (UW2).** Header is `[Int32 count][2 bytes padding]
[offsets×N][compressionFlags×N][dataLengths×N][reservedSpace×N]` then block
data at the recorded offsets. Loader reads the per-block compression flag
and dispatches — flag `0 == UW2_NOCOMPRESSION` is valid.

**`lev.ark` container (UW1).** Simpler: `[Int16 count][offsets×N]` then block
data. No per-block metadata; the caller supplies `targetDataLen`. The writer
computes each block's length as `offset[i+1] - offset[i]` (or `fileLen -
offset[i]` for the last present block) so non-tilemap blocks (overlay,
texmap, automap, notes) round-trip correctly.

**`scd.ark`.** UW2-only. 16-block UW2-format ARK container. Because the
port's SCD loader is lazy and null-sets blocks after processing, there is no
persistent source buffer — the writer takes a `sourceFolder` argument and
re-reads unmutated blocks from disk at save time.

## Known limitations

**UW2 lev.ark is written uncompressed (compression flag = 0).** The port's
`RepackUW2` compressor in `dataloader.cs:148` is a documented stub (copy-
record logic commented out as "THIS IS WRONG"). Writing uncompressed
sidesteps it. Files are roughly 3.8× larger than DOS originals
(~1.4 MB vs ~367 KB for a saved level). **DOS `UW.EXE` cannot load these
files.** Cross-compat is a follow-up; finish `RepackUW2` first.

**No atomic write.** If `File.WriteAllBytes` fails mid-sequence (disk full,
permission loss), `SAVE{n}/` is left in a partial state that the loader may
crash on. Mitigation: write all files to `SAVE{n}.tmp/` then
`Directory.Move` on success. Not implemented.

**Automap visited-tile state and map-notes pass through from the source
ARK.** If the player walked new tiles since last level-load, those visited
tiles are not yet saved back to `lev.ark` automap block. UW1 same issue.
Requires serialising `automap.automaps[i]` and `automapnote.automapsnotes[i]`
back into ARK blocks `i+27`/`i+36` (UW1) or `i+160`/`i+240` (UW2).

**UI polish.** Description fallback is `"Save {slot}"`; no text-input prompt
for custom descriptions. Existing slot DESC is reused on overwrite. Save-menu
dispatches the first click with no confirm-overwrite.

**No gravestone trick.** The DOS save path pushed the player into the
current tile's object list as `item_id = 63` (gravestone) at save time, then
reloaded `player.dat` to restore live state. We skip this because the port
takes player position from `player.dat` at restore, not from a gravestone in
the level. Two follow-ons flow from this:

- A restore inserts `LevelObjects[1]` (player) into the saved tile's object
  list via `PlacePlayerInTile`. The player's own adventurer sprite renders
  at their position unless filtered — fixed by early-return in
  `ObjectCreator.RenderObject` when `obj.index == 1` (commit `3d46427`).
- Motion also inserts the player into every new tile during walking
  (`motion_player.cs:282`). Saved state preserves that, so the restore's
  `PlacePlayerInTile` would double-insert and create a `LevelObjects[1].next
  = 1` self-loop on slot 1 — fix is an idempotent head-check
  (commit `e17a022`).

## Testing

`tests/Underworld.Save.Tests/` (xUnit, shares `[Collection("UWClassState")]`
for serialised static-state mutation):

- **BGlobalRoundTripTests**: real UW2/SAVE0 fixture round-trip + empty/one-slot
  boundary cases.
- **PlayerDatRoundTripTests**: UW1 and UW2 `InitEmptyPlayer` → serialise →
  decrypt → byte-compare in populated region.
- **LevArkRoundTripTests**: per-block extraction round-trip, full-container
  round-trip (UW1 + UW2), and a visited-level test that seeds `dungeons[0]`
  with a known byte to verify the live-state path.
- **ScdArkRoundTripTests**: UW2/SAVE0 real-fixture round-trip.
- **SaveGameOrchestratorTests**: all-five-files UW2 happy path, UW1
  SCD.ARK-absent case, invalid-slot throws, DESC byte-content checks.

23 tests total. Manual playthrough on UW1 and UW2 validated at the end: new
game → move → drop item → save → quit → restore → state preserved.

## Files

```
src/savegame/
├── SaveGame.cs         — public orchestrator (Save + StashLiveStateToPdat)
├── BGlobalWriter.cs    — bglobals.dat format writer
├── PlayerDatWriter.cs  — player.dat format writer (UW1/UW2)
├── LevArkWriter.cs     — lev.ark container writer (UW1/UW2)
└── ScdArkWriter.cs     — scd.ark container writer (UW2)

src/ui/uimanager_options.cs          — wired Save button (case 0), try/catch
src/player/playerdatobject.cs        — idempotent PlacePlayerInTile fix
src/utility/ObjectCreator.cs         — skip rendering LevelObjects[1]
src/ui/uimanager_flasks.cs           — null guards for test-context callers

tests/Underworld.Save.Tests/         — 23 tests
docs/superpowers/plans/2026-04-19-save-game.md   — original plan
docs/save-architecture.md            — this document
```
