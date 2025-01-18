namespace Underworld
{
    public partial class scd : UWClass
    {
        /// <summary>
        /// Changes the attitude for set of NPCs.
        /// </summary>
        /// <param name="currentblock"></param>
        /// <param name="eventOffset"></param>
        /// <returns></returns>
        static int SetAttitude(byte[] currentblock, int eventOffset)
        {
            RunCodeOnObjects_SCD(
                methodToCall: SetAttitude, 
                mode: currentblock[eventOffset + 5],
                filter: currentblock[eventOffset + 6], 
                loopAll: true, 
                currentblock: currentblock, 
                eventOffset: eventOffset);
            return 0;
        }


        static void SetAttitude(uwObject obj, int[] paramsarray)
        {
            obj.npc_attitude = (byte)paramsarray[7];
            if (obj.npc_attitude != 0)
            {
                obj.ProjectileSourceID = 0;//clear last character to hit the npc. so they won't go hostile again
            }
        }
    }//end class
}//end namesace