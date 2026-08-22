using System;
using System.IO;
using System.Text;

namespace Underworld
{
    /// <summary>
    /// The description shown against a save slot, stored in the slot's DESC file.
    ///
    /// Measured from saves written by real DOS, both games: DESC is raw ASCII with no
    /// terminator and is exactly as long as the text typed. Case is preserved. Nothing is
    /// filtered within printable ASCII, so "{|}~" are stored verbatim even though the font
    /// has no glyphs for them. The limit is 30 characters. An empty description is accepted
    /// and writes a zero length file, which is not the same as an unused slot.
    ///
    /// No Godot types here on purpose, so the headless save tests can reach all of it.
    /// </summary>
    public static class SaveDescription
    {
        /// <summary>DOS stops accepting at 30 characters.</summary>
        public const int MaxLength = 30;

        private const char FirstSupported = (char)0x20;
        private const char LastSupported = (char)0x7E;

        /// <summary>
        /// Whether a character can be stored. The bound is what DOS was measured writing,
        /// not what it was measured rejecting: bytes outside this range were never tested,
        /// so they are refused rather than guessed at.
        /// </summary>
        public static bool IsSupported(char c) => c >= FirstSupported && c <= LastSupported;

        public static bool IsSupported(string text)
        {
            if (string.IsNullOrEmpty(text)) return true;
            foreach (char c in text)
            {
                if (!IsSupported(c)) return false;
            }
            return true;
        }

        /// <summary>
        /// The bytes for a slot's DESC file. Throws rather than silently trimming or
        /// substituting, so a caller that is not the save menu cannot write a description
        /// different from the one it asked for. Encoding.ASCII would turn an unsupported
        /// character into '?' without saying so.
        /// </summary>
        public static byte[] Encode(string description)
        {
            if (string.IsNullOrEmpty(description)) return Array.Empty<byte>();

            if (description.Length > MaxLength)
            {
                throw new ArgumentException(
                    $"a save description is at most {MaxLength} characters, got {description.Length}",
                    nameof(description));
            }

            var bytes = new byte[description.Length];
            for (int i = 0; i < description.Length; i++)
            {
                char c = description[i];
                if (!IsSupported(c))
                {
                    throw new ArgumentException(
                        $"character 0x{(int)c:X2} at position {i} cannot be stored in a save description",
                        nameof(description));
                }
                bytes[i] = (byte)c;
            }
            return bytes;
        }

        /// <summary>
        /// What a slot's DESC file says, for the save and load lists.
        ///
        /// Occupancy is deliberately not File.Exists: it returns false when access is
        /// denied, which would show an existing save as an empty slot and invite the player
        /// to overwrite it. Once the file is known to exist, any later failure to read or
        /// any content we cannot display leaves the slot occupied with no name, which is
        /// still true and still refuses to lose someone's save.
        /// </summary>
        public static bool TryReadSlot(string descPath, out string description)
        {
            description = "";

            try
            {
                _ = File.GetAttributes(descPath);
            }
            catch (FileNotFoundException) { return false; }
            catch (DirectoryNotFoundException) { return false; }
            catch (IOException) { return true; }
            catch (UnauthorizedAccessException) { return true; }
            catch (NotSupportedException) { return false; }
            catch (ArgumentException) { return false; }

            byte[] raw;
            try
            {
                raw = File.ReadAllBytes(descPath);
            }
            catch (IOException) { return true; }
            catch (UnauthorizedAccessException) { return true; }

            // The bytes are checked before they become a string. Encoding.ASCII turns
            // anything above 0x7F into '?', so decoding first would invent a name that
            // passes inspection and would show two different damaged files as the same.
            if (raw.Length > MaxLength)
            {
                // Written by something else, or damaged. The slot is real, the name is not
                // something we can show.
                return true;
            }
            foreach (byte b in raw)
            {
                if (b < 0x20 || b > 0x7E) return true;
            }

            description = Encoding.ASCII.GetString(raw);
            return true;
        }
    }
}
