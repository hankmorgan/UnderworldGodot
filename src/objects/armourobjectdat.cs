namespace Underworld
{
    /// <summary>
    /// Class for armour properties
    /// </summary>
    public class armourObjectDat:objectDat
    {
        const int offset = 0xb2;


        /// <summary>
        /// The protection provided by this armour
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public int protection (int item_id)
        {
            return buffer[offset + (item_id & 0x3f) * 8];
        }

        /// <summary>
        /// The durability of the armour
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public int durability (int item_id)
        {
            return buffer[offset + 1 + (item_id & 0x3f) * 8];
        }

        /// <summary>
        /// The slot this armour fits into
        ///      00: shield
        ///      01: body armour
        ///      03: leggings
        ///      04: gloves
        ///      05: boots
        ///      08: helm
        ///      09: ring
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public int slot (int item_id)
        {
            return buffer[offset + 1 + (item_id & 0x3f) * 8];
        }
    }
}//end namespace