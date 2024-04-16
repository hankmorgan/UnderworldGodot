namespace Underworld
{

    /// <summary>
    /// Trap that moves player to another tile or to another level
    /// </summary>
    public class a_teleport_trap : trap
    {

        public static int Activate(uwObject trapObj, uwObject[] objList)
        {
            if (main.JustTeleported)
            {
                main.JustTeleported = false;
                return 0;
            }

            if (trapObj.zpos == 0)
            {
                main.TeleportLevel = -1;
                main.TeleportTileX = trapObj.quality;
                main.TeleportTileY = trapObj.owner;               
            }
            else
            {
                main.TeleportLevel = trapObj.zpos;
                main.TeleportTileX = trapObj.quality;
                main.TeleportTileY = trapObj.owner;
            }
            uimanager.FlashColour(1, uimanager.Cuts3DWin, 0.1f);
            //main.gamecam.Position = uwObject.GetCoordinate(tileX, tileY, xpos, ypos, camerazpos);
            return trapObj.link;
        }

    }//end class
}//end namespace