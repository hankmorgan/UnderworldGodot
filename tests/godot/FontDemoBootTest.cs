using Godot;
using Underworld;

/// <summary>
/// Boots the real scene in demo mode against a valid installation and asserts the fonts are
/// published like any other game.
///
/// This test used to assert the opposite. The demo was refused outright, because nobody had
/// compared its .SYS fonts to the parser and a record count says nothing about bitmap layout.
/// That reason went away: the provider now identifies a font by charsize, height and
/// rowbytes, and it checks whatever the player actually has, which was the point of building
/// fonts from the player's own data in the first place.
///
/// Refusing on sight also removed a configuration this port supports in eight other places,
/// and stranded players who never chose the demo: LaunchMenu classifies any UW1 folder
/// holding UWDEMO.EXE as demo by itself.
///
/// It still boots the scene DIRECTLY rather than through the launcher, because config.cs
/// reads gametoload from settings.json and that path bypasses the launcher entirely.
/// </summary>
public partial class FontDemoBootTest : Node
{
    private int _failures;
    private void Fail(string m) { _failures++; GD.PrintErr("FAIL " + m); }

    public override void _Ready() => Callable.From(Run).CallDeferred();

    private async void Run()
    {
        _ = uwsettings.instance;
        byte savedRes = UWClass._RES;
        string savedPath = UWClass.BasePath;

        try
        {
            string dataDir = System.IO.Path.Combine(UWClass.BasePath ?? "", "DATA");
            foreach (string f in SysFontProvider.FontNames)
            {
                if (!System.IO.File.Exists(System.IO.Path.Combine(dataDir, f + ".SYS")))
                {
                    GD.Print("GODOT-TEST-VERDICT SKIP no game data present");
                    GetTree().Quit(0); return;
                }
            }

            UWClass._RES = UWClass.GAME_UWDEMO;   // a real installation, flagged as the demo

            var root = GD.Load<PackedScene>("res://scenes/Underworld.tscn").Instantiate();
            GetTree().Root.AddChild(root);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            if (root is not UnderworldRoot uwRoot)
            {
                Fail("the scene root is not an UnderworldRoot");
            }
            else
            {
                GD.Print($"  FontsReady: {uwRoot.FontsReady}   FontError: {uwRoot.FontError ?? "none"}");
                if (!uwRoot.FontsReady)
                {
                    Fail($"demo mode was refused even though the fonts are usable: {uwRoot.FontError}");
                }
            }
        }
        catch (System.Exception ex) { Fail(ex.ToString()); }
        finally { UWClass._RES = savedRes; UWClass.BasePath = savedPath; }

        GD.Print(_failures == 0 ? "GODOT-TEST-VERDICT PASS" : $"GODOT-TEST-VERDICT FAIL {_failures}");
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }
}
