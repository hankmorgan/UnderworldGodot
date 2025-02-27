namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the talk verb
    /// </summary>
    public class talk : UWClass
    {
        public static bool Talk(uwObject ObjectUsed, bool WorldObject = true)
        {
            if (ObjectUsed != null)
            {
                //var obj = objList[index];
                switch (ObjectUsed.majorclass)
                {
                    case 1: //NPCs
                        {
                            uimanager.InteractionModeToggle(uimanager.InteractionModes.ModeTalk);//ensure this mode is on                                    
                            ConversationVM.StartConversation(ObjectUsed);
                            break;
                        }
                    default:
                        {
                            if (_RES == GAME_UW2)
                            {
                                if (ObjectUsed.item_id == 461)
                                {//a wisp, which is a static object in UW2

                                    var wisp = SpawnTemporaryTalker(whoami: 48, tileX: playerdat.tileX, tileY: playerdat.tileY);
                                    uimanager.InteractionModeToggle(uimanager.InteractionModes.ModeTalk);//ensure this mode is on        
                                    ConversationVM.StartConversation(wisp);
                                    return false;
                                }
                            }
                            uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_cannot_talk_to_that_));
                            break;
                        }
                }
            }
            return false;
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