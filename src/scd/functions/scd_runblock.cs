using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// What the hell is SCD.ARK? It's an event system. Data is read from scd.ark at the end of conversations, level transitions, by a hack trap, when a block of time is added to the clock and when Killorn is crashing.
    /// </summary>
    public partial class scd:UWClass
    {

        /// <summary>
        /// Evaluates variables and runs a block of other SCD functions.
        /// </summary>
        /// <returns></returns>
        public static int RunBlock(byte[] currentblock, int eventOffset)
        {
            var si_variableindex = (int)Loader.getAt(currentblock, eventOffset+5, 16);// currentblock[eventOffset + 5];
            var var6_upper = si_variableindex + currentblock[eventOffset+7] - 1;
            //var CF = 0;//carry flag to detect if the variable operations have overflowed.
            var di = 0; 
            var tmp = a_check_variable_trap.GetVarQuestOrClockValue(si_variableindex);

            while (true)
            {
                di = di + tmp;
                si_variableindex++;
                if (si_variableindex <= var6_upper)
                {
                    tmp = a_check_variable_trap.GetVarQuestOrClockValue(si_variableindex); 
                    tmp = a_set_variable_trap.VariableTransformUW2(di, currentblock[eventOffset+8], tmp);
                }
                else
                {
                    break;
                }
            }

            if (currentblock[eventOffset+0xA] == di)
            {
                tmp = 0;
            }
            else
            {
                tmp = 1;
            }


            int dx;
            if ((sbyte)currentblock[eventOffset+9]>0)
            {//sbb calculation like below
                dx = 0;
            }
            else
            {
                dx = 1;
            }
            
            // if (di >0xFF)
            // {
            //     CF = 1;//The operation has overflowed.
            // }
            //DEST = DEST – (SRC + CF);
            // var dx = currentblock[eventOffset+9];
            // dx = (byte)- dx;
            // dx = (byte)(dx - (dx + CF) + 1);
            
            if (tmp != dx)
            {
                Debug.Print("The block will try and execute");
                while (true)
                {
                    eventOffset = eventOffset + 0x10;
                    if ((sbyte)(currentblock[eventOffset+4])>=0)
                    {
                        return 0;
                    }
                    //if cf ==1 var = 0; if cf == 0 var = 1/
                    //cf is 1 when [bx+3] is not zero,

                    //les     bx, [bp+SCDParams_arg_0]
                    //mov     al, es:[bx+3]
                    //mov     ah, 0
                    //neg     ax              ; Two's Complement Negation
                    //sbb     ax, ax          ; Integer Subtraction with Borrow
                    //inc     ax  
                    int var7;
                    if ((sbyte)currentblock[eventOffset+3]>0)
                    {
                        var7 = 0;
                    }
                    else
                    {
                        var7 = 1;
                    }
                    // var al = currentblock[eventOffset+3];
                    // al = (byte)-al;
                    // var var7 = al - al + 1;
                    //al = (byte)-al;
                    //var var7 = (byte)(al - (al + CF) + 1);
                    currentblock[eventOffset+4] = (byte)-currentblock[eventOffset+4];
                    Debug.Print($"The block will try and run {eventOffset}");
                    if (ProcessSCDEventRow(currentblock, eventOffset) == 4)
                    {
                        Debug.Print($"Event Row deleted at {eventOffset}");
                        eventOffset = eventOffset - 0x10;
                    }

                    if (var7!=0)
                    {
                        currentblock[eventOffset+4] = (byte)-currentblock[eventOffset+4];
                    }

                    if ((sbyte)(-currentblock[eventOffset+4]) == 0xA)
                    {
                        return 0;
                    }
                }
            }
            else
            {
                Debug.Print("The block has not executed");
                return 0;
            }
        }
    }//end class
}//end namespace
