using System.ComponentModel;
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// What the hell is SCD.ARK? It's an event system. Data is read from scd.ark at the end of conversations, level transitions, by a hack trap, when a block of time is added to the clock and when Killorn is crashing.
    /// </summary>
    public partial class scd : UWClass
    {
        public static int RunSCDFunction(byte[] currentblock, int eventOffset)
        {
            Debug.Print($"Running SCD function {currentblock[eventOffset + 4]} at {eventOffset}");
            switch (currentblock[eventOffset + 4])
            {
                case 0:
                    return 0;//does nothing
                case 1:
                    //Set goal and gtarg
                    return 0;
                case 2://move npcs
                    {
                        return MoveNPCs(
                            currentblock: currentblock,
                            eventOffset: eventOffset);
                    }
                case 3://Kill NPCs
                    {
                        return 0;
                    }
                case 4: // Change quest
                    {
                        return ChangeQuest(
                             currentblock: currentblock,
                             eventOffset: eventOffset);
                    }
                case 5: //run trigger
                    {
                        return RunTrigger(
                             currentblock: currentblock,
                             eventOffset: eventOffset);
                    }
                case 6:// does nothing
                    return 0;
                case 7:// Run extended commands
                   return RunSCDFunction_extended(
                            currentblock: currentblock,
                            eventOffset: eventOffset);//runs extra commands defined by eventrow[5]
                case 8://set attitude
                    return 0;
                case 9://perform variable operation
                    return 0;
                case 10:// run a block of events.
                    {
                        return RunBlock(
                            currentblock: currentblock,
                            eventOffset: eventOffset);
                    }
                case 11://remove object from tile
                    return 0;
            }
            return 0;
        }


    /// <summary>
    /// Additional functions that are called when eventrow[4] == 7. Function defined by eventrow[5]
    /// </summary>
    /// <param name="currentblock"></param>
    /// <param name="eventOffset"></param>
    /// <returns></returns>
    static int RunSCDFunction_extended(byte[] currentblock, int eventOffset)
    {
        Debug.Print($"Running Extended SCD function {currentblock[eventOffset + 5]} at {eventOffset}");
        switch (currentblock[eventOffset + 5])
        {
            case 0:
                return 0;//does nothing
            case 1:
                return 0;//variable operation involving NPCs
            case 2://Maybe move an NPC
                return 0;
            case 3: //Change a tile
                return 0;
            case 4://maybe close doors
                return 0;
            case 5:// does nothing
                return 0;
            case 6: // Hp Change on npc
                return 0;
            case 7://maybe move npcs
                return 0;    
        }
        return 0;
    }

    }//end class
}//end namespace