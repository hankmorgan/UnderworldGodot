using System;
using System.IO;
using Godot;
using Underworld;

/// <summary>
/// Renders glyphs through the PRODUCTION path — SysFont.Parse then
/// SysFontBuilder.Build — and compares each pixel against an oracle that reads the
/// raw .SYS bytes inline. The oracle deliberately does not use SysFont: if it did,
/// a misreading of the format would agree with itself and prove nothing.
///
/// Needs a real rendering driver. Under --headless the RenderingServer returns
/// dummy values, so the readback is meaningless and this fails rather than skips.
/// </summary>
public partial class FontRenderTest : Control
{
    private const int RenderSize = 64;                  // 4x the design size
    private static readonly Vector2 Pen = new(20, 200); // baseline origin

    private int _failures;
    private int _tested;
    private FontFile _font;
    private byte[] _raw;
    private int _charSize, _height, _rowBytes, _cell;
    private char _probe;

    /// <summary>Oracle: is this source pixel inked? Reads the bytes directly.</summary>
    private bool OracleInk(int codepoint, int row, int col)
    {
        int at = 12 + codepoint * (_charSize + 1) + row * _rowBytes + (col >> 3);
        return (_raw[at] & (0x80 >> (col & 7))) != 0;
    }

    private int OracleAdvance(int codepoint) =>
        _raw[12 + codepoint * (_charSize + 1) + _charSize];

    private void Fail(string msg)
    {
        _failures++;
        GD.Print("  FAIL " + msg);
    }

    public override void _Ready()
    {
        GD.Print("font render test on " + Engine.GetVersionInfo()["string"]);
        // Not CallDeferred(nameof(RunAll)): name-based dispatch cannot reach a private
        // C# method, fails silently, and the process then hangs with nothing run.
        Callable.From(RunAll).CallDeferred();
    }

    private async void RunAll()
    {
        try
        {
            foreach (var target in Targets())
            {
                if (!File.Exists(target.Item2))
                {
                    GD.Print("  skip " + target.Item1 + " (absent)");
                    continue;
                }
                await Check(target.Item1, target.Item2, target.Item3);
            }

            if (_tested == 0)
            {
                GD.PrintErr("no font was tested: the render job proves nothing, so it fails");
                GD.Print("GODOT-TEST-VERDICT FAIL nothing-tested");
                GetTree().Quit(1);
                return;
            }
            if (_failures > 0)
            {
                GD.PrintErr(_failures + " failure(s) across " + _tested + " font(s)");
                GD.Print("GODOT-TEST-VERDICT FAIL " + _failures);
                GetTree().Quit(1);
                return;
            }
            GD.Print(_tested + " font(s) render to their .SYS bitmaps");
            // The verdict is this line, not the exit status: Godot 4.3 mono faults on
            // shutdown after Quit() and the process hangs, so whatever killed it sets the
            // status. Every headless test in this repo prints the same marker.
            GD.Print("GODOT-TEST-VERDICT PASS");
            GetTree().Quit(0);
        }
        catch (Exception ex)
        {
            // async void has no caller to observe a fault: without this, an
            // exception here logs and then hangs forever with the process never
            // exiting, which is worse than any failure this test could report.
            GD.PrintErr(ex.ToString());
            GD.Print("GODOT-TEST-VERDICT FAIL exception");
            GetTree().Quit(1);
        }
    }

    /// <summary>The committed fixture always runs; the shipped fonts run when present.</summary>
    private (string, string, char)[] Targets()
    {
        string repo = ProjectSettings.GlobalizePath("res://");
        string data = OS.GetEnvironment("UW1_DATA");
        if (string.IsNullOrEmpty(data))
        {
            data = Path.Combine(OS.GetEnvironment("HOME"), "UWGOG", "UW1", "DATA");
        }
        return new (string, string, char)[]
        {
            ("fixture",  Path.Combine(repo, "tests", "godot", "fixtures", "stride_msb_wide.SYS"), 'A'),
            ("FONT4X5P", Path.Combine(data, "FONT4X5P.SYS"), 'F'),
            ("FONT5X6I", Path.Combine(data, "FONT5X6I.SYS"), 'F'),
            ("FONT5X6P", Path.Combine(data, "FONT5X6P.SYS"), 'F'),
            ("FONTBIG",  Path.Combine(data, "FONTBIG.SYS"),  'M'),
        };
    }

    private async System.Threading.Tasks.Task Check(string label, string path, char probe)
    {
        _raw = File.ReadAllBytes(path);
        _charSize = _raw[2] | (_raw[3] << 8);
        _height   = _raw[6] | (_raw[7] << 8);
        _rowBytes = _raw[8] | (_raw[9] << 8);
        _cell     = _rowBytes * 8;
        _probe    = probe;

        // The production path, exactly as the game would use it.
        var parsed = SysFont.Parse(_raw);
        // The four UI fonts build with their layout contract. The committed fixture has no
        // such contract, so it gets the values its own height implies; this test only checks
        // pixels, not line boxes.
        var (ascent, descent) = label == "fixture"
            ? (parsed.Height - 1f, 1f)
            : SysFontProvider.LegacyMetricsFor(label);
        _font = SysFontBuilder.Build(parsed, ascent, descent);
        int scale = RenderSize / SysFontBuilder.DesignSize;

        // Advances must equal the declared widths, scaled.
        int count = (_raw.Length - 12) / (_charSize + 1);
        for (int cp = 0x20; cp < Math.Min(count, 0x7F); cp++)
        {
            float want = OracleAdvance(cp) * scale;
            float got = _font.GetStringSize(((char)cp).ToString(), 0, -1, RenderSize).X;
            if (Math.Abs(got - want) > 0.01f)
            {
                Fail(label + " advance 0x" + cp.ToString("X2") + ": want " + want + " got " + got);
                return;
            }
        }

        QueueRedraw();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // DrawChar puts the baseline exactly at Pen, and the builder sets the glyph
        // offset to -(height-1) at the design size, so the bitmap's top-left is here.
        var img = GetViewport().GetTexture().GetImage();
        if (img == null)
        {
            Fail(label + ": viewport readback returned nothing, which usually means "
                 + "there is no real rendering driver");
            return;
        }
        int left = (int)Pen.X;
        // Sample where the glyph actually is. This test checks the ink PATTERN, so following
        // the glyph is correct here. Where the glyph BELONGS is a different property with a
        // different reference, and it lives in FontPlacementTest: deriving it from the
        // builder's own expression, as this line used to, made the oracle move with the bug
        // and hid FONTBIG rendering three source pixels too high.
        int top  = (int)Pen.Y + (int)_font.GetGlyphOffset(0, new Vector2I(SysFontBuilder.DesignSize, 0), _probe).Y * scale;

        for (int r = 0; r < _height; r++)
        {
            for (int c = 0; c < _cell; c++)
            {
                bool want = OracleInk(_probe, r, c);
                int px = left + c * scale + scale / 2;
                int py = top + r * scale + scale / 2;
                if (px < 0 || py < 0 || px >= img.GetWidth() || py >= img.GetHeight())
                {
                    Fail(label + " sample (" + r + "," + c + ") is outside the viewport");
                    return;
                }
                bool got = img.GetPixel(px, py).R > 0.5f;
                if (want != got)
                {
                    Fail(label + " glyph '" + _probe + "' pixel (row " + r + ", col " + c
                         + "): want " + want + " got " + got);
                    return;
                }
            }
        }

        _tested++;
        GD.Print("  ok   " + label);
    }

    public override void _Draw()
    {
        if (_font == null) return;
        DrawRect(new Rect2(Vector2.Zero, Size), Colors.Black);
        _font.DrawChar(GetCanvasItem(), Pen, _probe, RenderSize, Colors.White);
    }
}
