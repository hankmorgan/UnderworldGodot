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
    }
}
