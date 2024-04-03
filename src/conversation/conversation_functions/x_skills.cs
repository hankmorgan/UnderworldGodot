using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void x_skills()
        {
            if (_RES!=GAME_UW2)
            {
                var NewValue = at(at(stackptr-1));
                var SkillNo = at(at(stackptr-2));
                Debug.Print($"x_skills {NewValue} {SkillNo}");
                if (NewValue==10000)
                {
                    //advance skill using skill gain logic
                    playerdat.IncreaseSkill(SkillNo);
                }
                else
                {
                    if ((NewValue>=0) && (NewValue<=30))
                    {
                        playerdat.SetSkillValue(SkillNo, NewValue);                        
                    }
                }
                result_register = playerdat.GetSkillValue(SkillNo);
            }
            else
            {
                //uw2 version differs as it takes into account skillpoints
                Debug.Print("UW2 Version of X_Skills");
                var NewValue = at(at(stackptr-1));
                var SkillNo = at(at(stackptr-2));
                Debug.Print($"x_skills {NewValue} {SkillNo}");
                if (NewValue > 10000)
                {
                    if (playerdat.SkillPoints > 0)
                    {
                        var result = playerdat.IncreaseSkillGroup(SkillNo);
                        playerdat.UpdateAttributes(true);
                        if (result==1)
                        {
                            playerdat.SkillPoints--;
                        }
                        result_register = result;
                    }
                    else
                    {
                        result_register = 0;
                    }
                }
                else
                {
                    if (NewValue != 10000)
                    {
                        if ((NewValue>=0) && (NewValue<=30))
                        {
                            playerdat.SetSkillValue(SkillNo, NewValue);
                            playerdat.UpdateAttributes(true);
                        }
                    }
                    else
                    {
                        //General skill increase function based on governing stats. 
                        //Uses UW1 implementation from shrines
                        playerdat.IncreaseSkill(SkillNo);
                        playerdat.UpdateAttributes(true);
                    }
                    result_register = playerdat.GetSkillValue(SkillNo);
                }
            }
        }
    } //end class
}//end namespace