using System.Collections;
using System.Diagnostics;
using Godot;
using Peaky.Coroutines;
namespace Underworld
{
    /// <summary>
    /// Implementation note. When getting arguments for functions. Half the offset value compared to the stack offsets in the disassembly
    /// </summary>
    public partial class ConversationVM : UWClass
    {
        public static IEnumerator run_imported_function(ImportedFunctions func, uwObject talker)
        {
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
                default:
                    {
                        Debug.Print($"Unimplemented {func.importname}"); break;
                    }
            }
            //set the value at the stackptr to the result
            Set(stackptr, result_register);
            yield return null;
        }
    }
}//end namespace