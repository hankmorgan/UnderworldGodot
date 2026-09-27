using System.Diagnostics;
using System.Collections;
using Peaky.Coroutines;
using System.Collections.Generic;
using Godot;

namespace Underworld
{
    /// <summary>
    /// The main virtual machine for running the cutscenes.
    /// Executes bytecode commands from .N00 control files, plays LPF animations,
    /// handles panorama scrolling with LBACK backgrounds, palette interpolation,
    /// and sprite overlays.
    ///
    /// Architecture based on reverse engineering of ovr108 (79 functions) and
    /// DPaint LPF format analysis. See "Cutscene Engine RE Notes.md".
    /// </summary>
    public partial class cutsplayer : UWClass
    {
        static int FrameNo = 0;
        static bool FullScreen;
        static CutsLoader cuts;
        static int currentFileExt;

        // Scroll direction table (from disassembly dseg_67d6+0x1068/0x1070)
        // Index 0=Down, 1=Right, 2=Up, 3=Left
        static readonly int[] ScrollDX = { 0, 1, 0, -1 };
        static readonly int[] ScrollDY = { 1, 0, -1, 0 };

        // Viewport/panorama state
        static int vpCanvasWidth, vpCanvasHeight;
        static int vpStartX, vpStartY;
        static int vpOffsetY;
        static int vpScrollDX, vpScrollDY;
        static bool vpScrollActive;
        static int vpFrameOffset;

        // LBACK panorama composite — built from LBACK*.BYT files by func 22/21.
        // The composite is wider (horizontal) or taller (vertical) than 320x200.
        // See CutsceneBitmap_ovr108_33E0 (line 446657) for LBACK loading.
        static Godot.Image vpComposite;
        static bool vpIsHorizontal;
        static System.Collections.Generic.List<(int pos, int extent, int fileIdx, byte[] rawPixels)> vpFileMappings = new();

        // Sprite overlay state for scroll animation (N03 cart, N06 flags).
        // DrawArtToScreen (line 444058) draws at fixed screen position.
        static CutsLoader vpSpriteLoader;
        static int vpSpriteFrame;

        // === Animation during a panorama scroll ===
        //
        // While a panorama scrolls, DOS does not keep the LPF frame in a buffer
        // of its own. Each frame's delta is decoded straight onto the screen at
        // the current view position (ovr108_24AB: record type 1 goes to
        // DRAW_RELATED_ovr108_189F, which writes through the row table that
        // seg003_0272_2A0D rebuilds at every pan step). Skipped pixels leave
        // the scrolled picture alone, and written pixels become part of the
        // picture and scroll with it from then on.
        //
        // The sprite LPFs are authored for that. CS000.N06 (castle flags)
        // redraws one of its two flags every 4th frame, alternating, with the
        // 1 pixel per frame scroll already built in, and each delta repaints
        // the background where that flag was drawn 8 frames (8 pixels) earlier.
        // CS000.N03 (the cart) likewise paints the background over the cart
        // that LBACK000 holds at rest. So we write each delta's pixels into
        // vpComposite at the view position and crop, as DOS does.
        //
        // Earlier versions reset the sprite to the backdrop on every keyframe,
        // pasted it over each crop and nudged it between keyframes. That tore
        // the castle flags, because whichever flag a keyframe did not redraw
        // fell back to the one baked into LBACK002, and it needed a
        // substitute backdrop (N04 frame 0) for the cart.

        // CutsceneNo accessible to ExecuteCommand for sprite path construction
        static int currentCutsceneNo;

        /// <summary>
        /// True when a panorama viewport is active (canvas differs from 320x200).
        /// </summary>
        static bool PanoramaActive =>
            vpCanvasWidth != 0 && vpCanvasHeight != 0 &&
            (vpCanvasWidth != 320 || vpCanvasHeight != 200);

        /// <summary>
        /// Height of the scene area on the 320x200 display.
        /// When a panorama viewport is active and vpOffsetY is set, the scene
        /// occupies 200 - vpOffsetY pixels, with the remainder reserved for
        /// subtitles. Otherwise the full 200 pixels are used.
        /// Derived from viewport-setup (func 20) bytecode parameters.
        /// </summary>
        static int SceneDisplayH =>
            PanoramaActive && vpOffsetY > 0 ? 200 - vpOffsetY : 200;

        // Palette interpolation state (func 19).
        // Linear interpolation matching InterpolatePaletteRange_ovr108_32CC (line 446461):
        //   result = source + (step * (target - source)) / total
        static bool palInterpActive;
        static int palInterpSpeed;     // interpolate every N frames
        static int palInterpStep;      // current step counter
        static int palInterpTotal;     // total frames from bytecode param[2]
        static int palInterpTargetIndex;
        static Palette palInterpSource;  // snapshot of palette when func 19 fires
        static Palette palInterpTarget;  // target from PALS.DAT

        // Colour cycling state (CRNG from LPF header).
        // Applied per-frame by UpdatePaletteFadeTimers_ovr108_934 (line 438583)
        // which calls RotatePaletteEntry_seg023_9 (line 98009).
        static Palette crngPalette;  // working copy of palette for cycling
        static CutsLoader.CrngEntry[] crngRanges;
        static int[] crngCounters;   // per-range accumulator (mirrors disasm pad field)

        static int StringBlock;
        static bool cancelRequested;

        public static bool IsPlaying { get; private set; }

        public static List<CutSceneCommand> commands;


        /// <summary>
        /// Initialise colour cycling state from an LPF file's CRNG block.
        /// Called each time a new LPF animation file is loaded.
        /// </summary>
        static void InitCrngCycling(CutsLoader loader)
        {
            crngRanges = loader?.CrngRanges;
            crngCounters = crngRanges != null ? new int[crngRanges.Length] : null;
            if (loader?.EmbeddedPalette != null)
            {
                // Take a working copy of the palette for cycling
                crngPalette = new Palette();
                System.Array.Copy(loader.EmbeddedPalette.red, crngPalette.red, 256);
                System.Array.Copy(loader.EmbeddedPalette.green, crngPalette.green, 256);
                System.Array.Copy(loader.EmbeddedPalette.blue, crngPalette.blue, 256);
            }
            else
            {
                crngPalette = null;
            }
        }

        /// <summary>
        /// Returns true if any CRNG ranges are active (rate > 0 with valid indices).
        /// </summary>
        static bool HasActiveCrng()
        {
            if (crngRanges == null || crngPalette == null) return false;
            for (int i = 0; i < crngRanges.Length; i++)
            {
                if (crngRanges[i].Rate > 0 && crngRanges[i].Low >= 0 &&
                    crngRanges[i].High > crngRanges[i].Low && crngRanges[i].High < 256)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Apply one tick of CRNG colour cycling to the working palette.
        /// Mirrors UpdatePaletteFadeTimers_ovr108_934 (line 438583):
        /// for each range with rate > 0, accumulate the counter and rotate
        /// when it overflows. RotatePaletteEntry_seg023_9 (line 98009)
        /// rotates forward: save first, shift entries down, wrap to end.
        /// </summary>
        static void ApplyCrngCycling()
        {
            if (crngRanges == null || crngPalette == null || crngCounters == null) return;
            for (int i = 0; i < crngRanges.Length; i++)
            {
                if (crngRanges[i].Rate <= 0) continue;
                int low = crngRanges[i].Low;
                int high = crngRanges[i].High;
                if (low < 0 || high < 0 || high >= 256 || low >= high) continue;

                crngCounters[i] += crngRanges[i].Rate;
                // The disasm accumulates rate per tick; rotate when counter >= threshold.
                // From DOSBox frame capture analysis (frames 425-750):
                //   CRNG 0 (rate 18): 1 step every 14 DOSBox frames at ~70fps = 5.0 steps/sec
                //   CRNG 3 (rate 14): 1 step every 17 DOSBox frames at ~70fps = 4.1 steps/sec
                // At PIT timer rate 18.2 Hz, threshold = 65 gives:
                //   rate 18: 18/65 * 18.2 = 5.04/sec ✓
                //   rate 14: 14/65 * 18.2 = 3.92/sec ✓
                while (crngCounters[i] >= 65)
                {
                    crngCounters[i] -= 65;
                    // Forward rotation: save first, shift down, wrap to end
                    // Matches RotatePaletteEntry_seg023_9 (line 98009)
                    Palette.cyclePalette(crngPalette, low, high - low + 1);
                }
            }
        }

        public static void StopCutscene()
        {
            cancelRequested = true;
            //MusicStreamPlayer.Instance?.Stop();
            //XMIMusic.CurrentlyPlayingThemeNo = 0;
        }

        /// <summary>
        /// Loads and begins a cutscene
        /// </summary>
        /// <param name="CutsceneNo">The index number of the cutscene to play</param>
        /// <param name="callBackMethod">Function to call after the cutscene has played</param>
        public static void PlayCutscene(int CutsceneNo, CallBacks.CutsceneCallBack callBackMethod, bool useSingleRedChannel = false)
        {
            cancelRequested = false;
            crngRanges = null;
            crngCounters = null;
            crngPalette = null;
            if (CutsceneNo >= 256)
            {
                FullScreen = false;
            }
            else
            {
                FullScreen = true;
            }

            StringBlock = 0xC00 + CutsceneNo;
            currentCutsceneNo = CutsceneNo;

            // Read the .N00 control file
            if (Loader.ReadStreamFile(
                System.IO.Path.Combine(
                    BasePath, "CUTS", GetsCutsceneFileName(CutsceneNo, 0)
                    ), out byte[] CutsData)
                )
            {
                commands = LoadCutsceneData(CutsData);
            }

            uimanager.EnableDisable(uimanager.instance.PanelMainMenu, false);

            // Start the cutscene
            _ = Coroutine.Run(
                RunCutscene(CutsceneNo: CutsceneNo, callBackMethod: callBackMethod, useSingleRedChannel: useSingleRedChannel),
                main.instance);
        }

        /// <summary>
        /// Build the panorama composite from mapped LBACK files.
        /// Horizontal panoramas (canvas width > 320): LBACKs side by side,
        /// composite width = 320 + vpStartX.
        /// Vertical panoramas (canvas height > 200): LBACKs stacked vertically.
        /// Uses the current LPF file's embedded palette to render indexed pixels.
        /// See CutsceneBitmap_ovr108_33E0 (line 446657) for LBACK loading,
        /// DrawBitmapScreen (line 446795) for VGA buffer placement.
        /// </summary>
        static void BuildPanoramaComposite()
        {
            vpIsHorizontal = vpCanvasWidth > 320;
            var pal = cuts.EmbeddedPalette;

            // Composite width: for horizontal, use the map-file position params
            // to determine width (rightmost LBACK position + 320).
            int compW = vpIsHorizontal ? vpCanvasWidth : vpCanvasWidth;
            int compH = vpCanvasHeight;

            var img = Godot.Image.Create(compW, compH, false, Godot.Image.Format.Rgb8);

            int runningY = 0;
            foreach (var mapping in vpFileMappings)
            {
                var (pos, extent, fileIdx, rawPixels) = mapping;
                if (rawPixels == null) continue;

                if (vpIsHorizontal)
                {
                    // Horizontal: each LBACK placed at its position parameter
                    // from the map-file command. Composite grows to fit.
                    int blitX = pos;
                    int totalW = System.Math.Max(img.GetWidth(), blitX + 320);
                    if (img.GetWidth() < totalW)
                    {
                        var newImg = Godot.Image.Create(totalW, compH, false, Godot.Image.Format.Rgb8);
                        newImg.BlitRect(img, new Rect2I(0, 0, img.GetWidth(), compH), Vector2I.Zero);
                        img = newImg;
                        vpCanvasWidth = totalW;
                        compW = totalW;
                    }
                    int blitW = System.Math.Min(320, compW - blitX);
                    int blitH = System.Math.Min(200, compH);
                    for (int y = 0; y < blitH; y++)
                        for (int x = 0; x < blitW; x++)
                        {
                            byte idx = rawPixels[y * 320 + x];
                            img.SetPixel(blitX + x, y, pal.ColorAtIndex(idx, false, false));
                        }
                }
                else
                {
                    // Vertical: stack LBACKs sequentially
                    int blitH = System.Math.Min(200, compH - runningY);
                    int blitW = System.Math.Min(320, compW);
                    for (int y = 0; y < blitH; y++)
                        for (int x = 0; x < blitW; x++)
                        {
                            byte idx = rawPixels[y * 320 + x];
                            img.SetPixel(x, runningY + y, pal.ColorAtIndex(idx, false, false));
                        }
                    runningY += 200;
                }
            }

            vpComposite = img;
            Debug.Print($"  Composite: {compW}x{compH}, scroll={(vpIsHorizontal ? "horizontal" : "vertical")}");
        }

        /// <summary>
        /// Top-left of the view within the panorama composite at the given
        /// frame. Implements the scroll formula from
        /// AnimateViewportScroll_ovr108_B8E (line 439098 in uw2_asm.asm):
        ///   pos = start + (frame+1) * delta * direction_table[index]
        /// The (frame+1) comes from `inc ax` at line 439116 before the multiply,
        /// and applies to both axes.
        /// Direction tables at dseg_67d6+0x1068 (DX) and +0x1070 (DY) (line 358318).
        /// VGA hardware wraps the CRT start address; we use linear equivalents.
        /// </summary>
        static Vector2I ScrollOrigin(int totalFrame)
        {
            int frameAdj = totalFrame + 1; // assembly uses frame+1 (ovr108_B9E, line 439116)
            if (vpIsHorizontal)
            {
                int scrollX = vpStartX + vpScrollDX * frameAdj;
                int maxScroll = vpComposite.GetWidth() - 320;
                return new Vector2I(System.Math.Clamp(scrollX, 0, maxScroll), 0);
            }
            // Vertical scroll: convert VGA wrapping start position to composite Y.
            // initial_y = (vp_start_y + 1) % canvas_h maps VGA start to composite.
            // DY direction is inverted for composite: DY=-1 → pos increases,
            // DY=+1 → pos decreases. This inversion occurs because VGA CRT start
            // address increase pans content upward on screen.
            // Examples from bytecode:
            //   CS000: set-start [0,359], scroll Up (DY=-1): top row = frame+1
            //   CS002: set-start [0,199], scroll Down (DY=+1): top row = 200-(frame+1)
            int initialY = (vpStartY + 1) % vpComposite.GetHeight();
            int scrollPos = initialY + (-vpScrollDY) * frameAdj;
            int maxScrollY = vpComposite.GetHeight() - SceneDisplayH;
            return new Vector2I(0, System.Math.Clamp(scrollPos, 0, maxScrollY));
        }

        /// <summary>
        /// Extract the visible viewport region from the panorama composite at the
        /// current scroll position.
        /// </summary>
        static Godot.Image GetScrollFrame(int totalFrame)
        {
            if (vpComposite == null) return null;
            var origin = ScrollOrigin(totalFrame);
            int w = 320;
            int h = vpIsHorizontal ? vpComposite.GetHeight() : SceneDisplayH;
            var region = Godot.Image.Create(w, h, false, vpComposite.GetFormat());
            region.BlitRect(vpComposite, new Rect2I(origin.X, origin.Y, w, h), Vector2I.Zero);
            return region;
        }

        /// <summary>
        /// Write the current sprite frame's delta into the panorama composite at
        /// the view position, as DOS decodes it onto the scrolled screen (see the
        /// block comment at the top of the file). Only keyframes carry pixels;
        /// the sprite LPF's per-frame RLE write mask says which pixels the delta
        /// wrote. Advances vpSpriteFrame.
        /// </summary>
        static void WriteSpriteDelta(int totalFrame)
        {
            if (vpSpriteLoader == null || vpSpriteLoader.WriteMasks == null) return;
            int f = vpSpriteFrame++;
            if (f >= vpSpriteLoader.ImageCache.Length || !vpSpriteLoader.IsKeyFrame[f]) return;
            var mask = vpSpriteLoader.WriteMasks[f];
            var spriteTex = vpSpriteLoader.ImageCache[f];
            if (spriteTex == null || mask == null) return;

            var spriteImg = spriteTex.GetImage();
            var origin = ScrollOrigin(totalFrame);
            int sw = spriteImg.GetWidth();
            int viewH = vpIsHorizontal ? vpComposite.GetHeight() : SceneDisplayH;
            int h = System.Math.Min(spriteImg.GetHeight(), viewH);
            int w = System.Math.Min(sw, 320);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (mask[y * sw + x] != 0)
                        vpComposite.SetPixel(origin.X + x, origin.Y + y, spriteImg.GetPixel(x, y));
        }

        // --- Debug: dump composited scroll frames for pixel-perfect comparison
        // against DOSBox captures. Off by default; flip DumpScrollFrames=true to
        // enable. Frames land at user://cuts_dump/CSnnn_Nxx/frame_YYY.png
        // (~/Library/Application Support/Godot/app_userdata/Underworld/cuts_dump
        // on macOS). Use with the pixel-diff scripts in the PR description.
        public static bool DumpScrollFrames = false;

        static void DumpFrame(Godot.Image img, int cutsNo, int fileExt, int totalFrame)
        {
            if (!DumpScrollFrames || img == null) return;
            string subdir = $"CS{cutsNo:D3}_N{fileExt:D2}";
            string dirAbs = Godot.ProjectSettings.GlobalizePath($"user://cuts_dump/{subdir}");
            Godot.DirAccess.MakeDirRecursiveAbsolute(dirAbs);
            img.SavePng($"{dirAbs}/frame_{totalFrame:D3}.png");
        }

        /// <summary>
        /// Build a 320x200 indexed-pixel "base" for sprite LPF decoding. The
        /// sprite LPFs (N03, N06) are authored as deltas against the backdrop
        /// pixels at their draw position; if we decoded them onto a black buffer
        /// their RLE Skip regions would leave voids. Feeding LBACK pixels as the
        /// decode base means Skip regions show LBACK content, matching the DOS
        /// engine's VGA screen where Skip opcodes leave existing pixels alone.
        /// For vertical scroll (vpStartX=0): base = first LBACK raw pixels.
        /// For horizontal scroll (vpStartX=70): base = LBACK000[70:] + LBACK001[:70].
        /// </summary>
        static byte[] BuildLbackBase()
        {
            if (vpFileMappings.Count == 0) return null;
            var lbackBase = new byte[320 * 200];
            int fx = vpStartX;

            if (vpIsHorizontal)
            {
                var raws = new System.Collections.Generic.List<byte[]>();
                foreach (var m in vpFileMappings)
                    raws.Add(m.rawPixels ?? new byte[64000]);

                for (int y = 0; y < 200; y++)
                    for (int c = 0; c < 320; c++)
                    {
                        int vgaX = fx + c;
                        int lbackIdx = vgaX / 320;
                        int localX = vgaX % 320;
                        if (lbackIdx < raws.Count)
                            lbackBase[y * 320 + c] = raws[lbackIdx][y * 320 + localX];
                    }
            }
            else
            {
                // Vertical: sprite covers full width at x=0, use first LBACK
                var firstRaw = vpFileMappings[0].rawPixels;
                if (firstRaw != null)
                    System.Array.Copy(firstRaw, lbackBase,
                        System.Math.Min(firstRaw.Length, lbackBase.Length));
            }
            return lbackBase;
        }

        /// <summary>
        /// Executes a single cutscene command. Returns the number of params consumed.
        /// </summary>
        public static IEnumerator ExecuteCommand(CutSceneCommand cmd, TextureRect cutscontrol, int CutsceneNo)
        {
            if (cancelRequested)
            {
                yield return new WaitOneFrame();
            }
            string paramlist = "";
            for (int p = 0; p < cmd.NoOfParams; p++)
            {
                paramlist += $"({cmd.functionParams[p]})";
            }
            Debug.Print($"  [{cmd.frame}] {cmd.FunctionName}: {paramlist}");

            switch (cmd.functionNo)
            {
                case 0: // show-text with colour (palette index)
                    uimanager.instance.CutsSubtitle.Text = $"[center]{GameStrings.GetString(StringBlock, cmd.functionParams[1])}[/center]";
                    break;

                case 1: // set-flag — clears internal animation flag
                    break;

                case 6: // end-cutsc
                    uimanager.EnableDisable(cutscontrol, false);
                    uimanager.EnableDisable(uimanager.instance.CutsSubtitle, false);
                    break;

                case 8: // open-file
                    {
                        var extNo = cmd.functionParams[1];
                        var filePath = System.IO.Path.Combine(
                            BasePath, "CUTS", GetsCutsceneFileName(cmd.functionParams[0], extNo));

                        if (cmd.functionParams[0] == 996)
                        {
                            if (_RES == GAME_UW2)
                            {
                                Debug.Print("opening random guardian background.");
                                //when file no is 996 then a random file is opened from index 0x1C onwards (guardian taunts.
                                var randomfile = 0x1C + Rng.r.Next(4);
                                filePath = System.IO.Path.Combine(
                                    BasePath, "CUTS", GetsCutsceneFileName(randomfile, extNo));
                            }
                            else
                            {
                                Debug.Print("use of 996 open-file in uw1. fixme.");
                            }
                        }

                        Debug.Print($"  Open {GetsCutsceneFileName(cmd.functionParams[0], extNo)}");

                        if (System.IO.File.Exists(filePath))
                        {
                            cuts = new CutsLoader(filePath);
                            InitCrngCycling(cuts);
                            currentFileExt = extNo;
                            FrameNo = 0;
                        }
                        break;
                    }

                case 9: // fade-out
                    {
                        var rate = cmd.functionParams[0];
                        if (rate > 0)
                        {
                            float duration = 2.0f / rate;
                            // Modulate to black
                            cutscontrol.Modulate = new Color(0f, 0f, 0f, 1f);
                        }
                        else
                        {
                            cutscontrol.Modulate = new Color(0f, 0f, 0f, 1f);
                        }
                        break;
                    }

                case 10: // fade-in
                    {
                        var rate = cmd.functionParams[0];
                        cutscontrol.Modulate = new Color(1f, 1f, 1f, 1f);
                        break;
                    }

                case 11: // frame-trigger — conditional frame advance
                    {
                        var target = cmd.functionParams[0] - 1;
                        Debug.Print($"  Frame-trigger: target={target}");
                        break;
                    }

                case 12: // bit-control — sets flag bit 4
                    {
                        Debug.Print($"  Bit-control: bit4 = {cmd.functionParams[0] & 1}");
                        break;
                    }

                case 13: // text-play with audio
                    {
                        // Subtitle rendering: uses FONTBIG.SYS (font index 3),
                        // hardcoded via OpenFont(3) at ovr108_2E1A (line 445603).
                        // Text positioning from RenderCutsceneText_ovr108_157B (line 441321):
                        //   line spacing = font height (TextPos[bx+6], line 441390)
                        //   bottom margin = 2px (inc dx; inc dx at ovr108_15CE, lines 441401-441403)
                        //   word wrap at 320px, no inter-character spacing
                        // NOTE: game uses underscore-as-dot convention: "_." should render as " ."
                        var audioPlayer = main.instance?.DigitalAudioPlayer;
                        while (audioPlayer != null && audioPlayer.Playing)
                        {
                            if (cancelRequested)
                            {
                                yield break;
                            }

                            yield return new WaitForSeconds(0.05f);
                        }

                        if ((short)cmd.functionParams[1] >= 0)
                        {
                            uimanager.instance.CutsSubtitle.Text = $"[center]{GameStrings.GetString(StringBlock, cmd.functionParams[1])}[/center]";
                        }
                        else
                        {
                            uimanager.instance.CutsSubtitle.Text = ""; // clear subtitle (0xFFFF = -1)
                        }
                        if (cmd.functionParams[2] != 999)
                        {
                            string vocfile;
                            if (_RES == GAME_UW2)
                            {
                                //in UW2 the cutscene audio is the guardian speech.
                                vocfile = System.IO.Path.Combine(
                                    BasePath, "SOUND",
                                    $"BSP{cmd.functionParams[2]:0#}.VOC");
                            }
                            else
                            {
                                vocfile = System.IO.Path.Combine(
                                    BasePath, "SOUND",
                                    $"{cmd.functionParams[2]:0#}.VOC");
                            }
                            var sound = vocLoader.Load(vocfile);
                            if (sound != null)
                            {
                                main.instance.DigitalAudioPlayer.Stream = sound.toWav();
                                main.instance.DigitalAudioPlayer.Play();
                            }
                        }
                        break;
                    }

                case 15: // klang — play klang sound (UW1 only)
                    Debug.Print("  Klang sound");
                    UWsoundeffects.PlaySoundEffectAtAvatar(UWsoundeffects.SoundEffectKlang, 0x40, 0);
                    break;

                case 16: // pal-copy — palette data copy
                    Debug.Print($"  Palette copy (not implemented)");
                    break;

                case 17: // timer-cb — register timer callback
                    Debug.Print($"  Timer callback (not implemented)");
                    break;

                case 19: // pal-interp — interpolate palette toward PALS.DAT target
                    {
                        // Cutscene_19_Unk_ovr108_1229 (line 440603):
                        //   param[0] = PALS.DAT index (loaded via OpenPalsData, line 440668)
                        //   param[1] = speed (timer delay — interpolate every N frames)
                        //   param[2] = total frames
                        // Source palette snapshot stored at [si+55CAh] (lines 440689-440699).
                        palInterpTargetIndex = cmd.functionParams[0];
                        palInterpSpeed = cmd.functionParams[1];
                        palInterpTotal = cmd.NoOfParams > 2 ? cmd.functionParams[2] : 80;
                        palInterpStep = 0;
                        // Snapshot current palette as source
                        if (cuts != null && cuts.EmbeddedPalette != null)
                        {
                            palInterpSource = new Palette();
                            for (int i = 0; i < 256; i++)
                            {
                                palInterpSource.red[i] = cuts.EmbeddedPalette.red[i];
                                palInterpSource.green[i] = cuts.EmbeddedPalette.green[i];
                                palInterpSource.blue[i] = cuts.EmbeddedPalette.blue[i];
                            }
                        }
                        // Load target palette directly from PALS.DAT file.
                        // Note: PaletteLoader.Palettes has a bug (uses palNo*256 instead
                        // of palNo*768 for record size), so we load directly here.
                        var palsPath = System.IO.Path.Combine(BasePath, "DATA", "PALS.DAT");
                        if (Loader.ReadStreamFile(palsPath, out byte[] palsData))
                        {
                            int palOffset = palInterpTargetIndex * 768; // 256 entries * 3 bytes
                            if (palOffset + 768 <= palsData.Length)
                            {
                                palInterpTarget = new Palette();
                                for (int i = 0; i < 256; i++)
                                {
                                    // VGA 6-bit to 8-bit: shift left 2 (same as PaletteLoader)
                                    palInterpTarget.red[i] = (byte)((palsData[palOffset + i * 3] & 0x3F) << 2);
                                    palInterpTarget.green[i] = (byte)((palsData[palOffset + i * 3 + 1] & 0x3F) << 2);
                                    palInterpTarget.blue[i] = (byte)((palsData[palOffset + i * 3 + 2] & 0x3F) << 2);
                                }
                                palInterpActive = true;
                            }
                        }
                        Debug.Print($"  Palette interp: target PALS.DAT[{palInterpTargetIndex}], speed={palInterpSpeed}, total={palInterpTotal}");
                        // TODO: implement actual palette interpolation using PaletteLoader
                        // and the LPF's color cycling ranges
                        break;
                    }

                case 20: // viewport-setup
                    {
                        // Sets panorama canvas dimensions and subtitle offset.
                        // When canvas != 320x200, sets [si+4Fh] panorama mode flag
                        // (ovr108_12D3, line 440713).
                        vpCanvasWidth = cmd.functionParams[0];
                        vpCanvasHeight = cmd.functionParams[1];
                        vpOffsetY = cmd.NoOfParams > 2 ? cmd.functionParams[2] : 0;

                        // Position subtitle label in the black bar area.
                        // From RenderCutsceneText_ovr108_157B (line 441321):
                        //   text is bottom-aligned with 2px margin (ovr108_15CE lines 441401-441403)
                        //   scene height = 200 - vpOffsetY
                        // The fullscreen cutscene renders at 4x scale (1280x800 for 320x200).
                        if (PanoramaActive && vpOffsetY > 0)
                        {
                            float scale = cutscontrol.Size.Y / 200f;
                            float subtitleTop = (200 - vpOffsetY) * scale;
                            float subtitleBottom = cutscontrol.Size.Y;
                            uimanager.instance.CutsSubtitle.Position = new Vector2(
                                uimanager.instance.CutsSubtitle.Position.X, subtitleTop);
                            uimanager.instance.CutsSubtitle.Size = new Vector2(
                                uimanager.instance.CutsSubtitle.Size.X, subtitleBottom - subtitleTop);
                        }
                        vpScrollActive = false;
                        vpFrameOffset = 0;
                        vpFileMappings.Clear();
                        vpComposite = null;
                        vpSpriteLoader = null;
                        vpSpriteFrame = 0;
                        Debug.Print($"  Viewport: {vpCanvasWidth}x{vpCanvasHeight} offsetY={vpOffsetY}");
                        break;
                    }

                case 21: // set-start
                    {
                        vpStartX = cmd.functionParams[0];
                        vpStartY = cmd.NoOfParams > 1 ? cmd.functionParams[1] : 0;
                        Debug.Print($"  Start: x={vpStartX} y={vpStartY}");
                        // Build composite now so pre-scroll segment can display it
                        if (vpFileMappings.Count > 0 && cuts != null)
                            BuildPanoramaComposite();
                        break;
                    }

                case 22: // map-file — load LBACK background bitmap
                    {
                        // LBACK*.BYT files are raw 320x200 indexed pixel data (64000 bytes, no header).
                        // Loaded by CutsceneBitmap_ovr108_33E0 (line 446657) via LoadDATFile.
                        var fileIdx = cmd.functionParams[2];
                        var lbackName = $"LBACK{fileIdx:D3}.BYT";
                        var lbackPath = System.IO.Path.Combine(BasePath, "CUTS", lbackName);
                        byte[] rawPixels = null;
                        if (Loader.ReadStreamFile(lbackPath, out byte[] lbackData) && lbackData.Length >= 64000)
                        {
                            rawPixels = lbackData;
                        }
                        vpFileMappings.Add((cmd.functionParams[0], cmd.functionParams[1], fileIdx, rawPixels));
                        Debug.Print($"  Map: fileIdx={fileIdx} -> {lbackName}");
                        break;
                    }

                case 23: // start-scroll — direction table lookup
                    {
                        // Scroll direction from table (disassembly dseg_67d6+0x1068/0x1070)
                        // Index 0=Down, 1=Right, 2=Up, 3=Left
                        var tableIdx = cmd.functionParams[0];
                        var delta = cmd.functionParams[1];
                        if (tableIdx < 4)
                        {
                            vpScrollDX = ScrollDX[tableIdx] * delta;
                            vpScrollDY = ScrollDY[tableIdx] * delta;
                        }
                        vpScrollActive = true;
                        vpFrameOffset = 0;
                        string[] dirs = { "Down", "Right", "Up", "Left" };
                        Debug.Print($"  Scroll: {dirs[tableIdx % 4]}, delta={delta}, dx={vpScrollDX} dy={vpScrollDY}");

                        // Prepare sprite overlay: decode current LPF file in sprite mode
                        // with LBACK as base. Resets buffer to LBACK before each keyframe
                        // so RLE skip areas contain LBACK content. Write masks track which
                        // pixels the RLE explicitly wrote (animated flags/cart).
                        var lbackBase = BuildLbackBase();
                        if (lbackBase != null)
                        {
                            var spritePath = System.IO.Path.Combine(
                                BasePath, "CUTS", GetsCutsceneFileName(currentCutsceneNo, currentFileExt));
                            if (System.IO.File.Exists(spritePath))
                            {
                                vpSpriteLoader = new CutsLoader(
                                    GetsCutsceneFileName(currentCutsceneNo, currentFileExt), lbackBase);
                                vpSpriteFrame = 0;
                                Debug.Print($"  Sprite overlay: ext {currentFileExt} (LBACK base, per-keyframe reset)");
                            }
                        }

                        break;
                    }

                case 24: // audio-setup
                    {
                        var fileNo = cmd.functionParams[0];
                        Debug.Print($"  Audio setup: {(fileNo == 999 ? "none" : $"file {fileNo - 1}")}");
                        break;
                    }

                case 25: // music
                    Debug.Print($"  Music: theme {cmd.functionParams[0]}");
                    XMIMusic.LoadXMI((byte)cmd.functionParams[0]);
                    break;

                case 27: // audio-wait — wait for audio playback completion or timeout
                    {
                        var audioPlayer = main.instance?.DigitalAudioPlayer;
                        var timeoutSec = cmd.functionParams[0] > 0 ? cmd.functionParams[0] / 2.0f : 60f;
                        var elapsed = 0f;
                        while (audioPlayer != null && audioPlayer.Playing && elapsed < timeoutSec)
                        {
                            if (cancelRequested)
                            {
                                break;
                            }

                            yield return new WaitForSeconds(0.05f);
                            elapsed += 0.05f;
                        }

                        break;
                    }

                case 3: // pause — wait arg[0] / 2 seconds (from disassembly)
                    if (cmd.functionParams[0] > 0)
                    {
                        if (cmd.functionParams[0] >= 999)
                        {
                            //pause indefinately.
                            Debug.Print($"  Pause: {cmd.functionParams[0]}");
                            while (true)
                            {
                                yield return new WaitForSeconds(0.25f);
                                if (cancelRequested)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            //pause for  
                            Debug.Print($"  Pause: {cmd.functionParams[0] / 2.0f}s");
                            var pausecounter = (cmd.functionParams[0] / 2) * 4;//increase by 4 to give more responsive cancelling.
                            while (pausecounter > 0)
                            {
                                if (cancelRequested)
                                {
                                    break;
                                }
                                pausecounter--;
                                yield return new WaitForSeconds(0.25f);
                            }
                        }
                    }
                    break;

                default:
                    Debug.Print($"  Unimplemented cmd {cmd.functionNo} {cmd.FunctionName}");
                    break;
            }
        }

        /// <summary>
        /// Main cutscene playback coroutine. Processes commands in segments.
        /// Each segment ends at a frame-set (func 5) or end-cutsc (func 6).
        /// </summary>
        public static IEnumerator RunCutscene(int CutsceneNo, CallBacks.CutsceneCallBack callBackMethod = null, bool useSingleRedChannel = false)
        {
            var OrigGameMode = uimanager.CurrentGameMode;
            uimanager.CurrentGameMode = uimanager.GameModes.CUTSCENE;
            MessageDisplay.WaitingForMore = false;
            Debug.Print($"Running cutscene {CutsceneNo}");
            IsPlaying = true;
            TextureRect cutscontrol;
            if (FullScreen)
                cutscontrol = uimanager.CutsFullscreen;
            else
                cutscontrol = uimanager.CutsSmall;

            uimanager.EnableDisable(cutscontrol, true);
            uimanager.EnableDisable(uimanager.instance.CutsSubtitle, true);
            uimanager.instance.CutsSubtitle.Text = "";

            FrameNo = 0;
            currentFileExt = 1;
            vpScrollActive = false;

            // Splash screen sequence for the intro, matching the original engine's
            // SplashPart1_ovr112_36 (line 461013) → SplashPart2 (line 461467):
            //   1. Origin logo: BYT.ARK entry 6, palette 5 (LoadBitMap line 461220)
            //   2. LGS logo: BYT.ARK entry 7, palette 6 (LoadBitMap line 461269)
            //   3. CS011 animated title (PlayCutscene(9) from SplashPart2 line 461480)
            // CutsceneNo 9 = CS011 (octal 011, the title screen cutscene).
            // DOSBox capture: Origin at frame 1, LGS at frame 20, title at frame 89.
            if (CutsceneNo == 9 && _RES == GAME_UW2 && !cancelRequested)
            {
                cutscontrol.Modulate = new Color(1f, 1f, 1f, 1f);
                cutscontrol.Texture = uimanager.bitmaps.LoadImageAt(6); // Origin logo
                if (cancelRequested) goto cleanup;
                yield return new WaitForSeconds(2.0f);
                if (cancelRequested) goto cleanup;
                cutscontrol.Texture = null;
                yield return new WaitForSeconds(0.5f);
                if (cancelRequested) goto cleanup;
                cutscontrol.Texture = uimanager.bitmaps.LoadImageAt(7); // LGS logo
                yield return new WaitForSeconds(2.0f);
                if (cancelRequested) goto cleanup;
                cutscontrol.Texture = null;
                yield return new WaitForSeconds(0.5f);
                if (cancelRequested) goto cleanup;
            }
            else if (CutsceneNo == 9 && _RES != GAME_UW2 && !cancelRequested)
            {
                cutscontrol.Modulate = new Color(1f, 1f, 1f, 1f);
                cutscontrol.Texture = uimanager.bitmaps.LoadImageAt(5); // Origin logo
                if (cancelRequested) goto cleanup;
                yield return new WaitForSeconds(2.0f);
                if (cancelRequested) goto cleanup;
                if (_RES != GAME_UWDEMO)
                {
                    cutscontrol.Texture = null;
                    yield return new WaitForSeconds(0.5f);
                    if (cancelRequested) goto cleanup;
                    cutscontrol.Texture = uimanager.bitmaps.LoadImageAt(6); // Blue sky logo
                    yield return new WaitForSeconds(2.0f);
                    if (cancelRequested) goto cleanup;
                    cutscontrol.Texture = null;
                    yield return new WaitForSeconds(0.5f);
                    if (cancelRequested) goto cleanup;
                }
            }
            palInterpActive = false;

            // Load the first animation file (.N01)
            cuts = null;
            var firstFile = System.IO.Path.Combine(BasePath, "CUTS", GetsCutsceneFileName(CutsceneNo, 1));
            //check if the first command in the n01 file is not a change file command.
            if (commands[0] != null)
            {
                if (commands[0].functionNo == 8)
                {
                    if (commands[0].functionParams[0] == 996)
                    {
                        if (_RES == GAME_UW2)
                        {
                            Debug.Print("opening random guardian background.");
                            //when file no is 996 then a random file is opened from index 0x1C onwards (guardian taunts.
                            var randomfile = 0x1C + Rng.r.Next(4);
                            firstFile = System.IO.Path.Combine(
                                BasePath, "CUTS", GetsCutsceneFileName(randomfile, 1));
                        }
                        else
                        {
                            Debug.Print("use of 996 open-file in uw1. fixme.");
                        }
                    }
                }
            }
            if (System.IO.File.Exists(firstFile))
            {
                cuts = new CutsLoader(firstFile, useSingleRedChannel);
            }
            else
            {
                Debug.Print($"File not found {firstFile}");
            }
            InitCrngCycling(cuts);

            // Set initial frame to black
            cutscontrol.Modulate = new Color(0f, 0f, 0f, 1f);
            uimanager.FlashColour(
                colour: 0, targetControl: cutscontrol, duration: 1, IgnoreDelay: true);

            // Process commands in segments
            int cmdIndex = 0;
            bool cutsceneRunning = true;
            bool firstSegment = true;

            bool fileChanged = false;

            while (cutsceneRunning && !cancelRequested && cmdIndex < commands.Count)
            {
                if (cancelRequested)
                {
                    goto cleanup;
                }
                // Auto-advance to the next animation file (unless open-file changed it)
                if (!firstSegment && !fileChanged)
                {
                    int nextExt = currentFileExt + 1;
                    var nextFile = System.IO.Path.Combine(
                        BasePath, "CUTS", GetsCutsceneFileName(CutsceneNo, nextExt));
                    if (System.IO.File.Exists(nextFile))
                    {
                        cuts = new CutsLoader(nextFile);
                        InitCrngCycling(cuts);
                    }
                    currentFileExt = nextExt;
                    FrameNo = 0;
                }
                firstSegment = false;
                fileChanged = false;

                // Reset palette interpolation per segment (func 19 re-enables it).
                palInterpActive = false;

                // Collect this segment's commands
                var scheduledCmds = new List<CutSceneCommand>();
                var postCmds = new List<CutSceneCommand>();
                int segmentFrameCount = 0;
                bool hasFrameSet = false;
                bool isEndCutscene = false;

                int scanIndex = cmdIndex;
                while (scanIndex < commands.Count)
                {
                    var cmd = commands[scanIndex];
                    scanIndex++;

                    if (cmd.functionNo == 5) // frame-set
                    {
                        segmentFrameCount = cmd.frame;
                        hasFrameSet = true;
                        scheduledCmds.Add(cmd);
                        break;
                    }
                    if (cmd.functionNo == 6) // end-cutsc
                    {
                        // If no frame-set was found in this segment AND no frame-set
                        // has been seen in any prior segment, use end-cutsc frame as
                        // the segment length. CS011 (title screen) uses this: fade-in
                        // at frame 1, end at frame 37, with CRNG cycling across 37 frames.
                        // Multi-segment cutscenes (e.g. CS000) have frame-sets in earlier
                        // segments, so their trailing end-cutsc won't trigger this.
                        if (!hasFrameSet && cmd.frame > 0)
                        {
                            segmentFrameCount = cmd.frame;
                            hasFrameSet = true;
                        }
                        scheduledCmds.Add(cmd);
                        isEndCutscene = true;
                        break;
                    }
                    scheduledCmds.Add(cmd);
                }

                // Collect post-animation (frame=999) commands
                while (scanIndex < commands.Count && commands[scanIndex].frame == 999)
                {
                    postCmds.Add(commands[scanIndex]);
                    scanIndex++;
                }

                Debug.Print($"--- Segment: {scheduledCmds.Count} cmds, {postCmds.Count} post-cmds, {segmentFrameCount} frames, ext={currentFileExt} ---");

                // Scroll state: if this segment has no start-scroll command,
                // deactivate scrolling. The panorama composite persists (only
                // cleared by viewport-setup func 20) but the viewport stops moving.
                // Segments without start-scroll render LPF frames directly.
                bool hasScrollCmd = false;
                foreach (var cmd in scheduledCmds)
                    if (cmd.functionNo == 23) { hasScrollCmd = true; break; }
                if (!hasScrollCmd)
                    vpScrollActive = false;

                // Execute the segment
                if (hasFrameSet && segmentFrameCount > 0)
                {
                    // Frame timing from LPF file header fps field (offset 0x44).
                    // Each LPF specifies its own playback rate (e.g. N03: 14fps, N06: 40fps).
                    float frameTime = 0.1f; // default 10fps fallback
                    if (cuts != null && cuts.FramesPerSecond > 0)
                    {
                        frameTime = 1.0f / cuts.FramesPerSecond;
                    }

                    for (int frame = 0; frame < segmentFrameCount; frame++)
                    {
                        if (cancelRequested)
                        {
                            goto cleanup;
                        }

                        // Fire commands scheduled for this frame
                        foreach (var cmd in scheduledCmds)
                        {
                            if (cmd.frame == frame && cmd.functionNo != 5)
                            {
                                if (cancelRequested)
                                {
                                    goto cleanup;
                                }
                                yield return ExecuteCommand(cmd, cutscontrol, CutsceneNo);
                                if (cmd.functionNo == 8) // open-file
                                    fileChanged = true;
                            }
                        }

                        // Display animation frame.
                        // When panorama mode is active ([si+4Fh]!=0, set by func 20
                        // when canvas != 320x200), the normal DrawBitMap call is SKIPPED
                        // (ovr108_24AB, line 443935). Instead:
                        // 1. AnimateViewportScroll shifts the VGA CRT start address
                        // 2. A delta record (type 1) is decoded straight onto the
                        //    screen at the view position by DRAW_RELATED_ovr108_189F
                        // We simulate this by writing the frame's delta into the
                        // LBACK composite at the view position and cropping it.
                        // See the panorama block comment at the top of the file.
                        if (vpScrollActive && vpComposite != null)
                        {
                            int totalFrame = vpFrameOffset + frame;
                            WriteSpriteDelta(totalFrame);
                            var scrollRegion = GetScrollFrame(totalFrame);
                            if (scrollRegion != null)
                            {
                                uimanager.DisplayScrollFrame(scrollRegion, cutscontrol);
                                DumpFrame(scrollRegion, CutsceneNo, currentFileExt, totalFrame);
                            }
                            FrameNo++; // advance LPF frame for delta chaining
                        }
                        else if (cuts != null)
                        {
                            if (FrameNo > cuts.ImageCache.GetUpperBound(0))
                                FrameNo = 0;

                            // Palette interpolation: re-render frame with interpolated palette.
                            // Linear interpolation from InterpolatePaletteRange_ovr108_32CC (line 446461):
                            //   result = source + (step * (target - source)) / total
                            if (palInterpActive && palInterpSource != null && palInterpTarget != null
                                && cuts.RawPixelData != null && FrameNo < cuts.RawPixelData.Length
                                && cuts.RawPixelData[FrameNo] != null)
                            {
                                palInterpStep++;
                                // Update interpolated palette on speed-divisible frames
                                int actualStep = System.Math.Max(1, palInterpStep / System.Math.Max(1, palInterpSpeed));
                                int totalSteps = System.Math.Max(1, palInterpTotal / System.Math.Max(1, palInterpSpeed));
                                actualStep = System.Math.Min(actualStep, totalSteps);

                                // Build interpolated palette
                                var interpPal = new Palette();
                                for (int i = 0; i < 256; i++)
                                {
                                    int sr = palInterpSource.red[i], sg = palInterpSource.green[i], sb = palInterpSource.blue[i];
                                    int tr = palInterpTarget.red[i], tg = palInterpTarget.green[i], tb = palInterpTarget.blue[i];
                                    interpPal.red[i] = (byte)System.Math.Clamp(sr + (actualStep * (tr - sr)) / totalSteps, 0, 255);
                                    interpPal.green[i] = (byte)System.Math.Clamp(sg + (actualStep * (tg - sg)) / totalSteps, 0, 255);
                                    interpPal.blue[i] = (byte)System.Math.Clamp(sb + (actualStep * (tb - sb)) / totalSteps, 0, 255);
                                }

                                // Re-render frame with interpolated palette
                                var rerendered = ArtLoader.Image(
                                    databuffer: cuts.RawPixelData[FrameNo],
                                    dataOffSet: 0,
                                    width: 320, height: 200,
                                    palette: interpPal,
                                    useAlphaChannel: false,
                                    useSingleRedChannel: false,
                                    crop: false);
                                cutscontrol.Texture = rerendered;
                                FrameNo++;
                            }
                            else if (HasActiveCrng()
                                && cuts.RawPixelData != null && FrameNo < cuts.RawPixelData.Length
                                && cuts.RawPixelData[FrameNo] != null)
                            {
                                // CRNG colour cycling: the DOS engine updates VGA palette
                                // at the PIT timer rate (~18.2 Hz), independently of the
                                // LPF animation frame rate. We render multiple cycling
                                // ticks per LPF frame to match, then hold the last result.
                                byte[] pixelData = cuts.RawPixelData[FrameNo];
                                int cycleTicksPerFrame = System.Math.Max(1,
                                    (int)(18.2f / System.Math.Max(1, cuts.FramesPerSecond)));
                                float tickTime = frameTime / cycleTicksPerFrame;

                                for (int tick = 0; tick < cycleTicksPerFrame; tick++)
                                {
                                    if (cancelRequested)
                                    {
                                        goto cleanup;
                                    }
                                    ApplyCrngCycling();
                                    var rerendered = ArtLoader.Image(
                                        databuffer: pixelData,
                                        dataOffSet: 0,
                                        width: 320, height: 200,
                                        palette: crngPalette,
                                        useAlphaChannel: false,
                                        useSingleRedChannel: false,
                                        crop: false);
                                    cutscontrol.Texture = rerendered;
                                    if (tick < cycleTicksPerFrame - 1)
                                        yield return new WaitForSeconds(tickTime);
                                }
                                FrameNo++;
                            }
                            else
                            {
                                uimanager.DisplayCutsImage(
                                    cuts: cuts, imageNo: FrameNo++, targetControl: cutscontrol,
                                    cropHeight: SceneDisplayH, useSingleRedChannel: useSingleRedChannel);
                            }
                        }
                        if (cancelRequested)
                        {
                            goto cleanup;
                        }
                        yield return new WaitForSeconds(frameTime);
                        if (cancelRequested)
                        {
                            goto cleanup;
                        }
                    }

                    // Execute commands at the frame-set boundary frame number.
                    // These fire after the last displayed frame (frame-set frame
                    // is not displayed per DPaint LPF format). Typically fade or
                    // open-file commands that carry over into the next segment.
                    foreach (var cmd in scheduledCmds)
                    {
                        if (cmd.frame == segmentFrameCount && cmd.functionNo != 5)
                        {
                            yield return ExecuteCommand(cmd, cutscontrol, CutsceneNo);
                            if (cmd.functionNo == 8) // open-file
                                fileChanged = true;
                        }
                    }

                    if (vpScrollActive)
                        vpFrameOffset += segmentFrameCount;
                }
                else
                {
                    // No frame-set: commands have varying frame numbers.
                    // Use the max frame number as the segment length and
                    // interleave commands with frame display.
                    int maxFrame = 0;
                    foreach (var cmd in scheduledCmds)
                        if (cmd.frame < 999 && cmd.frame > maxFrame)
                            maxFrame = cmd.frame;

                    if (maxFrame > 0)
                    {
                        // Frame timing from LPF fps
                        float frameTime = 0.1f;
                        if (cuts != null && cuts.FramesPerSecond > 0)
                            frameTime = 1.0f / cuts.FramesPerSecond;

                        for (int frame = 0; frame <= maxFrame; frame++)
                        {
                            foreach (var cmd in scheduledCmds)
                            {
                                if (cmd.frame == frame)
                                    yield return ExecuteCommand(cmd, cutscontrol, CutsceneNo);
                            }

                            // Display frame
                            if (cuts != null)
                            {
                                if (FrameNo > cuts.ImageCache.GetUpperBound(0))
                                    FrameNo = 0;
                                if (FrameNo <= cuts.ImageCache.GetUpperBound(0))
                                {
                                    uimanager.DisplayCutsImage(
                                        cuts: cuts, imageNo: FrameNo++, targetControl: cutscontrol,
                                        cropHeight: SceneDisplayH, useSingleRedChannel: useSingleRedChannel);
                                }
                            }
                            yield return new WaitForSeconds(frameTime);
                            if (cancelRequested)
                            {
                                goto cleanup;
                            }
                        }
                    }
                    else
                    {
                        // All commands at frame 0: execute sequentially (blocking)
                        foreach (var cmd in scheduledCmds)
                        {
                            if (cancelRequested)
                            {
                                goto cleanup;
                            }
                            yield return ExecuteCommand(cmd, cutscontrol, CutsceneNo);
                            if (cancelRequested)
                            {
                                goto cleanup;
                            }
                            // Blocking waits
                            if (cmd.functionNo == 3) // pause
                            {
                                if (cancelRequested)
                                {
                                    goto cleanup;
                                }
                                yield return new WaitForSeconds(cmd.functionParams[0] / 2f);
                                if (cancelRequested)
                                {
                                    goto cleanup;
                                }
                            }
                            else if (cmd.functionNo == 14) // wait-secs
                            {
                                if (cancelRequested)
                                {
                                    goto cleanup;
                                }
                                yield return new WaitForSeconds(cmd.functionParams[0]);
                                if (cancelRequested)
                                {
                                    goto cleanup;
                                }
                            }
                            else if (cmd.functionNo == 9 && cmd.functionParams[0] > 0) // blocking fade-out
                            {
                                float duration = 2.0f / cmd.functionParams[0];
                                int steps = 10;
                                float stepTime = duration / steps;
                                for (int i = 1; i <= steps; i++)
                                {
                                    float t = (float)i / steps;
                                    cutscontrol.Modulate = new Color(1f - t, 1f - t, 1f - t, 1f);
                                    if (cancelRequested)
                                    {
                                        goto cleanup;
                                    }
                                    yield return new WaitForSeconds(stepTime);
                                    if (cancelRequested)
                                    {
                                        goto cleanup;
                                    }
                                }
                            }
                            else if (cmd.functionNo == 10 && cmd.functionParams[0] > 0) // blocking fade-in
                            {
                                float duration = 2.0f / cmd.functionParams[0];
                                int steps = 10;
                                float stepTime = duration / steps;
                                for (int i = 1; i <= steps; i++)
                                {
                                    float t = (float)i / steps;
                                    cutscontrol.Modulate = new Color(t, t, t, 1f);
                                    if (cancelRequested)
                                    {
                                        goto cleanup;
                                    }
                                    yield return new WaitForSeconds(stepTime);
                                    if (cancelRequested)
                                    {
                                        goto cleanup;
                                    }
                                }
                            }
                            else if (cmd.functionNo == 4) // to-frame (inline)
                            {
                                for (int i = 0; i < cmd.functionParams[0]; i++)
                                {
                                    if (cuts != null)
                                    {
                                        if (FrameNo > cuts.ImageCache.GetUpperBound(0))
                                            FrameNo = 0;
                                        uimanager.DisplayCutsImage(
                                            cuts: cuts, imageNo: FrameNo++, targetControl: cutscontrol, useSingleRedChannel: useSingleRedChannel);
                                    }
                                    if (cancelRequested)
                                    {
                                        goto cleanup;
                                    }
                                    yield return new WaitForSeconds(0.2f);
                                    if (cancelRequested)
                                    {
                                        goto cleanup;
                                    }
                                }
                            }
                            else if (cmd.functionNo == 7) // rep-seg
                            {
                                Debug.Print($"  Rep-seg: repeat {cmd.functionParams[0]} times");
                                //NOTE. From what it appears to do. 
                                // For the frame from the previous frame-set function. 
                                // Repeats that frame and frame + 1 , arg[0] times. Eg death skulls eye flashing                            
                            }
                        }
                    } // end else (all commands at frame 0)
                } // end else (no frame-set)

                // Execute post-animation (frame=999) commands with blocking
                foreach (var cmd in postCmds)
                {
                    if (cancelRequested)
                    {
                        goto cleanup;
                    }
                    yield return ExecuteCommand(cmd, cutscontrol, CutsceneNo);
                    if (cancelRequested)
                    {
                        goto cleanup;
                    }
                    if (cmd.functionNo == 9 && cmd.functionParams[0] > 0) // fade-out
                    {
                        float duration = 2.0f / cmd.functionParams[0];
                        int steps = 10;
                        float stepTime = duration / steps;
                        for (int i = 1; i <= steps; i++)
                        {
                            float t = (float)i / steps;
                            cutscontrol.Modulate = new Color(1f - t, 1f - t, 1f - t, 1f);
                            if (cancelRequested)
                            {
                                goto cleanup;
                            }
                            yield return new WaitForSeconds(stepTime);
                            if (cancelRequested)
                            {
                                goto cleanup;
                            }
                        }
                    }
                    else if (cmd.functionNo == 10 && cmd.functionParams[0] > 0) // fade-in
                    {
                        if (cancelRequested)
                        {
                            goto cleanup;
                        }
                        float duration = 2.0f / cmd.functionParams[0];
                        int steps = 10;
                        float stepTime = duration / steps;
                        for (int i = 1; i <= steps; i++)
                        {
                            float t = (float)i / steps;
                            cutscontrol.Modulate = new Color(t, t, t, 1f);
                            if (cancelRequested)
                            {
                                goto cleanup;
                            }
                            yield return new WaitForSeconds(stepTime);
                            if (cancelRequested)
                            {
                                goto cleanup;
                            }
                        }
                    }
                }

                if (isEndCutscene)
                {
                    cutsceneRunning = false;
                }

                cmdIndex = scanIndex;
            }

        cleanup:
            IsPlaying = false;
            uimanager.CurrentGameMode = OrigGameMode;//restore gamemode before any new cutscenes start.
            uimanager.EnableDisable(cutscontrol, false);
            uimanager.EnableDisable(uimanager.instance.CutsSubtitle, false);

            if ((cancelRequested) && (_RES == GAME_UW2) && (CutsceneNo == 0))
            {
                // Escape was pressed — skip the chained callback (which would play
                // the next cutscene in the intro sequence) and return straight to
                // the main menu. 
                // Done for UW2 intro cutscene only. Otherwise always make sure to run the callback as it may be game critical. 
                // Eg endgame sequence or if a cutscene is interupted during normal gameplay.
                uimanager.ReturnToMainMenu();
                callBackMethod = null;
            }
            else if (callBackMethod != null)
            {
                callBackMethod();
                callBackMethod = null;
            }

            yield return new WaitOneFrame();
        }

        /// <summary>
        /// Used to support some windowed cutscenes that turn off the cameras.
        /// </summary>
        public static void RestoreViewPort()
        {
            uimanager.EnableDisable(uimanager.instance.uwviewport, true);
        }

    } // end class
} // end namespace
