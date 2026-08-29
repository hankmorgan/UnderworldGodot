using System.IO;
using Xunit;

namespace Underworld.Save.Tests;

/// <summary>
/// Where the shipped game data lives, and whether it is here.
///
/// xUnit 2.x has no runtime skip, so the decision is made at discovery: these attributes
/// set Skip when the data is absent and the runner reports the test as skipped.
///
/// The alternative, an early `return` inside the test body, reports PASS whether or not
/// anything ran. Several tests here did exactly that, so a run with no game data looked
/// identical to a run that proved something.
/// </summary>
public static class GameData
{
    public static string Uw1Data => Path.Combine(TestData.UW2GogRoot, "UW1", "DATA");
    public static string Uw2Data => Path.Combine(TestData.UW2GogRoot, "UW2", "DATA");

    public static bool Present =>
        Directory.Exists(Uw1Data) || Directory.Exists(Uw2Data);

    internal const string Absent =
        "the shipped game data is not present; this test needs it and cannot run here";
}

/// <summary>A Fact that skips, visibly, when the shipped game data is absent.</summary>
public sealed class GameDataFactAttribute : FactAttribute
{
    public GameDataFactAttribute()
    {
        if (!GameData.Present) Skip = GameData.Absent;
    }
}

/// <summary>A Theory that skips, visibly, when the shipped game data is absent.</summary>
public sealed class GameDataTheoryAttribute : TheoryAttribute
{
    public GameDataTheoryAttribute()
    {
        if (!GameData.Present) Skip = GameData.Absent;
    }
}
