namespace Underworld
{
    //The pits of carnage arena fights are fairly complex.
    public partial class playerdat : Loader
    {
        public static bool IsFightingInPit
        {
            get
            {
                return ((GetAt(0x64) >> 2) & 0x1) == 1;
            }
            set
            {
                var currval = GetAt(0x64);
                if (value)
                {//set
                    currval |= 4;
                }
                else
                {//clear
                    currval &= 0xFB;
                }
                SetAt(0x64, currval);
            }
        }

        /// <summary>
        /// Removes a pit fighter from player data and flags that the player is no longer fighting in the pit if that is the case.
        /// </summary>
        /// <param name="fighterToRemove"></param>
        public static bool RemovePitFighter(int fighterToRemove)
        {
            int fightercounter = 0;
            bool fighterRemoved = false;
            for (int i = 0; i < 5; i++)
            {
                var currentfighter = GetAt(0x361 + i);
                if (currentfighter != 0)
                {
                    fightercounter++;
                    if (currentfighter == fighterToRemove)
                    {
                        SetAt(0x361 + i, 0);
                        fighterRemoved = true;
                    }
                }
            }
            if (fightercounter <= 1)
            {//no longer fighting
                IsFightingInPit = false;
            }
            return fighterRemoved;
        }


        /// <summary>
        /// Checks if the specified npc is fighting against the avatar in the pits
        /// </summary>
        /// <returns></returns>
        public static bool IsDuelingAgainstCritter(int index)
        {
            for (int i = 0; i < 5; i++)
            {
                var currentfighter = GetAt(0x361 + i);
                if (currentfighter != 0)
                {
                    if (currentfighter == index)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }//end class
}//end namespace