namespace Underworld
{
    public class gloves:objectInstance
    {
        public static int GetSpriteIndex(uwObject obj)
        {     
            if (obj==null)
            {
                return -1; //clear sprite
            }    
            if (obj.classindex<15)
            {
                return ((obj.quality/15) * 15 + obj.classindex);
            }   
            else
            {
                return  60 + (obj.item_id - 47);
            }
        }
    }
}