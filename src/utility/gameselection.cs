namespace Underworld;

/// <summary>
/// Turns the <c>gametoload</c> setting into the game it names.
///
/// Godot-free so it can be unit tested. <see cref="uwsettings"/> resolves its path through
/// <c>ProjectSettings.GlobalizePath</c>, and touching that class at all runs its static
/// constructor, which a headless test process cannot do.
/// </summary>
public static class GameSelection
{
    /// <summary>
    /// Resolve <paramref name="gametoload"/> to a game. Returns false for a value naming no
    /// game, including null, rather than throwing.
    ///
    /// This used to throw out of <c>LoadSettings</c>, which runs from a static constructor, so
    /// a settings file that parsed but named nothing recognisable faulted the type for the rest
    /// of the process and the game could not start. A file being unusable is the same problem
    /// as a file being unreadable and gets the same treatment.
    /// </summary>
    public static bool TryResolve(string gametoload, out byte res)
    {
        res = UWClass.GAME_UW1;
        if (gametoload == null) { return false; }

        switch (gametoload.ToUpper())
        {
            case "UW2":
            case "2":
                res = UWClass.GAME_UW2;
                return true;
            case "UW1":
            case "1":
                res = UWClass.GAME_UW1;
                return true;
            case "UWDEMO":
            case "0":
                res = UWClass.GAME_UWDEMO;
                return true;
            default:
                return false;
        }
    }
}
