namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void show_inv()
        {
            var ResultIndices = at(stackptr - 1);
            var ResultItemIds = at(stackptr - 2);            
            var itemcount = GetPlayerSelectedTradeItems(out int[] itemIds, out int[] itemIndices);
            for (int s = 0; s<uimanager.NoOfTradeSlots; s++)
            {
                if (s<itemcount)
                {
                    Set(ResultItemIds + s, itemIds[s]);
                    Set(ResultIndices + s, itemIndices[s]);
                }
                else
                {
                    Set(ResultItemIds + s, 0);
                    Set(ResultIndices + s, 0);
                }
            }
            result_register = itemcount;
        }
    }   //end class
}//end namespace