using System;
using Xunit;

namespace Underworld.Save.Tests;

/// <summary>
/// The prompt's decisions, without Godot. The UI layer owns a LineEdit and forwards events;
/// everything worth getting wrong lives here.
/// </summary>
public class SaveDescriptionPromptTests
{
    private static SaveDescriptionPrompt Opened(string existing = "", int slot = 1)
    {
        var p = new SaveDescriptionPrompt();
        p.Open(slot, existing);
        return p;
    }

    [Fact]
    public void Open_OnAnOccupiedSlot_PrefillsAndSelects()
    {
        var p = Opened("Original Name");
        Assert.True(p.Active);
        Assert.Equal(1, p.Slot);
        Assert.Equal("Original Name", p.Buffer);
        Assert.True(p.SelectionPending);
    }

    [Fact]
    public void Open_WithAnExistingNameWeCannotShow_StartsEmptyRatherThanRefusing()
    {
        Assert.Equal("", Opened("A\0B").Buffer);
        Assert.Equal("", Opened(new string('A', 31)).Buffer);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void Open_RejectsASlotThatIsNotOneToFour(int slot) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Opened(slot: slot));

    [Fact]
    public void Commit_ReturnsTheBufferAndCloses()
    {
        var p = Opened("Original Name");
        Assert.Equal("Original Name", p.Commit());
        Assert.False(p.Active);
        Assert.Equal(0, p.Slot);
    }

    [Fact]
    public void Commit_WithNothingTyped_IsAValidEmptyDescription()
    {
        // Not the same as map notes, where an empty note is discarded. Here DOS writes a
        // zero byte file, so an empty commit has to be distinguishable from a cancel.
        var p = Opened();
        Assert.Equal("", p.Commit());
        Assert.False(p.Active);
    }

    [Fact]
    public void Commit_WhenNothingIsOpen_Throws() =>
        Assert.Throws<InvalidOperationException>(() => new SaveDescriptionPrompt().Commit());

    [Fact]
    public void Cancel_ClosesWithoutReturningAnything()
    {
        var p = Opened("Original Name");
        p.Cancel();
        Assert.False(p.Active);
        Assert.Equal(0, p.Slot);
        Assert.Equal("", p.Buffer);
    }

    // ---- the deferred focus/select guard -------------------------------------------

    [Fact]
    public void DeferredCallback_RunsForTheGenerationItWasQueuedWith()
    {
        var p = Opened("Name");
        Assert.True(p.MayRunDeferred(p.Generation));
    }

    [Fact]
    public void DeferredCallback_DoesNotRunAfterCancel()
    {
        // Otherwise it would focus and select a field that is no longer on screen.
        var p = Opened("Name");
        int queued = p.Generation;
        p.Cancel();
        Assert.False(p.MayRunDeferred(queued));
    }

    [Fact]
    public void DeferredCallback_DoesNotRunAfterTheUserHasTyped()
    {
        // It would otherwise reselect everything and throw away what they had started.
        var p = Opened("Name");
        int queued = p.Generation;
        p.NoteInteraction();
        Assert.False(p.MayRunDeferred(queued));
    }

    [Fact]
    public void DeferredCallback_DoesNotRunForAnOlderPrompt()
    {
        var p = Opened("First", slot: 1);
        int stale = p.Generation;
        p.Cancel();
        p.Open(2, "Second");
        Assert.False(p.MayRunDeferred(stale));
        Assert.True(p.MayRunDeferred(p.Generation));
    }

    [Fact]
    public void EveryStateChangeBumpsTheGeneration()
    {
        // MayRunDeferred's other two conditions are redundant while this holds: a stale
        // callback is caught by the generation alone. Mutating either of them out changes
        // nothing, which is only safe as long as this property is true, so it is pinned
        // here rather than left implied.
        var p = new SaveDescriptionPrompt();

        int start = p.Generation;
        p.Open(1, "Name");
        Assert.NotEqual(start, p.Generation);

        int opened = p.Generation;
        p.NoteInteraction();
        Assert.NotEqual(opened, p.Generation);

        int interacted = p.Generation;
        p.Cancel();
        Assert.NotEqual(interacted, p.Generation);

        p.Open(2, "Other");
        int reopened = p.Generation;
        p.Commit();
        Assert.NotEqual(reopened, p.Generation);
    }

    // ---- the select-all marker ------------------------------------------------------

    [Fact]
    public void NoteInteraction_ClearsTheSelectionOnlyOnce()
    {
        var p = Opened("Name");
        p.NoteInteraction();
        int after = p.Generation;
        p.NoteInteraction();
        Assert.Equal(after, p.Generation);   // later events are not first interactions
        Assert.False(p.SelectionPending);
    }

    [Fact]
    public void BeginEditingPrefill_AppliesOnlyWhileThePrefillIsStillSelected()
    {
        var p = Opened("Name");
        Assert.True(p.BeginEditingPrefill());    // Backspace edits the existing name
        Assert.False(p.SelectionPending);
        Assert.False(p.BeginEditingPrefill());   // a later Backspace is just a Backspace
    }

    [Fact]
    public void BeginEditingPrefill_DoesNothingWhenClosed() =>
        Assert.False(new SaveDescriptionPrompt().BeginEditingPrefill());

    // ---- validation is a rollback, because Godot changes the text first --------------

    [Fact]
    public void TryAccept_KeepsAStorableValue()
    {
        var p = Opened();
        Assert.False(p.TryAccept("Testing123{|}~"));
        Assert.Equal("Testing123{|}~", p.Buffer);
    }

    [Fact]
    public void TryAccept_RejectsAndLeavesTheBufferAlone()
    {
        var p = Opened("Good");
        Assert.True(p.TryAccept("Bad\0Value"));
        Assert.Equal("Good", p.Buffer);          // the field is reset to this
    }

    [Fact]
    public void TryAccept_RejectsSomethingTooLong()
    {
        // A paste is truncated by MaxLength before it reaches here, so this is the path for
        // anything that gets past that.
        var p = Opened("Good");
        Assert.True(p.TryAccept(new string('A', 31)));
        Assert.Equal("Good", p.Buffer);
    }

    [Fact]
    public void TryAccept_AcceptsExactlyThirty()
    {
        var p = Opened();
        Assert.False(p.TryAccept(new string('A', 30)));
        Assert.Equal(30, p.Buffer.Length);
    }

    [Fact]
    public void TryAccept_WhenClosed_ChangesNothing()
    {
        var p = new SaveDescriptionPrompt();
        Assert.False(p.TryAccept("anything"));
        Assert.Equal("", p.Buffer);
    }

    [Fact]
    public void TypingAfterOpeningReplacesThePrefill_ButBackspaceEditsIt()
    {
        // The two halves of what DOS does with an occupied slot.
        var replace = Opened("Original Name");
        replace.NoteInteraction();
        Assert.False(replace.TryAccept("N"));
        Assert.Equal("N", replace.Buffer);

        var edit = Opened("Original Name");
        Assert.True(edit.BeginEditingPrefill());
        Assert.False(edit.TryAccept("Original Nam"));
        Assert.Equal("Original Nam", edit.Buffer);
    }
}
