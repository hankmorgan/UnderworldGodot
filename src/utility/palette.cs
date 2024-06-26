using Godot;

namespace Underworld
{

    public class Palette : UWClass
    {

        public static int CurrentPalette
        {
            get
            {
                return _currentpalette;
            }
            set
            {
                bool update = (_currentpalette != value);
                _currentpalette = value;
                if (update)
                {
                    PaletteLoader.UpdateShaderParams();
                }                
            }
        }

        static int _currentpalette = 0;
        /// <summary>
        /// 0 = normal colours, 1 for greyscale
        /// </summary>
        public static int ColourTone = 0;
        public byte[] red = new byte[256];
        public byte[] blue = new byte[256];
        public byte[] green = new byte[256];
        public byte[] alpha = new byte[256];


        /// <summary>
        /// Prebuilt array of cycled palettes for use in shaders.
        /// </summary>
        public ImageTexture[,,] cycledGamePalette;   //[mono|shaded, ]
        public ImageTexture[] cycledUIPalette;

        /// <summary>
        /// Returns the color for the specified palette index. 
        /// </summary>
        /// <returns>The at pixel.</returns>
        /// <param name="index">Pixel.</param>
        /// <param name="useAlphaChannel">If set to <c>true</c> alpha.</param>
        public Color ColorAtIndex(byte index, bool useAlphaChannel, bool useSingleRedChannel)
        {
            byte alphabyte;
            if (useAlphaChannel == true)
            {
                if (index != 0) //Alpha
                {
                    alphabyte = alpha[index]; //255
                }
                else
                {
                    alphabyte = 0;  //transparent
                }
            }
            else
            {
                alphabyte = 255; //no alpha
            }

            if (useSingleRedChannel)
            { //This means the shader will contain the colour information in a palette parameter
                return Color.Color8(
                    g8: 0,
                    r8: index,
                    b8: 0,
                    a8: 0
                );
            }
            else
            {
                return Color.Color8(
                        g8: green[index],
                        r8: red[index],
                        b8: blue[index],
                        a8: alphabyte
                    );
            }
        }

        public Color ColorAtPixelAlpha(byte pixel, byte alpha)
        {
            return new Color(red[pixel], green[pixel], blue[pixel], alpha);
        }

        //Returns a 2x2 texture representing the pixel index for use in rendering a single colour on a model.
        public static ImageTexture IndexToImage(byte index)
        {
            var img = Image.Create(2,2,false,Image.Format.R8);
            var c = new Color(
                    g: 0,
                    r: index / 255f,
                    b: 0,
                    a: 0
                );
            img.SetPixel(0,0,c);
            img.SetPixel(0,1,c);
            img.SetPixel(1,0,c);
            img.SetPixel(1,1,c);
            var tex = new Godot.ImageTexture();
            tex.SetImage(img);
            return tex;
        }


        public virtual ImageTexture toImage(int ColourBandSize = 1)
        {
            int ImageHeight, NoOfColours;
            byte[] imgData;
            BuildPaletteImgData(ColourBandSize, out ImageHeight, out NoOfColours, out imgData);
            var output = ArtLoader.Image(
                    databuffer: imgData, 
                    dataOffSet: 0, 
                    width: NoOfColours * ColourBandSize, 
                    height: ImageHeight, 
                    palette: this, 
                    useAlphaChannel: true, 
                    useSingleRedChannel: false,
                    crop: false);
            return output;
        }

        protected static void BuildPaletteImgData(int ColourBandSize, out int ImageHeight, out int NoOfColours, out byte[] imgData)
        {
            ImageHeight = 16;
            NoOfColours = 256;
            imgData = new byte[ImageHeight * NoOfColours * ColourBandSize];
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