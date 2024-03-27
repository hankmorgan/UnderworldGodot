namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        /// <summary>
        /// returns the true value of an object to the npc and stores the identification string of the object.
        /// </summary>
        public static void identify_inv()
        {
            var index = at(at(stackptr-4));
            var lorecheck = at(at(stackptr-1));
            var value = GetTrueItemValue(
                    ApplyLikeDislike: true, 
                    appraise_accuracy: NPCAppraisalAccuracy, 
                    applyAccuracy: true, 
                    index: index);
            var obj = UWTileMap.current_tilemap.LevelObjects[index];
            var idString = look.GetDescriptionString(
                obj: obj, 
                objList: UWTileMap.current_tilemap.LevelObjects, 
                lorecheckresult: lorecheck, 
                IncludeYouSee: false);
            var stringIDNo = GameStrings.AddString(currentConversation.StringBlock, idString);

            Set(at(stackptr-2), stringIDNo );
            result_register = value;
        }
    }
}