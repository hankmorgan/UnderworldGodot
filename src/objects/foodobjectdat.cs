namespace Underworld
{
    public class foodObjectDat:objectDat
    {
        public const int offset = 0xd82;

        /// <summary>
        /// How much hunger is restored by eating this food
        /// When negative the value is the amount of intoxication provided by a drink.
        /// Note some non food objects provide hard coded nutrition. eg plants
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public int nutrition(int item_id)
        {
            var res = (sbyte)buffer[offset + (item_id & 0xF)]; //signed byte to get negative alcohol values
            return res;
        }
    }//end class
}//end namespace