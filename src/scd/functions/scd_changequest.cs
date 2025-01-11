using System.Diagnostics;

namespace Underworld
{
    public partial class scd:UWClass
    {
        public static int ChangeQuest(byte[] currentblock, int eventOffset)
        {
            Debug.Print ("Change Quest in SCD");
            PrintRow(
                currentblock: currentblock, 
                eventOffset: eventOffset);

            return 0;
        }
    }//end class
}//end namespace