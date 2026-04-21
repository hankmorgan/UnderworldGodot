using System.IO;
using Xunit;

namespace Underworld.Save.Tests;

[Collection("UWClassState")]
public class AutomapNotesRoundTripTests : System.IDisposable
{
    private readonly string _origBasePath;
    private readonly byte _origRes;

    public AutomapNotesRoundTripTests()
    {
        _origBasePath = Underworld.UWClass.BasePath;
        _origRes = Underworld.UWClass._RES;
    }

    public void Dispose()
    {
        Underworld.UWClass.BasePath = _origBasePath;
        Underworld.UWClass._RES = _origRes;
    }

    [Fact]
    public void Serialize_EmptyNotes_ReturnsEmptyByteArray()
    {
        var note = new Underworld.automapnote();
        byte[] result = note.Serialize();
        Assert.Empty(result);
    }

    [Fact]
    public void Serialize_OneNote_Writes54Bytes_NullTerminatedStringAtOffset0PosAt0x32()
    {
        var note = new Underworld.automapnote();
        note.notes.Add(new Underworld.automapnote.mapnotetext("hello", 42, 7));
        byte[] result = note.Serialize();

        Assert.Equal(54, result.Length);
        Assert.Equal((byte)'h', result[0]);
        Assert.Equal((byte)'e', result[1]);
        Assert.Equal((byte)'l', result[2]);
        Assert.Equal((byte)'l', result[3]);
        Assert.Equal((byte)'o', result[4]);
        Assert.Equal((byte)0, result[5]);
        Assert.Equal((byte)42, result[0x32]);
        Assert.Equal((byte)0, result[0x33]);
        Assert.Equal((byte)7, result[0x34]);
        Assert.Equal((byte)0, result[0x35]);
    }

    [Fact]
    public void Serialize_TwoNotes_EmitsBackToBack108Bytes_SecondNoteAtOffset54()
    {
        var note = new Underworld.automapnote();
        note.notes.Add(new Underworld.automapnote.mapnotetext("A", 1, 2));
        note.notes.Add(new Underworld.automapnote.mapnotetext("B", 3, 4));
        byte[] result = note.Serialize();

        Assert.Equal(108, result.Length);
        Assert.Equal((byte)'A', result[0]);
        Assert.Equal((byte)'B', result[54]);
        Assert.Equal((byte)1, result[0x32]);
        Assert.Equal((byte)2, result[0x34]);
        Assert.Equal((byte)3, result[54 + 0x32]);
        Assert.Equal((byte)4, result[54 + 0x34]);
    }
}
