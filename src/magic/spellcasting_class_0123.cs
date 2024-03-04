using System.Diagnostics;
namespace Underworld
{
    public partial class SpellCasting : UWClass
    {
        private static void CastClass0123_Spells(int majorclass, int minorclass)
        {
            //TODO add special handling for ironflesh (plot handling for xclock3) and leviation/fly spells (stop falling)
            if ((majorclass == 1) && (((minorclass & 0x3F) == 3) || ((minorclass & 0x3F) == 5)))
            {
                Debug.Print("Leviate/Fly cast. Stop jumping"); //what happens here if all active effects are running???
            }
            if ((_RES == GAME_UW2) && (majorclass == 2) && ((minorclass & 0x3F) == 5))
            {
                //iron flesh.
                //if xclock == 4 
                // glaze over
                //set xclock = 5
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 335));
            }

            //Apply the active effect if possible.
            PlayerActiveStatusEffectSpells(
                major: majorclass,
                minor: minorclass & 0x3F,
                stabilityclass: minorclass & 0xC0);

            playerdat.PlayerStatusUpdate();
        }

        /// <summary>
        /// General bonuses
        /// </summary>
        /// <param name="minorclass"></param>
        /// <param name="StealthBonus"></param>
        public static void CastClass3Enchantment(int minorclass, ref int StealthBonus)
        {
            switch (minorclass)
            {
                case 1: //LUCK
                    {//A +3 bonus to location protection
                        playerdat.LocationalProtectionValues[0]+=3;
                        playerdat.LocationalProtectionValues[1]+=3;
                        playerdat.LocationalProtectionValues[2]+=3;
                        playerdat.LocationalProtectionValues[3]+=3;
                        break;
                    }
                case 2:
                case 3:
                case 4://stealth
                    {   //set the bit field of stealth bonuses. These are used to adjust the player stealth score which does something..
                        StealthBonus |= (1 << (minorclass-1)); //OR in this bit
                        break;
                    }
                case 5:
                case 6:
                case 7:
                case 8:
                case 9://Damage proof protections, eg missile, flameproof
                    {
                        var bits = new byte[]{0x40, 8, 0x10, 1, 2};//what bits to set
                        var bit = bits[minorclass-5];
                        playerdat.PlayerDamageTypeScale |= bit;//set the relevant x-proof
                        break;
                    }
                case 0x10:
                    {
                        //Valour spell. This spell is unique to UW2.
                        if (_RES==GAME_UW2)
                        {
                            playerdat.ValourBonus = 10 + playerdat.Casting/5;
                        }                        
                        break;
                    }
                case 0xB:
                    {
                        //Poison weapon, unique to UW2
                        if (_RES==GAME_UW2)
                        {
                            playerdat.PoisonedWeapon = true;
                        }
                        break;
                    }
            }
        }


    }// end class
}//end namespace