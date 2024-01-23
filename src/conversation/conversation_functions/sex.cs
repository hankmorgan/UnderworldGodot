using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        /// <summary>
        /// Retrieve from the stack the correct pronouns based on the player sex. The possible pronouns are stored in advanced
        /// </summary>
        public static void Sex()
        {
            var gender = playerdat.gender - 2; //-2 if male, -1 if female
            result_register = GetConvoStackValueAtPtr(stackptr + gender);
            Debug.Print(getString(result_register));
            return;
        }
    } 
}