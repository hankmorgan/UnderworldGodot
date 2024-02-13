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
                Debug.Print($"ConversationNo is {conversationNo}");
                currentConversation = null;
                //Check if npc can be talked to
                if ((conversations[conversationNo].CodeSize == 0) || (talker.npc_whoami == 255))
                {//006~007~001~You get no response.
                    uimanager.AddToMessageScroll(GameStrings.GetString(7, 1));
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
                        RunConversationVM(talker),
                        main.instance);
                }
            }
        }

        private static void SetupConversationUI(uwObject talker)
        {

            if (_RES==GAME_UW2)
            {//set up the unique ui for uw2 conversations
                uimanager.instance.mainwindowUW2.Texture = uimanager.bitmaps.LoadImageAt(BytLoader.CONV_BYT, false);
            }
            else
            {
                //UW1 specific.
            }

            Likes = 0;
            Dislikes = 0;
            
            for (int i=0;i<uimanager.NoOfTradeSlots;i++)
            {
               uimanager.SetPlayerTradeSlot(i,-1,false);
               uimanager.SetNPCTradeSlot(i,-1,false);
            }
            
            //Clear existing text
            uimanager.instance.scroll.Clear();
            uimanager.instance.convo.Clear();
            //uimanager.instance.ConversationText.Text = "";
            
            var head = new GRLoader(GRLoader.HEADS_GR, GRLoader.GRShaderMode.UIShader);
            //set up relevant UI
            uimanager.EnableDisable(uimanager.instance.ConversationPanelUW1, _RES!=GAME_UW2);
            uimanager.EnableDisable(uimanager.instance.ConversationPanelUW2, UWClass._RES==UWClass.GAME_UW2);

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

            //Init conversation trade globals
            Rng.r = new System.Random(talker.item_id);//rng is always set to npcs item id
            TradeThreshold = Rng.RandomOffset(critterObjectDat.TradeThreshold(talker.item_id), -25, +25);
            TradePatience = Rng.RandomOffset(critterObjectDat.TradePatience(talker.item_id), -20, +100); 
            NPCAppraisalAccuracy = Rng.RandomOffset( (16 - critterObjectDat.TradeAppraisal(talker.item_id)) * 6 , -25,+50); 
            PreviousEvaluation = 0;

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
                if (_RES == GAME_UW2)
                {//some special portrait cases due to weirdness with charhead.gr
                    switch (talker.npc_whoami)
                    {
                        case 0x11://Janar (for some reason his portrait loads as molloy)
                            return chead.LoadImageAt(2);
                    }
                }
                return chead.LoadImageAt(talker.npc_whoami - 1);
            }
        }
    }//end class
}//end namespace