using System.Diagnostics;
namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
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

            if (conversations != null)
            {
                int conversationNo = GetConversationNumber(npc);
                Debug.Print ($"ConversationNo is {conversationNo}");

                //Check if npc can be talked to
                if ((conversations[conversationNo].CodeSize == 0) || (npc.npc_whoami == 255))
                {//006~007~001~You get no response.
                    messageScroll.AddString(GameStrings.GetString(7, 1));
                    return;
                }
                else
                { //a conversation can be had (TODO take hostility into account. Some special NPCs can be talked to in combat. eg rodric and patterson)

                    InConversation = true;
                    SetupConversationUI(npc);

                    InitialiseConversationMemory();    

                    ImportVariables(npc, conversations[conversationNo]);

                    //Launch conversation VM co-routine 
                    _ = Peaky.Coroutines.Coroutine.Run(
                        ConversationVM.RunConversationVM(npc, conversations[conversationNo]),
                        main.instance);
                }
            }
        }

        private static void SetupConversationUI(uwObject npc)
        {
            var head = new GRLoader(GRLoader.HEADS_GR, GRLoader.GRShaderMode.UIShader);
            //set up relevant UI
            uimanager.EnableDisable(uimanager.instance.ConversationPanel, true);

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
        /// Gets the appropiate portrait for an NPC
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
    }//end class
}//end namespace