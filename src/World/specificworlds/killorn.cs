using System.Diagnostics;
namespace Underworld
{
    /// <summary>
    /// Specific code for the killorn
    /// </summary>
    public class killorn:UWClass
    {

        public static void KilornIsCrashing(bool isEnteringLevel)
        {
            special_effects.SpecialEffect(effecttype: 4, effectparam: 0);

            playerdat.SetQuest(54,1);
            playerdat.IncrementXClock(15);
            if (isEnteringLevel)
            {
                scd.ProcessSCDArk(1);
            }
        }
    }
}