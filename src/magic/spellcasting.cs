using System.Diagnostics;

namespace Underworld
{

    public class SpellCasting : UWClass
    {

        /// <summary>
        /// Logic for casting spells. Assumes all spell casting requirements have been met.
        /// </summary>
        /// <param name="majorclass"></param>
        /// <param name="minorclass"></param>
        /// <param name="caster">Casting object (use if not the player casting)</param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static void CastSpell(int majorclass, int minorclass, uwObject caster, uwObject target, int tileX, int tileY, bool PlayerCast = true)
        {            
            Debug.Print ($"Casting {majorclass},{minorclass}");
            switch (majorclass)
            {                
                //0-3 affect active player statuses
                //In this case use minorclass & 0xC0 as the spell stability class and minorclass & 0x3F as the minor class within the function.
                case 0://
                case 1://motion related
                case 2://
                case 3://
                    //TODO add special handling for ironflesh etc
                    PlayerStatusEffectSpells(
                        major: majorclass, 
                        minor: minorclass & 0x3F, 
                        stabilityclass: minorclass & 0xC0);
                    break;
                case 4://healing
                    break;
                case 5://projectile spells
                    break;
                case 6://spells that run code in an area around the player
                    break;
                case 7://spells with prompts and code callbacks.
                    break;
                case 8://spells that create or summon
                    break;
                case 9://curses
                    break;
                case 10://mana change spells
                    break;
                case 11://misc or special spells
                    break;
                case 12://not castable here
                    break;
                case 13://misc spells
                    break;
                case 14://cutscene spells.
                    break;
            }
        }


        /// <summary>
        /// Player status spells
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        static void PlayerStatusEffectSpells(int major, int minor, int stabilityclass)
        {//updates the player status active effects. The actual impact of the spell is processed by PlayerRefreshUpdates()

        }
    }//end class
}//end namespace
