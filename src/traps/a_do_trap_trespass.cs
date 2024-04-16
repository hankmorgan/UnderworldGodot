using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Trap which flags an illegal action to a race of npcs. Subject to stealth checks
    /// </summary>
    public class a_do_trap_trespass : hack_trap
    {
        public static void Activate(uwObject trapObj, uwObject[] objList)
        {
            Debug.Print($"Flag trespass to {trapObj.owner}");            
        }
    } //end class
}//end namespace