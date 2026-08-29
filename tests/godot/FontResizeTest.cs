using System.IO;
using Godot;
using Underworld;

/// <summary>
/// Pins that the runtime-built fonts survive a window resize.
///
/// They did not. With window/stretch/scale_mode absent, Godot 4.3 EMPTIES a manually
/// populated FontFile's glyph cache when the window reaches a non-integer scale: measured,
/// the (16,0) entry went from 127 glyphs to 0. The data is written by code and has no source
/// to re-rasterise from, so it is simply lost, and with AllowSystemFallback off every
/// character drew as a missing-glyph box. Re-populating afterwards restores the glyphs but
/// NOT the rendering, because the shaped text stays cached; a theme-changed notification
/// across the whole tree and a full text reset both failed to recover it. So there is no
/// repair path, only prevention.
///
/// gui/fonts/dynamic_fonts/use_oversampling=false prevents it, by stopping Godot driving
/// the oversampling change that triggers the invalidation. Delete that setting and this
/// test fails, which is the point of it.
///
/// window/stretch/scale_mode="integer" also works and was measured, but it avoids one
/// trigger rather than the mechanism, and it letterboxes. It is NOT what ships.
/// </summary>
public partial class FontResizeTest : Node2D
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


    public override void _Ready() => Callable.From(Run).CallDeferred();

    private void Fail(string msg) { _failures++; GD.PrintErr("FAIL " + msg); }

    private async System.Threading.Tasks.Task Settle()
    {
        for (int i = 0; i < 6; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
    }

    private async void Run()
    {
        try
        {
            _ = uwsettings.instance;
            _ = uwsettings.instance;
            if (!GameDataPresent(out string skipWhy))
            {
                GD.Print($"GODOT-TEST-VERDICT SKIP {skipWhy}");
                GetTree().Quit(0); return;
            }

            string dataDir = Path.Combine(UWClass.BasePath, "DATA");
            var fonts = new System.Collections.Generic.Dictionary<string, FontFile>();
            var before = new System.Collections.Generic.Dictionary<string, Vector2>();

            foreach (string name in SysFontProvider.FontNames)
            {
                var sys = SysFont.Parse(File.ReadAllBytes(Path.Combine(dataDir, name + ".SYS")));
                var (a, d) = SysFontProvider.LegacyMetricsFor(name);
                var f = SysFontBuilder.Build(sys, a, d);
                fonts[name] = f;
                before[name] = f.GetStringSize("35 1 255", HorizontalAlignment.Left, -1, 64);
                int n = f.GetGlyphList(0, new Vector2I(SysFontBuilder.DesignSize, 0)).Length;
                GD.Print($"  {name}: {n} glyphs, string {before[name]}");
                if (n != sys.GlyphCount) Fail($"{name} started with {n} glyphs, expected {sys.GlyphCount}");
            }

            // Every transition that can change the engine's effective font oversampling,
            // not just the one that was reported. A fullscreen round trip and a return to
            // the base size catch one-way recovery bugs that a single resize would miss.
            async System.Threading.Tasks.Task Check(string stage)
            {
                await Settle();
                foreach (string name in SysFontProvider.FontNames)
                {
                    var f = fonts[name];
                    int n = f.GetGlyphList(0, new Vector2I(SysFontBuilder.DesignSize, 0)).Length;
                    var now = f.GetStringSize("35 1 255", HorizontalAlignment.Left, -1, 64);
                    GD.Print($"  [{stage}] {name}: {n} glyphs, string {now}");
                    if (n == 0) Fail($"{stage}: {name} lost every glyph");
                    else if (!f.HasChar('3')) Fail($"{stage}: {name} no longer has '3'");
                    else if (now != before[name]) Fail($"{stage}: {name} string size {before[name]} -> {now}");
                }
            }

            // 1330x831 against a 1280x800 base is deliberately not a whole multiple.
            DisplayServer.WindowSetSize(new Vector2I(1330, 831));
            await Check("fractional resize");

            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            await Check("fullscreen");

            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            DisplayServer.WindowSetSize(new Vector2I(1280, 800));
            await Check("back to base");
        }
        catch (System.Exception ex) { Fail(ex.ToString()); }

        GD.Print(_failures == 0 ? "GODOT-TEST-VERDICT PASS" : $"GODOT-TEST-VERDICT FAIL {_failures}");
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }
}
