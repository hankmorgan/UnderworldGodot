namespace Underworld
{

    /// <summary>
    /// Trap that moves player to another tile or to another level
    /// </summary>
    public class a_teleport_trap : trap
    {

        public static int Activate(uwObject trapObj, uwObject[] objList)
        {
            if (trigger.JustTeleported)
            {
                trigger.JustTeleported = false;
                return 0;
            }

            if (trapObj.zpos == 0)
            {
                trigger.TeleportLevel = -1;
                trigger.TeleportTileX = trapObj.quality;
                trigger.TeleportTileY = trapObj.owner;
                uimanager.FlashColour(1, uimanager.Cuts3DWin, 0.1f);
            }
            else
            {
                trigger.TeleportLevel = trapObj.zpos;
                trigger.TeleportTileX = trapObj.quality;
                trigger.TeleportTileY = trapObj.owner;
            }

            //main.gamecam.Position = uwObject.GetCoordinate(tileX, tileY, xpos, ypos, camerazpos);
            return trapObj.link;
        }

    }//end class
}//end namespace