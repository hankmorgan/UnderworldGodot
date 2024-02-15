using System;
using System.Collections;
using System.Diagnostics;
using Godot;
//using Peaky.Coroutines;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Messagescroll")]
        [Export] public RichTextLabel messageScrollUW1;
        [Export] public RichTextLabel messageScrollUW2;

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
        public static void AddToMessageScroll(string stringToAdd, int option = 0, int colour=0)
        {
            //MessageScroll.Text = stringToAdd;
            // _ = Peaky.Coroutines.Coroutine.Run(
            instance.scroll.AddText(newText: stringToAdd, option:option, colour:colour); //,
                                                  //     main.instance
                                                  //      );
        }


        /// <summary>
        /// Adds the text to the conversation scroll
        /// </summary>
        /// <param name="stringToAdd"></param>
        /// <param name="colour"></param>
        public static void AddToConvoScroll(string stringToAdd, int colour = 0)
        {
            //MessageScroll.Text = stringToAdd;
            // _ = Peaky.Coroutines.Coroutine.Run(
            instance.convo.AddText(newText: stringToAdd, colour: colour); //,
                                                  //     main.instance
                                                  //      );
        }

    } //end class



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



    public class MessageDisplay
    {
        public RichTextLabel[] OutputControl;

        public MessageScrollLine[] Lines = new MessageScrollLine[5];

        /// <summary>
        /// The number of characters including spaces to print on a single line
        /// </summary>
        public int Columns = 64;

        /// <summary>
        /// The number of rows of text to print.
        /// </summary>
        public int Rows = 5;

        public int LinePtr = 0;
        public void Clear()
        {
            for (int i = 0; i <= Lines.GetUpperBound(0); i++)
            {
                Lines[i].SetLine("");
            }
            LinePtr = 0;
            UpdateMessageDisplay();
        }

        public void AddLine(string newText, int Option, bool WaitForMore = false, int Colour = 0)
        {
            if (newText.Trim() == "") { return; }
            switch (Colour)
            {
                case ConversationVM.PC_SAY:
                    newText = $"[color=#883E14]{newText}[/color]";
                    break;
                case ConversationVM.PRINT_SAY:
                    newText = $"[color=black]{newText}[/color]";
                    break;
                case ConversationVM.NPC_SAY:
                default:
                    newText = $"[color=#331C13]{newText}[/color]";
                    break;
            }            
            if (LinePtr <= Lines.GetUpperBound(0))
            {
                Lines[LinePtr++].SetLine(newText, Option);
            }
            else
            {
                //shift all lines up.
                for (int i = 1; i <= Lines.GetUpperBound(0); i++)
                {
                    Lines[i - 1].SetLine(Lines[i].LineText, Lines[i].OptionNo);
                    //TODO associate option no with a click event index.
                }
                Lines[Rows - 1].SetLine(newText, Option);
            }

            UpdateMessageDisplay();
            //yield return 0;
        }

        /// <summary>
        /// Outputs all text to the control
        /// </summary>
        private void UpdateMessageDisplay()
        {
            //output all text 
            var output = "";
            for (int i = 0; i <= Lines.GetUpperBound(0); i++)
            {
                if (output.Length > 0)
                {
                    output += "\n";
                }
                output += Lines[i].LineText;
            }
            OutputControl[0].Text = output;
        }

        public void AddText(string newText, int option = -1, int colour = 0)
        {
            //split by new lines
            var TextLines = newText.Split('\n');
            foreach (var textline in TextLines)
            {//then split by whitespace into words
                var Words = textline.Split(' ');
                bool firstWord = false;//Always add the first word.
                string LineToAdd = "";
                var length = 0;
                int WordPtr = 0;
                for (WordPtr = 0; WordPtr <= Words.GetUpperBound(0); WordPtr++)
                {
                    bool AddNewLine = false;
                    length = LineToAdd.Length;
                    if (!firstWord)
                    {
                        LineToAdd = Words[WordPtr] + " ";
                        firstWord = true;
                    }
                    else
                    {
                        while (WordPtr <= Words.GetUpperBound(0) && AddNewLine == false)
                        {
                            AddNewLine = false;
                            if (length + Words[WordPtr].Length <= Columns)
                            {//space to add next word
                                LineToAdd += Words[WordPtr] + " ";
                                WordPtr++;
                            }
                            else
                            {
                                AddNewLine = true;
                                //Debug.Print($"{LineToAdd}");
                                AddLine(newText: LineToAdd, Option: option, Colour: colour);
                                LineToAdd = Words[WordPtr] + " "; //new word. new line.
                            }
                            length = LineToAdd.Length;                            
                        }//end while
                    }
                }//end loop
                if (LineToAdd != "")
                {//Store remaining data
                    //Debug.Print($"{LineToAdd}");
                    AddLine(newText: LineToAdd, Option: option, Colour: colour);
                }
            }
            //yield return 0;
        }

    }
}//end namespace