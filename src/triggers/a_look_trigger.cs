using System.Diagnostics;
namespace Underworld
{
    public partial class trigger : UWClass
    {
        /// <summary>
        /// Sets off a look trigger linked to this object
        /// </summary>
        /// <param name="srcObject"></param>
        /// <param name="triggerIndex"></param>
        /// <param name="objList"></param>
        public static void LookTrigger(uwObject srcObject, int triggerIndex, uwObject[] objList)
        {
            var triggerObj = objList[triggerIndex];
            if (triggerObj == null)
            {
                Debug.Print($"Null trigger at {triggerIndex}");
                return;
            }
            //a_use trigger	4	4
            if ((triggerObj.majorclass == 6) && ((triggerObj.minorclass == 2) || (triggerObj.minorclass == 3)) && (triggerObj.classindex == 3))
            {//use trigger class , 6-2-3 or 6-3-3 
                //activate trap
                trigger.StartTriggerChainEvents();
                Debug.Print($"Activating trap {triggerObj.link}");
                trap.ActivateTrap(
                    triggerObj: triggerObj,
                    trapIndex: triggerObj.link,
                    objList: objList);
                trigger.EndTriggerChainEvents();
            }
        }
    }//end class
}//end namespace