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
    }//end class
}//end namespace