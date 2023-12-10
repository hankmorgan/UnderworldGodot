namespace Underworld
{
    //Clock/time player data
    public partial class playerdat : Loader
    {
        /// <summary>
        /// Hour for pocket watch
        /// </summary>
        public static int TwelveHourClock
        {
            get
            {
                int ClockValue;
                if (_RES == GAME_UW2)
                {
                    ClockValue = GetAt32(0x36A);
                }
                else
                {
                    ClockValue = GetAt32(0xCF);
                }

                return (ClockValue / 0xE1000) % 12;
            }
        }

        /// <summary>
        /// Minute for pocket watch
        /// </summary>
        public static int Minute
        {
            get
            {
                int ClockValue;
                if (_RES == GAME_UW2)
                {
                    ClockValue = GetAt32(0x36A);
                }
                else
                {
                    ClockValue = GetAt32(0xCF);
                }
                return (ClockValue / 0x3C00) % 60;
            }
        }
    }  //end class
}//end namespace