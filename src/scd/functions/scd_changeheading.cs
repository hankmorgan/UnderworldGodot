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
        static int ChangeHeading(byte[] currentblock, int eventOffset)
        {
            RunCodeOnObjects_SCD(
                methodToCall: ChangeObjectHeading, 
                mode: currentblock[eventOffset + 6],
                filter: currentblock[eventOffset + 7], 
                loopAll: true, 
                currentblock: currentblock, 
                eventOffset: eventOffset);
            return 0;
        }


        static void ChangeObjectHeading(uwObject obj, int[] paramsarray)
        {
            obj.ProjectileHeading = (short)(paramsarray[8]<<5);
            obj.heading = (short)paramsarray[8];
            obj.npc_heading = 0;
            objectInstance.Reposition(obj);
        }
    }//end class
}//end namesace