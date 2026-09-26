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
    //          6+(N*8)-1:       flags[N]            (N × Int32 LE, see below)
    //   Bytes 6+(N*8) ..
    //          6+(N*12)-1:      dataLengths[N]      (N × Int32 LE; bytes on disk)
    //   Bytes 6+(N*12) ..
    //          6+(N*16)-1:      availableSpace[N]   (N × Int32 LE)
    //   Then block data at each recorded offset.
    //
    // Flags: bit 0 asks DOS to compress the block when it next writes it, bit 1 says it
    // is compressed now, bit 2 says availableSpace is larger than the data, left as slack
    // for a block that may compress less well next time (uw-formats 9.1).
    //
    // UW1 LEV.ARK container header layout (default case in DataLoader.LoadUWBlock):
    //   Bytes 0-1:              NoOfBlocks (Int16 LE)
    //   Bytes 2 .. 2+(N*4)-1:  offsets[N]          (N × Int32 LE; 0 = block absent)
    //   Block data follows immediately after the offset table.
    //   No per-block metadata; targetDataLen is passed in by the caller.
    //
    // What DOS UW2 needs from a save, from UW2.EXE and from two DOS-written saves:
    //
    //   ReadArkFileBlock_ovr093_C33 reads an uncompressed block by copying dataLengths[i]
    //   bytes straight to its destination. A level goes into a buffer of exactly 0x7E08
    //   bytes (InitialiseEmptyTileMapData_ovr128_0), so a level block must not be longer.
    //   DOS itself writes every level block uncompressed at 0x7E08 with flags 0.
    //
    //   WriteDataToARKFile_ovr093_779D_4B1 overwrites a block in place only when the new
    //   data fits: within availableSpace when bit 2 is set, or exactly equal to it when
    //   bit 2 is clear. Anything else makes it rebuild the archive. So availableSpace has
    //   to describe the bytes actually reserved on disk, never more.
    //
    // The writer therefore copies every block it has not changed exactly as the source
    // holds it: compressed bytes, flags, length, available space and any slack. Blocks it
    // replaces from live state are written uncompressed with flags 0 and availableSpace
    // equal to their length, which is what DOS does for level blocks. Compression is not
    // needed for DOS to read a block, since the reader only tests bit 1.

    /// <summary>
    /// Rebuilds a LEV.ARK container from in-memory game state.
    /// Visited levels use UWTileMap.dungeons[i].lev_ark_block.Data directly.
    /// Unvisited levels pass through from LevArkLoader.lev_ark_file_data.
    /// </summary>
    public static class LevArkWriter
    {
        // UW2 per-level block size: tilemap, objects and free lists to 0x7C08, then 64
        // six-byte animation overlays to 0x7D88, then 64 timer words to 0x7E08. This is
        // the size of DOS's tilemap buffer and what DOS writes for every level.
        private const int UW2BlockSize = 0x7E08;
        // UW2 automap block: one byte per tile of a 64 × 64 map.
        private const int UW2AutomapBlockSize = 64 * 64;
        // UW1 per-level block size
        private const int UW1BlockSize = UWTileMap.TileMapDataSize; // 0x7C08
        // UW1 per-level animation-overlay block size. 64 slots × 6 bytes, matching
        // the targetDataLen LevArkLoader.LoadOverlayBlock asks for.
        private const int UW1OverlayBlockSize = 64 * 6;

        /// <summary>
        /// Serialize one level block (UWBlock) to the raw bytes that should be
        /// stored in the ARK container.  For UW2 the result is exactly 0x7E08 bytes;
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
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                PackOverlays(result, LevArkLoader.UW2OverlayStart);
            }
            return result;
        }

        /// <summary>
        /// Serialize one UW1 animation-overlay block to the fixed 384 bytes the
        /// container stores. UW1 only: UW2 keeps overlays inside the level block
        /// itself, and the demo keeps them in LEVEL13.ANX rather than in the ARK.
        /// </summary>
        public static byte[] SerializeOverlayBlock(UWBlock block)
        {
            byte[] result = new byte[UW1OverlayBlockSize];
            if (block?.Data != null)
            {
                // A short buffer just means the remaining overlay slots are absent, so
                // zero-padding is correct. An over-long one cannot be represented in 64
                // six-byte slots and means something upstream grew the buffer, so say so
                // rather than truncating in silence. Writing a well-formed block still
                // beats throwing: LEV.ARK is written after DESC, PLAYER.DAT and
                // BGLOBALS.DAT, so aborting here would leave a half-updated save slot.
                // Console.Error, not GD.PushWarning: this runs in the headless save
                // tests, and touching Godot with no engine segfaults rather than
                // throwing. Not Debug.Print or Trace.WriteLine either, since those
                // compile out without DEBUG/TRACE and the warning would vanish from
                // release builds, which is where a silent truncation matters most.
                if (block.Data.Length > UW1OverlayBlockSize)
                {
                    Console.Error.WriteLine(
                        $"LevArkWriter: UW1 overlay block is {block.Data.Length} bytes, expected {UW1OverlayBlockSize}; truncating.");
                }
                int copyLen = Math.Min(block.Data.Length, UW1OverlayBlockSize);
                Buffer.BlockCopy(block.Data, 0, result, 0, copyLen);
            }
            PackOverlays(result, 0);
            return result;
        }

        /// <summary>
        /// Rewrites an overlay list the way DOS keeps it: live records packed from slot 0
        /// in their current order, and nothing after them.
        ///
        /// DOS counts overlays from slot 0 up to the first record with no object link and
        /// ignores the rest (see LevArkLoader.DiscardOverlaysPastEndOfList). The port can
        /// leave a gap mid-list, because its removal copies slot 63 rather than the last live
        /// record over the one it removes, and a gap would hide every overlay after it from
        /// DOS. A record is live when it links an object and its duration is not 0, which is
        /// the port's own test for a free slot in GetFreeAnimoSlot.
        /// </summary>
        public static void PackOverlays(byte[] data, int start)
        {
            if (data == null || start + LevArkLoader.OverlayCount * LevArkLoader.OverlayRecordSize > data.Length) return;
            int size = LevArkLoader.OverlayRecordSize;
            byte[] packed = new byte[LevArkLoader.OverlayCount * size];
            int n = 0;
            for (int i = 0; i < LevArkLoader.OverlayCount; i++)
            {
                int p = start + i * size;
                int link = ((data[p] | (data[p + 1] << 8)) >> 6) & 0x3FF;
                int duration = (short)(data[p + 2] | (data[p + 3] << 8));
                if (link != 0 && duration != 0)
                {
                    Buffer.BlockCopy(data, p, packed, n * size, size);
                    n++;
                }
            }
            Buffer.BlockCopy(packed, 0, data, start, packed.Length);
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

            // ---- Step 1: blocks replaced from live state ------------------------
            // null means "not replaced, copy the source block as it is". An empty array
            // means "replaced by nothing", which the layout step writes as absent.
            byte[][] replaced = new byte[noOfBlocks][];

            // Visited levels: the live tilemap block.
            if (UWTileMap.dungeons != null)
            {
                int levels = Math.Min(UWTileMap.NO_OF_LEVELS, UWTileMap.dungeons.Length);
                for (int lvl = 0; lvl < levels; lvl++)
                {
                    UWBlock live = UWTileMap.dungeons[lvl]?.lev_ark_block;
                    if (live?.Data != null)
                    {
                        replaced[lvl] = SerializeLevelBlock(live);
                    }
                }
            }

            // Automaps for any level loaded this session. DOS writes one for every level
            // the player has been on; without this a UW2 save kept whatever automap the
            // source held and lost everything explored since. See issue #69.
            if (automap.automaps != null)
            {
                int levels = Math.Min(80, automap.automaps.Length);
                for (int lvl = 0; lvl < levels; lvl++)
                {
                    byte[] buffer = automap.automaps[lvl]?.buffer;
                    if (buffer != null)
                    {
                        // The loader can hand back a few bytes past the 4096 the block
                        // declares, because DataLoader.unpackUW2 finishes the control byte
                        // it is on. Only the first 4096 are the map.
                        byte[] block = new byte[UW2AutomapBlockSize];
                        Buffer.BlockCopy(buffer, 0, block, 0, Math.Min(buffer.Length, UW2AutomapBlockSize));
                        replaced[160 + lvl] = block;
                    }
                }
            }

            // Automap-note blocks (240..319) from the in-memory notes.
            // A level whose notes were all deleted serialises to an empty array, which the
            // layout step below turns into an absent block. Skipping the replacement instead
            // would write the source ARK's notes back out and they would reappear on reload.
            // automapsnotes[lvl] is only non-null for a level that has been loaded, and the
            // constructor reads the source block, so an empty list means no notes.
            if (automapnote.automapsnotes != null)
            {
                int levels = Math.Min(80, automapnote.automapsnotes.Length);
                for (int lvl = 0; lvl < levels; lvl++)
                {
                    if (automapnote.automapsnotes[lvl] != null)
                    {
                        replaced[240 + lvl] = automapnote.automapsnotes[lvl].Serialize();
                    }
                }
            }

            // ---- Step 2: compute layout ----------------------------------------
            // Header size: 4 (count) + 2 (padding) + N * (4+4+4+4) = 6 + N*16
            int headerSize = 6 + noOfBlocks * 16;
            int[] offsets = new int[noOfBlocks];
            int[] flags = new int[noOfBlocks];
            int[] lengths = new int[noOfBlocks];
            int[] available = new int[noOfBlocks];
            byte[][] onDisk = new byte[noOfBlocks][];

            byte[] source = LevArkLoader.lev_ark_file_data;
            bool sourceUsable = source != null
                && source.Length >= headerSize
                && (int)Loader.getAt(source, 0, 32) == noOfBlocks;

            int cursor = headerSize;
            for (int i = 0; i < noOfBlocks; i++)
            {
                if (replaced[i] != null)
                {
                    if (replaced[i].Length == 0) continue; // absent: offset, length, space all 0
                    onDisk[i] = replaced[i];
                    flags[i] = DataLoader.UW2_NOCOMPRESSION; // 0, as DOS writes level blocks
                    lengths[i] = replaced[i].Length;
                    available[i] = replaced[i].Length;
                }
                else if (sourceUsable)
                {
                    int srcOffset = (int)Loader.getAt(source, 6 + i * 4, 32);
                    if (srcOffset == 0) continue; // absent in the source too
                    int srcFlags = (int)Loader.getAt(source, 6 + noOfBlocks * 4 + i * 4, 32);
                    int srcLength = (int)Loader.getAt(source, 6 + noOfBlocks * 8 + i * 4, 32);
                    int srcAvailable = (int)Loader.getAt(source, 6 + noOfBlocks * 12 + i * 4, 32);

                    // A block whose data runs past the end of the source cannot be copied
                    // faithfully. Fail rather than pad it with zeros: the slot transaction
                    // then leaves the previous save in place.
                    if (srcOffset < 0 || srcLength < 0 || (long)srcOffset + srcLength > source.Length)
                    {
                        throw new InvalidDataException(
                            $"LevArkWriter: source block {i} claims {srcLength} bytes at {srcOffset}, past the end of a {source.Length}-byte archive.");
                    }

                    // An uncompressed level block longer than DOS's 0x7E08 buffer is trimmed
                    // to it. Earlier port builds wrote 0x8000, and DOS itself sometimes leaves
                    // a few bytes of junk past 0x7E08; nothing past 0x7E08 is level data.
                    if (i < 80 && (srcFlags & 2) == 0 && srcLength > UW2BlockSize)
                    {
                        srcLength = UW2BlockSize;
                        srcFlags &= ~4;
                    }

                    // With bit 2 set, DOS may later write up to srcAvailable bytes here in
                    // place, so the slack has to come with the block or that write would run
                    // into whatever the layout puts next. Without it, the space is exactly
                    // the data. Slack past the end of the file is zeros, as DOS pads it.
                    bool hasSlack = (srcFlags & 4) != 0 && srcAvailable > srcLength;
                    int reserve = hasSlack ? srcAvailable : srcLength;
                    byte[] raw = new byte[reserve];
                    int copyLen = Math.Min(reserve, source.Length - srcOffset);
                    Buffer.BlockCopy(source, srcOffset, raw, 0, copyLen);

                    onDisk[i] = raw;
                    flags[i] = srcFlags;
                    lengths[i] = srcLength;
                    available[i] = reserve;
                }
                else
                {
                    continue;
                }
                offsets[i] = cursor;
                cursor += onDisk[i].Length;
            }

            // ---- Step 3: write output ------------------------------------------
            using var ms = new MemoryStream(cursor);
            using var bw = new BinaryWriter(ms);

            // Header: count (Int32) + padding (Int16)
            bw.Write((int)noOfBlocks);
            bw.Write((short)0); // 2 padding bytes

            for (int i = 0; i < noOfBlocks; i++) bw.Write(offsets[i]);
            for (int i = 0; i < noOfBlocks; i++) bw.Write(flags[i]);
            for (int i = 0; i < noOfBlocks; i++) bw.Write(lengths[i]);
            for (int i = 0; i < noOfBlocks; i++) bw.Write(available[i]);

            for (int i = 0; i < noOfBlocks; i++)
            {
                if (onDisk[i] != null) bw.Write(onDisk[i]);
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
            // because the format has no per-block length metadata, only an offset table.
            // UW1BlockLength measures each block from the offsets, which handles all block
            // types (tilemap, overlay, texmap, automap, notes) without hard-coding per-type
            // sizes.
            byte[] uw1Src = LevArkLoader.lev_ark_file_data;
            int uw1HeaderBlocks = (uw1Src != null) ? (int)Loader.getAt(uw1Src, 0, 16) : noOfBlocks;

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
                    tLen = LevArkLoader.UW1BlockLength(uw1Src, i, uw1HeaderBlocks);
                }
                UWBlock src = ExtractSourceBlock(i, targetLen: tLen);
                blockData[i] = src?.Data;
            }

            // Replace visited tilemap blocks with live dungeon data.
            if (UWTileMap.dungeons != null)
            {
                // Same bound as the overlay loop below: dungeons is a public static
                // array, so do not assume its length agrees with NO_OF_LEVELS.
                int tileLevels = Math.Min(UWTileMap.NO_OF_LEVELS, UWTileMap.dungeons.Length);
                for (int lvl = 0; lvl < tileLevels; lvl++)
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

            // Replace overlay blocks (9..17) with the live animation-overlay data
            // for any visited level.
            //
            // Without this the two halves of an in-flight animation disagree. A
            // moving door writes its object record into the tilemap block (door.cs
            // sets item 0x1CF) and its overlay record into ovl_ark_block (animo.cs
            // stores the object link, tile and duration). Serialising only the
            // tilemap leaves a moving-door object with no matching overlay, which
            // is an internally inconsistent LEV.ARK and is what a save taken
            // mid-animation produced. See hankmorgan/UnderworldGodot#43.
            //
            // UW1 proper only. UW2 has no separate overlay block, and the demo
            // reads overlays from LEVEL13.ANX rather than from the ARK, so in
            // neither case does block 9+lvl hold overlay data to replace.
            if (UWClass._RES == UWClass.GAME_UW1 && UWTileMap.dungeons != null)
            {
                // dungeons is a public static array, so bound by its actual length as
                // well as by NO_OF_LEVELS rather than trusting the two to agree.
                int overlayLevels = Math.Min(UWTileMap.NO_OF_LEVELS, UWTileMap.dungeons.Length);
                for (int lvl = 0; lvl < overlayLevels; lvl++)
                {
                    UWBlock liveOverlay = UWTileMap.dungeons[lvl]?.ovl_ark_block;
                    if (liveOverlay?.Data != null)
                    {
                        blockData[9 + lvl] = SerializeOverlayBlock(liveOverlay);
                    }
                }
            }

            // Replace automap blocks (27..35) with the in-memory automap buffer
            // for any visited level. DOS UW.EXE writes at least block 27 (the
            // current level's automap) on every save; without it, UW.EXE treats
            // the level as un-automapped and may mis-render the automap view.
            //
            // Each per-level automap buffer is a fixed 4096 bytes (64×64 tiles
            // × 1 byte/tile of visited+display flags).
            if (automap.automaps != null)
            {
                for (int lvl = 0; lvl < 9; lvl++)
                {
                    if (automap.automaps[lvl]?.buffer != null &&
                        automap.automaps[lvl].buffer.Length == 64 * 64)
                    {
                        blockData[27 + lvl] = automap.automaps[lvl].buffer;
                    }
                }
            }

            // Replace automap-note blocks (36..44) with the in-memory notes.
            // Without this, newly-created notes are silently lost on save because
            // ExtractSourceBlock returns the pre-play bytes from the source ARK. The same
            // applies in reverse to deletion: a level whose notes were all deleted
            // serialises to an empty array, which the layout step turns into an absent
            // block, matching how the shipped archive represents a level with no notes.
            if (automapnote.automapsnotes != null)
            {
                for (int lvl = 0; lvl < 9; lvl++)
                {
                    if (automapnote.automapsnotes[lvl] != null)
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
