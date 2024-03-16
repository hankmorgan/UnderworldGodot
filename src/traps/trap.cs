using System.Diagnostics;

namespace Underworld
{
    public class trap:UWClass
    {
        public static int ObjectThatStartedChain=0;

        public static void ActivateTrap(uwObject triggerObj, int trapIndex, uwObject[] objList)
        {
            var trapObj = objList[trapIndex];
            if (trapObj == null)
            {
                Debug.Print($"Null trap at {trapIndex}");
                return;
            }
            if (trapObj.majorclass==6)
            {
                switch (trapObj.minorclass)
                {
                    case 0: // class 6-0 traps
                        {
                            switch(trapObj.classindex)
                            {
                                case 0://damage traps
                                {
                                    a_damage_trap.activate( 
                                            trapObj: trapObj,
                                            triggerObj: triggerObj,
                                            objList: objList);
                                    break;
                                }                            
                                case 3:// Do and hack traps
                                {
                                    hack_trap.activate( 
                                            trapObj: trapObj,
                                            triggerObj: triggerObj,
                                            objList: objList);
                                    break;
                                }
                                case 8://door trap
                                {
                                    a_door_trap.activate(triggerObj, trapObj, objList);
                                    break;
                                }
                            }
                            break;
                        }
                    case 1://
                        {
                            switch(trapObj.classindex)
                            {
                                case 0: //6-1-0 Text String Trap
                                    {
                                        a_text_string_trap.activate(trapObj,objList);
                                        break;  
                                    } 
                                 
                            }
                            break;
                        }
                    default:
                        Debug.Print ($"Unknown Trap Class {trapObj.item_id}");
                        return; //stop execution.
                }
            }

            TriggerNext(trapObj, objList);//probably need to have a guardrail to stop execution
        } //end activate trap


        public static void TriggerNext(uwObject trapObj, uwObject[] objList)
        {
            //Continue the trigger-trap chain if possible.
            if ((trapObj.link != 0) && (trapObj.is_quant == 0))
            {
                trigger.Trigger(
                    srcObject: trapObj,
                    triggerIndex: trapObj.link,
                    objList: objList);
            }
        }

    }//end class
}//end namespace