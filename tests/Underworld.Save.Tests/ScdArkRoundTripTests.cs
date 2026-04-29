using System;
using System.IO;
using Xunit;

namespace Underworld.Save.Tests;

// Share collection with other UWClass-state tests to serialise them and avoid static-state races.
[Collection("UWClassState")]
public class ScdArkRoundTripTests : IDisposable
{
    private readonly string _origBasePath;
    private readonly byte _origRes;
    private readonly Underworld.UWBlock[] _origScdData;

    public ScdArkRoundTripTests()
    {
        _origBasePath = Underworld.UWClass.BasePath;
        _origRes      = Underworld.UWClass._RES;
        _origScdData  = Underworld.scd.scd_data;
    }

    public void Dispose()
    {
        Underworld.UWClass.BasePath = _origBasePath;
        Underworld.UWClass._RES     = _origRes;
        Underworld.scd.scd_data     = _origScdData;
    }

    [Fact]
    public void Uw2Scd_LoadAndSerialize_ByteIdentical()
    {
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW2");
        Underworld.UWClass._RES     = Underworld.UWClass.GAME_UW2;

        // Ensure no stale in-memory blocks influence the round-trip.
        Underworld.scd.scd_data = null;

        byte[] original = File.ReadAllBytes(TestData.Uw2Save0("SCD.ARK"));

        byte[] rewritten = ScdArkWriter.Serialize("SAVE0");

        Assert.Equal(original, rewritten);
    }
}
