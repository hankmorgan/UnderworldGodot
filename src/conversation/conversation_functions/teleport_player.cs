using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void teleport_player()
        {
            DoTeleport = true;
            TeleportToLevel = GetConvoStackValueAtPtr(stack + stackptr-1);
            TeleportTileY = GetConvoStackValueAtPtr(stack + stackptr-2);
            TeleportTileX = GetConvoStackValueAtPtr(stack + stackptr-3);
            Debug.Print($"Teleport Player to {TeleportToLevel},{TeleportTileX},{TeleportTileY}");          
        }
    }//end class
}//end namespace