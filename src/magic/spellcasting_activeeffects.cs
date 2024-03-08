namespace Underworld
{
    public  partial class SpellCasting : UWClass
    {
/// <summary>
        /// Applies player active status effects
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="stabilityclass"></param>
        static void PlayerActiveStatusEffectSpells(int major, int minor, int stabilityclass)
        {//updates the player status active effects. The actual impact of the spell is processed by PlayerRefreshUpdates()
            if (playerdat.ActiveSpellEffectCount < 3)
            {
                var stability = 0;
                //calc stability
                switch (stabilityclass)
                {
                    case 0x80:
                        stability = Rng.DiceRoll(3, 24); break;
                    case 0x40:
                        stability = Rng.DiceRoll(2, 8); break;
                    case 1:
                        stability = 1; break;
                    case 0:
                        stability = Rng.DiceRoll(2, 3); break;
                    default:
                        stability = 0; break;
                }

                //apply effect to player data
                playerdat.SetSpellEffect(
                    index: playerdat.ActiveSpellEffectCount,
                    effectid: (minor << 4) + major,
                    stability: stability);
                playerdat.ActiveSpellEffectCount++;
            }
            else
            {
                return; //no more effects allowed
            }
        }
    }//end class
}//end namespace