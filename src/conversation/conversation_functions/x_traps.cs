using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void x_traps()
        {
            Debug.Print($"X_traps {GetConvoStackValueAtPtr(stackptr-1)},{GetConvoStackValueAtPtr(stackptr-2)}");

            var di_arg1 = GetConvoStackValueAtPtr(stackptr-2);
            var si_arg0 = GetConvoStackValueAtPtr(stackptr-1);

            if ((si_arg0 >=0) && (di_arg1<0x3FF))
            {
                //sets variable di to value in si
                a_set_variable_trap.VariableOperationUW2(
                    LeftVariable: di_arg1, 
                    Operation: 2, 
                    RightVariable: si_arg0);
            }

            result_register = a_check_variable_trap.GetVarQuestOrClockValue(di_arg1);
        }
    }//end class
}//end namespace