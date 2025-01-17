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
            Debug.Print("Killorn is crashing. move me to somewhere better once I start implementing Kilorn");
            special_effects.SpecialEffect(4,0);
            playerdat.SetQuest(54,1);
            playerdat.IncrementXClock(15);
            if (isEnteringLevel)
            {
                scd.ProcessSCDArk(1);
            }
        }
    }
}