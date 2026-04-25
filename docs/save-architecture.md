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

I chose **Option B: consolidate at save-time**. `SaveGame.Save` writes all
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

**UW2 save is UI-gated pending upstream compressor.** The `SaveGame.Save`
orchestrator and all UW2 writers are still present and tested — they produce
files that round-trip through the port's own loader — but the Save button
in the UW2 Options menu refuses to invoke them. Rationale: the port's
`RepackUW2` is a stub, and writing uncompressed UW2 blocks (which load
fine in the port) causes DOS `UW.EXE` to crash on any save with >80
uncompressed blocks. Once the upstream compressor lands, remove the gate.

**UW1 save is DOS-compatible in format.** UW1 `lev.ark` is uncompressed by
spec, so the compression issue doesn't apply. DOS round-trip is verified
end-to-end via dos-mcp / DOSBox — see "UW1 DOS round-trip" section below
for the four byte-level fixes that were required.

**No atomic write.** If `File.WriteAllBytes` fails mid-sequence (disk full,
permission loss), `SAVE{n}/` is left in a partial state that the loader may
crash on. Mitigation: write all files to `SAVE{n}.tmp/` then
`Directory.Move` on success. Not implemented.

**Automap visited-tile state passes through from source ARK.** Map notes
ARE now persisted (see `LevArkWriter` automap-notes reconstruction), but
visited-tile shading is not: requires serialising `automap.automaps[i]`
back into ARK blocks `i+27` (UW1) or `i+160` (UW2). Noted as follow-up.

**UI polish.** Description fallback is `"Save {slot}"`; no text-input prompt
for custom descriptions. Existing slot DESC is reused on overwrite. Save-menu
dispatches the first click with no confirm-overwrite.

**No gravestone trick.** The DOS save path pushed the player into the
current tile's object list as `item_id = 63` (gravestone) at save time, then
reloaded `player.dat` to restore live state. I skip this because the port
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

## UW1 DOS round-trip

A port save loaded by genuine DOS `UW.EXE` (under DOSBox or dos-mcp's
js-dos backend) needs **six** byte-level adjustments beyond what the
in-port loader requires. All six are implemented in `SaveGame.Save`,
`PlayerDatWriter`, and `playerdatainit.InitEmptyPlayer`; the section
below documents them so future field changes don't accidentally regress.

### 1. Chargen spawn tile (32, 2), not (32, 1)

`uimanager_mainmenu.JourneyOnwards` hardcoded the UW1 starting tile
to `(32, 1)`, one tile north of where DOS chargen spawns the Avatar.
The port itself rendered fine (player faces an open corridor by
chance), but DOS-loaded port saves placed the camera against the
solid wall at `(32, 0)` — the entire viewport was occluded by the
wall face. Fixed by changing the call to
`Teleportation.InitialisePlayerOnLevelOrPositionChange(32, 2)`.

### 2. Detach the player from the tile object chain at save-time

`PlacePlayerInTile` inserts `LevelObjects[1]` at the head of the
current tile's `indexObjectList` so the in-game collision pipeline
can find the player. DOS UW.EXE never writes the player into a
tile's object chain — slot 1 is reachable only through the
`pdat[PlayerObjectStoragePTR..]` storage path. When DOS loads a
save where slot 1 IS in a tile chain, it prints "Underworld
internal error - Problems in object list" and bails out of the
3D render with walls/floor/ceiling all flat.

`SaveGame.DetachPlayerFromCurrentTile` walks the chain at the
player's current tile, unlinks slot 1 (or returns clean if the
player isn't in the chain — shouldn't happen but defensive), and
clears `LevelObjects[1].next`. A `finally`-guarded
`ReattachPlayerToCurrentTile` re-inserts after the file write so
in-memory state stays consistent if the user keeps playing.

Order is critical: detach BEFORE `StashLiveStateToPdat`, otherwise
the stash captures `next = <previous chain head>` into the pdat
copy of the player object. DOS validates pdat-vs-LEV-slot-1
consistency for bytes 1..26; mismatch yields "Bad save file".

### 3. Slot-1 marker bytes for the player avatar

DOS UW.EXE writes specific marker bytes at LEV.ARK slot 1 to
distinguish "this slot represents the player camera, do not render
an NPC sprite at this world position":

- `slot1[0]`: `item_id` bit 6 cleared. Avatar's real `item_id` 127
  (`0x7F`) is masked to 63 (`0x3F`), an invisible-placeholder NPC
  type that UW's renderer skips.
- `slot1[1]` bit 5: doordir flag set. Repurposed on the player
  slot as a "this is the player" signal.
- `slot1[26]` (npc_whoami): `0xFD` sentinel. Without this, UW's
  NPC sprite-draw loop renders the Avatar mesh at the player's
  own position; the camera ends up inside the mesh, producing
  the same flat-untextured appearance as a wall occlusion.

`pdat[0xD5+i]` (the player-object copy) mirrors slot 1 bytes 1..26
exactly. Byte 0 is the only allowed cross-file divergence:
`pdat[0xD5]` stays `0x7F` (avatar) so in-game inventory / paperdoll
code keeps the right `item_id` reference; `slot1[0]` is masked
only in LEV.ARK.

`SaveGame.ApplySlot1Markers` runs after `StashLiveStateToPdat` so
the byte-0 mask is applied to LEV.ARK only and never propagates
into pdat.

### 4. UW1 Detail Level (pdat[0xB6] bits 4-5)

`pdat[0xB6]` bits 4-5 encode the in-game **Detail Level** option:
0 = Low, 1 = Medium, 2 = High, 3 = Very High. DOS chargen defaults
this to 3; without it, DOS-loaded saves render with walls (and/or
floor/ceiling) untextured. Specifically:

- `0b00`: walls + floor + ceiling all flat
- `0b01`: walls textured, floor + ceiling flat
- `0b10`: walls + floor textured, ceiling flat
- `0b11`: all surfaces textured (Very High)

Port chargen never wrote these bits, leaving every fresh save at
detail level 0. The `playerdat.DetailLevel` accessor reads/writes
the field, and `InitEmptyPlayer` sets `DetailLevel = 3` for UW1.
UW2 storage offset is TBD; the accessor short-circuits to 3 for
UW2 to avoid touching unknown bytes.

### 5. pdat[0xD3] = 0x08 (UW1 chargen, semantics TBD)

DOS UW.EXE chargen writes `0x08` at `pdat[0xD3]` and the Journey-
Onward path validates it. Port chargen left this byte at `0`; on
DOS load, that 0 sends UW.EXE into a non-returning loop somewhere
in the load sequence (player UI never appears, game hangs after
"You enter the Abyss" splash).

Setting `pdat[0xD3] = 0x08` in `InitEmptyPlayer` (UW1 branch only)
unblocks the load. Empirically pinned by single-byte bisection
between matched fresh-chargen+bag-pickup port and DOS saves.

**Semantics: TBD.** The byte sits between `pdat[0xCE-0xD1]` (Int32
game time per uw-formats Abysmal — port treats it as 0xCF-0xD2,
1-byte off from the docs) and `pdat[0xDC]` (current vitality), in
a 10-byte region that neither uw-formats project documents. The
exported `UW1_asm.asm` has no symbol-named annotation for this
offset; finding the read site needs UW.idb opened in IDA Pro to
dereference `pdat_base + 0xD3`. Filed as a follow-up RE task.

### 6. Avatar self-link `pdat[0xDB-0xDC] = 0x0040` (link = 1)

The player object copy at `pdat[0xD5..0xEF]` includes a `link`
field at byte 6 (= `pdat[0xDB-0xDC]`). DOS chargen writes `link = 1`
— a self-reference (slot 1 IS the player avatar in the mobile
object list per uw-formats §4.3 "object 1 is always the avatar").
DOS Journey-Onward reads this as the inventory-presence flag when
populating the paperdoll/backpack UI; `link = 0` renders the
inventory as empty.

The port's load code at `uimanager_mainmenu.cs:283` explicitly
forces `LevelObjects[1].link = 0` after copying pdat → LevelObjects
to break what the comment calls an "infinite loop" — and that 0
propagates back into pdat on the next save via `StashLiveStateToPdat`.

We can't write `link = 1` to in-memory pdat permanently (port's
own loader would re-read it and hit the cycle). Instead,
`SaveGame.PatchPlayerLinkInSerialised` patches the **serialised
PLAYER.DAT bytes only** after the writer returns and before the
file write. In-memory pdat stays at 0; on-disk pdat has `link = 1`.

**Semantics: TBD.** "What does `link = 1` actually flag?" — same
RE follow-up as §5.

### Diagnostic workflow

The four issues above were each found by byte-diffing a
fresh-chargen port save against a fresh-chargen DOS save — `cmp -l`
on the raw files for LEV.ARK, plus the symmetric `EncryptDecryptUW1`
routine for PLAYER.DAT comparison. Tools:

- **dos-mcp** (`abedegno/dos-mcp`) drives DOSBox in a headless
  Chromium tab, with `fs_push_dir` / `fs_pull_dir` for moving
  saves between host and the VFS. UW.EXE scans the save dirs at
  startup, so pushed files only register if they were in the
  host source dir at `load_bundle` time — populate the host save
  dir before reloading the bundle.
- A small Python helper in `tests/scripts/decrypt_pdat.py`
  (TODO: extract from the inline scripts used during diagnosis)
  decrypts UW1 PLAYER.DAT for byte-level comparison.

### Open follow-ups

- **Identify pdat[0xD3] and pdat[0xDB] semantics from disassembly.**
  Both fields are documented above only by their empirical effect
  on DOS Journey-Onward. Open `UWReverseEngineering/UW.idb` in IDA
  Pro, find the function that reads each offset during the load
  sequence, and update the doc with the field's actual meaning.
  The exported `UW1_asm.asm` doesn't preserve enough symbol
  context for grep-style searches against the raw text.
- **DOS → port round-trip** (DOS chargen + play, then port load)
  has not been tested under the new fixes.
- **Description text input.** UI still uses `"Save {slot}"` as
  the description. DESC writes only the first ASCII byte
  regardless, so this is cosmetic.

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
