namespace Underworld
{
    public partial class trigger:UWClass
    {
        /// <summary>
        /// General trigger function for the execution of triggers generically (call this from the traps only when continuing a chain
        /// Specialised triggers like look and use triggers should be called directly by the interaction modes
        /// </summary>
        /// <param name="srcObject"></param>
        /// <param name="triggerIndex"></param>
        /// <param name="objList"></param>
        public static void Trigger(uwObject srcObject, int triggerIndex, uwObject[] objList)
        {
            if (triggerIndex!=0)
            {
                var Trig = objList[triggerIndex];
                if (Trig!=null)
                {
                    if (Trig.link!=0)
                    {         
                        trap.ActivateTrap(Trig, Trig.link, objList);
                    }
                }
            }
        }  
    }//end class
}//end namespace