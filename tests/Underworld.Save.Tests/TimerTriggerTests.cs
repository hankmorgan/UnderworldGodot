using System.IO;
using Xunit;

namespace Underworld.Save.Tests;

/// <summary>
/// UW2 timer triggers are a list that ends at the first zero, as DOS counts them when it
/// loads a level (LoadAnimationOverlays_ovr128_271). Removing one follows DOS's
/// MoveObjectIndexInArrayToNewPositionTimerRelated_seg044_1094: the last entry moves into
/// the removed one's place, so the list stays packed.
/// </summary>
[Collection("UWClassState")]
public class TimerTriggerTests : System.IDisposable
{
    private readonly string _origBasePath = UWClass.BasePath;
    private readonly byte _origRes = UWClass._RES;
    private readonly byte[] _origLevArkFileData = LevArkLoader.lev_ark_file_data;
    private readonly UWTileMap _origCurrent = UWTileMap.current_tilemap;

    public void Dispose()
    {
        UWClass.BasePath = _origBasePath;
        UWClass._RES = _origRes;
        LevArkLoader.lev_ark_file_data = _origLevArkFileData;
        UWTileMap.current_tilemap = _origCurrent;
    }

    private static void LoadUw2Level0()
    {
        UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW2");
        UWClass._RES = UWClass.GAME_UW2;
        LevArkLoader.LoadLevArkFileData(folder: "SAVE0");
        UWTileMap.current_tilemap = new UWTileMap(0);
        for (int t = 0; t < 64; t++) timers.SetTimer(t, 0);
    }

    [Fact]
    public void RemoveTimer_MovesTheLastEntryIntoThePlaceOfTheRemovedOne()
    {
        LoadUw2Level0();
        timers.SetTimer(0, 500);
        timers.SetTimer(1, 600);
        timers.SetTimer(2, 700);
        timers.SetTimer(3, 800);

        Assert.True(timers.RemoveTimer(600));

        Assert.Equal(3, timers.NoOfTimerTriggers);
        Assert.Equal(500, timers.GetTimer(0));
        Assert.Equal(800, timers.GetTimer(1));
        Assert.Equal(700, timers.GetTimer(2));
        Assert.Equal(0, timers.GetTimer(3));
    }

    [Fact]
    public void RemoveTimer_LastEntryAndUnknownObject()
    {
        LoadUw2Level0();
        timers.SetTimer(0, 500);
        timers.SetTimer(1, 600);

        Assert.True(timers.RemoveTimer(600));
        Assert.Equal(1, timers.NoOfTimerTriggers);
        Assert.Equal(0, timers.GetTimer(1));

        Assert.False(timers.RemoveTimer(999));
        Assert.Equal(1, timers.NoOfTimerTriggers);
    }
}
