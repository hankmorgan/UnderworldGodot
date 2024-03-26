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
                        Debug.Print("Basillisk oil. do an INT skill check to avoid hallucination");
                        break;
                    }
                case 5:
                    {
                        Debug.Print("Basillisk oil. no skill check to avoid hallucination");
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