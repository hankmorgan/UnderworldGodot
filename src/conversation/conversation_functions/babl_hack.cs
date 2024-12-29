
using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void babl_hack(uwObject talker)
        {
            var mode = at(at(stackptr - 1));

            switch (mode)
            {
                case 1:
                    {
                        if (playerdat.IsAvatarInPitFightGlobal)
                        {
                            playerdat.IsAvatarInPitFightGlobal = false;
                            result_register = 1;
                        }
                        else
                        {
                            result_register = 0;
                        }
                        break;
                    }
                default:
                    Debug.Print($"unimplemented babl hack mode {mode}");
                    break;
            }
        }
    }
}