using System.Diagnostics;

namespace Underworld
{
    public  partial class SpellCasting : UWClass
    {

        /// <summary>
        /// Casts spells from enchanted equipment. Not all spells will cast from here.
        /// </summary>
        /// <param name="majorclass"></param>
        /// <param name="minorclass"></param>
        /// <param name="TriggeredByInventoryEvent">First time trigger from adding to inventory, used for curse objects initial message in UW2</param>
        public static void CastEnchantedItemSpell(int majorclass, int minorclass, bool TriggeredByInventoryEvent, ref int DamageResistance, ref int StealthBonus, int PaperDollSlot)
        {
            switch (majorclass)
            {
                case 0://lighting spells
                    {
                        if (playerdat.lightlevel < minorclass)
                        {
                            playerdat.lightlevel = minorclass;
                        }
                        if (minorclass == 5)
                        {
                            //apply mono palette for nightvision.
                            Palette.ColourTone = 1;
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
                case 3://bonuses
                    {
                        CastClass3Enchantment(minorclass, ref StealthBonus);
                        break;
                    }
                case 9://curses
                    {
                        CastClass9_Curse(minorclass, TriggeredByInventoryEvent);
                        break;
                    }
                case 0xB:
                    {
                        CastClassBEnchantment(minorclass); break;
                    }
                case 0xC: //toughness or protection.
                    {   //toughness applies a damage resistance to a specific body part. 
                        //Protection makes the body part harder to hit
                        if (PaperDollSlot < 2) //appears to only work when helm and chest?
                        {
                            var slot = 0;
                            if (PaperDollSlot == 0) { slot = 3; }
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
                    Debug.Print($"Unhandled enchantment {majorclass} {minorclass}");
                    break;
            }
        }

        /// <summary>
        /// Casts appropiate spells from objects
        /// </summary>
        /// <param name="spellno"></param>
        /// <param name="obj"></param>
        public static void CastSpellFromObject(int spellno, uwObject obj)
        {
            if (((spellno & 0xC0) == 0) && (spellno <= RunicMagic.SpellList.GetUpperBound(0)))
            {
                //do a runic table lookup
                var spell = RunicMagic.SpellList[spellno];
                RunicMagic.CastRunicSpellWithoutRules(spell);
            }
            else
            {
                var major = 12 + ((spellno & 0xC0) >> 6);
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