using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void teleport_talker(uwObject talker)
        {
            TeleportTileY = GetConvoStackValueAtPtr(stackptr-1);
            TeleportTileX = GetConvoStackValueAtPtr(stackptr-2);
            Debug.Print($"Teleport Talker to {TeleportTileX},{TeleportTileY}");          
            npc.moveNPCToTile(talker, TeleportTileX, TeleportTileY);
        }
    }//end class
}//end namespace