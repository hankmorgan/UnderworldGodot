using System.Diagnostics;

namespace Underworld
{

    /// <summary>
    /// For repacking of UW2 ark files
    /// </summary>
    public class Repacker : Loader
    {

        static byte[] CompressedData;


        /// <summary>
        /// Launch repack and test if it matches the original compressed data.
        /// </summary>
        /// <param name="blockNoToCompress"></param>
        public static void TestRepack(int blockNoToCompress)
        {
            LevArkLoader.LoadLevArkFileData();
            bool Pass = true;
            var lev_ark_block = LevArkLoader.LoadLevArkBlock(blockNoToCompress);
            if (((lev_ark_block.CompressionFlag >> 1) & 0x01) == 1)
            {
                var repacked = CompressData(lev_ark_block);
                
                for (int i = 0; i <= lev_ark_block.Data.GetUpperBound(0); i++)
                {
                
                    if (i <= repacked.Data.GetUpperBound(0))
                    {
                        if (repacked.Data[i] != lev_ark_block.Data[i])
                        {
                            Debug.Print($"Data mismatch at offset {i}");
                            Pass = false;
                            break;
                        }
                    }
                    else
                    {
                        Debug.Print("Out of bounds error on repacked block.");                        
                        Pass = false;
                        break;
                    }
                }
            }
            else
            {
                Debug.Print("Test file is not a compressed Ark file");
                Pass = false;
            }
            if (Pass)
            {
                Debug.Print("Repack has suceeded. What a day!");
            }
            else
            {
                Debug.Print("Repack has failed. Try try agin.!");
            }
        }


        static UWBlock CompressData(UWBlock InputArk, int DataSize = 0x7E08, int arkType = 1)
        {
            CompressedData = new byte[0x4000]; //to confirm size requirements. possibly should be  0x722f (29231d). Ref GetOffsetsForCompression_ovr153_
            //Do stuff with the data to repack it.
            var Packed =  new UWBlock();
            Packed.Data = new byte[5];


            //implementation
            //set some values first in CompressedData.
            //ovr_127_7C3

            //ovr_127_82F

            //ovr_127_84B

            //ovr_127_874


            //Main loop ovr_127+994
            //every 18 bytes until no more bytes. 
            //call ovr_127_0 depending on counter value




            return Packed;
        }

        static  void DataCompressionSubfunction_127_23C(short arg0)
        {
            
        }

        static void DataCompressionSubfunction_127_0(short arg0)
        {
            
        }
    }
}