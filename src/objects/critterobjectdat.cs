namespace Underworld
{
    /// <summary>
    /// Object.dat information for npcs.
    /// </summary>
    public class critterObjectDat:objectDat
    {
        const int offset = 0x132;

        /// <summary>
        /// The level of the creature.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int level (int item_id)
        {
            return buffer[offset + (item_id & 0x3F) * 48];
        }

        /// <summary>
        /// Array of 3 values representing the locational protection of armour on the NPC. 
        /// Suspect this means helm, body and leggings protection
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="bodypart">0, 1 or 2</param>
        /// <returns></returns>
        public static int protection(int item_id, int bodypart)
        {
            return buffer[offset + 1 + bodypart + (item_id & 0x3F) * 48];
        }

        /// <summary>
        /// The average hit points for the creation. Used for estimating how close the npc is to death in combat
        /// and for spawning new NPCs of that type.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int avghit(int item_id)
        {
            return buffer[offset + 4 + (item_id & 0x3F) * 48 ];
        }


        /// <summary>
        /// NPCs strength stat
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int strength(int item_id)
        {
            return buffer[offset + 5 + (item_id & 0x3F) * 48 ];
        }

        /// <summary>
        /// NPCs dexterity stat
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int dexterity(int item_id)
        {
            return buffer[offset + 6 + (item_id & 0x3F) * 48 ];
        }

        /// <summary>
        /// NPCs intelligence stat
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int intelligence(int item_id)
        {
            return buffer[offset + 7 + (item_id & 0x3F) * 48 ];
        }


    /// AND SO ON!!


    } //end class
}//end namespace