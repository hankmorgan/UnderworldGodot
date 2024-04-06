using System.Diagnostics;
using Godot;
namespace Underworld
{
    public class hack_trap : trap
    {
        public static bool CreateDoTrap(Node3D parent, uwObject trapObj, string name)
        {
            switch (trapObj.quality)
            {
                case 2://camera
                    {
                        return a_do_trap_camera.CreateDoTrapCamera(parent, trapObj, name);
                    }
            }
            return true;//unimplemented
        }

        public static bool ActivateHackTrap(uwObject trapObj, uwObject triggerObj, uwObject[] objList)
        {
            switch (trapObj.quality)
            {
                case 2: //do trap camera
                    {
                        a_do_trap_camera.Activate(
                            trapObj: trapObj,
                            triggerObj: triggerObj,
                            objList: objList
                        );
                        return true;
                    }
                case 3: //do trap platform
                    {
                        a_do_trap_platform.Activate(
                            trapObj: trapObj,
                            triggerObj: triggerObj,
                            objList: objList
                        );
                        return true;
                    }
                case 5:// trespass trap
                    {
                        a_do_trap_trespass.Activate(
                            trapObj: trapObj,
                            triggerObj: triggerObj,
                            objList: objList
                        );
                        return false;
                    }
                default:
                    {
                        Debug.Print($"Unimplemented hack trap {trapObj.quality}");
                        return false;
                    }
            }
        }
    }//end class
}//end namespace