using System.Collections.Generic;
using System.Reflection;

namespace Underworld
{
    public class objectCombination : Loader
    {
        public static List<objectCombination> ObjectCombinations = new();

        public int Obj_A;
        public int Obj_B;

        public bool DestroyA;
        public bool DestroyB;

        public int Obj_Out;

        static objectCombination()
        {
            byte[] buffer;
            ReadStreamFile(System.IO.Path.Combine(BasePath, "DATA", "cmb.dat"), out buffer);
            {
                int add_ptr = 0;
                for (int i = 0; i < 10; i++)
                {
                    int obj_a = (int)getAt(buffer, add_ptr, 16);
                    int obj_b = (int)getAt(buffer, add_ptr + 2, 16);
                    int output = (int)getAt(buffer, add_ptr + 4, 16);
                    if (!(((obj_a & 0x7fff)  == (obj_b & 0x7fff)) && ((obj_a & 0x7fff) == (output & 0x7fff))))   // only add if the 3 objects are different.
                        {
                        objectCombination.ObjectCombinations.Add(
                            new objectCombination
                                (
                                    obj_a & 0x7fff,
                                    obj_b & 0x7fff,
                                    (obj_a >> 15) == 1,
                                    (obj_b >> 15) == 1,
                                    output & 0x7fff
                                )
                        );
                    }

                    add_ptr += 6;
                }
            }
        }

        public objectCombination(int obj_a, int obj_b, bool destroy_a, bool destroy_b, int obj_out)
        {
            Obj_A = obj_a;
            Obj_B = obj_b;
            DestroyA = destroy_a;
            DestroyB = destroy_b;
            Obj_Out = obj_out;
        }

        /// <summary>
        /// Checks if the supplied objects (o1 and o2) can combine, 
        /// if so return the combination object that defines how the combination will work
        /// </summary>
        /// <param name="obj_a"></param>
        /// <param name="obj_b"></param>
        /// <param name="output"></param>
        /// <returns>the objectCombination</returns>
        public static objectCombination GetCombination(int obj_a, int obj_b)
        {
            foreach (var cmb in ObjectCombinations)
            {
                if (
                    (cmb.Obj_A == obj_a) && (cmb.Obj_B == obj_b)
                    ||
                    ((cmb.Obj_A == obj_b) && (cmb.Obj_B == obj_a))
                    )
                {
                    return cmb;
                }
            }
            return null; // no combination found.
        }

    }//end class
}//end namespace