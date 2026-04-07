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
        static Godot.Image vpCompositeClean;  // pristine LBACK for per-frame reset
        static bool vpIsHorizontal;
        static System.Collections.Generic.List<(int pos, int extent, int fileIdx, byte[] rawPixels)> vpFileMappings = new();

        // Sprite overlay state for scroll animation (N03 cart, N06 flags).
        // DrawArtToScreen (line 444058) draws at fixed screen position.
        static CutsLoader vpSpriteLoader;
        static int vpSpriteFrame;

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

        // Palette interpolation state (func 19)
        static bool palInterpActive;
        static int palInterpSpeed;
        static int palInterpStep;
        static int palInterpTargetIndex;

        static int StringBlock;

        public static List<CutSceneCommand> commands;


        /// <summary>
        /// Loads and begins a cutscene
        /// </summary>
        /// <param name="CutsceneNo">The index number of the cutscene to play</param>
        /// <param name="callBackMethod">Function to call after the cutscene has played</param>
        public static void PlayCutscene(int CutsceneNo, CallBacks.CutsceneCallBack callBackMethod)
        {
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
                RunCutscene(CutsceneNo, callBackMethod),
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
            vpCompositeClean = (Godot.Image)img.Duplicate();
            Debug.Print($"  Composite: {compW}x{compH}, scroll={(vpIsHorizontal ? "horizontal" : "vertical")}");
        }

        /// <summary>
        /// Extract the visible viewport region from the panorama composite at the
        /// current scroll position. Implements the scroll formula from
        /// AnimateViewportScroll_ovr108_B8E (line 439098 in uw2_asm.asm):
        ///   pos = start + (frame+1) * delta * direction_table[index]
        /// The (frame+1) comes from `inc ax` at line 439116 before the multiply.
        /// Direction tables at dseg_67d6+0x1068 (DX) and +0x1070 (DY) (line 358318).
        /// VGA hardware wraps the CRT start address; we use linear equivalents.
        /// </summary>
        static Godot.Image GetScrollFrame(int totalFrame)
        {
            if (vpComposite == null) return null;

            int frameAdj = totalFrame + 1; // assembly uses frame+1 (ovr108_B9E, line 439116)
            int displayH = SceneDisplayH;

            Godot.Image region;
            if (vpIsHorizontal)
            {
                int scrollX = vpStartX + vpScrollDX * frameAdj;
                int maxScroll = vpComposite.GetWidth() - 320;
                int scrollPos = System.Math.Clamp(scrollX, 0, maxScroll);
                region = Godot.Image.Create(320, vpComposite.GetHeight(), false, vpComposite.GetFormat());
                region.BlitRect(vpComposite,
                    new Rect2I(scrollPos, 0, 320, vpComposite.GetHeight()),
                    Vector2I.Zero);
            }
            else
            {
                // Vertical scroll: convert VGA wrapping start position to composite Y.
                // initial_y = (vp_start_y + 1) % canvas_h maps VGA start to composite.
                // DY direction is inverted for composite: DY=-1 → pos increases,
                // DY=+1 → pos decreases. This inversion occurs because VGA CRT start
                // address increase pans content upward on screen.
                // Examples from bytecode:
                //   CS000: set-start [0,359], scroll Up (DY=-1): initial=0, increases 0→200
                //   CS002: set-start [0,199], scroll Down (DY=+1): initial=200, decreases 200→0
                int initialY = (vpStartY + 1) % vpComposite.GetHeight();
                int scrollPos = initialY + (-vpScrollDY) * totalFrame;
                int maxScroll = vpComposite.GetHeight() - displayH;
                scrollPos = System.Math.Clamp(scrollPos, 0, maxScroll);
                region = Godot.Image.Create(320, displayH, false, vpComposite.GetFormat());
                region.BlitRect(vpComposite,
                    new Rect2I(0, scrollPos, 320, displayH),
                    Vector2I.Zero);
            }

            return region;
        }

        /// <summary>
        /// Overlay animated sprite pixels onto the viewport region at fixed screen
        /// positions. Uses RLE write masks to draw only explicitly written pixels
        /// (the animated flags/cart), preserving the scrolling LBACK background.
        /// Matches the original engine's DrawArtToScreen (line 444058) which draws
        /// at a fixed VGA screen position ([si+13h]=0, [si+15h]=0), not into the
        /// scrolling panorama buffer. The LPF animation already compensates for
        /// scroll progression in its coordinate space.
        /// </summary>
        static void ApplySpriteOverlay(Godot.Image region)
        {
            if (vpSpriteLoader == null || vpSpriteLoader.WriteMasks == null) return;
            if (vpSpriteFrame >= vpSpriteLoader.ImageCache.Length) return;

            var spriteTex = vpSpriteLoader.ImageCache[vpSpriteFrame];
            var mask = vpSpriteLoader.WriteMasks[vpSpriteFrame];
            if (spriteTex == null || mask == null) { vpSpriteFrame++; return; }

            var spriteImg = spriteTex.GetImage();
            int rw = region.GetWidth();
            int rh = region.GetHeight();
            int sw = spriteImg.GetWidth();
            int sh = spriteImg.GetHeight();
            int h = System.Math.Min(sh, rh);
            int w = System.Math.Min(sw, rw);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (mask[y * sw + x] != 0)
                        region.SetPixel(x, y, spriteImg.GetPixel(x, y));
                }

            vpSpriteFrame++;
        }

        /// <summary>
        /// Build raw indexed pixel buffer from LBACK data at the sprite position.
        /// Used as the decode base for sprite LPF files so that RLE skip areas
        /// contain LBACK content (matching the VGA screen during panorama scroll).
        /// For vertical scroll (vpStartX=0): base = first LBACK raw pixels.
        /// For horizontal scroll (vpStartX=70): base = LBACK000[70:] + LBACK001[:70].
        /// The LPF frame covers a 320px-wide region starting at vpStartX in the
        /// VGA buffer, so the base must contain the LBACK pixels at those positions.
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
        static void ExecuteCommand(CutSceneCommand cmd, TextureRect cutscontrol, int CutsceneNo)
        {
            string paramlist = "";
            for (int p = 0; p < cmd.NoOfParams; p++)
            {
                paramlist += $"({cmd.functionParams[p]})";
            }
            Debug.Print($"  [{cmd.frame}] {cmd.FunctionName}: {paramlist}");

            switch (cmd.functionNo)
            {
                case 0: // show-text with colour (palette index)
                    uimanager.instance.CutsSubtitle.Text = GameStrings.GetString(StringBlock, cmd.functionParams[1]);
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
                        Debug.Print($"  Open {GetsCutsceneFileName(cmd.functionParams[0], extNo)}");
                        var filePath = System.IO.Path.Combine(
                            BasePath, "CUTS", GetsCutsceneFileName(cmd.functionParams[0], extNo));
                        if (System.IO.File.Exists(filePath))
                        {
                            cuts = new CutsLoader(filePath);
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
                        if ((short)cmd.functionParams[1] >= 0)
                        {
                            uimanager.instance.CutsSubtitle.Text = GameStrings.GetString(StringBlock, cmd.functionParams[1]);
                        }
                        else
                        {
                            uimanager.instance.CutsSubtitle.Text = ""; // clear subtitle (0xFFFF = -1)
                        }
                        if (cmd.functionParams[2] != 999)
                        {
                            var sound = vocLoader.Load(
                                System.IO.Path.Combine(
                                    BasePath, "SOUND",
                                    $"{cmd.functionParams[2]:0#}.VOC"));
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
                    break;

                case 16: // pal-copy — palette data copy
                    Debug.Print($"  Palette copy (not implemented)");
                    break;

                case 17: // timer-cb — register timer callback
                    Debug.Print($"  Timer callback (not implemented)");
                    break;

                case 19: // pal-interp — interpolate palette toward PALS.DAT target
                    {
                        palInterpTargetIndex = cmd.functionParams[0];
                        palInterpSpeed = cmd.functionParams[1];
                        palInterpStep = 0;
                        palInterpActive = true;
                        Debug.Print($"  Palette interp: target PALS.DAT[{palInterpTargetIndex}], speed={palInterpSpeed}");
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
                        vpScrollActive = false;
                        vpFrameOffset = 0;
                        vpFileMappings.Clear();
                        vpComposite = null;
                        vpCompositeClean = null;
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
                    break;

                case 27: // audio-wait
                    Debug.Print($"  Audio wait: timeout={cmd.functionParams[0]}");
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
        public static IEnumerator RunCutscene(int CutsceneNo, CallBacks.CutsceneCallBack callBackMethod = null)
        {
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
            if (CutsceneNo == 9 && _RES == GAME_UW2)
            {
                cutscontrol.Modulate = new Color(1f, 1f, 1f, 1f);
                cutscontrol.Texture = uimanager.bitmaps.LoadImageAt(6); // Origin logo
                yield return new WaitForSeconds(2.0f);
                cutscontrol.Texture = null;
                yield return new WaitForSeconds(0.5f);
                cutscontrol.Texture = uimanager.bitmaps.LoadImageAt(7); // LGS logo
                yield return new WaitForSeconds(2.0f);
                cutscontrol.Texture = null;
                yield return new WaitForSeconds(0.5f);
            }
            palInterpActive = false;

            // Load the first animation file (.N01)
            cuts = null;
            var firstFile = System.IO.Path.Combine(BasePath, "CUTS", GetsCutsceneFileName(CutsceneNo, 1));
            if (System.IO.File.Exists(firstFile))
            {
                cuts = new CutsLoader(firstFile);
            }

            // Set initial frame to black
            cutscontrol.Modulate = new Color(0f, 0f, 0f, 1f);
            uimanager.FlashColour(
                colour: 0, targetControl: cutscontrol, duration: 1, IgnoreDelay: true);

            // Process commands in segments
            int cmdIndex = 0;
            bool cutsceneRunning = true;
            bool firstSegment = true;

            bool fileChanged = false;

            while (cutsceneRunning && cmdIndex < commands.Count)
            {
                // Auto-advance to the next animation file (unless open-file changed it)
                if (!firstSegment && !fileChanged)
                {
                    int nextExt = currentFileExt + 1;
                    var nextFile = System.IO.Path.Combine(
                        BasePath, "CUTS", GetsCutsceneFileName(CutsceneNo, nextExt));
                    if (System.IO.File.Exists(nextFile))
                    {
                        cuts = new CutsLoader(nextFile);
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
                        // Fire commands scheduled for this frame
                        foreach (var cmd in scheduledCmds)
                        {
                            if (cmd.frame == frame && cmd.functionNo != 5)
                            {
                                ExecuteCommand(cmd, cutscontrol, CutsceneNo);
                                if (cmd.functionNo == 8) // open-file
                                    fileChanged = true;
                            }
                        }

                        // Display animation frame.
                        // When panorama mode is active ([si+4Fh]!=0, set by func 20
                        // when canvas != 320x200), the normal DrawBitMap call is SKIPPED
                        // (ovr108_24AB, line 443935). Instead:
                        // 1. AnimateViewportScroll shifts the VGA CRT start address
                        // 2. LPF frame decoded into persistent buffer (delta chaining)
                        // 3. DrawArtToScreen draws at fixed screen position
                        // We simulate this by cropping the LBACK composite and overlaying
                        // sprite pixels via write masks at fixed screen positions.
                        if (vpScrollActive && vpComposite != null)
                        {
                            int totalFrame = vpFrameOffset + frame;
                            var scrollRegion = GetScrollFrame(totalFrame);
                            if (scrollRegion != null)
                            {
                                ApplySpriteOverlay(scrollRegion);
                                uimanager.DisplayScrollFrame(scrollRegion, cutscontrol);
                            }
                            FrameNo++; // advance LPF frame for delta chaining
                        }
                        else if (cuts != null)
                        {
                            if (FrameNo > cuts.ImageCache.GetUpperBound(0))
                                FrameNo = 0;
                            uimanager.DisplayCutsImage(
                                cuts: cuts, imageNo: FrameNo++, targetControl: cutscontrol,
                                cropHeight: SceneDisplayH);
                        }
                        yield return new WaitForSeconds(frameTime);
                    }

                    // Execute commands at the frame-set boundary frame number.
                    // These fire after the last displayed frame (frame-set frame
                    // is not displayed per DPaint LPF format). Typically fade or
                    // open-file commands that carry over into the next segment.
                    foreach (var cmd in scheduledCmds)
                    {
                        if (cmd.frame == segmentFrameCount && cmd.functionNo != 5)
                        {
                            ExecuteCommand(cmd, cutscontrol, CutsceneNo);
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
                                    ExecuteCommand(cmd, cutscontrol, CutsceneNo);
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
                                        cropHeight: SceneDisplayH);
                                }
                            }
                            yield return new WaitForSeconds(frameTime);
                        }
                    }
                    else
                    {
                    // All commands at frame 0: execute sequentially (blocking)
                    foreach (var cmd in scheduledCmds)
                    {
                        ExecuteCommand(cmd, cutscontrol, CutsceneNo);

                        // Blocking waits
                        if (cmd.functionNo == 3) // pause
                        {
                            yield return new WaitForSeconds(cmd.functionParams[0] / 2f);
                        }
                        else if (cmd.functionNo == 14) // wait-secs
                        {
                            yield return new WaitForSeconds(cmd.functionParams[0]);
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
                                yield return new WaitForSeconds(stepTime);
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
                                yield return new WaitForSeconds(stepTime);
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
                                        cuts: cuts, imageNo: FrameNo++, targetControl: cutscontrol);
                                }
                                yield return new WaitForSeconds(0.2f);
                            }
                        }
                        else if (cmd.functionNo == 7) // rep-seg
                        {
                            Debug.Print($"  Rep-seg: repeat {cmd.functionParams[0]} times");
                        }
                    }
                    } // end else (all commands at frame 0)
                } // end else (no frame-set)

                // Execute post-animation (frame=999) commands with blocking
                foreach (var cmd in postCmds)
                {
                    ExecuteCommand(cmd, cutscontrol, CutsceneNo);

                    if (cmd.functionNo == 9 && cmd.functionParams[0] > 0) // fade-out
                    {
                        float duration = 2.0f / cmd.functionParams[0];
                        int steps = 10;
                        float stepTime = duration / steps;
                        for (int i = 1; i <= steps; i++)
                        {
                            float t = (float)i / steps;
                            cutscontrol.Modulate = new Color(1f - t, 1f - t, 1f - t, 1f);
                            yield return new WaitForSeconds(stepTime);
                        }
                    }
                    else if (cmd.functionNo == 10 && cmd.functionParams[0] > 0) // fade-in
                    {
                        float duration = 2.0f / cmd.functionParams[0];
                        int steps = 10;
                        float stepTime = duration / steps;
                        for (int i = 1; i <= steps; i++)
                        {
                            float t = (float)i / steps;
                            cutscontrol.Modulate = new Color(t, t, t, 1f);
                            yield return new WaitForSeconds(stepTime);
                        }
                    }
                }

                if (isEndCutscene)
                {
                    cutsceneRunning = false;
                }

                cmdIndex = scanIndex;
            }

            if (callBackMethod != null)
            {
                callBackMethod();
            }

            yield return null;
        }

    } // end class
} // end namespace
