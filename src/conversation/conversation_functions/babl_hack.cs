
using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void babl_hack(uwObject talker)
        {
            var mode = at(at(stackptr - 1));
            
            Debug.Print($"babl hack mode {mode}");
            switch (mode)
            {
                case 0:
                    {
                        Teleportation.CodeToRunOnTeleport = SetFightingInPit_Callback;
                        playerdat.SetPitFighter(0, (byte)talker.index);
                        result_register = 0;
                        break;
                    }
                case 1://returns 1 if the player has triggered the cowardice hack trap to begin this conversation.
                    {
                        if (pitsofcarnage.IsAvatarInPitFightGlobal)
                        {
                            pitsofcarnage.IsAvatarInPitFightGlobal = false;
                            result_register = 1;
                        }
                        else
                        {
                            result_register = 0;
                        }
                        break;
                    }
                case 3:
                    {//gets and clears jospurs debt.
                        result_register = playerdat.GetQuest(133);
                        playerdat.SetQuest(133,0);
                        break;
                    }
                default:                
                    Debug.Print($"unimplemented babl hack mode {mode}");
                    break;
            }
        }

        static void SetFightingInPit_Callback()
        {
            playerdat.IsFightingInPit = true;
            Teleportation.CodeToRunOnTeleport = null;
        }

    }//end class
}//end namespace