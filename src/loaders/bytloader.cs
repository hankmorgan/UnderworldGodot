using System;
using System.IO;
using System.Diagnostics;

namespace Underworld
{    
    public class BytLoader : ArtLoader
    {
        public const int BLNKMAP_BYT = 0;
        public const int CHARGEN_BYT = 1;
        public const int CONV_BYT = 2;
        public const int MAIN_BYT = 3;
        public const int OPSCR_BYT = 4;
        public const int PRES1_BYT = 5;
        public const int PRES2_BYT = 6;
        public const int WIN1_BYT = 7;
        public const int WIN2_BYT = 8;
        public const int PRESD_BYT = 9;

        //UW2 bitmap indices
        public const int UW2MAIN_BYT = 5;

        private readonly int currentIndex = -1;

        private readonly string[] FilePaths ={
                "BLNKMAP.BYT",
                "CHARGEN.BYT",
                "CONV.BYT",
                "MAIN.BYT",
                "OPSCR.BYT",
                "PRES1.BYT",
                "PRES2.BYT",
                "WIN1.BYT",
                "WIN2.BYT",
                "PRESD.BYT"
            };

        private readonly int[] PaletteIndices =
        {
                3,
                9,
                0,
                0,
                6,
                15,
                15,
                21,
                21,
                0
            };


        private readonly int[] PaletteIndicesUW2 =
        {
                3,
                0,
                0,
                0,
                0,
                0,
                15,
                15,
                0,
                0,
                0
            };




        /// <summary>
        /// Loads the texture form a byt file
        /// </summary>
        /// <returns>The <see cref="UnityEngine.Texture2D"/>.</returns>
        /// <param name="index">Index.</param>
        /// In this case the index is a loading of the seperate file. 
        public override Godot.Image LoadImageAt(int index)
        {
            return LoadImageAt(index, false);
        }

        public override Godot.Image LoadImageAt(int index, bool Alpha)
        {
            switch (_RES)
            {
                case GAME_UW2:
                    {
                        return null;
                        //return extractUW2Bitmap(Path.Combine(BasePath, "DATA", "BYT.ARK"), index, Alpha);      //    "DATA" + sep + "BYT.ARK", index, Alpha);
                    }
                default:
                    {
                        var toLoad = Path.Combine(BasePath, "DATA", FilePaths[index]);
                        var toLoadMod = Path.Combine(toLoad, "001.tga");
                        if (File.Exists(toLoadMod))
                        {
                            //return TGALoader.LoadTGA(toLoadMod);
                        }
                        if (currentIndex != index)
                        {//Only load from disk if the image to bring back has changed.
                            DataLoaded = false;
                            filePath = toLoad;   //FilePaths[index];
                            LoadImageFile();
                        }
                        //return Image(ImageFileData, 0, 320, 200, "name_goes_here", GameWorldController.instance.palLoader.Palettes[PaletteIndices[index]], Alpha);

                        return Image(ImageFileData, 0, 320, 200, "name_goes_here",  PaletteLoader.Palettes[PaletteIndices[index]], Alpha);
                    }
            }
        }
    }

}//namespace