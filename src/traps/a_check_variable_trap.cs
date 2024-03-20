using System.Diagnostics;

namespace Underworld
{
    public class a_check_variable_trap : trap
    {   


        /// <summary>
        /// Runs a check variable trap. Returns the index of the trigger to execute next. 0 if nothing to trigger
        /// </summary>
        /// <param name="triggerObj"></param>
        /// <param name="trapObj"></param>
        /// <returns></returns>
        public static short activate(uwObject triggerObj, uwObject trapObj, uwObject[] objList)
        {
            if (_RES == GAME_UW2)
            {
                //TODO: Check if same or new logic applies in UW2
                return 0;//fail
            }
            else
            {
                var si = trapObj.zpos;
                var var_18 = trapObj.zpos + trapObj.heading;
                var var_1A = (trapObj.quality << 5) | (int)trapObj.owner;
                var_1A <<= 3;
                var_1A |= (int)trapObj.ypos;
                var di = 0;

                while (si<=var_18)
                {//loop through heading no of variables.
                    var gamevar = playerdat.GetGameVariable(si);
                    if (trapObj.xpos==0)
                    {
                        di<<=3;
                        di |= (gamevar & 0x7);
                    }
                    else
                    {
                        di+=gamevar;
                    }
                    si++;
                }
                
                 //check results
                Debug.Print($"Comparing {di} to {var_1A}");
                if (di==var_1A)
                {
                    return trapObj.link; //trigger the true condition chain.
                }
                else
                { 
                    return GetAlternativeLink(trapObj, objList);//trigger the false condition chain
                }                
            }            
        }

        /// <summary>
        /// Gets the alternative trigger to run when the check variable condition is false. 
        /// This will be the next object of the object the check variable is linked to
        /// </summary>
        /// <param name="trapObj"></param>
        /// <param name="objList"></param>
        /// <returns></returns>
        static short GetAlternativeLink(uwObject trapObj, uwObject[] objList)
        {
            if (trapObj.link!=0)
            {
                var linked = objList[trapObj.link];
                if (linked!=null)
                {
                   if (linked.next!=0)
                   {
                    return linked.next;
                   }
                }
            }
            return 0;
        }

    }//end class
}//end namespace