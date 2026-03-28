using Godot;
using Peaky.Coroutines;
using System.Collections;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Cutscenes")]
        //full screen cutscene output
        [Export] TextureRect CutsFull;
        //cutscene outputs that stretch from 0,0 to the bottom right corner of the 3d view.
        [Export] TextureRect CutsSmallUW1;
        [Export] TextureRect CutsSmallUW2;

        // cutscene outputs that just use the 3d window area
        [Export] TextureRect Cuts3DWinUW1;
        [Export] TextureRect Cuts3DWinUW2;

        [Export] public RichTextLabel CutsSubtitle;


        public static void InitCuts()
        {
            EnableDisable(CutsSmall, false);
            EnableDisable(CutsFullscreen, false);
        }


        public static TextureRect CutsFullscreen
        {
            get

            {
                return instance.CutsFull;
            }
        }

        /// <summary>
        /// Gets the smaller cuts window
        /// </summary>
        public static TextureRect CutsSmall
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return instance.CutsSmallUW2;
                }
                else
                {
                    return instance.CutsSmallUW1;
                }
            }
        }


        public static TextureRect Cuts3DWin
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return instance.Cuts3DWinUW2;
                }
                else
                {
                    return instance.Cuts3DWinUW1;
                }
            }
        }


        /// <summary>
        /// Loads a cuts file and display image at specified index on a target control
        /// </summary>
        /// <param name="cutsfile"></param>
        /// <param name="imageNo"></param>
        /// <param name="targetControl"></param>
        public static void DisplayCutsImage(string cutsfile, int imageNo, TextureRect targetControl, bool DisableCamera = true)
        {
            var cuts = new CutsLoader(cutsfile);
            if (!csCuts.ContainsKey(cutsfile))
            {
                csCuts.Add(cutsfile, new CutsLoader(cutsfile));
            }
            _ = Coroutine.Run(
                    DisplayCutsImageWithWait(cuts: csCuts[cutsfile], imageNo: imageNo, targetControl: targetControl, DisableCamera: DisableCamera),
                    main.instance
               );
        }

        public static void FlashColour(byte colour, TextureRect targetControl, float duration = 0.2f, bool IgnoreDelay = false)
        {
            var palette = PaletteLoader.Palettes[0];
            var width = 2; var height = 2;
            var img = Image.CreateEmpty(width, height, false, Image.Format.Rgb8);
            for (int iRow = 0; iRow < height; iRow++)
            {
                int iCol = 0;
                for (int j = iRow * width; j < (iRow * width) + width; j++)
                {
                    img.SetPixel(iCol, iRow, palette.ColorAtIndex(colour, false, false));
                    iCol++;
                }
            }

            var tex = new ImageTexture();
            tex.SetImage(img);
            if (!IgnoreDelay)
            {
                _ = Coroutine.Run(
                        FlashColourWithDelay(colorimg: tex, targetControl: targetControl, duration: duration),
                        main.instance
                );
            }
            else
            {
                EnableDisable(targetControl, true);
                targetControl.Texture = tex;
            }
        }

        private static IEnumerator FlashColourWithDelay(ImageTexture colorimg, TextureRect targetControl, float duration = 0.2f)
        {
            EnableDisable(targetControl, true);
            targetControl.Texture = colorimg;
            yield return new WaitForSeconds(duration);
            EnableDisable(targetControl, false);
            yield return 0;
        }

        /// <summary>
        /// Displays the specified image number from a cuts file and waits for input
        /// </summary>
        /// <param name="cuts"></param>
        /// <param name="imageNo"></param>
        /// <param name="targetControl"></param>
        /// <returns></returns>
        private static IEnumerator DisplayCutsImageWithWait(CutsLoader cuts, int imageNo, TextureRect targetControl, bool DisableCamera = true)
        {
            targetControl.Texture = cuts.LoadImageAt(imageNo);
            targetControl.Material = cuts.GetMaterial(imageNo);
            EnableDisable(targetControl, true);
            if (DisableCamera)
            {
                instance.uwsubviewport.Disable3D = true;
            }
            MessageDisplay.WaitingForMore = true;
            while (MessageDisplay.WaitingForMore)
            {//wait until key input before clearing the image
                yield return new WaitOneFrame();
            }
            EnableDisable(targetControl, false);
            instance.uwsubviewport.Disable3D = false;
            yield return 0;
        }


        
        public static void DisplayCutsImage(CutsLoader cuts, int imageNo, TextureRect targetControl)
        {
            targetControl.Texture = cuts.LoadImageAt(imageNo);
            //targetControl.Material = cuts.GetMaterial(imageNo);
        }


    }//end class
}//end namespace