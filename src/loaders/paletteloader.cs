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
        public static Palette GreyScaleIndexPalette = null;

        public static Palette CritterPalette = null;

        /// <summary>
        /// Palettes loaded by lights.dat
        /// </summary>
        public static lightmap[] light = null;

        /// <summary>
        /// Palettes loaded by mono.dat
        /// </summary>
        public static lightmap[] mono = null;

        public static int NextPaletteCycle = 0;

        static PaletteLoader()
        {
            var path_pals = System.IO.Path.Combine(BasePath, "DATA", "PALS.DAT");
            var path_light = System.IO.Path.Combine(BasePath, "DATA", "LIGHT.DAT");
            var path_mono = System.IO.Path.Combine(BasePath, "DATA", "MONO.DAT");

            GreyScaleIndexPalette = new Palette();
            for (int i = 0; i <= GreyScaleIndexPalette.blue.GetUpperBound(0); i++)
            {
                GreyScaleIndexPalette.red[i] = (byte)i;
                GreyScaleIndexPalette.blue[i] = 0;// (byte)i;
                GreyScaleIndexPalette.green[i] = 0;// (byte)i;                
            }
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
                                    Palettes[palNo].red[pixel] = (byte)(getAt(pals_dat, palNo * 256 + (pixel * 3) + 0, 8) << 2);
                                    Palettes[palNo].green[pixel] = (byte)(getAt(pals_dat, palNo * 256 + (pixel * 3) + 1, 8) << 2);
                                    Palettes[palNo].blue[pixel] = (byte)(getAt(pals_dat, palNo * 256 + (pixel * 3) + 2, 8) << 2);

                                    switch (pixel)
                                    {
                                        case 0:
                                            Palettes[palNo].alpha[pixel] = 0;break; //transparent
                                        default:
                                            Palettes[palNo].alpha[pixel] = 255; break;//no transparency
                                    }
                                }
                            }
                        }

                        light = new lightmap[16];
                        if (ReadStreamFile(path_light, out byte[] light_dat))
                        {
                            for (int palNo = 0; palNo <= light.GetUpperBound(0); palNo++)
                            {
                                light[palNo] = new lightmap();
                                for (int pixel = 0; pixel < 256; pixel++)
                                { //just store the index values.
                                    light[palNo].red[pixel] = (byte)getAt(light_dat, palNo * 256 + pixel + 0, 8);
                                    light[palNo].blue[pixel] = (byte)getAt(light_dat, palNo * 256 + pixel + 0, 8);
                                    light[palNo].green[pixel] = (byte)getAt(light_dat, palNo * 256 + pixel + 0, 8);
                                    light[palNo].alpha[pixel] = 255;
                                }
                            }
                        }


                        mono = new lightmap[16];
                        if (ReadStreamFile(path_mono, out byte[] mono_dat))
                        {
                            for (int palNo = 0; palNo <= mono.GetUpperBound(0); palNo++)
                            {
                                mono[palNo] = new lightmap();
                                for (int pixel = 0; pixel < 256; pixel++)
                                { //just store the index values.
                                    mono[palNo].red[pixel] = (byte)getAt(mono_dat, palNo * 256 + pixel + 0, 8);
                                    mono[palNo].blue[pixel] = (byte)getAt(mono_dat, palNo * 256 + pixel + 0, 8);
                                    mono[palNo].green[pixel] = (byte)getAt(mono_dat, palNo * 256 + pixel + 0, 8);
                                    mono[palNo].alpha[pixel] = 255;
                                }
                            }
                        }

                    }
                    break;
            }

            Palettes[0].cycledGamePalette = CreateFullTexturePaletteCycles(Palettes[0]);//init the first palette as cycled
            Palettes[0].cycledUIPalette = CreateSimpleTexturePaletteCycles(Palettes[0]); //set up a simple palette cycle for fullbright ui sprites

            //main menu
           // Palettes[6].cycledGamePalette = CreateFullTexturePaletteCycles(Palettes[6]);//init the first palette as cycled
           // Palettes[6].cycledUIPalette = CreateSimpleTexturePaletteCycles(Palettes[6]); //set up a simple palette cycle for fullbright ui sprites
                    

            //Init palette shader params
 
            RenderingServer.GlobalShaderParameterAdd("uipalette", RenderingServer.GlobalShaderParameterType.Sampler2D, Palettes[Palette.CurrentPalette].cycledUIPalette[0]);
            RenderingServer.GlobalShaderParameterAdd("cutoffdistance", RenderingServer.GlobalShaderParameterType.Float, shade.GetViewingDistance(uwsettings.instance.lightlevel));
            RenderingServer.GlobalShaderParameterAdd(
                name: "smoothpalette", 
                type: RenderingServer.GlobalShaderParameterType.Sampler2D, 
                defaultValue: (Texture)Palettes[Palette.CurrentPalette].cycledGamePalette[0,0,0]);
        
        }

        public static int[] LoadAuxilaryPalIndices(string auxPalPath, int auxPalIndex)
        {
            int[] auxpal = new int[16];

            if (ReadStreamFile(auxPalPath, out byte[] palf))
            {
                for (int j = 0; j < 16; j++)
                {
                    auxpal[j] = (int)getAt(palf, auxPalIndex * 16 + j, 8);
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
                    int value = (int)getAt(palf, auxPalIndex * 16 + j, 8);
                    auxpal.green[j] = gamepal.green[value];
                    auxpal.blue[j] = gamepal.blue[value];
                    auxpal.red[j] = gamepal.red[value];
                    auxpal.alpha[j] = gamepal.alpha[value];
                }
            }
            return auxpal;
        }


        /// <summary>
        /// Loads all the lightmaps as a single image to use as a global lookup in the shader.
        /// </summary>
        /// <param name="maps"></param>
        /// <returns></returns>
        public static ImageTexture AllLightMaps(lightmap[] maps)
        {
            byte[] imgdata = new byte[maps.GetUpperBound(0) * 256];
            for (int l = 0; l < maps.GetUpperBound(0); l++)
            {
                for (int b = 0; b < 256; b++)
                {
                    imgdata[(l * 256) + b] = maps[l].red[b];
                }
            }
            var output = Image(imgdata, 0, 256, maps.GetUpperBound(0), GreyScaleIndexPalette, true, true);
            return output;
        }


        /// <summary>
        /// Creates a full set of cycled colour palettes based on the inital palette for all mono,light and shade levels
        /// </summary>
        /// <param name="toCycle"></param>
        /// <returns></returns>
        public static ImageTexture[, ,] CreateFullTexturePaletteCycles(Palette toCycle)
        {
            //copy initial palette
            var tmpPalette = new Palette();
            for (int i = 0; i < 256; i++)
            {
                tmpPalette.red = toCycle.red;
                tmpPalette.green = toCycle.green;
                tmpPalette.blue = toCycle.blue;
                tmpPalette.alpha = toCycle.alpha;
            }

            var NewCycledPalette = new ImageTexture[2,8,28]; //mono/light,light level,Cycle
            for (int l=0; l<8;l++)
            {
                for (int c = 0; c <= 27; c++)
                {//Create palette cycles
                    switch (_RES)
                    {
                        case GAME_UW2:
                            Palette.cyclePalette(tmpPalette, 224, 16);
                            Palette.cyclePaletteReverse(tmpPalette, 3, 6);
                            break;
                        default:
                            Palette.cyclePalette(tmpPalette, 48, 16);//Forward
                            Palette.cyclePaletteReverse(tmpPalette, 16, 7);//Reverse direction.
                            break;
                    }
                    NewCycledPalette[0,l,c] =  shade.GetFullShadingImage(pal: tmpPalette, light, l, $"light_{l}_{c}");   // tmpPalette.toImage();
                    NewCycledPalette[1,l,c] =  shade.GetFullShadingImage(pal: tmpPalette, mono, l, $"mono_{l}_{c}");
                }
            }

            return NewCycledPalette;
        }


        /// <summary>
        /// Creates a simple palette cycle for only the specifie palette at fullbright
        /// </summary>
        /// <param name="toCycle"></param>
        /// <returns></returns>
        public static ImageTexture[] CreateSimpleTexturePaletteCycles(Palette toCycle)
        {
            //copy initial palette
            var tmpPalette = new Palette();
            for (int i = 0; i < 256; i++)
            {
                tmpPalette.red = toCycle.red;
                tmpPalette.green = toCycle.green;
                tmpPalette.blue = toCycle.blue;
                tmpPalette.alpha = toCycle.alpha;
            }

            var NewCycledPalette = new ImageTexture[28]; //cycle
            for (int l=0; l<8;l++)
            {
                for (int c = 0; c <= 27; c++)
                {//Create palette cycles
                    switch (_RES)
                    {
                        case GAME_UW2:
                            Palette.cyclePalette(tmpPalette, 224, 16);
                            Palette.cyclePaletteReverse(tmpPalette, 3, 6);
                            break;
                        default:
                            Palette.cyclePalette(tmpPalette, 48, 16);//Forward
                            Palette.cyclePaletteReverse(tmpPalette, 16, 7);//Reverse direction.
                            break;
                    }
                    NewCycledPalette[c] = tmpPalette.toImage();   // tmpPalette.toImage();
                }
            }

            return NewCycledPalette;
        }


        public static void UpdatePaletteCycles()
        {
            //Cycle the palette		
            RenderingServer.GlobalShaderParameterSet("smoothpalette", (Texture)Palettes[Palette.CurrentPalette].cycledGamePalette[0, uwsettings.instance.lightlevel, NextPaletteCycle]);
            RenderingServer.GlobalShaderParameterSet("uipalette", (Texture)Palettes[Palette.CurrentPalette].cycledUIPalette[NextPaletteCycle]);


            NextPaletteCycle++;

            if (NextPaletteCycle > PaletteLoader.Palettes[Palette.CurrentPalette].cycledGamePalette.GetUpperBound(2))
            {
                NextPaletteCycle = 0;
            }
        }

    }//end class


}//end namespace