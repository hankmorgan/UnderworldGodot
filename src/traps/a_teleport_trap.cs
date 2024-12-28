namespace Underworld
{

    /// <summary>
    /// Trap that moves player to another tile or to another level
    /// </summary>
    public class a_teleport_trap : trap
    {

        public static int Activate(uwObject trapObj, uwObject[] objList)
        {
            if (Teleportation.JustTeleported)
            {
                Teleportation.JustTeleported = false;
                return 0;
            }

            // if (trapObj.zpos == 0)
            // {
            //     // Teleportation.TeleportLevel = -1;
            //     // Teleportation.TeleportTileX = trapObj.quality;
            //     // Teleportation.TeleportTileY = trapObj.owner;   

                Teleportation.Teleport(
                    character: 0, 
                    tileX: trapObj.quality, 
                    tileY: trapObj.owner, 
                    newLevel: trapObj.zpos, 
                    heading: 0);            
            // }
            // else
            // {
            //     Teleportation.TeleportLevel = trapObj.zpos;
            //     Teleportation.TeleportTileX = trapObj.quality;
            //     Teleportation.TeleportTileY = trapObj.owner;
            // }
            //TODO: include heading after teleport
            //TODO: use proper teleport function.
            uimanager.FlashColour(1, uimanager.Cuts3DWin, 0.1f);
            //Teleportation.gamecam.Position = uwObject.GetCoordinate(tileX, tileY, xpos, ypos, camerazpos);
            return trapObj.link;
        }

    }//end class
}//end namespace