using System.IO;
using Godot;
using Underworld;

/// <summary>
/// Pins where the save-description block cursor sits against the text beside it.
///
/// Nothing did, and the cost was three rounds of changing a constant and asking a person to
/// look. The cursor is an inline image whose position depends on the font's ascent, so the
/// runtime fonts moved it: they declare 44 at font size 64 where the converted TTFs declared
/// 43. It is pinned relative to the glyph, not in absolute rows, so moving the label cannot
/// break the test.
///
/// Two things worth knowing before changing any of this.
///
/// `[img=WxH]` SCALES the image; it does not offset it. Changing the height constant alone
/// stretches the block instead of moving it, which looks exactly like nothing happening.
///
/// Godot 4.3's `[img]` alignment keywords do not give fine control here. All twelve
/// combinations were measured and they produce only two positions, both further from the
/// character cell than the padded image, and the first keyword has no effect at all. The
/// transparent padding in the PNG is the only tunable mechanism, and its effect is not
/// linear: four more rows moved the block one pixel and changed its height.
/// </summary>
public partial class CursorPlacementTest : Node2D
{
    private RichTextLabel _rtl;
    private int _failures;
    private void Fail(string m) { _failures++; GD.PrintErr("FAIL " + m); }

    // Measured with the shipped 20x43 image against FONT5X6P at size 64.
    private const int ExpectedTopVsGlyphTop = -3;
    private const int ExpectedBottomVsGlyphBottom = 1;

    public override void _Ready()
    {
        _ = uwsettings.instance;
        var bg = new ColorRect { Color = Colors.Black, Size = new Vector2(800, 220) };
        AddChild(bg);
        _rtl = new RichTextLabel
        {
            BbcodeEnabled = true, ScrollActive = false,
            Position = new Vector2(10, 10), Size = new Vector2(760, 200),
        };
        _rtl.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        _rtl.AddThemeFontSizeOverride("normal_font_size", 64);
        _rtl.AddThemeConstantOverride("line_separation", -25);   // as scenes/Underworld.tscn ships
        _rtl.AddThemeColorOverride("default_color", Colors.White);
        AddChild(_rtl);
        Callable.From(Run).CallDeferred();
    }

    private async System.Threading.Tasks.Task<(int gT, int gB, int cT, int cB)> Measure(string bb)
    {
        _rtl.Text = bb;
        for (int i = 0; i < 4; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        var img = GetViewport().GetTexture().GetImage();
        int gT = int.MaxValue, gB = int.MinValue, cT = int.MaxValue, cB = int.MinValue;
        for (int y = 0; y < 200; y++)
            for (int x = 0; x < 700; x++)
            {
                var p = img.GetPixel(x, y);
                if (p.A < 0.5f) continue;
                bool white = p.R > 0.7f && p.G > 0.7f && p.B > 0.7f;
                // The label renders sRGB into a linear buffer, so the image's (136,62,20)
                // arrives near (62,12,1). Detect "neither black nor white" rather than a value.
                bool black = p.R < 0.02f && p.G < 0.02f && p.B < 0.02f;
                if (white) { if (y < gT) gT = y; if (y > gB) gB = y; }
                else if (!black) { if (y < cT) cT = y; if (y > cB) cB = y; }
            }
        return (gT, gB, cT, cB);
    }

    private async void Run()
    {
        string dir = string.IsNullOrWhiteSpace(UWClass.BasePath)
            ? null : Path.Combine(UWClass.BasePath, "DATA");
        if (dir == null || !File.Exists(Path.Combine(dir, "FONT5X6P.SYS")))
        {
            GD.Print("GODOT-TEST-VERDICT SKIP no game data present");
            GetTree().Quit(0); return;
        }
        var sys = SysFont.Parse(File.ReadAllBytes(Path.Combine(dir, "FONT5X6P.SYS")));
        var (a, d) = SysFontProvider.LegacyMetricsFor("FONT5X6P");
        _rtl.AddThemeFontOverride("normal_font", SysFontBuilder.Build(sys, a, d));

        var m = await Measure("T[img=20x43]res://resources/textcursor.png[/img]");
        if (m.gT == int.MaxValue) { Fail("no glyph rendered"); }
        else if (m.cT == int.MaxValue) { Fail("no cursor rendered"); }
        else
        {
            int top = m.cT - m.gT, bot = m.cB - m.gB;
            GD.Print($"  glyph {m.gT}..{m.gB}  cursor {m.cT}..{m.cB}  "
                   + $"topOffset={top} (want {ExpectedTopVsGlyphTop})  "
                   + $"bottomOffset={bot} (want {ExpectedBottomVsGlyphBottom})");
            if (top != ExpectedTopVsGlyphTop) Fail($"cursor top offset {top}, expected {ExpectedTopVsGlyphTop}");
            if (bot != ExpectedBottomVsGlyphBottom) Fail($"cursor bottom offset {bot}, expected {ExpectedBottomVsGlyphBottom}");
        }

        GD.Print(_failures == 0 ? "GODOT-TEST-VERDICT PASS" : $"GODOT-TEST-VERDICT FAIL {_failures}");
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }
}
