
using System;

namespace Underworld
{
    /// <summary>
    /// Handles RNG calls for the game. Eventually this may replicate the original rng that is based on system time rather than .NET rng.
    /// </summary>
    public class Rng : UWClass
    {
        public static Random r = new();
        public static Random r_unseeded = new(); //for rng calls where the seed never gets reset

        /// <summary>
        /// Returns a random value which is base value offset by the upper and lower range
        /// </summary>
        /// <param name="basevalue"></param>
        /// <param name="lower_offset"></param>
        /// <param name="upper_offset"></param>
        /// <returns></returns>
        public static int RandomOffset(int basevalue, int lower_offset, int upper_offset)
        {
            var range = upper_offset-lower_offset;
            var rnd = r.Next(0, range);
            return basevalue + basevalue * (lower_offset + rnd) /100;        
        }


        /// <summary>
        /// Returns NoOfLoops + ({NoOfLoops}D{DiceRange})
        /// </summary>
        /// <param name="NoOfLoops"></param>
        /// <param name="diceRange"></param>
        /// <returns></returns>
        public static int DiceRoll(int NoOfLoops, int diceRange)
        {
            //var di = diceRange;
            var si = NoOfLoops;
            if (diceRange <= 0)
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
                        si += r.Next(0, diceRange);
                        NoOfLoops--;
                    }
                    return si;
                }
            }
        }
    }//end class
}//end namespace