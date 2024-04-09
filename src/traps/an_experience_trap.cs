namespace Underworld
{
    public class an_experience_trap : trap
    {
        public static void Activate(uwObject trapObj)
        {
            var newEXP =  trapObj.quality<<3;
            newEXP += (trapObj.owner & 0x7);
            newEXP -= 0x100;
            var multiplier = (trapObj.owner>>3);
            multiplier = 1<< multiplier;
            newEXP = newEXP * multiplier;
            playerdat.ChangeExperience(newEXP);
        }
    }//end class
}//end namespace