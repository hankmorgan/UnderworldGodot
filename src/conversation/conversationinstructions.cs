namespace Underworld
{
    /// <summary>
    /// Conversation VM .ark data and commands
    /// </summary>
    public partial class ConversationVM:UWClass
    {

        /// <summary>
        /// Imported function and memory data from the conv.ark file
        /// </summary>
        public struct ImportedFunctions
        {
            //0000   Int16   length of function name
            //0002   n*char  name of function
            public string functionName;
            //n+02   Int16   ID (imported func.) / memory address (variable)
            public int ID_or_Address;
            //	n+04   Int16   unknown, always seems to be 1
            public int import_type;//n+06   Int16   import type (0x010F=variable, 0x0111=imported func.)
            public int return_type; //n+08   Int16   return type (0x0000=void, 0x0129=int, 0x012B=string)
        };


        public struct conversation
        {
            //0000   Int16   unknown, always seems to be 0x0828, or 28 08
            //0002   Int16   unknown, always 0x0000
            public int CodeSize;  //0004   Int16   code size in number of instructions (16-bit words)
                                ////0006   Int16   unknown, always 0x0000
                                //0008   Int16   unknown, always 0x0000
            public int StringBlock;//		000A   Int16   game strings block to use for conversation strings
            public int NoOfMemorySlots;//	000C   Int16   number of memory slots reserved for variables (*)
            public int NoOfImportedGlobals;//000E   Int16   number of imported globals (functions + variables)
                                        //0010           start of imported functions list	
            public ImportedFunctions[] functions;
            public short[] instuctions;
        };

        public static conversation[] conversations;

    }
}//end namespace