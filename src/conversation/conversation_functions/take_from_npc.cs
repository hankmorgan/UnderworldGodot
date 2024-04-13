using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void take_from_npc(uwObject talker)
        {
            var arg1 = GetConvoStackValueAtPtr(stackptr-1);
            Debug.Print($"take_from_npc({arg1})");


            // if (playerdat.ObjectInHand!=-1)
            // {
            //     result_register = 0; return;
            // }

            if (talker.LootSpawnedFlag==0)
            {
                npc.spawnloot(talker);
            }

            var nextItem = talker.link;
            var previous = talker;
            while (nextItem!=0)
            {
                var obj = UWTileMap.current_tilemap.LevelObjects[nextItem];
                bool match = false;
                if (arg1>999)
                {
                    if (obj.OneF0Class == arg1 - 1000)
                    {
                        match = true;
                    }
                }
                else
                {
                    if (arg1 == obj.item_id)
                    {
                        match = true;
                    }
                }
                
                if(match)
                {   
                    //Unlink from npc
                    if (talker.link == obj.index)
                    {
                        talker.link = obj.next;
                        obj.next = 0;
                    }
                    else
                    {
                        previous.next = obj.next;
                        obj.next = 0;
                    }

                    //TODO add a weight check. if passed it goes to hand, if not add to trade slots
                    if (playerdat.CanCarryWeight(obj))
                    {
                        playerdat.ObjectInHand = obj.index;
                        uimanager.instance.mousecursor.SetCursorToObject(obj.item_id);
                        result_register = 1;
                        return;
                    }
                    else
                    {
                        //add to a free slot
                        for (int i =0; i<uimanager.NoOfTradeSlots;i++)
                        {
                            if (uimanager.GetPlayerTradeSlot(i,false) ==-1)
                            {
                                //add to that slot
                                uimanager.SetPlayerTradeSlot(i, obj.index, false);
                                result_register = 1;
                                return;
                            }
                        }
                    }
                }
                previous = obj;
                nextItem = obj.next;
            }


        }
    }   //end class
}//end namespace