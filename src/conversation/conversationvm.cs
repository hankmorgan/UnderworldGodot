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

		public static bool InConversation = false;
		/// <summary>
		/// The currently referenced conversation.
		/// </summary>
		public static conversation currentConversation;

		/// <summary>
		/// The instruction Pointer.
		/// </summary>
		public static int instrp = 0;

		/// <summary>
		/// The base pointer
		/// </summary>
		public static int basep = 0;

		public static IEnumerator RunConversationVM(uwObject talker)
		{
			bool testing = false;
			bool finished = false;

			while (!finished)
			{
				switch (currentConversation.instuctions[instrp])
				{
					case cnv_NOP:
						{
							if (testing) { Debug.Print($"{instrp}:NOP"); }
							break;
						}

					case cnv_OPADD:
						{
							int arg1 = Pop();
							int arg2 = Pop();
							if (testing) { Debug.Print($"{instrp}:Add {arg1},{arg2}"); }
							Push(arg1 + arg2);
							break;
						}


					case cnv_OPMUL:
						{
							int arg1 = Pop();
							int arg2 = Pop();
							if (testing) { Debug.Print($"{instrp}:Mul {arg1},{arg2}"); }
							Push(arg1 * arg2);
							break;
						}


					case cnv_OPSUB:
						{
							int arg1 = Pop();
							int arg2 = Pop();
							if (testing) { Debug.Print($"{instrp}:Sub {arg1},{arg2}"); }
							Push(arg2 - arg1);
							break;
						}


					case cnv_OPDIV:
						{
							int arg1 = Pop();
							int arg2 = Pop();
							//if (arg1==0)
							//	throw ua_ex_div_by_zero;
							if (testing) { Debug.Print($"{instrp}:Div {arg2},{arg1}"); }
							Push(arg2 / arg1);
							break;
						}


					case cnv_OPMOD:
						{
							int arg1 = Pop();
							int arg2 = Pop();
							if (testing) { Debug.Print($"{instrp}:Mod {arg1},{arg2}"); }
							//if (arg1==0)
							//	throw ua_ex_div_by_zero;
							Push(arg2 % arg1);
							break;
						}


					case cnv_OPOR:
						{
							var arg1 = Pop();
							var arg2 = Pop();
							if (testing) { Debug.Print($"{instrp}:Or {arg1},{arg2}"); }
							Push(arg1 | arg2);
						}
						break;

					case cnv_OPAND:
						{
							var arg1 = Pop();
							var arg2 = Pop();
							if (testing) { Debug.Print($"{instrp}:AND {arg1},{arg2}"); }
							Push(arg1 & arg2);
						}
						break;

					case cnv_OPNOT:
						{
							if (testing) { Debug.Print($"NOT"); }
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
							if (testing) { Debug.Print($"{instrp}:TSTGT {arg1},{arg2}"); }
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
							if (testing) { Debug.Print($"{instrp}:TSTGE {arg1},{arg2}"); }
							if (arg2 >= arg1)
							{
								Push(1);
							}
							else
							{
								Push(0);
							}
							break;
						}


					case cnv_TSTLT:
						{
							int arg1 = Pop();
							int arg2 = Pop();
							if (testing) { Debug.Print($"{instrp}:TSTLT {arg1},{arg2}"); }
							if (arg2 < arg1)
							{
								Push(1);
							}
							else
							{
								Push(0);
							}
							break;
						}


					case cnv_TSTLE:
						{
							int arg1 = Pop();
							int arg2 = Pop();
							if (testing) { Debug.Print($"{instrp}:TSTLE {arg1},{arg2}"); }
							if (arg2 <= arg1)
							{
								Push(1);
							}
							else
							{
								Push(0);
							}
							break;
						}


					case cnv_TSTEQ:
						{
							int arg1 = Pop();
							int arg2 = Pop();
							if (testing) { Debug.Print($"{instrp}:TSTEQ {arg1},{arg2}"); }
							if (arg1 == arg2)
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
							int arg1 = Pop();
							int arg2 = Pop();
							if (testing) { Debug.Print($"{instrp}:TSTNE {arg1},{arg2}"); }
							if (arg1 != arg2)
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
							instrp = currentConversation.instuctions[instrp + 1] - 1;
							if (testing) { Debug.Print($"{instrp}:JMP {instrp}"); }
							break;
						}

					case cnv_BEQ:
						{
							if (Pop() == 0)
							{
								instrp += currentConversation.instuctions[instrp + 1];
							}
							else
							{
								instrp++;
							}
							if (testing) { Debug.Print($"{instrp}:BEQ {instrp}"); }
							break;
						}

					case cnv_BNE:
						{
							if (Pop() != 0)
							{
								instrp += currentConversation.instuctions[instrp + 1];
							}
							else
							{
								instrp++;
							}
							if (testing) { Debug.Print($"{instrp}:BNE {instrp}"); }
							break;
						}

					case cnv_BRA:
						{
							//int origInstrp= instrp;
							instrp += currentConversation.instuctions[instrp + 1];
							if (testing) { Debug.Print($"{instrp}:BRA {instrp}"); }
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
							instrp = currentConversation.instuctions[instrp + 1] - 1;
							if (testing) { Debug.Print($"{instrp}:Call {instrp}"); }
							call_level++;
							break;
						}

					case cnv_CALLI: // imported function
						{
							int arg1 = currentConversation.instuctions[++instrp];
							for (int i = 0; i <= currentConversation.functions.GetUpperBound(0); i++)
							{
								if ((currentConversation.functions[i].ID_or_Address == arg1) && (currentConversation.functions[i].import_type == import_function))
								{
									if (testing) { Debug.Print($"{instrp}:CALLI {arg1}"); }
									Debug.Print("Calling function  " + arg1 + " which is currently : " + currentConversation.functions[i].importname);
									yield return run_imported_function(currentConversation.functions[i], talker);
									break;
								}
							}
							break;
						}


					case cnv_RET:
						{

							if (--call_level < 0)
							{
								if (testing) { Debug.Print($"{instrp}:RET (Finished)"); }
								// conversation ended
								finished = true;
							}
							else
							{
								instrp = Pop();
								if (testing) { Debug.Print($"{instrp}:RET {instrp}"); }
							}
							break;
						}


					case cnv_PUSHI:
						{
							//Debug.Log("Instruction:" + instrp +" Pushing Immediate value :" +conv.instuctions[instrp+1] + " => " + stackptr);
							if (testing) { Debug.Print($"{instrp}:PUSHI {currentConversation.instuctions[instrp + 1]}"); }
							Push(currentConversation.instuctions[++instrp]);
							break;
						}


					case cnv_PUSHI_EFF:
						{
							// int offset = conv.instuctions[instrp + 1];
							// if (offset >= 0)
							// {
							//     Push(basep + offset);
							// }
							// else
							// {
							//     offset--; //to skip over base ptr;  //TODO figure out if this is correct behaviour
							//     Push(basep + offset);
							// }
							if (testing) { Debug.Print($"{instrp}:PUSHI_EFF {basep + currentConversation.instuctions[instrp + 1]}"); }
							Push(basep + currentConversation.instuctions[instrp + 1]);
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
							if (testing) { Debug.Print($"{instrp}:SWAP {arg1},{arg2}"); }
							Push(arg1);
							Push(arg2);
							break;
						}

					case cnv_PUSHBP:
						{
							//Debug.Print("Instruction:" + instrp +" Pushing Base Ptr :" + basep + " => " + stackptr);	
							if (testing) { Debug.Print($"{instrp}:PUSHBP {basep}"); }
							Push(basep);
							break;
						}


					case cnv_POPBP:
						{
							if (stackptr < 0)
							{
								Debug.Print($"{instrp} StackPtr is {stackptr}. Possible UW1 bug in some conversations. Exiting conversation!");
								finished = true;
							}
							else
							{
								//Debug.Print("Instruction:" + instrp +" Popping Base Ptr :" + basep + " => " + stackptr);	
								int arg1 = Pop();
								if (testing) { Debug.Print($"{instrp}:POPBP {arg1}"); }
								basep = arg1;
							}
							break;
						}

					case cnv_SPTOBP:
						{
							//Debug.Print("Instruction:" + instrp +" Setting Base Ptr to Stack :" + basep + " => " + stackptr);
							if (testing) { Debug.Print($"{instrp}:SPTOBP {stackptr}"); }
							basep = stackptr;
							break;
						}


					case cnv_BPTOSP:
						{
							//Debug.Print("Instruction:" + instrp +" Setting Stack Ptr to BaseP :" + basep + " => " + stackptr);
							if (testing) { Debug.Print($"{instrp}:BPTOSP {basep}"); }
							set_stackp(basep);
							break;
						}


					case cnv_ADDSP:
						{
							int arg1 = at(stackptr);

							if (testing) { Debug.Print($"{instrp}:ADDSP {arg1}"); }
							/// fill reserved stack space with dummy values
							//for (int i = 0; i <= arg1; i++)
							//	Push(0);

							set_stackp(stackptr + arg1 - 1);

							break;
						}


					case cnv_FETCHM:
						{
							var address = Pop();
							var arg1 = at(address);
							var varname = GetVariableNameAtAddress(address);
							if (varname!="")
							{
								Debug.Print ($"Fetching {varname} with value {arg1}");
							}
							if (testing) { Debug.Print($"{instrp}:FETCHM {arg1}"); }
							Push(arg1);
							break;
						}


					case cnv_STO:
						{
							int value = Pop();
							int index = Pop();
							if (testing) { Debug.Print($"{instrp}:STO {index} {value}"); }
							Set(index, value);

							break;
						}


					case cnv_OFFSET:
						{
							//int arg1 = Pop();
							int arg1 = at(stackptr);
							int arg2 = at(stackptr - 1);//Pop();
							stackptr--;
							if (testing) { Debug.Print($"{instrp}:Offset {arg1} {arg2}"); }
							arg1 += arg2 - 1;  //why -1
							Set(stackptr, arg1);
							//Push(arg1);
							break;
						}


					case cnv_START:
						{
							if (testing) { Debug.Print($"{instrp}:Start"); }
							break;
						}

					case cnv_SAVE_REG:
						{
							var arg1 = Pop();
							if (testing) { Debug.Print($"{instrp}:Save_Reg {arg1}"); }
							result_register = arg1;
							break;
						}


					case cnv_PUSH_REG:
						{
							if (testing) { Debug.Print($"{instrp}:Push_Reg {result_register}"); }
							Push(result_register);
							break;
						}


					case cnv_EXIT_OP:
						{
							// finish processing (we still might be in some sub function)
							if (testing) { Debug.Print($"{instrp}:EXIT"); }
							finished = true;
							break;
						}


					case cnv_SAY_OP:
						{
							int arg1 = Pop();
							if (testing) { Debug.Print($"{instrp}:SAY_OP {arg1} {getString(arg1)}"); }
							yield return say_op(arg1);
							break;
						}

					case cnv_RESPOND_OP:
						{// do nothing
							if (testing) { Debug.Print($"{instrp}:RESPOND"); }
							Debug.Print("Respond_Op");
							break;
						}

					case cnv_OPNEG:
						{
							if (testing) { Debug.Print($"{instrp}:NEG"); }
							Push(-Pop());
							break;
						}


					default: // unknown opcode
						if (testing) { Debug.Print($"{instrp}:UNKNOWN"); }
						//throw ua_ex_unk_opcode;
						break;
				} //end switch

				// process next instruction or decide if finished.
				instrp++;
				if ((instrp > currentConversation.instuctions.GetUpperBound(0)) || (instrp<0))
				{//if out of bounds safely end conversation
					Debug.Print($"Conversation has gone out of bounds!");
					finished = true;
				}
			} //end loop            

			//should have a wait here
			yield return new WaitForSeconds(3);

			ExitConversation(talker, currentConversation);
			yield return null;
		}

		private static void ExitConversation(uwObject talker, conversation conv)
		{
			ExportVariables(talker);
			uimanager.EnableDisable(uimanager.instance.ConversationPanelUW1, false);
			uimanager.EnableDisable(uimanager.instance.ConversationPanelUW2, false);
			if (_RES == GAME_UW2)
			{//restore background and other ui elements hidden
				uimanager.instance.mainwindowUW2.Texture = uimanager.bitmaps.LoadImageAt(BytLoader.UW2ThreeDWin_BYT, true);
				for (int i = 0; i <= uimanager.instance.SelectedRunes.GetUpperBound(0); i++)
				{
					uimanager.EnableDisable(uimanager.instance.SelectedRunes[i], true);
				}
				for (int i = 0; i <= uimanager.instance.InteractionButtonsUW2.GetUpperBound(0); i++)
				{//disable interaction buttons
					uimanager.EnableDisable(uimanager.instance.InteractionButtonsUW2[i], true);
				}
				uimanager.EnableDisable(uimanager.instance.CompassPanelUW2,true);
				uimanager.EnableDisable(uimanager.instance.PowerGemUW2,true);
				uimanager.instance.messageScrollUW2.Size = new Godot.Vector2(840,140);
				uimanager.instance.scroll.Columns = 44;				
			}
			for (int i = 0; i < uimanager.NoOfTradeSlots; i++)
			{//move remaining objects in the trade area to the players tile.
				var objindex = uimanager.GetPlayerTradeSlot(i, true);
				if (objindex != -1)
				{
					pickup.Drop(
						index: objindex,
						objList: UWTileMap.current_tilemap.LevelObjects,
						dropPosition: main.gamecam.Position,
						tileX: playerdat.tileX,
						tileY: playerdat.tileY);
				}
			}
			uimanager.instance.convo.Clear();
			InConversation = false;
			main.gamecam.Set("MOVE", true);
		}

		private static void InitialiseConversationMemory()
		{
			StackValues = new short[4096];
			stackptr = 200;
			result_register = 0;
			instrp = 0;
			call_level = 1;
		}
	}
}//end namespace
