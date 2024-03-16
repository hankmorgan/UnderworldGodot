namespace Underworld
{
    public class containerObjectDat:objectDat
    {
        const int offset = 0xd32;


        /// <summary>
        /// Container capactity in 0.1 stones
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int capacity(int item_id)
        {
            return buffer[offset + (item_id & 0xf) * 3];
        }

        /// <summary>
        /// Object Mask - what objects it can take 
        /// 0xFFFF can take all, 
        /// if>512d then mask on object type see types below
        /// if<512d then match specific item id (unused functionality)
        /// when mask > 512 then mask - 512 =  
        ///     0: runes, 1: arrows, 2: scrolls, 3: edibles, 4: keys, 0xFFFF all.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static short objectmask(int item_id)
        {
           return (short)getAt(buffer,offset + 1 + (item_id & 0xF) * 3,16);
        }
    
    }//end class

}//end namespace