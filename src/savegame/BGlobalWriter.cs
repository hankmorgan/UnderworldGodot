using System.IO;

namespace Underworld
{
    /// <summary>
    /// Serializes bglobal.BablGlobal[] to BGLOBALS.DAT wire format (little-endian, no header/footer).
    /// </summary>
    public static class BGlobalWriter
    {
        /// <summary>
        /// Serialize an array of BablGlobal to BGLOBALS.DAT format.
        /// Format (little-endian, repeating until EOF):
        /// - Int16 ConversationNo
        /// - Int16 Size (number of globals in this slot)
        /// - Size × Int16 globals
        /// </summary>
        public static byte[] Serialize(bglobal.BablGlobal[] globals)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            foreach (var g in globals)
            {
                bw.Write(g.ConversationNo);
                bw.Write(g.Size);
                for (int i = 0; i < g.Size; i++)
                {
                    bw.Write(g.Globals[i]);
                }
            }

            return ms.ToArray();
        }
    }
}
