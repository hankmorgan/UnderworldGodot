using System.Collections;
using Peaky.Coroutines;
namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static IEnumerator run_imported_function(ImportedFunctions func, uwObject npc)
        {
            switch (func.functionName.ToLower())
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
            }
            yield return null;
        }
    }
}//end namespace