using System.Collections;
using System.Diagnostics;
using Peaky.Coroutines;
namespace Underworld
{
    /// <summary>
    /// Implementation note. When getting arguments for functions. Half the offset value compared to the stack offsets in the disassembly
    /// </summary>
    public partial class ConversationVM : UWClass
    {
        public static IEnumerator run_imported_function(ImportedFunctions func, uwObject npc)
        {
            switch (func.importname.ToLower())
            {
                case "babl_menu":
                    {
                        yield return babl_menu();
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
                default:
                    {
                        Debug.Print($"Unimplemented {func.importname}");break;
                    }
            }
            yield return null;
        }
    }
}//end namespace