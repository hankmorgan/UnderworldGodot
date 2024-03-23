using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Trap that moves player to another tile or to another level
    /// </summary>
    public class a_teleport_trap : trap
    {
        public static int activate(uwObject trapObj, uwObject[] objList)
        {
            if (trapObj.zpos == 0)
            {
                Debug.Print ($"Teleport within this level {trapObj.quality},{trapObj.owner}");
                var targetTile = UWTileMap.current_tilemap.Tiles[trapObj.quality, trapObj.owner];
                playerdat.zpos  = targetTile.floorHeight<<2;
                playerdat.xpos= 3; playerdat.ypos= 3;
                playerdat.tileX = trapObj.quality; playerdat.tileY = trapObj.owner;
                uimanager.FlashColour(1, uimanager.Cuts3DWin, 0.1f);            
                main.gamecam.Position = uwObject.GetCoordinate(playerdat.tileX, playerdat.tileY, playerdat.xpos, playerdat.ypos, playerdat.camerazpos);
            }
            else
            {
                Debug.Print ($"Teleport to dungeon {trapObj.zpos}  {trapObj.quality},{trapObj.owner}");
            }

            //main.gamecam.Position = uwObject.GetCoordinate(tileX, tileY, xpos, ypos, camerazpos);
            return 0;
        }
    }//end class
}//end namespace