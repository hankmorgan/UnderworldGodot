using System;
using System.IO;

namespace Underworld
{
    // UW2 LEV.ARK container header layout (verified from DataLoader.LoadUWBlock):
    //
    //   Bytes 0-3:              NoOfBlocks (Int32 LE)
    //   Bytes 4-5:              2 padding/unknown bytes (0x0000 in all observed files)
    //   Bytes 6 .. 6+(N*4)-1:  offsets[N]          (N × Int32 LE; 0 = block absent)
    //   Bytes 6+(N*4) ..
    //          6+(N*8)-1:       compressionFlags[N] (N × Int32 LE; 0=none, 2=compressed)
    //   Bytes 6+(N*8) ..
    //          6+(N*12)-1:      dataLengths[N]      (N × Int32 LE; uncompressed length)
    //   Bytes 6+(N*12) ..
    //          6+(N*16)-1:      reservedSpace[N]    (N × Int32 LE)
    //   Then block data at each recorded offset.
    //
    // UW1 LEV.ARK container header layout (default case in DataLoader.LoadUWBlock):
    //   Bytes 0-1:              NoOfBlocks (Int16 LE)
    //   Bytes 2 .. 2+(N*4)-1:  offsets[N]          (N × Int32 LE; 0 = block absent)
    //   Block data follows immediately after the offset table.
    //   No per-block metadata; targetDataLen is passed in by the caller.
    //
    // For UW2 we always write uncompressed blocks (compressionFlag = 0) to avoid the
    // incomplete RepackUW2 compressor.  The loader handles uncompressed blocks at
    // DataLoader.cs:418-422.

    /// <summary>
    /// Rebuilds a LEV.ARK container from in-memory game state.
    /// Visited levels use UWTileMap.dungeons[i].lev_ark_block.Data directly.
    /// Unvisited levels pass through from LevArkLoader.lev_ark_file_data.
    /// </summary>
    public static class LevArkWriter
    {
        // UW2 per-level block size (tilemap + animation overlay)
        private const int UW2BlockSize = 0x8000;
        // UW1 per-level block size
        private const int UW1BlockSize = UWTileMap.TileMapDataSize; // 0x7C08

        /// <summary>
        /// Serialize one level block (UWBlock) to the raw bytes that should be
        /// stored in the ARK container.  For UW2 the result is exactly 0x8000 bytes;
        /// for UW1 it is TileMapDataSize bytes.
        /// </summary>
        public static byte[] SerializeLevelBlock(UWBlock block)
        {
            int targetSize = (UWClass._RES == UWClass.GAME_UW2) ? UW2BlockSize : UW1BlockSize;
            byte[] result = new byte[targetSize];
            if (block?.Data != null)
            {
                int copyLen = Math.Min(block.Data.Length, targetSize);
                Buffer.BlockCopy(block.Data, 0, result, 0, copyLen);
            }
            return result;
        }

        /// <summary>
        /// Rebuild the full LEV.ARK container from current game state.
        /// Returns the raw bytes ready to write to disk as SAVE{n}/LEV.ARK.
        /// </summary>
        public static byte[] Serialize()
        {
            return UWClass._RES == UWClass.GAME_UW2
                ? AssembleUW2Ark()
                : AssembleUW1Ark();
        }

        // -----------------------------------------------------------------------
        // UW2 writer
        // -----------------------------------------------------------------------

        private static byte[] AssembleUW2Ark()
        {
            // UW2 block layout: 80 levels × 4 slot types = 320 blocks.
            //   blocks   0..79  = tilemap+overlay for level 0..79
            //   blocks  80..159 = texmap for level 0..79
            //   blocks 160..239 = automap for level 0..79
            //   blocks 240..319 = notes for level 0..79

            int noOfBlocks = 320;

            // ---- Step 1: gather each block's raw bytes -------------------------
            byte[][] blockData = new byte[noOfBlocks][];

            for (int i = 0; i < noOfBlocks; i++)
            {
                UWBlock src = ExtractSourceBlock(i, targetLen: -1);
                blockData[i] = src?.Data; // null means absent (address == 0)
            }

            // For visited levels, replace the tilemap block (index 0..79) with live data.
            if (UWTileMap.dungeons != null)
            {
                for (int lvl = 0; lvl < UWTileMap.NO_OF_LEVELS; lvl++)
                {
                    if (UWTileMap.dungeons[lvl] != null)
                    {
                        UWBlock live = UWTileMap.dungeons[lvl].lev_ark_block;
                        if (live?.Data != null)
                        {
                            blockData[lvl] = SerializeLevelBlock(live);
                        }
                    }
                }
            }

            // Replace automap-note blocks (240..319) with the in-memory notes when non-empty.
            if (automapnote.automapsnotes != null)
            {
                for (int lvl = 0; lvl < 80; lvl++)
                {
                    if (automapnote.automapsnotes[lvl] != null && automapnote.automapsnotes[lvl].notes.Count > 0)
                    {
                        blockData[240 + lvl] = automapnote.automapsnotes[lvl].Serialize();
                    }
                }
            }

            // ---- Step 2: compute layout ----------------------------------------
            // Header size: 4 (count) + 2 (padding) + N * (4+4+4+4) = 6 + N*16
            int headerSize = 6 + noOfBlocks * 16;
            int[] offsets = new int[noOfBlocks];
            int[] flags = new int[noOfBlocks];
            int[] lengths = new int[noOfBlocks];
            int[] reserved = new int[noOfBlocks];

            // The source ARK's reservedSpace values — preserve them.
            // (They are read from source block metadata at load time.)
            byte[] source = LevArkLoader.lev_ark_file_data;
            for (int i = 0; i < noOfBlocks; i++)
            {
                // Read reservedSpace from source header if available.
                int srcReserved = 0;
                if (source != null && source.Length >= 6 + noOfBlocks * 16)
                {
                    int srcNoOfBlocks = (int)Loader.getAt(source, 0, 32);
                    if (srcNoOfBlocks == noOfBlocks)
                    {
                        srcReserved = (int)Loader.getAt(source,
                            6 + (i * 4) + (noOfBlocks * 12), 32);
                    }
                }
                reserved[i] = srcReserved;
            }

            // Assign offsets: layout all present blocks sequentially after header.
            int cursor = headerSize;
            for (int i = 0; i < noOfBlocks; i++)
            {
                if (blockData[i] != null && blockData[i].Length > 0)
                {
                    offsets[i] = cursor;
                    flags[i] = DataLoader.UW2_NOCOMPRESSION; // 0 — uncompressed
                    lengths[i] = blockData[i].Length;
                    cursor += blockData[i].Length;
                }
                else
                {
                    offsets[i] = 0;
                    flags[i] = 0;
                    lengths[i] = 0;
                }
            }

            // ---- Step 3: write output ------------------------------------------
            using var ms = new MemoryStream(cursor);
            using var bw = new BinaryWriter(ms);

            // Header: count (Int32) + padding (Int16)
            bw.Write((int)noOfBlocks);
            bw.Write((short)0); // 2 padding bytes

            // Offsets table
            for (int i = 0; i < noOfBlocks; i++) bw.Write(offsets[i]);
            // Compression flags table
            for (int i = 0; i < noOfBlocks; i++) bw.Write(flags[i]);
            // Data lengths table
            for (int i = 0; i < noOfBlocks; i++) bw.Write(lengths[i]);
            // Reserved space table
            for (int i = 0; i < noOfBlocks; i++) bw.Write(reserved[i]);

            // Block data
            for (int i = 0; i < noOfBlocks; i++)
            {
                if (blockData[i] != null && blockData[i].Length > 0)
                {
                    bw.Write(blockData[i]);
                }
            }

            return ms.ToArray();
        }

        // -----------------------------------------------------------------------
        // UW1 writer
        // -----------------------------------------------------------------------

        private static byte[] AssembleUW1Ark()
        {
            // UW1 block layout: 9 levels × 15 slot types = 135 blocks.
            //   blocks  0..8   = level tilemap
            //   blocks  9..17  = per-level overlay
            //   blocks 18..26  = texmap
            //   blocks 27..35  = automap
            //   blocks 36..44  = notes
            //   blocks 45..134 = unused

            int noOfBlocks = 135;
            byte[][] blockData = new byte[noOfBlocks][];

            // For UW1, LoadUWBlock (default case) requires the caller to supply targetDataLen
            // because the format has no per-block length metadata — only an offset table.
            // We compute each block's length as offset[i+1] - offset[i] (or fileLen - offset[i]
            // for the last present block).  This handles all block types (tilemap, overlay,
            // texmap, automap, notes) correctly without hard-coding per-type sizes.
            byte[] uw1Src = LevArkLoader.lev_ark_file_data;
            int uw1HeaderBlocks = (uw1Src != null) ? (int)Loader.getAt(uw1Src, 0, 16) : noOfBlocks;
            int uw1HeaderSize = 2 + uw1HeaderBlocks * 4;

            for (int i = 0; i < noOfBlocks; i++)
            {
                int tLen;
                if (i < 9)
                {
                    tLen = UW1BlockSize;
                }
                else if (uw1Src == null || i >= uw1HeaderBlocks)
                {
                    tLen = 0;
                }
                else
                {
                    // Read this block's offset and find the next non-zero offset to compute length.
                    int thisOff = (int)Loader.getAt(uw1Src, 2 + i * 4, 32);
                    if (thisOff == 0)
                    {
                        tLen = 0; // absent block
                    }
                    else
                    {
                        int nextOff = uw1Src.Length; // default: run to end of file
                        for (int j = i + 1; j < uw1HeaderBlocks; j++)
                        {
                            int candidate = (int)Loader.getAt(uw1Src, 2 + j * 4, 32);
                            if (candidate > thisOff)
                            {
                                nextOff = candidate;
                                break;
                            }
                        }
                        tLen = nextOff - thisOff;
                    }
                }
                UWBlock src = ExtractSourceBlock(i, targetLen: tLen);
                blockData[i] = src?.Data;
            }

            // Replace visited tilemap blocks with live dungeon data.
            if (UWTileMap.dungeons != null)
            {
                for (int lvl = 0; lvl < UWTileMap.NO_OF_LEVELS; lvl++)
                {
                    if (UWTileMap.dungeons[lvl] != null)
                    {
                        UWBlock live = UWTileMap.dungeons[lvl].lev_ark_block;
                        if (live?.Data != null)
                        {
                            blockData[lvl] = SerializeLevelBlock(live);
                        }
                    }
                }
            }

            // Replace automap-note blocks (36..44) with the in-memory notes when non-empty.
            // Without this, newly-created notes are silently lost on save because
            // ExtractSourceBlock returns the pre-play bytes from the source ARK.
            if (automapnote.automapsnotes != null)
            {
                for (int lvl = 0; lvl < 9; lvl++)
                {
                    if (automapnote.automapsnotes[lvl] != null && automapnote.automapsnotes[lvl].notes.Count > 0)
                    {
                        blockData[36 + lvl] = automapnote.automapsnotes[lvl].Serialize();
                    }
                }
            }

            // Header: 2-byte count + N × 4-byte offsets
            int headerSize = 2 + noOfBlocks * 4;
            int[] offsets = new int[noOfBlocks];
            int cursor = headerSize;
            for (int i = 0; i < noOfBlocks; i++)
            {
                if (blockData[i] != null && blockData[i].Length > 0)
                {
                    offsets[i] = cursor;
                    cursor += blockData[i].Length;
                }
                else
                {
                    offsets[i] = 0;
                }
            }

            using var ms = new MemoryStream(cursor);
            using var bw = new BinaryWriter(ms);

            bw.Write((short)noOfBlocks);
            for (int i = 0; i < noOfBlocks; i++) bw.Write(offsets[i]);
            for (int i = 0; i < noOfBlocks; i++)
            {
                if (blockData[i] != null && blockData[i].Length > 0)
                    bw.Write(blockData[i]);
            }

            return ms.ToArray();
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Extract block <paramref name="blockNo"/> from the source ARK file data.
        /// Returns null if the block is absent (address == 0).
        /// </summary>
        private static UWBlock ExtractSourceBlock(int blockNo, int targetLen)
        {
            byte[] src = LevArkLoader.lev_ark_file_data;
            if (src == null) return null;
            DataLoader.LoadUWBlock(src, blockNo, targetLen, out UWBlock uwb);
            return (uwb.DataLen > 0) ? uwb : null;
        }
    }
}
