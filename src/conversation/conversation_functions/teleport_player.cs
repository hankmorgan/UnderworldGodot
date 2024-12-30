using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void teleport_player()
        {
            DoTeleport = true;
            TeleportToLevel = GetConvoStackValueAtPtr(stackptr-1);
            TeleportTileY = GetConvoStackValueAtPtr(stackptr-2);
            TeleportTileX = GetConvoStackValueAtPtr(stackptr-3);
            Debug.Print($"Teleport Player to {TeleportToLevel},{TeleportTileX},{TeleportTileY}");          
        }
    }//end class
}//end namespace