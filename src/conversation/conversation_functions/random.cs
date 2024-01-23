namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        /// <summary>
        /// Retrieve from the stack the correct pronouns based on the player sex. The possible pronouns are stored in advanced
        /// </summary>
        public static void Random()
        {
            var max = GetConvoStackValueAtPtr(stackptr - 1);  
            result_register = Rng.r.Next(max);
            return;
        }
    } 
}