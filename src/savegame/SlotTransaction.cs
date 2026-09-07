using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Underworld
{
    /// <summary>
    /// What a slot must contain before it is allowed to replace the one already there.
    /// The caller supplies this because only it knows which files the game writes:
    /// SCD.ARK is required for UW2 and never written for UW1.
    /// </summary>
    public sealed class SlotRequirements
    {
        /// <summary>Files that must exist, whatever their length. DESC is one: DOS writes a
        /// zero-length DESC for an empty description and the slot is still occupied.</summary>
        public string[] MustExist = Array.Empty<string>();

        /// <summary>Files that must exist and hold something. An empty archive is a failed
        /// write, not a save: ScdArkWriter returns an empty array when its source is missing,
        /// which would otherwise be committed as a complete UW2 save.</summary>
        public string[] MustHaveContent = Array.Empty<string>();
    }

    /// <summary>
    /// Replaces a save slot as a unit, so a failure leaves either the whole previous save or
    /// the whole new one, never a mixture of the two.
    ///
    /// Issue #74. SaveGame.Save used to write DESC, PLAYER.DAT, BGLOBALS.DAT and LEV.ARK
    /// straight into the live slot, so anything going wrong part way left old and new files
    /// together. DESC is written first and the two slot listers key off it, while restore keys
    /// off LEV.ARK, so the usual result was a slot listed with a name that refused to load.
    ///
    /// Real DOS behaves the same way, measured by driving UW.EXE: saving over an occupied slot
    /// overwrites the files it writes and leaves everything else alone. So this is a deliberate
    /// improvement on the original rather than emulation of it, and the foreign files DOS
    /// leaves behind are carried across so that much still matches.
    ///
    /// Godot-free, and takes its base path rather than reading UWClass.BasePath, so it can be
    /// tested against a temporary directory.
    /// </summary>
    public static class SlotTransaction
    {
        /// <summary>
        /// Names this class owns, all beside the slot directory. The journal is a fixed name so
        /// recovery can find it; the other two carry the attempt's id so they cannot collide
        /// with anything this class did not create.
        /// </summary>
        private static string JournalPath(string basePath, int slot) =>
            Path.Combine(basePath, $"SAVE{slot}.txn");
        private static string SlotPath(string basePath, int slot) =>
            Path.Combine(basePath, $"SAVE{slot}");
        private static string StagingPath(string basePath, int slot, string id) =>
            Path.Combine(basePath, $"SAVE{slot}.tmp-{id}");
        private static string BackupPath(string basePath, int slot, string id) =>
            Path.Combine(basePath, $"SAVE{slot}.old-{id}");

        /// <summary>An id is 32 lower-case hex characters and nothing else. Recovery derives
        /// path names from it, so a journal that does not carry one is ignored rather than
        /// acted on.</summary>
        private static bool IsWellFormedId(string id)
        {
            if (id == null || id.Length != 32) { return false; }
            foreach (char c in id)
            {
                bool ok = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f');
                if (!ok) { return false; }
            }
            return true;
        }

        private sealed class Journal
        {
            public int Slot;
            public string Id;
            public bool Staged;
        }

        /// <summary>
        /// The journal records only the slot, the attempt's id, and whether the staging
        /// directory passed validation. It deliberately does not record paths: recovery derives
        /// them, so a journal can only ever name its own two siblings.
        /// </summary>
        private static void WriteJournal(string basePath, int slot, string id, bool staged)
        {
            string body = $"slot={slot}\nid={id}\nstaged={(staged ? 1 : 0)}\n";
            string final = JournalPath(basePath, slot);
            // The scratch name carries the id like everything else this class creates, so
            // publishing a journal cannot truncate a file somebody else left at a fixed name.
            string scratch = Path.Combine(basePath, $"SAVE{slot}.txn-{id}.writing");
            File.WriteAllBytes(scratch, Encoding.ASCII.GetBytes(body));
            File.Move(scratch, final, overwrite: true);
        }

        private static Journal ReadJournal(string basePath, int slot)
        {
            string path = JournalPath(basePath, slot);
            string text;
            try
            {
                if (!File.Exists(path)) { return null; }
                text = File.ReadAllText(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                return null;
            }

            var j = new Journal { Slot = -1, Id = null, Staged = false };
            foreach (string line in text.Split('\n'))
            {
                int eq = line.IndexOf('=');
                if (eq <= 0) { continue; }
                string k = line.Substring(0, eq).Trim();
                string v = line.Substring(eq + 1).Trim();
                if (k == "slot" && int.TryParse(v, out int n)) { j.Slot = n; }
                else if (k == "id") { j.Id = v; }
                else if (k == "staged") { j.Staged = v == "1"; }
            }
            // A journal for another slot, or without a usable id, authorises nothing.
            if (j.Slot != slot || !IsWellFormedId(j.Id)) { return null; }
            return j;
        }

        /// <summary>
        /// The directory holding save <paramref name="slot"/>, with any interrupted replacement
        /// finished or undone first.
        ///
        /// Everything that reads a slot goes through this rather than building the path itself,
        /// so a save interrupted by a crash is resolved before anyone looks at what it left.
        /// The two listers and the restore path all need it: without recovery at the read side,
        /// an interrupted save would leave the slot missing until the next save happened to
        /// clean it up.
        /// </summary>
        public static string SlotDirectory(string basePath, int slot)
        {
            Recover(basePath, slot);
            return SlotPath(basePath, slot);
        }

        /// <summary>
        /// Finish or undo an interrupted replacement of <paramref name="slot"/>. Safe to call
        /// at any time and safe to call twice: the journal is removed last, so an interruption
        /// part way leaves the next call repeating the same steps.
        ///
        /// Touches only the two names derived from the journal's id. A SAVE1.old, a
        /// SAVE1.tmp-something from another attempt, or a slot left torn by an older build are
        /// all left exactly as they are.
        /// </summary>
        public static void Recover(string basePath, int slot)
        {
            Journal j = ReadJournal(basePath, slot);
            if (j == null) { return; }

            string live = SlotPath(basePath, slot);
            string staging = StagingPath(basePath, slot, j.Id);
            string backup = BackupPath(basePath, slot, j.Id);

            bool cleared;
            if (Directory.Exists(live))
            {
                // Either the swap completed, or it never started. The slot in place is the one
                // to keep either way, and both of the others are ours to remove.
                cleared = TryDeleteDirectory(backup) & TryDeleteDirectory(staging);
            }
            else if (Directory.Exists(backup))
            {
                // Stopped between the two renames. Put the previous save back.
                Directory.Move(backup, live);
                cleared = TryDeleteDirectory(staging);
            }
            else if (j.Staged && Directory.Exists(staging))
            {
                // The new save was complete but never landed. Finish the job rather than throw
                // it away: it is what the player asked for, and on a filesystem where a
                // directory rename is not atomic it may be the only copy left.
                Directory.Move(staging, live);
                cleared = true;
            }
            else
            {
                // Nothing worth keeping, or nothing left to keep. A journal claiming a staged
                // directory that is no longer there has nothing to recover from.
                cleared = TryDeleteDirectory(staging);
            }

            // Only now, and only if what it named has gone. Removing it while a directory it
            // names survives would leave that directory with nothing to record it.
            if (cleared) { TryDeleteFile(JournalPath(basePath, slot)); }
        }

        /// <summary>
        /// Build the new contents of <paramref name="slot"/> in a staging directory and swap it
        /// in. <paramref name="writeInto"/> is given the staging directory and must write the
        /// whole slot; it is called after any foreign files have been copied across.
        ///
        /// Returns once the new slot is in place. Anything after the swap is cleanup and cannot
        /// fail the save.
        /// </summary>
        public static void Replace(string basePath, int slot, SlotRequirements required,
                                   Action<string> writeInto)
        {
            if (basePath == null) { throw new ArgumentNullException(nameof(basePath)); }
            if (writeInto == null) { throw new ArgumentNullException(nameof(writeInto)); }
            required = required ?? new SlotRequirements();

            // Anything left by an earlier attempt is resolved before this one starts.
            Recover(basePath, slot);

            // Recovery consumes a journal it recognises, so one still sitting here is not ours.
            // Publishing over it would destroy whatever it is.
            if (File.Exists(JournalPath(basePath, slot)))
            {
                throw new IOException(
                    $"save slot {slot}: {Path.GetFileName(JournalPath(basePath, slot))} already "
                    + "exists and was not written by the game");
            }

            string live = SlotPath(basePath, slot);
            string id = Guid.NewGuid().ToString("N");
            string staging = StagingPath(basePath, slot, id);
            string backup = BackupPath(basePath, slot, id);

            // Refuse rather than adopt a name that is already taken. Astronomically unlikely
            // with a GUID, but the whole scheme rests on those two names being ours.
            if (Directory.Exists(staging) || File.Exists(staging) ||
                Directory.Exists(backup) || File.Exists(backup))
            {
                throw new IOException($"save slot {slot}: a working name is already in use");
            }

            WriteJournal(basePath, slot, id, staged: false);
            Directory.CreateDirectory(staging);
            CopyForeignEntries(live, staging, required);

            writeInto(staging);
            Validate(staging, required);

            // Only now is the staging directory worth keeping if everything else falls over.
            WriteJournal(basePath, slot, id, staged: true);

            if (Directory.Exists(live)) { Directory.Move(live, backup); }
            Directory.Move(staging, live);

            // Committed. Nothing below here may throw out of this method: the save has landed
            // and reporting it as failed would be a lie, and the slot listing that follows
            // would try again and fail again.
            // Same rule as recovery: the journal may only go once what it names has, or the
            // backup would be left with nothing recording it.
            if (TryDeleteDirectory(backup)) { TryDeleteFile(JournalPath(basePath, slot)); }
        }

        /// <summary>
        /// Carry across anything already in the slot that this save does not write itself. DOS
        /// leaves such files alone, and swapping a fresh directory in would silently delete
        /// them.
        ///
        /// Regular files only. Anything else stops the save, because the alternative is to drop
        /// it quietly: a subdirectory would be lost when the backup is deleted, and copying a
        /// symlink would follow it and replace the link with its target.
        /// </summary>
        private static void CopyForeignEntries(string live, string staging, SlotRequirements required)
        {
            if (!Directory.Exists(live)) { return; }

            // Ordinal, not case-insensitive: on a case-sensitive filesystem a foreign "desc"
            // is a different file from the "DESC" we write, and treating it as ours would skip
            // it here and lose it when the backup goes.
            var ours = new HashSet<string>(StringComparer.Ordinal);
            foreach (string n in required.MustExist) { ours.Add(n); }
            foreach (string n in required.MustHaveContent) { ours.Add(n); }

            foreach (string entry in Directory.GetFileSystemEntries(live))
            {
                string name = Path.GetFileName(entry);
                var info = new FileInfo(entry);
                bool plainFile = info.Exists
                    && (info.Attributes & FileAttributes.Directory) == 0
                    && (info.Attributes & FileAttributes.ReparsePoint) == 0;
                if (!plainFile)
                {
                    throw new IOException(
                        $"save slot: {name} is not a plain file, so saving would lose it");
                }
                if (ours.Contains(name)) { continue; }
                File.Copy(entry, Path.Combine(staging, name));
            }
        }

        private static void Validate(string staging, SlotRequirements required)
        {
            foreach (string name in required.MustExist)
            {
                if (!File.Exists(Path.Combine(staging, name)))
                {
                    throw new IOException($"save is incomplete: {name} was not written");
                }
            }
            foreach (string name in required.MustHaveContent)
            {
                var f = new FileInfo(Path.Combine(staging, name));
                if (!f.Exists) { throw new IOException($"save is incomplete: {name} was not written"); }
                if (f.Length == 0) { throw new IOException($"save is incomplete: {name} is empty"); }
            }
        }

        /// <summary>Returns whether the directory is gone afterwards. The journal may only be
        /// removed once everything it names has been, or a later run would have no record of
        /// what is left to clear up.</summary>
        private static bool TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path)) { Directory.Delete(path, recursive: true); }
                return !Directory.Exists(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static void TryDeleteFile(string path)
        {
            try { if (File.Exists(path)) { File.Delete(path); } }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { }
        }
    }
}
