using System;
using System.IO;
using Godot;
using Underworld;

/// <summary>
/// The gate for the whole approach.
///
/// Godot instantiates a whole PackedScene, runs constructors and assigns exported properties
/// BEFORE adding the root to the tree. A Control could therefore shape its text before the
/// root's _EnterTree publishes the fonts, and would then hold metrics from an empty font for
/// the rest of the session. Nothing in the lifecycle rules forbids it. This test is the only
/// thing that answers whether it happens in this scene.
///
/// It asserts EXACT widths derived from each glyph's declared width byte. A tolerance of
/// "greater than zero" would be useless, because an unpopulated FontFile does not measure
/// zero: measured on 4.3, "AB" at size 64 gives 64 with system fallback off and 85 with it
/// on, against 40 when correctly populated. Both wrong answers look plausible. Only the
/// exact figure separates them.
///
/// If it fails, do not adjust it. It means the scene must be restructured so fonts are
/// published before any Control is instantiated.
/// </summary>
public partial class FontBootTest : Node
{
    private int _failures;

    /// <summary>
    /// True when the shipped .SYS fonts are present. CI has no game data, so a test that
    /// needs it reports SKIP rather than failing: a red CI that everyone learns to ignore
    /// protects nothing.
    /// </summary>
    private static bool GameDataPresent(out string why)
    {
        string dir = string.IsNullOrWhiteSpace(UWClass.BasePath)
            ? null : System.IO.Path.Combine(UWClass.BasePath, "DATA");
        if (dir == null || !System.IO.Directory.Exists(dir))
        {
            why = "no game data directory is configured";
            return false;
        }
        foreach (string n in SysFontProvider.FontNames)
        {
            if (!System.IO.File.Exists(System.IO.Path.Combine(dir, n + ".SYS")))
            {
                why = $"{n}.SYS is not present under {dir}";
                return false;
            }
        }
        why = null;
        return true;
    }


    /// <summary>Labels that must exist and must measure correctly. Missing is a failure.</summary>
    private static readonly (string Path, string Font, int Size)[] Mandatory =
    {
        ("UI/Common/StatsDisplay/CharLevel", "FONT5X6I", 64),
        ("UI/Common/StatsDisplay/VIT",       "FONT5X6I", 64),
    };

    public override void _Ready() => Callable.From(Run).CallDeferred();

    /// <summary>
    /// Failures carry a distinct marker per kind. Step 7's negative control has to tell a
    /// width mismatch apart from a missing node, an uncovered character or a thrown
    /// exception, because all of those also produce a non-zero exit and would otherwise be
    /// accepted as proof that the gate detected stale shaping.
    /// </summary>
    private void Fail(string kind, string msg) { _failures++; GD.PrintErr($"{kind} {msg}"); }

    private async void Run()
    {
        try
        {
            _ = uwsettings.instance;
            if (!GameDataPresent(out string skipWhy))
            {
                GD.Print($"GODOT-TEST-VERDICT SKIP {skipWhy}");
                GetTree().Quit(0); return;
            }

            // Reading the environment here, in the test, rather than in UnderworldRoot:
            // production has no business being switchable by an inherited variable.
            bool control = OS.GetEnvironment("UW_FONT_TEST_CONTROL") == "1";

            var packed = GD.Load<PackedScene>("res://scenes/Underworld.tscn");
            var root = packed.Instantiate();
            // Set BEFORE AddChild. _EnterTree fires during AddChild, and that is where the
            // fonts are published, so afterwards would be too late.
            if (control && root is UnderworldRoot pre) pre.WithholdFontsForTest = true;
            GetTree().Root.AddChild(root);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            if (root is not UnderworldRoot uwRoot)
            {
                GD.PrintErr("FAIL: the scene root is not an UnderworldRoot");
                GetTree().Quit(1); return;
            }
            GD.Print($"fonts ready: {uwRoot.FontsReady}  error: {uwRoot.FontError ?? "none"}");
            // The negative control withholds the fonts on purpose, and needs the run to
            // reach the measurements rather than stop here. Every other route to
            // FontsReady == false is a real failure.
            if (!uwRoot.FontsReady && !control)
            {
                GD.PrintErr("FAIL: fonts did not load, so nothing here proves anything about shaping");
                GetTree().Quit(1); return;
            }

            string dataDir = Path.Combine(UWClass.BasePath, "DATA");
            foreach (var (path, fontName, size) in Mandatory)
            {
                var lbl = root.GetNodeOrNull<RichTextLabel>(path);
                if (lbl == null) { Fail("FAIL-MISSING", path); continue; }

                var sys = SysFont.Parse(File.ReadAllBytes(Path.Combine(dataDir, fontName + ".SYS")));
                string text = lbl.Text;
                int declared = 0;
                bool covered = true;
                foreach (char c in text)
                {
                    if (!sys.Covers(c))
                    {
                        Fail("FAIL-UNCOVERED", $"{path} {fontName} does not cover '{c}'");
                        covered = false;
                        break;
                    }
                    declared += sys.AdvanceOf(c);
                }
                if (!covered) continue;   // never compare a width built from a partial sum

                float expected = declared * (size / (float)SysFontBuilder.DesignSize);
                float actual = lbl.GetContentWidth();
                // One MEASURE line per label that was found AND fully covered. Step 7 counts
                // these to prove the run reached every measurement rather than falling out early.
                GD.Print($"MEASURE {path} expected={expected} actual={actual} text=\"{text}\"");
                if (Math.Abs(actual - expected) > 0.5f)
                {
                    Fail("FAIL-WIDTH", $"{path} measured {actual}, expected {expected}. A label "
                       + "that shaped before the fonts were published would not match.");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr("FAIL: " + ex);
            GetTree().Quit(1); return;
        }

        // Godot 4.3 mono reliably faults on shutdown here with
        //   FATAL: Condition "!rc_owner" is true.  (modules/mono/csharp_script.cpp:1237)
        // after Quit(), so the process hangs and its exit status is whatever killed it. The
        // fault is pre-existing and unrelated to fonts: it shows up in other headless runs in
        // this project. This line is therefore the verdict, not the exit code, and the
        // harness keys off it.
        GD.Print(_failures == 0 ? "GODOT-TEST-VERDICT PASS" : $"GODOT-TEST-VERDICT FAIL {_failures}");
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }
}
