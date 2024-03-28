using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void give_ptr_npc(uwObject talker)
        {
            var qty = at(at(stackptr-1));
            var ObjectIndex = at(at(stackptr-2));
            Debug.Print($"{qty} of {ObjectIndex}");
            //try and find in trade area first.
            var itemcount = GetPlayerSelectedTradeItems(out int[] itemIds, out int[] itemIndices);

            for (int i=0; i<itemcount;i++)
            {
                if (ObjectIndex == itemIndices[i])
                {
                    GiveItemIndexToNPC(talker, ObjectIndex);
                    result_register =1;
                    return;
                }                
            }
            //if not found try and find directly in player inventory and take qty of that object
            Debug.Print("Incomplete behaviour. give_ptr_npc has not found object and needs to search for qty");

            result_register = 0;//nothing traded            
        }
    }//end class
}//end namespace