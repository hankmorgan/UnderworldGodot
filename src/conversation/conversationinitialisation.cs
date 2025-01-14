using System.Diagnostics;
using Godot;
namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static uwObject currentTalker;
        public static void StartConversation(uwObject talker)
        {
            currentTalker=talker;
            //talker.npc_whoami = 46; jerry the rat
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
                if(conversations[conversationNo] == null)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(7, 1));
                    return;
                }
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
                    DoTeleport = false;
                    TeleportToLevel = -1;
                    TeleportTileX = - 1;
                    TeleportTileY = -1;

                    SetupConversationUI(talker);

                    InitialiseConversationMemory();

                    ImportVariables(talker);
                    
                    //stop player motion
                    main.gamecam.Set("MOVE", false);

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
                for (int i=0; i<=uimanager.instance.SelectedRunes.GetUpperBound(0);i++)
                {
                uimanager.EnableDisable(uimanager.instance.SelectedRunes[i],false);
                }
                for (int i=0; i<=uimanager.instance.InteractionButtonsUW2.GetUpperBound(0);i++)
                {//disable interaction buttons
                    uimanager.EnableDisable(uimanager.instance.InteractionButtonsUW2[i],false);
                }

                uimanager.instance.scroll.Columns = 100;
                uimanager.instance.messageScrollUW2.SetSize(new Godot.Vector2(1160,140));
                uimanager.EnableDisable(uimanager.instance.CompassPanelUW2,false);
                uimanager.EnableDisable(uimanager.instance.PowerGemUW2,false);
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

            if(uimanager.PanelMode!=0)
            {
                uimanager.SetPanelMode(0);//make sure inventory paperdoll is displayed
            }

            //turn off mouselook to allow clicking around the screen.
            Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Hidden;
            main.gamecam.Set("MOUSELOOK", false);
            

            //npc name and portrait
            uimanager.instance.NPCNameLabel.Text = talker.a_name;
            uimanager.instance.NPCPortrait.Texture = NPCPortrait(talker.npc_whoami, talker.item_id);

            //Init conversation trade globals
            Rng.r = new System.Random(talker.item_id);//rng is always set to npcs item id
            TradeThreshold = Rng.RandomOffset(critterObjectDat.TradeThreshold(talker.item_id), -25, +25);
            TradePatience = Rng.RandomOffset(critterObjectDat.TradePatience(talker.item_id), -20, +100); 
            NPCAppraisalAccuracy = Rng.RandomOffset( (16 - critterObjectDat.TradeAppraisal(talker.item_id)) * 6 , -25,+50); 
            PreviousEvaluation = 0;
            TradeResult = 0;

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
        private static Texture2D NPCPortrait(int whoami, int item_id)
        {
            //Assume generic head first.

            bool UseGenericHead = true;
            switch (_RES)
            {
                case GAME_UW2:
                    if (whoami != 0)
                    {
                        UseGenericHead = false;
                    }
                    break;
                default:
                    {
                        if ((whoami > 0) && (whoami <= 28))
                        {
                            UseGenericHead = false;
                        }
                        break;
                    }
            }
            if (UseGenericHead)
            {
                var ghead = new GRLoader(GRLoader.GENHEAD_GR, GRLoader.GRShaderMode.UIShader);
                return ghead.LoadImageAt(item_id & 0x3F);
            }
            else
            {
                var chead = new GRLoader(GRLoader.CHARHEAD_GR, GRLoader.GRShaderMode.UIShader);
                if (_RES == GAME_UW2)
                {//some special portrait cases due to weirdness with charhead.gr
                    switch (whoami)
                    {
                        case 0x11://Janar (for some reason his portrait loads as molloy)
                            return chead.LoadImageAt(2);
                    }
                }
                return chead.LoadImageAt(whoami - 1);
            }
        }

    }//end class
}//end namespace