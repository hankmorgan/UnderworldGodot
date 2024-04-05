using System.Diagnostics;

namespace Underworld
{

    public partial class ConversationVM : UWClass
    {
        /// <summary>
        /// Finds all matches and total quantity of a matching item.
        /// </summary>
        public static void find_barter_total()
        {
            var item_id = at(at(stackptr - 4));
            var PtrNoOfMatches = at(stackptr - 3);
            var PtrResultArray = at(stackptr - 2);
            var PtrTotalQty = at(stackptr - 1);

            Debug.Print($"find_barter_total({item_id})");
            var itemcount = GetPlayerSelectedTradeItems(out int[] itemIds, out int[] itemIndices);
            int[] MatchingIndices = new int[uimanager.NoOfTradeSlots];
            var di_qty = 0;
            var NoOfMatches = 0;
            if (item_id < 1000)
            {//note in UW2 there appears to be a bug involving not being able to search on itemids>=1000. 
                for (int si = 0; si < itemcount; si++)
                {
                    if (itemIds[si] == item_id)
                    {
                        MatchingIndices[NoOfMatches] = itemIds[si];
                        //match found.
                        var obj = UWTileMap.current_tilemap.LevelObjects[itemIndices[si]];
                        if (obj != null)
                        {
                            di_qty += obj.ObjectQuantity;
                        }
                        NoOfMatches++;
                    }
                }
            }
            Set(PtrNoOfMatches, NoOfMatches);
            Set(PtrTotalQty, di_qty);

            //Store array of item indices on the stack
            for (int si = 0; si < NoOfMatches; si++)
            {
                Set(PtrResultArray, MatchingIndices[si]);
                PtrResultArray++;
            }
            if (di_qty == 0)
            {
                result_register = 0;
            }
            else
            {
                result_register = 1;
            }
        }
    }//end class
}//end namespace