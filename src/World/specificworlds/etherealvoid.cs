
using System;

namespace Underworld
{
    /// <summary>
    /// Specific code for the ethereal void (UW1)
    /// </summary>
    public class Etherealvoid:UWClass
    {
        
        /// <summary>
        /// Codes runs on a random chance every no of frames
        /// Causes light flashes and drains player health during the finale of UW1.
        /// </summary>
        public static void EtherealVoidEndGameSpecialEffects()
        {
            
            uimanager.FlashColour(0xB5, uimanager.Cuts3DWin, 0.05f);

            if (playerdat.play_hp > 0x64)
            {
                playerdat.play_hp -= (byte)Rng.r.Next(6);
            }
            else
            {
                if (playerdat.play_hp> 0x32)
                {
                    playerdat.play_hp -= (byte)Rng.r.Next(4);
                }
                else
                {
                    if (
                        (playerdat.play_hp > 0x14)
                        &&
                        (Rng.r.Next(3)<1)
                        )
                    {
                        playerdat.play_hp -= (byte)Rng.r.Next(3);
                    }
                    else
                    {
                        if (playerdat.play_hp > 1)
                        {
                            if ((Rng.r.Next(0x7FFF) & 7) == 0)
                            {
                                playerdat.play_hp -= 1;
                            }
                        }
                    }
                }
            }

            //shake screen
            special_effects.Screenshake(0x40, 0xF + Rng.r.Next(0x1E));

            //randomise compass
            uimanager.PointCompassInDirection(Rng.r.Next(0x7FFF) & 0xF);
        }
    }//end class
}//end namespace
