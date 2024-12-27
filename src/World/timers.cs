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
        static int GetTimer(int index)
        {
            return (int)Loader.getAt(UWTileMap.current_tilemap.lev_ark_block.Data, 0x7D88 + (index * 2), 16);
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
                            if (Math.Abs(playerdat.tileX - tTrigger.tileX) <= 8)
                            {
                                if (Math.Abs(playerdat.tileY - tTrigger.tileY) <= 8)
                                {
                                    while (noOfRuns>0)
                                    {                                        
                                        trigger.RunTrigger(
                                            character: 0,
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