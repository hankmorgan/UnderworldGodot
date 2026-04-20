using System;

namespace Underworld
{
    /// <summary>
    /// Serialises playerdat.pdat to on-disk player.dat format, applying the
    /// game-specific encryption. UW1 encrypts bytes 1..210 (0xD2) via XOR;
    /// UW2 encrypts bytes 1..0x37D via an 80-byte chained cipher and copies
    /// 0x37E..EOF verbatim. Inventory lives in pdat from InventoryPtr onward.
    /// See src/player/playerdatutil.cs for the inverse (load) routines; the
    /// EncryptDecryptUW1 and EncryptDecryptUW2 functions there are symmetric.
    /// </summary>
    public static class PlayerDatWriter
    {
        public static byte[] Serialize()
        {
            int lastSlot = LastPopulatedInventorySlot();
            // Slot i is stored at PTR = InventoryPtr + (i-1)*8 per the load loop in
            // playerdatutil.cs:Load which starts oIndex=1 at CurrentInventoryPtr=InventoryPtr.
            // So N populated slots occupy [InventoryPtr, InventoryPtr + N*8).
            int fileLen = playerdat.InventoryPtr + lastSlot * 8;

            // pdat is oversized (InventoryPtr + 512*8) after load; trim to real file length.
            byte[] plain = new byte[fileLen];
            Array.Copy(playerdat.pdat, plain, fileLen);

            byte seed = plain[0];

            return Loader._RES == Loader.GAME_UW2
                ? playerdat.EncryptDecryptUW2(plain, seed)
                : playerdat.EncryptDecryptUW1(plain, seed);
        }

        /// <summary>
        /// Returns the highest index i where playerdat.InventoryObjects[i] != null.
        /// Returns 0 if no slots are populated (slot 0 is unused by convention — see
        /// the load loop in playerdatutil.cs which starts oIndex at 1).
        /// </summary>
        public static int LastPopulatedInventorySlot()
        {
            var inv = playerdat.InventoryObjects;
            for (int i = inv.GetUpperBound(0); i >= 1; i--)
            {
                if (inv[i] != null) return i;
            }
            return 0;
        }
    }
}
