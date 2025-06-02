using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void take_id_from_npc(uwObject talker)
        {
            var id = GetConvoStackValueAtPtr(stackptr-1);
            Debug.Print($"Take ID {id}");
            var ObjectToGive = UWTileMap.current_tilemap.LevelObjects[id];
            if (playerdat.ObjectInHand  == -1)
            {
                //player is holding nothing                
                if (talker.link!=0)
                {
                    // if (talker.link == id)
                    // {
                    //     talker.link = ObjectToGive.next;
                    //     ObjectToGive.next = 0;
                    // }
                    // else
                    // {
                        ObjectRemover.RemoveObjectFromLinkedList(talker.link, id, UWTileMap.current_tilemap.LevelObjects, talker.PTR+6);
                    //}
                }
                                
                if (playerdat.CanCarryWeight(ObjectToGive))
                {
                    //player can hold, place in hand
                    playerdat.ObjectInHand = ObjectToGive.index;
                    uimanager.instance.mousecursor.SetCursorToObject(ObjectToGive.item_id);
                    result_register = 1;
                }
                else
                {   //player cannot hold. Try and place on ground
                    objectInstance.PlaceObjectInTile(playerdat.tileX_depreciated, playerdat.tileY_depreciated, ObjectToGive);
                    result_register = 2;
                }
            }   
            else
            {
                //player is already holding something, try and place in trade slots
                for (int i = 0; i<uimanager.NoOfTradeSlots;i++)
                {
                    var objAtSlot = uimanager.GetPlayerTradeSlot(i, false);
                    if (objAtSlot==-1)
                    {   //place at slot
                        uimanager.SetPlayerTradeSlot(i,ObjectToGive.index,false);

                        result_register = 1;
                        return;
                    }
                }
                //cannot place in trade slot. drop on ground.
                objectInstance.PlaceObjectInTile(playerdat.tileX_depreciated, playerdat.tileY_depreciated, ObjectToGive);
                result_register = 2;
            }
        }

    }//end class
}//end namespace