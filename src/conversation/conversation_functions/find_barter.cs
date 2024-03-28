namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void find_barter()
        {
            var toFind = at(at(stackptr - 1));
            var itemcount = GetPlayerSelectedTradeItems(out int[] itemIds, out int[] itemIndices);
            if (toFind < 1000)
            {//find exact match
                for (int i=0; i<itemcount;i++)
                {
                    if (itemIds[i] == toFind)
                        {
                            result_register = itemIndices[i];
                            return;
                        }
                }
            }
            else
            {
                var major = (toFind-1000) >> 2;
                var minor = (toFind-1000) & 0x3;
                
                for (int i=0; i<itemcount;i++)
                {
                    var itemToCheck = UWTileMap.current_tilemap.LevelObjects[itemIndices[i]];
                    if (itemToCheck!=null)
                    {
                        if ((itemToCheck.majorclass== major) && (itemToCheck.majorclass==minor))
                        {
                            result_register = itemIndices[i];
                            return;
                        }
                    }
                }
            }
            result_register = 0;
        }
    }   //end class
}//end namespace