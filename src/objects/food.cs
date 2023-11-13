using System.Diagnostics;

namespace Underworld
{
    public class food : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (WorldObject == false)
            {// use in inventory
                Debug.Print("You eat");
                //Remove Object From Inventory
            }
            else
            {//do nothing when object is in the world
                
            }
            return true;
        }
    }
}//end namespace