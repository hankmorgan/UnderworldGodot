using System.Diagnostics;
using System.IO;

namespace Underworld
{
    /// <summary>
    /// Class for storing the current lev ark file data.
    /// </summary>
    public class LevArkLoader : Loader
    {
        /// <summary>
        /// The full file data
        /// </summary>
        public static byte[] lev_ark_file_data;

        /// <summary>
        /// How many bytes block <paramref name="blockNo"/> occupies in a UW1 LEV.ARK. Returns 0
        /// for a block the table records as absent, and for anything it cannot measure.
        ///
        /// UW1 stores an Int16 block count and then one Int32 offset per block, and nothing
        /// else. A block therefore runs from its own offset to wherever the next block starts,
        /// and the next block is whichever has the smallest offset larger than this one. It is
        /// not the next block by index, and it need not have a higher index at all, because DOS
        /// writes blocks in whatever order it likes. In the save reported on pull request #71
        /// the tail runs
        /// 27, 28, 36, 29, 30, 39, 31, 38, 37, 40, so block 36 is followed by block 29.
        ///
        /// Measuring against a subset of the blocks is what caused that issue. The writer
        /// searched forward by index and the note reader looked only at the nine note blocks,
        /// so both ran past their real neighbours: block 36 measured 15312 and 9056 bytes
        /// respectively where it holds 864. Notes are counted as length / 54, so the surplus
        /// was drawn as note records on the automap.
        ///
        /// <paramref name="headerBlocks"/> is supplied by the caller and bounds how much of the
        /// offset table is read. Callers take it from the count in the first two bytes of the
        /// file, which is why nothing here trusts it.
        ///
        /// UW2 is a different format with its own lengths in the header and does not use this.
        /// </summary>
        public static int UW1BlockLength(byte[] src, int blockNo, int headerBlocks)
        {
            if (src == null || blockNo < 0 || blockNo >= headerBlocks) { return 0; }

            // The count comes from the first two bytes of the file, so it is at most 0xFFFF in
            // practice. Bound it anyway rather than let 2 + headerBlocks * 4 overflow to a
            // small number and wave the size check through.
            if (headerBlocks > (int.MaxValue - 2) / 4) { return 0; }
            int headerSize = 2 + headerBlocks * 4;
            if (headerSize > src.Length) { return 0; }

            int thisOff = (int)getAt(src, 2 + blockNo * 4, 32);
            // Offset 0 records an absent block. Anything pointing into the header or past the
            // end of the file is a table we cannot trust, so measure nothing rather than guess.
            if (thisOff < headerSize || thisOff > src.Length) { return 0; }

            int nextOff = src.Length; // the last block by position runs to the end of the file
            for (int j = 0; j < headerBlocks; j++)
            {
                if (j == blockNo) { continue; }
                int candidate = (int)getAt(src, 2 + j * 4, 32);
                if (candidate > thisOff && candidate < nextOff && candidate <= src.Length)
                {
                    nextOff = candidate;
                }
            }
            return nextOff - thisOff;
        }

        public static bool LoadLevArkFileData(string Lev_Ark_File = "lev.ark", string folder = "DATA")
        {
            //Load up my tile maps
            //First read in my lev_ark file
            switch (_RES)
            {
                case GAME_UWDEMO:
                    Lev_Ark_File = Path.Combine(folder, "LEVEL13.ST");
                    break;
                case GAME_UW2:
                case GAME_UW1:
                default:
                    Lev_Ark_File = Path.Combine(BasePath, folder, "LEV.ARK");  //  Lev_Ark_File_Selected; //"DATA\\lev.ark";//Eventually this will be a save game.
                    break;
            }
            var toLoad = Path.Combine(BasePath, Lev_Ark_File);
            Debug.Print($"Loading {toLoad}");
            if (!ReadStreamFile(toLoad, out lev_ark_file_data))
            {
                Debug.Print(toLoad + "File not loaded");
                return false;
            }
            else
            {
               return true;
            }
        }


        public static UWBlock LoadLevArkBlock(int LevelBlockNo)
        {
            UWBlock lev_ark_block;
            if (_RES == GAME_UWDEMO)
            {//In UWDemo there is no block structure. Just copy the data directly from file.
                lev_ark_block = new UWBlock
                {
                    DataLen = 0x7c06,
                    Data = lev_ark_file_data
                };
            }
            else
            {
                int targetLen = 0x7c08;                
                //Load the tile and object blocks
                if (_RES==GAME_UW2)
                {
                    targetLen = 0x8000; //extra space needed for the animation overlay data.
                }
                DataLoader.LoadUWBlock(lev_ark_file_data, LevelBlockNo, targetLen, out lev_ark_block);
                //Trim to the correct size for lev ark blocks.
                //Array.Resize(ref lev_ark_block.Data, 0x7c08);
            }
            return lev_ark_block;
        }


        public static UWBlock LoadTexArkBlock(int LevelBlockNo)
        {
            //Load the texture maps
            switch (_RES)
            {
                case GAME_UWDEMO:
                    var tex_ark_block = new UWBlock();
                    ReadStreamFile(Path.Combine(BasePath, "DATA", "LEVEL13.TXM"), out tex_ark_block.Data);
                    tex_ark_block.DataLen = tex_ark_block.Data.GetUpperBound(0);
                    return tex_ark_block;
                case GAME_UW2:
                    DataLoader.LoadUWBlock(lev_ark_file_data, LevelBlockNo + 80, -1, out tex_ark_block);
                    return tex_ark_block;
                case GAME_UW1:
                default:
                    DataLoader.LoadUWBlock(lev_ark_file_data, LevelBlockNo + 18, 0x7a, out tex_ark_block);
                    return tex_ark_block;
            }
        }

        public static UWBlock LoadOverlayBlock(int LevelBlockNo)
        {
            //Load the texture maps
            switch (_RES)
            {
                case GAME_UWDEMO:
                    var ovl_ark_block =  new UWBlock();
                    ReadStreamFile(Path.Combine(BasePath, "DATA", "LEVEL13.ANX"), out ovl_ark_block.Data);
                    ovl_ark_block.DataLen = ovl_ark_block.Data.GetUpperBound(0);
                    return ovl_ark_block;
                case GAME_UW2:
                    //DataLoader.LoadUWBlock(lev_ark_file_data, LevelBlockNo , -1, out ovl_ark_block);//overlay data in uw2 is immediately after the tilemap
                    //ovl_ark_block = null; // UW2 does not have a seperate overlay block
                    return null;
                case GAME_UW1:
                default:
                    DataLoader.LoadUWBlock(lev_ark_file_data, LevelBlockNo + 9, 64*6, out ovl_ark_block);
                   return ovl_ark_block;
            }
        }

    }//end class

}//end namespace