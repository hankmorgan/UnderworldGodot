using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Object.dat information for npcs.
    /// UNTESTED. USE AT OWN RISK!!
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
        /// and for spawning new NPCs of that type to set their initial HP.
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

        /// <summary>
        /// The blood stains left behind by the npc on death.
        /// Value is offset by +217d as an index in to object lists (UW2)
        /// Value of 0 is no remains.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int fluids(int item_id)
        {
            return (buffer[offset + 8 + (item_id & 0x3F) * 48 ] >> 5) & 0x7;
        }


        /// <summary>
        /// Index no of a (likely) damage vulnerability this npc has
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int vulnerability(int item_id)
        {
            return (buffer[offset + 8 + (item_id & 0x3F) * 48 ] >> 3) & 0x3;
        }

        /// <summary>
        /// The sound to play on this NPCs death
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int deathsound(int item_id)
        {
            return (buffer[offset + 8 + (item_id & 0x3F) * 48 ]) & 0x7;
        }

        /// <summary>
        /// The race this npc is part of.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int race(int item_id)
        {
            Debug.Print("Race. Needs to be reconfirmed");
            return (buffer[offset + 9 + (item_id & 0x3F) * 48 ] >> 2) & 0x3F;
        }


        /// <summary>
        /// True if NPC is a swimmer.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static bool isSwimmer(int item_id)
        {   
            return ((buffer[offset + 10 + (item_id & 0x3F) * 48 ] >> 6) & 0x1) == 1;
        }
        
        /// <summary>
        /// True if NPC is a flier
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static bool isFlier(int item_id)
        {
            return ((buffer[offset + 10 + (item_id & 0x3F) * 48 ] >> 7) & 0x1) == 1;
        }


        /// <summary>
        /// Returns the index of the corpse dropped by this NPC
        /// Value + 0xC0 is the item_id of the corpse
        /// 0 means nothing is dropped.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int corpse(int item_id)
        {
            return (buffer[offset + 10 + (item_id & 0x3F) * 48 ] >> 2) & 0x3F;
        }

        /// <summary>
        /// Returns the movement speed of the NPC
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int speed(int item_id)
        {
            return buffer[offset + 12 + (item_id & 0x3F) * 48 ];
        }

        /// <summary>
        /// The poison damage the npc is capable of
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int poisondamage(int item_id)
        {
            return buffer[offset + 15 + (item_id & 0x3F) * 48 ];
        }

        /// <summary>
        /// A general type of the NPC
        /// 	Ethereal = 0x00 (Ethereal critters like ghosts, wisps, and shadow beasts), 
		/// 	Humanoid = 0x01 (Humanlike non-thinking forms like lizardmen, trolls, ghouls, and mages), 
		/// 	Flying = 0x02 (Flying critters like bats and imps), 
		/// 	Swimming = 0x03 (Swimming critters like lurkers), 
		/// 	Creeping = 0x04 (Creeping critters like rats and spiders), 
		/// 	Crawling = 0x05 (Crawling critters like slugs, worms, reapers (!), and fire elementals (!!)), EarthGolem = 0x11 (Only used for the earth golem), 
		///     Human = 0x51 (Humanlike thinking forms like goblins, skeletons, mountainmen, fighters, outcasts, and stone and metal golems).
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int category(int item_id)
        {//=BITAND(HEX2DEC(AE4),15)
            return (buffer[offset + 16 + (item_id & 0x3F) * 48 ] ) & 0xF;
        }


        /// <summary>
        /// How much equipment damage the NPC is capable of applying?
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int equipmentdamage(int item_id)
        {
            return buffer[offset + 17 + (item_id & 0x3F) * 48 ] ;
        }   

        /// <summary>
        /// Maybe the defence stat for the NPC
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int maybedefence(int item_id)
        {
            return buffer[offset + 18 + (item_id & 0x3F) * 48 ] ;
        }


        /// <summary>
        /// The chance an attack will hit.
        /// up to 3 attacks can be referred to. One of slash, bash or stab (order to be confirmed)
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="attackno"></param>
        /// <returns></returns>
        public static int chancetohit(int item_id, int attackno)
        {
            return buffer[offset + 19 + (attackno*3) + (item_id & 0x3F) * 48 ] ;
        }

        /// <summary>
        /// The base damage applied by the attack. 
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="attackno"></param>
        /// <returns></returns>
        public static int attackdamage(int item_id, int attackno)
        {
            return buffer[offset + 20 + (attackno*3) + (item_id & 0x3F) * 48 ] ;
        }

        /// <summary>
        /// The probability the npc will choose to make this attack.
        /// Value of 0 means the attack is not used at all.
        /// Total of all probabilities should equal to 100
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="attackno"></param>
        /// <returns></returns>
        public static int attackprobability(int item_id, int attackno)
        {
            return buffer[offset + 21 + (attackno*3) + (item_id & 0x3F) * 48 ] ;
        }

        /// <summary>
        /// Likely how far away the npc will search (or maybe follow the player)
        /// This value squared + 3 is used to compare with player distance from npc.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int probablydetectionrange(int item_id)
        {
            return buffer[offset + 28 + (item_id & 0x3F) * 48 ] ;
        }

        /// <summary>
        /// Gets the item id of the loot item at the specified index (0 to 2)
        /// If loot is not enabled return 0 for no loot dropped
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="loot_no"></param>
        /// <returns></returns>
        public static int loot(int item_id, int loot_no)
        {
            bool enabled = (buffer[offset + 32 + loot_no + (item_id & 0x3F) * 48 ] & 0x1) == 1;
            if (enabled)
            {
                //get initial value
                var val = buffer[offset + 32 + loot_no + (item_id & 0x3F) * 48 ] >> 1;
                //=BITAND(BITRSHIFT(val,4),3)+BITAND(val,15)
                return (val >> 4) & 0x3 + (val & 0xf);  // this is an odd calculation...
            }
            return 0; // no loot item.
        }

        /// <summary>
        /// Experience rewarded for killing this npc
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int experience(int item_id)
        {
            return (int)getValAtAddress(buffer, 40 + (item_id & 0x3F) * 48, 16);
        }

        /// <summary>
        /// If the NPC is a spell caster this will return the spells cast by the npc
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="spell_index"></param>
        /// <returns></returns>
        public static int spell (int item_id, int spell_index)
        {
            return buffer[offset + 42 + spell_index + (item_id & 0x3F) * 48 ] ;
        }


        /// <summary>
        /// Returns true if the NPC uses a spell list
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static bool isCaster( int item_id)
        {
            return (buffer[offset + 45 + (item_id & 0x3F) * 48 ] & 0x1) == 1;
        }

        /// <summary>
        /// The NPCs ability skill at opening doors
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int dooropenskill(int item_id)
        {
            return buffer[offset + 46 + (item_id & 0x3F) * 48] ;
        }

    } //end class
}//end namespace