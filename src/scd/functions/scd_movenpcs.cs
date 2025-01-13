using System.Diagnostics;

namespace Underworld
{
    public partial class scd : UWClass
    {
        static int MoveNPCs(byte[] currentblock, int eventOffset)
        {
            //homex = row[5], homey = row[6]
            Debug.Print("Move NPCS in SCD");
            PrintRow(
                currentblock: currentblock,
                eventOffset: eventOffset);
            RunCodeOnObjects_SCD(
                methodToCall: MoveNPC_WithParams,
                mode: currentblock[eventOffset + 7],
                filter: currentblock[eventOffset + 8],
                loopAll: true,
                currentblock: currentblock,
                eventOffset: eventOffset);
            return 0;
        }

        static void MoveNPC_WithParams(uwObject obj, int[] paramsarray)
        {
            Debug.Print("THERE is a lot more to this function but this is enough to simulate Garg behaviour for now");
            obj.npc_xhome = (short)paramsarray[5];
            obj.npc_yhome = (short)paramsarray[6];
            npc.moveNPCToTile(obj, paramsarray[5], paramsarray[6]);
        }
    }//end class
}//end namespace