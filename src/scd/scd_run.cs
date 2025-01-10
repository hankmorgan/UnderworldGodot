using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// What the hell is SCD.ARK? It's an event system. Data is read from scd.ark at the end of conversations, level transitions, by a hack trap, when a block of time is added to the clock and when Killorn is crashing.
    /// </summary>
    public partial class scd:UWClass
    {
        public static int RunSCDFunction(byte[] currentblock, int eventOffset)
        {
            Debug.Print($"Running SCD function {currentblock[eventOffset + 4]}");
            switch (currentblock[eventOffset + 4])
            {
                case 10:// run a block of events.
                    {
                        return RunBlock(
                            currentblock: currentblock,
                            eventOffset: eventOffset);
                    }
            }
            return 0;
        }

    }//end class
}//end namespace