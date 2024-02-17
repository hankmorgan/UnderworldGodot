using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void give_to_npc()
        {
            switch(_RES)
                {
                    case GAME_UW2:
                        Debug.Print("UW2 variation of give_to_npc()");
                        break;

                    default:
                        give_to_npc_uw1();
                        break;
                }
        }

        static void give_to_npc_uw1()
        {
            var arg1 = GetConvoStackValueAtPtr(stackptr-2);
            var arg2 = GetConvoStackValueAtPtr(stackptr-1);
            Debug.Print($"{arg1}, {arg2}");
        }
    }//end class
}//end namespace