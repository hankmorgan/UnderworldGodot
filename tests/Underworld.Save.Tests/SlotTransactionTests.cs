using System;
using System.IO;
using System.Linq;

namespace Underworld.Save.Tests;

/// <summary>
/// Covers <see cref="SlotTransaction"/>, which replaces a save slot as a unit.
///
/// Issue #74: the slot was written in place, so a failure part way left old and new files
/// mixed. DESC is written first and both slot listers key off it while restore keys off
/// LEV.ARK, so the usual result was a slot listed with a name that refused to load.
///
/// These drive the helper against a temporary directory. It takes its base path rather than
/// reading UWClass.BasePath precisely so this is possible without touching global state.
/// </summary>
public class SlotTransactionTests : IDisposable
{
    private readonly string _root;

    public SlotTransactionTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "uwslot-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { /* best effort */ }
    }

    private string Slot(int n = 1) => Path.Combine(_root, $"SAVE{n}");
    private string Journal(int n = 1) => Path.Combine(_root, $"SAVE{n}.txn");
    /// <summary>Working directories left beside a slot. Excludes the slot itself: .NET's
    /// "SAVE1.*" also matches "SAVE1", which is not what this is asking.</summary>
    private string[] Siblings(int n = 1) =>
        Directory.GetFileSystemEntries(_root)
                 .Select(Path.GetFileName)
                 .Where(x => x.StartsWith($"SAVE{n}.", StringComparison.Ordinal))
                 .ToArray();

    private static SlotRequirements Uw1 => new()
    {
        MustExist = new[] { "DESC" },
        MustHaveContent = new[] { "PLAYER.DAT", "LEV.ARK" },
    };

    private static void WriteCompleteSlot(string dir)
    {
        File.WriteAllText(Path.Combine(dir, "DESC"), "new");
        File.WriteAllText(Path.Combine(dir, "PLAYER.DAT"), "player");
        File.WriteAllText(Path.Combine(dir, "LEV.ARK"), "level");
    }

    private void GiveSlotAnExistingSave(int n = 1)
    {
        Directory.CreateDirectory(Slot(n));
        File.WriteAllText(Path.Combine(Slot(n), "DESC"), "old");
        File.WriteAllText(Path.Combine(Slot(n), "PLAYER.DAT"), "old player");
        File.WriteAllText(Path.Combine(Slot(n), "LEV.ARK"), "old level");
    }

    // ---- the ordinary paths -------------------------------------------------------------

    [Fact]
    public void ReplacingAnEmptySlotLeavesTheNewSaveAndNothingElse()
    {
        SlotTransaction.Replace(_root, 1, Uw1, WriteCompleteSlot);

        Assert.Equal("new", File.ReadAllText(Path.Combine(Slot(), "DESC")));
        Assert.Empty(Siblings());
    }

    [Fact]
    public void ReplacingAnOccupiedSlotLeavesOnlyTheNewSave()
    {
        GiveSlotAnExistingSave();

        SlotTransaction.Replace(_root, 1, Uw1, WriteCompleteSlot);

        Assert.Equal("new", File.ReadAllText(Path.Combine(Slot(), "DESC")));
        Assert.Equal("player", File.ReadAllText(Path.Combine(Slot(), "PLAYER.DAT")));
        // No journal, no staging directory, no backup left behind.
        Assert.Empty(Siblings());
    }

    [Fact]
    public void AWriterThatThrowsLeavesThePreviousSaveExactlyAsItWas()
    {
        GiveSlotAnExistingSave();

        Assert.Throws<InvalidOperationException>(() =>
            SlotTransaction.Replace(_root, 1, Uw1, dir =>
            {
                File.WriteAllText(Path.Combine(dir, "DESC"), "half");
                throw new InvalidOperationException("writer gave up");
            }));

        // This is the whole point of the issue: the old save survives intact.
        Assert.Equal("old", File.ReadAllText(Path.Combine(Slot(), "DESC")));
        Assert.Equal("old player", File.ReadAllText(Path.Combine(Slot(), "PLAYER.DAT")));
        Assert.Equal("old level", File.ReadAllText(Path.Combine(Slot(), "LEV.ARK")));
    }

    [Fact]
    public void AWriterThatThrowsOnAnEmptySlotLeavesTheSlotEmpty()
    {
        Assert.Throws<InvalidOperationException>(() =>
            SlotTransaction.Replace(_root, 1, Uw1, dir =>
            {
                File.WriteAllText(Path.Combine(dir, "DESC"), "half");
                throw new InvalidOperationException("writer gave up");
            }));

        Assert.False(Directory.Exists(Slot()));
    }

    // ---- validation ---------------------------------------------------------------------

    [Fact]
    public void AMissingRequiredFileStopsTheSaveAndKeepsTheOldOne()
    {
        GiveSlotAnExistingSave();

        Assert.ThrowsAny<IOException>(() =>
            SlotTransaction.Replace(_root, 1, Uw1, dir =>
            {
                File.WriteAllText(Path.Combine(dir, "DESC"), "new");
                File.WriteAllText(Path.Combine(dir, "PLAYER.DAT"), "player");
                // LEV.ARK never written.
            }));

        Assert.Equal("old", File.ReadAllText(Path.Combine(Slot(), "DESC")));
    }

    [Fact]
    public void AnEmptyArchiveStopsTheSave()
    {
        GiveSlotAnExistingSave();

        // ScdArkWriter returns an empty array when its source is missing, which without this
        // check would be committed as a finished UW2 save.
        Assert.ThrowsAny<IOException>(() =>
            SlotTransaction.Replace(_root, 1, Uw1, dir =>
            {
                File.WriteAllText(Path.Combine(dir, "DESC"), "new");
                File.WriteAllText(Path.Combine(dir, "PLAYER.DAT"), "player");
                File.WriteAllBytes(Path.Combine(dir, "LEV.ARK"), Array.Empty<byte>());
            }));

        Assert.Equal("old", File.ReadAllText(Path.Combine(Slot(), "DESC")));
    }

    [Fact]
    public void AnEmptyDescriptionIsAllowed()
    {
        // DOS writes a zero length DESC for an empty description and the slot is still
        // occupied, so this must not be treated as an incomplete save.
        SlotTransaction.Replace(_root, 1, Uw1, dir =>
        {
            File.WriteAllBytes(Path.Combine(dir, "DESC"), Array.Empty<byte>());
            File.WriteAllText(Path.Combine(dir, "PLAYER.DAT"), "player");
            File.WriteAllText(Path.Combine(dir, "LEV.ARK"), "level");
        });

        Assert.True(File.Exists(Path.Combine(Slot(), "DESC")));
        Assert.Equal(0, new FileInfo(Path.Combine(Slot(), "DESC")).Length);
    }

    // ---- foreign entries ----------------------------------------------------------------

    [Fact]
    public void AFileTheGameDidNotWriteSurvivesTheSave()
    {
        GiveSlotAnExistingSave();
        File.WriteAllText(Path.Combine(Slot(), "NOTES.TXT"), "mine");

        SlotTransaction.Replace(_root, 1, Uw1, WriteCompleteSlot);

        // DOS leaves such files alone, so a bare directory swap would be a regression.
        Assert.Equal("mine", File.ReadAllText(Path.Combine(Slot(), "NOTES.TXT")));
        Assert.Equal("new", File.ReadAllText(Path.Combine(Slot(), "DESC")));
    }

    [Fact]
    public void ASubdirectoryInTheSlotStopsTheSaveRatherThanBeingLost()
    {
        GiveSlotAnExistingSave();
        Directory.CreateDirectory(Path.Combine(Slot(), "SUBDIR"));

        // Copying only files would drop it silently when the backup is deleted.
        Assert.ThrowsAny<IOException>(() =>
            SlotTransaction.Replace(_root, 1, Uw1, WriteCompleteSlot));

        Assert.True(Directory.Exists(Path.Combine(Slot(), "SUBDIR")));
        Assert.Equal("old", File.ReadAllText(Path.Combine(Slot(), "DESC")));
    }

    // ---- recovery -----------------------------------------------------------------------

    [Fact]
    public void RecoveryWithNoJournalDoesNothingAtAll()
    {
        GiveSlotAnExistingSave();
        Directory.CreateDirectory(Path.Combine(_root, "SAVE1.old"));
        Directory.CreateDirectory(Path.Combine(_root, "SAVE1.tmp-whatever"));

        SlotTransaction.Recover(_root, 1);

        // Names that look like ours but are not named by a journal are none of our business.
        Assert.True(Directory.Exists(Path.Combine(_root, "SAVE1.old")));
        Assert.True(Directory.Exists(Path.Combine(_root, "SAVE1.tmp-whatever")));
        Assert.Equal("old", File.ReadAllText(Path.Combine(Slot(), "DESC")));
    }

    [Fact]
    public void AJournalWithAMalformedIdIsIgnored()
    {
        GiveSlotAnExistingSave();
        File.WriteAllText(Journal(), "slot=1\nid=../../etc\nstaged=1\n");

        SlotTransaction.Recover(_root, 1);

        Assert.Equal("old", File.ReadAllText(Path.Combine(Slot(), "DESC")));
        // Ignored means untouched. Acting on it at all, even to tidy it away, would mean the
        // id had been used to derive a path.
        Assert.True(File.Exists(Journal()));
    }

    [Fact]
    public void AJournalNamingAnotherSlotIsIgnored()
    {
        GiveSlotAnExistingSave();
        string id = new string('a', 32);
        File.WriteAllText(Journal(), $"slot=2\nid={id}\nstaged=1\n");

        SlotTransaction.Recover(_root, 1);

        Assert.Equal("old", File.ReadAllText(Path.Combine(Slot(), "DESC")));
        Assert.True(File.Exists(Journal()));
    }

    [Fact]
    public void AnInterruptionBetweenTheTwoRenamesPutsThePreviousSaveBack()
    {
        // The state after SAVE1 was renamed aside but the new one had not landed.
        string id = new string('a', 32);
        string backup = Path.Combine(_root, $"SAVE1.old-{id}");
        string staging = Path.Combine(_root, $"SAVE1.tmp-{id}");
        Directory.CreateDirectory(backup);
        File.WriteAllText(Path.Combine(backup, "DESC"), "old");
        Directory.CreateDirectory(staging);
        File.WriteAllText(Journal(), $"slot=1\nid={id}\nstaged=1\n");

        SlotTransaction.Recover(_root, 1);

        Assert.Equal("old", File.ReadAllText(Path.Combine(Slot(), "DESC")));
        Assert.Empty(Siblings());
    }

    [Fact]
    public void AStagedSaveThatNeverLandedIsCompletedRatherThanDiscarded()
    {
        // Slot and backup both absent, staging validated. Deleting it would throw away a
        // complete save the player asked for, and on a filesystem where a directory rename is
        // not atomic it could be the only copy left.
        string id = new string('b', 32);
        string staging = Path.Combine(_root, $"SAVE1.tmp-{id}");
        Directory.CreateDirectory(staging);
        File.WriteAllText(Path.Combine(staging, "DESC"), "rescued");
        File.WriteAllText(Journal(), $"slot=1\nid={id}\nstaged=1\n");

        SlotTransaction.Recover(_root, 1);

        Assert.Equal("rescued", File.ReadAllText(Path.Combine(Slot(), "DESC")));
        Assert.Empty(Siblings());
    }

    [Fact]
    public void AnUnstagedDirectoryThatNeverLandedIsDiscarded()
    {
        string id = new string('c', 32);
        string staging = Path.Combine(_root, $"SAVE1.tmp-{id}");
        Directory.CreateDirectory(staging);
        File.WriteAllText(Path.Combine(staging, "DESC"), "half written");
        File.WriteAllText(Journal(), $"slot=1\nid={id}\nstaged=0\n");

        SlotTransaction.Recover(_root, 1);

        Assert.False(Directory.Exists(Slot()));
        Assert.Empty(Siblings());
    }

    [Fact]
    public void AnInterruptionAfterTheCommitClearsUpAndKeepsTheNewSave()
    {
        string id = new string('d', 32);
        GiveSlotAnExistingSave();
        File.WriteAllText(Path.Combine(Slot(), "DESC"), "new");
        Directory.CreateDirectory(Path.Combine(_root, $"SAVE1.old-{id}"));
        File.WriteAllText(Journal(), $"slot=1\nid={id}\nstaged=1\n");

        SlotTransaction.Recover(_root, 1);

        Assert.Equal("new", File.ReadAllText(Path.Combine(Slot(), "DESC")));
        Assert.Empty(Siblings());
    }

    [Fact]
    public void RecoveryRunsTwiceWithoutChangingItsAnswer()
    {
        string id = new string('e', 32);
        string backup = Path.Combine(_root, $"SAVE1.old-{id}");
        Directory.CreateDirectory(backup);
        File.WriteAllText(Path.Combine(backup, "DESC"), "old");
        File.WriteAllText(Journal(), $"slot=1\nid={id}\nstaged=1\n");

        SlotTransaction.Recover(_root, 1);
        SlotTransaction.Recover(_root, 1);

        Assert.Equal("old", File.ReadAllText(Path.Combine(Slot(), "DESC")));
        Assert.Empty(Siblings());
    }

    [Fact]
    public void ReplacingRunsRecoveryFirst()
    {
        // A save left half done by a previous run must be resolved before the next one starts,
        // not layered on top of.
        string id = new string('f', 32);
        string backup = Path.Combine(_root, $"SAVE1.old-{id}");
        Directory.CreateDirectory(backup);
        File.WriteAllText(Path.Combine(backup, "DESC"), "old");
        File.WriteAllText(Journal(), $"slot=1\nid={id}\nstaged=1\n");

        SlotTransaction.Replace(_root, 1, Uw1, WriteCompleteSlot);

        Assert.Equal("new", File.ReadAllText(Path.Combine(Slot(), "DESC")));
        Assert.Empty(Siblings());
    }

    [Fact]
    public void SlotDirectoryResolvesAnInterruptedSaveBeforeAnyoneReadsIt()
    {
        // The listers and the restore path go through this, so a crash mid-save is undone
        // before the slot is inspected rather than showing as missing.
        string id = new string('a', 32);
        string backup = Path.Combine(_root, $"SAVE1.old-{id}");
        Directory.CreateDirectory(backup);
        File.WriteAllText(Path.Combine(backup, "DESC"), "old");
        File.WriteAllText(Journal(), $"slot=1\nid={id}\nstaged=1\n");

        string dir = SlotTransaction.SlotDirectory(_root, 1);

        Assert.Equal(Slot(), dir);
        Assert.Equal("old", File.ReadAllText(Path.Combine(dir, "DESC")));
    }

    [Fact]
    public void AJournalWeDidNotWriteStopsTheSaveRatherThanBeingOverwritten()
    {
        GiveSlotAnExistingSave();
        File.WriteAllText(Journal(), "something else entirely");

        // Recovery ignores a journal it does not recognise, so publishing over it would
        // destroy whatever it is.
        Assert.ThrowsAny<IOException>(() =>
            SlotTransaction.Replace(_root, 1, Uw1, WriteCompleteSlot));

        Assert.Equal("something else entirely", File.ReadAllText(Journal()));
        Assert.Equal("old", File.ReadAllText(Path.Combine(Slot(), "DESC")));
    }

    [Fact]
    public void AScratchNameSomebodyElseLeftIsNotTruncated()
    {
        // The journal used to be published through a fixed SAVE1.txn.writing, which would
        // overwrite whatever was sitting there.
        File.WriteAllText(Path.Combine(_root, "SAVE1.txn.writing"), "not ours");

        SlotTransaction.Replace(_root, 1, Uw1, WriteCompleteSlot);

        Assert.Equal("not ours", File.ReadAllText(Path.Combine(_root, "SAVE1.txn.writing")));
    }

    [Fact]
    public void AForeignFileDifferingOnlyByCaseIsNotLost()
    {
        GiveSlotAnExistingSave();
        string lower = Path.Combine(Slot(), "desc");
        bool caseSensitive = !File.Exists(lower);
        File.WriteAllText(lower, "mine");

        SlotTransaction.Replace(_root, 1, Uw1, WriteCompleteSlot);

        if (caseSensitive)
        {
            // "desc" is a different file from the "DESC" we write. Comparing owned names
            // case-insensitively would treat it as ours, skip the copy, and lose it with the
            // backup.
            Assert.Equal("mine", File.ReadAllText(lower));
        }
        else
        {
            // Same file, so the save simply overwrites it. What must not happen either way is
            // the name disappearing.
            Assert.Equal("new", File.ReadAllText(lower));
        }
        Assert.Equal("new", File.ReadAllText(Path.Combine(Slot(), "DESC")));
    }

    [Fact]
    public void AJournalNamingAStagedDirectoryThatIsGoneIsCleanedUp()
    {
        string id = new string('a', 32);
        File.WriteAllText(Journal(), $"slot=1\nid={id}\nstaged=1\n");

        SlotTransaction.Recover(_root, 1);

        // Nothing to recover from, so the journal goes rather than blocking the slot.
        Assert.False(File.Exists(Journal()));
        Assert.False(Directory.Exists(Slot()));
    }

    [Fact]
    public void OneSlotsRecoveryLeavesAnotherAlone()
    {
        GiveSlotAnExistingSave(2);
        string id = new string('a', 32);
        Directory.CreateDirectory(Path.Combine(_root, $"SAVE1.tmp-{id}"));
        File.WriteAllText(Journal(), $"slot=1\nid={id}\nstaged=0\n");

        SlotTransaction.Recover(_root, 1);

        Assert.Equal("old", File.ReadAllText(Path.Combine(Slot(2), "DESC")));
    }
}
