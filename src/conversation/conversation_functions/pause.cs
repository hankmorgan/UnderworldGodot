using System.Collections;
using Peaky.Coroutines;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static IEnumerator pause()
        {
            MessageDisplay.WaitingForMore = true;
            while (MessageDisplay.WaitingForMore)            
            {
                yield return new WaitOneFrame();
            } 
            MessageDisplay.WaitingForMore =false;
        }
    }//end class
}//end namespace