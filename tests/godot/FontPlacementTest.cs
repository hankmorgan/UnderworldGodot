using System.IO;
using Godot;
using Underworld;

/// <summary>
/// Pins where each font's ink sits relative to the baseline.
///
/// Nothing else measures this. FontRenderTest checks the ink pattern and samples at the
/// glyph's actual position, so it follows the glyph wherever it goes. FontBootTest measures
/// width only. The result was that FONTBIG rendered three source pixels too high, twelve at
/// font size 64, across every save slot label, the chargen screens, cutscene subtitles and
/// the automap level number, with every test green.
///
/// The expected rows were measured from the converted TTFs this loader replaces, by drawing
/// at a known baseline and reading back the viewport. They are the compatibility reference
/// and survive those files being deleted.
/// </summary>
public partial class FontPlacementTest : Node2D
{
    private const int Baseline = 60;
    private const int Size = 64;              // 4 screen pixels per source pixel
    private int _failures;
    private FontFile _pen;

    /// <summary>font, glyph, first inked row, last inked row + 1, in source pixels.</summary>
    private static readonly (string Font, char Ch, float Top, float Bottom)[] Expected =
    {
        ("FONT4X5P", 'A',  -3f,  1f),
        ("FONT5X6I", 'A',  -6f, -1f),
        ("FONT5X6P", 'A',  -5f,  0f),
        ("FONTBIG",  'A', -11f,  0f),
    };

    public override void _Ready() => Callable.From(Run).CallDeferred();
    private void Fail(string m) { _failures++; GD.PrintErr("FAIL " + m); }

    private char _ch = 'A';
    public override void _Draw()
    {
        if (_pen == null) return;
        DrawRect(new Rect2(0, 0, 220, 170), Colors.Black);
        _pen.DrawChar(GetCanvasItem(), new Vector2(20, Baseline), _ch, Size, Colors.White);
    }

    private async void Run()
    {
        _ = uwsettings.instance;
        string dir = string.IsNullOrWhiteSpace(UWClass.BasePath)
            ? null : Path.Combine(UWClass.BasePath, "DATA");
        if (dir == null || !Directory.Exists(dir))
        {
            GD.Print("GODOT-TEST-VERDICT SKIP no game data directory is configured");
            GetTree().Quit(0); return;
        }

        foreach (var (name, ch, wantTop, wantBot) in Expected)
        {
            string path = Path.Combine(dir, name + ".SYS");
            if (!File.Exists(path))
            {
                GD.Print($"GODOT-TEST-VERDICT SKIP {name}.SYS is not present");
                GetTree().Quit(0); return;
            }
            var sys = SysFont.Parse(File.ReadAllBytes(path));
            // This commit does not yet have the provider, so the metrics live here. They
            // are measured from the converted TTFs and explained where they land.
            (float a, float d) = name == "FONTBIG" ? (11f, 4f) : (11f, 1f);
            _pen = SysFontBuilder.Build(sys, a, d);
            _ch = ch;
            QueueRedraw();
            for (int i = 0; i < 3; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);

            var img = GetViewport().GetTexture().GetImage();
            int top = int.MaxValue, bot = int.MinValue;
            for (int y = 0; y < 170; y++)
                for (int x = 0; x < 220; x++)
                    if (img.GetPixel(x, y).R > 0.5f) { if (y < top) top = y; if (y > bot) bot = y; }

            if (top == int.MaxValue) { Fail($"{name} '{ch}' drew nothing"); continue; }
            float gotTop = (top - Baseline) / 4f;
            float gotBot = (bot + 1 - Baseline) / 4f;
            GD.Print($"  {name,-9} '{ch}' rows {gotTop}..{gotBot}  expected {wantTop}..{wantBot}");
            if (gotTop != wantTop || gotBot != wantBot)
            {
                Fail($"{name} '{ch}' sits at {gotTop}..{gotBot}, expected {wantTop}..{wantBot}");
            }
        }

        GD.Print(_failures == 0 ? "GODOT-TEST-VERDICT PASS" : $"GODOT-TEST-VERDICT FAIL {_failures}");
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }
}
