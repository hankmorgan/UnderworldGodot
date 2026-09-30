namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the talk verb
    /// </summary>
    public class talk : UWClass
    {
        public static void Talk(uwObject ConversationNPC, bool WorldObject = true, bool dyingNPC = false)
        {
            if (ConversationNPC != null)
            {
                if (_RES == GAME_UW2)
                {
                    TalkUW2(ConversationNPC);
                }
                else
                {
                    //uw1 logic.
                }
            }
        }

        private static void TalkUW2(uwObject ConversationNPC)
        {
            if (ConversationNPC.item_id == 0x1CD)
            {
                //a wisp, which is a static object in UW2
                var wisp = SpawnTemporaryTalker(whoami: 48, tileX: playerdat.playerObject.tileX, tileY: playerdat.playerObject.tileY);
                ConversationVM.StartConversation(wisp);
            }

            else
            {
                if (ConversationNPC.majorclass == 1)
                {
                    if (ConversationNPC.npc_goal != 0xF)
                    {
                        if (playerdat.FreezeTimeEnchantment)
                        {
                            //world is subject to freeze time.
                            uimanager.AddToMessageScroll(GameStrings.GetString(7, 1)); // you get no response.
                        }
                        else
                        {
                            switch (ConversationNPC.npc_whoami)
                            {
                                case 0x8C: //patterson is the only npc that will talk in combat
                                    ConversationVM.StartConversation(ConversationNPC);
                                    break;
                                default:
                                    {
                                        if ((ConversationNPC.npc_goal == (byte)npc.npc_goals.npc_goal_attack_5) || (ConversationNPC.npc_goal == (byte)npc.npc_goals.npc_goal_fear_6) || (ConversationNPC.npc_goal == (byte)npc.npc_goals.npc_goal_attack_9))
                                        {
                                            goto ovr103_EA;
                                        }
                                        else
                                        {
                                            goto ovr103_FD;
                                        }

                                    ovr103_EA:
                                        if (ConversationNPC.npc_gtarg == 1)
                                        {
                                            goto ovr103_10F;
                                        }
                                        else
                                        {
                                            goto ovr103_FD;
                                        }

                                    ovr103_FD:
                                        if (ConversationNPC.npc_attitude != 0)
                                        {
                                            goto ovr103_123;
                                        }

                                    ovr103_10F:
                                        if (ConversationNPC.IsAlly == 0)
                                        {
                                            goto ovr103_129;
                                        }

                                    ovr103_123:
                                        //talk to npc. Further logic like generic npc and empty size code blocks is handled in the next function.
                                        if (ConversationNPC.npc_whoami != 0xFF)
                                        {
                                            goto ovr103_152;
                                        }
                                        else
                                        {
                                            goto ovr103_129;
                                        }

                                    ovr103_129:
                                        if (ConversationNPC.npc_goal != (byte)npc.npc_goals.npc_goal_want_to_talk)
                                        {
                                            uimanager.AddToMessageScroll(GameStrings.GetString(7, 1)); // you get no response.
                                            return;
                                        }
                                        else
                                        {
                                            goto ovr103_152;
                                        }

                                    ovr103_152:
                                        //further logic like generic conversations  (whoami==0) is handled in the next function.
                                        ConversationVM.StartConversation(ConversationNPC);
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        //goal is 0xF (petrified/stonestrike)
                        uimanager.AddToMessageScroll(GameStrings.GetString(7, 1)); // you get no response.
                    }
                }
                else
                {
                    //not an npc
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_cannot_talk_to_that_));
                }
            }
        }



        /// <summary>
        /// Creates a temporary talker for special conversatons, Eg talking door or UW2 wisps
        /// </summary>
        /// <param name="whoami"></param>
        /// <returns></returns>
        public static uwObject SpawnTemporaryTalker(int whoami, int itemid = 64, int tileX = 32, int tileY = 32, int attitude = 3, int goal = 10)
        {
            var temporaryTalker = ObjectCreator.spawnObjectInTile(
                itemid: itemid,
                tileX: tileX, tileY: tileY,
                xpos: 0, ypos: 0, zpos: 0,
                WhichList: ObjectFreeLists.ObjectListType.MobileList);
            temporaryTalker.npc_whoami = (short)whoami;
            temporaryTalker.npc_attitude = (short)attitude;
            temporaryTalker.npc_goal = (byte)goal;
            ConversationVM.TemporaryTalker = true;
            return temporaryTalker;
        }

    } //end class
} //end namespace