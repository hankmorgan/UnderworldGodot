
using System;
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Handles RNG calls for the game. Eventually this may replicate the original rng that is based on system time rather than .NET rng.
    /// </summary>
    public class Rng : UWClass
    {
        public static uint Seed;
        const int Multiplier = 0x15A4E35;
        public static Rng r = new();
        //public static Random r_unseeded = new(); //for rng calls where the seed never gets reset

        public Rng()
        {
            InitRng();
        }

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
            var rnd = r.Next(range);
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


        /// <summary>
        /// The unix epoch is found and stored as the RNG seed.
        /// </summary>
        public static void InitRng()
        {
            Seed = (ushort)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() & 0xFFFF);
            Debug.Print($"Rng Seeded to {Seed}");
        }

        /// <summary>
        /// An RNG result is obtained by multipying Seed * Constant Multipler. 
        /// The multiplication result is stored as the next seed.
        /// The max value is mod divided into the (result & 0x7FFF) to get the RNG number
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public int Next(int max)
        {
            var newSeed = (uint)((Seed * Multiplier) + 1);
            Seed = newSeed;
            Debug.Print($"Rng={((newSeed) & 0x7FFF) % max} out of {max}. Next Seed is {Seed}");
            return (int)(((newSeed) & 0x7FFF) % max);
        }

        public int Next(int min, int max)
        {
            var res = Next(max-min);
            return min + res;
        }

        void RngTest()
        {
            int[] rngtest = new int[100];
            for (int i = 0; i<4000;i++)
            {
                var res = Rng.r.Next(100);
                rngtest[res]++;
            }
        }

    }//end class
}//end namespace