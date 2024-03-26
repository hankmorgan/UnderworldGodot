using System;
using System.Diagnostics;

namespace Underworld
{
    public partial class SpellCasting : UWClass
    {
        /// <summary>
        /// Special extra spells only found in UW2
        /// </summary>
        /// <param name="minorclass"></param>
        public static void CastClassD_Spells(int minorclass)
        {
            switch (minorclass)
            {
                case 0://altaras wand
                    {
                        Debug.Print("Altaras wand");
                        break;
                    }
                case 2://mind blast
                    {
                        Debug.Print("Mind blast");
                        break;
                    }
                case 3://basilisk oil
                case 4:
                    {
                        if (playerdat.shrooms == 0)
                        {
                            var SkillCheckResult = playerdat.SkillCheck(playerdat.INT, 0x14);
                            if (SkillCheckResult<=0)
                            {
                                uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_your_vision_distorts_and_you_feel_light_headed_));
                                playerdat.shrooms = 2;
                            }
                            else
                            {
                                uimanager.FlashColour(95, uimanager.CutsSmall);
                            }
                        }
                        else
                        {
                            uimanager.FlashColour(95, uimanager.CutsSmall);
                        }
                        break;
                    }
                case 5:
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_your_vision_distorts_and_you_feel_light_headed_));
                        playerdat.shrooms = 3;
                        break;
                    }
                case 7:
                    {//Map area
                        if (automap.CanMap(playerdat.dungeon_level))
                        {
                            var castscore = Math.Max(playerdat.Casting - 13, 0);
                            var range = 2 + (castscore / 5);

                            automap.MarkRangeOfTilesVisited(
                                range: range,
                                cX: playerdat.tileX,
                                cY: playerdat.tileY,
                                dungeon_level: playerdat.dungeon_level
                                );
                        }
                        else
                        {
                            uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_the_spell_has_no_discernable_effect_));
                        }
                        break;
                    }
                case 8:
                    {//acid spit
                        CastSpell(
                            majorclass: 5,
                            minorclass: 4,
                            caster: null,
                            target: null,
                            tileX: playerdat.tileX,
                            tileY: playerdat.tileY,
                            CastOnEquip: false);
                        break;
                    }
                case 9:
                    {//snowball
                        CastSpell(
                            majorclass: 5,
                            minorclass: 6,
                            caster: null,
                            target: null,
                            tileX: playerdat.tileX,
                            tileY: playerdat.tileY,
                            CastOnEquip: false);
                        break;
                    }
                case 12:
                case 13:
                case 14:
                case 15: //mana boost
                    {
                        var mc = ((minorclass-11)<<2)-1;
                        CastClass10_ManaBoost(mc);
                        break;
                    }
            }
        }
    }//end class
}//end namespace