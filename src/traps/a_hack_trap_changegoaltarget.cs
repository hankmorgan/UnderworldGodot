namespace Underworld
{
    /// <summary>
    /// Trap that changes the goal and target of matching npcs
    /// </summary>
    public class a_hack_trap_changegoaltarget : trap
    {
        public static void Activate(uwObject trapObj, int character)
        {
            var paramsarray = new int[2];
            paramsarray[1] = trapObj.owner;//the goal
            if (trapObj.ypos>0)
            {
                paramsarray[0] = 1; //the target
            }
            else
            {
                paramsarray[0] = character;
            }
            
            int whoami = 0;
            if (trapObj.heading>0)
            {
                whoami = 1<<7;
            }
            whoami = whoami + trapObj.zpos;

            CallBacks.RunCodeOnNPCS( 
                methodToCall: npc.set_goal_and_target_by_array, 
                whoami: whoami, 
                paramsArray: paramsarray, 
                loopAll: trapObj.xpos>0);
        }

    }//end class
}//end namespace