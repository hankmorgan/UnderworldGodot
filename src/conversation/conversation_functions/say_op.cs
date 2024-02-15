using System.Collections;
using System.Diagnostics;
using Peaky.Coroutines;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        /// <summary>
        /// The NPC is talking
        /// </summary>
        public const int NPC_SAY = 0;
        
        /// <summary>
        /// The Avatar is talking
        /// </summary>
        public const int PC_SAY = 1;
        
        /// <summary>
        /// Printed text is displayed
        /// </summary>
        public const int PRINT_SAY = 2;
        /// <summary>
        /// A UI Prompt such as [MORE] is printed
        /// </summary>
        public const int UI_SAY = 3;
        public static IEnumerator say_op(int arg1, int PrintType = NPC_SAY)
        {
            yield return new WaitForSeconds(0.3f);
            //Debug.Print(getString(arg1));
            //uimanager.instance.ConversationText.Text = getString(arg1,true);   
            uimanager.AddToConvoScroll(
                stringToAdd: getString(arg1,true),
                colour: PrintType
                );   
            while (MessageDisplay.WaitingForMore)            
            {
                yield return new WaitOneFrame();
            } 
            yield return null;
        }
    }
}