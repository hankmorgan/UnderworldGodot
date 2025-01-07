namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void set_inv_quality()
        {
            var index = GetConvoStackValueAtPtr(stackptr - 2);
            var newQuality = GetConvoStackValueAtPtr(stackptr - 1);
            var obj = UWTileMap.current_tilemap.LevelObjects[index];
            obj.quality = (short)newQuality;
        }
    }//end class
}//end namespace