using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void x_exp()
        {
            var newEXP = at(at(stackptr-1));
            Debug.Print($"Untested x_exp({newEXP})");
            playerdat.ChangeExperience(newEXP);
            result_register = playerdat.Exp>>4;
        }
    }//end class
}//end namespace