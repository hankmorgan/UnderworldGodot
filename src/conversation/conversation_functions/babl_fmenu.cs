using System.Collections;
using Peaky.Coroutines;

namespace Underworld
{

    public partial class ConversationVM : UWClass
    {
        public static bool usingBablF;
        public static int[] bablf_array = new int[5];

        /// <summary>
        /// A babl_menu variation that has flags to control if options appear or not
        /// </summary>
        /// <returns></returns>
        public static IEnumerator babl_fmenu()
        {
            yield return new WaitForSeconds(0.2f);
            
            int Start = at(stackptr - 1);
            int flagIndex = at(stackptr - 2);
            usingBablF = true;
            ClearConversationOptions();
            string finaltext = "";

            for (int i = 0; i <= bablf_array.GetUpperBound(0); i++)
            {//Reset the answers array
                bablf_array[i] = 0;
            }

            int j = 1;
            MaxAnswer = 0;
            for (int i = Start; i <= StackValues.GetUpperBound(0); i++)
            {
                if (at(i) != 0)
                {
                    if (at(flagIndex++) != 0)
                    {
                        string NewResponseOption = getString(at(i),true);
                        if (NewResponseOption.Contains("@"))
                        {
                            NewResponseOption = TextSubstitute(NewResponseOption);
                        }

                        bablf_array[j - 1] = at(i);

                        if (finaltext.Length > 0)
                        {
                            finaltext += "\n";
                        }
                        finaltext += $"{j}. {NewResponseOption}";

                        j++;
                        MaxAnswer++;
                    }
                }
                else
                {
                    break;
                }
            }
            messageScroll.AddString(finaltext);
            WaitingForInput = true;
            while (WaitingForInput)
            {
                //Debug.Print("Waiting!");
                yield return new WaitOneFrame();
            }
            usingBablF=false;
            yield return say_op(bablf_array[PlayerNumericAnswer - 1], PC_SAY);
            result_register = bablf_array[PlayerNumericAnswer - 1];
            yield return 0;
        }

    } //end class

}//end namespace