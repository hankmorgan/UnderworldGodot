using System;
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
                    Lev_Ark_File = System.IO.Path.Combine(BasePath, folder, "lev.ark");  //  Lev_Ark_File_Selected; //"DATA\\lev.ark";//Eventually this will be a save game.
                    break;
            }
            var toLoad = Path.Combine(Loader.BasePath, Lev_Ark_File);
            Debug.Print($"Loading {toLoad}");
            if (!Loader.ReadStreamFile(toLoad, out lev_ark_file_data))
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
                    Loader.ReadStreamFile(Path.Combine(Loader.BasePath, "DATA", "LEVEL13.TXM"), out tex_ark_block.Data);
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
                    Loader.ReadStreamFile(Path.Combine(Loader.BasePath, "DATA", "LEVEL13.ANX"), out ovl_ark_block.Data);
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