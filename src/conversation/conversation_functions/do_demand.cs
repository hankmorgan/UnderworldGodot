using System.Collections;
namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static IEnumerator do_demand(uwObject talker)
        {
            //2 versions needed. uw1 does not check for selected items, uw2 does and has extra strings

            switch (_RES)
            {
                case GAME_UW2:
                    yield return do_demandUW2(talker);
                    break;
                default:
                    yield return do_demandUW1(talker);
                    break;
            }

            yield return 0;
        }

        /// <summary>
        /// Variant used in UW1. No special level cases or check for empty item area
        /// </summary>
        /// <param name="talker"></param>
        /// <returns></returns>
        static IEnumerator do_demandUW1(uwObject talker)
        {
            var ifYouInsist = GetConvoStackValueAtPtr(stackptr - 2); //npc gives in
            var noYouWont = GetConvoStackValueAtPtr(stackptr - 1); //refuses
            var attitudeOffset = FindVariableAddress("npc_attitude");
            var attitude = at(attitudeOffset);

            int playerdemandscore = GetPlayerDemandScore();
            int npcdemandscore = GetNPCDemandScore(talker, attitude);

            if (playerdemandscore > npcdemandscore)
            {
                //succeed
                yield return say_op(ifYouInsist);
                result_register = 1;
                if (attitude > 1)
                {
                    attitude--;
                    Set(attitudeOffset, attitude);
                }
            }
            else
            {
                //fail
                yield return say_op(noYouWont);
                result_register = 0;
                npc.SetGoalAndGtarg(talker, 5, 1);//make npc hostile                    
            }

            yield return 0;
        }


        /// <summary>
        /// Variation of do_demand where trade slots are also checked to see if they are selected
        /// Also special cases for certain levels where it is harder to demand items
        /// </summary>
        /// <returns></returns>
        static IEnumerator do_demandUW2(uwObject talker)
        {
            var ifYouInsist = GetConvoStackValueAtPtr(stackptr - 2); //npc gives in
            var noYouWont = GetConvoStackValueAtPtr(stackptr - 1); //refuses
            var whatItems = GetConvoStackValueAtPtr(stackptr - 3); //there are no items selected

            var attitudeOffset = FindVariableAddress("npc_attitude");
            var attitude = at(attitudeOffset);

            if (IsNPCOfferingItems())
            {
                int playerdemandscore = GetPlayerDemandScore();
                int npcdemandscore = GetNPCDemandScore(talker, attitude);

                if ((playerdat.dungeon_level == 9) || (playerdat.dungeon_level == 17))
                {//special case. Unknown why 9 = prison tower basement, 17 is killorn main level
                    npcdemandscore = (npcdemandscore * 3) / 2;
                }
                if (playerdemandscore > npcdemandscore)
                {
                    //succeed
                    yield return say_op(ifYouInsist);
                    result_register = 1;
                    if (attitude > 1)
                    {
                        attitude--;
                        Set(attitudeOffset, attitude);
                    }
                }
                else
                {
                    //fail
                    yield return say_op(noYouWont);
                    result_register = 0;
                    npc.SetGoalAndGtarg(talker, 5, 1);//make npc hostile                    
                }
            }
            else
            {
                //no items offered.
                if (attitude > 1)
                {
                    attitude--;
                    Set(attitudeOffset, attitude);
                }
                yield return say_op(whatItems);
                TradeResult = 0;
            }

            yield return 0;
        }

        private static int GetNPCDemandScore(uwObject talker, short attitude)
        {
            var npcdemandscore = 1;
            //get value of items selected on npc area
            var npcvalue = GetNPCValue(
                ApplyLikeDislike: true,
                appraise_accuracy: NPCAppraisalAccuracy,
                applyAccuracy: true);
            var avghit = critterObjectDat.avghit(talker.item_id);
            if (avghit > 0)
            {
                npcdemandscore = 2 - (
                    ((avghit - talker.npc_hp) << 1)
                    / avghit
                    );
            }
            var hungerbit = ((talker.npc_hunger) & (0x40)) >> 6;
            var di = 0;
            if (hungerbit == 1)
            {
                di = -1;
            }
            else
            {
                if (attitude < 2)
                {
                    di = 1;
                }
            }
            npcdemandscore = (npcvalue / 10)
                + critterObjectDat.npc_level(talker.item_id)
                + di
                + npcdemandscore;
            return npcdemandscore;
        }

        private static int GetPlayerDemandScore()
        {
            var playerdemandscore = 1;
            if (playerdat.max_hp > 0)
            {
                playerdemandscore = 2 - (
                    ((playerdat.max_hp - playerdat.play_hp) << 1)
                    / playerdat.max_hp
                    );
            }
            var weapondrawn = playerdat.play_drawn;//although this is impossible to be set unless the npc initiates conversation....
            var charm = playerdat.Charm / 6;
            var level = playerdat.play_level;
            playerdemandscore += level + weapondrawn + charm;
            return playerdemandscore;
        }
    }//end class
}//end namespace