using System.Diagnostics;
namespace Underworld
{
    public class cnvArkLoader : Loader
    {
        public static bool Loaded = false;
        public static void LoadCnvArkUW1(string cnv_ark_path)
        {
            if (ReadStreamFile(cnv_ark_path, out byte[] cnv_ark))
            {
                int NoOfConversations = (int)getAt(cnv_ark, 0, 16);
                ConversationVM.conversations = new Conversation[NoOfConversations];
                for (int i = 0; i < NoOfConversations; i++)
                {
                    ConversationVM.conversations[i]= new();
                    int add_ptr = (int)getAt(cnv_ark, 2 + i * 4, 32);
                    if (add_ptr != 0)
                    {
                        /*
                        0000   Int16   unknown, always seems to be 0x0828, or 28 08
                        0002   Int16   unknown, always 0x0000
                        0004   Int16   code size in number of instructions (16-bit words)
                        0006   Int16   unknown, always 0x0000
                        0008   Int16   unknown, always 0x0000
                        000A   Int16   game strings block to use for conversation strings
                        000C   Int16   number of memory slots reserved for variables (*)
                        000E   Int16   number of imported globals (functions + variables)
                        0010           start of imported functions list
                        */
                        ConversationVM.conversations[i].conversationNo= i;
                        ConversationVM.conversations[i].CodeSize = (int)getAt(cnv_ark, add_ptr + 0x4, 16);
                        ConversationVM.conversations[i].StringBlock = (int)getAt(cnv_ark, add_ptr + 0xA, 16);
                        ConversationVM.conversations[i].NoOfMemorySlots = (int)getAt(cnv_ark, add_ptr + 0xC, 16);
                        ConversationVM.conversations[i].NoOfImportedGlobals = (int)getAt(cnv_ark, add_ptr + 0xE, 16);
                        ConversationVM.conversations[i].functions = new ConversationImports[ConversationVM.conversations[i].NoOfImportedGlobals];
                        int funcptr = add_ptr + 0x10;
                        for (int f = 0; f < ConversationVM.conversations[i].NoOfImportedGlobals; f++)
                        {
                            ConversationVM.conversations[i].functions[f] = new();
                            /*0000   Int16   length of function name
                            0002   n*char  name of function
                            n+02   Int16   ID (imported func.) / memory address (variable)
                            n+04   Int16   unknown, always seems to be 1
                            n+06   Int16   import type (0x010F=variable, 0x0111=imported func.)
                            n+08   Int16   return type (0x0000=void, 0x0129=int, 0x012B=string)*/
                            int len = (int)getAt(cnv_ark, funcptr, 16);
                            for (int j = 0; j < len; j++)
                            {                                
                                ConversationVM.conversations[i].functions[f].importname += (char)getAt(cnv_ark, funcptr + 2 + j, 8);
                            }
                            ConversationVM.conversations[i].functions[f].ID_or_Address = (int)getAt(cnv_ark, funcptr + len + 2, 16);
                            ConversationVM.conversations[i].functions[f].import_type = (int)getAt(cnv_ark, funcptr + len + 6, 16);
                            ConversationVM.conversations[i].functions[f].return_type = (int)getAt(cnv_ark, funcptr + len + 8, 16);
                            funcptr += len + 10;
                        }
                        ConversationVM.conversations[i].instuctions = new short[ConversationVM.conversations[i].CodeSize];
                        int counter = 0;
                        for (int c = 0; c < ConversationVM.conversations[i].CodeSize * 2; c += 2)
                        {
                            ConversationVM.conversations[i].instuctions[counter++] = (short)getAt(cnv_ark, funcptr + c, 16);
                        }
                    }
                    // if (ConversationVM.conversations[i].instuctions!=null)
                    // {
                    //     System.IO.File.WriteAllText( $"c:\\temp\\UW1_{i}_convo.txt", ConversationVM.conversations[i].PrintCode());
                    // }                    
                }
                //var json = System.Text.Json.JsonSerializer.Serialize(ConversationVM.conversations);
                //File.WriteAllText("c:\\temp\\conversations.txt", json);
                
                Loaded = true;
            }
        }

        /// <summary>
        /// Loads the cnv ark file and parses it to initialise the conversation headers and imported functions
        /// </summary>
        /// <param name="cnv_ark_path">Cnv ark path.</param>
        public static void LoadCnvArkUW2(string cnv_ark_path)
        {
            int address_pointer = 2;
            if (!ReadStreamFile(cnv_ark_path, out byte[] tmp_ark))
            {
                //Debug.Log("unable to load uw2 conv ark");           
                return;
            }

            int NoOfConversations = (int)getAt(tmp_ark, 0, 32);

            ConversationVM.conversations = new Conversation[NoOfConversations];

            for (int i = 0; i < NoOfConversations; i++)
            {
                int compressionFlag = (int)getAt(tmp_ark, address_pointer + (NoOfConversations * 4), 32);
                int isCompressed = (compressionFlag >> 1) & 0x01;
                int add_ptr = (int)getAt(tmp_ark, address_pointer, 32);
                if (add_ptr != 0)
                {
                    if (isCompressed == 1)
                    {
                        int datalen = 0;
                        byte[] cnv_ark =  DataLoader.unpackUW2(tmp_ark, add_ptr, ref datalen);
                        add_ptr = 0;
                        ConversationVM.conversations[i] = new();
                        ConversationVM.conversations[i].conversationNo= i;
                        /*
                         0000   Int16   unknown, always seems to be 0x0828, or 28 08
                         0002   Int16   unknown, always 0x0000
                         0004   Int16   code size in number of instructions (16-bit words)
                         0006   Int16   unknown, always 0x0000
                         0008   Int16   unknown, always 0x0000
                         000A   Int16   game strings block to use for conversation strings
                         000C   Int16   number of memory slots reserved for variables (*)
                         000E   Int16   number of imported globals (functions + variables)
                         0010           start of imported functions list
                         */
                        ConversationVM.conversations[i].CodeSize = (int)getAt(cnv_ark, add_ptr + 0x4, 16);
                        ConversationVM.conversations[i].StringBlock = (int)getAt(cnv_ark, add_ptr + 0xA, 16);
                        ConversationVM.conversations[i].NoOfMemorySlots = (int)getAt(cnv_ark, add_ptr + 0xC, 16);
                        if (ConversationVM.conversations[i].NoOfMemorySlots>0)
                        {
                            Debug.Print($"Memory slots for {i} is > 0 ");
                        }
                        ConversationVM.conversations[i].NoOfImportedGlobals = (int)getAt(cnv_ark, add_ptr + 0xE, 16);
                        ConversationVM.conversations[i].functions = new ConversationImports[ConversationVM.conversations[i].NoOfImportedGlobals];
                        long funcptr = add_ptr + 0x10;
                        for (int f = 0; f < ConversationVM.conversations[i].NoOfImportedGlobals; f++)
                        {
                            ConversationVM.conversations[i].functions[f]=new();
                            /*0000   Int16   length of function name
                             0002   n*char  name of function
                             n+02   Int16   ID (imported func.) / memory address (variable)
                             n+04   Int16   unknown, always seems to be 1
                             n+06   Int16   import type (0x010F=variable, 0x0111=imported func.)
                             n+08   Int16   return type (0x0000=void, 0x0129=int, 0x012B=string)
                             */
                            int len = (int)getAt(cnv_ark, funcptr, 16);
                            for (int j = 0; j < len; j++)
                            {                                
                                ConversationVM.conversations[i].functions[f].importname += (char)getAt(cnv_ark, funcptr + 2 + j, 8);
                            }
                            ConversationVM.conversations[i].functions[f].ID_or_Address = (int)getAt(cnv_ark, funcptr + len + 2, 16);
                            ConversationVM.conversations[i].functions[f].import_type = (int)getAt(cnv_ark, funcptr + len + 6, 16);
                            ConversationVM.conversations[i].functions[f].return_type = (int)getAt(cnv_ark, funcptr + len + 8, 16);
                            funcptr += len + 10;
                        }
                        ConversationVM.conversations[i].instuctions = new short[ConversationVM.conversations[i].CodeSize];
                        int counter = 0;
                        for (int c = 0; c < ConversationVM.conversations[i].CodeSize * 2; c += 2)
                        {
                            ConversationVM.conversations[i].instuctions[counter++] = (short)getAt(cnv_ark, funcptr + c, 16);
                        }

                        // if (ConversationVM.conversations[i].instuctions!=null)
                        // {
                        //     System.IO.File.WriteAllText( $"c:\\temp\\UW1_{i}_convo.txt", ConversationVM.conversations[i].PrintCode());
                        // }   
                    }
                    else
                    {
                        Debug.Print("uncompressed flag in cnv.ark");
                    }
                }
                address_pointer += 4;
            }
            // var json = System.Text.Json.JsonSerializer.Serialize(ConversationVM.conversations);
            // File.WriteAllText("c:\\temp\\conversations.txt", json);
        }
    }//endclass
}//end namespace