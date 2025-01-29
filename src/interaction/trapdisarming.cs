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
                    var found = objectsearch.FindMatchInObjectChain(next, 6, -1, -1, objList, SkipLinks: true);

                    if (found != null)
                    {
                        if (_RES == GAME_UW2)
                        {
                            if (found.minorclass == 3)
                            {//found a (movement/action) based trigger
                                //do skill check and return;
                                return DoTrapSkillCheck(searchSkill);
                            }
                            else
                            {
                                //if not major 6, minor 3 look again for another object
                                next = found.next;
                            }
                        }
                        else
                        {//UW1 logic
                            if (found.minorclass>=2)
                            {//found a trigger, getting it's trap
                                found = objList[found.link];
                            }
                            if (found.minorclass<2)
                            {//has found a trap
                                return DoTrapSkillCheck(searchSkill);
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
    }//end class
}//end namespace