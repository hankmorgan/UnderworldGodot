using System;
using System.Collections;
using System.Diagnostics;
using Peaky.Coroutines;

namespace Underworld
{
    /// <summary>
    /// Class for repairing items.
    /// </summary>
    public class repair : UWClass
    {



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
                                        ObjectRemover.DeleteObjectFromTile(itemToRepair.tileX, itemToRepair.tileY, itemToRepair.index);
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
                        ObjectRemover.DeleteObjectFromTile(itemToRepair.tileX, itemToRepair.tileY, itemToRepair.index);
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


    /// <summary>
    /// UW2 Repair/mend spell
    /// </summary>
    /// <param name="index"></param>
    /// <param name="objList"></param>
    public static void MendingSpell(int index, uwObject[] objList)
    {
        var objToRepair = objList[index];

        if (CanObjectBeMagicallyRepaired(objToRepair))
        {//the spell repairs the item name
            objToRepair.quality = 0x3F;
            uimanager.AddToMessageScroll($"{GameStrings.GetString(1, 0x12F)}{GameStrings.GetObjectNounUW(objToRepair.item_id)}");
            uimanager.UpdateInventoryDisplay();
            playerdat.PlayerStatusUpdate();
        }
        else
        {
            uimanager.AddToMessageScroll(GameStrings.GetString(1,0x142)); //Spell has no noticeable effect.           
        }
    }


    static bool CanObjectBeMagicallyRepaired(uwObject obj)
    {
        if (obj.majorclass == 0)
        {   
            return true;//weapons and armour
        }
        else
        {
            if (obj.majorclass == 2)//food, torches, wands etc.
            {
                if (obj.minorclass == 1)
                {//light sources and wands
                    if ((obj.OneF0Class == 0x90) || (obj.OneF0Class == 0x94))//this is possibly a vanilla bug. Item ids 0x90 and 0x94 is the lantern. Vanilla Code is masking 1F0 class and not 1FF
                        {
                            return false;
                        }
                    else
                        {
                            return true;
                        }
                }
                else
                {
                    if (obj.minorclass == 3)
                    {//food
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (obj.majorclass == 5)
                {
                    if (obj.minorclass == 0)
                    {//doors
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
    }


    }//end class
}//end namespace