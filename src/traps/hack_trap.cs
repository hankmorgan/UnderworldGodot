namespace Underworld
{

    public class hack_trap : trap
    {
        public static void activate(uwObject trapObj, uwObject triggerObj, uwObject[] objList)
        {
            switch (trapObj.quality)
            {
                case 3: //do trap platform
                    a_do_trap_platform.activate(
                        trapObj: trapObj,
                        triggerObj: triggerObj,
                        objList: objList
                    );
                return;
            }
        }
    }//end class
}//end namespace