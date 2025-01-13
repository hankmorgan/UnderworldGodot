using System.Diagnostics;

namespace Underworld
{
    public partial class scd : UWClass
    {
        /// <summary>
        /// A bit of a mouthfull but this checks an NPC's xy home againsts the params and if matching will update a quest variable. Used with the Troll in the prison tower
        /// </summary>
        /// <param name="currentblock"></param>
        /// <param name="eventOffset"></param>
        public static void changevariable_by_npcXYHome(byte[] currentblock, int eventOffset)
        {
            PrintRow(currentblock, eventOffset);
            RunCodeOnObjects_SCD(
                methodToCall: VariableOperationForXYHome,
                mode: currentblock[eventOffset + 6],
                filter: currentblock[eventOffset + 7],
                loopAll: true,
                currentblock: currentblock,
                eventOffset: eventOffset);
        }


        /// <summary>
        /// Checks if the target npc has an xy home that matches the params and if so sets a quest flag.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="paramsarray"></param>
        static void VariableOperationForXYHome(uwObject obj, int[] paramsarray)
        {
            if ((paramsarray[8] == obj.npc_xhome) && (paramsarray[9] == obj.npc_yhome))
            {
                var toChange = (paramsarray[11]<<8) | paramsarray[10];
                a_set_variable_trap.VariableOperationUW2(toChange, paramsarray[12], paramsarray[13]);
            }
        }
    }//end class
}//end namespace
