namespace Underworld
{

    /// <summary>
    /// Class for ranged weapons and ranged projectiles. Some values are not relevant depending on the type
    /// </summary>
    public class rangedObjectDat : objectDat
    {
        static int offset = 0x82;

        private static int PTR(int item_id)
        {
            return offset + (item_id & 0xf) * 3;
        }

        /// <summary>
        /// Return the damage for this projectle
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int damage(int item_id)
        {
            return buffer[PTR(item_id)];
        }

        /// <summary>
        /// Returns the item id (offset by 16) of ammo used by this ranged weapon
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int ammotype(int item_id)
        {
            return buffer[PTR(item_id) + 1];
        }

        /// <summary>
        /// Probably flags if this is ammo itself or not
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int RangedWeaponType(int item_id)
        {
            return buffer[PTR(item_id) + 2];
        }



    }

}//end namespace