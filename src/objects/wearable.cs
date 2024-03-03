namespace Underworld
{
    /// <summary>
    /// Base class for paperdoll wearables
    /// </summary>
    public class wearable:objectInstance
    {
        public static int GetSpriteIndex(uwObject obj)
        {                
            if (obj==null)
            {
                return -1; //clear sprite
            }    
            if ((obj.item_id>=48) && (obj.item_id<=50))
            {
                return 61 + obj.classindex;
            }
            if (obj.classindex<15)
            {
                return ((obj.quality/16) * 15 + obj.classindex);
            }   
            else
            {
                return  60 + (obj.item_id - 47);
            }
        }
    }
}