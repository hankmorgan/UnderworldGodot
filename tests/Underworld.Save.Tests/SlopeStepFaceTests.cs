using System.IO;
using Xunit;

namespace Underworld.Save.Tests;

/// <summary>
/// DOS does not draw the one-unit step between two neighbouring slopes of the same type
/// (seg032_2E9B adds one to this side's edge when both tiles share a type), which is what
/// makes the walls of the Talorus Escher staircase see-through (issue #180). The staircase
/// is on UW2 level 33 in the shipped data: a ring of slopes at floor height 12 round a
/// lower middle, east slopes along y = 2 from x = 42 to 46.
/// </summary>
[Collection("UWClassState")]
public class SlopeStepFaceTests : System.IDisposable
{
    private readonly string _origBasePath = UWClass.BasePath;
    private readonly byte _origRes = UWClass._RES;
    private readonly byte[] _origLevArkFileData = LevArkLoader.lev_ark_file_data;
    private readonly UWTileMap _origCurrent = UWTileMap.current_tilemap;
    private readonly UWTileMap[] _origDungeons = UWTileMap.dungeons;

    public void Dispose()
    {
        UWClass.BasePath = _origBasePath;
        UWClass._RES = _origRes;
        LevArkLoader.lev_ark_file_data = _origLevArkFileData;
        UWTileMap.current_tilemap = _origCurrent;
        UWTileMap.dungeons = _origDungeons;
    }

    private const int EscherLevel = 32; // level 33

    private static UWTileMap BuildEscherLevel(System.Action<byte[]> edit = null)
    {
        UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, "UW2");
        UWClass._RES = UWClass.GAME_UW2;
        LevArkLoader.LoadLevArkFileData(folder: "DATA");
        UWTileMap.dungeons = new UWTileMap[UWTileMap.NO_OF_LEVELS];
        var tm = new UWTileMap(EscherLevel);
        edit?.Invoke(tm.lev_ark_block.Data);
        UWTileMap.dungeons[EscherLevel] = tm;
        UWTileMap.current_tilemap = tm;
        tm.BuildTileMapUW(levelNo: EscherLevel, tex_ark: tm.tex_ark_block, ovl_ark: tm.ovl_ark_block);
        return tm;
    }

    private static void SetFloor(byte[] data, int x, int y, int height)
    {
        int p = (y * 64 + x) * 4;
        data[p] = (byte)((data[p] & 0x0F) | (height << 4));
    }

    [Fact]
    public void MatchingSlopes_OneUnitStep_IsNotDrawn()
    {
        var tm = BuildEscherLevel();
        Assert.Equal(UWTileMap.TILE_SLOPE_E, tm.Tiles[42, 2].tileType);
        Assert.Equal(UWTileMap.TILE_SLOPE_E, tm.Tiles[43, 2].tileType);

        // (42,2) rises to 13 at its east edge and (43,2) starts at 12: a one-unit step.
        Assert.False(tm.Tiles[42, 2].VisibleFaces[UWTileMap.vEAST]);
        Assert.False(tm.Tiles[43, 2].VisibleFaces[UWTileMap.vWEST]);

        // The same along the north slopes of the ring's east side.
        Assert.Equal(UWTileMap.TILE_SLOPE_N, tm.Tiles[47, 3].tileType);
        Assert.False(tm.Tiles[47, 3].VisibleFaces[UWTileMap.vNORTH]);
        Assert.False(tm.Tiles[47, 4].VisibleFaces[UWTileMap.vSOUTH]);
    }

    [Fact]
    public void MatchingSlopes_LargerStep_IsStillDrawn()
    {
        // Raise (43,2) to 15: its west edge is now three above (42,2)'s east edge of 13,
        // so DOS draws that face, while (42,2)'s face towards the higher tile stays hidden.
        var tm = BuildEscherLevel(data => SetFloor(data, 43, 2, 15));
        Assert.True(tm.Tiles[43, 2].VisibleFaces[UWTileMap.vWEST]);
        Assert.False(tm.Tiles[42, 2].VisibleFaces[UWTileMap.vEAST]);
    }

    [Fact]
    public void DifferentSlopeTypes_AreNotAffected()
    {
        // (46,2) is an east slope and (47,2) a north slope: DOS gives no bonus there, and
        // this rule must leave whatever the port already decided alone.
        var tm = BuildEscherLevel();
        Assert.Equal(UWTileMap.TILE_SLOPE_E, tm.Tiles[46, 2].tileType);
        Assert.Equal(UWTileMap.TILE_SLOPE_N, tm.Tiles[47, 2].tileType);
        Assert.True(tm.Tiles[46, 2].VisibleFaces[UWTileMap.vEAST]);
        Assert.True(tm.Tiles[47, 2].VisibleFaces[UWTileMap.vWEST]);
    }
}
