using System;
using System.Diagnostics;

namespace Underworld
{
    public partial class SpellCasting : UWClass
    {
        /// <summary>
        /// Special extra spells
        /// </summary>
        /// <param name="minorclass"></param>
        public static void CastClassD_Spells(int minorclass)
        {
            switch (minorclass)
            {
                case 0://altaras wand
                    {
                        Debug.Print("Altaras wand");
                        AltarasWand();
                        break;
                    }
                case 2://mind blast
                    {
                        Debug.Print("Mind blast");
                        break;
                    }
                case 3://basilisk oil and bullfrog
                    {
                        if (_RES==GAME_UW2)
                        {
                            Hallucination();
                        }
                        else
                        {
                            //bullfrog reset
                            a_do_trap_bullfrog.Bullfrog(mode: 4, triggerX: 0, triggerY: 0);
                        }                        
                        break;
                    }
                case 4:
                    {
                        Hallucination();
                        break;
                    }
                case 5:
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_your_vision_distorts_and_you_feel_light_headed_));
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
                            uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_the_spell_has_no_discernable_effect_));
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
                        var mc = ((minorclass - 11) << 2) - 1;
                        CastClass10_ManaBoost(mc);
                        break;
                    }
            }
        }

        private static void Hallucination()
        {
            if (playerdat.shrooms == 0)
            {
                var SkillCheckResult = playerdat.SkillCheck(playerdat.INT, 0x14);
                if (SkillCheckResult <= 0)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_your_vision_distorts_and_you_feel_light_headed_));
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
        }



        /// <summary>
        /// Checks objects in an area to see if a valid line of power can be cut here.
        /// </summary>
        static void AltarasWand()
        {
            var worldflag = 1 << ((worlds.GetWorldNo(playerdat.dungeon_level)) - 1);
            var range = 2;
            var cX = playerdat.tileX; var cY = playerdat.tileY;
            var linesOfPower = playerdat.GetQuest(128);

            //try and find a guardian signet ring in the area
            for (int aX = cX - range; aX <= cX + range; aX++)
            {
                for (int aY = cX - range; aY <= cY + range; aY++)
                {
                    if (UWTileMap.ValidTile(aY, aX))
                    {
                        var tile = UWTileMap.current_tilemap.Tiles[aX, aY];
                        var next = tile.indexObjectList;
                        while (next != 0)
                        {
                            var nextObj = UWTileMap.current_tilemap.LevelObjects[next];
                            if (nextObj.item_id == 53) //signet ring
                            {
                                //test this signet ring 
                                if (nextObj.invis == 1 && nextObj.doordir == 1)
                                {//line of power marked by an invisible signet ring with doordir=1
                                    nextObj.doordir = 0;
                                    if ((linesOfPower & worldflag) == 0)
                                    {//line can be cut

                                        //update bit
                                        linesOfPower |= worldflag;
                                        playerdat.SetQuest(128, linesOfPower);
                                        linesOfPower = playerdat.GetQuest(128);
                                        
                                        if (linesOfPower == 0xFF)
                                        {//all lines cut.
                                            playerdat.SetQuest(14, 1);
                                        }
                                        animo.SpawnAnimoAtPoint(7, nextObj.GetCoordinate(nextObj.tileX, nextObj.tileY));
                                        ObjectCreator.DeleteObjectFromTile(nextObj.tileX, nextObj.tileY, nextObj.index);

                                        if (worlds.GetWorldNo(playerdat.dungeon_level) == 3)
                                        {//in ice caverns
                                            playerdat.SetQuest(52,1);
                                        }
                                        special_effects.SpecialEffect(4, 15);//screenshake
                                        return;
                                    }
                                }
                            }
                            next = nextObj.next;
                        }
                    }
                }
            }
            uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_the_spell_has_no_discernable_effect_));
        }
    }//end class
}//end namespace