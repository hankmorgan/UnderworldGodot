namespace Underworld.Save.Tests;

/// <summary>
/// Covers <see cref="GameSelection.TryResolve"/>, which decides what game the gametoload
/// setting names.
///
/// It used to be a switch inside LoadSettings whose default threw. LoadSettings runs from a
/// static constructor, so a settings file that parsed but named no game faulted the type for
/// the rest of the process: every later read of uwsettings.instance threw and the game could
/// not start, with nothing on screen to say why. Verified against a real build before the
/// change, with gametoload set to null and to a nonsense string, and the launcher was never
/// reached in either case.
/// </summary>
public class GameSelectionTests
{
    [Theory]
    [InlineData("UW1")]
    [InlineData("uw1")]
    [InlineData("1")]
    public void TheFirstGameIsRecognisedByNameAndByNumber(string value)
    {
        Assert.True(GameSelection.TryResolve(value, out byte res));
        Assert.Equal(UWClass.GAME_UW1, res);
    }

    [Theory]
    [InlineData("UW2")]
    [InlineData("uw2")]
    [InlineData("2")]
    public void TheSecondGameIsRecognisedByNameAndByNumber(string value)
    {
        Assert.True(GameSelection.TryResolve(value, out byte res));
        Assert.Equal(UWClass.GAME_UW2, res);
    }

    [Theory]
    [InlineData("UWDEMO")]
    [InlineData("uwdemo")]
    [InlineData("0")]
    public void TheDemoIsRecognisedByNameAndByNumber(string value)
    {
        Assert.True(GameSelection.TryResolve(value, out byte res));
        Assert.Equal(UWClass.GAME_UWDEMO, res);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("banana")]
    [InlineData("UW3")]
    [InlineData("3")]
    public void AValueNamingNoGameIsRefusedRatherThanThrown(string value)
    {
        // Null is the case that matters most: a settings file holding "gametoload": null parses
        // perfectly well, and calling ToUpper on it threw a NullReferenceException out of the
        // static constructor.
        Assert.False(GameSelection.TryResolve(value, out byte res));
        Assert.Equal(UWClass.GAME_UW1, res);
    }

    [Fact]
    public void TheValueUsedAsTheFallbackResolves()
    {
        // LoadSettings falls back to a fresh uwsettings when the file names no game, and then
        // resolves that. If this stopped resolving, the fallback would be selecting a game
        // nobody asked for, so LoadSettings reports it rather than carrying on quietly.
        //
        // Spelled out rather than read from uwsettings.gametoload: constructing that class runs
        // its static constructor, which calls into Godot and takes a headless test host down
        // with it. If the default in config.cs ever changes, change it here too.
        Assert.True(GameSelection.TryResolve("UW1", out byte res));
        Assert.Equal(UWClass.GAME_UW1, res);
    }
}
