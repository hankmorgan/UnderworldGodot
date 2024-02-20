using System;
using System.Diagnostics;

namespace Underworld
{
    //For quest variable management
    public partial class playerdat : Loader
    {

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

        /// <summary>
        /// Setting of regular quest variables.
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
            {
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
                        GetAt(0x6a + (questno - 32));
                    }
                }
            }
            return 0; //default result
        }

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
                    byte mask = (byte)(bit << 1);
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
                    byte mask = (byte)(bit << 1);
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
    }//end class
}//end namespace