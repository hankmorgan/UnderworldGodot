using System.Diagnostics;
namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void StartConversation(uwObject talker)
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
                int conversationNo = GetConversationNumber(talker);
                Debug.Print ($"ConversationNo is {conversationNo}");
                currentConversation = null;
                //Check if npc can be talked to
                if ((conversations[conversationNo].CodeSize == 0) || (talker.npc_whoami == 255))
                {//006~007~001~You get no response.
                    messageScroll.AddString(GameStrings.GetString(7, 1));
                    return;
                }
                else
                { //a conversation can be had (TODO take hostility into account. Some special NPCs can be talked to in combat. eg rodric and patterson)
                    currentConversation = conversations[conversationNo]; 
                    InConversation = true;
                    SetupConversationUI(talker);

                    InitialiseConversationMemory();    

                    ImportVariables(talker);
                    //Test
                    
                    //Launch conversation VM co-routine 
                    _ = Peaky.Coroutines.Coroutine.Run(
                        ConversationVM.RunConversationVM(talker),
                        main.instance);
                }
            }
        }

        private static void SetupConversationUI(uwObject talker)
        {
            uimanager.instance.ConversationText.Text = "";
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
            uimanager.instance.NPCNameLabel.Text = talker._name;
            uimanager.instance.NPCPortrait.Texture = NPCPortrait(talker);
        }

        private static int GetConversationNumber(uwObject talker)
        {
            int conversationNo;
            if (talker.npc_whoami == 0)
            {
                conversationNo = 256 + (talker.item_id - 64);
            }
            else
            {
                conversationNo = talker.npc_whoami;
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
        public static Godot.Texture2D NPCPortrait(uwObject talker)
        {
            //Assume generic head first.
            bool UseGenericHead = true;
            switch (_RES)
            {
                case GAME_UW2:
                    if (talker.npc_whoami != 0)
                    {
                        UseGenericHead = false;
                    }
                    break;
                default:
                    {
                        if ((talker.npc_whoami > 0) && (talker.npc_whoami <= 28))
                        {
                            UseGenericHead = false;
                        }
                        break;
                    }
            }
            if (UseGenericHead)
            {
                var ghead = new GRLoader(GRLoader.GENHEAD_GR, GRLoader.GRShaderMode.UIShader);
                return ghead.LoadImageAt(talker.item_id & 0x3F);
            }
            else
            {
                var chead = new GRLoader(GRLoader.CHARHEAD_GR, GRLoader.GRShaderMode.UIShader);
                return chead.LoadImageAt(talker.npc_whoami - 1);
            }
        }
    }//end class
}//end namespace