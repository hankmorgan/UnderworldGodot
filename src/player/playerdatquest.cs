using System.Diagnostics;

namespace Underworld
{
    //For quest variable management
    public partial class playerdat : Loader
    {
        /// <summary>
        /// Gets regular game variables
        /// </summary>
        /// <param name="variableno"></param>
        /// <returns></returns>
        public static int GetGameVariable(int variableno)
        {
            if (_RES == GAME_UW2)
            {
                return GetAt(0xFA + (variableno * 2));
            }
            else
            {
                return GetAt(0x71 + variableno);
            }
        }

        /// <summary>
        /// Sets regular game variables
        /// </summary>
        /// <param name="variableno"></param>
        /// <param name="value"></param>
        public static void SetGameVariable(int variableno, int value)
        {
            value = value & 0x3F;//keep value within range.
            Debug.Print($"Setting {variableno} to {value}");
            if (_RES == GAME_UW2)
            {
                SetAt(0xFA + variableno * 2, (byte)value);
            }
            else
            {
                SetAt(0x71 + variableno, (byte)value);
            }
        }

        /// <summary>
        /// Getting of regular quest variables.
        /// </summary>
        /// <param name="questno"></param>
        /// <returns></returns>
        public static int GetQuest(int questno)
        {
            if (_RES == GAME_UW2)
            {
                if (questno <= 127)
                {//Quests are every 4 bytes. The first 4 bits are the four quests in that block of 4 bytes.
                    int offset = 0x67 + ((questno / 4) * 4);
                    int bit = questno % 4;
                    return (GetAt(offset) >> bit) & 0x1;
                }
                else
                {
                    return GetAt(0xE7 + (questno - 128));
                }
            }
            else
            {//UW1
                if (questno < 32)
                {
                    int offset = 0x66 + (questno / 8);
                    int bit = questno % 8;
                    return (GetAt(offset) >> bit) & 0x1;
                }
                else
                {
                    if (questno < 38)
                    {
                        return GetAt(0x6a + (questno - 32));
                    }
                }
            }
            Debug.Print($"Returning default variable for {questno}");
            return 0; //default result
        }


        /// <summary>
        /// Setting Quest variable value
        /// </summary>
        /// <param name="questno"></param>
        /// <param name="newValue"></param>
        public static void SetQuest(int questno, int newValue)
        {
            Debug.Print($"Setting {questno} to {newValue}");
            if (_RES == GAME_UW2)
            {
                if (questno <= 127)
                {//Quests are every 4 bytes. The first 4 bits are the four quests in that block of 4 bytes.
                    newValue = newValue & 0x1;
                    int offset = 0x67 + (questno / 4) * 4;
                    int bit = questno % 4;
                    byte existingValue = GetAt(offset);
                    byte mask = (byte)(1 << bit);
                    if (newValue >= 1)
                    {//set
                        existingValue |= mask;
                    }
                    else
                    {//unset
                        existingValue = (byte)(existingValue & (~mask));
                    }
                    SetAt(offset, existingValue);
                }
                else
                {
                    SetAt(0xE7 + (questno - 128), (byte)newValue);
                }
            }
            else
            {
                if (questno <= 31)
                {//read the quest from the bit field quests.
                    newValue = newValue & 0x1;
                    int offset = 0x66 + questno / 8;
                    int bit = questno % 8;

                    byte existingValue = GetAt(offset);
                    byte mask = (byte)(1 << bit);
                    if (newValue >= 1)
                    {//set
                        existingValue |= mask;
                    }
                    else
                    {//unset
                        existingValue = (byte)(existingValue & (~mask));
                    }
                    SetAt(offset, existingValue);
                }
                else
                {
                    SetAt(0x6a + (questno - 32), (byte)newValue);
                }
            }
        }


        /// <summary>
        /// Gets one of 15/16 xclocks which track plot progression
        /// </summary>
        /// <param name="clockno"></param>
        /// <returns></returns>
        public static int GetXClock(int clockno)
        {
            return GetAt(0x36E + clockno);
        }

        public static void SetXClock(int clockno, int value)
        {
            SetAt(0x36E + clockno, (byte)value);
        }

        public static void IncrementXClock(int clockno)
        {
            SetXClock(clockno, GetXClock(clockno) + 1);
        }


        public static bool GotKeyOfTruth
        {
            get
            {
                return ((GetAt(0x61) >> 6) & 1) == 1;
            }
            set
            {
                var tmp = GetAt(0x61);
                tmp |= (1 << 6);
                SetAt(0x61, tmp);
            }
        }

        public static bool GotCupOfWonder
        {
            get
            {
                return ((GetAt(0x61) >> 7) & 1) == 1;
            }
            set
            {
                var tmp = GetAt(0x61);
                tmp |= (1 << 7);
                SetAt(0x61, tmp);
            }
        }

        public static byte WorldsVisited
        {
            get
            {
                return GetAt(0xF4);
            }
        }
        /// <summary>
        /// Checks  if worlds has already been visited by the player
        /// </summary>
        /// <param name="worldno"></param>
        /// <returns></returns>
        public static bool HasWorldBeenVisited(int worldno)
        {
            return ((1 << worldno) & WorldsVisited) != 0;
        }

        /// <summary>
        /// The dungeon number where the silver tree is planted
        /// </summary>
        public static int SilverTreeDungeon
        {
            get
            {
                return GetAt(0x5F) >> 4;
            }
            set
            {
                value = value & 0xF;
                var tmp = GetAt(0x5F);
                tmp &= 0xF;
                tmp |= (byte)(value << 4);
                SetAt(0x5F, tmp);
            }
        }


        /// <summary>
        /// True when the player has fallen asleep under the effect of dream plants
        /// </summary>
        public static bool DreamingInVoid
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return ((GetAt(0x63) >> 1) & 1) == 1;
                }
                return false;//not applicable to UW2
            }
        }



    }//end class
}//end namespace

//quests are
/// 7.8.1 UW1 quest flags
/// 
/// flag   description
/// 
/// 0    Dr. Owl's assistant Murgo freed
/// 1    talked to Hagbard
/// 2    met Dr. Owl?
/// 3    permission to speak to king Ketchaval
/// 4    Goldthirst's quest to kill the gazer (1: gazer killed)
/// 5    Garamon, find talismans and throw into lava
/// 6    friend of Lizardman folk
/// 7    ?? (conv #24, Murgo)
/// 8    book from Bronus for Morlock has exploded
/// 9    "find Gurstang" quest
/// 10    where to find Zak, for Delanrey
/// 11    Rodrick killed
/// 
/// 32    status of "Knight of the Crux" quest
/// 0: no knight
/// 1: seek out Dorna Ironfist
/// 2: started quest to search the "writ of Lorne"
/// 3: found writ
/// 4: door to armoury opened
/// 
/// 36: No of talismans still to destroy.
/// 37: Garamon dream stages
/// 
/// UW2 Quest flags
/// 0: PT related - The troll is released
/// 1: PT related - Bishop is free?
/// 2: PT related - Borne wants you to kill bishop
/// 3: PT related - Bishop is dead
/// 
/// 6: Helena asks you to speak to praetor loth
/// 7: Loth is dead
/// 8:  Kintara tells you about Javra
/// 9:  Lobar tells you about the "virtues"
/// 10: Killed Freemis
/// 11: Listener under the castle is dead.
/// 12: used in Ice caverns to say the avatar can banish the guardians presence. Wand of altara?
/// 13: Mystral wants you to spy on altara
/// 14: Likely all lines of power have been cut.
/// 15: Altara tells you about the listener
/// 
/// 18: You learn that the Trikili can talk.
/// 19: You know Relk has found the black jewel (josie tells you)
/// 20: You've met Mokpo
/// 22: Blog is now your friend(?), cleared if you kill him
/// 23: You have used Blog to defeat dorstag
/// 24: You have won at least one duel in the arena. (Krillner does not count!)
/// 25: You have defeated Zaria in the pits
/// 26: You know about the magic scepter (for the Zoranthus)
/// 27: You know about the sigil of binding/got the djinn bottle(by bringing the scepter to zorantus)
/// 28: Took Krilner as a slave
/// 29: You know Dorstag has the gem(?)
/// 30: Dorstag refused your challenge(?)
/// 32: Met a Talorid
/// 33: You have agreed to help the talorid (historian)
/// 34: Met or heard of Eloemosynathor
/// 35: The vortz are attacking!
/// 36: Quest to clarify question for Data Integrator
/// 37: talorus related (checked by futurian)
/// 38: talorus related *bliy scup is regenerated
/// 39: talorus related
/// 40: Dialogian has helped with data integrator
/// 43: Patterson has gone postal
/// 45: Janar has been met and befriended
/// 
/// 47: You have recieve the quest from the triliki
/// 48: You have dreamed about the void
/// 49: Bishop tells you about the gem.
/// 50: The keep is going to crash.
/// 51: You have visited the ice caves (britannia becomes icy)
/// 52: Have you cut the line of power in the ice caverns
/// 53: You have killed Mokpo
/// 54: Checked by Mors Gotha? related to keep crashing
/// 55: Banner of Killorn returned (based on Scd.ark research)
/// 58: Set when meeting bishop. Bishop tells you about altara
/// 60: Probably means Goblins have been killed in the prison tower. used in check trap on second highest level
/// 61: You've brought water to Josie
/// 
/// 63: Blog has given you the gem
/// 64: Is mors dead
/// 65: Pits related (checked by dorstag)
/// 66: You have killed Mystell
/// 68: You have given the answers to nystrul and the invasion (endgame) has begun.
/// 69: you have killed Altara
/// 100: you have killed bishop
/// 104: Set when you enter scintilus level 5 (set by variable trap)
/// 105: Set when the air daemon is absorbed. (see also xclock1 and xclock3 changes)
/// 106: Got or read mors spellbook
/// 107: Set after freeing praetor loth and you/others now know about the horn.
/// 109: Set to 1 after first LB conversation. All castle occupants check this on first talk.
/// 110: Checked when talking to LB and Dupre. The guardians forces are attacking
/// 112: checked when talking to LB. You have been fighting with the others
/// 114: checked when talking to LB. The servants are restless
/// 115: checked when talking to LB. The servants are on strike
/// 116: The strike has been called off.
/// 117: Mors has been defeated in Kilorn
/// 118: The wisps tell you about the triliki
/// 119: Fizzit the thief surrenders
/// 120: checked by miranda?
/// 121: You have defeated Dorstag
/// 122: You have killed the bly scup ductosnore
/// 123: Relk is dead
/// 124 & 126 are referenced in teleport_trap
/// 128: 0-128 bit field of where the lines of power have been broken.
/// 129: How many enemies killed in the pits (also xclock 14)
/// 131: You are told that you are in the prison tower =1  
/// 	You are told you are in kilhorn keep =3
/// 	You are told you are in scintilus = 19
/// 	(this is probably a bit field.)
/// 132: Set to 2 during Kintara conversation
/// 133: How much Jospur owes you for fighting in the pits
/// 134: The password for the prison tower (random value)
/// 135: Checked by goblin in sewers  (no of worms killed on level. At more than 8 they give you fish)
/// 137: 3 is added to this when Zaria is killed by the avatar
/// 143: Set to 33 after first LB conversation. Set to 3 during endgame (this value is the cutscene to play at the end of a conversation)
/// 


/// <summary>
/// The x clocks tracks progress during the game and is used in firing events.
/// </summary>
/// The xclock is a range of 16 variables. When references by traps the index is -16 to get the below values.
/// The X Clock is tied closely to SCD.ark and the scheduled events within that file.
/// 1=Miranda conversations & related events in the castle
///     1 - Nystrul is curious about exploration.Set after entering lvl 1 from the route downwards. (set variable traps 17 may be related)
///     2 - After you visited another world.  (variable 17 is set to 16), dupre is tempted
///     3 - servants getting restless
///     4 - concerned about water, dupre is annoyed by patterson
///     5 - Dupre is bored / dupre is fetching water
///     7 - Miranda wants to talk to you pre tori murder
///     8 - tori is murdered
///     9 - Charles finds a key
///     11 - Go see Nelson
///     12 - Patterson kills Nelson
///     13 - Patterson is dead
///     14 - Gem is weak/Mors is in killorn(?)
///     15 - Nystrul wants to see you again re endgame
///     16 - Nystrul questions have been answered Mars Gotha comes
/// 2=Nystrul conversations and no of blackrock gems treated
/// 3=Djinn Capture
///     2 = balisk oil is in the mud
///     3 = bathed in oil
///     4 = baked in lava
///     5 = iron flesh cast (does not need to be still on when you break the bottle)
///     6 = djinn captured in body
/// 14=Tracks no of enemies killed in pits. Does things like update graffiti.
/// 15=Used in multiple convos. Possibly tells the game to process a change when updated?