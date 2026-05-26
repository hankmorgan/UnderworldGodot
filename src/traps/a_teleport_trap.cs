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

            int NewHeadingHeightTeleportFlag = 0;
            if (_RES == GAME_UW2)
            {
                NewHeadingHeightTeleportFlag = trapObj.xpos & 0x1;
                if (((trapObj.xpos & 0x2)>>1) == 1)
                {
                    NewHeadingHeightTeleportFlag = NewHeadingHeightTeleportFlag | ((trapObj.heading + 8)<<2);
                }
            }
                Teleportation.Teleport(
                    character: 0, 
                    tileX: trapObj.quality, 
                    tileY: trapObj.owner, 
                    newLevel: trapObj.zpos, 
                    HeadingHeightFlag: NewHeadingHeightTeleportFlag);            

            uimanager.FlashColour(1, uimanager.Cuts3DWin, 0.1f);
            return trapObj.link;
        }

    }//end class
}//end namespace