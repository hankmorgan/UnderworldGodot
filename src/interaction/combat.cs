namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the look verb
    /// </summary>
    public class combat : UWClass
    {
        public static int stage = 0;  //0=weapdrawn, 1 = charge bulding, 2=release attack, 3=do result, 4= wait for next strike window?
        public static double combattimer = 0.0;
        public static void CombatLoop(double delta)
        {
            stage = 1;
            combattimer += delta;
            if (combattimer>0.2) //this timer should be based on the weapon speed rating
            {
                uimanager.IncreasePower();
                combattimer = 0f;
            }            
        }

        public static void EndCombatLoop()
        {
            stage = 0;
            uimanager.ResetPower();
            combattimer = 0;
        }
    }//end class
}//end namespace