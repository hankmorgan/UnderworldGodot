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
            Debug.Print("UNTESTED give_ptr_npc code!");
            var ptrObject = UWTileMap.current_tilemap.LevelObjects[ObjectIndex];
            if (ptrObject!=null)
            {
                if (qty<=0)
                    {
                        //give all
                        var link = talker.link;
                        ptrObject.next = talker.link; //assumes no existing next???
                        talker.link = ptrObject.index;
                    }
                else
                {                   
                    if (ptrObject.ObjectQuantity<qty)
                    {
                         //give qty of object.
                        var clone = ObjectCreator.spawnObjectInTile(
                            itemid: ptrObject.item_id, 
                            tileX: 99, tileY: 99, 
                            xpos: ptrObject.xpos, ypos: ptrObject.ypos, zpos: ptrObject.zpos, 
                            WhichList: ObjectCreator.ObjectListType.StaticList);

                        clone.is_quant = ptrObject.is_quant;
                        clone.flags_full = ptrObject.flags_full;
                        clone.quality = ptrObject.quality;
                        clone.owner = ptrObject.owner;                        
                        clone.link = qty;
                        ptrObject.link -= qty;

                        var link = talker.link;
                        clone.next = talker.link; //assumes no existing next???
                        talker.link = clone.index;
                    }
                    else
                    {
                        //give all
                        var link = talker.link;
                        ptrObject.next = talker.link; //assumes no existing next???
                        talker.link = ptrObject.index;
                    }
                }   
                
            }

            Debug.Print("Incomplete behaviour. give_ptr_npc has not found object and needs to search for qty");
            
            result_register = 0;//nothing traded            
        }
    }//end class
}//end namespace