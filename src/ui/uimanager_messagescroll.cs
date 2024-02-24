using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Messagescroll")]
        [Export] public RichTextLabel messageScrollUW1;
        [Export] public RichTextLabel messageScrollUW2;
        
         /// <summary>
        /// Any typed content, eg "other text" in conversations, quantities to be picked up.
        /// Will auto replace the special text {TYPEDINPUT} in the scrolls.
        /// </summary>
  
        [Export] public LineEdit TypedInput;

        public MessageDisplay scroll = new();

        public MessageDisplay convo = new();



        public static void InitMessageScrolls()
        {
            instance.scroll = new();
            for (int i = 0; i <= instance.scroll.Lines.GetUpperBound(0); i++)
            {
                instance.scroll.Lines[i] = new();
            }
            instance.scroll.OutputControl = new RichTextLabel[] { MessageScroll };

            instance.convo = new();
            instance.convo.Lines = new MessageScrollLine[13];
            instance.convo.Rows = 13;
            instance.convo.Columns = 36;
            for (int i = 0; i <= instance.convo.Lines.GetUpperBound(0); i++)
            {
                instance.convo.Lines[i] = new();
            }
            instance.convo.OutputControl = new RichTextLabel[] { ConversationText };
        }

        public static RichTextLabel MessageScroll
        {
            get
            {
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        return instance.messageScrollUW2;
                    default:
                        return instance.messageScrollUW1;
                }
            }
        }

        /// <summary>
        /// Adds to the bottom scroll
        /// </summary>
        /// <param name="stringToAdd"></param>
        /// <param name="option"></param>
        /// <param name="colour"></param>
        public static void AddToMessageScroll(string stringToAdd, int option = 0, int colour = 0)
        {
            _ = Peaky.Coroutines.Coroutine.Run(
            instance.scroll.AddText(newText: stringToAdd, option: option, colour: colour),
                main.instance
                );
        }


        /// <summary>
        /// Adds the text to the conversation scroll
        /// </summary>
        /// <param name="stringToAdd"></param>
        /// <param name="colour"></param>
        public static void AddToConvoScroll(string stringToAdd, int colour = 0)
        {
            //MessageScroll.Text = stringToAdd;
            _ = Peaky.Coroutines.Coroutine.Run(
                instance.convo.AddText(newText: stringToAdd, colour: colour),
                main.instance
                );
        }

    } //end class

}//end namespace