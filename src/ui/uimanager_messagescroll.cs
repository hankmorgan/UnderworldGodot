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

        public static bool MessageScrollIsTemporary = false;



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
        public static void AddToMessageScroll(string stringToAdd, int option = 0, int colour = 0, MessageDisplay.MessageDisplayMode mode = MessageDisplay.MessageDisplayMode.NormalMode)
        {
            if (MessageScrollIsTemporary)
            {//don't allow any overwriting when a message is already being displayed temporarily.
                return;
            }



            switch (mode)
            {
                case MessageDisplay.MessageDisplayMode.TypedInput:
                    //TODO.
                    break;
                case MessageDisplay.MessageDisplayMode.TemporaryMessage:
                    {
                        //back up lines
                        var backup = BackupLines(instance.scroll.Lines, 5);
                        MessageScrollIsTemporary = true;
                        instance.scroll.Clear();
                        _ = Peaky.Coroutines.Coroutine.Run(
                            instance.scroll.AddText(newText: stringToAdd, option: option, colour: colour),
                            main.instance
                            );

                        _ = Peaky.Coroutines.Coroutine.Run(
                            instance.scroll.RestoreLinesAfterWait(backup, 2f),
                            main.instance
                            );
                        break;
                    }
                case MessageDisplay.MessageDisplayMode.NormalMode:
                default:
                    {
                        _ = Peaky.Coroutines.Coroutine.Run(
                                instance.scroll.AddText(newText: stringToAdd, option: option, colour: colour),
                                main.instance
                                );
                        break;
                    }
            }
        }

        private static MessageScrollLine[] BackupLines(MessageScrollLine[] toBackup, int size)
        {
            var existingLines = new MessageScrollLine[size];
            for (int i = 0; i <= existingLines.GetUpperBound(0); i++)
            {
                existingLines[i] = new MessageScrollLine(toBackup[i].OptionNo, toBackup[i].LineText);
            }
            return existingLines;
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