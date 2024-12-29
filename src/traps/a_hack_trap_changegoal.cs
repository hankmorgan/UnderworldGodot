using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Trap that changes the goal of the linked npc. Only known instance is an NPC triggered instance where LBs goal is changed when avatar is put in jail.
    /// </summary>
    public class a_hack_trap_changegoal : trap
    {
        public static void Activate(uwObject trapObj, int character)
        {
            Debug.Print("UNTESTED HACK TRAP CHANGE GOAL. HAS LORD BRITISH TRIGGERED THIS?")
            var linkedObj = UWTileMap.current_tilemap.LevelObjects[trapObj.link];            
           
            if (linkedObj.index != character)
            {
                if (trapObj.link !=1)
                {
                    return;
                }
            }
            Debug.Print($"Running change goal to {trapObj.owner} on {linkedObj.a_name}");
            linkedObj.npc_goal = (byte)trapObj.owner;
        }
    }
}

