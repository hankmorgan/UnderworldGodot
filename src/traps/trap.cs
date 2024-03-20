using System.Diagnostics;

namespace Underworld
{
    public class trap : UWClass
    {
        public static int ObjectThatStartedChain = 0;

        public static void ActivateTrap(uwObject triggerObj, int trapIndex, uwObject[] objList)
        {
            var trapObj = objList[trapIndex];
            if (trapObj == null)
            {
                Debug.Print($"Null trap at {trapIndex}");
                return;
            }
            else
            {
                Debug.Print($"Running trap {trapObj.a_name}");
            }
            var triggerNextIndex = trapObj.link; //default object to trigger next. This may change due to the results of a check_variable_trap
            bool implemented = false;
            if (trapObj.majorclass == 6)
            {
                switch (trapObj.minorclass)
                {
                    case 0: // class 6-0 traps
                        {
                            switch (trapObj.classindex)
                            {
                                case 0://damage traps
                                    {
                                        implemented = true;
                                        a_damage_trap.activate(
                                                trapObj: trapObj,
                                                triggerObj: triggerObj,
                                                objList: objList);
                                        break;
                                    }
                                case 3:// Do and hack traps
                                    {
                                        implemented = true;
                                        hack_trap.activate(
                                                trapObj: trapObj,
                                                triggerObj: triggerObj,
                                                objList: objList);
                                        break;
                                    }
                                case 8://door trap
                                    {
                                        implemented = true;
                                        a_door_trap.activate(triggerObj, trapObj, objList);
                                        break;
                                    }
                                case 0xD://set variable trap
                                    {
                                        implemented = true;
                                        a_set_variable_trap.activate(triggerObj, trapObj);
                                        break;
                                    }
                                case 0xE://check variable trap
                                    {
                                        implemented = true;
                                        triggerNextIndex = a_check_variable_trap.activate(triggerObj, trapObj, objList);
                                        break;
                                    }
                            }
                            break;
                        }
                    case 1://class 6-1
                        {
                            switch (trapObj.classindex)
                            {
                                case 0: //6-1-0 Text String Trap
                                    {
                                        implemented = true;
                                        a_text_string_trap.activate(trapObj, objList);
                                        break;
                                    }

                            }
                            break;
                        }
                }
            }

            if (!implemented)
            {
                Debug.Print($"Unknown/unimplemented Trap Class {trapObj.majorclass} {trapObj.minorclass} {trapObj.classindex} {trapObj.a_name} i:{trapObj.index}");
            }
            else
            {
                if (triggerNextIndex!=0)
                {
                    var triggerNextObject = objList[triggerNextIndex];
                    if (triggerNextObject!=null)
                    {
                        if (triggerNextObject.majorclass == 6)//only trigger next a trap/trigger.
                        {
                            switch (triggerNextObject.minorclass)
                            {
                                case 0:
                                case 1://traps
                                    ActivateTrap(triggerObj, triggerNextIndex, objList); //am i right re-using the original trigger?
                                    break;
                                case 2:
                                case 3://triggers
                                    TriggerNext(trapObj, objList, triggerNextIndex);
                                    break;

                            }
                            
                        }
                    }                    
                }                
            }

            if (triggerObj.flags1 == 0)
            {
                //remove trigger chain
                Debug.Print("TEST ME, THIS TRIGGER SHOULD ONLY FIRE ONCE and clear the trigger chain");
            }
        } //end activate trap


        public static void TriggerNext(uwObject trapObj, uwObject[] objList, int triggerNextIndex)
        {
            //Continue the trigger-trap chain if possible.
            if ((trapObj.link != 0) && (trapObj.is_quant == 0) && (triggerNextIndex !=0 ))
            {
                trigger.Trigger(
                    srcObject: trapObj,
                    triggerIndex: triggerNextIndex,
                    objList: objList);
            }
        }

    }//end class
}//end namespace