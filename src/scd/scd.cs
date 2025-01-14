using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// What the hell is SCD.ARK? It's an event system. Data is read from scd.ark at the end of conversations, level transitions, by a hack trap, when a block of time is added to the clock and when Killorn is crashing.
    /// </summary>
    public partial class scd : UWClass
    {
        static int BlockIdentifier;
        /// <summary>
        /// Starts the SCD process.
        /// Checks if scd_data is loaded and check each xclock to see if events need to be ran based on various conditions.
        /// </summary>
        public static void ProcessSCDArk(int mode)
        {
            Debug.Print("Starting to process SCU.ARK");
            ArkHasBeenModified = false;
            if (scd_data == null)
            {
                scd_data = new UWBlock[0x10];
            }
            int error_var2 = 0;//flags if an error has occurred and stop execution.
            for (int blockno = 0; blockno < 0x10; blockno++)
            {
                BlockIdentifier = blockno;
                Debug.Print($"Processing SCD Block {blockno}");
                var si_xclock = playerdat.GetXClock(blockno);
                if (scd_data[blockno] == null)
                {
                    if (!LoadSCDBlock(blockno))
                    {
                        Debug.Print($"Error loading scd block {blockno}");
                        return; //stop                     
                    }
                }

                scd_data[blockno].Data[2] = (byte)blockno;

                if (blockno == 0)
                {
                    error_var2 = ProcessBlock0(
                        currentblock: scd_data[blockno].Data,
                        arg0_xclock: playerdat.GetXClock(0),
                        NoOfMaps: 72,
                        mode_arg4: mode);
                }
                else
                {
                    error_var2 = ProcessGeneralBlock(
                        currentblock: scd_data[blockno].Data,
                        XClockValue: si_xclock,
                        mode: mode);
                }
                if (error_var2 != 0)
                {
                    return;
                }
                else
                {
                    if (ArkHasBeenModified)
                    {
                        Debug.Print("Ark file needs to be saved with new data.");
                    }
                    else
                    {
                        scd_data[blockno]= null;//simulate discarding of data that is unsaved.
                    }
                }
            }
        }

        /// <summary>
        /// Special case that is called on xclock 0
        /// </summary>
        /// <param name="currentblock"></param>
        /// <param name="arg0_xclock"></param>
        /// <param name="NoOfMaps"></param>
        /// <param name="mode_arg4"></param>
        /// <returns></returns>
        static int ProcessBlock0(byte[] currentblock, int arg0_xclock, int NoOfMaps, int mode_arg4)
        {
            Debug.Print($"ProcessBlock0 for {arg0_xclock}");
            var di = arg0_xclock % NoOfMaps;
            var var1 = 0;
            if (currentblock[4 + (playerdat.dungeon_level * 4)] > di)
            {
                Debug.Print($"ProcessBlock0: found valid block to process at {4 + (playerdat.dungeon_level * 4)}");
                var1 = ProcessGeneralBlock(
                    currentblock: currentblock,
                    XClockValue: NoOfMaps - 1,
                    mode: mode_arg4);
                if (var1 != 0)
                {
                    return var1;
                }
                else
                {
                    currentblock[4 + (playerdat.dungeon_level * 4)] = 0;
                    currentblock[6 + (playerdat.dungeon_level * 4)] = 0;
                }
            }

            return ProcessGeneralBlock(
                currentblock: currentblock,
                XClockValue: di,
                mode: mode_arg4);

        }

        /// <summary>
        /// General Processing of a SCD block.
        /// </summary>
        /// <param name="currentblock"></param>
        /// <param name="XClockValue"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        static int ProcessGeneralBlock(byte[] currentblock, int XClockValue, int mode)
        {
            Debug.Print($"ProcessGeneralBlock xclockvalue {XClockValue}");
            var si_offset = currentblock[4 + playerdat.dungeon_level * 4];
            var di = XClockValue;
            currentblock[4 + (playerdat.dungeon_level * 4)] = (byte)XClockValue;
            if (si_offset != di)
            {
                if (si_offset > di)
                {
                    if (mode == 0)
                    {
                        return SomethingWithSCDRows_ovr151_D3(mode);//TOOD figure out.
                    }
                    else
                    {
                        currentblock[6 + (playerdat.dungeon_level * 4)] = 0;
                        return FindSCDRowsToExecute(currentblock, mode);
                    }
                }
                else
                {
                    return FindSCDRowsToExecute(currentblock, mode);
                }
            }
            else
            {
                return 0;
            }
        }


        static int SomethingWithSCDRows_ovr151_D3(int mode)
        {
            Debug.Print("Something with SCD Rows");
            return 0;
        }

        /// <summary>
        /// Loops event rows to find possible matches to run
        /// </summary>
        /// <param name="currentblock"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        static int FindSCDRowsToExecute(byte[] currentblock, int mode)
        {
            int var1 = 0;
            Debug.Print("Trying to find SCD Rows to Execute");
            var si = currentblock[6 + (playerdat.dungeon_level * 4)];
            if (currentblock[2] == 0xF)
            {
                si = 0;
            }

            while (currentblock[0] > si)
            {
                if (currentblock[324 + (si * 16)] <= currentblock[4 + (playerdat.dungeon_level * 4)])
                {
                    if (mode != 0)
                    {
                        var1 = ProcessSCDEventRow(currentblock, 324 + (si * 16));
                        switch (var1)
                        {
                            case 0:
                                ArkHasBeenModified = true;
                                si++;
                                break;
                            case 4: //deleted record (do not come here until record deletion is in place!)                                
                                ArkHasBeenModified = true;
                                Debug.Print("Deleted Record"); 
                                si--;                                
                                break;
                            case 5:
                                si++;
                                break;
                            default:
                                currentblock[6 + playerdat.dungeon_level * 4] = si;
                                return var1;
                        }
                    }
                    else
                    {
                        si++;
                    }
                }
                else
                {
                    break;
                }
            }

            currentblock[6 + (playerdat.dungeon_level * 4)] = si;
            // if (scd_data[2] == 0xF)
            // {
            //     si = 0;
            // }

            return 0;
        }


        /// <summary>
        /// Calls the event based on player being in the matching level, world or any levels
        /// </summary>
        /// <param name="currentblock"></param>
        /// <param name="eventOffset"></param>
        /// <returns></returns>
        static int ProcessSCDEventRow(byte[] currentblock, int eventOffset)
        {            
            if (
                (currentblock[eventOffset + 2] == playerdat.dungeon_level)
                || (currentblock[eventOffset + 2] == 0xFF)
                || (currentblock[eventOffset + 2] - 246 == playerdat.CurrentWorld)
                )
            {
                Debug.Print($"Process Event Row at Offset {eventOffset}  #{(eventOffset-324)/16}");
                //DoEvent = true;
                // var var1 = 1;
                // var var2 = 1;

                if ((sbyte)(currentblock[eventOffset + 4]) >= 0)
                {
                    if ((sbyte)(currentblock[eventOffset + 4]) <0xC)
                    {                      
                        var var2 = RunSCDFunction(
                            currentblock: currentblock,
                            eventOffset: eventOffset);
                        
                        if (currentblock[eventOffset + 3] != 0)
                        {
                            Debug.Print($"{eventOffset} to be deleted!");
                            DeleteRow(currentblock, eventOffset);
                            return 4;//return row has been deleted.
                        }
                        else
                        {
                            return var2;//result of function call.
                        }
                    }
                    else
                    {
                        return 6; //error stops processing
                    }
                }
                else
                {
                    return 5;//process next
                }
            }
            else
            {
                return 5;//process next
            }
        }
    }//end class
}//end namespace