using System;

namespace Underworld
{
    /// <summary>
    /// For managing UW2 timer triggers
    /// A timer trigger runs every zpos frames
    /// It takes 127 frames or so to run each timer.
    /// </summary>
    public class timers : UWClass
    {
        public static long FrameNo = 0;
        public static int GetTimer(int index)
        {
            return (int)Loader.getAt(UWTileMap.current_tilemap.lev_ark_block.Data, 0x7D88 + (index * 2), 16);
        }

        public static void SetTimer(int index, int value)
        {
           Loader.setAt(UWTileMap.current_tilemap.lev_ark_block.Data, 0x7D88 + (index * 2), 16, value);
        }

        public static int NoOfTimerTriggers
        {
            get
            {
                int counter = 0;
                for (int i = 0; i < 64; i++)
                {
                    var index = GetTimer(i);
                    if (index != 0)
                    {
                        counter++;
                    }
                    else
                    {
                        return counter;
                    }
                }
                return counter;
            }
        }
        /// <summary>
        /// Takes a timer trigger out of the level's timer list when the trigger is removed.
        ///
        /// DOS does this whenever it removes a trigger chain (RemoveTriggerChain_ovr166_253F and
        /// RemoveTriggersPointingAtTrapToRemove_ovr166_1B84 both call
        /// MoveObjectIndexInArrayToNewPositionTimerRelated_seg044_1094): it finds the entry,
        /// lowers the count and moves the last entry into its place, so the list stays packed.
        /// The port has no separate count and ends the list at the first zero, as DOS does
        /// when it loads a level, so the last entry is zeroed instead. Left in place, the
        /// entry would keep running a trigger that no longer exists, and a save would give
        /// DOS a timer pointing at whatever object later reused the slot.
        /// </summary>
        /// <returns>Whether the object was in the list.</returns>
        public static bool RemoveTimer(int objectIndex)
        {
            int count = NoOfTimerTriggers;
            for (int t = 0; t < count; t++)
            {
                if (GetTimer(t) == objectIndex)
                {
                    SetTimer(t, GetTimer(count - 1));
                    SetTimer(count - 1, 0);
                    return true;
                }
            }
            return false;
        }

        public static void RunTimerTriggers(int delta = 1)
        {
            if (_RES != GAME_UW2) 
            { 
                return;
            }
            if (playerdat.FreezeTimeEnchantment)
            {
                return;
            }
            //loop all the timers in the data
            var counter = NoOfTimerTriggers;
            for (int t = 0; t < counter; t++)
            {
                var tIndex = GetTimer(t);
                if (tIndex != 0)
                {
                    var tTrigger = UWTileMap.current_tilemap.LevelObjects[tIndex];
                    if (tTrigger.item_id == 425)
                    {
                        //this gets the number of times the trigger would have run by this frame,
                        //and subtracts the number of times the trigger would have ran by the last frame. 
                        //The difference is the number of times the trigger needs to run in this frame.
                        var noOfRuns = ((FrameNo + delta) / (tTrigger.zpos + 1)) - (FrameNo / (tTrigger.zpos + 1));
                        if (noOfRuns > 0)
                        {
                            if (Math.Abs(playerdat.playerObject.tileX - tTrigger.tileX) <= 8)
                            {
                                if (Math.Abs(playerdat.playerObject.tileY - tTrigger.tileY) <= 8)
                                {
                                    while (noOfRuns>0)
                                    {                                        
                                        trigger.RunTrigger(
                                            character: 1,
                                            ObjectUsed: null,
                                            TriggerObject: tTrigger,
                                            triggerType: (int)triggerObjectDat.triggertypes.TIMER,
                                            objList: UWTileMap.current_tilemap.LevelObjects);
                                        noOfRuns--;
                                    }

                                }
                            }
                        }
                    }
                }
            }
            FrameNo+= delta;//advance the frame count by the delta
        }
    }//end class
}//end namespace