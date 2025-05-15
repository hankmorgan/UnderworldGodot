
using System;

namespace Underworld
{
    /// <summary>
    /// Trap which rotates the worlds the player will access when colliding with the blackrock gem
    /// </summary>
    public class a_hack_trap_gemteleport : hack_trap
    {

        static int[] dungeonNos = new int[8] { 9, 17, 25, 33, 41, 57, 49, 69 };
        static int[] arriveX = new int[8] { 33, 27, 18, 31, 4, 59, 32, 32 };
        static int[] arriveY = new int[8] { 32, 34, 39, 31, 38, 20, 32, 32 };
        static int[] heading = new int[8] { 56, 32, 48, 1, 48, 32, 48, 35 };

        public static int Activate()
        {
            var var4X = (playerdat.tileX << 3) + playerdat.playerObject.xpos - 227;
            var cxY = (playerdat.tileY << 3) + playerdat.playerObject.ypos - 323;
            int si_world = -1;

            int di_var2;
            if (Math.Abs(var4X) >= Math.Abs(cxY))
            {
                di_var2 = 0;
            }
            else
            {
                di_var2 = 1;
            }
            if (cxY <= 0)
            {
                di_var2 += 2;
            }
            else
            {
                di_var2 = 1 - di_var2;
            }

            if (var4X < 0)
            {
                di_var2 = 7 - di_var2;
            }



            var tmp = (1 << di_var2) & playerdat.GetQuest(130);
            if (
                (tmp != 0)
                ||
                ((tmp == 0) && ((playerdat.GetGameVariable(6) & 0x7) == di_var2))
                )
            {
                si_world = di_var2;
            }

            if (si_world == -1)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x15C));//you are bounced back
                return 4;
            }
            else
            {
                var4X = si_world;
                switch (si_world)
                {//swap 5 and 6
                    case 5:
                        var4X = 6;
                        break;
                    case 6:
                        var4X = 5;
                        break;
                }

                var visited = playerdat.GetQuest(141); //bitfield of worlds visited
                visited = visited | (1 << var4X);
                playerdat.SetQuest(141, visited);

                Teleportation.Teleport(
                    character: 0,
                    tileX: arriveX[si_world],
                    tileY: arriveY[si_world],
                    newLevel: dungeonNos[si_world],
                    heading: heading[si_world]);
                // Teleportation.TeleportRotation = heading[si_world];                
                // Teleportation.TeleportLevel = dungeonNos[si_world];
                // Teleportation.TeleportTileX = arriveX[si_world];
                // Teleportation.TeleportTileY = arriveY[si_world];
                // Teleportation.JustTeleported = true;
                return 2;

            }
        }
    }//end class
}//end namespace