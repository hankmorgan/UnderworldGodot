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

            var y = startY;
            var x = startX;

        ovr113_747:
            if (endY >= y)
            {
                x = startX;

            ovr113_73d:

                if (endX >= x)
                {
                    //ovr113_726
                    if (npc.moveNPCToTile(obj, x, y))
                    {
                        //ovr113_746
                        y++;
                        goto ovr113_747;
                    }
                    else
                    {                        
                        x++;
                        goto ovr113_73d;
                    }
                }
                else
                {
                    goto ovr113_747;
                }
            }
            else
            {
                //ovr113_752
                obj.quality = (short)x;
                obj.owner = (short)y;
            }
        }
    }//end class
}//end namesace