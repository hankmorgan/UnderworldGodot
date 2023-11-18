using System.Diagnostics;

namespace Underworld
{
    public class food : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (WorldObject == false)
            {// use in inventory
                Debug.Print($"You eat {foodObjectDat.nutrition(obj.item_id)} at hunger level {playerdat.play_hunger}");
                
                //var nutrition = foodObjectDat.nutrition(obj.item_id);
                //Remove Object From Inventory
                playerdat.RemoveFromInventory(obj.index);
            }
            else
            {//do nothing when object is in the world
                
            }
            return true;
        }
    }
}//end namespace