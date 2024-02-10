
using System;

namespace Underworld
{
    public class Rng : UWClass
    {
        public static Random r = new();


        /// <summary>
        /// Returns a random value which is base value offset by the upper and lower range
        /// </summary>
        /// <param name="basevalue"></param>
        /// <param name="lower_offset"></param>
        /// <param name="upper_offset"></param>
        /// <returns></returns>
        public static int RandomOffset(int basevalue, int lower_offset, int upper_offset)
        {
            return r.Next(basevalue + lower_offset, basevalue + upper_offset);
        }


        /// <summary>
        /// Returns NoOfLoops + ({NoOfLoops}D{DiceRange})
        /// </summary>
        /// <param name="NoOfLoops"></param>
        /// <param name="diceRange"></param>
        /// <returns></returns>
        public static int DiceRoll(int NoOfLoops, int diceRange)
        {
            var di = diceRange;
            var si = NoOfLoops;
            if (di <= 0)
            {
                return si;
            }
            else
            {
                if (NoOfLoops <= 0)
                {
                    return si;
                }
                else
                {
                    while (NoOfLoops != 0)
                    {
                        si += r.Next(0, di);
                        NoOfLoops--;
                    }
                    return si;
                }
            }
        }
    }
}