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
        public static void CastSpell(int majorclass, int minorclass, uwObject caster, uwObject target, int tileX, int tileY, bool CastOnEquip, bool PlayerCast = true)
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
                    {
                        //TODO add special handling for ironflesh (plot handling for xclock3) and leviation/fly spells (stop falling)
                        if ((majorclass == 2) && (((minorclass & 0x3F) == 3) || ((minorclass & 0x3F) == 5)))
                            {
                                Debug.Print("Leviate/Fly cast. Stop jumping"); //what happens here if all active effects are running???
                            }
                        if ( (_RES==GAME_UW2) && (majorclass == 2) && ((minorclass & 0x3F) == 5) )
                        {
                            //iron flesh.
                            //if xclock == 4 
                            // glaze over
                            //set xclock = 5
                            uimanager.AddToMessageScroll(GameStrings.GetString(1,335));
                        }

                        //Apply the active effect if possible.
                        PlayerActiveStatusEffectSpells(
                            major: majorclass, 
                            minor: minorclass & 0x3F, 
                            stabilityclass: minorclass & 0xC0);
                        
                        playerdat.PlayerStatusUpdate();                        
                        break;
                    }
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
                    CastClass9_Curse(
                        minorclass: minorclass, 
                        CastOnEquip: CastOnEquip);                    
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
                        stability = Rng.DiceRoll(3,24); break;
                    case 0x40:
                        stability = Rng.DiceRoll(2,8); break;
                    case 1:
                        stability = 1;break;
                    case 0:
                        stability = Rng.DiceRoll(2,3); break;
                    default:
                        stability = 0;break;
                }

                //apply effect to player data
                playerdat.SetSpellEffect(
                    index: playerdat.ActiveSpellEffectCount, 
                    effectid: (minor<<4)  + major, 
                    stability: stability);
                playerdat.ActiveSpellEffectCount++;   
            }
            else
            {
                return; //no more effects allowed
            }
        }

        /// <summary>
        /// Makes the necessary changes to set the effects of status effect spells
        /// </summary>
        /// <param name="majorclass"></param>
        /// <param name="minorclass"></param>
        public static void ApplyStatusEffectSpell(int majorclass, int minorclass, bool TriggeredByInventoryEvent)
        {
            switch(majorclass)
            {
                case 0://lighting spells
                    {
                        if (playerdat.lightlevel<minorclass)
                        {
                            playerdat.lightlevel = minorclass;
                        }
                        break;
                    }

                case 9://curses
                    {
                        CastClass9_Curse(minorclass, TriggeredByInventoryEvent);
                        break;
                    }
            }
        }


        /// <summary>
        /// Applies curse object damage
        /// Assumes it can only affect the player character.
        /// </summary>
        /// <param name="minorclass"></param>
        /// <param name="CastOnEquip">Has the cursed item just been equiped</param>
        public static void CastClass9_Curse(int minorclass,bool CastOnEquip)
        {
            var dmg = Rng.DiceRoll(minorclass, 8);
            if (playerdat.play_hp-dmg>=3)
                {
                     playerdat.play_hp-= dmg;
                }
            else
                {
                    playerdat.play_hp = 3;                   
                }
            if (CastOnEquip)
            {
                if (_RES==GAME_UW2)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 362));
                }                
            }
            else
            {
                if (_RES==GAME_UW2)
                {
                    uimanager.FlashColour(0x30, uimanager.Cuts3DWin,0.2f);
                }
                else
                {
                    uimanager.FlashColour(0xA8, uimanager.Cuts3DWin,0.2f);
                }   
            }
        } 

    }//end class
}//end namespace
