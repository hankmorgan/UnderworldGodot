namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void set_inv_quality()
        {
            var index = GetConvoStackValueAtPtr(stack + stackptr - 2);
            var newQuality = GetConvoStackValueAtPtr(stack + stackptr - 1);
            var obj = UWTileMap.current_tilemap.LevelObjects[index];
            obj.quality = (short)newQuality;
        }
    }//end class
}//end namespace