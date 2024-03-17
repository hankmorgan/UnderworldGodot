using System.Diagnostics;

namespace Underworld
{
    public class automap : Loader
    {

        public static int currentautomap;
        public static int currentworld = 0;

        /// <summary>
        /// Array of all cached automaps
        /// </summary>
        public static automap[] automaps;


        //The raw data for this automap.
        public byte[] buffer;

        public automaptileinfo[,] tiles = new automaptileinfo[64, 64];


        /// <summary>
        /// Initialises an automap for the specified level no and loads the automap data from the lev.ark file
        /// </summary>
        /// <param name="LevelNo"></param>
        public automap(int LevelNo, int gameNo)
        {
            //load buffer. then init tiles with their offsets
            int blockno;
            if (gameNo == UWClass.GAME_UW2) //this is weird. I had to pass gameno as a parm or otherwise this if-else would not work??
            {
                Debug.Print("UW2");
                blockno = 160 + LevelNo;
            }
            else
            {
                blockno = LevelNo + 27;
            }
            // switch(_RES)
            // {
            //     case GAME_UW2:
            //         blockno = 160 + LevelNo * 4;
            //         break;
            //     default:
            //         blockno = LevelNo + 27;
            //         break;
            // }
            DataLoader.LoadUWBlock(LevArkLoader.lev_ark_file_data, blockno, 64 * 64, out UWBlock block);
            if (block.Data == null)
            { //init a blank map
                buffer = new byte[64 * 64];
            }
            else
            {
                buffer = block.Data;
            }

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    tiles[x, y] = new automaptileinfo((y * 64) + x, ref buffer);
                }
            }
        }
    }//end class
}//end namespace
