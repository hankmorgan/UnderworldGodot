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
    public class shade :ArtLoader
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
            var bandwidth=1;
            var shadearray = ExtractShadeArray();
            byte[] imgdata =new byte[16*bandwidth];
            for (int l = 0; l< 16;l++)
            {
                for (int i=0;i<bandwidth;i++)
                {       
                    imgdata[l*bandwidth + i] = (byte)(shadearray[l]*16); //mult by 16 to get a full range
                }
                
            }
            var output = ArtLoader.Image(imgdata, 0, 16*bandwidth, 1, "name here", PaletteLoader.GreyScale, true, true);
            return output;
        }



        public int[] CalculateShades()
        {
            if (shadeCutOff<16)
            {
                int[] shadesArray = ExtractShadeArray();
                ExtractShadingTable(shadesArray);
            }
            return new int[33*17*2];
        }

        /// <summary>
        /// I think this returns a v large look up table for matching raycasts for shading.
        /// I'm goint to ignore in favour of the shadearray
        /// </summary>
        /// <param name="shadesArray"></param>
        /// <returns></returns>
        public int[] ExtractShadingTable(int[] shadesArray)
        {
            int[] largeShadeArray = new int[33*17*2];
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
                    ax = ax + (si << 1);
                    if (var2 > shadeCutOff)
                    {
                        largeShadeArray[ax] = 0xF; //darkness?
                    }
                    else
                    {
                        largeShadeArray[ax] = shadesArray[var2];
                    }
                }
            } //loop di
            return largeShadeArray;
        }

        /// <summary>
        /// Returns an array of the light maps to be used in this shade.
        /// </summary>
        /// <param name="shadesArray"></param>
        public int[] ExtractShadeArray()
        {
            int[] shadesArray =new int[16];
            if (shadeCutOff>=16)
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
            mapindex =_index;
            neardistance = _nearDist;
            nearlightmap= _nearMap & 0xF;
            fardistance = _farDist;
            shadeCutOff = _ShadeCutoff  & 0xF;
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
                                _index : i, 
                                _nearDist: (int)(Int16)getValAtAddress(buffer, 0 + (i * 12), 16 ),
                                _nearMap : (int)getValAtAddress(buffer, 2 + (i * 12), 16 ),
                                _farDist : (int)(Int16)getValAtAddress(buffer, 4 + (i * 12), 16 ),
                                _ShadeCutoff: (int)getValAtAddress(buffer, 6 + (i * 12), 16 )
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