
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void babl_hack(uwObject talker)
        {
            var mode = at(at(stack + stackptr - 1));

            Debug.Print($"babl hack mode {mode}");
            switch (mode)
            {
                case 0: //challenge a fighter in the pits
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
                case 2: //set up a pit fight via Jospur
                    {
                        result_register = SetUpArenaFight();
                        break;
                    }
                case 3://gets and clears jospurs debt.
                    {
                        result_register = playerdat.GetQuest(133);
                        playerdat.SetQuest(133, 0);                        
                        break;
                    }
                case 4: //check if fighting in the pits
                    {
                        Debug.Print("untested checkifinpitsfight");
                        if (playerdat.IsFightingInPit)
                        {
                            result_register = 1;
                        }
                        else
                        {
                            result_register = 0;
                        }
                        break;
                    }
                case 5://set bit 7 at offset 0xA on an npc whoami
                    {
                        Debug.Print("untested set bit on npcwhoami");
                        var who = at(at(stack + stackptr - 2));
                        CallBacks.RunCodeOnNPCS_WhoAmI(
                            methodToCall: npc.set_unkABit7,
                            whoami: who,
                            paramsArray: new int[] { 1 },
                            loopAll: false);
                        break;
                    }
                case 6://gets a sound flag
                    {
                        Debug.Print ("GetSoundRelatedFlagValue in BablHack()");
                        result_register = 0;
                        break;
                    }
                case 7://Wand recharge.  (untested but I believe this is one of the merchants in the Keep)
                    {
                        Debug.Print("untested recharge wand");
                        var newCharge = at(at(stack + stackptr - 3));
                        var objI = at(at(stack + stackptr - 2));
                        var obj = UWTileMap.current_tilemap.LevelObjects[objI];
                        result_register = enchanting.MagicObjectChargeUpdate(
                            obj: obj,
                            objList: UWTileMap.current_tilemap.LevelObjects,
                            WorldObject: true,
                            ChargeChangeFactor: newCharge);
                        break;
                    }
                case 8: //modify trade evaluation threshold
                    {
                        Debug.Print("untested change trade evaluation threshold");
                        var multiplier = at(at(stack + stackptr - 2));
                        TradePatience = multiplier * TradePatience;
                        result_register = TradePatience;
                        break;
                    }
                case 9: //trade bonus
                    {
                        Debug.Print("untested babltradebonus");
                        BablTradeBonus = at(at(stack + stackptr - 2));
                        result_register = BablTradeBonus;
                        break;
                    }
                case 0xA://check for guardian signet ring
                    {
                        Debug.Print("Untested guardian signet ring check");
                        if(playerdat.LeftRingObject !=null)
                        {
                            if (playerdat.LeftRingObject.item_id == 0x35)
                            {
                                result_register = 1;
                                break;
                            }
                            else
                            {
                                if (playerdat.RightRingObject !=null)
                                {
                                    if (playerdat.RightRingObject.item_id == 0x35)
                                    {
                                        result_register = 1;
                                        break;
                                    }
                                }
                            }
                        }
                        result_register = 0; //player not wearing a signet ring.
                        break;
                    }
                default:
                    Debug.Print($"unimplemented babl hack mode {mode}");
                    result_register = 0;
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
        static int SetUpArenaFight()
        {
            var IsPowerFullprobability_var4 = GetConvoStackValueAtPtr(stack + stackptr - 4);
            var Arena_var6 = GetConvoStackValueAtPtr(stack + stackptr - 3);
            var di_noOfFighters = GetConvoStackValueAtPtr(stack + stackptr - 2);
            var xOffset_var8 = 1;
            var yOffset_varA = 1;
            var var10 = 0;


            //RNG is reinitialised here

            if (Arena_var6 == 1 || Arena_var6 == 2)
            {
                xOffset_var8 = -1;
            }
            if (Arena_var6 > 1)
            {
                yOffset_varA = -1;
            }

            if (di_noOfFighters == 5)
            {
                var newFighter = pitsofcarnage.CreateRandomPitFighter((xOffset_var8 * 6) + 0x1F, (yOffset_varA << 2) + 0x1F, 99);
                if (newFighter != null)
                {
                    var10++;
                    di_noOfFighters--;
                    playerdat.SetPitFighter(di_noOfFighters, (byte)newFighter.index);//set slot 4 (zero based)                    
                }
            }

            var varC = 0;

            while (varC < 3)
            {
                var si = 0;
                while ((si <= varC))
                {
                    var Y = 0x1F + ((varC - si + 4) * yOffset_varA);
                    var X = 0x1F + ((si + 4) * xOffset_var8);
                    var NewFighter = pitsofcarnage.CreateRandomPitFighter(X, Y, IsPowerFullprobability_var4);
                    if (NewFighter != null)
                    {
                        var10++;
                        di_noOfFighters--;
                        playerdat.SetPitFighter(di_noOfFighters, (byte)NewFighter.index);
                        if (di_noOfFighters == 0)
                        {//last fighter
                            CalculateJospurDebt(var10);
                            Teleportation.CodeToRunOnTeleport = SetFightingInPit_Callback;
                            return var10;
                        }
                    }
                    si++;
                }
                varC++;
            }
            return var10;
        }

        /// <summary>
        /// Sets how much jospur will pay out on victory in the pits
        /// </summary>
        /// <param name="NoOfFighters"></param>
        static void CalculateJospurDebt(int NoOfFighters)
        {
            switch (NoOfFighters)
            {
                case 2:
                    playerdat.SetQuest(133, 8); break;
                case 3:
                    playerdat.SetQuest(133, 0xC); break;
                case 4:
                    playerdat.SetQuest(133, 0x14); break;
                case 5:
                    playerdat.SetQuest(133, 0x28); break;
                default:
                    playerdat.SetQuest(133, 0); break;
            }
        }

    }//end class
}//end namespace