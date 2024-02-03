using System.Collections;
using System.Diagnostics;
using Peaky.Coroutines;

namespace Underworld
{

    public partial class ConversationVM : UWClass
    {       
        public static int MaxAnswer;
        public static IEnumerator babl_menu()
        {
            int[] args = new int[1];
            args[0] = at(stackptr - 1);//ptr to value

            int Start = args[0];
            yield return new WaitForSeconds(0.2f);
            
            MaxAnswer = 0;
            int j = 1;
            ClearConversationOptions();
            string finaltext = "";
            for (int i = Start; i <= StackValues.GetUpperBound(0); i++)
            {
                if (at(i) > 0)
                {
                    string NewResponseOption = getString(at(i),true);
                    if (NewResponseOption.Contains("@"))
                    {
                        NewResponseOption = TextSubstitute(NewResponseOption);
                    }
                    if (finaltext.Length>0)
                    {
                        finaltext += "\n";
                    }
                    finaltext += $"{j}. {NewResponseOption}";

                    //Debug.Print($"{j} {TextLine}");
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
            uimanager.AddToMessageScroll(finaltext);
            WaitingForInput=true;
            while(WaitingForInput)
            {
                //Debug.Print("Waiting!");
                yield return new WaitOneFrame();     
            }
            int AnswerIndex = at(Start + PlayerNumericAnswer - 1);
            yield return say_op(AnswerIndex, PC_SAY);            
            result_register =  PlayerNumericAnswer;
            yield return 0;
        }


        static void ClearConversationOptions()
        {

        }
    }  //end class
} //end namespace