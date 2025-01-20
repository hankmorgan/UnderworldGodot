namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        /// <summary>
        /// Returns a random number
        /// </summary>
        public static void Random()
        {
            var max = GetConvoStackValueAtPtr(stackptr - 1);  
            result_register = Rng.r.Next(max) + 1;
            return;
        }
    } 
}