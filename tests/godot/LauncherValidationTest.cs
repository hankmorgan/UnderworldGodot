using System.IO;
using Godot;
using Underworld;

/// <summary>
/// Drives the launcher's rejection path directly, because nothing else does.
///
/// No test referenced LaunchMenu at all, so the constraint this task exists to enforce (a
/// refused installation must not be persisted) rested entirely on reading the code. Four
/// mutations would have compiled and passed the whole suite: moving Save() back above the
/// check, dropping the return in the failure branch, inverting the condition, and validating
/// the wrong path.
///
/// The wrong-path mutation is why this selects UW2 with a bad pathuw2 while pathuw1 stays
/// good. Validating pathuw1 instead of BasePath would then wrongly succeed, and only a case
/// where the two differ can tell.
///
/// WARNING to anyone mutation-testing the launcher with this. On correct code nothing is
/// written: the install is refused before Save() is reached. A mutation that breaks the
/// refusal makes the launcher persist the deliberately bogus path into the REAL
/// user://settings.json, and if that run then dies before the restore below, the damage
/// outlives it and every other font test starts failing with a path that no longer resolves.
/// That happened. Back the file up in the shell around any mutation run, because a process
/// that crashes cannot clean up after itself.
/// </summary>
public partial class LauncherValidationTest : Node
{
    private int _failures;
    private void Fail(string m) { _failures++; GD.PrintErr("FAIL " + m); }

    public override void _Ready() => Callable.From(Run).CallDeferred();

    private async void Run()
    {
        _ = uwsettings.instance;
        string settingsPath = ProjectSettings.GlobalizePath("user://settings.json");
        string savedFile = File.Exists(settingsPath) ? File.ReadAllText(settingsPath) : null;
        string savedUw2 = uwsettings.instance.pathuw2;
        string savedGame = uwsettings.instance.gametoload;
        byte savedRes = UWClass._RES;
        string savedBase = UWClass.BasePath;

        try
        {
            var menu = GD.Load<PackedScene>("res://scenes/Launch.tscn").Instantiate();
            GetTree().Root.AddChild(menu);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            var lm = menu as LaunchMenu;
            if (lm == null) { Fail("Launch.tscn root is not a LaunchMenu"); goto done; }
            if (lm.LaunchError == null) { Fail("LaunchError is not wired up in the scene"); goto done; }

            // pathuw1 stays valid; only the UW2 path is broken.
            uwsettings.instance.pathuw2 = "/tmp/no-such-install-" + System.Guid.NewGuid().ToString("N");

            var ev = new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true };
            lm.OnLaunchInput(ev, UWClass.GAME_UW2);

            // Assert BEFORE yielding a frame. Save() is synchronous, so a mutation that
            // persists a refused install has already written the file by now, whereas
            // ChangeSceneToPacked is deferred to the end of the frame. Waiting would let a
            // broken launcher tear this test down mid-run, which the harness would report as
            // a missing verdict: a real failure signal, but a crash rather than an assertion.
            if (string.IsNullOrEmpty(lm.LaunchError.Text))
                Fail("a rejected installation showed no reason to the player");
            else
                GD.Print($"  reported: {lm.LaunchError.Text}");

            string now = File.Exists(settingsPath) ? File.ReadAllText(settingsPath) : null;
            if (now != savedFile)
                Fail("a rejected installation was written to settings.json");

            // A refused install must not have started the handover either. ChangeSceneToPacked
            // is deferred, so a pending change is visible as the game scene being loaded.
            if (ResourceLoader.HasCached("res://scenes/Underworld.tscn")
                && GetTree().CurrentScene != null
                && GetTree().CurrentScene.SceneFilePath.EndsWith("Underworld.tscn"))
                Fail("the launcher handed over to the game despite refusing the install");
        }
        catch (System.Exception ex) { Fail(ex.ToString()); }

    done:
        uwsettings.instance.pathuw2 = savedUw2;
        uwsettings.instance.gametoload = savedGame;
        UWClass._RES = savedRes;
        UWClass.BasePath = savedBase;
        if (savedFile != null) File.WriteAllText(settingsPath, savedFile);

        GD.Print(_failures == 0 ? "GODOT-TEST-VERDICT PASS" : $"GODOT-TEST-VERDICT FAIL {_failures}");
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }
}
