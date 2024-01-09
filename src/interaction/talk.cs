using System.Diagnostics;

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
                switch (_RES)
                {
                    case GAME_UW2:
                        cnvArkLoader.LoadCnvArkUW2(System.IO.Path.Combine(BasePath, "DATA", "CNV.ARK")); break;
                    default:
                        cnvArkLoader.LoadCnvArkUW1(System.IO.Path.Combine(BasePath, "DATA", "CNV.ARK")); break;
                }

            }

            if (ConversationVM.conversations != null)
            {
                int conversationNo;
                conversationNo = GetConversationNumber(npc);

                //Check if npc can be talked to
                if ((ConversationVM.conversations[conversationNo].CodeSize == 0) || (npc.npc_whoami == 255))
                {//006~007~001~You get no response.
                    messageScroll.AddString(GameStrings.GetString(7, 1));
                    return;
                }
                else
                { //a conversation can be had (TODO take hostility into account. Some special NPCs can be talked to in combat. eg rodric and patterson)
                    var head = new GRLoader(GRLoader.HEADS_GR, GRLoader.GRShaderMode.UIShader);

                    //set up relevant UI
                    Debug.Print($"Talking to {npc._name} conversation no {conversationNo}");
                    uimanager.EnableDisable(uimanager.instance.ConversationPanel, true);
                    ConversationVM.InConversation = true;

                    //Player name and portrait
                    if (playerdat.isFemale)
                    {
                        uimanager.instance.PlayerPortrait.Texture = head.LoadImageAt(playerdat.Body + 5);
                    }
                    else
                    {
                        uimanager.instance.PlayerPortrait.Texture = head.LoadImageAt(playerdat.Body);
                    }
                    uimanager.instance.PlayerNameLabel.Text = playerdat.CharName;

                    //npc name and portrait
                    uimanager.instance.NPCNameLabel.Text = npc._name;
                    uimanager.instance.NPCPortrait.Texture = NPCPortrait(npc);


                    //Launch conversation VM corouting. 
                    _ = Peaky.Coroutines.Coroutine.Run(
                        ConversationVM.RunConversationVM(npc, ConversationVM.conversations[conversationNo]),
                        main.instance);
                }
            }
        }

        private static int GetConversationNumber(uwObject npc)
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

            return conversationNo;
        }

        /// <summary>
        /// Gets the appropiate protrait for an NPC
        /// </summary>
        /// <param name="whoami"></param>
        /// <returns></returns>
        public static Godot.Texture2D NPCPortrait(uwObject npc)
        {
            //Assume generic head first.
            bool UseGenericHead = true;
            switch (_RES)
            {
                case GAME_UW2:
                    if (npc.npc_whoami != 0)
                    {
                        UseGenericHead = false;
                    }
                    break;
                default:
                    {
                        if ((npc.npc_whoami > 0) && (npc.npc_whoami <= 28))
                        {
                            UseGenericHead = false;
                        }
                        break;
                    }
            }
            if (UseGenericHead)
            {
                var ghead = new GRLoader(GRLoader.GENHEAD_GR, GRLoader.GRShaderMode.UIShader);
                return ghead.LoadImageAt(npc.item_id & 0x3F);
            }
            else
            {
                var chead = new GRLoader(GRLoader.CHARHEAD_GR, GRLoader.GRShaderMode.UIShader);
                return chead.LoadImageAt(npc.npc_whoami - 1);
            }
        }
    } //end class
} //end namespace