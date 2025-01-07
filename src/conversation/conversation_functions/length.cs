namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void length()
        {
            var stringaddress =  GetConvoStackValueAtPtr(stackptr - 1);           
            result_register = getString(stringaddress).Length;
        }
    }//end class
}//end namespace