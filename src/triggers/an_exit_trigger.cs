namespace Underworld
{
    public partial class trigger : UWClass
    {
        /// <summary>
        /// Triggers a trigger that runs when the player Exits any part of a tile
        /// </summary>
        /// <param name="triggerIndex"></param>
        /// <param name="objList"></param>
        // public static bool ExitTrigger(uwObject srcObject, int triggerIndex, uwObject[] objList)
        // {
        //     var triggerObj = objList[triggerIndex];
        //     if (triggerObj == null)
        //     {
        //         Debug.Print($"Null trigger at {triggerIndex}");
        //         return false;
        //     }
        //     //activate trap
        //     trigger.StartTriggerChainEvents();
        //     Debug.Print($"Activating trap {triggerObj.link}");
        //     trap.ActivateTrap(
        //         triggerObj: triggerObj,
        //         trapIndex: triggerObj.link,
        //         objList: objList);
        //     trigger.EndTriggerChainEvents();
        //     return true;
        // }
    }//end class
}//end namespace