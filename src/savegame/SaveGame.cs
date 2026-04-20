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

            StashLiveStateToPdat();

            File.WriteAllBytes(Path.Combine(saveDir, "DESC"), Encoding.ASCII.GetBytes(description ?? "Quick save"));
            File.WriteAllBytes(Path.Combine(saveDir, "PLAYER.DAT"), PlayerDatWriter.Serialize());
            File.WriteAllBytes(Path.Combine(saveDir, "BGLOBALS.DAT"), BGlobalWriter.Serialize(bglobal.bGlobals));
            File.WriteAllBytes(Path.Combine(saveDir, "LEV.ARK"), LevArkWriter.Serialize());

            if (UWClass._RES == UWClass.GAME_UW2)
            {
                string sourceFolder = playerdat.currentfolder;
                File.WriteAllBytes(Path.Combine(saveDir, "SCD.ARK"), ScdArkWriter.Serialize(sourceFolder));
            }
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
