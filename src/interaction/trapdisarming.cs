using System.Diagnostics;

namespace Underworld
{
    public class trapdisarming : UWClass
    {

        /// <summary>
        /// Checks for a linked damage trap-trigger
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        /// <param name="searchSkill"></param>
        /// <returns>Skill check result if a trigger linked to a trap is found</returns>
        public static playerdat.SkillCheckResult DetectTrapTrigger(int index, uwObject[] objList, int searchSkill)
        {
            var objToCheck = objList[index];

            if (objToCheck.is_quant == 0 && objToCheck.link != 0)
            {
                var next = objToCheck.link;
                while (next != 0)
                {
                    var foundtrigger = objectsearch.FindMatchInObjectChain(next, 6, -1, -1, objList, SkipLinks: true);

                    if (foundtrigger != null)
                    {
                        if (_RES == GAME_UW2)
                        {
                            if (foundtrigger.minorclass == 3)
                            {//found a (movement/action) based trigger
                                //do skill check and return;
                                return DoTrapSkillCheck(searchSkill);
                            }
                            else
                            {
                                //if not major 6, minor 3 look again for another object
                                next = foundtrigger.next;
                            }
                        }
                        else
                        {//UW1 logic
                            if (foundtrigger.link!=0)
                            {
                                if (foundtrigger.minorclass >= 2)
                                {//found a trigger, getting it's trap
                                    foundtrigger = objList[foundtrigger.link];
                                }
                                if (foundtrigger.minorclass < 2)
                                {//has found a trap
                                    return DoTrapSkillCheck(searchSkill);
                                }
                                else
                                {
                                    return playerdat.SkillCheckResult.Fail;
                                }
                            }
                            else
                            {
                                next = foundtrigger.next;
                            }
                        }
                    }
                    else
                    {
                        return playerdat.SkillCheckResult.Fail;
                    }
                }
            }
            return playerdat.SkillCheckResult.Fail;
        }


        /// <summary>
        /// Does the skill check for searching against a trap. Based on world no in UW2, and static value of 8 in UW1.
        /// </summary>
        /// <param name="searchskill"></param>
        /// <returns></returns>
        static playerdat.SkillCheckResult DoTrapSkillCheck(int searchskill)
        {
            if (_RES == GAME_UW2)
            {
                var difficulty = 0xA + (worlds.GetWorldNo(playerdat.dungeon_level) << 1);
                return playerdat.SkillCheck(searchskill, difficulty, true);
            }
            else
            {
                return playerdat.SkillCheck(searchskill, 8, true);
            }
        }

        /// <summary>
        /// Finds and Disarms a trap
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        /// <param name="trapsSkill"></param>
        public static playerdat.SkillCheckResult DisarmTrap(int index, uwObject[] objList, int trapsSkill)
        {
            var objToCheck = objList[index];

            if (objToCheck.is_quant == 0 && objToCheck.link != 0)
            {
                var next = objToCheck.link;
                while (next != 0)
                {
                    var foundtrigger = objectsearch.FindMatchInObjectChain(next, 6, -1, -1, objList, SkipLinks: true);

                    if (foundtrigger != null)
                    {
                        if (_RES == GAME_UW2)
                        {
                            if (foundtrigger.minorclass == 3)
                            {//found a (movement/action) based trigger
                                if (foundtrigger.link!=0)
                                {
                                    var foundtrap = objList[foundtrigger.link];
                                    return DoTrapDisarm(objList: objList, trapsSkill: trapsSkill, objToCheck: objToCheck, foundtrigger: foundtrigger, foundtrap: foundtrap);
                                }
                                //if not major 6, minor 3 look again for another object
                                next = foundtrigger.next;
                            }
                        }
                        else
                        {//UW1 logic   
                            var foundtrap = foundtrigger;                         
                            if (foundtrigger.minorclass >= 2)
                            {//found a trigger, getting it's trap
                                foundtrap = objList[foundtrigger.link];
                            }
                            else
                            {
                                foundtrap = foundtrigger;
                                foundtrigger = null;
                            }
                            if ((foundtrap.item_id & 0x3F) <= 2)
                            {//has found a trap (damage, arrow or teleport)
                                return DoTrapDisarm(objList: objList, trapsSkill: trapsSkill, objToCheck: objToCheck, foundtrigger: foundtrigger, foundtrap: foundtrap);
                            }
                            else
                            {
                                return playerdat.SkillCheckResult.Fail;
                            }
                        }
                    }
                    else
                    {
                        return playerdat.SkillCheckResult.Fail;
                    }
                }
            }
            return playerdat.SkillCheckResult.Fail;
        }


        /// <summary>
        /// Does the skill check and actions of disarming a trap.
        /// </summary>
        /// <param name="objList"></param>
        /// <param name="trapsSkill"></param>
        /// <param name="objToCheck"></param>
        /// <param name="foundtrigger"></param>
        /// <param name="foundtrap"></param>
        /// <returns></returns>
        private static playerdat.SkillCheckResult DoTrapDisarm(uwObject[] objList, int trapsSkill, uwObject objToCheck, uwObject foundtrigger, uwObject foundtrap)
        {
            string traptype = GetTrapType(foundtrap);
            var skillcheckresult = DoTrapSkillCheck(trapsSkill);
            switch (skillcheckresult)
            {
                case playerdat.SkillCheckResult.Success:
                case playerdat.SkillCheckResult.CritSucess:
                    {
                        //The trap on the object was disarmed
                        uimanager.AddToMessageScroll($"The {traptype} on the {GameStrings.GetObjectNounUW(objToCheck.item_id)}{GameStrings.GetString(1, 0x168)}");
                        if (foundtrigger != null)
                        {
                            foundtrigger.link = 0;
                            // if (UWTileMap.ValidTile(foundtrigger.quality, foundtrigger.owner))
                            // {
                            //     var tile = UWTileMap.current_tilemap.Tiles[foundtrigger.quality, foundtrigger.owner];
                            //     ObjectRemover.RemoveTrapTriggerChain(foundtrap, tile.Ptr + 2);
                            // }
                        }
                        break;
                    }
                case playerdat.SkillCheckResult.Fail:
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x167));//you have failed to disarm
                    break;
                case playerdat.SkillCheckResult.CritFail:
                    uimanager.AddToMessageScroll($"{GameStrings.GetString(1, 0x166)} {traptype}");//your bumbling efforts
                    if (foundtrigger == null)
                    {
                        trap.ActivateTrap(character: 1, trapObj: foundtrap, ObjectUsed: objToCheck, triggerX: playerdat.playerObject.tileX, triggerY: playerdat.playerObject.tileY, objList: objList);
                    }
                    else
                    {
                        trigger.RunTrigger(character: 1, ObjectUsed: objToCheck, TriggerObject: foundtrigger, triggerType: -1, objList: objList);
                    }
                    break;
            }
            return skillcheckresult;
        }


        /// <summary>
        /// Gets the name for the trap
        /// </summary>
        /// <param name="foundtrap"></param>
        /// <returns></returns>
        private static string GetTrapType(uwObject foundtrap)
        {
            int trapclass = foundtrap.item_id & 0x3F;
            string traptype;
            if (trapclass == 0xF)
            {
                //null trap
                traptype = "trap";
            }
            else
            {
                if (trapclass == 0 && foundtrap.owner != 0)
                {
                    traptype = "poison trap";
                }
                else
                {
                    traptype = GameStrings.GetObjectNounUW(foundtrap.item_id);
                }
            }

            return traptype;
        }


        /// <summary>
        /// The trap disarming spell
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        public static void TrapDisarmSpell(int index, uwObject[] objList)
        {
            switch (trapdisarming.DetectTrapTrigger(index, objList, 45))
            {
                case playerdat.SkillCheckResult.Success:
                case playerdat.SkillCheckResult.CritSucess:
                    trapdisarming.DisarmTrap(index, objList, 0x2D); break;//you have detected
                default:
                    Debug.Print("There is no trap detected on that object!");
                    break;
            }
        }


    }//end class
}//end namespace