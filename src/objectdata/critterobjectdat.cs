namespace Underworld
{
    /// <summary>
    /// Object.dat information for npcs.
    /// UNTESTED. USE AT OWN RISK!!
    /// </summary>
    public class critterObjectDat:objectDat
    {
        const int offset = 0x132;

        //Return the full offset in the data buffer containing the critter info
        public static int CritterOffset(int item_id)
        {
            return offset + (item_id & 0x3F) * 48;
        }

        /// <summary>
        /// The level of the creature.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int level (int item_id)
        {
            return buffer[CritterOffset(item_id)];
        }

        /// <summary>
        /// Array of 4 values representing the locational damage reduction provided by body part armour on the NPC. 
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="bodypart">0, 1 or 2</param>
        /// <returns></returns>
        public static sbyte toughness(int item_id, int bodypart)
        {
            return (sbyte)buffer[CritterOffset(item_id) + bodypart];
        }

        /// <summary>
        /// The average hit points for the creation. Used for estimating how close the npc is to death in combat
        /// and for spawning new NPCs of that type to set their initial HP.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int avghit(int item_id)
        {
            return buffer[CritterOffset(item_id) + 0x4];
        }


        /// <summary>
        /// NPCs strength stat
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int strength(int item_id)
        {
            return buffer[CritterOffset(item_id) + 0x5];
        }

        /// <summary>
        /// NPCs dexterity stat
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int dexterity(int item_id)
        {
            return buffer[CritterOffset(item_id) + 0x6];
        }

        /// <summary>
        /// NPCs intelligence stat
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int intelligence(int item_id)
        {
            return buffer[CritterOffset(item_id) + 0x7];
        }

        /// <summary>
        /// If not equal to 0 the npc can bleed
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int bleed(int item_id)
        {
            return (buffer[CritterOffset(item_id) + 0x8] >> 3) & 0x3;
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
            return (buffer[CritterOffset(item_id) + 0x8] >> 5) & 0x7;
        }


        /// <summary>
        /// Index no of a (likely) damage vulnerability this npc has
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int maybepoisonvulnerability(int item_id)
        {
            return (buffer[CritterOffset(item_id) + 0x8] >> 3) & 0x3;
        }

        /// <summary>
        /// The sound to play on this NPCs death
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int deathsound(int item_id)
        {
            return (buffer[CritterOffset(item_id) + 0x8]) & 0x7;
        }


        public static int generaltype(int item_id)
        {
           // Debug.Print("Race. Needs to be reconfirmed");
            return buffer[CritterOffset(item_id) + 0x9];
        }

        /// <summary>
        /// The race this npc is part of.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int race(int item_id)
        {
            return (buffer[CritterOffset(item_id) + 0x9]) & 0x3F;
        }


        /// <summary>
        /// The player weapon will get damaged on a crit miss against this enemy.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static bool damagesWeaponOnCritMiss(int item_id)
        {   
            return ((buffer[CritterOffset(item_id) + 0xA]) & 0x1) == 1;
        }

        /// <summary>
        /// Unknown meaning. If set critter cannot be summoned in a spell?
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static bool unkPassivenessProperty(int item_id)
        {   
            return ((buffer[CritterOffset(item_id) + 0xA] >> 1) & 0x1) == 1;
        }
        

        /// <summary>
        /// True if NPC is a swimmer.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static bool isSwimmer(int item_id)
        {   
            return ((buffer[CritterOffset(item_id) + 0xA] >> 6) & 0x1) == 1;
        }
        
        /// <summary>
        /// True if NPC is a flier
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static bool isFlier(int item_id)
        {
            return ((buffer[CritterOffset(item_id) + 0xA] >> 7) & 0x1) == 1;
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
            return (buffer[CritterOffset(item_id) + 0xA] >> 2) & 0x3F;
        }

        /// <summary>
        /// Returns the movement speed of the NPC
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int speed(int item_id)
        {
            return buffer[CritterOffset(item_id) + 0xC];
        }

        /// <summary>
        /// Used for conversations. not to be confused with the regular level variable??
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int npc_level(int item_id)
        {
            return buffer[CritterOffset(item_id) + 0xD] & 0xF;
        }

        /// <summary>
        /// Used to calculate how good the NPC is at appraising trades offered to it.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int TradeAppraisal(int item_id)
        {
            return ((buffer[CritterOffset(item_id) + 0xD])>>4) & 0xF;
        }

        /// <summary>
        /// Used to calculate what trade value evaluation score will a trade offer be accepted.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int TradeThreshold(int item_id)
        {
             return buffer[CritterOffset(item_id) + 0xE] & 0xF;            
        }


        /// <summary>
        /// How much tolerance the NPC has to bad or insulting trade offers.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int TradePatience(int item_id)
        {
             return (buffer[CritterOffset(item_id) + 0xE]>>4) & 0xF;            
        }

        /// <summary>
        /// The poison damage the npc is capable of
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int poisondamage(int item_id)
        {
            return buffer[CritterOffset(item_id) + 0xF];
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
            return (buffer[CritterOffset(item_id) + 0x10] ) & 0xF;
        }


        /// <summary>
        /// How much equipment damage the NPC is capable of applying?
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int equipmentdamage(int item_id)
        {
            return buffer[CritterOffset(item_id) + 0x11] ;
        }   

        /// <summary>
        /// The defence stat for the NPC
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int defence(int item_id)
        {
            return buffer[CritterOffset(item_id) + 0x12] ;
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
            return buffer[CritterOffset(item_id) + 0x13 + (attackno*3)] ;
        }

        /// <summary>
        /// The base damage applied by the attack. 
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="attackno"></param>
        /// <returns></returns>
        public static int attackdamage(int item_id, int attackno)
        {
            return buffer[CritterOffset(item_id) + 0x14 + (attackno*3)] ;
        }

        /// <summary>
        /// Same as attack 1 type?
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int npc_arms(int item_id)
        {
            return buffer[CritterOffset(item_id) + 0x13];
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
            return buffer[CritterOffset(item_id) + 0x15 + (attackno*3)] ;//TODO double check this
        }

        /// <summary>
        /// Likely how far away the npc will search (or maybe follow the player)
        /// This value squared + 3 is used to compare with player distance from npc.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int probablydetectionrange(int item_id)
        {
            return buffer[CritterOffset(item_id) + 0x1c] ;
        }

        /// <summary>
        /// Gets the item id of the loot item at the specified index (0 to 2)
        /// If loot is not enabled return -1 for no loot dropped
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="loot_no"></param>
        /// <returns></returns>
        public static int weaponloot(int item_id, int loot_no)
        {
            bool enabled = (buffer[CritterOffset(item_id) + 0x20 + loot_no] & 0x1) == 1;
            if (enabled)
            {
                //get initial value
                var val = 0x7f & buffer[CritterOffset(item_id) + 0x20 + loot_no] >> 1;
                return (((val >> 4) & 0x3)<<4) + (val & 0xf);  
            }
            return -1; // no loot item.
        }

        /// <summary>
        /// The other loot will spawn. Higher values are better. Vs an Rng(0-16)
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="loot_no"></param>
        /// <returns></returns>
        public static int otherloot_probability(int item_id, int loot_no)
        {
            var val = (int)getAt(buffer,CritterOffset(item_id) + 0x22 + (loot_no * 2), 16);
            return val & 0xF;            
        }

        /// <summary>
        /// The item to be spawned as additional loot by this critter. May include bones and remains
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="loot_no"></param>
        /// <returns></returns>
        public static int otherloot_item(int item_id, int loot_no)
        {
            var val = (int)getAt(buffer,CritterOffset(item_id) + 0x22 + (loot_no*2), 16);
            return (val >> 4) & 0xFFF;            
        }


        public static int foodloot_probability(int item_id)
        {
            var val = buffer[CritterOffset(item_id) + 0x27];
            return val & 0xF;            
        }

        /// <summary>
        /// The food item to be dropped by the critter
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="loot_no"></param>
        /// <returns></returns>
        public static int foodloot_item(int item_id)
        {
            var val = buffer[CritterOffset(item_id) + 0x27];
            return (val >> 4) & 0xF;            
        }

        /// <summary>
        /// This value is used in relation to loot drops
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int valuable_loot_probability(int item_id)
        {
            return (buffer[CritterOffset(item_id) + 0x26] >>4 ) & 0xF;
        }

    public static int valuable_multipleprobability(int item_id)
        {
            return (buffer[CritterOffset(item_id) + 0x26]) & 0xF;
        }

        /// <summary>
        /// Experience rewarded for killing this npc
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int experience(int item_id)
        {
            return (int)getAt(buffer, CritterOffset(item_id) + 0x28, 16);
        }

        /// <summary>
        /// If the NPC is a spell caster this will return the spells cast by the npc
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="spell_index"></param>
        /// <returns></returns>
        public static int spell (int item_id, int spell_index)
        {
            return buffer[CritterOffset(item_id) + 0x2a + spell_index] ;
        }



        /// <summary>
        /// Used in study monster and in the npc_power calculation
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int UNK0x2DBits1To7( int item_id)
        {
            return (buffer[CritterOffset(item_id) + 0x2D]>>1) & 0x7f;
        }
        /// <summary>
        /// Returns true if the NPC uses a spell list
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static bool isCaster( int item_id)
        {
            return (buffer[CritterOffset(item_id) + 0x2D] & 0x1) == 1;
        }

        /// <summary>
        /// The NPCs ability skill at opening doors
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int dooropenskill(int item_id) //possibly this is not just door open skill...
        {
            return buffer[CritterOffset(item_id) + 0x2E] ;
        }

    } //end class
}//end namespace