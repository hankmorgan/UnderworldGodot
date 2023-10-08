using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;

namespace Underworld
{
    /// <summary>
    /// Class for loading and accessing shades.dat
    /// </summary>
    public class shade : Loader
    {
        public int mapindex;
        int nearlightmap;
        int farlightmap;
        float neardistance;
        float fardistance;

        static shade[] shadesdata;

        public static int getNearMap(int index)
        {
            return shadesdata[index].nearlightmap;
        }

        public static int getFarMap(int index)
        {
            return shadesdata[index].farlightmap;
        }

        public static float getNearDist(int index)
        {
            return shadesdata[index].neardistance;
        }

        public static float getFarDist(int index)
        {
            return shadesdata[index].fardistance;
        }



        public shade(int _index, float _nearDist, int _nearMap, float _farDist, int _farMap)
        {
            mapindex =_index;
            neardistance = _nearDist;
            nearlightmap= _nearMap;
            fardistance = _farDist;
            farlightmap = _farMap;
            Debug.Print($"{_index} {_nearDist} {_nearMap} {_farDist} {_farMap}");
        }

        static shade()
        {            
            var path = System.IO.Path.Combine(BasePath, "DATA", "SHADES.DAT");
            if (System.IO.File.Exists(path))
            {
                if (ReadStreamFile(path, out byte[] buffer))
                {
                    shadesdata = new shade[12];
                    for (int i = 0; i < 12; i++)
                    {
                        try
                        {
                            shadesdata[i] = new shade(
                                _index : i, 
                                _nearDist: (float)(Int16)getValAtAddress(buffer, 0 + (i * 8), 16 ),
                                _nearMap : (int)getValAtAddress(buffer, 2 + (i * 8), 16 ),
                                _farDist : (float)(Int16)getValAtAddress(buffer, 4 + (i * 8), 16 ),
                                _farMap: (int)getValAtAddress(buffer, 6 + (i * 8), 16 )
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
                    _nearDist: 0f,
                    _nearMap: 0,
                    _farDist: 1000f,
                    _farMap: 0
                 );
            }
        }
    }//end class
}//end namespace