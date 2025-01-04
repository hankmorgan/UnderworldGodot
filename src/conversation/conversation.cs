using System.Text;

namespace Underworld
{
    /// <summary>
    /// Basic conversation data structure
    /// </summary>
    public class Conversation : UWClass
    {
        public int conversationNo { get; set; }
        //0000   Int16   unknown, always seems to be 0x0828, or 28 08
        //0002   Int16   unknown, always 0x0000
        public int CodeSize { get; set; }  //0004   Int16   code size in number of instructions (16-bit words)
                                           ////0006   Int16   unknown, always 0x0000
                                           //0008   Int16   unknown, always 0x0000
        public int StringBlock { get; set; }//		000A   Int16   game strings block to use for conversation strings
        public int NoOfMemorySlots { get; set; }//	000C   Int16   number of memory slots reserved for variables (*)
        public int NoOfImportedGlobals { get; set; }//000E   Int16   number of imported globals (functions + variables)
                                                    //0010           start of imported functions list	
        public ImportedFunctions[] functions { get; set; }
        public short[] instuctions;


        public string PrintCode()
        {
            var sb = new StringBuilder();
            var instrp = 0;
            bool finished = false;
            while (!finished)
            {
                sb.Append($"\n{instrp}:");
                switch (instuctions[instrp])
                {
                    case ConversationVM.cnv_NOP:
                        {
                            sb.Append($"NOP");
                            break;
                        }
                    case ConversationVM.cnv_OPADD:
                        {
                            sb.Append("OPAPP");
                            break;
                        }
                    case ConversationVM.cnv_OPMUL:
                        {
                            sb.Append("OPMUL");
                            break;
                        }
                    case ConversationVM.cnv_OPSUB:
                        {
                            sb.Append("OPSUB");
                            break;
                        }
                    case ConversationVM.cnv_OPDIV:
                        {
                            sb.Append("OPDIV");
                            break;
                        }
                    case ConversationVM.cnv_OPMOD:
                        {
                            sb.Append("OPMOD");
                            break;
                        }
                    case ConversationVM.cnv_OPOR:
                        {
                            sb.Append("OPOR");
                        }
                        break;

                    case ConversationVM.cnv_OPAND:
                        {
                            sb.Append("OPAND");
                        }
                        break;

                    case ConversationVM.cnv_OPNOT:
                        {
                            sb.Append("OPNOT");
                            break;
                        }

                    case ConversationVM.cnv_TSTGT:
                        {
                            sb.Append("TSTGT");
                            break;
                        }

                    case ConversationVM.cnv_TSTGE:
                        {
                            sb.Append("TSTGE");
                            break;
                        }

                    case ConversationVM.cnv_TSTLT:
                        {
                            sb.Append("TSTLT");
                            break;
                        }

                    case ConversationVM.cnv_TSTLE:
                        {
                            sb.Append("TSTLE");
                            break;
                        }

                    case ConversationVM.cnv_TSTEQ:
                        {
                            sb.Append("TSTEQ");
                            break;
                        }

                    case ConversationVM.cnv_TSTNE:
                        {
                            sb.Append("TSTNE");
                            break;
                        }

                    case ConversationVM.cnv_JMP:
                        {
                            sb.Append($"JMP {instuctions[instrp + 1]}");
                            break;
                        }
                    case ConversationVM.cnv_BEQ:
                        {
                            sb.Append($"BEQ {instuctions[instrp + 1]+1}");
                            break;
                        }

                    case ConversationVM.cnv_BNE:
                        {
                            sb.Append($"BNE {instuctions[instrp + 1]}");
                            break;
                        }

                    case ConversationVM.cnv_BRA:
                        {
                            sb.Append($"BNE {instuctions[instrp + 1]}");
                            break;
                        }

                    case ConversationVM.cnv_CALL: // local function
                        {
                            sb.Append($"CALL {instuctions[instrp + 1]}");
                            break;
                        }

                    case ConversationVM.cnv_CALLI: // imported function
                        {
                            int arg1 = instuctions[++instrp];
                            for (int i = 0; i <= functions.GetUpperBound(0); i++)
                            {
                                if ((functions[i].ID_or_Address == arg1) && (functions[i].import_type == ConversationVM.import_function))
                                {
                                    //if (testing) { Debug.Print($"{instrp}:CALLI {arg1}"); }
                                    //Debug.Print("Calling function  " + arg1 + " which is currently : " + currentConversation.functions[i].importname);
                                    sb.Append($"CALLI {functions[i].importname}");
                                    break;
                                }
                            }
                            break;
                        }
                    case ConversationVM.cnv_RET:
                        {
                            sb.Append("RET");
                            break;
                        }
                    case ConversationVM.cnv_PUSHI:
                        {
                            sb.Append($"PUSHI {instuctions[++instrp]}");
                            break;
                        }
                    case ConversationVM.cnv_PUSHI_EFF:
                        {
                            sb.Append($"PUSHI_EFF BP+{instuctions[instrp + 1]}");
                            instrp++;
                            break;
                        }

                    case ConversationVM.cnv_POP:
                        {
                            sb.Append("POP");
                            break;
                        }
                    case ConversationVM.cnv_SWAP:
                        {
                            sb.Append("SWAP");
                            break;
                        }
                    case ConversationVM.cnv_PUSHBP:
                        {
                            sb.Append("PUSHBP");
                            break;
                        }
                    case ConversationVM.cnv_POPBP:
                        {
                            sb.Append("POPBP");
                            break;
                        }
                    case ConversationVM.cnv_SPTOBP:
                        {
                            sb.Append("SPTOBP");
                            break;
                        }
                    case ConversationVM.cnv_BPTOSP:
                        {
                            sb.Append("BPTOSP");
                            break;
                        }
                    case ConversationVM.cnv_ADDSP:
                        {
                            sb.Append("ADDSP");
                            break;
                        }

                    case ConversationVM.cnv_FETCHM:
                        {
                            sb.Append("FETCHM");
                            break;
                        }
                    case ConversationVM.cnv_STO:
                        {
                            sb.Append("STO");
                            break;
                        }
                    case ConversationVM.cnv_OFFSET:
                        {
                            sb.Append("OFFSET");
                            break;
                        }
                    case ConversationVM.cnv_START:
                        {
                            sb.Append("START");
                            break;
                        }
                    case ConversationVM.cnv_SAVE_REG:
                        {
                            sb.Append("SAVEREG");
                            break;
                        }
                    case ConversationVM.cnv_PUSH_REG:
                        {
                            sb.Append("PUSH_REG");
                            break;
                        }
                    case ConversationVM.cnv_EXIT_OP:
                        {
                            sb.Append("EXIT");
                            break;
                        }
                    case ConversationVM.cnv_SAY_OP:
                        {
                            sb.Append("SAYOP");
                            break;
                        }
                    case ConversationVM.cnv_RESPOND_OP:
                        {// do nothing
                            sb.Append("RESPOND_OP");
                            break;
                        }
                    case ConversationVM.cnv_OPNEG:
                        {
                            sb.Append("OPNEG");
                            break;
                        }
                    default: // unknown opcode
                        sb.Append("UNK!");
                        break;
                } //end switch

                // process next instruction or decide if finished.
                instrp++;
                if ((instrp > instuctions.GetUpperBound(0)) || (instrp < 0))
                {//if out of bounds safely end conversation
                 //Debug.Print($"Conversation has gone out of bounds!");
                    finished = true;
                }
            } //end loop
            return sb.ToString();
        }         

    }//end class
}//end namespace