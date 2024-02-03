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
            //<extension>: format: C<number>: use array index <number>

            string RegExForFindingReplacements = "([@][GSP][SI])([0-9]*)([S][I])?([0-9]*)?([C][0-9]*)?";
            //"(@)([GSP])([SI])([0-9])*([S][I][0-9]*)*([C][0-9])*";
            //string RegExForFindingReplacementsTypes = "(@)([GSP])([SI])";

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
                                    break;
                                case 4: //Offset value
                                    {
                                        if (int.TryParse(matches[sm].Groups[sg].Value, out int val))
                                        {
                                            OffsetValue = val;
                                        }
                                        else
                                        {
                                            OffsetValue = 0;
                                        }
                                        break;
                                    }
                                case 5: //formatting specifier (unimplemented
                                    string formatting = matches[sm].Groups[sg].Value;
                                    break;
                            }
                        }
                        //Debug.Log("group " + matches[sm].Groups[sg].Success + " " + matches[sm].Groups[sg].Value);
                    }



                    //Now replace
                    switch (ReplacementType)
                    {
                        case "@GS": //Global string.
                            {
                                FoundString =getString(at(ReplacementValue));
                                break;
                            }
                        case "@GI": //Global integer
                            {
                                //Debug.Print("@GI String replacement (" + ReplacementValue + ")");//Sometimes this works with val+1 other times val!!
                                FoundString = at(ReplacementValue).ToString();
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
                            {//TODO: this +1 behaves inconsistently. UW1 or UW2 difference???
                                // if (_RES == GAME_UW2)
                                // {
                                    FoundString = at(basep + ReplacementValue).ToString();
                                // }
                                // else
                                // {
                                //     FoundString = at(basep + ReplacementValue + 1).ToString();//Skip over 1 for basepointer	
                                // }

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