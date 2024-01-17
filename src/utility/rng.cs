
using System;

namespace Underworld
{
    public class Rng : UWClass
    {
        public static Random r = new();


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
                if (NoOfLoops<=0)
                    {
                        return si;
                    }
                else   
                    {
                        while (NoOfLoops!=0)
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