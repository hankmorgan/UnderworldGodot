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
        public static bool UseTrigger(uwObject srcObject, int triggerIndex, uwObject[] objList)
        {
            var triggerObj = objList[triggerIndex];
            if (triggerObj == null)
            {
                Debug.Print($"Null trigger at {triggerIndex}");
                return false;
            }
            //a_use trigger	4	4
            if ((triggerObj.majorclass == 6) && ((triggerObj.minorclass == 2) || (triggerObj.minorclass == 3)) && (triggerObj.classindex == 2))
            {//use trigger class , 6-2-2 or 6-3-2
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