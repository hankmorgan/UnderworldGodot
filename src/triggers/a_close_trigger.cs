using System.Diagnostics;
namespace Underworld
{
    public partial class trigger : UWClass
    {
        /// <summary>
        /// Triggers an Close trigger can be linked to the srcObj directly or as a next to a lock.
        /// </summary>
        /// <param name="triggerIndex"></param>
        /// <param name="objList"></param>
        public static bool CloseTrigger(uwObject srcObject, int triggerIndex, uwObject[] objList)
        {
            //Close trigger. 62B in UW2 only
            uwObject triggerObj;
            if (_RES==GAME_UW2)
            {
                triggerObj =  objectsearch.FindMatchInObjectChain(triggerIndex, 6, 2, 0xB, UWTileMap.current_tilemap.LevelObjects);
            }
            else
            {
                return false;
            }            
            if (triggerObj == null)
            {//no Close trigger found                
                return false;
            }            
            else
            {
                //activate trap
                trigger.StartTriggerChainEvents();
                Debug.Print($"Activating trap {triggerObj.link}");
                trap.ActivateTrap(
                    triggerObj: triggerObj,
                    trapIndex: triggerObj.link,
                    objList: objList);
                trigger.EndTriggerChainEvents();
                return true;
            }
        }
    }//end class
}//end namespace