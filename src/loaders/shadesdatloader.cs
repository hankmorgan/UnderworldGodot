using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    /// <summary>
    /// Class for loading and accessing shades.dat
    /// </summary>
    public class shade : ArtLoader
    {
        public int mapindex;
        int StartingLightLevel;
        int ViewingDistance;
        int Shading;
        int StartOfShadingDistance;

        public static shade[] shadesdata;

        ImageTexture cachedimage;

        public static float GetViewingDistance(int index)
        {
            return 4.8f * 7; //(float)shadesdata[index].ViewingDistance;
        }

        /// <summary>
        /// Creates a banded light map image for the uwshader that lerps shade bands to allow smoother shading.
        /// </summary>
        /// <param name="pal"></param>
        /// <param name="maps"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Godot.ImageTexture GetFullShadingImage(Palette pal, lightmap[] maps, int index, string filename)
        {
            int BandSize = Math.Max(uwsettings.instance.shaderbandsize,1);
            var img = Godot.Image.Create(256, BandSize * 15, false, Godot.Image.Format.Rgba8);
            var arr = shadesdata[index].ExtractShadeArray();
            //int y = 0;
            lightmap basemap = maps[0];
            lightmap nextmap = maps[1];
            for (int i = 0; i < arr.GetUpperBound(0); i++)
            {
                for (int y = 0; y < BandSize; y++)
                {
                    if (y % BandSize == 0)
                    {//At a band that contains colours specified by the light map.
                        //Apply primary colour band
                        basemap = maps[arr[i]];
                        if (i + 1 < maps.GetUpperBound(0))
                        {
                            nextmap = maps[arr[i + 1]];
                        }
                        else
                        {
                            //on last band. finish here.
                            //Debug.Print("LastBand");
                        }
                        for (int x = 0; x < 256; x++)
                        {
                            Color colour;
                            int pixel = basemap.red[x];

                            switch (x)
                            {
                                //Special handling for transparencies
                                case 0xf9:
                                case 0xf0://red
                                case 0xf4://blue
                                case 0xf8://green
                                case 0xfb://used by shadow beast?
                                case 0xfc://white
                                case 0xfd://black???
                                    {
                                        colour = pal.ColorAtIndex((byte)x, true, false);
                                        colour.A8 = 180;
                                        var nextcolour = new Color(0, 0, 0, 0);
                                        colour = colour.Lerp(nextcolour, (float)(arr[i] / 15f));
                                        break;
                                    }
                                    
                                default:                                    
                                    colour = pal.ColorAtIndex((byte)pixel, true, false);                                   
                                    break;
                            }
                            img.SetPixel(x, y + i * BandSize, colour);

                        }
                    }
                    else
                    {//in betweeen lightmap bands. Lerp from the first band to this.
                        for (int x = 0; x < 256; x++)
                        { //apply a lerped colour band from the last to the next
                            var basepixel = basemap.red[x];
                            var nextpixel = nextmap.red[x];
                            var basecolour = pal.ColorAtIndex((byte)basepixel, true, false);
                            var nextcolour = pal.ColorAtIndex((byte)nextpixel, true, false);
                            Color lerpedcolour;
                            switch (x)
                            {//An attempt at simulating xfer transparencies
                                case 0xf9:
                                case 0xf0://red
                                case 0xf4://blue
                                case 0xf8://green
                                case 0xfb://used by shadow beast?
                                case 0xfc://white
                                case 0xfd://black???
                                    {
                                        basecolour = pal.ColorAtIndex((byte)x, true, false);
                                        basecolour.A8 = 180;
                                        nextcolour = new Color(0, 0, 0, 0); //Should this final colour be different depending on the index?
                                        lerpedcolour = basecolour.Lerp(nextcolour, (float)(arr[i] / 15f));
                                        break;
                                    }
                                default:
                                    {
                                        lerpedcolour = basecolour.Lerp(nextcolour, (float)(y % BandSize) / (float)BandSize);
                                        break;
                                    }
                            }

                            img.SetPixel(x, y + i * BandSize, lerpedcolour);
                        }
                    }
                }
            }
            //img.SavePng($"c:\\temp\\{filename}.png");

            var tex = new ImageTexture();
            tex.SetImage(img);
            return tex;
        }

        public ImageTexture GetImage()
        {
            if (cachedimage == null)
            {
                cachedimage = ToImage();
            }
            return cachedimage;
        }

        /// <summary>
        /// Returns the shade map as a single channel image for use in shaders.
        /// </summary>
        /// <returns></returns>
        private Godot.ImageTexture ToImage()
        {
            var bandwidth = 1;
            var shadearray = ExtractShadeArray();
            //var AllShades = ExtractShadingTable(shadearray);
            byte[] imgdata = new byte[16 * bandwidth];
            for (int l = 0; l < 16; l++)
            {
                for (int i = 0; i < bandwidth; i++)
                {
                    imgdata[l * bandwidth + i] = (byte)(shadearray[l] * 16); //mult by 16 to get a full range
                }

            }
            var output = Image(
                databuffer: imgdata,
                dataOffSet: 0,
                width: 16 * bandwidth, height: 1,
                palette: PaletteLoader.GreyScaleIndexPalette,
                useAlphaChannel: true,
                useSingleRedChannel: true,
                crop: false);
            return output;
        }


        /// <summary>
        /// Returns the shade map as a single channel image for use in shaders.
        /// This variant shifts the pixels to the right to allow for smooth shading
        /// </summary>
        /// <returns></returns>
        public Godot.ImageTexture ToShiftedImage()
        {
            var bandwidth = 1;
            var tmparray = ExtractShadeArray();
            var shadearray = new int[tmparray.Length];
            shadearray[0] = tmparray[0];
            for (int i = 1; i <= tmparray.GetUpperBound(0); i++)
            {
                shadearray[i] = tmparray[i - 1];
            }


            byte[] imgdata = new byte[16 * bandwidth];
            for (int l = 0; l < 16; l++)
            {
                for (int i = 0; i < bandwidth; i++)
                {
                    imgdata[l * bandwidth + i] = (byte)(shadearray[l] * 16); //mult by 16 to get a full range
                }

            }
            var output = Image(
                databuffer: imgdata,
                dataOffSet: 0,
                width: 16 * bandwidth, height: 1,
                palette: PaletteLoader.GreyScaleIndexPalette,
                useAlphaChannel: true,
                useSingleRedChannel: true,
                crop: false);
            return output;
        }

        /// <summary>
        /// Extract the full shades array as a image
        /// </summary>
        /// <returns></returns>
        public Godot.ImageTexture FullShadingImage()
        {
            var pal = PaletteLoader.GreyScaleIndexPalette;
            var shadearray = ExtractShadeArray();
            var AllShades = ExtractShadingTable(shadearray);
            var width = AllShades.GetUpperBound(0);
            var height = AllShades.GetUpperBound(1);
            var img = Godot.Image.Create(width, height, false, Godot.Image.Format.R8);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pixel = (byte)(AllShades[x, y] * 16);
                    img.SetPixel(x, y, pal.ColorAtIndex(pixel, false, true));
                }
            }
            var tex = new Godot.ImageTexture();
            tex.SetImage(img);
            return tex;
        }



        // public int[] CalculateShades()
        // {
        //     if (shadeCutOff<16)
        //     {
        //         int[] shadesArray = ExtractShadeArray();
        //         ExtractShadingTable(shadesArray);
        //     }
        //     return new int[33*17*2];
        // }

        /// <summary>
        /// I think this returns a v large look up table for matching raycasts for shading.
        /// I'm goint to ignore in favour of the shadearray
        /// </summary>
        /// <param name="shadesArray"></param>
        /// <returns></returns>
        public int[,] ExtractShadingTable(int[] shadesArray)
        {
            int[,] largeShadeArray = new int[17, 33]; //new int[33*17*2];
            for (int di = 0; di < 17; di++)
            {
                for (int si = 0; si < 33; si++)
                {
                    int ax = 16;
                    ax = ax - si;
                    ax = ax * ax;

                    ax = ax + (di * di);
                    ax = (int)Math.Sqrt(ax);
                    int var2 = ax;
                    ax = di * 66;
                    //ax = ax + (si << 1);
                    //ax = ax + (si);
                    if (var2 > ViewingDistance)
                    {
                        largeShadeArray[di, si] = 0xF; //darkness?
                    }
                    else
                    {
                        largeShadeArray[di, si] = shadesArray[var2];
                    }
                }
            } //loop di
            // string result = "";
            // for (int x = 0; x < 17; x++)
            // {
            //     for (int y = 0; y < 33; y++)
            //     {
            //         result += largeShadeArray[x, y].ToString("#0");
            //     }
            //     result += "\n";
            // }
            //Debug.Print(result);
            return largeShadeArray;
        }

        /// <summary>
        /// Returns an array of the light maps to be used in this shade.
        /// </summary>
        /// <param name="shadesArray"></param>
        public int[] ExtractShadeArray()
        {
            int[] shadesArray = new int[16];
            if (ViewingDistance >= 16)
            {   //return all zeros.
                return shadesArray;
            }
            for (int si = 0; si < 16; si++)
            {
                if (si < ViewingDistance)
                {
                    int ax = si;
                    ax = (int)Math.Pow(ax * 8, 2);
                    //int var6 = ax;
                    ax = ax << 1;
                    //int var4 = ax;
                    ax = (int)Math.Sqrt(ax);
                    int var6 = ax;
                    int var4 = (int)(var6 * Shading / 64);
                    var4 += StartOfShadingDistance;
                    if (var4 < 0)
                    {
                        var4 = 0;
                    }
                    var6 = var4 + StartingLightLevel;
                    if (var6 > 14)
                    {
                        var6 = 14;
                    }
                    shadesArray[si] = var6;
                }
                else
                {
                    shadesArray[si] = 0xF; //darkness
                }
            } //loop si 1
            return shadesArray;
        }

        public shade(int _index, int _Shading, int _StartingLightLevel, int _StartOfShadingDistance, int _ViewingDistance)
        {
            mapindex = _index;
            Shading = _Shading; //I had this as near dist. UnderworldAdventures calls it shading?
            StartingLightLevel = _StartingLightLevel & 0xF;
            StartOfShadingDistance = _StartOfShadingDistance;
            ViewingDistance = _ViewingDistance & 0xF;
            //Debug.Print($"{_index} {_nearDist} {_nearMap} {_farDist} {_ShadeCutoff}");
        }

        static shade()
        {
            var path = System.IO.Path.Combine(BasePath, "DATA", "SHADES.DAT");
            if (System.IO.File.Exists(path))
            {
                if (ReadStreamFile(path, out byte[] buffer))
                {
                    shadesdata = new shade[8];
                    for (int i = 0; i < 8; i++)
                    {
                        try
                        {
                            shadesdata[i] = new shade(
                                _index: i,
                                _Shading: (int)(Int16)getAt(buffer, 0 + (i * 12), 16),
                                _StartingLightLevel: (int)getAt(buffer, 2 + (i * 12), 16),
                                _StartOfShadingDistance: (int)(Int16)getAt(buffer, 4 + (i * 12), 16),
                                _ViewingDistance: (int)getAt(buffer, 6 + (i * 12), 16)
                            );
                        }
                        catch
                        {
                            CreateEmptyShades();
                            return;
                        }
                    }
                }
            }
            else
            {
                CreateEmptyShades();
            }
        }

        private static void CreateEmptyShades()
        {
            shadesdata = new shade[8];
            Debug.Print("Defaulting to fullbright shades.");
            //initial an array of empty shades that provide full bright
            for (int i = 0; i < 8; i++)
            {
                shadesdata[i] = new shade(
                    _index: i,
                    _Shading: 0,
                    _StartingLightLevel: 0,
                    _StartOfShadingDistance: 0,
                    _ViewingDistance: 20
                 );
            }
        }
    }//end class
}//end namespace