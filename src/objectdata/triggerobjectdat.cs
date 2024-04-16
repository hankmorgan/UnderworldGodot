namespace Underworld
{
    public class triggerObjectDat:objectDat
    {
        const int offset = 0xd92;


        public enum triggertypes
        { 
            ALL = -1,           
            MOVE = 0,
            STEP_ON = 1,
            PICKUP = 2,
            USE = 4,
            LOOK = 5,
            ENTER = 6,
            UNLOCK_UW1 = 6,
            PRESSURE = 7,
            OPEN_UW1  = 7,
            OPEN_UW2  = 8,
            CLOSE = 9,
            TIMER = 10,
            UNLOCK_UW2 = 11,
            SCHEDULED = 12,
            EXIT = 14,
            PRESSURE_RELEASE = 15            
        }

        /// <summary>
        /// The type of trigger this is
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int triggertype(int item_id)
        {
            return buffer[offset + (item_id & 0xf)];
        }

        public static int triggertype_masked(int item_id)
        {
            return triggertype(item_id) & 0xF07;
        }

    }//end class

}//end namespace