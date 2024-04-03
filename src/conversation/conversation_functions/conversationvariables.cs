using System;
using System.Diagnostics;

namespace Underworld
{
	public partial class ConversationVM : UWClass
	{

		/// <summary>
		/// Import the variables from bglobals and the characters.
		/// </summary>
		/// <param name="npc"></param>
		public static void ImportVariables(uwObject npc)
		{
			//Copy the stored values from bglobal.dat files first.
			//This may be overwritten by the imported variables below.
			for (int c = 0; c <= bglobal.bGlobals.GetUpperBound(0); c++)
			{
				if (npc.npc_whoami == bglobal.bGlobals[c].ConversationNo)
				{
					for (int x = 0; x <= bglobal.bGlobals[c].Globals.GetUpperBound(0); x++)
					{
						Debug.Print($"Importing {bglobal.bGlobals[c].Globals[x]} to {x}");
						Set(x, bglobal.bGlobals[c].Globals[x]);
					}
					break;
				}
			}
			//imported variables
			//Add in the imported variables at the addresses specified
			for (int i = 0; i <= currentConversation.functions.GetUpperBound(0); i++)
			{
				if (currentConversation.functions[i].import_type == import_variable)
				{
					var address = currentConversation.functions[i].ID_or_Address;

					int valueToImport = 0;
					switch (currentConversation.functions[i].importname.ToLower())
					{
						case "npc_whoami":
							{
								valueToImport = npc.npc_whoami; break;
							}
						case "npc_hunger": //bit 7 only
							{
								valueToImport = (npc.npc_hunger & 0x80) >> 7;
								if (valueToImport == 1)
								{
									valueToImport = 16;
								}
								else
								{
									valueToImport = 192;
								}
								break;
							}
						case "npc_health": //generaly hp*256/AvgHit
							{
								var avghit = critterObjectDat.avghit(npc.item_id);
								if (avghit == 0)
								{
									valueToImport = 128;
								}
								else
								{
									valueToImport = (npc.npc_hp << 8) / avghit;
								}
								break;
							}
						case "npc_hp":
							{
								valueToImport = npc.npc_hp; break;
							}
						case "npc_arms": //possibly primary attack type
							{
								valueToImport = critterObjectDat.npc_arms(npc.item_id); break;
							}
						case "npc_power":
							{
								var str = critterObjectDat.strength(npc.item_id);
								var unk = critterObjectDat.UNK0x2DBits1To7(npc.item_id);
								valueToImport = str + unk;
								break;
							}
						case "npc_goal":
							{
								valueToImport = npc.npc_goal; break;
							}
						case "npc_gtarg":
							{
								valueToImport = npc.npc_gtarg; break;
							}
						case "npc_talkedto":
							{
								valueToImport = npc.npc_talkedto; break;
							}
						case "npc_level":
							{
								valueToImport = critterObjectDat.npc_level(npc.item_id); break;
							}
						case "npc_xhome":
							{
								valueToImport = npc.quality; break;
							}
						case "npc_yhome":
							{
								valueToImport = npc.owner; break;
							}
						case "npc_name":
							{
								valueToImport = GameStrings.AddString(0x125, npc.a_name);
								// if (npc.npc_whoami == 0)
								// {//generic npc, name is in block 4
								// 	valueToImport = npc.item_id;
								// 	valueToImport |= 0x800;

								// }
								// else
								// {
								// 	//Name of npc in string block 7
								// 	valueToImport = npc.npc_whoami + 0x10;
								// 	valueToImport |= 0xE00;
								// }
								break;
							}
						case "npc_attitude":
							{
								if ((npc.npc_goal == 5) && (npc.npc_gtarg == 1))
								{
									valueToImport = 6;
								}
								else
								{
									var flag = (npc.npc_hunger & 0x40) >> 6;
									if (flag == 0)
									{
										valueToImport = 6;
									}
									else
									{
										valueToImport = npc.npc_attitude;
									}
								}
								break;
							}
						case "play_hunger":
							{
								valueToImport = playerdat.play_hunger; break;
							}
						case "play_health":
							{
								valueToImport = (playerdat.play_hp << 8) / playerdat.max_hp; break;
							}
						case "play_hp":
							{
								valueToImport = playerdat.play_hp; break;
							}
						case "play_arms":
							{
								valueToImport = playerdat.STR + playerdat.Attack; break;
							}
						case "play_power":
							{
								valueToImport = playerdat.DEX + playerdat.play_mana + playerdat.Missile; break; // Why dex + mana + missile?
							}
						case "play_mana":
							{
								valueToImport = playerdat.play_mana; break;
							}
						case "play_level":
							{
								valueToImport = playerdat.play_level; break;
							}
						case "dungeon_level":
							{
								valueToImport = playerdat.dungeon_level; break;
							}
						case "game_time":
							{
								valueToImport = playerdat.game_time; break;
							}
						case "game_mins":
							{
								valueToImport = playerdat.game_mins; break;
							}
						case "game_days":
							{
								valueToImport = playerdat.game_days; break;
							}
						case "new_player_exp":
							{
								valueToImport = 0; break;
							}
						case "play_sex":
							{
								if (playerdat.isFemale)
								{
									valueToImport = 1;
								}
								else
								{
									valueToImport = 0;
								}
								break;
							}
						case "play_poison":
							{
								valueToImport = playerdat.play_poison; break;
							}
						case "play_drawn":
							{
								valueToImport = playerdat.play_drawn; break;
							}
						case "play_name": //reference to a custom gamestring for the player name
							{
								//valueToImport = (0x125<<9) | playerdat.CharNameStringNo;
								valueToImport = playerdat.CharNameStringNo;//pointer to string in block 0x125
								break;
							}
					}
					Debug.Print($"Importing {currentConversation.functions[i].importname} to {address} with value {valueToImport}");
					Set(address, valueToImport);
				}
			}
		} //end importvars

		public static void ExportVariables(uwObject npc)
		{
			//TODO Export the values to bglobals and back to the npc and player
			for (int c = 0; c <= bglobal.bGlobals.GetUpperBound(0); c++)
			{
				if (npc.npc_whoami == bglobal.bGlobals[c].ConversationNo)
				{
					for (int x = 0; x <= bglobal.bGlobals[c].Globals.GetUpperBound(0); x++)
					{
						Debug.Print($"Exporting {at(x)} from {x}");
						bglobal.bGlobals[c].Globals[x] = at(x);
					}
					break;
				}
			}
			
			var tmp = FindVariable("npc_hunger");
			if (tmp>=32)
				{//Set bit 7
					npc.npc_hunger = (short)((npc.npc_hunger & 0x7F) | (1<<7));
				}
			else
				{
					//unset bit
					npc.npc_hunger = ((short)(npc.npc_hunger & 0x7F));
				}
			
			npc.npc_hp = (byte)FindVariable("npc_hp");
			npc.owner = FindVariable("npc_yhome");
			npc.quality = FindVariable("npc_xhome");
			npc.npc_goal = (byte)FindVariable("npc_goal"); //There may be deeper logic applying here
			npc.npc_gtarg = (byte)FindVariable("npc_gtarg");
			npc.npc_talkedto=1;// this always gets set to 1
			tmp = FindVariable("npc_attitude");
			if (tmp>3)
			{
				npc.npc_attitude = 3;
				npc.npc_hunger = (short)(npc.npc_hunger | 0x40); //set bit 6 after conv
			}
			else
			{
				npc.npc_attitude = (short)Math.Max(0, (int)tmp);
			}

			playerdat.play_hunger = (byte)FindVariable("play_hunger");
			playerdat.play_hp = (byte)FindVariable("play_hp");
			playerdat.play_mana = (byte)FindVariable("play_mana");
			playerdat.play_poison = (byte)FindVariable("play_poison");
			//Add new_exp
			tmp = FindVariable("new_player_exp");
			if (tmp!=0)
			{
				Debug.Print("Change in xp. To implement.");//the exp change logic is dungeon level dependant with some randomisation
			}
		}

		/// <summary>
		/// Matches up an imported variable and returns the stack value at it's addres
		/// </summary>
		/// <param name="varname"></param>
		/// <param name="currentConversation"></param>
		/// <returns></returns>
		public static short FindVariable(string varname)
		{
			for (int i=0; i<= currentConversation.functions.GetUpperBound(0); i++)
			{
				if (currentConversation.functions[i].import_type == 0x010F)
					{
						if (varname.ToLower() == currentConversation.functions[i].importname.ToLower())
							{
								Debug.Print($"{varname}  = {at(currentConversation.functions[i].ID_or_Address)}");
								return at(currentConversation.functions[i].ID_or_Address);
							}
					}
			}
			Debug.Print($"Imported Variable {varname} not found!");
			return 0;
		}

		/// <summary>
		/// Returns the stack address where a variable is stored at.
		/// </summary>
		/// <param name="varname"></param>
		/// <returns></returns>
		public static int FindVariableAddress(string varname)
		{
			for (int i=0; i<= currentConversation.functions.GetUpperBound(0); i++)
			{
				if (currentConversation.functions[i].import_type == 0x010F)
					{
						if (varname.ToLower() == currentConversation.functions[i].importname.ToLower())
							{
								//Debug.Print($"{varname}  = {at(currentConversation.functions[i].ID_or_Address)}");
								return currentConversation.functions[i].ID_or_Address;
							}
					}
			}
			Debug.Print($"Imported Variable {varname} not found!");
			return 0;
		}

		public static string GetVariableNameAtAddress(int address)
		{
			if (address<=currentConversation.functions.GetUpperBound(0))
			{
				if (currentConversation.functions[address].import_type == 0x010F)
				{
					return currentConversation.functions[address].importname;
				}
			}
			return "";
		}
	}//end class
}// end namespace
