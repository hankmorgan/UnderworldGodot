using System;
using System.Collections.Generic;

namespace Underworld
{
    /// <summary>
    /// Serialises playerdat.pdat to on-disk player.dat format, applying the
    /// game-specific encryption. UW1 encrypts bytes 1..210 (0xD2) via XOR;
    /// UW2 encrypts bytes 1..0x37D via an 80-byte chained cipher and copies
    /// 0x37E..EOF verbatim. Inventory lives in pdat from InventoryPtr onward.
    /// See src/player/playerdatutil.cs for the inverse (load) routines; the
    /// EncryptDecryptUW1 and EncryptDecryptUW2 functions there are symmetric.
    ///
    /// For UW1 we remap the inventory into DOS-canonical order before
    /// encrypting: top-level items (BP0..BP7 containers + equipped paperdoll
    /// items) form one unified next-chain starting at slot 1, with container
    /// contents chained internally via link/next. Paperdoll and BP pointers
    /// are rewritten to the new slot indices. Without this pass, DOS UW.EXE
    /// loads bag-only but truncates the chain before reaching equipped items.
    /// </summary>
    public static class PlayerDatWriter
    {
        // UW1 paperdoll slot-pointer offsets in emission order.
        private static readonly int[] Uw1PaperdollOffsets =
        {
            0xF8, // Helm
            0xFA, // ChestArmour
            0xFC, // Gloves
            0xFE, // Leggings
            0x100,// Boots
            0x102,// RightShoulder
            0x104,// LeftShoulder
            0x106,// RightHand
            0x108,// LeftHand
            0x10A,// RightRing
            0x10C,// LeftRing
        };

        private const int Uw1BpOffsetBase = 0x10E;
        private const int Uw1BpCount = 8;

        public static byte[] Serialize()
        {
            if (Loader._RES == Loader.GAME_UW2)
            {
                // UW2 save format not yet DOS-round-trip verified; keep legacy
                // straight-copy path until UW2 reference data is captured.
                return SerializeLegacy();
            }

            return SerializeUw1Canonical();
        }

        private static byte[] SerializeLegacy()
        {
            int lastSlot = LastPopulatedInventorySlot();
            int fileLen = playerdat.InventoryPtr + lastSlot * 8;
            byte[] plain = new byte[fileLen];
            Array.Copy(playerdat.pdat, plain, fileLen);
            byte seed = plain[0];
            return Loader._RES == Loader.GAME_UW2
                ? playerdat.EncryptDecryptUW2(plain, seed)
                : playerdat.EncryptDecryptUW1(plain, seed);
        }

        private static byte[] SerializeUw1Canonical()
        {
            // 1. Gather top-level source slots: BP0..BP7 first, then paperdoll.
            var topLevel = new List<int>();
            for (int bp = 0; bp < Uw1BpCount; bp++)
            {
                int s = GetSlotPtr(playerdat.pdat, Uw1BpOffsetBase + bp * 2);
                if (s != 0) topLevel.Add(s);
            }
            foreach (int off in Uw1PaperdollOffsets)
            {
                int s = GetSlotPtr(playerdat.pdat, off);
                if (s != 0) topLevel.Add(s);
            }

            // 2. Depth-first walk from each top-level root. Each visited source
            //    slot is assigned a new slot index in emission order.
            var order = new List<int>();         // new_slot_index -> source_slot
            var remap = new Dictionary<int, int>(); // source_slot -> new_slot_index
            remap[0] = 0;
            var topLevelNewSlots = new List<int>();

            foreach (int root in topLevel)
            {
                if (remap.ContainsKey(root)) continue; // shouldn't happen; defensive
                int rootNew = VisitDfs(root, order, remap);
                topLevelNewSlots.Add(rootNew);
            }

            int newLast = order.Count;
            int fileLen = playerdat.InventoryPtr + newLast * 8;
            byte[] plain = new byte[fileLen];
            Array.Copy(playerdat.pdat, plain, playerdat.InventoryPtr);

            // 3. Emit each slot's 8 bytes at its new location, remapping next/link.
            for (int newIdx = 1; newIdx <= newLast; newIdx++)
            {
                int srcSlot = order[newIdx - 1];
                int srcOff = playerdat.InventoryPtr + (srcSlot - 1) * 8;
                int dstOff = playerdat.InventoryPtr + (newIdx - 1) * 8;
                Array.Copy(playerdat.pdat, srcOff, plain, dstOff, 8);

                int srcNext = ExtractBits10(playerdat.pdat, srcOff + 4, 6);
                int srcLink = ExtractBits10(playerdat.pdat, srcOff + 6, 6);
                int newNext = remap.TryGetValue(srcNext, out var nn) ? nn : 0;
                int newLink = remap.TryGetValue(srcLink, out var nl) ? nl : 0;
                WriteBits10(plain, dstOff + 4, 6, newNext);
                WriteBits10(plain, dstOff + 6, 6, newLink);
            }

            // 4. Overwrite next for each top-level slot to form the unified chain.
            for (int i = 0; i < topLevelNewSlots.Count; i++)
            {
                int myNew = topLevelNewSlots[i];
                int myOff = playerdat.InventoryPtr + (myNew - 1) * 8;
                int nextTop = (i + 1 < topLevelNewSlots.Count) ? topLevelNewSlots[i + 1] : 0;
                WriteBits10(plain, myOff + 4, 6, nextTop);
            }

            // 5. Rewrite paperdoll + BP pointers to new slot indices.
            for (int bp = 0; bp < Uw1BpCount; bp++)
            {
                int off = Uw1BpOffsetBase + bp * 2;
                int oldSlot = GetSlotPtr(playerdat.pdat, off);
                int newSlot = remap.TryGetValue(oldSlot, out var v) ? v : 0;
                SetSlotPtr(plain, off, newSlot);
            }
            foreach (int off in Uw1PaperdollOffsets)
            {
                int oldSlot = GetSlotPtr(playerdat.pdat, off);
                int newSlot = remap.TryGetValue(oldSlot, out var v) ? v : 0;
                SetSlotPtr(plain, off, newSlot);
            }

            byte seed = plain[0];
            return playerdat.EncryptDecryptUW1(plain, seed);
        }

        // Depth-first: assign new slot to src, then walk contents (src.link +
        // sibling chain via .next) recursively. Returns src's new slot index.
        private static int VisitDfs(int src, List<int> order, Dictionary<int, int> remap)
        {
            order.Add(src);
            int newIdx = order.Count;
            remap[src] = newIdx;

            int child = ExtractBits10(playerdat.pdat, InvOff(src) + 6, 6); // src.link
            while (child != 0 && !remap.ContainsKey(child))
            {
                VisitDfs(child, order, remap);
                child = ExtractBits10(playerdat.pdat, InvOff(child) + 4, 6); // child.next
            }
            return newIdx;
        }

        private static int InvOff(int slot) => playerdat.InventoryPtr + (slot - 1) * 8;

        // 10-bit slot pointer stored in bits 6..15 of a 16-bit word.
        private static int GetSlotPtr(byte[] buf, int off)
        {
            int w = buf[off] | (buf[off + 1] << 8);
            return (w >> 6) & 0x3FF;
        }

        private static void SetSlotPtr(byte[] buf, int off, int slot)
        {
            int w = buf[off] | (buf[off + 1] << 8);
            w = (w & 0x003F) | ((slot & 0x3FF) << 6);
            buf[off] = (byte)(w & 0xFF);
            buf[off + 1] = (byte)((w >> 8) & 0xFF);
        }

        private static int ExtractBits10(byte[] buf, int off, int shift)
        {
            int w = buf[off] | (buf[off + 1] << 8);
            return (w >> shift) & 0x3FF;
        }

        private static void WriteBits10(byte[] buf, int off, int shift, int value)
        {
            int w = buf[off] | (buf[off + 1] << 8);
            int mask = 0x3FF << shift;
            w = (w & ~mask) | ((value & 0x3FF) << shift);
            buf[off] = (byte)(w & 0xFF);
            buf[off + 1] = (byte)((w >> 8) & 0xFF);
        }

        /// <summary>
        /// Returns the highest index i where playerdat.InventoryObjects[i] refers
        /// to an object with item_id != 0. Retained for legacy path / tests.
        /// </summary>
        public static int LastPopulatedInventorySlot()
        {
            var inv = playerdat.InventoryObjects;
            for (int i = inv.GetUpperBound(0); i >= 1; i--)
            {
                if (inv[i] != null && inv[i].item_id != 0) return i;
            }
            return 0;
        }
    }
}
