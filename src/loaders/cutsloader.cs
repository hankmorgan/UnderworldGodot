using Godot;
using System.IO;

namespace Underworld
{
    public class CutsLoader : ArtLoader
    {
        private bool _alpha;//does thie file use an alpha channel

        public ImageTexture[] ImageCache = new ImageTexture[1];
        byte[] dstImage; //repeating buffer
        public byte[] FinalPixelBuffer; // raw pixel data after last displayable frame (for delta chaining)

        /// <summary>
        /// Raw indexed pixel data per frame (palette indices, not RGB).
        /// Used for palette interpolation — the frame can be re-rendered with
        /// a modified palette without re-decoding the RLE data.
        /// </summary>
        public byte[][] RawPixelData;

        /// <summary>
        /// The embedded palette from the LPF file header (offset 0x100-0x4FF).
        /// Needed to render LBACK*.BYT files which are raw indexed pixel data.
        /// </summary>
        public Palette EmbeddedPalette;

        /// <summary>
        /// Frames per second from the LPF file header (offset 0x44).
        /// Used for frame timing instead of hardcoded values.
        /// </summary>
        public int FramesPerSecond;

        /// <summary>
        /// RLE write masks for sprite mode — tracks which pixels were explicitly
        /// written (dump/run) vs skipped by the RLE decoder. Used to overlay only
        /// animated sprite pixels onto the scrolling panorama without overwriting
        /// the LBACK background. One byte per pixel: 1 = written, 0 = skipped.
        /// Only populated when decoded in sprite mode (CutsLoader(file, basePixels)).
        /// </summary>
        public byte[][] WriteMasks;
        // True when this frame's LPF record contained actual RLE writes (recordSize>4).
        // False = null-delta frame where the sprite image is identical to the prior frame.
        // Used by cutsplayer to detect "stale" sprite frames that need pan-compensation.
        public bool[] IsKeyFrame;

        /// <summary>
        /// IFF CRNG colour cycling range from LPF header (offset 0x80-0xFF).
        /// 16 entries of 8 bytes each. Used by RotatePaletteEntry (seg023_9)
        /// called from UpdatePaletteFadeTimers (ovr108_934) every frame.
        /// </summary>
        public struct CrngEntry
        {
            public int Pad;    // bytes 0-1: counter/accumulator
            public int Rate;   // bytes 2-3: cycling speed
            public int Flags;  // bytes 4-5: flags (bit 0 = active in standard IFF)
            public int Low;    // byte 6: low palette index
            public int High;   // byte 7: high palette index
        }

        public CrngEntry[] CrngRanges;

        public ShaderMaterial[] materials = new ShaderMaterial[1];
        public Shader textureshader;


        struct lpHeader
        {
            public int NoOfPages;
            public int NoOfRecords;
            public int width;
            public int height;
            public int nFrames;
            public int framesPerSecond;
            public bool hasLastDelta;  // DPaint LPF: last frame is a loop-back delta, not displayable
        };

        struct lp_descriptor
        {
            public int baseRecord;  /* Number of first record in this large page. */
            public int nRecords;  /* Number of records in lp.
                        bit 15 of "nRecords" == "has continuation from previous lp".
                        bit 14 of "nRecords" == "final record continues on next lp". */
            public int nBytes;    /* Total number of bytes of contents, excluding header. */
        };


        public CutsLoader(string File)
        {
            filePath = Path.Combine(BasePath, "CUTS", File.ToUpper());
            _alpha = UseAlpha(File);
            if (LoadImageFile())
            {
                var filename = Path.GetFileName(File);
                ReadCutsFile(
                    cutsFile: ref ImageFileData,
                    Alpha: _alpha,
                    ErrorHandling: UseErrorHandling(filename),
                    file: File);
            }
            textureshader = (Shader)ResourceLoader.Load("res://resources/shaders/uisprite.gdshader");
        }

        /// <summary>
        /// Load an LPF file in sprite mode for panorama overlay.
        /// Resets the pixel buffer to basePixels before each keyframe (record_size > 4)
        /// so that RLE skip areas contain LBACK content and writes contain animated
        /// sprite pixels. Generates WriteMasks to track which pixels were written.
        /// See "Cutscene Engine RE Notes.md" — Sprite Overlay Approach.
        /// </summary>
        public CutsLoader(string File, byte[] basePixels)
        {
            filePath = Path.Combine(BasePath, "CUTS", File.ToUpper());
            _alpha = UseAlpha(File);
            if (LoadImageFile())
            {
                var filename = Path.GetFileName(File);
                ReadCutsFile(
                    cutsFile: ref ImageFileData,
                    Alpha: _alpha,
                    ErrorHandling: UseErrorHandling(filename),
                    file: File,
                    spriteBasePixels: basePixels);
            }
            textureshader = (Shader)ResourceLoader.Load("res://resources/shaders/uisprite.gdshader");
        }

        static bool UseAlpha(string File)
        {
            switch (File.ToLower())
            {
                //case "cs400.n01"://  Look graphics for volcano
                case "cs401.n01"://   grave stones
                case "cs402.n01"://   death skulls w / silver sapling
                case "cs403.n01"://   death skulls animation
                case "cs403.n02"://   death skull end anim
                case "cs404.n01"://   anvil graphics
                case "cs410.n01"://   map piece showing some traps
                    return true;
                default:
                    return false;
            }
        }

        bool UseErrorHandling(string File)
        {//Special case for bugged file
            switch (File.ToLower())
            {
                case "cs000.n23":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Reads the cuts file.
        /// </summary>
        /// <param name="cutsFile">Cuts file.</param>
        public void ReadCutsFile(ref byte[] cutsFile, bool Alpha, bool ErrorHandling, string file,
            byte[] spriteBasePixels = null)
        {
            long addptr = 0;
            int imagecount = 0;
            Palette pal = new Palette();
            //Read in lp header. Size is 128
            /*
        The file starts with a "large page file header":

    0000   Int32   file ID, always contains "LPF "
    ...
    0006   Int16   number of large pages in the file
    0008   Int32   number of records in the file
    ...
    0010   Int32   content type, always contains "ANIM"
    0014   Int16   width in pixels
    0016   Int16   height in pixels

    The whole header is 128 bytes long. After the header color cycling info
    follows (which also is 128 bytes long), which is not used in uw1. Then
    comes the color palette:*/
            lpHeader lpH;
            lpH.NoOfPages = (int)getAt(cutsFile, 0x6, 16);
            lpH.NoOfRecords = (int)getAt(cutsFile, 0x8, 32);
            lpH.width = (int)getAt(cutsFile, 0x14, 16);
            lpH.height = (int)getAt(cutsFile, 0x16, 16);
            lpH.nFrames = (int)getAt(cutsFile, 0x40, 32);
            lpH.framesPerSecond = (int)getAt(cutsFile, 0x44, 16);
            lpH.hasLastDelta = cutsFile[0x1A] != 0;
            FramesPerSecond = lpH.framesPerSecond;
            addptr += 128;//past header.
            // Color cycling block (128 bytes): 16 entries of 8 bytes each (IFF CRNG format).
            // Parsed by ReadAnimationHeader_ovr108_751 (line 438142).
            // Applied per-frame by UpdatePaletteFadeTimers_ovr108_934 (line 438583)
            // which calls RotatePaletteEntry_seg023_9 (line 98009) for each active range.
            CrngRanges = new CrngEntry[16];
            for (int i = 0; i < 16; i++)
            {
                CrngRanges[i].Pad = ((int)getAt(cutsFile, addptr, 8) << 8) | (int)getAt(cutsFile, addptr + 1, 8);
                CrngRanges[i].Rate = ((int)getAt(cutsFile, addptr + 2, 8) << 8) | (int)getAt(cutsFile, addptr + 3, 8);
                CrngRanges[i].Flags = ((int)getAt(cutsFile, addptr + 4, 8) << 8) | (int)getAt(cutsFile, addptr + 5, 8);
                CrngRanges[i].Low = (int)getAt(cutsFile, addptr + 6, 8);
                CrngRanges[i].High = (int)getAt(cutsFile, addptr + 7, 8);
                addptr += 8;
            }

            // DPaint LPF: if hasLastDelta, the last frame is a loop-back delta
            // that resets the pixel buffer to frame 0. It should NOT be displayed.
            // FinalPixelBuffer must be captured BEFORE this frame is decoded,
            // because it provides the base for delta-chaining to the next LPF file
            // (e.g. N01→N02→N03). Each subsequent file's RLE deltas are applied
            // on top of the previous file's FinalPixelBuffer.
            int nDisplayFrames = lpH.hasLastDelta ? lpH.nFrames - 1 : lpH.nFrames;

            //Init the buffer
            dstImage = new byte[lpH.height * lpH.width]; //+ 4000];
            bool isSpriteMode = spriteBasePixels != null;
            byte[] writeMask = isSpriteMode ? new byte[lpH.height * lpH.width] : null;
            if (isSpriteMode)
            {
                // Initialize with LBACK content so RLE skip areas match the panorama
                System.Array.Copy(spriteBasePixels, dstImage,
                    System.Math.Min(spriteBasePixels.Length, dstImage.Length));
            }

            //Read in the palette
            for (int i = 0; i < 256; i++)
            {
                pal.blue[i] = (byte)getAt(cutsFile, addptr++, 8);
                pal.green[i] = (byte)getAt(cutsFile, addptr++, 8);
                pal.red[i] = (byte)getAt(cutsFile, addptr++, 8);
                addptr++;//skip reserved.
                        //pal.reserved = fgetc(fd);
            }

            EmbeddedPalette = pal;

            //Read in 256 lp descriptors
            lp_descriptor[] lpd = new lp_descriptor[256];
            for (int i = 0; i < lpd.GetUpperBound(0); i++)
            {
                lpd[i].baseRecord = (int)getAt(cutsFile, addptr, 16);
                lpd[i].nRecords = (int)getAt(cutsFile, addptr + 2, 16);
                lpd[i].nBytes = (int)getAt(cutsFile, addptr + 4, 16);
                addptr += 6;
            }
            byte[] pages = new byte[cutsFile.GetUpperBound(0) - 2816 + 1];
            for (int i = 0; i <= pages.GetUpperBound(0); i++)
            {
                pages[i] = cutsFile[i + 2816];
            }
            ImageCache = new ImageTexture[nDisplayFrames];
            RawPixelData = new byte[nDisplayFrames][];
            materials = new ShaderMaterial[nDisplayFrames];
            if (isSpriteMode)
                WriteMasks = new byte[nDisplayFrames][];
            IsKeyFrame = new bool[nDisplayFrames];
            for (int framenumber = 0; framenumber < lpH.nFrames; framenumber++)
            {
                if ((ErrorHandling == true) && (framenumber == 10))
                {//Special case crashes on a particular cutscene. (doors closing on avatar)
                    return;
                }

                int i = 0;
                for (; i < lpH.NoOfPages; i++)
                    if ((lpd[i].baseRecord <= framenumber) && (lpd[i].baseRecord + lpd[i].nRecords > framenumber))
                        break;
                addptr = (0x10000 * i);
                long curlp = addptr;
                //long page= addptr;
                lp_descriptor curl;
                curl.baseRecord = (int)getAt(pages, curlp + 0, 16);
                curl.nRecords = (int)getAt(pages, curlp + 2, 16);
                curl.nBytes = (int)getAt(pages, curlp + 4, 16);
                long thepage = curlp + 6 + 2;//reinterpret_cast<Uint8*>(curlp)+sizeof(lp_descriptor)+2 ;
                                            //long thepage = curlp;
                int destframe = framenumber - curl.baseRecord;

                int offset = 0;
                long pagepointer = thepage;
                for (int k = 0; k < destframe; k++)
                {
                    offset += (int)getAt(pages, pagepointer + (k * 2), 16);
                }
                //offset+= (int)cutsFile[k+pagepointer];//offset += pagepointer[i];
                //offset += (int)DataLoader.getValAtAddress(cutsFile,thepage,16);

                // Get the record size for this frame to detect empty delta frames
                int recordSize = (int)getAt(pages, pagepointer + (destframe * 2), 16);

                long ppointer = thepage + (curl.nRecords * 2) + offset;

                //Uint16 *ppointer16 = (Uint16*)(ppointer);
                if (cutsFile[ppointer + 1] == 0)
                {
                    ppointer += (4 + (cutsFile[ppointer + 1] + (cutsFile[ppointer + 1] & 1)));
                }
                else
                {
                    ppointer += 4;
                }
                //	byte[] imgOut ;//= //new byte[lpH.height*lpH.width+ 4000];
                // Capture FinalPixelBuffer before the loop delta frame
                if (lpH.hasLastDelta && framenumber == nDisplayFrames)
                {
                    FinalPixelBuffer = (byte[])dstImage.Clone();
                }

                // Sprite mode: reset buffer to LBACK base before each keyframe
                // so each keyframe independently shows its sprite pixels without
                // accumulation from prior keyframes. This is necessary because
                // each keyframe only updates ONE animated element (e.g. one flag
                // out of three) — without reset, all previous keyframes' writes
                // would accumulate, creating smearing artifacts.
                // Empty frames (recordSize<=4) keep previous keyframe's data
                // so the sprite persists between keyframes.
                if (isSpriteMode && recordSize > 4)
                {
                    System.Array.Copy(spriteBasePixels, dstImage,
                        System.Math.Min(spriteBasePixels.Length, dstImage.Length));
                    if (writeMask != null)
                        System.Array.Clear(writeMask, 0, writeMask.Length);
                }

                if (!SkipImage(file, imagecount))
                {
                    if (recordSize > 4) // Only decode if there's RLE data beyond the 4-byte record header
                    {
                        try
                        {
                            myPlayRunSkipDump(ppointer, pages, writeMask);//Stores in the global memory
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            // RLE decoder ran past the data boundary for this frame.
                            // Use whatever is in dstImage from the previous frame (delta buffer).
                            System.Diagnostics.Debug.Print($"  Warning: RLE decode failed for frame {imagecount} in {file}");
                        }
                    }
                    // else: empty delta frame — dstImage stays unchanged (correct for delta encoding)
                }

                // Don't store the loop delta frame in ImageCache
                if (framenumber < nDisplayFrames)
                {
                    IsKeyFrame[imagecount] = recordSize > 4;
                    if (isSpriteMode && writeMask != null)
                        WriteMasks[imagecount] = (byte[])writeMask.Clone();

                    // Store raw indexed pixel data for palette interpolation
                    RawPixelData[imagecount] = (byte[])dstImage.Clone();

                    ImageCache[imagecount++] = Image(
                        databuffer: dstImage,
                        dataOffSet: 0,
                        width: lpH.width, height: lpH.height,
                        palette: pal,
                        useAlphaChannel: Alpha,
                        useSingleRedChannel: false,
                        crop: UseCropping);
                }

            }
            // Save final pixel buffer for chaining delta-dependent files.
            // If hasLastDelta, this was captured before the loop delta above.
            // Otherwise, save after the last frame.
            if (FinalPixelBuffer == null)
            {
                FinalPixelBuffer = (byte[])dstImage.Clone();
            }
        }


        /// <summary>
        /// Decodes a cutscene file. Heavily based on Underworld Adventures Hacking Tools
        /// </summary>
        /// <param name="inptr">Inptr.</param>
        /// <param name="srcData">Source data.</param>
        /// <summary>
        /// DPaint LPF Run/Skip/Dump RLE decoder.
        /// If writeMask is provided, marks each pixel as 1 if written by a
        /// dump/run operation, 0 if skipped. This distinguishes between
        /// 'RLE explicitly wrote index 0' and 'RLE skipped this pixel',
        /// which is critical for sprite overlay during panorama scroll.
        /// </summary>
        void myPlayRunSkipDump(long inptr, byte[] srcData, byte[] writeMask = null)
        {//From an implemtation by Underworld Adventures (hacking tools)
            long outPtr = 0;

            //dstImage = new byte[size];
            while (true)
            {
                int sgn = (srcData[inptr] & 0x80) >> 7;//try and get the sign.
                if (sgn == 1)
                {
                    sgn = -1;
                }
                else
                {
                    sgn = 1;
                }
                int cnt = srcData[inptr++]; //(Sint8)*srcP++;
                                            //cnt=cnt*sgn;
                if (cnt * sgn > 0)
                {
                    // dump: copy raw pixels
                    while (cnt > 0)
                    {
                        //*dstP++ = *srcP++;
                        dstImage[outPtr] = srcData[inptr++];
                        if (writeMask != null) writeMask[outPtr] = 1;
                        outPtr++;
                        cnt--;
                    }
                }
                else if (cnt == 0)
                {
                    // run: fill with single pixel
                    //Uint8 wordCnt = *srcP++;
                    int wordCnt = srcData[inptr++];
                    //Uint8 pixel = *srcP++;
                    byte pixel = srcData[inptr++];
                    while (wordCnt > 0)
                    {
                        //*dstP++ = pixel;
                        dstImage[outPtr] = pixel;
                        if (writeMask != null) writeMask[outPtr] = 1;
                        outPtr++;
                        wordCnt--;
                    }
                }
                else
                {
                    cnt &= 0x7f; // cnt -= 0x80;
                    if (cnt != 0)
                    {
                        // shortSkip — advance without writing (preserves existing buffer content)
                        //dstP += cnt;
                        outPtr += cnt;
                    }
                    else
                    {
                        // longOp
                        //Uint16 wordCnt = *((Uint16*)srcP);
                        int wordCnt = (int)getAt(srcData, inptr, 16);//srcData[inptr];
                                                                            //srcP += 2;
                        inptr += 2;
                        int wordcntSign = (wordCnt & 0x8000) >> 15;//try and get the sign.
                        if (wordcntSign == 1)
                        {
                            wordcntSign = -1;
                        }
                        else
                        {
                            wordcntSign = 1;
                        }
                        if (wordCnt * wordcntSign <= 0)
                        {
                            // notLongSkip
                            if (wordCnt == 0)
                            {
                                break; // end loop
                            }

                            wordCnt &= 0x7fff; // wordCnt -= 0x8000; // Remove sign bit.
                            if (wordCnt >= 0x4000)
                            {
                                // longRun
                                wordCnt -= 0x4000; // Clear "longRun" bit
                                                //Uint8 pixel = *srcP++;
                                byte pixel = srcData[inptr++];
                                while (wordCnt > 0)
                                {
                                    //*dstP++ = pixel;
                                    dstImage[outPtr] = pixel;
                                    if (writeMask != null) writeMask[outPtr] = 1;
                                    outPtr++;
                                    wordCnt--;
                                }
                                //                  dstP += wordCnt;
                            }
                            else
                            {
                                // longDump
                                while (wordCnt > 0)
                                {
                                    //*dstP++ = *srcP++;
                                    dstImage[outPtr] = srcData[inptr++];
                                    if (writeMask != null) writeMask[outPtr] = 1;
                                    outPtr++;
                                    wordCnt--;
                                }

                                //                  dstP += wordCnt;
                                //                  srcP += wordCnt;
                            }
                        }
                        else
                        {
                            // longSkip — advance without writing
                            //dstP += wordCnt;
                            outPtr += wordCnt;
                        }
                    }
                }
            }
        }


        public override ImageTexture LoadImageAt(int index)
        {
            return ImageCache[index];
        }


        public ShaderMaterial GetMaterial(int textureno)
        {            
            if (materials[textureno] == null)
            {
                //materials[textureno] = new surfacematerial(textureno);
                //create this material and add it to the list
                var newmaterial = new ShaderMaterial();
                newmaterial.Shader = textureshader;
                newmaterial.SetShaderParameter("texture_albedo", (Texture)LoadImageAt(textureno,true));
                newmaterial.SetShaderParameter("albedo", new Color(1, 1, 1, 1));
                newmaterial.SetShaderParameter("uv1_scale", new Vector3(1, 1, 1));
                newmaterial.SetShaderParameter("uv2_scale", new Vector3(1, 1, 1));
                newmaterial.SetShaderParameter("UseAlpha", _alpha);
                materials[textureno] = newmaterial;
            }
            return materials[textureno];    
        }

        /// <summary>
        /// IN UW2 for some reason some frames are bugged and overwrite dstimage in the wrong location screwing up the rest of the cuts file
        /// this hack skips loading the image data for these images by not overwriting dstimage with their data.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        bool SkipImage(string file, int frame)
        {
            if (_RES == GAME_UW2)
            {
                switch(file.ToUpper())
                {
                    case "CS012.N01"://in acknowledgements only 1 in 4 frames is valid
                        return !(frame % 4 == 0);
                }
            }
                return false;
        }

    }//end class
} //end namespace