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
        public class ImportedFunctions
        {
            //0000   Int16   length of function name
            //0002   n*char  name of function
            public string importname {get;set;}
            //n+02   Int16   ID (imported func.) / memory address (variable)
            public int ID_or_Address {get;set;}
            //	n+04   Int16   unknown, always seems to be 1
            public int import_type {get;set;}//n+06   Int16   import type (0x010F=variable, 0x0111=imported func.)
            public int return_type {get;set;} //n+08   Int16   return type (0x0000=void, 0x0129=int, 0x012B=string)
        };


        public class conversation
        {
            public int conversationNo {get;set;}
            //0000   Int16   unknown, always seems to be 0x0828, or 28 08
            //0002   Int16   unknown, always 0x0000
            public int CodeSize { get; set; }  //0004   Int16   code size in number of instructions (16-bit words)
                                ////0006   Int16   unknown, always 0x0000
                                //0008   Int16   unknown, always 0x0000
            public int StringBlock { get; set; }//		000A   Int16   game strings block to use for conversation strings
            public int NoOfMemorySlots { get; set; }//	000C   Int16   number of memory slots reserved for variables (*)
            public int NoOfImportedGlobals { get; set; }//000E   Int16   number of imported globals (functions + variables)
                                        //0010           start of imported functions list	
            public ImportedFunctions[] functions {get;set;}
            public short[] instuctions;
        };

        public static conversation[] conversations  { get; set; }

    }
}//end namespace