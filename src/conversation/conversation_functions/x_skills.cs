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
                Debug.Print($"{NewValue} {SkillNo}");
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
            }
        }
    } //end class
}//end namespace