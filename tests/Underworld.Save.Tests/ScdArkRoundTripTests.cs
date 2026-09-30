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

    [Fact]
    public void Uw2Scd_ShrunkLiveBlock_AvailableSpaceMatchesItsLength()
    {
        // A live SCD block shrinks as its events are consumed. DOS overwrites a block that
        // has no slack only when the new length equals the available space, so a stale
        // larger figure from the source would describe space the file does not hold.
        Underworld.UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW2");
        Underworld.UWClass._RES     = Underworld.UWClass.GAME_UW2;

        byte[] source = File.ReadAllBytes(TestData.Uw2Save0("SCD.ARK"));
        const int n = 16;
        int sourceLen0 = (int)Underworld.Loader.getAt(source, 6 + n * 8, 32);
        Assert.True(sourceLen0 > 16, "block 0 is too short to shrink");

        Underworld.scd.scd_data = new Underworld.UWBlock[n];
        Underworld.scd.scd_data[0] = new Underworld.UWBlock
        {
            Data = new byte[sourceLen0 - 16],
            DataLen = sourceLen0 - 16,
        };

        byte[] rewritten = ScdArkWriter.Serialize("SAVE0");

        for (int i = 0; i < n; i++)
        {
            int len = (int)Underworld.Loader.getAt(rewritten, 6 + n * 8 + i * 4, 32);
            int avail = (int)Underworld.Loader.getAt(rewritten, 6 + n * 12 + i * 4, 32);
            Assert.Equal(len, avail);
        }
        Assert.Equal(sourceLen0 - 16, (int)Underworld.Loader.getAt(rewritten, 6 + n * 8, 32));
    }
}
