using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void teleport_talker(uwObject talker)
        {
            TeleportTileY = GetConvoStackValueAtPtr(stack + stackptr-1);
            TeleportTileX = GetConvoStackValueAtPtr(stack + stackptr-2);
            Debug.Print($"Teleport Talker to {TeleportTileX},{TeleportTileY}");          
            npc.moveNPCToTile(talker, TeleportTileX, TeleportTileY);
        }
    }//end class
}//end namespace