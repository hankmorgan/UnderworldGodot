using System.Collections;
using System.Diagnostics;
using Peaky.Coroutines;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        ///The NPC is talking
        public const int NPC_SAY = 0;
        ///The PC is talking
        public const int PC_SAY = 1;
        ///Printed text is displayed
        public const int PRINT_SAY = 2;
        public static IEnumerator say_op(int arg1, int PrintType = NPC_SAY)
        {
            yield return new WaitForSeconds(0.3f);
            Debug.Print(getString(arg1));
            messageScroll.AddString(getString(arg1));
            yield return null;
        }
    }
}