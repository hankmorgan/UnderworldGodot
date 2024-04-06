using System.Diagnostics;
namespace Underworld
{
    public partial class trigger : UWClass
    {
        /// <summary>
        /// Triggers a use trigger linked to the source object at index
        /// </summary>
        /// <param name="triggerIndex"></param>
        /// <param name="objList"></param>
        public static bool OpenTrigger(uwObject srcObject, int triggerIndex, uwObject[] objList)
        {
            var triggerObj = objList[triggerIndex];
            if (triggerObj == null)
            {
                Debug.Print($"Null trigger at {triggerIndex}");
                return false;
            }
            //open trigger. 625 in UW1, 62A and 63A in UW2
            if (
                ((triggerObj.majorclass == 6) && (triggerObj.minorclass == 2) && (triggerObj.classindex == 5) && (_RES!=GAME_UW2))
                ||
                ((triggerObj.majorclass == 6) && (triggerObj.minorclass == 2) && (triggerObj.classindex == 0xA) && (_RES==GAME_UW2))
                ||
                ((triggerObj.majorclass == 6) && (triggerObj.minorclass == 3) && (triggerObj.classindex == 0xA) && (_RES==GAME_UW2))
                )
            {
                //activate trap
                Debug.Print($"Activating trap {triggerObj.link}");
                trap.ActivateTrap(
                    triggerObj: triggerObj,
                    trapIndex: triggerObj.link,
                    objList: objList);
                return true;
            }
            return false;
        }
    }//end class
}//end namespace