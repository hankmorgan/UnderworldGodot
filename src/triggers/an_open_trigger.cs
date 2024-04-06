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
                Debug.Print($"Activating trap {triggerObj.link}");
                trap.ActivateTrap(
                    triggerObj: triggerObj,
                    trapIndex: triggerObj.link,
                    objList: objList);
                return true;
            }
        }
    }//end class
}//end namespace