using System.Diagnostics;
using System.Collections;
using Peaky.Coroutines;

namespace Underworld
{
    /// <summary>
    /// Trap that moves player to another tile or to another level
    /// </summary>
    public class a_teleport_trap : trap
    {
        /// <summary>
        /// To prevent teleporting again when the teleport destination in inside a teleport trap
        /// </summary>
        static bool JustTeleported;
        public static int Activate(uwObject trapObj, uwObject[] objList)
        {
            if (trapObj.zpos == 0)
            {
                if (!JustTeleported)
                {
                    uimanager.FlashColour(1, uimanager.Cuts3DWin, 0.1f);
                    Debug.Print($"Teleport within this level {trapObj.quality},{trapObj.owner}");
                    var targetTile = UWTileMap.current_tilemap.Tiles[trapObj.quality, trapObj.owner];
                    playerdat.zpos = targetTile.floorHeight << 2;
                    playerdat.xpos = 3; playerdat.ypos = 3;
                    playerdat.tileX = trapObj.quality; playerdat.tileY = trapObj.owner;
                    main.gamecam.Position = uwObject.GetCoordinate(playerdat.tileX, playerdat.tileY, playerdat.xpos, playerdat.ypos, playerdat.camerazpos);
                    JustTeleported = true;
                    _ = Peaky.Coroutines.Coroutine.Run(
                    PauseTeleport(),
                    main.instance
                    );
                }
                else
                {
                    JustTeleported = false;
                }
            }
            else
            {
                if (!JustTeleported)
                {
                    var targetX = trapObj.quality; var targetY = trapObj.owner;
                    Debug.Print($"Teleport to dungeon {trapObj.zpos}  {trapObj.quality},{trapObj.owner}");
                    playerdat.dungeon_level = trapObj.zpos;

                    UWTileMap.LoadTileMap(
                        newLevelNo: playerdat.dungeon_level - 1,
                        datafolder: playerdat.currentfolder,
                        newGameSession: false);

                    var targetTile = UWTileMap.current_tilemap.Tiles[targetX, targetY];
                    Debug.Print($"Player ZPOS {playerdat.zpos} -> {targetTile.floorHeight << 2}");
                    playerdat.zpos = targetTile.floorHeight << 2;
                    Debug.Print($"Player ZPOS is now {playerdat.zpos}");
                    playerdat.xpos = 3; playerdat.ypos = 3;
                    playerdat.tileX = targetX; playerdat.tileY = targetY;
                    main.gamecam.Position = uwObject.GetCoordinate(targetX, targetY, playerdat.xpos, playerdat.ypos, playerdat.camerazpos);
                    JustTeleported = true;
                    _ = Peaky.Coroutines.Coroutine.Run(
                    PauseTeleport(),
                    main.instance
                    );
                }
                else
                {
                    JustTeleported = false;
                }
            }

            //main.gamecam.Position = uwObject.GetCoordinate(tileX, tileY, xpos, ypos, camerazpos);
            return 0;
        }


        /// <summary>
        /// Puts a block on sucessive level transitions due to teleport placing player in a new move trigger
        /// </summary>
        /// <returns></returns>
        static IEnumerator PauseTeleport()
        {
            JustTeleported = true;
            yield return new WaitForSeconds(1);
            JustTeleported = false;
            yield return 0;
        }


    }//end class
}//end namespace