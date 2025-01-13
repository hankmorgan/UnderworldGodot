
namespace Underworld
{
    public partial class scd : UWClass
    {
        public static void scd_variableoperation(byte[] currentblock, int eventOffset)
        {
           // var x = Loader.getAt(currentblock,eventOffset+5,16);
            a_set_variable_trap.VariableOperationUW2((int)Loader.getAt(currentblock,eventOffset+5,16), currentblock[eventOffset+7], currentblock[eventOffset+8]);
        }
    }//end class
}//end namespace