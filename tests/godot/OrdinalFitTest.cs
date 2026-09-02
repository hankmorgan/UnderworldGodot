using System.IO;
using System.Text.RegularExpressions;
using Godot;
using Underworld;

/// <summary>
/// Measures whether the stats panel strings fit their boxes, with the real shaper.
///
/// Two backlog items say they do not: "Stats display sometimes cuts off hp numbers. Eg 56/78
/// hp renders as 56/7" and "ordinal is cut off when player level >10 eg 11th is 11t". Both
/// predate the runtime fonts, which take each advance from the glyph's declared width byte
/// where the converted TTFs derived it from the ink and came out too wide.
///
/// Measured here rather than computed, because a hand parser has been wrong about this before.
/// </summary>
public partial class OrdinalFitTest : Node2D
{
    private const int Size = 64;              // 4 screen pixels per source pixel
    private int _failures;

    /// <summary>node, and the strings it has to hold. Widths come from the scene.</summary>
    private static readonly (string Node, string[] Text)[] Cases =
    {
        // DOS saves a 35 source pixel background rectangle at x 277 for the three numeric
        // rows and right aligns each value to source x 311. At four screen pixels per source
        // pixel that is 140, and the port had 120, which clipped 100/100. See issue #92.
        ("VIT",       new[] { "56/78", "100/100" }),
        ("MANA",      new[] { "100/100" }),
        ("EXP",       new[] { "999999" }),
        ("CharLevel", new[] { "10TH", "11TH", "12TH", "13TH", "14TH", "15TH", "16TH" }),
    };

    private const string ScenePath = "res://scenes/Underworld.tscn";

    /// <summary>
    /// The node's width, read from the scene rather than restated here.
    ///
    /// Hard coding the widths made this test pass against a scene that still clipped: it
    /// measured strings against numbers in its own source, so reverting the scene would not
    /// have failed it. Reading the scene is what makes it pin the fix.
    ///
    /// The scene text is parsed rather than instantiated, because instantiating
    /// Underworld.tscn starts the game.
    /// </summary>
    private static float BoxWidth(string sceneText, string node)
    {
        var m = Regex.Match(
            sceneText,
            "\\[node name=\"" + Regex.Escape(node) + "\" type=\"RichTextLabel\"[^\\]]*\\]\\n(?<body>(?:(?!\\[node)[\\s\\S])*)");
        if (!m.Success) { return float.NaN; }
        string body = m.Groups["body"].Value;
        var l = Regex.Match(body, "^offset_left = (-?[0-9.]+)$", RegexOptions.Multiline);
        var r = Regex.Match(body, "^offset_right = (-?[0-9.]+)$", RegexOptions.Multiline);
        if (!l.Success || !r.Success) { return float.NaN; }
        return float.Parse(r.Groups[1].Value) - float.Parse(l.Groups[1].Value);
    }

    public override void _Ready() => Callable.From(Run).CallDeferred();
    private void Fail(string m) { _failures++; GD.PrintErr("FAIL " + m); }

    private void Run()
    {
        _ = uwsettings.instance;
        string dir = string.IsNullOrWhiteSpace(UWClass.BasePath)
            ? null : Path.Combine(UWClass.BasePath, "DATA");
        string sysPath = dir == null ? null : Path.Combine(dir, "FONT5X6I.SYS");
        if (sysPath == null || !File.Exists(sysPath))
        {
            GD.Print("GODOT-TEST-VERDICT SKIP FONT5X6I.SYS is not present");
            GetTree().Quit(0); return;
        }

        var sys = SysFont.Parse(File.ReadAllBytes(sysPath));
        var (a, d) = SysFontProvider.LegacyMetricsFor("FONT5X6I");
        FontFile runtime = SysFontBuilder.Build(sys, a, d);
        var legacy = GD.Load<FontFile>("res://resources/fonts/FONT5X6I.ttf");

        using var scene = Godot.FileAccess.Open(ScenePath, Godot.FileAccess.ModeFlags.Read);
        if (scene == null)
        {
            GD.Print("GODOT-TEST-VERDICT FAIL cannot read " + ScenePath);
            GetTree().Quit(1); return;
        }
        string sceneText = scene.GetAsText();

        foreach (var (node, texts) in Cases)
        {
            float box = BoxWidth(sceneText, node);
            if (float.IsNaN(box)) { Fail($"{node} has no offsets in the scene"); continue; }

            foreach (string t in texts)
            {
                float now = runtime.GetStringSize(t, HorizontalAlignment.Left, -1, Size).X;
                float was = legacy == null ? float.NaN
                          : legacy.GetStringSize(t, HorizontalAlignment.Left, -1, Size).X;
                GD.Print($"  {node,-9} {t,-8} scene box {box,5:0} runtime {now,6:0.0} ({now / 4:0.0} src) "
                       + $"legacy {was,6:0.0} ({was / 4:0.0} src)  {(now <= box ? "fits" : "OVERFLOWS")}");
                if (now > box) { Fail($"{node} '{t}' needs {now:0.0} of the scene's {box:0}"); }
            }
        }

        GD.Print(_failures == 0 ? "GODOT-TEST-VERDICT PASS" : $"GODOT-TEST-VERDICT FAIL {_failures}");
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }
}
