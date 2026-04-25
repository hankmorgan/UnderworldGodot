using System;
using System.IO;
using System.Text;

namespace Underworld
{
    /// <summary>
    /// Top-level save-game orchestrator. Writes all five save files to {BasePath}/SAVE{slot}/.
    /// UW1 saves omit SCD.ARK (UW1 has no script compiler data file).
    /// </summary>
    public static class SaveGame
    {
        public static void Save(int slot, string description)
        {
            if (slot < 1 || slot > 4)
                throw new ArgumentOutOfRangeException(nameof(slot), slot, "slot must be 1..4");

            string saveDir = Path.Combine(UWClass.BasePath, $"SAVE{slot}");
            Directory.CreateDirectory(saveDir);

            // Detach the player from its tile's object chain FIRST so the
            // pdat stash (which copies LevelObjects[1] bytes into pdat) sees
            // a clean .next = 0 player rather than the chain-inserted
            // next = <previous head> that PlacePlayerInTile produced during
            // play. DOS expects the player object to have next = 0.
            int savedChainHead = DetachPlayerFromCurrentTile();
            StashLiveStateToPdat();
            ApplySlot1Markers();

            // DOS UW.EXE never writes the player (slot 1) into a tile's
            // indexObjectList — it tracks player position separately via the
            // mouse/motion path. The port's PlacePlayerInTile intentionally
            // inserts slot 1 at the chain head for in-game collision, and
            // without this detach step the serialised LEV.ARK contains a
            // tile whose first-object-in-chain points at slot 1, which DOS
            // treats as an invalid object list ("problems in object list"
            // error, render bails out with wall/floor/ceiling textures
            // missing).
            try
            {
                WriteAllSaveFiles(saveDir, description);
            }
            finally
            {
                // Re-insert slot 1 so in-memory game state stays consistent
                // if the user keeps playing after saving.
                ReattachPlayerToCurrentTile(savedChainHead);
            }
        }

        private static void WriteAllSaveFiles(string saveDir, string description)
        {
            // DESC in UW1 DOS saves is a single byte — seemingly an in-use/slot
            // indicator rather than a textual description. Writing a multi-byte
            // ASCII description leaves trailing junk that DOS interprets
            // inconsistently when enumerating save slots.
            //
            // We pack the first character of `description` into the single
            // byte when possible; the UI doesn't actually surface the string
            // in the DOS load picker anyway (it shows "Save 1", "Save 2" etc.
            // based on slot number).
            byte descByte = 0x01;
            if (!string.IsNullOrEmpty(description))
            {
                byte firstChar = Encoding.ASCII.GetBytes(description.Substring(0, 1))[0];
                if (firstChar != 0) descByte = firstChar;
            }
            File.WriteAllBytes(Path.Combine(saveDir, "DESC"), new[] { descByte });
            File.WriteAllBytes(Path.Combine(saveDir, "PLAYER.DAT"), PatchPlayerLinkInSerialised(PlayerDatWriter.Serialize()));
            File.WriteAllBytes(Path.Combine(saveDir, "BGLOBALS.DAT"), BGlobalWriter.Serialize(bglobal.bGlobals));
            File.WriteAllBytes(Path.Combine(saveDir, "LEV.ARK"), LevArkWriter.Serialize());

            if (UWClass._RES == UWClass.GAME_UW2)
            {
                string sourceFolder = playerdat.currentfolder;
                File.WriteAllBytes(Path.Combine(saveDir, "SCD.ARK"), ScdArkWriter.Serialize(sourceFolder));
            }
        }

        /// <summary>
        /// Unlink slot 1 (player) from its current tile's object chain and
        /// return the chain head as it was before the detach, so the caller
        /// can restore it. The tile head is rewritten to slot 1's .next,
        /// and slot 1's .next is cleared so the serialised slot looks like
        /// a DOS-format untethered player slot.
        /// </summary>
        private static int DetachPlayerFromCurrentTile()
        {
            if (UWTileMap.current_tilemap == null ||
                UWTileMap.current_tilemap.LevelObjects == null ||
                UWTileMap.current_tilemap.LevelObjects[1] == null)
            {
                return 0;
            }
            int tx = motion.playerMotionParams.x_0 >> 8;
            int ty = motion.playerMotionParams.y_2 >> 8;
            if (!UWTileMap.ValidTile(tx, ty)) return 0;
            var tile = UWTileMap.current_tilemap.Tiles[tx, ty];
            int prevHead = tile.indexObjectList;
            ObjectRemover_OLD.RemoveObjectFromLinkedList(
                tile.indexObjectList, 1,
                UWTileMap.current_tilemap.LevelObjects,
                tile.Ptr + 2);
            // Clear slot 1's next so a freshly loaded save has a clean player
            // slot with no dangling chain pointer.
            UWTileMap.current_tilemap.LevelObjects[1].next = 0;
            return prevHead;
        }

        /// <summary>
        /// Apply the DOS-format slot-1 player markers AFTER the pdat stash so
        /// the byte-0 mask doesn't propagate into pdat (DOS keeps
        /// pdat[0xD5]=0x7F = avatar item_id while masking LEV.ARK slot1[0] to
        /// 0x3F = invisible placeholder; cross-file mismatch on byte 0 is
        /// expected, but bytes 1..26 must match between the two stores).
        ///
        /// Markers DOS writes for the player avatar:
        ///   - LEV.ARK slot1 byte 0: item_id bit 6 cleared (127 → 63)
        ///   - LEV.ARK slot1 byte 1: doordir flag set (bit 5 of high byte)
        ///   - LEV.ARK slot1 byte 26 (npc_whoami): 0xFD sentinel
        ///   - pdat 0xD5+1, 0xD5+26: same as LEV.ARK (markers go in both)
        ///   - pdat 0xD5+0: untouched (stays 0x7F = avatar)
        ///
        /// Without these, DOS UW.EXE renders the Avatar NPC mesh at the
        /// player's own world position — the camera ends up inside the mesh,
        /// producing the "wall/floor/ceiling textures missing" symptom.
        /// </summary>
        private static void ApplySlot1Markers()
        {
            if (UWTileMap.current_tilemap == null ||
                UWTileMap.current_tilemap.LevelObjects == null ||
                UWTileMap.current_tilemap.LevelObjects[1] == null)
            {
                return;
            }
            var pobj = UWTileMap.current_tilemap.LevelObjects[1];
            int p0 = pobj.PTR;
            byte[] buf = pobj.DataBuffer;
            buf[p0]      = (byte)(buf[p0] & 0x3F);
            buf[p0 + 1]  = (byte)(buf[p0 + 1] | 0x20);
            buf[p0 + 26] = 0xFD;

            int pp = playerdat.PlayerObjectStoragePTR;
            // pdat byte 0 (item_id) stays as the avatar (0x7F); only mirror
            // the marker bits at byte 1 and the whoami sentinel at byte 26.
            playerdat.pdat[pp + 1]  = (byte)(playerdat.pdat[pp + 1] | 0x20);
            playerdat.pdat[pp + 26] = 0xFD;
        }

        /// <summary>
        /// DOS UW.EXE stores the avatar's pdat `link` field = 1 — a self-
        /// reference that DOS reads as "this character HAS inventory" when
        /// populating paperdoll/backpack on Journey Onward. The port's load
        /// code at uimanager_mainmenu.cs:283 forces LevelObjects[1].link = 0
        /// to prevent in-memory chain cycles, and that 0 propagates back to
        /// pdat[0xDB-0xDC] on save → DOS renders an empty inventory.
        ///
        /// We can't write link=1 to in-memory pdat permanently because the
        /// port's own loader would re-read it and hit the cycle on the next
        /// Journey Onward. So serialise PLAYER.DAT once, patch the link
        /// bytes in the SERIALISED ciphertext only, write that to disk, and
        /// leave in-memory pdat untouched.
        ///
        /// pdat post-encryption layout for UW1: bytes 0..0xD2 are XOR-
        /// encrypted with a position-dependent key; pdat[0xD3+] is plain.
        /// PlayerObjectStoragePTR = 0xD5, so bytes 6-7 of the player obj
        /// copy live at file offsets 0xDB and 0xDC — both in the plaintext
        /// region. We can write to them directly without touching the seed.
        /// </summary>
        private static byte[] PatchPlayerLinkInSerialised(byte[] serialised)
        {
            int linkOff = playerdat.PlayerObjectStoragePTR + 6;
            if (linkOff + 1 >= serialised.Length) return serialised;
            // Set link bits 6-15 = 1, preserve owner bits 0-5 of byte 6.
            serialised[linkOff]     = (byte)((serialised[linkOff] & 0x3F) | 0x40);
            serialised[linkOff + 1] = 0;
            return serialised;
        }

        private static void ReattachPlayerToCurrentTile(int _)
        {
            if (UWTileMap.current_tilemap == null ||
                UWTileMap.current_tilemap.LevelObjects == null ||
                UWTileMap.current_tilemap.LevelObjects[1] == null)
            {
                return;
            }
            int tx = motion.playerMotionParams.x_0 >> 8;
            int ty = motion.playerMotionParams.y_2 >> 8;
            if (!UWTileMap.ValidTile(tx, ty)) return;
            var tile = UWTileMap.current_tilemap.Tiles[tx, ty];
            // PlacePlayerInTile's head-insert logic: obj.next = current head, head = 1.
            UWTileMap.current_tilemap.LevelObjects[1].next = (short)tile.indexObjectList;
            tile.indexObjectList = 1;
        }

        /// <summary>
        /// Copies live runtime state back into pdat so the serialised player.dat reflects
        /// the current game state. The port keeps player position in motion.playerMotionParams
        /// and player-object fields in UWTileMap.current_tilemap.LevelObjects[1]; these are
        /// never written back to pdat during play, so without this stash the serialised
        /// player.dat preserves load-time values and restoring yields a stale/zero position.
        /// </summary>
        private static void StashLiveStateToPdat()
        {
            playerdat.XCoordinate = motion.playerMotionParams.x_0;
            playerdat.YCoordinate = motion.playerMotionParams.y_2;
            playerdat.Z = motion.playerMotionParams.z_4;
            playerdat.SetAt16(0x5B, motion.PlayerHeadingMinor_dseg_8294);

            // RelatedToMotionState: high 5 bits = tilestate25, low 3 bits = swim state.
            // Swim state is already written to pdat by motion updates; refresh tilestate only.
            int swimBits = playerdat.RelatedToMotionState & 0x7;
            playerdat.RelatedToMotionState = (motion.playerMotionParams.tilestate25 << 3) | swimBits;

            // Copy the 27-byte player object (general + NPC extra) from the live LevelObjects[1]
            // back into pdat[PlayerObjectStoragePTR..]. This is the inverse of the load-time
            // copy at uimanager_mainmenu.cs:275-278.
            if (UWTileMap.current_tilemap != null && UWTileMap.current_tilemap.LevelObjects[1] != null)
            {
                var playerObj = UWTileMap.current_tilemap.LevelObjects[1];
                for (int i = 0; i <= 0x1A; i++)
                {
                    playerdat.pdat[playerdat.PlayerObjectStoragePTR + i] = playerObj.DataBuffer[playerObj.PTR + i];
                }
            }
        }
    }
}
