namespace Underworld
{
    public class lockpick : objectInstance
    {

        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (!WorldObject)
            {
                useon.CurrentItemBeingUsed = new useon(obj, WorldObject);
                //print use message
                //8	What lock would you like to try to pick?
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 8));
            }
            return true;
        }

        public static bool UseOn(uwObject LockpickObject, uwObject targetObject)
        {//assumes lockpick is in inventory. door is in world
            var isDoor = ((targetObject.majorclass == 5) && (targetObject.minorclass == 0));
            //door doorInstance = null;
            if (isDoor)
            {
                //doorInstance = (door)targetObject.instance;
                if (door.isOpen(targetObject))
                {
                    //6	That is already open.
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 6));
                    return true;
                }
            }
            var lockobj = a_lock.LockObject(targetObject);
            if (a_lock.LockObject != null)
            {
                if (a_lock.GetIsLocked(targetObject))
                {
                    //locked. try and picklock using skill check
                    //The skill check is ``lockpick`` + 1 vs 3 times the ``zpos`` value of any ``a_lock`` object linked to the locked door or chest.
                    // Possible results are
                    // * Critical Success and Success- Door is unlocked
                    // * Failure - Door remains locked
                    // * Critical Failure - Door remains locked. Another skill check of ``Dexterity`` vs 20d takes place. If this fails the lockpick is broken.

                    var result = playerdat.SkillCheck(playerdat.PickLock + 1, lockobj.zpos * 3);
                    switch (result)
                    {
                        case playerdat.SkillCheckResult.CritSucess:
                        case playerdat.SkillCheckResult.Success:
                            {
                                //open the lock
                                a_lock.SetIsLocked(targetObject, false, 0);
                                if (isDoor)
                                {
                                    door.ToggleDoor(targetObject);
                                }
                                
                                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_succeed_in_picking_the_lock_));
                                break;
                            }
                        case playerdat.SkillCheckResult.Fail:
                            {
                                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_your_lockpicking_attempt_failed_));
                                break;
                            }
                        case playerdat.SkillCheckResult.CritFail:
                            {
                                //do a skill check dex vs 20d to see if the pick breaks;
                                switch (playerdat.SkillCheck(playerdat.DEX, 20))
                                {
                                    case playerdat.SkillCheckResult.Fail:
                                    case playerdat.SkillCheckResult.CritFail:
                                        uimanager.AddToMessageScroll("You broke your pick. \n");
                                        playerdat.RemoveFromInventory(LockpickObject.index, true);
                                        break;
                                    default:
                                        uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_your_lockpicking_attempt_failed_));
                                        break;
                                }
                                break;
                            }
                    }
                }
                else
                {
                    //122 That is not locked.
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_that_is_not_locked_));
                    //
                }
            }
            else
            {
                //else return no lock on that  message
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 3));
            }
            return true;
        }
    }//end class
}//end namespace