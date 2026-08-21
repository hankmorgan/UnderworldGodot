using System;
using System.IO;
using System.Text;
using Xunit;

namespace Underworld.Save.Tests;

/// <summary>
/// The DESC rules, measured from saves written by real DOS in both games and pulled back
/// out of the emulator. The port previously wrote a single byte and justified it with a
/// comment claiming DOS used DESC as an in-use marker and never showed the string. Both
/// halves of that were wrong: DOS writes what the player typed, and the port's own save and
/// load lists display it.
/// </summary>
public class SaveDescriptionTests
{
    [Fact]
    public void Encode_EmptyDescription_IsZeroBytes()
    {
        // DOS accepts an empty description and leaves a zero length file behind, which is
        // still an occupied slot.
        Assert.Empty(SaveDescription.Encode(""));
        Assert.Empty(SaveDescription.Encode(null));
    }

    [Fact]
    public void Encode_PreservesCase()
    {
        Assert.Equal("Testing123", Encoding.ASCII.GetString(SaveDescription.Encode("Testing123")));
    }

    [Fact]
    public void Encode_StoresTheBytesDosWasMeasuredWriting()
    {
        // Typing Testing123{|}~ in DOS produced exactly these 14 bytes. The last four only
        // render as '?' because the font has no glyphs for them; they are stored verbatim.
        byte[] expected =
        {
            0x54, 0x65, 0x73, 0x74, 0x69, 0x6e, 0x67, 0x31, 0x32, 0x33, 0x7b, 0x7c, 0x7d, 0x7e,
        };
        Assert.Equal(expected, SaveDescription.Encode("Testing123{|}~"));
    }

    [Theory]
    [InlineData(' ')]
    [InlineData('~')]
    [InlineData('{')]
    [InlineData('A')]
    [InlineData('z')]
    [InlineData('9')]
    public void IsSupported_AcceptsPrintableAscii(char c) => Assert.True(SaveDescription.IsSupported(c));

    [Theory]
    [InlineData('\0')]
    [InlineData('\t')]
    [InlineData('\n')]
    [InlineData((char)0x1F)]
    [InlineData((char)0x7F)]
    [InlineData('é')]
    public void IsSupported_RefusesEverythingElse(char c) => Assert.False(SaveDescription.IsSupported(c));

    [Fact]
    public void Encode_EveryPrintableAsciiCharacterRoundTrips()
    {
        for (char c = (char)0x20; c <= (char)0x7E; c++)
        {
            byte[] bytes = SaveDescription.Encode(c.ToString());
            Assert.Equal(new[] { (byte)c }, bytes);
        }
    }

    [Fact]
    public void Encode_ExactlyThirtyCharacters_IsAccepted()
    {
        Assert.Equal(30, SaveDescription.MaxLength);
        Assert.Equal(30, SaveDescription.Encode(new string('A', 30)).Length);
    }

    [Fact]
    public void Encode_ThirtyOneCharacters_Throws()
    {
        // Never silently capped: a caller that is not the save menu would otherwise save a
        // description different from the one it supplied.
        var ex = Assert.Throws<ArgumentException>(() => SaveDescription.Encode(new string('A', 31)));
        Assert.Contains("30", ex.Message);
    }

    [Fact]
    public void Encode_UnsupportedCharacter_ThrowsRatherThanSubstituting()
    {
        // Encoding.ASCII would turn this into '?' without a word.
        var ex = Assert.Throws<ArgumentException>(() => SaveDescription.Encode("café"));
        Assert.Contains("0xE9", ex.Message);
        Assert.DoesNotContain("?", Encoding.ASCII.GetString(SaveDescription.Encode("cafe")));
    }
}

public class SaveDescriptionSlotReadTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "uwdesc-" + Guid.NewGuid().ToString("N"));

    public SaveDescriptionSlotReadTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
    }

    private string Write(byte[] bytes)
    {
        string p = Path.Combine(_dir, "DESC");
        File.WriteAllBytes(p, bytes);
        return p;
    }

    [Fact]
    public void MissingFile_IsAnUnusedSlot()
    {
        Assert.False(SaveDescription.TryReadSlot(Path.Combine(_dir, "nothing-here"), out string name));
        Assert.Equal("", name);
    }

    [Fact]
    public void ZeroByteFile_IsOccupiedWithNoName()
    {
        // The distinction that matters: DOS writes this for an empty description, and it
        // must not read back as a free slot the player is invited to overwrite.
        Assert.True(SaveDescription.TryReadSlot(Write(Array.Empty<byte>()), out string name));
        Assert.Equal("", name);
    }

    [Fact]
    public void OrdinaryDescription_ReadsBack()
    {
        Assert.True(SaveDescription.TryReadSlot(Write(Encoding.ASCII.GetBytes("Testing123")), out string name));
        Assert.Equal("Testing123", name);
    }

    [Fact]
    public void OverlongFile_IsOccupiedButHasNoDisplayableName()
    {
        Assert.True(SaveDescription.TryReadSlot(Write(Encoding.ASCII.GetBytes(new string('A', 31))), out string name));
        Assert.Equal("", name);
    }

    [Fact]
    public void FileHoldingBytesWeCannotShow_IsOccupiedButHasNoDisplayableName()
    {
        Assert.True(SaveDescription.TryReadSlot(Write(new byte[] { 0x41, 0x00, 0x42 }), out string name));
        Assert.Equal("", name);
    }

    [Theory]
    [InlineData(new byte[] { 0x80 })]
    [InlineData(new byte[] { 0xFF })]
    [InlineData(new byte[] { 0x41, 0x80, 0x42 })]
    public void FileHoldingHighBytes_IsOccupiedButHasNoDisplayableName(byte[] raw)
    {
        // Encoding.ASCII turns anything above 0x7F into '?', so decoding first and checking
        // afterwards would accept a fabricated name and show two different corrupt files as
        // the same thing. The bytes are checked before they become a string.
        Assert.True(SaveDescription.TryReadSlot(Write(raw), out string name));
        Assert.Equal("", name);
    }

    [Fact]
    public void SingleByteFileFromAnEarlierPortBuild_StillReadsAsThatCharacter()
    {
        // Old saves keep working; they just show the one character they were given.
        Assert.True(SaveDescription.TryReadSlot(Write(new byte[] { (byte)'S' }), out string name));
        Assert.Equal("S", name);
    }
}

/// <summary>
/// Game strings carry display codes as a backslash and a digit. Nothing in the message
/// scroll strips them, so they reached the screen verbatim: the save prompt appeared as
/// "\6 Please enter a save file description" and the failure message as "\4Save game
/// failed" with a stray "\0" on the next line.
/// </summary>
public class GameStringsDisplayCodeTests
{
    [Theory]
    [InlineData("\\6 Please enter a save file description:", "Please enter a save file description:")]
    [InlineData("\\4Save game failed.\\0", "Save game failed.")]
    [InlineData("\\0", "")]
    [InlineData("No codes here", "No codes here")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void StripDisplayCodes_RemovesBackslashDigitPairs(string input, string expected)
    {
        Assert.Equal(expected, GameStringFormat.StripDisplayCodes(input));
    }

    [Fact]
    public void StripDisplayCodes_LeavesABackslashThatIsNotACode()
    {
        // A path or a stray backslash is not a display code and must survive.
        Assert.Equal(@"a\b", GameStringFormat.StripDisplayCodes(@"a\b"));
    }

    [Fact]
    public void StripDisplayCodes_HandlesATrailingBackslash()
    {
        Assert.Equal(@"end\", GameStringFormat.StripDisplayCodes(@"end\"));
    }
}
