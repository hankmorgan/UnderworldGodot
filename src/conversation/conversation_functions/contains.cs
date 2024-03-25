using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void contains()
        {
            int[] args = new int[2];
            args[0] = at(stackptr - 1);
            args[1] = at(stackptr - 2);

            var string1 = getString(at(args[0]));
            var string2 = getString(at(args[1]));

            Debug.Print($"Check if string {string1} contains {string2}");
            if (string1.Contains(string2,System.StringComparison.OrdinalIgnoreCase))
            {
                result_register = 1;
            }
            else
            {
                result_register = 0;
            }
        }
    }//end class
}//end namespace