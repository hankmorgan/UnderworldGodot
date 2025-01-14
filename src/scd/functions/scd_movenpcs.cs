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
            Debug.Print($"Try and move {obj.a_name} to {paramsarray[5]} {paramsarray[6]}");

            if (paramsarray[0xA] == 0)
            {
                //Do Check if in front of player
                if (uwObject.CheckIfInFrontOfPlayer(obj))
                {//don't move if player can see it happening.
                    if (paramsarray[0xC]>0)
                    {//This probably allows for retries by copying the event to a new row in the block
                        Debug.Print ("UNIMPLEMENTED COPY EVENT ROW");
                        return;
                    }
                }    
            }

            if (npc.moveNPCToTile(obj, paramsarray[5], paramsarray[6]))
            {
                if (paramsarray[0xB] !=0)
                {
                    obj.quality = (short)paramsarray[5];
                    obj.owner = (short)paramsarray[6];
                }
            }
            else
            {//unable to move
                if (paramsarray[0xC]>0)
                {
                    Debug.Print ("UNIMPLEMENTED COPY EVENT ROW");
                    return;
                }
            }
        }
    }//end class
}//end namespace