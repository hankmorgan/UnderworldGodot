using System.Diagnostics;

namespace Underworld
{
    public partial class scd : UWClass
    {
        /// <summary>
        /// Changes the goal and goal target for set of NPCs.
        /// </summary>
        /// <param name="currentblock"></param>
        /// <param name="eventOffset"></param>
        /// <returns></returns>
        static int SetGoalAndGTarg(byte[] currentblock, int eventOffset)
        {
            RunCodeOnObjects_SCD(
                methodToCall: SetGoalAndGTarg, 
                mode: currentblock[eventOffset + 5],
                filter: currentblock[eventOffset + 6], 
                loopAll: true, 
                currentblock: currentblock, 
                eventOffset: eventOffset);
            return 0;
        }


        static void SetGoalAndGTarg(uwObject obj, int[] paramsarray)
        {
            Debug.Print($"Change goal/gtarg for {obj.a_name} to {paramsarray[7]}/{paramsarray[8]}");
            obj.npc_goal = (byte)paramsarray[7];
            obj.npc_gtarg = (byte)paramsarray[8];
        }
    }//end class
}//end namesace