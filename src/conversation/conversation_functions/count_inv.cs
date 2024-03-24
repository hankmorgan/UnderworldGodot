namespace Underworld
{

    public partial class ConversationVM : UWClass
    {   
        public static void count_inv()
        {
            var itemindex = at(at(stackptr-1));

            var obj = UWTileMap.current_tilemap.LevelObjects[itemindex];
            if (obj!=null)
            {
                result_register = obj.ObjectQuantity;
            }
            else
            {
                result_register = 0;
            }
        }
    }//end class
}//end namespace