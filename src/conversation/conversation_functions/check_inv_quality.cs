namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        static void check_inv_quality()
        {
            var index= at(at(stack + stackptr-1));
            if (index!=0)
            {
                var obj = UWTileMap.current_tilemap.LevelObjects[index];
                if (obj!=null)
                {
                    result_register = obj.quality;
                    return;
                }
            }
            result_register = 0;
        }
    }//end class
}//end namespace