using System;
using System.IO;

namespace Underworld
{
    // Step 1 findings (investigated 2026-04-19):
    //
    // SCD.ARK is a UW2-format multi-block ARK container with exactly 16 blocks (0x10).
    // Header layout is identical to LEV.ARK (UW2 case in DataLoader.LoadUWBlock):
    //   Bytes 0-3:              NoOfBlocks (Int32 LE) = 16
    //   Bytes 4-5:              2 padding bytes (0x0000)
    //   Bytes 6 .. 6+(N*4)-1:  offsets[N]          (N × Int32 LE)
    //   Bytes 6+(N*4) ..
    //          6+(N*8)-1:       compressionFlags[N] (N × Int32 LE; all 0 = uncompressed in SAVE0)
    //   Bytes 6+(N*8) ..
    //          6+(N*12)-1:      dataLengths[N]      (N × Int32 LE)
    //   Bytes 6+(N*12) ..
    //          6+(N*16)-1:      reservedSpace[N]    (N × Int32 LE; mirrors dataLengths in SAVE0)
    //
    // File size of SAVE0/SCD.ARK: 8950 bytes.
    // Blocks 0-15 are all present and uncompressed; sizes range from 324 to 1668 bytes.
    //
    // In-memory representation: scd.scd_data is UWBlock[] of length 16.
    // Blocks are loaded lazily per-demand in ProcessSCDArk; after processing, unmodified blocks
    // are set back to null (scd.cs:67). There is NO persistent "source byte array" like
    // LevArkLoader.lev_ark_file_data — each call re-reads from disk.
    //
    // Writer strategy: read the source SCD.ARK bytes from disk (BasePath + folder + "/SCD.ARK"),
    // then for each block overlay any live scd_data[i] entry that is non-null (those are the
    // blocks the game has mutated during the session). Reassemble the UW2 ARK container
    // uncompressed, preserving reservedSpace from the source header.
    //
    // UW1 has no SCD.ARK — Serialize() returns an empty array for UW1.

    /// <summary>
    /// Rebuilds a SCD.ARK container from in-memory game state (UW2 only).
    /// Mutated blocks come from scd.scd_data[i]; unvisited blocks are preserved
    /// verbatim from the source file on disk.
    /// </summary>
    public static class ScdArkWriter
    {
        private const int ScdBlockCount = 16; // 0x10 — fixed in UW2

        /// <summary>
        /// Serialize the SCD.ARK for the given save folder.
        /// Returns an empty array for UW1 (no SCD.ARK).
        /// </summary>
        /// <param name="folder">Save-game subfolder, e.g. "SAVE0".</param>
        public static byte[] Serialize(string folder)
        {
            if (UWClass._RES != UWClass.GAME_UW2)
                return new byte[0];

            var path = Path.Combine(UWClass.BasePath, folder, "SCD.ARK");
            if (!File.Exists(path))
                return new byte[0];

            byte[] source = File.ReadAllBytes(path);

            // Read the actual block count from the source header (must equal 16).
            int noOfBlocks = (int)Loader.getAt(source, 0, 32);

            // Gather each block's raw bytes: prefer live scd_data[i] if present.
            byte[][] blockData = new byte[noOfBlocks][];
            for (int i = 0; i < noOfBlocks; i++)
            {
                // Check for a live (possibly mutated) block in memory.
                UWBlock live = (scd.scd_data != null && i < scd.scd_data.Length)
                    ? scd.scd_data[i]
                    : null;

                if (live?.Data != null && live.DataLen > 0)
                {
                    blockData[i] = live.Data;
                }
                else
                {
                    // Fall back: extract block from source bytes.
                    DataLoader.LoadUWBlock(source, i, 0, out UWBlock uwb);
                    blockData[i] = (uwb?.DataLen > 0) ? uwb.Data : null;
                }
            }

            // Preserve reservedSpace from the source header.
            int[] reserved = new int[noOfBlocks];
            if (source.Length >= 6 + noOfBlocks * 16)
            {
                for (int i = 0; i < noOfBlocks; i++)
                {
                    reserved[i] = (int)Loader.getAt(source, 6 + (i * 4) + (noOfBlocks * 12), 32);
                }
            }

            // Compute layout: header = 6 + noOfBlocks*16 bytes.
            int headerSize = 6 + noOfBlocks * 16;
            int[] offsets = new int[noOfBlocks];
            int[] flags   = new int[noOfBlocks]; // all 0 = uncompressed
            int[] lengths = new int[noOfBlocks];

            int cursor = headerSize;
            for (int i = 0; i < noOfBlocks; i++)
            {
                if (blockData[i] != null && blockData[i].Length > 0)
                {
                    offsets[i] = cursor;
                    flags[i]   = DataLoader.UW2_NOCOMPRESSION; // 0
                    lengths[i] = blockData[i].Length;
                    cursor += blockData[i].Length;
                }
                // else: offsets[i] = 0, flags[i] = 0, lengths[i] = 0 (absent block)
            }

            // Write output.
            using var ms = new MemoryStream(cursor);
            using var bw = new BinaryWriter(ms);

            bw.Write((int)noOfBlocks);
            bw.Write((short)0); // 2 padding bytes

            for (int i = 0; i < noOfBlocks; i++) bw.Write(offsets[i]);
            for (int i = 0; i < noOfBlocks; i++) bw.Write(flags[i]);
            for (int i = 0; i < noOfBlocks; i++) bw.Write(lengths[i]);
            for (int i = 0; i < noOfBlocks; i++) bw.Write(reserved[i]);

            for (int i = 0; i < noOfBlocks; i++)
            {
                if (blockData[i] != null && blockData[i].Length > 0)
                    bw.Write(blockData[i]);
            }

            return ms.ToArray();
        }
    }
}
