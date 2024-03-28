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
            if (_RES==GAME_UW2)
            {
                return GetAt(0x7A+variableno*2);
            }
            else
            {
                return GetAt(0x71+variableno);
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
            if (_RES==GAME_UW2)
            {
               SetAt(0x7A+variableno*2, (byte)value);
            }
            else
            {
                SetAt(0x71+variableno, (byte)value);
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
                    int offset = 0x66 + (questno / 4) * 4;
                    int bit = questno % 4;
                    byte existingValue = GetAt(offset);
                    byte mask = (byte)(1<< bit);
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
                    byte mask = (byte)(1<< bit);
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


        public static bool GotKeyOfTruth
        {
            get
            {
                return ((GetAt(0x61) >>6 ) & 1) == 1;
            }
            set
            {
                var tmp = GetAt(0x61);
                tmp |= (1<<6);               
                SetAt(0x61, tmp);
            }
        }

        public static bool GotCupOfWonder
        {
            get
            {
                return ((GetAt(0x61) >>7 ) & 1) == 1;
            }
            set
            {
                var tmp = GetAt(0x61);
                tmp |= (1<<7);               
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
            return ((1<<worldno) & WorldsVisited) !=0;
        }
    }//end class
}//end namespace