using System;

namespace Underworld
{
    /// <summary>
    /// The state behind "please enter a save file description".
    ///
    /// Kept free of Godot so the headless tests can drive it. The UI owns a LineEdit and
    /// forwards events here; this decides what should happen. The buffer here is the truth
    /// and the LineEdit is its view, which is what makes rejecting an invalid paste a
    /// rollback to a known value rather than a guess.
    ///
    /// DOS behaviour this reproduces: Enter commits, Escape abandons the whole save,
    /// an occupied slot starts prefilled with its existing name, and the first thing typed
    /// replaces that name while Backspace instead edits it.
    /// </summary>
    public class SaveDescriptionPrompt
    {
        public bool Active { get; private set; }

        /// <summary>Slot 1..4 being saved to, or 0 when not prompting.</summary>
        public int Slot { get; private set; }

        /// <summary>The last text known to be storable. The LineEdit is reset to this.</summary>
        public string Buffer { get; private set; } = "";

        /// <summary>
        /// True from opening until the player does something. While set, the whole prefill
        /// is selected, so typing replaces it.
        /// </summary>
        public bool SelectionPending { get; private set; }

        /// <summary>
        /// Bumped on every open and on the first interaction. A deferred focus or select
        /// callback carries the value it was queued with and does nothing if it no longer
        /// matches, so it cannot act on a prompt that has since been abandoned or typed in.
        /// </summary>
        public int Generation { get; private set; }

        public void Open(int slot, string existingDescription)
        {
            if (slot < 1 || slot > 4)
                throw new ArgumentOutOfRangeException(nameof(slot), slot, "slot must be 1..4");

            Active = true;
            Slot = slot;
            Buffer = SaveDescription.IsSupported(existingDescription)
                     && (existingDescription?.Length ?? 0) <= SaveDescription.MaxLength
                ? existingDescription ?? ""
                : "";   // unreadable or not storable: start empty rather than refuse to open
            SelectionPending = true;
            Generation++;
        }

        /// <summary>
        /// Whether a queued focus or selection callback should still run. Three things have
        /// to hold: same prompt, still open, and the player has not touched the field.
        /// </summary>
        public bool MayRunDeferred(int generation) =>
            Active && SelectionPending && generation == Generation;

        /// <summary>
        /// The player did something actionable: a pressed key, mouse button or touch. Not
        /// pointer motion or key releases, which would otherwise cancel the initial
        /// selection just because the pointer crossed the field.
        /// </summary>
        public void NoteInteraction()
        {
            if (!Active || !SelectionPending) return;
            SelectionPending = false;
            Generation++;
        }

        /// <summary>
        /// Backspace while the prefill is still selected. DOS treats it as editing the
        /// existing name rather than replacing it, so the selection is dropped and the
        /// caret goes to the end. The event itself is left alone so the deletion happens
        /// normally, which is why this returns whether it applied rather than doing it.
        /// </summary>
        public bool BeginEditingPrefill()
        {
            if (!Active || !SelectionPending) return false;
            NoteInteraction();
            return true;
        }

        /// <summary>
        /// Godot changes the text and then tells us, so an unusable value is undone rather
        /// than prevented. Returns true when the caller should push Buffer back into the
        /// field.
        /// </summary>
        public bool TryAccept(string candidate)
        {
            if (!Active) return false;

            candidate ??= "";
            if (candidate.Length > SaveDescription.MaxLength || !SaveDescription.IsSupported(candidate))
            {
                return true;    // rejected: Buffer is unchanged and the field must be reset
            }

            Buffer = candidate;
            return false;
        }

        /// <summary>Enter. Returns the description to save and closes the prompt.</summary>
        public string Commit()
        {
            if (!Active) throw new InvalidOperationException("no prompt is open");
            string description = Buffer;
            Close();
            return description;
        }

        /// <summary>Escape. Nothing is saved.</summary>
        public void Cancel() => Close();

        private void Close()
        {
            Active = false;
            Slot = 0;
            Buffer = "";
            SelectionPending = false;
            Generation++;
        }
    }
}
