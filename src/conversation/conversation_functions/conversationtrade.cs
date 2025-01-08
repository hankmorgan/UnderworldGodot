using System.Diagnostics;

namespace Underworld
{
    //Common data and code for item trading.
    public partial class ConversationVM : UWClass
    {

        /// <summary>
        /// The trade evaluation that has to be met to allow the NPC to trade. 
        /// Calculated using random offset and CritterDat[0xEh] bits 0-3
        /// RNG is seeded by critter item_id
        /// </summary>
        public static int TradeThreshold = 0;

        /// <summary>
        /// How much patience the NPC has for bad or insulting trade offers. 
        /// </summary>
        public static int TradePatience = 0;


        /// <summary>
        /// Measure of how good this NPC is at appraising the value of a trade item.
        /// </summary>
        public static int NPCAppraisalAccuracy = 0;

        /// <summary>
        /// Score of the previous trade evaluation. Used to determine if a better or worse offer has been made on repeated offers
        /// </summary>
        public static int PreviousEvaluation = 0;

        /// <summary>
		/// Pointer to Array in the stack that contains the items the npc likes
		/// </summary>
		public static int Likes = 0;

        /// <summary>
        /// Pointer to the array in the stack that contains the items the npc dislikes
        /// </summary>
        public static int Dislikes = 0;

        public static int TradeResult = 0;

        /// <summary>
        /// Get the total value of the players trade area (selected items)
        /// </summary>
        /// <param name="ApplyLikeDislike"></param>
        /// <param name="appraise_accuracy"></param>
        /// <returns></returns>
        static int GetNPCValue(bool ApplyLikeDislike = false, int appraise_accuracy = 0, bool applyAccuracy = false)
        {
            var result = 0;
            for (int i = 0; i < uimanager.NoOfTradeSlots; i++)
            {
                //if slot select get monetary value, possibly adjusted for likes dislike and player appraisal score
                var index = uimanager.GetNPCTradeSlot(i);
                result += GetTrueItemValue(
                    ApplyLikeDislike: ApplyLikeDislike,
                    appraise_accuracy: appraise_accuracy,
                    applyAccuracy: applyAccuracy,
                    index: index);
            }
            return result;
        }


        /// <summary>
        /// Get the total value of the PC trade area (selected items)
        /// </summary>
        /// <param name="ApplyLikeDislike"></param>
        /// <param name="appraise_accuracy"></param>
        /// <param name="applyAccuracy"></param>
        /// <returns></returns>
        static int GetPCValue(bool ApplyLikeDislike = false, int appraise_accuracy = 0, bool applyAccuracy = false)
        {
            var result = 0;
            for (int i = 0; i < uimanager.NoOfTradeSlots; i++)
            {
                //if slot select get monetary value, possibly adjusted for likes dislike and player appraisal score
                var index = uimanager.GetPlayerTradeSlot(i);
                result += GetTrueItemValue(
                    ApplyLikeDislike: ApplyLikeDislike,
                    appraise_accuracy: appraise_accuracy,
                    applyAccuracy: applyAccuracy,
                    index: index);
            }
            return result;
        }

        private static int GetTrueItemValue(bool ApplyLikeDislike, int appraise_accuracy, bool applyAccuracy, int index)
        {
            if (index != -1)
            {
                var obj = UWTileMap.current_tilemap.LevelObjects[index];
                if (obj != null)
                {
                    //obj.ObjectQuantity;
                    var itemvalue = commonObjDat.monetaryvalue(obj.item_id);
                    if (ApplyLikeDislike)
                    {//todo
                        itemvalue = itemvalue * 1;
                    }
                    itemvalue = itemvalue * obj.ObjectQuantity;
                    var quality = obj.quality;
                    //if coin quality = 0x40
                    if (
                        (obj.item_id == 160)
                        ||
                        ((_RES != GAME_UW2) && (obj.item_id == 161))
                        )
                    {
                        quality = 0x3F;
                    }

                    itemvalue = (itemvalue * quality) >> 6;
                    if (applyAccuracy)
                    {
                        Rng.r = new System.Random(obj.item_id);//RNG is always re-seeded with item_id before getting offset here
                        itemvalue = Rng.RandomOffset(itemvalue, -appraise_accuracy, +appraise_accuracy);
                    }
                    return itemvalue;
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets the value 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ApplyLikeDislike"></param>
        /// <param name="appraise_accuracy"></param>
        /// <returns></returns>
        static int GetValueOfItemToNPC(uwObject obj, bool ApplyLikeDislike, int appraise_accuracy)
        {
            float multiplier = 1f;
            int itemvalue = 0;
            var itemCategory = obj.item_id >> 4;

            var quality = obj.quality;
            if (((obj.item_id == 160) || (obj.item_id == 161)) && (_RES != GAME_UW2))
            {
                quality = 63;
            }
            if (((obj.item_id == 160)) && (_RES == GAME_UW2))
            {
                quality = 63;
            }

            if (ApplyLikeDislike)
            {
                //return the value adjusted for the npc likes dislikes.
                if (Likes != 0)
                {//loop the stack starting at Likes until value 0xFF is hit.
                 //if value is >=1000 then liked is a class of object
                 //if value < 1000 then liked is an exact object match
                 //if liked value is 1.5 times
                 //itemvalue = (commonObjDat.monetaryvalue(obj.item_id) / 3) * 2;
                 //Debug.Print($"Likes are {Likes}");                   

                    var arrayindex = Likes;
                    var entry = at(arrayindex);
                    while (entry != -1)
                    {
                        if (entry >= 1000)
                        {
                            if (itemCategory == entry - 1000)
                            {
                                multiplier = 1.5f;
                                Debug.Print($"NPC likes item category of {GameStrings.GetSimpleObjectNameUW(obj.item_id)}");
                            }
                        }
                        else
                        {

                            if (entry == obj.item_id)
                            {
                                multiplier = 1.5f;
                                Debug.Print($"NPC likes item {GameStrings.GetSimpleObjectNameUW(obj.item_id)}");
                            }
                        }
                        arrayindex++;
                        entry = at(arrayindex);
                    }
                }
                if (Dislikes != 0)
                {
                    //if unliked no value
                    //itemvalue = 0;
                    Debug.Print($"Dislikes are {Dislikes}");

                    var arrayindex = Dislikes;
                    var entry = at(arrayindex);
                    while (entry != -1)
                    {
                        if (entry >= 1000)
                        {
                            if (itemCategory == entry - 1000)
                            {
                                multiplier = 0f;
                                Debug.Print($"NPC dislikes item category of {GameStrings.GetSimpleObjectNameUW(obj.item_id)}");
                                return 0;
                            }
                        }
                        else
                        {
                            if (obj.item_id == entry)
                            {
                                multiplier = 0f;
                                Debug.Print($"NPC dislikes item {GameStrings.GetSimpleObjectNameUW(obj.item_id)}");
                                return 0;
                            }
                        }
                        arrayindex++;
                        entry = at(arrayindex);
                    }
                }

                itemvalue = (int)(commonObjDat.monetaryvalue(obj.item_id) * multiplier);
                itemvalue = (itemvalue * quality) >> 6;
            }
            else
            {
                //return the true base value adjusted for quality.
                itemvalue = commonObjDat.monetaryvalue(obj.item_id);
                itemvalue = (itemvalue * quality) >> 6;
            }

            //Adjust the value of the item based on the players/npcs appraisal accuracy score
            var offset = Rng.RandomOffset(itemvalue, -appraise_accuracy, +appraise_accuracy);
            return itemvalue;
        }

        /// <summary>
        /// Check if at least one item in the player trade is selected
        /// </summary>
        /// <returns></returns>
        static bool IsPlayerOfferingItems()
        {
            for (int i = 0; i < uimanager.NoOfTradeSlots; i++)
            {
                if (uimanager.GetPlayerTradeSlot(i) != -1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if at least one item in the npc trade is selected
        /// </summary>
        /// <returns></returns>
        static bool IsNPCOfferingItems()
        {
            for (int i = 0; i < uimanager.NoOfTradeSlots; i++)
            {
                if (uimanager.GetNPCTradeSlot(i) != -1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates arrays of the selected items and their item indices
        /// </summary>
        /// <param name="itemIds"></param>
        /// <param name="itemdIndices"></param>
        /// <returns>No of items</returns>
        static int GetPlayerSelectedTradeItems(out int[] itemIds, out int[] itemdIndices)
        {
            int count = 0;
            itemIds = new int[uimanager.NoOfTradeSlots];
            itemdIndices = new int[uimanager.NoOfTradeSlots];

            for (int i = 0; i < uimanager.NoOfTradeSlots; i++)
            {
                var item = uimanager.GetPlayerTradeSlot(i);
                if (item != -1)
                {
                    var obj = UWTileMap.current_tilemap.LevelObjects[item];
                    itemIds[count] = obj.item_id;
                    itemdIndices[count] = item;
                    count++;
                }
            }
            return count;
        }


        /// <summary>
        /// Gives the item at the specified index from the trade area to the npc.
        /// </summary>
        /// <param name="talker"></param>
        /// <param name="itemindex"></param>
        public static void GiveItemIndexToNPC(uwObject talker, int itemindex)
        {
            for (int i = 0; i < uimanager.NoOfTradeSlots; i++)
            {
                var x = uimanager.GetPlayerTradeSlot(i, false);
                if (x != -1)
                {
                    var obj = UWTileMap.current_tilemap.LevelObjects[x];
                    if (obj.index==itemindex)
                    {
                        //insert to talkers object list
                        obj.next = talker.link;
                        talker.link = obj.index;
                        uimanager.SetPlayerTradeSlot(i, -1, false);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Completes the trade between player and NPC and moves objects between their inventory
        /// </summary>
        /// <param name="talker"></param>
        private static void SwapTradedObjects(uwObject talker)
        {
            //transfer trade item
            //player(world) to Npc
            for (int i = 0; i < uimanager.NoOfTradeSlots; i++)
            {
                var objindex = uimanager.GetPlayerTradeSlot(i);
                if (objindex != -1)
                {
                    var tradedobject = UWTileMap.current_tilemap.LevelObjects[objindex];
                    //insert at head of npcs object list
                    tradedobject.next = talker.link;
                    talker.link = tradedobject.index;
                    uimanager.SetPlayerTradeSlot(i, -1, false);
                }
            }

            //npc to player. use free backpack slots or else drop at player position
            //Some deliberate jank here. close opened containers
            if (uimanager.OpenedContainerIndex != -1)
            {//keep closing until we reach the top
                while (uimanager.OpenedContainerIndex != -1)
                {
                    container.Close(uimanager.OpenedContainerIndex, playerdat.InventoryObjects);
                }
            }
            
            for (int i = 0; i < uimanager.NoOfTradeSlots; i++)
            {//remove from the NPCs chain first
                var objindex = uimanager.GetNPCTradeSlot(i);
                if (objindex != -1)
                {
                    ObjectRemover.RemoveObjectFromLinkedList(talker.link, objindex, UWTileMap.current_tilemap.LevelObjects, talker.PTR+6);
                }
            }

            for (int i = 0; i < uimanager.NoOfTradeSlots; i++)
            {
                var objindex = uimanager.GetNPCTradeSlot(i);
                if (objindex != -1)
                {
                    var tradedobject = UWTileMap.current_tilemap.LevelObjects[objindex];

                    uimanager.CurrentSlot = uimanager.FreeGeneralUseSlot;
                    if (uimanager.CurrentSlot != -1)
                    {
                        playerdat.ObjectInHand = tradedobject.index;
                        uimanager.PickupToEmptySlot(tradedobject.index);
                        uimanager.CurrentSlot = -1;
                    }
                    else
                    {
                        var tile = UWTileMap.current_tilemap.Tiles[playerdat.tileX,playerdat.tileY];
					    UWTileMap.GetRandomXYZForTile(tile, out int newxpos, out int newypos, out int newzpos);
					    var dropcoordinate = uwObject.GetCoordinate(playerdat.tileX, playerdat.tileY, newxpos, newypos, newzpos);

                        //drop at players location
                        pickup.Drop(
                            index: objindex,
                            objList: UWTileMap.current_tilemap.LevelObjects,
                            dropPosition: dropcoordinate,
                            tileX: playerdat.tileX,
                            tileY: playerdat.tileY);
                    }
                    uimanager.SetNPCTradeSlot(i, -1, false);
                }
            }
            uimanager.UpdateInventoryDisplay();
        }

    } //end class
}//end namespace