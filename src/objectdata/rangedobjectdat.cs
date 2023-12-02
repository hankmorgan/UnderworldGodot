namespace Underworld
{

    /// <summary>
    /// Class for ranged weapons and ranged projectiles. Some values are not relevant depending on the type
    /// </summary>
    public class rangedObjectDat : objectDat
    {
        static int offset = 0x82;

        /// <summary>
        /// Return the damage for this projectle
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int damage(int item_id)
        {
            return buffer[offset + (item_id & 0xf) * 3];
        }

        /// <summary>
        /// Returns the item id of ammo used by this ranged weapon
        /// To keep it simple the actual value is transcoded value returned is index + 16
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int ammotype(int item_id)
        {
            return buffer[offset + 1 + (item_id & 0xf) * 3] + 16;
        }
    }

}//end namespace