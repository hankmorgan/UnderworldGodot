using System.Diagnostics;
using Godot;

namespace Underworld
{
    public class trap:UWClass
    {

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
        } //end activate trap

    }//end class

}//end namespace