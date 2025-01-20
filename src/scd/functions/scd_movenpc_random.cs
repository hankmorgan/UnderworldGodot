using System.Diagnostics;

namespace Underworld
{
    public partial class scd : UWClass
    {
        /// <summary>
        /// Moves npc into the first random valid spot in a range of tiles. Example usage moving Nystrul to his quarters when he is waiting for the final answers
        /// </summary>
        /// <param name="currentblock"></param>
        /// <param name="eventOffset"></param>
        /// <returns></returns>
        static int movenpc_random(byte[] currentblock, int eventOffset)
        {
            Debug.Print("Move NPCS in random range. UNTESTED!");
            RunCodeOnObjects_SCD(
                methodToCall: movenpc_random, 
                mode: currentblock[eventOffset + 8],
                filter: currentblock[eventOffset + 9], 
                loopAll: true, 
                currentblock: currentblock, 
                eventOffset: eventOffset);
            return 0;
        }


        static void movenpc_random(uwObject obj, int[] paramsarray)
        {
            if ((paramsarray[6]!=0) || ((paramsarray[6]==0) && (paramsarray[7]!=0)))           
            {
                if (!((obj.npc_xhome == paramsarray[6]) && (obj.npc_yhome == paramsarray[7])))
                {   
                    Debug.Print("movenpc_random(). LOOK at me if you see this condition run. There is no known instances where I am applicable!");
                    return;
                }
            }

            var startX = paramsarray[10];
            var startY = paramsarray[11];
            var endX = paramsarray[12];
            var endY = paramsarray[13];

            var Maxretries = (1 + endX - startX) * (1 + endY - startY);

            while (Maxretries>0)
            {
                var x = startX + Rng.r.Next(endX - startX);
                var y = startY + Rng.r.Next(endY - startY);
                if (UWTileMap.ValidTile(x,y))
                {
                    if(npc.moveNPCToTile(obj, x,y))
                    {
                        obj.quality = (short)x;
                        obj.owner = (short)y;
                        return;
                    }
                }
                Maxretries--;
            }
        }
    }//end class
}//end namesace