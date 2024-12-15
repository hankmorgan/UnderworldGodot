using System.Reflection.Emit;

namespace Underworld
{
    /// <summary>
    /// Trap which rotates the worlds the player will access when colliding with the blackrock gem
    /// </summary>
    public class a_hack_trap_gemrotate : hack_trap
    {
        public static void Activate()
        {
            var si = 8;
            var gamevar6 = playerdat.GetGameVariable(6);
            var di = 0;
            switch (playerdat.GetXClock(1))
            {
                case < 4:
                    si = 1; break;
                case >= 4 and < 8:
                    si = 3; break;
                case >= 8 and < 0xD:
                    si = 6; break;
            }
            si=8;
            int rngresult;
            var ax = Rng.r.Next(0x7FFF);
        
        ovr110_3f90: //loop until a valid value is found that is not the existing value in gamevar6 and is a valid value from quest 130.
            rngresult = ax % si;

            if (
                (rngresult == gamevar6)
                ||
                (
                    (
                        (rngresult != gamevar6)
                             &&
                        (((1 << rngresult) & playerdat.GetQuest(130)) != 0)
                    )
                )
                 )
            {
                di++;
                if (di - 1 < 8)
                {
                    ax = rngresult + 1;
                    goto ovr110_3f90;
                }
            }

            if (di >= 8)
            {
                rngresult = 8;
            }
            playerdat.SetGameVariable(6, rngresult);
            return;

        }
    }//end class
}//end namespace