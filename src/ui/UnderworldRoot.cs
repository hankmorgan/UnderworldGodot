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

        private bool _quitStarted;
        private long _quitAfterFrame = -1;
        private bool _autoAcceptQuitWasSet;
        private bool _previousAutoAcceptQuit;

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
            // AutoAcceptQuit belongs to the SceneTree, not to this scene, so the previous
            // value has to go back when this root leaves. The game can return to its
            // launcher, and leaving the tree refusing quits with our handler gone would
            // stop the window close button working entirely.
            var tree = GetTree();
            if (tree != null)
            {
                _previousAutoAcceptQuit = tree.AutoAcceptQuit;
                tree.AutoAcceptQuit = false;
                _autoAcceptQuitWasSet = true;
            }
            _quitAfterFrame = ParseQuitAfter();

            if (!FontsReady) ShowFontError();
        }

        public override void _Process(double delta)
        {
            // --quit-after tears the tree down on its own deadline, which is too late to
            // drain audio. Start early enough that the drain finishes first.
            if (_quitAfterFrame >= 0 && !_quitStarted &&
                (long)GetTree().GetFrame() >= _quitAfterFrame)
            {
                RequestQuit();
            }
        }

        public override void _Notification(int what)
        {
            if (what == NotificationWMCloseRequest) RequestQuit();
        }

        /// <summary>
        /// Quits after letting the AudioServer let go of our stream playbacks.
        ///
        /// On Godot 4.3 the engine finalises C# before it destroys the AudioServer, and its
        /// CSharpLanguage::finalize does not free instance bindings, so a playback the server
        /// still owns is unreferenced through a stale binding and hangs the process. Stop()
        /// only marks the playback for deletion, so the audio thread and a later main-thread
        /// AudioServer.update() both have to run before the wrappers can be disposed. See #78.
        /// </summary>
        public void RequestQuit(int exitCode = 0)
        {
            if (_quitStarted) return;
            _quitStarted = true;
            DrainAudioThenQuit(exitCode);
        }

        private async void DrainAudioThenQuit(int exitCode)
        {
            // Captured before the first await: the continuation can resume after this node
            // has left the tree, and GetTree() would then return null or throw. Quitting has
            // to happen whatever else fails, because AutoAcceptQuit is false.
            SceneTree tree = GetTree();
            try
            {
                // Held across the awaits. Both nodes clear their static Instance in
                // _ExitTree, so if this root is freed mid-drain the release below would
                // find nothing and the wrappers would never be disposed.
                var music = MusicStreamPlayer.Instance;
                var sfx = Sfx.SfxStreamPlayer.Instance;

                // Phase one has to succeed before the drain wait means anything: the wait
                // exists to let the server collect a playback that Stop() has detached, and
                // a producer that missed its join has not detached one yet. Retry first,
                // then drain, so the two never collapse into the same frame.
                bool stopped = Stop(music) & Stop(sfx);
                for (int attempt = 0; !stopped && attempt < 3; attempt++)
                {
                    var retry = tree.CreateTimer(0.1, true, false, true);
                    await ToSignal(retry, SceneTreeTimer.SignalName.Timeout);
                    // A short join on retries: the 100ms wait above already gave the
                    // producer time, and blocking another half second per node per attempt
                    // would freeze quitting for seconds.
                    stopped = Stop(music, 50) & Stop(sfx, 50);
                }

                // Real time for the audio thread to mix the playback out, then main-thread
                // frames for AudioServer.update() to collect it.
                var timer = tree.CreateTimer(0.25, true, false, true);
                await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
                await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                await ToSignal(tree, SceneTree.SignalName.ProcessFrame);

                if (!stopped)
                {
                    GD.PushError("Audio producers did not stop; leaving their objects alone.");
                }

                else
                {
                    if (music != null && GodotObject.IsInstanceValid(music))
                    {
                        music.ReleaseGodotAudioBindings();
                    }
                    if (sfx != null && GodotObject.IsInstanceValid(sfx))
                    {
                        sfx.ReleaseGodotAudioBindings();
                    }
                }

                await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            }
            catch (Exception ex)
            {
                GD.PushError($"Audio shutdown failed while quitting: {ex.Message}. Quitting anyway.");
            }
            finally
            {
                try
                {
                    tree.Quit(exitCode);
                }
                catch (Exception ex)
                {
                    GD.PushError($"Quit failed: {ex.Message}. Terminating.");
                    OS.Kill(OS.GetProcessId());
                }
            }
        }

        /// <summary>
        /// Test hook. Godot consumes --quit-after itself, so it never reaches
        /// OS.GetCmdlineArgs and cannot be intercepted. This drives the same RequestQuit
        /// path the Quit menu item and the window close button use.
        /// </summary>
        /// <summary>Runs phase one on a node that may already have left the tree.</summary>
        private static bool Stop(Node node, int joinMs = 500)
        {
            if (node == null || !GodotObject.IsInstanceValid(node)) return true;
            return node is MusicStreamPlayer m
                ? m.BeginGodotAudioShutdown(joinMs)
                : ((Sfx.SfxStreamPlayer)node).BeginGodotAudioShutdown(joinMs);
        }

        private static long ParseQuitAfter()
        {
            // A user argument rather than an environment variable, so nothing a packaged or
            // launcher-managed build happens to inherit can quit the game on its own. Godot
            // consumes --quit-after itself, so it never reaches a script and the drain
            // cannot be exercised through it.
            //   Godot --path . res://scenes/Underworld.tscn -- --uwtest-quit-after 300
            string[] args = OS.GetCmdlineUserArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--uwtest-quit-after" && i + 1 < args.Length &&
                    long.TryParse(args[i + 1], out long n)) return n;
            }
            return -1;
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
            if (_autoAcceptQuitWasSet)
            {
                var tree = GetTree();
                if (tree != null) tree.AutoAcceptQuit = _previousAutoAcceptQuit;
            }
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
