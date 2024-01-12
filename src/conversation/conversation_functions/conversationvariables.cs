using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        /// <summary>
        /// Import the variables from bglobals and the characters.
        /// </summary>
        /// <param name="npc"></param>
        public static void ImportVariables(uwObject npc, conversation _newConv)
        {
            //Copy the stored values from bglobal.dat files first.
            //This may be overwritten by the imported variables below.
            for (int c = 0; c <= bglobal.bGlobals.GetUpperBound(0); c++)
            {
                if (npc.npc_whoami == bglobal.bGlobals[c].ConversationNo)
                {
                    for (int x = 0; x <= bglobal.bGlobals[c].Globals.GetUpperBound(0); x++)
                    {
                        Debug.Print($"Importing {bglobal.bGlobals[c].Globals[x]} to {x}" );
                        Set(x, bglobal.bGlobals[c].Globals[x]);
                    }
                    break;
                }
            }
            //imported variables
            //Add in the imported variables at the addresses specified
            for (int i = 0; i <= _newConv.functions.GetUpperBound(0); i++)
            {
                if (_newConv.functions[i].import_type == import_variable)
                {
                    int address = _newConv.functions[i].ID_or_Address;
                    Debug.Print($"Importing {_newConv.functions[i].importname} to {address}");
                }
            }



        }
    }//end class
}// end namespace