using System;
using System.Diagnostics;
using System.IO;
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

        public short[] shadingbasedata;

        public byte[] ShadingArray_26EE = new byte[17 * 66];//This array is probably the light map that should be used for the shading but the existing effect looks right enough. possible structure is byte0 - is point visible, byte1 shading value to use at that point?

        //ImageTexture cachedimage;

        public static float GetViewingDistance(int index)
        {
            return 4.8f * 7;
            //return (float)shadesdata[index].ViewingDistance * 1.2f * 4;
        }

        /// <summary>
        /// Creates a banded light map image for the uwshader that lerps shade bands to allow smoother shading.
        /// </summary>
        /// <param name="pal"></param>
        /// <param name="maps"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Godot.ImageTexture GetFullShadingImage(Palette pal, lightmap[] maps, int index, short[] shadingdata)
        {
            int BandSize = Math.Max(uwsettings.instance.shaderbandsize, 1);
            var img = Godot.Image.CreateEmpty(256, BandSize * 15, false, Godot.Image.Format.Rgba8);
            //var arr = shadesdata[index].ExtractShadeArray();
            //int y = 0;
            lightmap basemap = maps[0];
            lightmap nextmap = maps[1];
            for (int i = 0; i < shadingdata.GetUpperBound(0); i++)
            {
                for (int y = 0; y < BandSize; y++)
                {
                    if (y % BandSize == 0)
                    {//At a band that contains colours specified by the light map.
                        //Apply primary colour band
                        basemap = maps[shadingdata[i]];
                        if (i + 1 < maps.GetUpperBound(0))
                        {
                            nextmap = maps[shadingdata[i + 1]];
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
                                        colour = colour.Lerp(nextcolour, (float)(shadingdata[i] / 15f));
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
                                        lerpedcolour = basecolour.Lerp(nextcolour, (float)(shadingdata[i] / 15f));
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


        /// <summary>
        /// Returns an array of the light maps to be used in this shade. Likely I am not returning the correct shading values but the current effect looks okay enough.
        /// </summary>
        /// <param name="shadesArray"></param>
        public short[] ExtractShadeArray()
        {
            short[] shadesArray = new short[16];
            if (ViewingDistance >= 16)
            {   //return all zeros.
                return shadesArray;
            }
            for (short si = 0; si < 16; si++)
            {
                if (si <= ViewingDistance)
                {
                    short ax = si;
                    ax = (short)Math.Pow(ax * 8, 2);
                    //int var6 = ax;
                    ax = (short)(ax << 1);
                    //int var4 = ax;
                    ax = (short)UnderWorldSqrt.sqrt_vanilla((ushort)ax);  //(short)Math.Sqrt(ax);
                    short var6 = ax;
                    int var4 = (short)(var6 * Shading / 64);
                    var4 += StartOfShadingDistance;
                    if (var4 < 0)
                    {
                        var4 = 0;
                    }
                    var6 = (short)(var4 + StartingLightLevel);
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

            var di = 0;
            while (di < 0x11)
            {
                var si = 0;
                while (si < 0x21)
                {
                    //seg32_54C
                    //var var2 = (int)Math.Round(Math.Sqrt((0x10 - si) * (0x10 - si) + di * di), 0); // 
                    //vanilla underworld sqrt is used here because it slightly different values are returned compared to .NET sqrt. 
                    // This has later impacts on tile visibility calcs for the automap
                    // .eg when di = 0x2 and si = 0xE the (int)sqrt() will return 2 but vanilla game will return 3
                    var var2 = (short)UnderWorldSqrt.sqrt_vanilla((ushort)((0x10 - si) * (0x10 - si) + (di * di)));
                    
                    if (var2 <= ViewingDistance)
                    {
                        //seg32_58B
                        ShadingArray_26EE[di * 66 + (si << 1) + 1] = (byte)shadesArray[var2];//33 used to be 66 
                    }
                    else
                    {
                        //Seg32_577
                        ShadingArray_26EE[di * 66 + (si << 1) + 1] = 0xF;
                    }
                    si++;
                }
                di++;
            }

            // File.WriteAllBytes($"c:\\temp\\shade_{mapindex}.dat", ShadingArray_26EF);
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
            shadingbasedata = ExtractShadeArray();
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