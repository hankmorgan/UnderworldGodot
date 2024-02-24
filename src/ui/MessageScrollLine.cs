
namespace Underworld
{
    /// <summary>
    /// Storage for displayed message text lines
    /// </summary>
    public class MessageScrollLine
    {
        /// <summary>
        /// The conversation option associated with this line.
        /// </summary>
        public int OptionNo = -1;

        /// <summary>
        /// The text contained within the line.
        /// </summary>
        public string LineText = "";

        public void SetLine(string newText, int option = -1)
        {
            LineText = newText;
            OptionNo = option;
        }
    }//end class

}
