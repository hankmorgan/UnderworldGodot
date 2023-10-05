using Godot;

namespace Underworld
{

    public class Palette : UWClass
    {

        public byte[] red = new byte[256];
        public byte[] blue = new byte[256];
        public byte[] green = new byte[256];

        /// <summary>
        /// Returns the color for the specified palette index. 
        /// </summary>
        /// <returns>The at pixel.</returns>
        /// <param name="pixel">Pixel.</param>
        /// <param name="Alpha">If set to <c>true</c> alpha.</param>
        public Color ColorAtPixel(byte pixel, bool Alpha, bool useGreyScale)
        {
            byte alphabyte;
            if (Alpha == true)
            {
                if (pixel != 0) //Alpha
                {
                    alphabyte = 255;
                }
                else
                {
                    alphabyte = 0;
                }
            }
            else
            {
                alphabyte = 0;
            }

            if (useGreyScale)
            {
                return new Color(
                    g: 0,
                    r: pixel / 255f,
                    b: 0,
                    a: 0
                );
            }
            else
            {
                return new Color(
                        g: green[pixel] / 255f,
                        r: red[pixel] / 255f,
                        b: blue[pixel] / 255f,
                        a: alphabyte / 255f
                    );
            }



            // uint rgba;
            // rgba = (uint)(red[pixel]<<24 | green[pixel]<<16 | blue[pixel]<<8 | alphabyte);
            // return new Color(rgba);
            // return new Color(red[pixel], green[pixel], blue[pixel], alpha);
        }

        public Color ColorAtPixelAlpha(byte pixel, byte alpha)
        {
            return new Color(red[pixel], green[pixel], blue[pixel], alpha);
        }


        public ImageTexture toImage(int ColourBandSize = 16)
        {
            int ImageHeight = 16;
            int NoOfColours = 256;
            byte[] imgData = new byte[ImageHeight * NoOfColours * ColourBandSize];
            int x = 0;
            for (int h = 0; h < ImageHeight; h++)
            {
                int i = 0;
                for (int w = 0; w < NoOfColours; w++)
                {
                    for (int b = 0; b < ColourBandSize; b++)
                    {
                        imgData[x++] = (byte)i;
                    }
                    i++;
                }
            }

            // for (int i = 0; i < imgData.GetUpperBound(0); i++)
            // {                
            //     imgData[i] = (byte)i;
            // }
            var output = ArtLoader.Image(imgData, 0, NoOfColours * ColourBandSize, ImageHeight, "name here", this, true, false);
            return output;
        }



        /// <summary>
        /// Cycles the palette.
        /// </summary>
        /// <param name="pal">Pal.</param>
        /// <param name="Start">Start.</param>
        /// <param name="length">Length.</param>
        public static void cyclePalette(Palette pal, int Start, int length)
        {
            /*Shifts the palette values around between the start and start+length. Used for texture animations and special effects*/
            byte firstRed = pal.red[Start];
            byte firstGreen = pal.green[Start];
            byte firstBlue = pal.blue[Start];
            for (int i = Start; i < Start + length - 1; i++)
            {
                pal.red[i] = pal.red[i + 1];
                pal.green[i] = pal.green[i + 1];
                pal.blue[i] = pal.blue[i + 1];
            }
            pal.red[Start + length - 1] = firstRed;
            pal.green[Start + length - 1] = firstGreen;
            pal.blue[Start + length - 1] = firstBlue;
        }


        public static void cyclePaletteReverse(Palette pal, int Start, int length)
        {
            byte nextRed = pal.red[Start];
            byte nextGreen = pal.green[Start];
            byte nextBlue = pal.blue[Start];

            for (int i = Start; i < Start + length; i++)
            {
                byte tmpRed = pal.red[i + 1];
                byte tmpGreen = pal.green[i + 1];
                byte tmpBlue = pal.blue[i + 1];
                pal.red[i + 1] = nextRed;
                pal.green[i + 1] = nextGreen;
                pal.blue[i + 1] = nextBlue;
                nextRed = tmpRed;
                nextGreen = tmpGreen;
                nextBlue = tmpBlue;
            }
            pal.red[Start] = nextRed;
            pal.green[Start] = nextGreen;
            pal.blue[Start] = nextBlue;
        }


    }//end class

}//end namespace