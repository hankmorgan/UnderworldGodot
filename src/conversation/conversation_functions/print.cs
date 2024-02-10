using System.Collections;
using Peaky.Coroutines;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static IEnumerator print()
        {
            yield return new WaitForSeconds(0.3f);
            //Debug.Print(getString(arg1));
            var arg1  = GetConvoStackValueAtPtr(stackptr - 1);  
            uimanager.AddToConvoScroll(
                stringToAdd: getString(arg1,true),
                colour: 2
                );     
            yield return null;
        }
    }
}