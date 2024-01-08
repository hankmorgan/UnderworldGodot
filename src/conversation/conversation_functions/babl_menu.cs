using System.Collections;
using System.Diagnostics;
using Peaky.Coroutines;

namespace Underworld
{

    public partial class ConversationVM : UWClass
    {
        static bool usingBablF;
        public static int MaxAnswer;
        public static IEnumerator babl_menu()
        {
            int[] args = new int[1];
            args[0] = at(stackptr - 1);//ptr to value

            int Start = args[0];

            yield return new WaitForSeconds(0.2f);
            usingBablF = false;
            MaxAnswer = 0;
            int j = 1;
            ClearConversationOptions();
            for (int i = Start; i <= StackValues.GetUpperBound(0); i++)
            {
                if (at(i) > 0)
                {
                    string TextLine = getString(at(i));
                    if (TextLine.Contains("@"))
                    {
                        TextLine = TextSubstitute(TextLine);
                    }
                    Debug.Print($"{j} {TextLine}");
                    //UWHUD.instance.ConversationOptions[j - 1].SetText(j + "." + TextLine + "");
                    //UWHUD.instance.EnableDisableControl(UWHUD.instance.ConversationOptions[j - 1], true);
                    j++;
                    MaxAnswer++;
                }
                else
                {
                    break;
                }
            }
            WaitingForInput=true;
            while(WaitingForInput)
            {
                //Debug.Print("Waiting!");
                yield return new WaitOneFrame();     
            }
            //yield return new WaitForInput();
            //yield return Coroutine.Run(WaitForInput(),main.instance);
            //yield return Coroutine.Run(WaitForInput(),main.instance);
            int AnswerIndex = at(Start + PlayerNumericAnswer - 1);
            yield return Coroutine.Run(say_op(AnswerIndex, PC_SAY),main.instance);
            result_register =  PlayerNumericAnswer;
            yield return 0;
        }


        static void ClearConversationOptions()
        {

        }
    }  //end class
} //end namespace