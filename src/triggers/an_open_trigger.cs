using System.Diagnostics;
namespace Underworld
{
    public partial class trigger : UWClass
    {
        /// <summary>
        /// Triggers an open trigger can be linked to the srcObj directly or as a next to a lock.
        /// </summary>
        /// <param name="triggerIndex"></param>
        /// <param name="objList"></param>
        public static bool OpenTrigger(uwObject srcObject, int triggerIndex, uwObject[] objList)
        {
            //open trigger. 625 in UW1, 62A and 63A in UW2
            uwObject triggerObj;
            if (_RES!=GAME_UW2)
            {
                triggerObj =  objectsearch.FindMatchInObjectChain(triggerIndex, 6, 2, 5, UWTileMap.current_tilemap.LevelObjects);
            }
            else
            {
                triggerObj =  objectsearch.FindMatchInObjectChain(triggerIndex, 6, 2, 0xA, UWTileMap.current_tilemap.LevelObjects);
                if (triggerObj== null)
                {
                    triggerObj =  objectsearch.FindMatchInObjectChain(triggerIndex, 6, 3, 0xA, UWTileMap.current_tilemap.LevelObjects);
                }
            }
           
            
            if (triggerObj == null)
            {//no open trigger found                
                return false;
            }            
            else
            {
                //activate trap
                RunOpenTrigger(objList, triggerObj);

                if (_RES == GAME_UW2)
                {//check if the next object of the open trigger is an open trigger as well. Supports the secret room in the avatars room. This may be needed for other combos as well?
                    if (triggerObj.next != 0)
                    {
                        var nextTrigger = objList[triggerObj.next];
                        if ((nextTrigger.item_id == 442) ||(nextTrigger.item_id == 426))
                        {
                            RunOpenTrigger(objList, nextTrigger);
                        }
                    }
                }
                return true;
            }
        }

        private static void RunOpenTrigger(uwObject[] objList, uwObject triggerObj)
        {
            trigger.StartTriggerChainEvents();
            Debug.Print($"Activating trap {triggerObj.link}");
            trap.ActivateTrap(
                triggerObj: triggerObj,
                trapIndex: triggerObj.link,
                objList: objList);
            trigger.EndTriggerChainEvents();
        }
    }//end class
}//end namespace