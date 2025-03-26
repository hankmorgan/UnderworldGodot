namespace Underworld
{

    public class Path5859 : Loader
    {
        //public byte[] data058 = new byte[0x80];      
        public static byte[] data = new byte[0x80 * 2];

        public static Path5859[] Path5859_Records = new Path5859[64];
        int PTR;
        static Path5859()
        {
            for (int i = 0; i <= data.GetUpperBound(0); i++)
            {
                Path5859_Records[i] = new Path5859(i);
            }
        }

        public Path5859(int i)
        {
            index = i;
            PTR = i * 2;
        }

        public int index;

        public int X0
        {
            get
            {
                return (int)getAt(data, PTR, 8);

            }
            set
            {
                setAt(data, PTR, 8, value);
            }
        }
        public int Y1
        {
            get
            {
                return (int)getAt(data, PTR + 1, 8);
            }
            set
            {
                setAt(data, PTR + 1, 8, value);
            }
        }
    }

    public class FinalPath56 : Loader
    {
        public static byte[] data056 = new byte[0xFC];

        public static FinalPath56[] finalpath = new FinalPath56[64];

        static FinalPath56()
        {
            for (int i = 0; i <= finalpath.GetUpperBound(0); i++)
            {
                finalpath[i] = new FinalPath56(i);
            }
        }

        public FinalPath56(int i)
        {
            PTR = i * 4;
        }

        int PTR;
        public int X0
        {
            get
            {
                return (int)getAt(data056, PTR, 8);
            }
            set
            {
                setAt(data056, PTR, 8, value);
            }
        }
        public int Y1
        {
            get
            {
                return (int)getAt(data056, PTR + 1, 8);
            }
            set
            {
                setAt(data056, PTR + 1, 8, value);
            }
        }

        public int flag3
        {
            get
            {
                return (int)getAt(data056, PTR + 3, 8);
            }
            set
            {
                setAt(data056, PTR + 1, 8, value);
            }
        }

    }


    public class PathFind57 : Loader
    {
        public static byte[] pathfind57data = new byte[0x1C0];
        public static PathFind57[] PathFind57Records = new PathFind57[16];
        int PTR;

        static PathFind57()
        {
            for (int i = 0; i <= PathFind57Records.GetUpperBound(0); i++)
            {
                PathFind57Records[i] = new PathFind57(i);
            }
        }

        public PathFind57(int i)
        {
            PTR = i * 0x1C;
        }
    }

    /// <summary>
    /// The grid where the path is calculated on.
    /// </summary>
    public class PathFindingData49 : Loader
    {

        public static byte[] data059 = new byte[0x80];


        /// <summary>
        /// 64*64 pathfind grid
        /// </summary>
        public static byte[] pathfindmap49;//= new byte[0x5000];

        public static PathFindingData49[,] pathfindtiles = new PathFindingData49[64, 64];
        int PTR;

        static PathFindingData49()
        {
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    pathfindtiles[x, y] = new PathFindingData49(x, y);
                }
            }
        }

        public PathFindingData49(int x, int y)
        {
            PTR = (x * 0x140) + y * 5;
        }

        public int X0
        {
            get
            {
                return (int)getAt(pathfindmap49, PTR, 8);
            }
            set
            {
                setAt(pathfindmap49, PTR, 8, value);
            }
        }

        public int Y1
        {
            get
            {
                return (int)getAt(pathfindmap49, PTR + 1, 8);
            }
            set
            {
                setAt(pathfindmap49, PTR + 1, 8, value);
            }
        }

        public int Z2
        {
            get
            {
                return (int)getAt(pathfindmap49, PTR + 2, 8);
            }
            set
            {
                setAt(pathfindmap49, PTR + 2, 8, value);
            }
        }

        public int unk3_bit0
        {
            get
            {
                return (int)getAt(pathfindmap49, PTR + 3, 8) & 0x1;
            }
            set
            {
                var tmp = (int)getAt(pathfindmap49, PTR + 3, 8);
                tmp = tmp & 0xFE;
                tmp = tmp | (value & 0x1);
                setAt(pathfindmap49, PTR + 3, 8, tmp);
            }
        }



        public int unk3_bit1_7
        {
            get
            {
                return (int)(getAt(pathfindmap49, PTR + 3, 8) >> 1) & 0x7F;
            }
            set
            {
                var tmp = (int)getAt(pathfindmap49, PTR + 3, 8);
                tmp = tmp & 0x1;
                tmp = tmp | ((value & 0x7F) << 1);
                setAt(pathfindmap49, PTR + 3, 8, tmp);
            }
        }


        public int unk4
        {
            get
            {
                return (int)getAt(pathfindmap49, PTR + 4, 8);
            }
            set
            {
                setAt(pathfindmap49, PTR + 4, 8, value);
            }
        }

    }
}