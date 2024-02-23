namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void give_to_npc(uwObject talker)
        {
            var CountItems_arg1 = GetConvoStackValueAtPtr(stackptr - 2);//how many items to get
            var ItemIndexToGive_arg2 = at(stackptr - 1); // array of item ids to get.  GetConvoStackValueAtPtr(stackptr-1);

            //Get the player inventory
            var itemcount = GetPlayerSelectedTradeItems(out int[] itemIds, out int[] itemIndices);
            if (itemcount >= CountItems_arg1)
            {
                bool ItemsGiven = false;
                for (int s = 0; s < CountItems_arg1; s++)
                {
                    var itemIndexToFind = at(ItemIndexToGive_arg2 + s);
                    //loop inventorys to find  matching item
                    bool itemFound = false;
                    for (int i = 0; i < uimanager.NoOfTradeSlots && !itemFound; i++)
                    {
                        if (itemIndices[i] == itemIndexToFind)
                        {                            
                            GiveItemIndexToNPC(talker, itemIndexToFind);
                            itemFound = true;
                            ItemsGiven = true;
                        }
                    }
                }
                if (ItemsGiven)
                {
                    result_register = 1;
                }
                else
                {
                    result_register = 0;
                }
                return;
            }
            else
            {//not enough items selected.
                result_register = 0;
                return;
            }
        }
    }//end class
}//end namespace