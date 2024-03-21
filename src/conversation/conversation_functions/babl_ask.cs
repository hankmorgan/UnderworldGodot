using System.Collections;
using Peaky.Coroutines;

namespace Underworld
{

    public partial class ConversationVM : UWClass
    {   

        public static IEnumerator babl_ask()
        {
            uimanager.instance.TypedInput.Text="";
            uimanager.instance.scroll.Clear();
            uimanager.AddToMessageScroll(">{TYPEDINPUT}");
            MessageDisplay.WaitingForTypedInput=true;
            while(MessageDisplay.WaitingForTypedInput)
            {
                yield return new WaitOneFrame();     
            }
            var typedinput = uimanager.instance.TypedInput.Text.ToUpper();
            result_register = GameStrings.AddString(currentConversation.StringBlock, typedinput);//store string in the conversation string data.
            yield return 0;
        }
    }//end class
}//end namespace