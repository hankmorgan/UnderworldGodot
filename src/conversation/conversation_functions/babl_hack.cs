
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
                case 2:
                    {
                        SetUpArenaFight();
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

        /// <summary>
        /// Callback to run on teleportation into a combat arena
        /// </summary>
        static void SetFightingInPit_Callback()
        {
            playerdat.IsFightingInPit = true;
            Teleportation.CodeToRunOnTeleport = null;
        }


        /// <summary>
        /// Creates an arena fight via conversation with Jospur
        /// </summary>
        static void SetUpArenaFight()
        {
            var IsPowerFullprobability_var4 = GetConvoStackValueAtPtr(stackptr-4);
            var Arena_var6 = GetConvoStackValueAtPtr(stackptr-3);
            var di_noOfFighters =GetConvoStackValueAtPtr(stackptr-2);
            var xOffset_var8 = 1;
            var yOffset_varA = 1;
            var var10 = 0;

            
            //RNG is reinitialised here

            if (Arena_var6 == 1 || Arena_var6 == 2)
            {
                xOffset_var8 = -1;
            }
            if (Arena_var6>1)
            {
                yOffset_varA = -1;
            }

            if (di_noOfFighters == 5)
            {
                var newFighter = pitsofcarnage.CreateRandomPitFighter((xOffset_var8 * 6) + 0x1F , (yOffset_varA<<2)+0x1F, 99);
                if (newFighter != null)
                {
                    var10++;
                    di_noOfFighters--;
                    playerdat.SetPitFighter(di_noOfFighters, (byte)newFighter.index);//set slot 4 (zero based)                    
                }
            }

            var varC = 0;

            while (varC<3)
            {
                var si = 0;
                while ((si<=varC))
                {
                    var Y = 0x1F + ((varC - si + 4) * yOffset_varA) ;
                    var X = 0x1F + ((si+4) * xOffset_var8);
                    var NewFighter = pitsofcarnage.CreateRandomPitFighter(X, Y, IsPowerFullprobability_var4);
                    if (NewFighter!=null)
                    {
                        var10++;
                        di_noOfFighters--;
                        playerdat.SetPitFighter(di_noOfFighters, (byte)NewFighter.index);     
                        if (di_noOfFighters == 0)
                        {//last fighter
                            CalculateJospurDebt(var10);
                            Teleportation.CodeToRunOnTeleport = SetFightingInPit_Callback;
                            result_register = var10;
                            return;
                        }                   
                    }
                    si++;
                }
                varC++;
            }
        }

    /// <summary>
    /// Sets how much jospur will pay out on victory in the pits
    /// </summary>
    /// <param name="NoOfFighters"></param>
    static void CalculateJospurDebt(int NoOfFighters)
    {
        switch(NoOfFighters)
        {
            case 2:
                playerdat.SetQuest(133,8);break;
            case 3:
                playerdat.SetQuest(133,0xC);break;
            case 4:
                playerdat.SetQuest(133,0x14);break;
            case 5:
                playerdat.SetQuest(133,0x28);break;
            default:
                playerdat.SetQuest(133,0); break;
        }
    }

    }//end class
}//end namespace