using Godot;

namespace Underworld
{

/// <summary>
/// Palette loader.
/// </summary>
    public class PaletteLoader : ArtLoader
    {
        
        /// <summary>
        /// Palettes in pals.dat
        /// </summary>
        public static Palette[] Palettes = new Palette[22];
        public static int NoOfPals = 22;

        /// <summary>
        /// A crappy greyscale palette
        /// </summary>
        //public static Palette GreyScale = null;

        /// <summary>
        /// Palettes loaded by lights.dat
        /// </summary>
        public static Palette[] lights = null;

        /// <summary>
        /// Palettes loaded by mono.dat
        /// </summary>
        public static Palette mono  = null;

        static PaletteLoader()  //void LoadPalettes(string filePath)
        {
            var path_pals = System.IO.Path.Combine(BasePath,"DATA","PALS.DAT");
            var path_lights = System.IO.Path.Combine(BasePath,"DATA","LIGHTS.DAT");
            var path_MONO = System.IO.Path.Combine(BasePath,"DATA","MONO.DAT");
            // GreyScale = new Palette();
            // for (int i = 0; i <= GreyScale.blue.GetUpperBound(0); i++)
            // {
            //     GreyScale.red[i] = (byte)i;
            //     GreyScale.blue[i] = (byte)i;
            //     GreyScale.green[i] = (byte)i;                
            // }
            switch (_RES)
            {
                default:
                    {
                        Palettes = new Palette[NoOfPals];
                        if (ReadStreamFile(path_pals, out byte[] pals_dat))
                        {
                            for (int palNo = 0; palNo <= Palettes.GetUpperBound(0); palNo++)
                            {
                                Palettes[palNo] = new Palette();
                                for (int pixel = 0; pixel < 256; pixel++)
                                {
                                    Palettes[palNo].red[pixel] = (byte)(getValAtAddress(pals_dat, palNo * 256 + (pixel * 3) + 0, 8) << 2);
                                    Palettes[palNo].green[pixel] = (byte)(getValAtAddress(pals_dat, palNo * 256 + (pixel * 3) + 1, 8) << 2);
                                    Palettes[palNo].blue[pixel] = (byte)(getValAtAddress(pals_dat, palNo * 256 + (pixel * 3) + 2, 8) << 2);
                                }
                            }

                        }
                    }
                    break;
            }
        }

        public static int[] LoadAuxilaryPalIndices(string auxPalPath, int auxPalIndex)
        {
            int[] auxpal = new int[16];

            if (ReadStreamFile(auxPalPath, out byte[] palf))
            {
                for (int j = 0; j < 16; j++)
                {
                    auxpal[j] = (int)getValAtAddress(palf, auxPalIndex * 16 + j, 8);
                }
            }
            return auxpal;
        }

        public static Palette LoadAuxilaryPal(string auxPalPath, Palette gamepal, int auxPalIndex)
        {
            Palette auxpal = new Palette
            {
                red = new byte[16],
                green = new byte[16],
                blue = new byte[16]
            };
            if (ReadStreamFile(auxPalPath, out byte[] palf))
            {
                for (int j = 0; j < 16; j++)
                {
                    int value = (int)getValAtAddress(palf, auxPalIndex * 16 + j, 8);
                    auxpal.green[j] = gamepal.green[value];
                    auxpal.blue[j] = gamepal.blue[value];
                    auxpal.red[j] = gamepal.red[value];
                }
            }
            return auxpal;
        }

    }//end class


}//end namespace