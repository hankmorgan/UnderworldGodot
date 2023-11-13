namespace Underworld
{
    public class ring : objectInstance
    {
        public static int GetSpriteIndex(uwObject obj)
        {
            if (obj == null)
            {
                return -1; //clear sprite
            }
            else
            {
                return obj.item_id;
            }
        }
    }//end class
}//end namespace