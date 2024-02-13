using System.Collections;
namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static IEnumerator do_offer()
        {
            //Get strings ids from the stack
            var YeahWeTrade = GetConvoStackValueAtPtr(stackptr - 5);//Guess            
            var INoLike = GetConvoStackValueAtPtr(stackptr - 4);//Trade is not enough
            var YouThinkMeStupid = GetConvoStackValueAtPtr(stackptr - 3);//really bad offer made
            var ImTiredOfThis = GetConvoStackValueAtPtr(stackptr - 2); //trade patience ran out

            var YouMakeNoSense = GetConvoStackValueAtPtr(stackptr - 1);//no items offered


            if (TradePatience < 0)
            {
                yield return say_op(ImTiredOfThis);
                result_register = 0;
                yield return 0;
            }
            else
            {
                if (IsPlayerOfferingItems() && IsNPCOfferingItems())
                {
                    var evaluation = 0;
                    //get the npc to evaluate the deal using their accuracy and applying their likes dislikes
                    var playervalue = GetPCValue(
                           ApplyLikeDislike: true,
                           appraise_accuracy: NPCAppraisalAccuracy,
                           applyAccuracy: true);
                    var npcvalue = GetNPCValue(
                       ApplyLikeDislike: true,
                       appraise_accuracy: NPCAppraisalAccuracy,
                       applyAccuracy: true);
                    if (npcvalue <= 0)
                    {
                        evaluation = 100;
                    }
                    else
                    {
                        evaluation = ((playervalue - npcvalue) * 100) / npcvalue;
                    }
                    if (evaluation >= TradeThreshold)
                    {//accept offer
                        yield return say_op(YeahWeTrade);
                        //TODO trade items.
                        result_register = 1;
                    }
                    else
                    {
                        var result =0; // 0 no change to patience, 1 = bad offer, 2 = insulting offer
                        //trade is not good enough. Check if it impacts on npc patience
                        if (PreviousEvaluation!=0)
                            {
                                if (evaluation>=PreviousEvaluation)
                                    {
                                        var ax = ((TradeThreshold - PreviousEvaluation) * 3)/2;
                                        var dx = TradeThreshold  - evaluation;
                                        if (ax<=dx)
                                        {
                                            result = 0; //no change
                                        }
                                        else
                                        {
                                            result = 1;//bad
                                        }
                                    }
                                    else
                                    {
                                        result = 2;//insulting
                                    }  
                            }
                        else
                            {
                                if (evaluation * 2 < TradeThreshold)
                                {
                                    result = 1;
                                }
                                else
                                {
                                    if (evaluation>=PreviousEvaluation)
                                    {
                                        var ax = ((TradeThreshold - PreviousEvaluation) * 3)/2;
                                        var dx = TradeThreshold  - evaluation;
                                        if (ax<=dx)
                                        {
                                            result = 0; //no change
                                        }
                                        else
                                        {
                                            result = 1;//bad
                                        }
                                    }
                                    else
                                    {
                                        result = 2;
                                    }
                                }
                            }

                            switch(result)
                            {
                                case 2: //insulting
                                    yield return say_op(YouThinkMeStupid);
                                    TradePatience-=2;
                                    break;
                                case 1: //bad offer
                                    yield return say_op(INoLike);
                                    TradePatience--;
                                    break;
                                case 0: //no change
                                    //for some reason nothing gets said here in this scenario.
                                    break;
                            }
                        result_register = 0;
                        //Store evaluation for next time
                        PreviousEvaluation = evaluation;
                    }
                }
                else
                {//no items offered
                    result_register = 0;
                    yield return say_op(YouMakeNoSense);
                }
            }
            yield return 0;
        }
    }//end class
}//end namespace