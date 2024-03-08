using System.Diagnostics;

namespace Underworld
{
    public partial class SpellCasting : UWClass
    {

        /// <summary>
        /// Misc active spells and others
        /// </summary>
        /// <param name="minorclass"></param>
        /// <param name="stability"></param>
        public static void CastClassB_Spells(int minorclass, int stability)
        {
            switch (minorclass)
            {
                case 0: //speed
                    {
                        PlayerActiveStatusEffectSpells(0xB, 2, stability);//Note the change in mapping here.
                        playerdat.PlayerStatusUpdate();
                        break;
                    }
                case 1://
                    {
                        if (_RES == GAME_UW2)
                        {
                            Debug.Print("PORTAL");
                        }
                        else
                        {
                            Debug.Print("DETECT MONSTER");
                        }
                        break;
                    }
                case 2:
                    {
                        if (_RES == GAME_UW2)
                        {
                            Debug.Print("RESTORATION");
                        }
                        else
                        {
                            Debug.Print("REMOVE TRAP");
                        }
                        break;
                    }

                case 3:
                    {
                        if (_RES == GAME_UW2)
                        {
                            Debug.Print("LOCATE");
                        }
                        else
                        {
                            Debug.Print("REMOVE TRAP");//again
                        }
                        break;
                    }

                case 4:
                case 5:
                    {
                        if (_RES != GAME_UW2)
                        {//no uw2 spells here.
                            Debug.Print("REMOVE TRAP"); //once more
                        }
                        break;
                    }
                case 6:
                    {//Cure poison
                        playerdat.play_poison = 0;
                        break;
                    }
                case 7:
                    {
                        //roaming sight
                        //Needs extra logic for UW2 (eg check if in void)
                        PlayerActiveStatusEffectSpells(0xB, 1, stability);//Note the change in mapping here.
                        playerdat.PlayerStatusUpdate();
                        break;
                    }
                case 8://telekenesis
                    {
                        PlayerActiveStatusEffectSpells(0xB, 3, stability);//Note the change in mapping here.
                        playerdat.PlayerStatusUpdate();
                        break;
                    }
                case 9://tremor
                    {
                        Debug.Print("!!!!EARTHQUAKKKKEEE!!!!");
                        break;
                    }
                case 0xA://gate travel
                    {
                        Debug.Print("GATE TRAVEL");
                        break;
                    }
                case 0xB://Freeze time
                    {
                        PlayerActiveStatusEffectSpells(0xB, 0, stability);//Note the change in mapping here.
                        playerdat.PlayerStatusUpdate();
                        break;
                    }
                case 0xC:   //armageddon
                    {
                        Debug.Print("ARMAGEDDON--> Everything vanishes");
                        break;
                    }
            }
        }

        /// <summary>
        /// Note these enchantments don't line up exactly with the class B spell cast list. This is the correct behaviour
        /// </summary>
        /// <param name="minorclass"></param>
        public static void CastClassBEnchantment(int minorclass)
        {
            switch (minorclass)
            {
                case 0://freezetime
                    playerdat.FreezeTimeEnchantment = true; break;
                case 1://roaming sight
                    playerdat.RoamingSightEnchantment = true; break;
                case 2://speed
                    playerdat.SpeedEnchantment = true; break;
                case 3://telekenesis
                    playerdat.TelekenesisEnchantment = true; break;
                case 0xE://health regen
                    playerdat.HealthRegenEnchantment = true; break;
                case 0xF://Mana regen       
                    playerdat.ManaRegenEnchantment = true; break;
            }
        }

    }//end class
}//end namespace