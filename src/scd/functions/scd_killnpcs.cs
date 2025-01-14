using System.Diagnostics;

namespace Underworld
{
    public partial class scd : UWClass
    {
        static int KillNPCs(byte[] currentblock, int eventOffset)
        {
            Debug.Print("Kill NPCS in SCD");
            PrintRow(
                currentblock: currentblock,
                eventOffset: eventOffset);
            RunCodeOnObjects_SCD(
                methodToCall: Kill,
                mode: currentblock[eventOffset + 5],
                filter: currentblock[eventOffset + 6],
                loopAll: true,
                currentblock: currentblock,
                eventOffset: eventOffset);
            return 0;
        }

        static void Kill(uwObject obj, int[] paramsarray)
        {
            if (paramsarray[7]==0)
            {
                if (obj == ConversationVM.currentTalker)
                {
                    if (paramsarray[8] >0)
                    {
                        //probably nelson?
                        if (scd_data[BlockIdentifier].Data[6] == 15)
                        {
                            if (paramsarray[3] == 0)
                            {
                                playerdat.IncrementXClock(15);       
                            }
                        }                        
                    }
                } 
                else
                {//when not the conversation npc
                   npc.KillCritter(obj); 
                }            
            }
            else
            {//when params[7] set
                npc.KillCritter(obj);
            }            
        }
    }//end class
}//end namespace