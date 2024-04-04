using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Underworld
{
    /// <summary>
    /// Code for Input and output relating to conversation VM.
    /// </summary>
    public partial class ConversationVM : UWClass
    {
        public static int PlayerNumericAnswer;
        public static bool WaitingForInput;

        /// <summary>
        /// Gets the specified string for the currently referenced conversation.
        /// </summary>
        /// <param name="stringno"></param>
        /// <returns></returns>
        public static string getString(int stringno, bool replaceValues = false)
        {
            if (replaceValues)
            {
                return TextSubstitute(GameStrings.GetString(currentConversation.StringBlock, stringno));
            }
            else
            {
                return GameStrings.GetString(currentConversation.StringBlock, stringno);
            }
        }


        /// <summary>
        /// For subsituting values into strings
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string TextSubstitute(string input)
        {
            //X: source of variable to substitute, one of these: GSP
            //G: game global variable
            //S: stack variable
            //P: pointer variable
            //Y: type of variable, one of these: SI
            //S: value is a string number into current string block
            //I: value is an integer value
            //<num>: decimal value
            //<extension>: format: C<number>: use array index <number-1> offset from the initial source value.
            if (!input.Contains("@"))
            {
                return input;
            }
            string RegExForFindingReplacements = "([@][GSP][SI])(-*[0-9]*)([S][I])?([0-9]*)?([C][0-9]*)?";

            Debug.Print($"Doing string replacement on line {input}");
            MatchCollection matches = Regex.Matches(input, RegExForFindingReplacements);
            
            for (int sm = 0; sm < matches.Count; sm++)
            {
                string ReplacementString = matches[sm].Value;
                if (matches[sm].Success)
                {
                    string ReplacementType = "";
                    int ReplacementValue = 0;
                    int OffsetValue = 0;
                    string FoundString = "";
                    int ArrayOffset = 0;
                    bool SIOffset = false;
                    for (int sg = 0; sg < matches[sm].Groups.Count; sg++)
                    {
                        if (matches[sm].Groups[sg].Success)
                        {
                            switch (sg)
                            {
                                case 1: //Replacement Type
                                    ReplacementType = matches[sm].Groups[sg].Value; break;
                                case 2: //Replacement value
                                    {
                                        if (int.TryParse(matches[sm].Groups[sg].Value, out int val))
                                        {
                                            ReplacementValue = val;
                                        }
                                        else
                                        {
                                            ReplacementValue = 0;
                                        }
                                        break;
                                    }
                                case 3: //Offset Type (should only be SI?)
                                    string OffsetType = matches[sm].Groups[sg].Value;
                                    SIOffset = true;
                                    //Debug.Print($"Unimplemented SI Offset {OffsetType}");
                                    break;
                                case 4: //Offset value
                                    {
                                        if (int.TryParse(matches[sm].Groups[sg].Value, out int val))
                                        {
                                            if (SIOffset)
                                            {
                                                OffsetValue = at(basep+ val) -1;
                                            }
                                            else
                                            {
                                                OffsetValue = val;
                                            }
                                            
                                        }
                                        else
                                        {
                                            OffsetValue = 0;
                                        }
                                        break;
                                    }
                                case 5: //c2 array offset
                                    {
                                        string CModifier = matches[sm].Groups[sg].Value;
                                        CModifier = CModifier.Replace("C", "");
                                        if (int.TryParse(CModifier, out int val))
                                        {
                                            ArrayOffset = val - 1; // array is 1-based
                                        }
                                        break;
                                    }
                            }
                        }
                    }

                    //Now replace
                    switch (ReplacementType)
                    {
                        case "@GS": //Global string.
                            {
                                //FoundString = GameStrings.GetString(0x125, at(ReplacementValue)); //getString(at(ReplacementValue));
                                FoundString = getString(at(ReplacementValue+OffsetValue));
                                break;
                            }
                        case "@GI": //Global integer
                            {
                                //Debug.Print("@GI String replacement (" + ReplacementValue + ")");//Depending on the presense of the 'C' extension a offset may be applied.
                                FoundString = at(ReplacementValue + ArrayOffset).ToString();
                                break;
                            }
                        case "@SS": //Stack string
                            {
                                if (OffsetValue != 0)
                                {
                                    int actualptr = at(basep + OffsetValue);
                                    FoundString = getString(at(basep + actualptr));
                                }
                                else
                                {
                                    FoundString = getString(at(basep + ReplacementValue));
                                }
                                break;
                            }
                        case "@SI": //Stack integer
                            {
                                FoundString = at(basep + ReplacementValue + ArrayOffset).ToString();
                                break;
                            }

                        case "@PS": //Pointer string
                            {
                                FoundString = getString(at(at(basep + ReplacementValue)));
                                break;
                            }
                        case "@PI": //Pointer integer
                            {
                                if (ReplacementValue < 0)
                                {
                                    FoundString = at(at(basep + ReplacementValue - 1)).ToString();//-1 for params
                                }
                                else
                                {
                                    FoundString = at(at(basep + ReplacementValue)).ToString();
                                }
                                break;
                            }
                    }
                    if (FoundString != "")
                    {
                        input = input.Replace(ReplacementString, FoundString);
                    }
                }
            }
            return input;
        }
    }//end class
}//end namespace