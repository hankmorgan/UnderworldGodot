namespace Underworld
{
    /// <summary>
    /// Class for loading the weapon data from objects.dat
    /// </summary>
    public class weaponObjectDat : objectDat
    {
        const int offset = 2; //2 + 0

        /// <summary>
        /// Damage modifier for Slash attack
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int slash(int item_id)
        {
            return buffer[offset + (item_id & 0xf) * 8];
        }

        /// <summary>
        /// Damage modifier for Bash attack
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int bash(int item_id)
        {
            return buffer[offset + 1 + (item_id & 0xf) * 8];
        }

        /// <summary>
        /// Damage modifier for stab attack
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int stab(int item_id)
        {
            return buffer[offset + 2 + (item_id & 0xf) * 8];
        }

        /// <summary>
        /// Possibly the charge value the weapon strike starts at.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int mincharge(int item_id)
        {
            return buffer[offset + 3 + (item_id & 0xf) * 8];
        }

        /// <summary>
        /// The rate at which the weapon charge builds up
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int chargespeed(int item_id)
        {
            return buffer[offset + 4 + (item_id & 0xf) * 8];
        }
        
        /// <summary>
        /// The max charge the weapon can build up
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int maxcharge(int item_id)
        {
            return buffer[offset + 5 + (item_id & 0xf) * 8];
        }

        /// <summary>
        /// The skill needed for this weapon 
        /// skill type (3: sword, 4: axe, 5: mace, 6: unarmed)
        /// </summary>        ///  
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int skill(int item_id)
        {
            return buffer[offset + 6 + (item_id & 0xf) * 8];
        }

        /// <summary>
        /// The weapon durability.
        /// A value of 0xFF means the weapon is invulnerable to wear and tear.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int durability(int item_id)
        {
            return buffer[offset + 7 + (item_id & 0xf) * 8];
        }

    }//end class
}