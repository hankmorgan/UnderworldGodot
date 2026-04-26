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
            var firstChildOf = new Dictionary<int, int>(); // src_slot -> first emitted child src_slot
            remap[0] = 0;
            var topLevelNewSlots = new List<int>();

            foreach (int root in topLevel)
            {
                if (remap.ContainsKey(root)) continue; // shouldn't happen; defensive
                int rootNew = VisitDfs(root, order, remap, firstChildOf);
                topLevelNewSlots.Add(rootNew);
            }

            int newLast = order.Count;
            int fileLen = playerdat.InventoryPtr + newLast * 8;
            byte[] plain = new byte[fileLen];
            Array.Copy(playerdat.pdat, plain, playerdat.InventoryPtr);

            var topLevelNewSet = new HashSet<int>(topLevelNewSlots);

            // 3. Emit each slot's 8 bytes at its new location, remapping next/link.
            for (int newIdx = 1; newIdx <= newLast; newIdx++)
            {
                int srcSlot = order[newIdx - 1];
                int srcOff = playerdat.InventoryPtr + (srcSlot - 1) * 8;
                int dstOff = playerdat.InventoryPtr + (newIdx - 1) * 8;
                Array.Copy(playerdat.pdat, srcOff, plain, dstOff, 8);

                // Close opened sacks ONLY at top-level positions (referenced
                // by BP0..BP7 or paperdoll). UW1 toggles a sack's classindex
                // bit 0 (item_id 128 closed → 129 open) when the player
                // opens the bag UI; that open state is a UI affordance and
                // must not persist on the equipped container — DOS UW.EXE
                // sees an open BP0 sack as invalid and renders inventory
                // empty. But for sack-class items NESTED INSIDE another
                // container, item_id bit 0 is part of the genuine item
                // distinction (e.g. Hank's DOS save Save4-Carrying-Backpack
                // has id=143 in slot 3 inside the BP0 pack). Restricting
                // the close-bit toggle to top-level prevents corrupting
                // those nested values.
                if (topLevelNewSet.Contains(newIdx))
                {
                    int origItemId = (plain[dstOff] | (plain[dstOff + 1] << 8)) & 0x1FF;
                    int majorclass = origItemId >> 6;
                    int minorclass = (origItemId & 0x30) >> 4;
                    int classindex = origItemId & 0xF;
                    // Container-class items 128-143 are majorclass=2, minorclass=0
                    // but only classindex 0..0xB use the bit-0 open/closed
                    // toggle (per src/objects/container.cs:26 — open code only
                    // fires for classindex <= 0xB). Items 140-143 (incl. the
                    // runebag at 143) are distinct items with their own
                    // identities, NOT open variants of 140/142 — restricting
                    // the close-bit toggle prevents corrupting a runebag-on-
                    // paperdoll save (143 → 142 was Hank's exact diagnostic
                    // scenario for nested items; the same bug applies to
                    // top-level placement).
                    if (majorclass == 2 && minorclass == 0
                        && classindex <= 0xB
                        && (origItemId & 0x1) != 0)
                    {
                        plain[dstOff] = (byte)(plain[dstOff] & 0xFE);
                    }
                }

                int srcNext = ExtractBits10(playerdat.pdat, srcOff + 4, 6);
                srcNext = SkipEmpties(srcNext);
                int newNext = remap.TryGetValue(srcNext, out var nn) ? nn : 0;
                // link semantics depend on the is_quant flag (word0 bit 15):
                //   is_quant=1  → link is a literal quantity/special property
                //                 (NOT a slot reference). Preserve verbatim.
                //   is_quant=0  → link is a slot reference: first child for
                //                 containers (rewritten via firstChildOf so
                //                 stale pickup-time pointers are cleared),
                //                 or sp_link for leaf items (default 0).
                int word0 = playerdat.pdat[srcOff] | (playerdat.pdat[srcOff + 1] << 8);
                bool isQuant = (word0 & 0x8000) != 0;
                int newLink;
                if (isQuant)
                {
                    newLink = ExtractBits10(playerdat.pdat, srcOff + 6, 6);
                }
                else
                {
                    int firstChildSrc = firstChildOf.TryGetValue(srcSlot, out var fc) ? fc : 0;
                    newLink = (firstChildSrc != 0 && remap.TryGetValue(firstChildSrc, out var nl)) ? nl : 0;
                }
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

        // Hard cap on emitted slots — defends against a pathological source
        // chain (cycle that survives remap.ContainsKey via a free-list
        // duplicate, or a deeper-than-realistic nested-container tree) blowing
        // the stack in VisitDfs's recursion.
        private const int MaxInventoryEmit = 1024;

        // Depth-first: assign new slot to src, then walk contents (src.link +
        // sibling chain via .next) recursively. Returns src's new slot index.
        // Empty source slots (item_id=0) — left behind in the link chain by
        // pickup/drop activity that didn't compact the linked list — are
        // skipped during the walk so the serialised output has no holes.
        // Tracks the first DFS-visited child of each parent in firstChildOf
        // so the emit pass can rewrite parent.link unconditionally — clearing
        // any stale link bytes carried over from when an item was last in a
        // tile object chain (e.g. before pickup) on non-container items.
        private static int VisitDfs(int src, List<int> order, Dictionary<int, int> remap, Dictionary<int, int> firstChildOf)
        {
            // Bounds-check src BEFORE emitting it, otherwise the emit pass
            // hits Array.Copy(pdat, srcOff, ...) and throws ArgumentException
            // for out-of-range slot indices. is_quant skip + SkipEmpties
            // normally prevent OOB src reaching here, but defensive: 0 is a
            // sentinel that callers' link-rewrite logic (remap.TryGetValue
            // returning false → 0) treats as "no slot".
            int srcOff = InvOff(src);
            if (srcOff < 0 || srcOff + 8 > playerdat.pdat.Length)
            {
                return 0;
            }
            if (order.Count >= MaxInventoryEmit)
            {
                throw new InvalidOperationException(
                    $"PlayerDatWriter: inventory emission exceeded {MaxInventoryEmit} slots — " +
                    "likely a cycle or unbounded nesting in the source chain.");
            }
            order.Add(src);
            int newIdx = order.Count;
            remap[src] = newIdx;

            int linkOff = srcOff + 6;
            // is_quant items reuse the link bits to encode quantity / special
            // property (NOT a slot index — see uw-formats §4.2 and Alfred's
            // letter at item 312 with link=514 = property 2 in Hank's
            // sample save Save4-Carrying-Backpack). Following an is_quant
            // link as a child slot blew up the DFS prior to bounds-checking
            // (Hank's IndexOutOfRange report on PR #33). Even with the
            // bounds-check it's still wrong semantically — skip cleanly.
            if (IsQuantAt(src))
            {
                firstChildOf[src] = 0;
                return newIdx;
            }
            int child = ExtractBits10(playerdat.pdat, linkOff, 6); // src.link
            int firstVisited = 0;
            while (child != 0 && !remap.ContainsKey(child))
            {
                int childNextOff = InvOff(child) + 4;
                if (childNextOff + 1 >= playerdat.pdat.Length) break;
                if (ItemIdAt(child) == 0)
                {
                    // Hop past the empty slot via its .next without recursing.
                    child = ExtractBits10(playerdat.pdat, childNextOff, 6);
                    continue;
                }
                if (firstVisited == 0) firstVisited = child;
                VisitDfs(child, order, remap, firstChildOf);
                child = ExtractBits10(playerdat.pdat, childNextOff, 6); // child.next
            }
            firstChildOf[src] = firstVisited;
            return newIdx;
        }

        private static int ItemIdAt(int slot)
        {
            int o = InvOff(slot);
            // The link / next fields are 10-bit (slots 0..1023) but port pdat
            // is only allocated up to InventoryPtr + 512*8. Defensive bound
            // check belt-and-braces with the is_quant skip in VisitDfs:
            // either path on its own should prevent OOB, both together also
            // protect against any future edge case.
            if (o < 0 || o + 1 >= playerdat.pdat.Length) return 0;
            int w = playerdat.pdat[o] | (playerdat.pdat[o + 1] << 8);
            return w & 0x1FF;
        }

        private static bool IsQuantAt(int slot)
        {
            int o = InvOff(slot);
            if (o < 0 || o + 1 >= playerdat.pdat.Length) return false;
            // is_quant flag = bit 15 of word 0 = bit 7 of byte 1.
            return (playerdat.pdat[o + 1] & 0x80) != 0;
        }

        // Walk the source .next chain past any empty (item_id=0) slots and
        // return the first non-empty slot index (or 0 if the chain ends in
        // empties). Defends against unbounded chains.
        private static int SkipEmpties(int slot)
        {
            int hops = 0;
            while (slot != 0 && ItemIdAt(slot) == 0 && hops < 1024)
            {
                int o = InvOff(slot) + 4;
                if (o + 1 >= playerdat.pdat.Length) return 0;
                slot = ExtractBits10(playerdat.pdat, o, 6);
                hops++;
            }
            return slot;
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
