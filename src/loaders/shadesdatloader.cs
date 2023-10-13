using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using Godot.NativeInterop;

namespace Underworld
{
    /// <summary>
    /// Class for loading and accessing shades.dat
    /// </summary>
    public class shade : ArtLoader
    {
        public int mapindex;
        int nearlightmap;
        int shadeCutOff;
        int neardistance;
        int fardistance;

        public static shade[] shadesdata;

        public static int getNearMap(int index)
        {
            return shadesdata[index].nearlightmap;
        }

        public static int getShadeCutoff(int index)
        {
            return shadesdata[index].shadeCutOff;
        }

        public static float getNearDist(int index)
        {
            return shadesdata[index].neardistance;
        }

        public static float getFarDist(int index)
        {
            return shadesdata[index].fardistance;
        }

        /// <summary>
        /// Returns the shade map as a single channel image for use in shaders.
        /// </summary>
        /// <returns></returns>
        public Godot.ImageTexture ToImage()
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
            var output = ArtLoader.Image(imgdata, 0, 16 * bandwidth, 1, "name here", PaletteLoader.GreyScale, true, true);
            return output;
        }

        /// <summary>
        /// Extract the full shades array as a image
        /// </summary>
        /// <returns></returns>
        public Godot.ImageTexture FullShadingImage()
        {
            var pal = PaletteLoader.GreyScale;
            var shadearray = ExtractShadeArray();
            var AllShades = ExtractShadingTable(shadearray);
            var width = AllShades.GetUpperBound(0);
            var height = AllShades.GetUpperBound(1);
            var img = Godot.Image.Create(width, height, false, Godot.Image.Format.R8);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pixel = (byte)(AllShades[x,y]*16);
                    img.SetPixel(x,y, pal.ColorAtIndex(pixel, false, true));
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
                    if (var2 > shadeCutOff)
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
            if (shadeCutOff >= 16)
            {   //return all zeros.
                return shadesArray;
            }
            for (int si = 0; si < 16; si++)
            {
                if (si < shadeCutOff)
                {
                    int ax = si;
                    ax = (int)Math.Pow(ax * 8, 2);
                    //int var6 = ax;
                    ax = ax << 1;
                    //int var4 = ax;
                    ax = (int)Math.Sqrt(ax);
                    int var6 = ax;
                    int var4 = (int)(var6 * neardistance / 64);
                    var4 += fardistance;
                    if (var4 < 0)
                    {
                        var4 = 0;
                    }
                    var6 = var4 + nearlightmap;
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

        public shade(int _index, int _nearDist, int _nearMap, int _farDist, int _ShadeCutoff)
        {
            mapindex = _index;
            neardistance = _nearDist;
            nearlightmap = _nearMap & 0xF;
            fardistance = _farDist;
            shadeCutOff = _ShadeCutoff & 0xF;
            Debug.Print($"{_index} {_nearDist} {_nearMap} {_farDist} {_ShadeCutoff}");
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
                                _nearDist: (int)(Int16)getValAtAddress(buffer, 0 + (i * 12), 16),
                                _nearMap: (int)getValAtAddress(buffer, 2 + (i * 12), 16),
                                _farDist: (int)(Int16)getValAtAddress(buffer, 4 + (i * 12), 16),
                                _ShadeCutoff: (int)getValAtAddress(buffer, 6 + (i * 12), 16)
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
            shadesdata = new shade[12];
            Debug.Print("Defaulting to fullbright shades.");
            //initial an array of empty shades that provide full brigh
            for (int i = 0; i < 16; i++)
            {
                shadesdata[i] = new shade(
                    _index: i,
                    _nearDist: 0,
                    _nearMap: 0,
                    _farDist: 0,
                    _ShadeCutoff: 0
                 );
            }
        }
    }//end class
}//end namespace