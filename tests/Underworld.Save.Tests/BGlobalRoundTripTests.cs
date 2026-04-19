using System;
using System.IO;

namespace Underworld.Save.Tests;

/// <summary>
/// Round-trip tests for BGlobalWriter: load → serialize → byte-identical.
/// </summary>
public class BGlobalRoundTripTests
{
    /// <summary>
    /// Load UW2 BGLOBALS.DAT, serialize via BGlobalWriter, assert byte-identical to original.
    /// </summary>
    [Fact]
    public void Uw2BGlobals_ByteIdentical_AfterSerialize()
    {
        // Arrange
        var fixtureFile = TestData.Uw2Save0("BGLOBALS.DAT");
        byte[] originalBytes = File.ReadAllBytes(fixtureFile);

        // Set up UWClass for LoadGlobals
        UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW2");
        UWClass._RES = Loader.GAME_UW2;

        // Load the globals
        bglobal.LoadGlobals("SAVE0");
        var loadedGlobals = bglobal.bGlobals;

        // Act
        byte[] serialized = BGlobalWriter.Serialize(loadedGlobals);

        // Assert
        Assert.Equal(originalBytes, serialized);
    }

    /// <summary>
    /// Empty array should return empty byte array.
    /// </summary>
    [Fact]
    public void Serialize_EmptyArray_ReturnsEmptyByteArray()
    {
        // Arrange
        var emptyGlobals = Array.Empty<bglobal.BablGlobal>();

        // Act
        byte[] result = BGlobalWriter.Serialize(emptyGlobals);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// One slot with ConversationNo=7, Size=0 writes exactly 4 bytes: {0x07, 0x00, 0x00, 0x00}.
    /// </summary>
    [Fact]
    public void Serialize_OneSlotZeroSize_Writes4Bytes()
    {
        // Arrange
        var oneSlot = new bglobal.BablGlobal[]
        {
            new bglobal.BablGlobal
            {
                ConversationNo = 7,
                Size = 0,
                Globals = new short[0]
            }
        };

        // Act
        byte[] result = BGlobalWriter.Serialize(oneSlot);

        // Assert
        byte[] expected = { 0x07, 0x00, 0x00, 0x00 };
        Assert.Equal(expected, result);
    }
}
