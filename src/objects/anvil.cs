using Peaky.Coroutines;

namespace Underworld
{
    public class anvil : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            //flag we are using the anvil
            useon.CurrentItemBeingUsed = new useon(obj, WorldObject);
            //print use message
            uimanager.AddToMessageScroll(useon.CurrentItemBeingUsed.GeneralUseOnString);
            return true;
        }


        /// <summary>
        /// Using an anvil prompt on an item
        /// </summary>
        /// <param name="anvil"></param>
        /// <param name="itemToRepair"></param>
        /// <param name="WorldObject"></param>
        /// <returns></returns>
        public static bool UseOn(uwObject anvil, uwObject itemToRepair, bool WorldObject)
        {
            if(WorldObject)
            {
                return true; //anvil cannot be used on world object
            }
            //Check if object can be repaired.
            if ((sbyte)itemToRepair.Durability >= 0)
            {    //can be repaired            
                _ = Coroutine.Run(
                    repair.RepairLogic(itemToRepair, playerdat.Repair, WorldObject),
                    main.instance
                    );

            }
            else
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_cannot_repair_that_));
            }


            return true;
        }

    }//end class
}//end namespace