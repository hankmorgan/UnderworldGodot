namespace Underworld
{
    //Common data and code for item trading.
    public partial class ConversationVM : UWClass
    {
        /// <summary>
		/// Pointer to Array in the stack that contains the items the npc likes
		/// </summary>
		public static int Likes = 0;

        /// <summary>
        /// Pointer to the array in the stack that contains the items the npc dislikes
        /// </summary>
        public static int Dislikes = 0;

        /// <summary>
        /// Get the total value of the players trade area (selected items)
        /// </summary>
        /// <param name="ApplyLikeDislike"></param>
        /// <param name="appraise_accuracy"></param>
        /// <returns></returns>
        static int GetNPCValue(bool ApplyLikeDislike=false, int appraise_accuracy = 0, bool applyAccuracy = false)
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
        static int GetPCValue(bool ApplyLikeDislike=false, int appraise_accuracy = 0, bool applyAccuracy = false)
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
            int itemvalue = 0;
            if (ApplyLikeDislike)
            {
                //return the value adjusted for the npc likes dislikes.
                if (Likes!=0)
                {//loop the stack starting at Likes until value 0xFF is hit.
                    //if value is >=1000 then liked is a class of object
                    //if value < 1000 then liked is an exact object match
                    //if liked value is 1.5 times
                    //itemvalue = (commonObjDat.monetaryvalue(obj.item_id) / 3) * 2;

                    //TODO item value has to take into account quality. (coins are always make quality.)
                }
                if (Dislikes!=0)
                {
                    //if unliked no value
                    //itemvalue = 0;
                }             
                
            }
            else
            {
                //return the true base value
                itemvalue = commonObjDat.monetaryvalue(obj.item_id);
            }

            //Adjust the value of the item based on the players appraisal accuracy score
            var offset = Rng.RandomOffset(itemvalue, -appraise_accuracy, +appraise_accuracy);
            return itemvalue;
        }


    } //end class
}//end namespace