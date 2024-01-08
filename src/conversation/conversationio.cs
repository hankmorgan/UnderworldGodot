using System.Collections;
using System.Diagnostics;
using Peaky.Coroutines;

namespace Underworld
{
    /// <summary>
    /// Code for Input and output relating to conversation VM.
    /// </summary>
    public partial class ConversationVM : UWClass
    {
        public static int PlayerNumericAnswer;
        public static bool WaitingForInput;

        /// <summary>
        /// Gets the specified string for the currently referenced conversation.
        /// </summary>
        /// <param name="stringno"></param>
        /// <returns></returns>
        public static string getString(int stringno)
        {           
            return GameStrings.GetString(conv.StringBlock, stringno);
        }


        /// <summary>
        /// For subsituting values into strings
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string TextSubstitute(string input)
        {
            return input;
        }

        /// <summary>
        /// Waits for input in babl_menu and bablf_menu
        /// </summary>
        // static IEnumerator WaitForInput()
        // {
        //     WaitingForInput = true;
        //     Debug.Print($"Start {WaitingForInput}");
        //     while (WaitingForInput)
        //     { 
        //         Debug.Print($"Wait {WaitingForInput}");
        //         //yield return null;
        //         yield return new WaitOneFrame();            
        //     }
        //     Debug.Print($"End {WaitingForInput}");             
        // }
    }
}//end namespace