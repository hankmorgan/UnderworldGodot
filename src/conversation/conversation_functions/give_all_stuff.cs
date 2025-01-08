namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        /// <summary>
        /// Transfers all items in the NPC trade area to the player.
        /// </summary>
        public static void give_all_stuff()
        {
            result_register = 0;//default case if nothing gets moved.
            for (int s =0; s<uimanager.NoOfTradeSlots; s++)
            {
                var itemAtIndex = uimanager.GetNPCTradeSlot(slotno: s, OnlySelected: false);
                if (itemAtIndex !=-1)
                {//mark the slot as selecte
                    uimanager.NPCTradeOn(s);
                    TradeResult = 1; 
                    result_register = 1;//at least one item to transfer                    
                }
                else
                {//mark the slot as off
                    uimanager.NpcTradeOff(s);
                }
                uimanager.PlayerTradeOff(s);//hack. Turn off player selected slot because at completion of conversation the item will get inadvertantly traded                
            }
        }
    }//end class
}//end namespace