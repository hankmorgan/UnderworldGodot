using System.Collections;
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Implementation note. When getting arguments for functions. Half the offset value compared to the stack offsets in the disassembly
    /// </summary>
    public partial class ConversationVM : UWClass
    {
        public static IEnumerator run_imported_function(ImportedFunctions func, uwObject talker)
        {
            result_register = 0;
            switch (func.importname.ToLower())
            {
                case "babl_menu":
                    {
                        yield return babl_menu();
                        break;
                    }
                case "babl_fmenu":
                    {
                        yield return babl_fmenu();
                        break;
                    }
                case "sex":
                    {
                        Sex();
                        break;
                    }
                case "get_quest":
                    {
                        get_quest();
                        break;
                    }
                case "set_quest":
                    {
                        set_quest();
                        break;
                    }
                case "random":
                    {
                        Random();
                        break;
                    }
                case "setup_to_barter":
                    {
                        setup_to_barter(talker);
                        break;
                    }
                case "print":
                    {
                        yield return print();
                        break;
                    }
                case "do_judgement":
                    {
                        yield return do_judgement();
                        break;
                    }
                case "do_offer":
                    {
                        yield return do_offer();
                        TradeResult = result_register;
                        break;
                    }
                case "do_demand":
                    {
                        yield return do_demand(talker);
                        TradeResult = result_register;
                        break;
                    }
                case "gronk_door":
                    {
                        gronk_door();
                        break;
                    }
                case "show_inv":
                    {
                        show_inv();
                        break;
                    }
                case "give_to_npc":
                    {
                        give_to_npc(talker);
                        break;
                    }
                case "take_from_npc":
                    {
                        take_from_npc(talker);
                        break;
                    }
                case "babl_ask":
                    {
                        yield return babl_ask();
                        break;
                    }
                case "compare":
                    {
                        compare();
                        break;
                    }
                case "count_inv":
                    {
                        count_inv();
                        break;
                    }
                case "remove_talker":
                    {
                        remove_talker(talker);
                        break;
                    }
                case "x_obj_stuff":
                    {
                        x_obj_stuff();
                        break;
                    }
                case "identify_inv":
                    {
                        identify_inv();
                        break;
                    }
                case "contains":
                    {
                        contains();
                        break;
                    }
                case "find_barter":
                    {
                        find_barter();
                        break;
                    }
                case "give_ptr_npc":
                    {
                        give_ptr_npc(talker);
                        break;
                    }
                case "check_inv_quality":
                    {
                        check_inv_quality();
                        break;
                    }
                case "x_skills":
                    {
                        x_skills();
                        break;
                    }
                case "set_likes_dislikes":
                    {
                        set_likes_dislikes();
                        break;
                    }
                case "find_inv":
                    {
                        find_inv(talker);
                        break;
                    }
                default:
                    {
                        Debug.Print($"Unimplemented {func.importname}"); break;
                    }
            }
            //set the value at the stackptr to the result
            Debug.Print($"Set result {result_register} at {stackptr}");
            Set(stackptr, result_register);

            if (TradeResult == 1)
            {
                SwapTradedObjects(talker);
                TradeResult = 0;
            }
            yield return null;
        }


    }//end class
}//end namespace