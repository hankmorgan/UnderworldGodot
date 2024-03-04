namespace Underworld
{

    public  partial class SpellCasting : UWClass
    {
        /// <summary>
        /// Healing spells
        /// </summary>
        /// <param name="minorclass"></param>
        public static void CastClass4_Heal(int minorclass)
        {
            int healamount = 0;
            if (minorclass == 0xF)
            {//what spell would do this???
                healamount = -1;
                playerdat.play_hp = System.Math.Max(0, playerdat.max_hp + healamount);//negative ???

            }
            else
            {
                healamount = Rng.DiceRoll(minorclass, 8);
                playerdat.play_hp = System.Math.Min(playerdat.max_hp, playerdat.max_hp + healamount);
            }
        }
    }//end namespace
}//end class
