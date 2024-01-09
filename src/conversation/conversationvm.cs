using System.Diagnostics;
using System.Collections;
using Peaky.Coroutines;

namespace Underworld
{
    /// <summary>
    /// The main virtual machine for running the conversations
    /// </summary>
    public partial class ConversationVM : UWClass
    {

        public static bool InConversation=false;
        /// <summary>
        /// The currently referenced conversation.
        /// </summary>
        public static conversation conv;

        /// <summary>
        /// The instruction Pointer.
        /// </summary>
        public static int instrp = 0;

        /// <summary>
        /// The base pointer
        /// </summary>
        public static int basep = 0;

        public static IEnumerator RunConversationVM(uwObject npc, conversation _newConv)
        {
            conv=_newConv;
            bool finished = false;

            StackValues = new short[4096];
            stackptr = 200;
            result_register = 1;//default value
            instrp = 0;
            call_level = 1;
            // for (int i = 0; i < StackValues.GetUpperBound(0); i++)
            // {
            //     StackValues[i] = 0;
            // }

            while (!finished)
            {
                switch (conv.instuctions[instrp])
                {
                    case cnv_NOP:
                        {
                            break;
                        }


                    case cnv_OPADD:
                        {
                            int arg1 = Pop();
                            int arg2 = Pop();
                            Push(arg1+arg2);
                            break;
                        }


                    case cnv_OPMUL:
                        {
                            int arg1 = Pop();
                            int arg2 = Pop();
                            Push(arg1*arg2);
                            break;
                        }


                    case cnv_OPSUB:
                        {
                            int arg1 = Pop();
                            int arg2 = Pop();
                            Push(arg2 - arg1);
                            break;
                        }


                    case cnv_OPDIV:
                        {
                            int arg1 = Pop();
                            int arg2 = Pop();
                            //if (arg1==0)
                            //	throw ua_ex_div_by_zero;
                            Push(arg2 / arg1);
                            break;
                        }


                    case cnv_OPMOD:
                        {
                            int arg1 = Pop();
                            int arg2 = Pop();
                            //if (arg1==0)
                            //	throw ua_ex_div_by_zero;
                            Push(arg2 % arg1);
                            break;
                        }


                    case cnv_OPOR:
                        {
                            Push(Pop() | Pop());
                        }
                        break;

                    case cnv_OPAND:
                        {
                            Push(Pop() & Pop());
                        }
                        break;

                    case cnv_OPNOT:
                        {
                            if (Pop() == 0)
                            {
                                Push(1);
                            }
                            else
                            {
                                Push(0);
                            }
                            //Push(!Pop());
                            break;
                        }


                    case cnv_TSTGT:
                        {
                            int arg1 = Pop();
                            int arg2 = Pop();
                            if (arg2 > arg1)
                            {
                                Push(1);
                            }
                            else
                            {
                                Push(0);
                            }
                            //Push(arg2 > arg1);
                            break;
                        }


                    case cnv_TSTGE:
                        {
                            int arg1 = Pop();
                            int arg2 = Pop();
                            if (arg2 >= arg1)
                            {
                                Push(1);
                            }
                            else
                            {
                                Push(0);
                            }

                            //Push(arg2 >= arg1);
                            break;
                        }


                    case cnv_TSTLT:
                        {
                            int arg1 = Pop();
                            int arg2 = Pop();
                            if (arg2 < arg1)
                            {
                                Push(1);
                            }
                            else
                            {
                                Push(0);
                            }
                            //Push(arg2 < arg1);
                            break;
                        }


                    case cnv_TSTLE:
                        {
                            int arg1 = Pop();
                            int arg2 = Pop();
                            if (arg2 <= arg1)
                            {
                                Push(1);
                            }
                            else
                            {
                                Push(0);
                            }
                            //Push(arg2 <= arg1);
                            break;
                        }


                    case cnv_TSTEQ:
                        {
                            if (Pop() == Pop())
                            {
                                Push(1);
                            }
                            else
                            {
                                Push(0);
                            }
                            break;
                        }


                    case cnv_TSTNE:
                        {
                            //int val1 = Pop();
                            //int val2 = Pop();
                            if (Pop() != Pop())
                            //if ((val1) != (val2))
                            {
                                Push(1);
                            }
                            else
                            {
                                Push(0);
                            }
                            //Push(arg2 != arg1);
                            break;
                        }

                    case cnv_JMP:
                        {//Debug.Log("instr = " +instrp + " JMP to " +  conv.instuctions[instrp+1]);
                            instrp = conv.instuctions[instrp + 1] - 1;
                            break;
                        }

                    case cnv_BEQ:
                        {
                            //int origInstrp= instrp;
                            if (Pop() == 0)
                            {
                                instrp += conv.instuctions[instrp + 1];
                            }

                            else
                            {
                                instrp++;
                            }
                            //Debug.Log("BEQ to " + instrp + " at " + origInstrp);
                            break;
                        }

                    case cnv_BNE:
                        {
                            //int origInstrp= instrp;
                            if (Pop() != 0)
                            {
                                instrp += conv.instuctions[instrp + 1];
                            }
                            else
                            {
                                instrp++;
                            }
                            //Debug.Log("BNE to " + instrp + " at " + origInstrp);
                            break;
                        }

                    case cnv_BRA:
                        {
                            //int origInstrp= instrp;
                            instrp += conv.instuctions[instrp + 1];
                            //Debug.Log("BRA to " + instrp + " at " + origInstrp);
                            /*int offset = conv.instuctions[instrp+1];
                            if (offset >0)
                            {							
                                instrp += offset;	
                            }
                            else
                            {		
                                instrp += offset;
                            }*/
                            break;
                        }

                    case cnv_CALL: // local function
                        {
                            //int origInstrp= instrp;
                            // stack value points to next instruction after call
                            //Debug.Log("inst=" + instrp + "stack ptr" + stackptr + " new inst=" + (conv.instuctions[instrp+1]-1));
                            Push(instrp + 1);
                            instrp = conv.instuctions[instrp + 1] - 1;
                            call_level++;
                            //Debug.Log("CALL to " + instrp + " at " + origInstrp);
                            break;
                        }

                    case cnv_CALLI: // imported function
                        {
                            int arg1 = conv.instuctions[++instrp];
                            for (int i = 0; i <= conv.functions.GetUpperBound(0); i++)
                            {
                                if ((conv.functions[i].ID_or_Address == arg1) && (conv.functions[i].import_type == import_function))
                                {
                                    Debug.Print("Calling function  " + arg1 + " which is currently : " + conv.functions[i].functionName);
                                    //yield return StartCoroutine(run_imported_function(conv.functions[i], npc));
                                    //await run_imported_function();
                                    // yield return Coroutine.Run(
                                    //     run_imported_function(conv.functions[i], npc)
                                    //     ,main.instance);

                                     yield return run_imported_function(conv.functions[i], npc);
                                        
                                    break;
                                }
                            }
                            break;
                        }


                    case cnv_RET:
                        {
                            if (--call_level < 0)
                            {
                                // conversation ended
                                finished = true;
                            }
                            else
                            {
                                //Debug.Log("instr = " +instrp + " returning to " + arg1);
                                instrp = Pop();
                            }
                            break;
                        }


                    case cnv_PUSHI:
                        {
                            //Debug.Log("Instruction:" + instrp +" Pushing Immediate value :" +conv.instuctions[instrp+1] + " => " + stackptr);
                            Push(conv.instuctions[++instrp]);
                            break;
                        }


                    case cnv_PUSHI_EFF:
                        {
                            int offset = conv.instuctions[instrp + 1];
                            if (offset >= 0)
                            {
                                Push(basep + offset);
                            }
                            else
                            {
                                offset--; //to skip over base ptr;
                                Push(basep + offset);
                            }
                            instrp++;
                            break;
                        }


                    case cnv_POP:
                        {
                            Pop();
                            break;
                        }


                    case cnv_SWAP:
                        {
                            int arg1 = Pop();
                            int arg2 = Pop();
                            Push(arg1);
                            Push(arg2);
                            break;
                        }

                    case cnv_PUSHBP:
                        {
                            //Debug.Log("Instruction:" + instrp +" Pushing Base Ptr :" + basep + " => " + stackptr);							
                            Push(basep);
                            break;
                        }


                    case cnv_POPBP:
                        {
                            int arg1 = Pop();
                            basep = arg1;
                            break;
                        }

                    case cnv_SPTOBP:
                        {
                            basep = stackptr;
                            break;
                        }


                    case cnv_BPTOSP:
                        {
                            set_stackp(basep);
                            break;
                        }


                    case cnv_ADDSP:
                        {
                            int arg1 = Pop();
                            /// fill reserved stack space with dummy values
                            for (int i = 0; i <= arg1; i++)
                                Push(0);

                            //Set_stackp(stackptr+arg1);//This will probably cause problems down the line....
                            //Set_stackp(stackptr+arg1);
                            break;
                        }


                    case cnv_FETCHM:
                        {
                            //Debug.Log("Instruction:" + instrp +" Fetching address :" + TopValue + " => " + at(TopValue));
                            //at(arg1);
                            Push(at(Pop()));
                            break;
                        }


                    case cnv_STO:
                        {
                            int value = Pop();
                            int index = Pop();
                            // if (index < conv.NoOfImportedGlobals)
                            // {
                            //     PrintImportedVariable(index, value);
                            // }
                            Set(index, value);

                            break;
                        }


                    case cnv_OFFSET:
                        {
                            int arg1 = Pop();
                            int arg2 = Pop();
                            //Debug.Log("Offset " +arg1 + " & " + arg2  + "= " + (arg1+arg2-1));
                            arg1 += arg2 - 1;
                            //Debug.Log("Instruction:" + instrp +" Offset pushed : " + arg1 + " => " + stackptr);

                            Push(arg1);
                            break;
                        }


                    case cnv_START:
                        {
                            // do nothing
                            break;
                        }

                    case cnv_SAVE_REG:
                        {
                            result_register = Pop();
                            break;
                        }


                    case cnv_PUSH_REG:
                        {
                            Push(result_register);
                            break;
                        }


                    case cnv_EXIT_OP:
                        {
                            // finish processing (we still might be in some sub function)
                            finished = true;
                            break;
                        }


                    case cnv_SAY_OP:
                        {
                            int arg1 = Pop();
                            yield return say_op(arg1);
                            //await Say_op(arg1);                            
                            break;
                        }

                    case cnv_RESPOND_OP:
                        {// do nothing
                            Debug.Print("Respond_Op");
                            break;
                        }

                    case cnv_OPNEG:
                        {
                            Push(-Pop());
                            break;
                        }


                    default: // unknown opcode
                             //throw ua_ex_unk_opcode;
                        break;
                } //end switch

                // process next instruction or decide if finished.
                instrp++;
                if (instrp > conv.instuctions.GetUpperBound(0))
                {
                    finished = true;
                }
            } //end loop

            //should have a wait here
            yield return new WaitForSeconds(3);
            
            uimanager.EnableDisable(uimanager.instance.ConversationPanel,false);
            uimanager.instance.ConversationText.Text="";
            ConversationVM.InConversation = false;
            yield return null;
        }
    }
}//end namespace