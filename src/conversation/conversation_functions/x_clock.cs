using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void x_clock()
        {
            var xvalue = at(at(stackptr-1));
            var xnumber = at(at(stackptr-2));            
           
            if (xvalue>0x100)
            {                
                result_register = playerdat.GetXClock(xnumber);
                Debug.Print($"getting xclock {xnumber} {result_register}");
            }
            else
            {
                if (xnumber==0)
                {//set gameclock
                    Debug.Print($"Unimplemented x_clock({xnumber},{xvalue}) Possibly advance time by variable value");
                }
                else
                {
                    //set xclock value
                    Debug.Print($"setting xclock {xnumber} to {xvalue}");
                    playerdat.SetXClock(xnumber,xvalue);
                }  
                result_register = 0;              
            }
        }
    } //end class
}//end namespace