using System.Diagnostics;

namespace Underworld
{

    public class SpellCasting : UWClass
    {

        /// <summary>
        /// Logic for casting spells that are not from enchanted objects. Assumes all spell casting requirements have been met. 
        /// </summary>
        /// <param name="majorclass"></param>
        /// <param name="minorclass"></param>
        /// <param name="caster">Casting object (use if not the player casting)</param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static void CastSpell(int majorclass, int minorclass, uwObject caster, uwObject target, int tileX, int tileY, bool CastOnEquip, bool PlayerCast = true)
        {
            Debug.Print($"Casting {majorclass},{minorclass}");
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
                    CastClass10ManaBoost(minorclass);
                    break;
                case 11://misc or special spells
                    break;
                case 12://not castable here
                    Debug.Print("Attempt to directly cast Class 0xC spell. This should not happen");
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

        /// <summary>
        /// Casts spells from enchanted equipment. Not all spells will cast from here.
        /// </summary>
        /// <param name="majorclass"></param>
        /// <param name="minorclass"></param>
        /// <param name="TriggeredByInventoryEvent">First time trigger from adding to inventory, used for curse objects initial message in UW2</param>
        public static void CastEnchantedItemSpell(int majorclass, int minorclass, bool TriggeredByInventoryEvent, ref int DamageResistance, int PaperDollSlot)
        {
            switch (majorclass)
            {
                case 0://lighting spells
                    {
                        if (playerdat.lightlevel < minorclass)
                        {
                            playerdat.lightlevel = minorclass;
                        }
                        break;
                    }
                case 1: //Motion abilities
                    {//set relevant bit
                        playerdat.MagicalMotionAbilities = (byte)(playerdat.MagicalMotionAbilities | (1 << (minorclass - 1)));
                        break;
                    }
                case 2: // resistances
                    {//these apply a blanket damage resistance to each body part.
                        if (minorclass > DamageResistance)
                        {
                            DamageResistance = minorclass;
                        }
                        break;
                    }
                case 9://curses
                    {
                        CastClass9_Curse(minorclass, TriggeredByInventoryEvent);
                        break;
                    }
                case 0xC: //toughness or protection.
                    {   //toughness applies a damage resistance to a specific body part. 
                        //Protection makes the body part harder to hit
                        if (PaperDollSlot < 2) //appears to only work when helm and chest?
                        {
                            var slot = 0; 
                            if (PaperDollSlot == 0){slot=3;}
                            if ((minorclass & 0x8) == 0)
                            {
                                //protection
                                playerdat.LocationalProtectionValues[slot] += (minorclass & 0x7);
                            }
                            else
                            {
                                //toughness
                                playerdat.LocationalProtectionValues[slot] += (minorclass & 0x7);
                            }
                        }
                        break;
                    }
                default:
                    Debug.Print ($"Unhandled enchantment {majorclass} {minorclass}");
                    break;
            }
        }

        public static void CastClass10ManaBoost(int minorclass)
        {
            //check mana rules for the academy test.
            if (_RES==GAME_UW2)
            {
                if (worlds.GetWorldNo(playerdat.dungeon_level) == 5)//Academy
                {
                    var academylevel = 1 + playerdat.dungeon_level%8;
                    if ((academylevel>1) && (academylevel<8))
                        {
                            return;
                        }
                    if (academylevel==8)
                    {
                        if (playerdat.tileX<25)
                        {
                            return;
                        }
                    }
                }
            }
            //Apply mana boost
            if (minorclass<0)
                {//boost mana by minus minus minor class. Not clear when this could happen...
                    playerdat.play_mana = System.Math.Min(playerdat.play_mana + minorclass, playerdat.max_mana);
                }
            else
                {
                    var increase =  1  + ((playerdat.max_mana * (minorclass + Rng.r.Next(0,4)))>>4);
                    playerdat.play_mana = System.Math.Min(playerdat.play_mana + increase, playerdat.max_mana);
                }
        }

        /// <summary>
        /// Applies curse object damage
        /// Assumes it can only affect the player character.
        /// </summary>
        /// <param name="minorclass"></param>
        /// <param name="CastOnEquip">Has the cursed item just been equiped</param>
        public static void CastClass9_Curse(int minorclass, bool CastOnEquip)
        {
            var dmg = Rng.DiceRoll(minorclass, 8);
            if (playerdat.play_hp - dmg >= 3)
            {
                playerdat.play_hp -= dmg;
            }
            else
            {
                playerdat.play_hp = 3;
            }
            if (CastOnEquip)
            {
                if (_RES == GAME_UW2)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 362));
                }
            }
            else
            {
                if (_RES == GAME_UW2)
                {
                    uimanager.FlashColour(0x30, uimanager.Cuts3DWin, 0.2f);
                }
                else
                {
                    uimanager.FlashColour(0xA8, uimanager.Cuts3DWin, 0.2f);
                }
            }
        }


        public static void CastSpellFromObject(int spellno, uwObject obj)
        {
            if ((spellno & 0xC0) == 0)
            {
                //do a runic table lookup
                var spell = RunicMagic.SpellList[spellno];
                RunicMagic.CastRunicSpellWithoutRules(spell);
            }
            else
            {
                var major = 12 + ((spellno & 0xC0)>>6);
                var minor = spellno & 0x3F;
                CastSpell(
                    majorclass: major, 
                    minorclass: minor, 
                    caster: obj, 
                    target: null, 
                    tileX: playerdat.tileX,
                    tileY: playerdat.tileY, 
                    CastOnEquip: false, 
                    PlayerCast: true);
            }
        }

    }//end class
}//end namespace
