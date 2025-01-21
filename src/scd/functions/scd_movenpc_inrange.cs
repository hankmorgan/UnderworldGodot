using System.Diagnostics;

namespace Underworld
{
    public partial class scd : UWClass
    {
        /// <summary>
        /// Moves npc into the first valid spot in a range of tiles. Used when spawning Blog in the confrontation with Dorstag
        /// </summary>
        /// <param name="currentblock"></param>
        /// <param name="eventOffset"></param>
        /// <returns></returns>
        static int movenpc_inrange(byte[] currentblock, int eventOffset)
        {
            RunCodeOnObjects_SCD(
                methodToCall: movenpc_inrange, 
                mode: currentblock[eventOffset + 6],
                filter: currentblock[eventOffset + 7], 
                loopAll: true, 
                currentblock: currentblock, 
                eventOffset: eventOffset);
            return 0;
        }


        static void movenpc_inrange(uwObject obj, int[] paramsarray)
        {
            var startX = paramsarray[8];
            var startY = paramsarray[9];
            var endX = paramsarray[10];
            var endY = paramsarray[11];
            var finalX = obj.quality;
            var finalY = obj.owner;
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    //Debug.Print($"Move {obj.a_name} into {x},{y}");
                    if (npc.moveNPCToTile(obj, x, y))
                    {
                        y++;//this spot is okay but vanilla behaviour is to keep trying on the next row until all possibilities are done.
                        finalX = (short)x; 
                        finalY = (short)y;//correct to do this after y++ Blog will walk towards tile.                        
                    }
                }
            }
            obj.quality = (short)finalX;
            obj.owner = (short)finalY;
        }
    }//end class
}//end namesace