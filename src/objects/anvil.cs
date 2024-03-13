using Peaky.Coroutines;
using System;
using System.Collections;
using System.Diagnostics;

namespace Underworld
{
    public class anvil : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            //flag we are using the anvil
            useon.CurrentItemBeingUsed = new useon(obj, WorldObject);
            //print use message
            uimanager.AddToMessageScroll(useon.CurrentItemBeingUsed.GeneralUseOnString);
            return true;
        }


        /// <summary>
        /// Using an anvil prompt on an item
        /// </summary>
        /// <param name="anvil"></param>
        /// <param name="itemToRepair"></param>
        /// <param name="WorldObject"></param>
        /// <returns></returns>
        public static bool UseOn(uwObject anvil, uwObject itemToRepair, bool WorldObject)
        {
            if(WorldObject)
            {
                return true; //anvil cannot be used on world object
            }
            //Check if object can be repaired.
            if ((sbyte)itemToRepair.Durability >= 0)
            {    //can be repaired            
                _ = Peaky.Coroutines.Coroutine.Run(
                    RepairLogic(itemToRepair, playerdat.Repair, WorldObject),
                    main.instance
                    );

            }
            else
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_cannot_repair_that_));
            }


            return true;
        }

        /// <summary>
        /// Prompts the player 
        /// </summary>
        /// <param name="itemToRepair"></param>
        /// <param name="repairskill"></param>
        /// <param name="WorldObject"></param>
        /// <param name="PlayerRepair"></param>
        /// <returns></returns>
        public static IEnumerator RepairLogic(uwObject itemToRepair, int repairskill, bool WorldObject, bool PlayerRepair = true)
        {
            if (PlayerRepair)
            {
                //do estimate of difficulty
                var repairscore = itemToRepair.Durability - repairskill + 15;
                int stringno;
                switch (repairscore)
                {
                    case < 0:
                        stringno = 0; break;
                    case >= 0 and <= 30:
                        stringno = 1 + repairscore / 10; break;
                    case > 30:
                        stringno = 4; break;
                }
                if (_RES == GAME_UW2)
                {
                    stringno += 234;
                }
                else
                {
                    stringno += 219;
                }

                var repairstring = $"{GameStrings.GetString(1, GameStrings.str_you_think_it_will_be_)}";
                repairstring += $"{GameStrings.GetString(1, stringno)}";//trivial etc
                if (_RES == GAME_UW2)
                {
                    repairstring += $"{GameStrings.GetString(1, 232)}";//to repair the 
                    repairstring += GameStrings.GetSimpleObjectNameUW(itemToRepair.item_id);
                    repairstring += $"{GameStrings.GetString(1, 233)}";//make the attempt
                }
                else
                {
                    repairstring += $"{GameStrings.GetString(1, 217)}";//to repair the 
                    repairstring += GameStrings.GetSimpleObjectNameUW(itemToRepair.item_id);
                    repairstring += $"{GameStrings.GetString(1, 218)}";//make the attempt
                }
                

                //Ask y/n to repair
                main.gamecam.Set("MOVE", false);
                uimanager.instance.TypedInput.Text="Yes";
                MessageDisplay.WaitingForYesOrNo = true;
                uimanager.instance.scroll.Clear();
                uimanager.AddToMessageScroll($"{repairstring}{{TYPEDINPUT}}", mode: MessageDisplay.MessageDisplayMode.TypedInput);

                while (MessageDisplay.WaitingForYesOrNo)
                {
                    yield return new WaitOneFrame();
                }
                var response = uimanager.instance.TypedInput.Text;
                if (response.ToUpper() == "YES")
                {
                    //Attempt repair
                    var repairtime = Math.Min(15, ((itemToRepair.Durability * 3) - repairskill) - (itemToRepair.quality / 2));
                    repairtime = Math.Abs(repairtime * 60);
                    var result = RepairObject(repairskill, itemToRepair);
                    if (PlayerRepair)
                    {
                        Debug.Print("play repair cutscene");
                        Debug.Print("BANG BANG BANG. I THINK A LOAD OF NOISE HAPPENS HERE. CHANGE 0x1D of player critter data to 0xF ");
                        playerdat.AdvanceTime(repairtime);

                        switch (result)
                        {
                            case -2: //critical fail
                                {
                                    uimanager.AddToMessageScroll($"{GameStrings.GetString(1, GameStrings.str_you_destroyed_the_)}{GameStrings.GetSimpleObjectNameUW(itemToRepair.item_id)}");
                                    if (WorldObject)
                                    {
                                        ObjectCreator.RemoveObject(itemToRepair);
                                    }
                                    else
                                    {
                                        playerdat.RemoveFromInventory(itemToRepair.index, true, true);
                                    }
                                    break;
                                }
                            case -1:// crit fail with parital damage
                            {
                                uimanager.AddToMessageScroll($"{GameStrings.GetString(1, GameStrings.str_you_damaged_the_)}{GameStrings.GetSimpleObjectNameUW(itemToRepair.item_id)}");
                                break;
                            }
                            case 0://unable to repair
                                {
                                    break;
                                }
                            case 1:
                                {
                                    uimanager.AddToMessageScroll($"{GameStrings.GetString(1, GameStrings.str_your_attempt_has_no_effect_on_the_)}{GameStrings.GetSimpleObjectNameUW(itemToRepair.item_id)}");
                                    break;
                                }
                            case 2://partial repair
                                {
                                    uimanager.AddToMessageScroll($"{GameStrings.GetString(1, GameStrings.str_you_have_partially_repaired_the_)}{GameStrings.GetSimpleObjectNameUW(itemToRepair.item_id)}");
                                    break;
                                }
                            case 3://full repair
                                {
                                    uimanager.AddToMessageScroll($"{GameStrings.GetString(1, GameStrings.str_you_have_fully_repaired_the_)}{GameStrings.GetSimpleObjectNameUW(itemToRepair.item_id)}");
                                    break;
                                }
                        }
                        uimanager.UpdateInventoryDisplay();
                    }
                    else
                    {
                        if (result == -2)
                        {

                        }
                    }
                    yield return 0;
                }
                else
                {
                    Debug.Print("Cancel");
                    yield return 0;
                }
            }
            else
            {//npc repair
                var result = RepairObject(repairskill, itemToRepair);
                if (result == -2)
                {
                    //npc failed to repair the object
                    if (WorldObject)
                    {
                        ObjectCreator.RemoveObject(itemToRepair);
                    }
                    else
                    {
                        playerdat.RemoveFromInventory(itemToRepair.index, true, true);//should not happen?
                    }
                }
            }
        }


        /// <summary>
        /// Attempts a repair on the specified object using the repair skill value
        /// Updates object quality
        /// </summary>
        /// <param name="repairskill"></param>
        /// <param name="itemToRepair"></param>
        /// <returns>3= fully repaired, 2=partial repair, 1=fail/no effect, 0=not allowed to repair, -1=partial damage, -2=destroy object</returns>
        public static int RepairObject(int repairskill, uwObject itemToRepair)
        {
            if (itemToRepair.Durability < 0)
            {
                return 0;
            }
            var result = playerdat.SkillCheck(repairskill, itemToRepair.Durability);
            switch (result)
            {
                case playerdat.SkillCheckResult.CritFail:
                    {//damage the object
                        if (Rng.r.Next(0, 63) <= itemToRepair.quality + repairskill)
                        {
                            itemToRepair.quality = 0;
                            return -2;
                        }
                        else
                        {//damage the object
                            var newq = itemToRepair.quality - (4 + Rng.r.Next(0, 7));
                            itemToRepair.quality = (short)Math.Max(0, newq);
                            if (itemToRepair.quality == 0)
                            {
                                return -2;
                            }
                            else
                            {
                                return -1;
                            }
                        }

                    }
                case playerdat.SkillCheckResult.Success:
                    {
                        var newq = itemToRepair.quality + 3 + (repairskill / 5);
                        itemToRepair.quality = (short)Math.Min(0x3f, newq);
                        if (itemToRepair.quality == 0x3F)
                        {
                            return 3;
                        }
                        else
                        {
                            return 2;
                        }
                    }
                case playerdat.SkillCheckResult.CritSucess:
                    {
                        itemToRepair.quality = 0x3F; return 3;
                    }
                case playerdat.SkillCheckResult.Fail:
                default:
                    {//no change
                        return 1;
                    }
            }
        }
    }//end class
}//end namespace