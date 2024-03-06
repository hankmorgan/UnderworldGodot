using System.Diagnostics;

namespace Underworld
{
    public partial class trigger:UWClass
    {
        public static void Trigger(uwObject srcObject, int triggerIndex, uwObject[] objList)
        {
            //General switch statemment function for the execution of triggers. (call this from the traps only). 
            //Specialised triggers like look and use triggers should be called directly by the interaction modes
        }       


    }//end class
}//end namespace