
using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        /// <summary>
        /// Store on the stack the correct pronouns based on the player sex.
        /// </summary>
        public static void Sex()
        {
            short[] args = new short[2];
            args[0] = at(stackptr - 1);//ptr to value
            args[1] = at(stackptr - 2);//ptr to value
            result_register = sex_string(at(args[0]), at(args[1]));
            return;
        }

    /// <summary>
    /// Returns the appropiate string or pronoun for the player gender.
    /// </summary>
    /// <param name="ParamFemale">String to return if player is female</param>
    /// <param name="ParamMale">String to return if player is male</param>
    /// <returns></returns>
    public static int sex_string(int ParamFemale, int ParamMale)
    {
        if (playerdat.isFemale)
        {
            return ParamFemale;
        }
        else
        {
            return ParamMale;
        }
    }
    } 
}