namespace Underworld
{
    /// <summary>
    /// Imported functions and memory headers from the conv.ark file
    /// </summary>
    public class ConversationImports
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
}