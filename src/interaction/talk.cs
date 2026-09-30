
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
                    TalkUW1(ConversationNPC);
                }
            }
        }


        /// <summary>
        /// Logic for initiating conversations in UW1. 
        /// Handles Golem, Tybal and Rodrick who can be spoken to during conversations.
        /// Special cases for the Shrine and the Chained Up Princess.
        /// </summary>
        /// <param name="ConversationNPC"></param>
        private static void TalkUW1(uwObject ConversationNPC)
        {
            switch(ConversationNPC.item_id)
            {
                case 0x157://shrine
                    shrine.Use(ConversationNPC);
                    break;
                case 0x16E://tmap. probably Ariel
                    var walltexture = UWTileMap.current_tilemap.texture_map[ConversationNPC.owner];
                    if(TerrainDatLoader.Terrain[walltexture] == 8)
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x110)); // There is no reaction from the princess.
                    }
                    else
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(7, 0)); // you can't talk to that
                    }
                    break;
                default:
                    {
                        switch (ConversationNPC.npc_whoami)
                        {
                            case 0x16://GOLEM
                            case 0x8E: //RODRICK
                            case 0xE7: //TYBAL
                                ConversationVM.StartConversation(ConversationNPC);
                                break;
                            default:
                                {
                                    DefaultTalkLogic(ConversationNPC);
                                    break;
                                }
                        }
                        break;
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
                            DefaultTalkLogic(ConversationNPC);
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
        /// Determines if a npc can be talked to based on AI state, Attitude and goals
        /// </summary>
        /// <param name="ConversationNPC"></param>
        private static void DefaultTalkLogic(uwObject ConversationNPC)
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
                            goto ovr103_129_checkiftalkgoal;
                        }

                    ovr103_123:                        
                        if (ConversationNPC.npc_whoami != 0xFF)
                        {
                            goto ovr103_152_startconversation;
                        }
                        else
                        {
                            goto ovr103_129_checkiftalkgoal;
                        }

                    ovr103_129_checkiftalkgoal:
                        if (ConversationNPC.npc_goal != (byte)npc.npc_goals.npc_goal_want_to_talk)
                        {
                            uimanager.AddToMessageScroll(GameStrings.GetString(7, 1)); // you get no response.
                            return;
                        }
                        else
                        {
                            goto ovr103_152_startconversation;
                        }

                    ovr103_152_startconversation:
                        //further logic like generic conversations  (whoami==0) is handled in the next function.
                        ConversationVM.StartConversation(ConversationNPC);
                        break;
                    }
            }

            return;
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