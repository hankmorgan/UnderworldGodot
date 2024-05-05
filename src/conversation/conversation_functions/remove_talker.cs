
namespace Underworld
{

    public partial class ConversationVM : UWClass
    {
        public static void remove_talker(uwObject talker)
        {
            var NPC = (npc)talker.instance;
            //ObjectCreator.npcs.Remove(NPC);
            
            ObjectCreator.DeleteObjectFromTile(                
                tileX: (short)talker.npc_xhome,
                tileY: (short)talker.npc_yhome,
                indexToDelete: talker.index);
        }
    }//end class
}//end namespace