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
                TriggerNext(trapObj, objList);//probably need to have a guardrail to stop infinite execution loops
            }

            if (triggerObj.flags1 == 0)
            {
                //remove trigger chain
                Debug.Print("TEST ME, THIS TRIGGER SHOULD ONLY FIRE ONCE");
            }
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