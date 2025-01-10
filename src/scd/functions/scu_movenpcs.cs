using System.Diagnostics;

namespace Underworld
{
    public partial class scd:UWClass
    {
        static int MoveNPCs(byte[] currentblock, int eventOffset)
        {
            Debug.Print ("Move NPCS in SCD");
            PrintRow(
                currentblock: currentblock, 
                eventOffset: eventOffset);
            return 0;
        }
    }//end class
}//end namespace