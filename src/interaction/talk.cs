namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the talk verb
    /// </summary>
    public class talk : UWClass
    {
        public static bool Talk(int index, uwObject[] objList, bool WorldObject = true)
        {
            if (index <= objList.GetUpperBound(0))
            {
                var obj = objList[index];
                switch (obj.majorclass)
                {
                    case 1: //NPCs
                        {
                            StartConversation(obj);
                            break;
                        }
                    default:
                        {
                            messageScroll.AddString(GameStrings.GetString(1, GameStrings.str_you_cannot_talk_to_that_));
                            break;
                        }
                }
            }
            return false;
        }


        public static void StartConversation(uwObject npc)
        {
            //Try and load the conversation from the ark files.
            if (!cnvArkLoader.Loaded)
            {
                switch(_RES)
                {
                    case GAME_UW2:
                        cnvArkLoader.LoadCnvArkUW2(System.IO.Path.Combine(BasePath,"DATA","CNV.ARK"));break;
                    default:
                        cnvArkLoader.LoadCnvArkUW1(System.IO.Path.Combine(BasePath,"DATA","CNV.ARK"));break;
                }
                
            }

            if (ConversationVM.conversations!=null)
            {
                int conversationNo;
                if (npc.npc_whoami == 0)
                {
                    conversationNo = 256 + (npc.item_id - 64);               
                }
                else
                {
                    conversationNo = npc.npc_whoami;
                    if (_RES == GAME_UW2)
                    {
                        conversationNo++;
                    }
                }              

            

                //set up relevant UI
                messageScroll.AddString($"Talking to {npc._name} conversation no {conversationNo}");

                //Launch conversation VM task. 
                // var t = Task.Run(() => ConversationVM.RunConversationVM(npc, ConversationVM.conversations[conversationNo]));
                _ = Peaky.Coroutines.Coroutine.Run(
                    ConversationVM.RunConversationVM(npc, ConversationVM.conversations[conversationNo]),
                    main.instance);
            }
        }
    }
}