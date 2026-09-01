using System;
using System.Collections.Generic;
using Godot;

namespace Underworld
{
    /// <summary>
    /// Publishes the game's fonts before anything can use them.
    ///
    /// The four ext_resources in this scene are empty placeholder FontFiles and all 51 theme
    /// overrides point at those instances, so filling them here fills every label.
    ///
    /// _EnterTree is the hook because Godot enters the tree parent-first: no descendant
    /// _EnterTree, theme notification, _Ready, deferred call or draw runs before it returns.
    /// _Ready would be far too late, since children are ready before their parent.
    ///
    /// Readiness lives on this node rather than in a static, so returning to the launcher and
    /// trying again cannot inherit a stale failure.
    /// </summary>
    public partial class UnderworldRoot : Node3D
    {
        public bool FontsReady { get; private set; }
        public string FontError { get; private set; }

        private static readonly Dictionary<string, string> PlaceholderPaths = new()
        {
            { "FONT4X5P", "res://resources/fonts/FONT4X5P.tres" },
            { "FONT5X6I", "res://resources/fonts/FONT5X6I.tres" },
            { "FONT5X6P", "res://resources/fonts/FONT5X6P.tres" },
            { "FONTBIG",  "res://resources/fonts/FONTBIG.tres"  },
        };

        /// <summary>
        /// Walks ancestors for the owning root. Callers use this rather than CurrentScene,
        /// which is not the root when the scene is instantiated as someone else's child.
        /// Returns null when there is none, and callers treat null as not ready.
        /// </summary>
        public static UnderworldRoot Find(Node from)
        {
            for (Node n = from; n != null; n = n.GetParent())
            {
                if (n is UnderworldRoot root) return root;
            }
            return null;
        }

        /// <summary>
        /// Test-only escape hatch, read once at boot. Set to "never" to withhold the fonts
        /// so the placeholders stay empty.
        ///
        /// It exists because the gate test needs a negative control, and two earlier
        /// attempts were not one. Detaching this script changes the scene root's TYPE, so
        /// the gate exits at its `is not UnderworldRoot` check having measured nothing.
        /// Deferring publication to _Ready does not fail either: measured on 4.3, the two
        /// labels report 76 and 100 whether the fonts are published in _EnterTree or in
        /// _Ready, because a Control re-shapes after its FontFile is populated. Stale
        /// shaping does not occur in this scene.
        ///
        /// Withholding the fonts does fail, and by the right route: the same two labels
        /// report 64 and 96 against the 76 and 100 the declared widths give. That is what
        /// makes the width assertion evidence rather than decoration.
        /// </summary>
        /// <summary>
        /// Test hook. The gate test sets this between Instantiate and AddChild, which is
        /// before _EnterTree runs, and the fonts are then withheld so it can prove its own
        /// width assertions can fail.
        ///
        /// It was an environment variable, which was worse in two ways: a release build
        /// still read it, so an inherited variable could stop the game publishing its
        /// fonts, and #if DEBUG does not line up with the csproj's Configuration == Debug
        /// that gates the test scripts, so Godot's ExportDebug had the hook live with no
        /// tests. A plain property has neither problem and needs no conditional compilation.
        /// </summary>
        public bool WithholdFontsForTest { get; set; }

        public override void _EnterTree()
        {
            if (WithholdFontsForTest)
            {
                GD.Print("WithholdFontsForTest: withholding the fonts (negative control)");
                return;
            }
            Publish();
        }

        public override void _Ready()
        {
            if (FontsReady) return;
            ShowFontError();
        }

        /// <summary>
        /// Releases the texture-valued global shader parameters on the way out.
        ///
        /// The root leaves the tree before the engine finalises C# and long before it clears
        /// the renderer's globals, so this is early enough for every exit route: the Quit menu
        /// item, closing the window and --quit-after all tear the tree down through here.
        /// See PaletteLoader.ReleaseTextureShaderGlobals and issue #78.
        /// </summary>
        public override void _ExitTree()
        {
            PaletteLoader.ReleaseTextureShaderGlobals();
        }

        /// <summary>
        /// Reports the failure in the engine default font, which is the only text this scene
        /// can still draw. No theme override is set, deliberately.
        /// </summary>
        private void ShowFontError()
        {
            var layer = new CanvasLayer { Layer = 128 };
            AddChild(layer);

            var backdrop = new ColorRect
            {
                Color = new Color(0, 0, 0, 0.85f),
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
            };
            layer.AddChild(backdrop);

            layer.AddChild(new Label
            {
                Text = "The game's fonts could not be loaded.\n\n"
                     + (FontError ?? "No reason was recorded.")
                     + "\n\nCheck the game path, then restart.",
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            });
        }

        private void Publish()
        {
            FontsReady = false;
            FontError = null;

            // Force uwsettings' static constructor, which is what runs LoadSettings and
            // therefore what sets UWClass._RES and UWClass.BasePath. Booting this scene
            // directly, as the tests do, may otherwise reach here before anything has
            // touched uwsettings, leaving both unset and the failure misattributed.
            _ = uwsettings.instance;

            string dataDir = string.IsNullOrWhiteSpace(UWClass.BasePath)
                ? null
                : System.IO.Path.Combine(UWClass.BasePath, "DATA");

            if (!SysFontProvider.TryLoadAll(dataDir, out var parsed, out string error))
            {
                FontError = error;
                GD.PrintErr($"Fonts not loaded: {error}");
                return;
            }

            // Resolve every placeholder BEFORE writing to any of them. The placeholders are
            // shared resources, so populating them one at a time means a failure halfway
            // leaves some fonts published and the rest empty, with FontsReady false. A retry
            // or a second scene instance would then see a partial set.
            var targets = new List<(FontFile Placeholder, SysFont Font, float Ascent, float Descent)>();
            foreach (var pair in PlaceholderPaths)
            {
                var placeholder = GD.Load<FontFile>(pair.Value);
                if (placeholder == null)
                {
                    FontError = $"Placeholder font resource missing: {pair.Value}";
                    GD.PrintErr(FontError);
                    return;
                }
                var (ascent, descent) = SysFontProvider.LegacyMetricsFor(pair.Key);
                targets.Add((placeholder, parsed[pair.Key], ascent, descent));
            }

            // Building writes into the engine, so it can still fail on a resource the header
            // check did not anticipate. Without this the failure escapes as an unhandled
            // exception and the player gets no recorded reason and no error screen, which is
            // exactly what the fail-closed path exists to prevent.
            try
            {
                foreach (var t in targets)
                {
                    SysFontBuilder.Populate(t.Placeholder, t.Font, t.Ascent, t.Descent);
                }
            }
            catch (Exception ex)
            {
                FontError = $"The fonts could not be built: {ex.Message}";
                GD.PrintErr(FontError);
                return;
            }

            FontsReady = true;
        }
    }
}
