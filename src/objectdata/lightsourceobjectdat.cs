namespace Underworld
{
    public class lightsourceObjectDat:objectDat
    {
        const int offset = 0xd62;

        /// <summary>
        /// How long the light lasts. 0 means does not go out.
        /// Quality change per period of refresh time( maybe minute?)
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int duration(int item_id)
        {
            return buffer[offset + (item_id & 0xF) * 2];
        }


        /// <summary>
        /// Level of brightness provided 0-4. 0 is unlit.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int brightness(int item_id)
        {
            return buffer[offset + 1 + (item_id & 0xF) * 2];
        }
    }
}//end namespace