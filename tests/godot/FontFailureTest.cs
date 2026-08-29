using System;
using Godot;
using Underworld;

/// <summary>
/// Boots the real scene with a deliberately wrong game path and asserts the game does not
/// start. Sets UWClass.BasePath directly rather than editing a settings file: config.cs
/// reads user://settings.json, so editing the repository copy would prove nothing.
/// </summary>
public partial class FontFailureTest : Node
{
    public override void _Ready() => Callable.From(Run).CallDeferred();

    private async void Run()
    {
        // Force the settings load BEFORE overriding BasePath. Publish() forces it too, and
        // LoadSettings assigns BasePath from settings.json, so a value set before the static
        // constructor has run would simply be overwritten and the test would quietly load
        // real fonts instead. Whether it has already run depends on what else touched
        // uwsettings first, which is not something a test may rely on.
        _ = uwsettings.instance;

        string saved = UWClass.BasePath;
        byte savedRes = UWClass._RES;
        int failures = 0;
        int startGameCalls = 0;
        // Assigned before the scene is instantiated, because uimanager._Ready fires during
        // AddChild. Assigning afterwards would observe nothing.
        main.StartGameObserver = () => startGameCalls++;
        try
        {
            UWClass.BasePath = "/tmp/definitely-not-a-game-" + Guid.NewGuid().ToString("N");

            var root = GD.Load<PackedScene>("res://scenes/Underworld.tscn").Instantiate();
            GetTree().Root.AddChild(root);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            // The guard has two halves and the scene only ever exercises one. Underworld.tscn's
            // own root IS the UnderworldRoot, so UnderworldRoot.Find never returns null here
            // and the `root == null` half is never reached. Pin its contract directly, so
            // that half rests on a test rather than on reading the code.
            var orphanParent = new Node();
            var orphanChild = new Node();
            orphanParent.AddChild(orphanChild);
            GetTree().Root.AddChild(orphanParent);
            if (UnderworldRoot.Find(orphanChild) != null)
            {
                GD.PrintErr("FAIL: Find returned a root for a node with no UnderworldRoot above it");
                failures++;
            }
            // And the guard's own null branch, which the real scene can never reach because
            // Underworld.tscn's root IS the UnderworldRoot. A uimanager with no such
            // ancestor must refuse by the designed route: print the reason and return. The
            // shell asserts that line, because without it a mutation dropping the null check
            // still refuses, by throwing, and nothing here can tell the two apart.
            var savedUi = uimanager.instance;
            var um = new uimanager();
            orphanParent.AddChild(um);        // _Ready fires on entry
            uimanager.instance = savedUi;     // _Ready set it to the orphan; put it back
            orphanParent.QueueFree();

            // The load-bearing assertion. Everything below it describes the root's state;
            // only this says the game did not start.
            if (startGameCalls != 0)
            {
                GD.PrintErr($"FAIL: StartGame ran {startGameCalls} time(s) with the fonts unloaded");
                failures++;
            }

            var uwRoot = root as UnderworldRoot;
            if (uwRoot == null) { GD.PrintErr("FAIL: root is not an UnderworldRoot"); failures++; }
            else
            {
                GD.Print($"  FontsReady: {uwRoot.FontsReady}");
                GD.Print($"  FontError:  {uwRoot.FontError}");
                if (uwRoot.FontsReady) { GD.PrintErr("FAIL: fonts reported ready with a bad path"); failures++; }
                if (string.IsNullOrEmpty(uwRoot.FontError)) { GD.PrintErr("FAIL: no reason recorded"); failures++; }
                if (uwRoot.FontError != null && !uwRoot.FontError.Contains("definitely-not-a-game"))
                {
                    GD.PrintErr("FAIL: the message does not name the path the user configured");
                    failures++;
                }
                bool sawErrorLabel = false;
                foreach (Node child in uwRoot.GetChildren())
                {
                    if (child is CanvasLayer cl)
                    {
                        foreach (Node g in cl.GetChildren()) if (g is Label) sawErrorLabel = true;
                    }
                }
                if (!sawErrorLabel) { GD.PrintErr("FAIL: no error label was shown"); failures++; }
            }
        }
        catch (Exception ex) { GD.PrintErr("FAIL: " + ex); failures++; }
        finally { UWClass.BasePath = saved; UWClass._RES = savedRes; main.StartGameObserver = null; }

        GD.Print(failures == 0
            ? "GODOT-TEST-VERDICT PASS"
            : $"GODOT-TEST-VERDICT FAIL {failures}");
        GetTree().Quit(failures == 0 ? 0 : 1);
    }
}
