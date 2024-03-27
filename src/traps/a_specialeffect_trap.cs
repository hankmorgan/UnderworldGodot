namespace Underworld
{
    public class a_specialeffect_trap : trap
    {
        public static void Activate(uwObject trapObj)
        {
            special_effects.SpecialEffect(trapObj.quality, trapObj.owner);
        }
    }//end class
}//end namespace