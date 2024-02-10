using System.Diagnostics;
using System.Collections;
using System.Dynamic;
using System.Net.Http.Headers;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {

        /// <summary>
        /// Assesses the value of the offered trade.
        /// </summary>
        public static IEnumerator do_judgement()
        {
            int evaluation;
            int appraisal_certainty;
            int final_evaluation;
            var appraise_accuracy = 50 - ((playerdat.Appraise * 45) / 30); //the variant range +/- that will be used to assess item values.

            //Get total value of player offering
            var playervalue = GetPCValue(false, appraise_accuracy);

            //Get total value of npc offering without applying likes dislikes.
            var npcvalue = GetNPCValue(false, appraise_accuracy);
            if (npcvalue == 0)
            {
                evaluation = 100;
            }
            else
            {
                evaluation = ((playervalue - npcvalue) * 100) / npcvalue;
            }

            switch (playerdat.Appraise)
            {
                case < 6:
                    appraisal_certainty = 0; break;
                case >=6 and <12:
                    appraisal_certainty = 1; break;
                case >=12 and <18:
                    appraisal_certainty = 2; break;
                case >=18 and <24:
                    appraisal_certainty = 3; break;
                case >=24:
                    appraisal_certainty = 4; break;                    
            }          

            switch (evaluation)
            {
                case > 50:
                    final_evaluation =0; break;
                case >35 and <=50:
                    final_evaluation = 1;break;
                case >25 and <=35:
                    final_evaluation = 2;break;
                case >10 and <=25:
                    final_evaluation = 3;break;
                case >-10 and <=10:
                    final_evaluation = 4;break;
                case >-25 and <=-10:
                    final_evaluation = 5;break;
                case >-35 and <=-25:
                    final_evaluation = 6;break;
                case >-50 and <=-35:
                    final_evaluation = 7;break;
                case <=-50:
                    final_evaluation = 8;break;
            }

            string output =
                GameStrings.GetString(7, 3 + appraisal_certainty)
                +
                GameStrings.GetString(7, 2)
                +
                GameStrings.GetString(7, 8 + final_evaluation);

            //Debug.Print(output);
            uimanager.AddToConvoScroll(output,PRINT_SAY);           
            yield return 0;
        } 
    }//end class
}//end namespace