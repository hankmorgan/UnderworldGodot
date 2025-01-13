using System.Diagnostics;
using System.IO;

namespace Underworld
{
    /// <summary>
    /// Utility functions for SCD.ARK
    /// </summary>
    public partial class scd:UWClass
    {
        public static UWBlock[] scd_data;//15 blocks of scd data

        /// <summary>
        /// Set to true when the ark file has been modified (eg row removal) and has to be resaved to the SAVEx game folder
        /// </summary>
        static bool ArkHasBeenModified = false;

        /// <summary>
        /// Loads the specified SCD.ARK block no into scd_data.
        /// </summary>
        /// <param name="blockno"></param>
        /// <returns>True if file exists and read.</returns>
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

        /// <summary>
        /// Deletes an event row and moves following data to overwrite.
        /// </summary>
        /// <param name="currentblock"></param>
        /// <param name="eventOffset"></param>
        static void DeleteRow(byte[] currentblock, int eventOffset)
        {
            var TotalnoOfEvents = currentblock[0];
            var di_eventNo = (eventOffset - 324)/16;
            var EOF = (TotalnoOfEvents - di_eventNo - 1) * 16;

            System.Buffer.BlockCopy(currentblock, eventOffset + 16, currentblock, eventOffset, EOF);

            for (int si = 0; si<80;si++)
            {
                if (currentblock[6 + si*4] > di_eventNo)
                {
                    currentblock[6 + si*4]--;
                }
            }   
            currentblock[0]--;
        }
    }//end class
}//end namespace