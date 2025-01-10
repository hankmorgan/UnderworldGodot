using System.Diagnostics;
using System.IO;

namespace Underworld
{
    /// <summary>
    /// What the hell is SCD.ARK? It's an event system. Data is read from scd.ark at the end of conversations, level transitions, by a hack trap, when a block of time is added to the clock and when Killorn is crashing.
    /// </summary>
    public partial class scd:UWClass
    {
        public static UWBlock[] scd_data;//15 blocks of scd data
        static bool LoadSCDBlock(int blockno)
        {
            var path = Path.Combine(BasePath, playerdat.currentfolder, "SCD.ARK");
            if (File.Exists(path))
            {
                var data = File.ReadAllBytes(path);
                DataLoader.LoadUWBlock(
                    arkData: data, 
                    blockNo: blockno, 
                    targetDataLen: 0, 
                    uwb: out scd_data[blockno] );
                return true;
            }
            return false;
        }

        /// <summary>
        /// Just prints out a row of event values in SCD.ARK
        /// </summary>
        /// <param name="currentblock"></param>
        /// <param name="eventOffset"></param>
        static void PrintRow(byte[] currentblock, int eventOffset)
        {
            Debug.Print($"Event Offset {eventOffset}");
            for (int i=0; i<16;i++)
            {
                Debug.Print($"{i}:{currentblock[eventOffset+i]}");
            }
        }
    }//end class
}//end namespace