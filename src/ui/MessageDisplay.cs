using Godot;
using Peaky.Coroutines;
using System.Collections;
using System.Runtime.Serialization;

namespace Underworld
{

    /// <summary>
    /// Class for processing message output being sent to the screen.
    /// </summary>
    public class MessageDisplay
    {
        /// <summary>
        /// DOS's seven message scroll escape colours, \0 to \6, as palette 0 indices.
        ///
        /// UW1 writes them as consecutive immediates through its font colour global in the
        /// scroll parser, UW.EXE 0x3976E through 0x397AA. UW2 looks them up in a table at
        /// UW2.EXE 0x69407, bytes 75 68 01 02 21 50 48.
        ///
        /// Three of these used to be hardcoded hex here, which is why they drifted: #883E14
        /// for \1 where DOS has (132,68,32) in UW1 and (124,64,36) in UW2, so it did not vary
        /// by game at all; white for \3 where DOS has (252,252,252); and 0x76 for \0 in UW2
        /// where the table says 0x75. Only the black for \2 was right.
        /// </summary>
        private static readonly byte[] ScrollColoursUw1 = { 0x2E, 0x26, 0xF1, 0x60, 0xB4, 0xC4, 0xD4 };
        private static readonly byte[] ScrollColoursUw2 = { 0x75, 0x68, 0x01, 0x02, 0x21, 0x50, 0x48 };

        /// <summary>The colour for one of DOS's scroll escapes, falling back to \0.</summary>
        public static string ScrollColour(int escape)
        {
            var table = UWClass._RES == UWClass.GAME_UW2 ? ScrollColoursUw2 : ScrollColoursUw1;
            if (escape < 0 || escape >= table.Length) { escape = 0; }
            return PaletteLoader.ToBBCode(0, table[escape]);
        }

        public enum MessageDisplayMode
        {
            NormalMode = 0,
            TypedInput = 1,
            TemporaryMessage = 2
        }
        /// <summary>
        /// Player has been prompted with a [MORE] wait
        /// </summary>
        public static bool WaitingForMore = false;

        /// <summary>
        /// Player has to type an input string
        /// </summary>
        public static bool WaitingForTypedInput = false;

        public static bool WaitingForYesOrNo = false;
        public static string YesNoOption = "Yes";

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

        private IEnumerator AddLineWithMore(string newText, int Option, int Colour = 0)
        {
            if (newText.Trim() == "") { yield return 0; }
            newText = $"[color={ScrollColour(Colour)}]{newText}[/color]";
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
            yield return 0;
        }


        public IEnumerator AddLine(string newText, int Option, int Colour = 0, bool ForceMore = false)
        {
            if (newText.Trim() != "")
            {
                if (ForceMore)
                {//causes [more] to happen without splitting.
                    yield return AddLineWithMore(newText: newText, Option: Option, Colour: ConversationVM.UI_SAY);
                    WaitingForMore = true;
                    while (WaitingForMore)
                    {
                        yield return new WaitOneFrame();
                    }
                    LinePtr--;//overwrite last line
                }
                else
                {
                    var newTextSplit = newText.Split("[MORE]");
                    for (int i = 0; i <= newTextSplit.GetUpperBound(0); i++)
                    {
                        if (i > 0)
                        {
                            yield return AddLineWithMore(newText: "[MORE]", Option: Option, Colour: ConversationVM.UI_SAY);
                            WaitingForMore = true;
                            while (WaitingForMore)
                            {
                                yield return new WaitOneFrame();
                            }
                            LinePtr--;//overwrite last line
                        }
                        yield return AddLineWithMore(newText: newTextSplit[i], Option: Option, Colour: Colour);
                    }
                    yield return 0;
                }
            }


        }

        /// <summary>
        /// Outputs all text to the control
        /// </summary>
        public void UpdateMessageDisplay()
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
            //Replace special characters
            if (MessageDisplay.WaitingForYesOrNo)
            {
                uimanager.instance.TypedInput.Text = MessageDisplay.YesNoOption;
                output = output.Replace("{TYPEDINPUT}", MessageDisplay.YesNoOption);
            }
            else
            {
                if (MessageDisplay.WaitingForTypedInput)
                {
                    output = output.Replace("{TYPEDINPUT}", uimanager.instance.TypedInput.Text);
                }
                else if (uimanager.SaveDescriptionPromptActive)
                {
                    // The save description prompt keeps its own state rather than the shared
                    // WaitingForTypedInput, because it has to tell an empty commit from a
                    // cancel and the two mean opposite things.
                    output = output.Replace("{TYPEDINPUT}", uimanager.SaveDescriptionText);
                }
            }

            OutputControl[0].Text = output;
        }

        public IEnumerator AddText(string newText, int option = -1, int colour = 0)
        {
            if (newText.EndsWith("\n"))
            {//trims an ending new line
                newText = newText.Substring(0, newText.Length - 1);
            }
            int NoOfRowsNeeded = 0;
            //split by new lines
            newText = newText.Replace("\\m", " [MORE] ");
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
                    if (NoOfRowsNeeded >= Rows - 1)
                    {//To force a [more] to appear when text is longer than the control
                        yield return AddLine(newText: "[MORE]", Option: -1, Colour: ConversationVM.UI_SAY, ForceMore: true);
                        NoOfRowsNeeded = 0;
                    }
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
                                yield return AddLine(newText: LineToAdd, Option: option, Colour: colour);
                                NoOfRowsNeeded++;
                                LineToAdd = Words[WordPtr] + " "; //new word. new line.
                            }
                            length = LineToAdd.Length;
                        }//end while
                    }
                }//end loop
                if (LineToAdd != "")
                {//Store remaining data
                    NoOfRowsNeeded++;
                    yield return AddLine(newText: LineToAdd, Option: option, Colour: colour);
                }
            }
            yield return 0;
        }

        public IEnumerator RestoreLinesAfterWait(MessageScrollLine[] linesToRestore, float waittime)
        {
            yield return new WaitForSeconds(waittime);
            //restore
            for (int i = 0; i <= Lines.GetUpperBound(0); i++)
            {
                Lines[i] = new MessageScrollLine(linesToRestore[i].OptionNo, linesToRestore[i].LineText);
            }
            UpdateMessageDisplay();
            uimanager.MessageScrollIsTemporary = false;
            yield return 0;
        }

    }//end class
}//end namespace
