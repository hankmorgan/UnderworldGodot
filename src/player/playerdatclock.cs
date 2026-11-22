using System.Diagnostics;

namespace Underworld
{
    //Clock/time player data
    public partial class playerdat : Loader
    {


        /// <summary>
        /// The value of the clock in the player data
        /// </summary>
        public static int ClockValue
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return GetAt32(0x36A);
                }
                else
                {
                    return GetAt32(0xCF);
                }
            }
            set
            {
                if (_RES==GAME_UW2)
                {
                    SetAt32(0x36A, value);
                }
                else
                {
                    SetAt32(0xCF, value);
                }
            }
        }

        public static int game_time
        {
            get
            {
                return ClockValue / 0x3BC4;
            }
        }

        public static int game_mins
        {
            get
            {
                return game_time % 0x5A0;
            }
        }

        public static int game_days
        {
            get
            {
                return ClockValue / 0x1502e80;
            }
        }


        /// <summary>
        /// Hour for pocket watch
        /// </summary>
        public static int TwelveHourClock
        {
            get
            {
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
                return (ClockValue / 0x3C00) % 60;
            }
        }

        /// <summary>
        /// Lets seconds of game time pass (sleeping, repairing, jail). UW2 then sets the time of day and
        /// brings the schedules up to date (pass_time, uw2 WORLDEV.C:163). UW1 only adds to the clock.
        /// </summary>
        public static void AdvanceTime(int span)
        {
            Debug.Print ($"Advance time {span}");
            ClockValue += span << 8;
            if (_RES == GAME_UW2)
            {
                SetXClock(0, (ClockValue / 0x4B000) % 0x48);
                scd.ProcessSCDArk(1);
            }
        }
    }  //end class
}//end namespace