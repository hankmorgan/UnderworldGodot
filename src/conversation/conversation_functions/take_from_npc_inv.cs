namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        /// <summary>
        /// Gets the nth item in the npc inventory
        /// </summary>
        public static void take_from_npc_inv(uwObject talker)
        {
            var di = GetConvoStackValueAtPtr(stackptr - 1);
            var si = 0;
            var next = talker.link;
            while (si<di)
            {
                if (next!=0)
                {
                    var nextobj = UWTileMap.current_tilemap.LevelObjects[next];
                    next = nextobj.next;
                }
                si++;
            }
            result_register = next;
        }
    }//end class
}//end namespace