namespace Underworld
{
    public class animationObjectDat:objectDat
    {
        const int offset = 0xda2;
        public static int type(int item_id)
        {
            return buffer[offset + (item_id & 0xf) * 4];
        }

        public static int startFrame(int item_id)
        {
            return buffer[2 + offset + (item_id & 0xf) * 4];
        }

        public static int noOfFrames(int item_id)
        {
            return buffer[3 + offset + (item_id & 0xf) * 4];
        }

        //Extensions
        public static int endFrame(int item_id)
        {
            return startFrame(item_id) + noOfFrames(item_id) - 1;
        }
    }//end class
}//end namespace