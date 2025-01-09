using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Underworld
{
    /// <summary>
    /// What the hell is SCD.ARK? It's an event system. Data is read from scd.ark at the end of conversations, level transitions, by a hack trap, when a block of time is added to the clock and when Killorn is crashing.
    /// </summary>
    public partial class scd : UWClass
    {

        static bool ArkHasBeenModified = false;

        /// <summary>
        /// Starts the SCD process.
        /// Checks if scd_data is loaded and check each xclock to see if events need to be ran based on various conditions.
        /// </summary>
        public static void ProcessSCDArk(int mode)
        {
            if (scd_data == null)
            {
                scd_data = new UWBlock[0x10];
            }
            int error_var2 = 0;//flags if an error has occurred and stop execution.
            for (int blockno = 0; blockno < 0x10; blockno++)
            {
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
                        currentblock: scd_data[blockno],
                        arg0_xclock: playerdat.GetXClock(0),
                        NoOfMaps: 72,
                        mode_arg4: mode);
                }
                else
                {
                    error_var2 = ProcessGeneralBlock(
                        currentblock: scd_data[blockno],
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
        static int ProcessBlock0(UWBlock currentblock, int arg0_xclock, int NoOfMaps, int mode_arg4)
        {
            var di = arg0_xclock % NoOfMaps;
            var var1 = 0;
            if (currentblock.Data[4 + (playerdat.dungeon_level * 4)] > di)
            {
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
                    currentblock.Data[4 + (playerdat.dungeon_level * 4)] = 0;
                    currentblock.Data[6 + (playerdat.dungeon_level * 4)] = 0;
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
        static int ProcessGeneralBlock(UWBlock currentblock, int XClockValue, int mode)
        {
            var si_offset = currentblock.Data[4 + playerdat.dungeon_level * 4];
            var di = XClockValue;
            currentblock.Data[4] = (byte)XClockValue;
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
                        currentblock.Data[4 + (playerdat.dungeon_level * 4)] = 0;
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
        static int FindSCDRowsToExecute(UWBlock currentblock, int mode)
        {
            int var1 = 0;
            Debug.Print("Process SCD Rows");
            var si = currentblock.Data[6];
            if (currentblock.Data[2] == 0xF)
            {
                si = 0;
            }

            while (currentblock.Data[0] > si)
            {
                if (currentblock.Data[324 + (si * 16)] <= currentblock.Data[4 + (playerdat.dungeon_level * 4)])
                {
                    if (mode != 0)
                    {
                        var1 = ProcessSCDEvent(currentblock, 324 + (si * 16));
                        switch (var1)
                        {
                            case 0:
                            case 4: //deleted record (do not come here until record deletion is in place!)
                                //si--; 
                                ArkHasBeenModified = true;
                                Debug.Print("Deleted Record");
                                si++;//remove me!
                                break;
                            case 5:
                                si++;
                                break;
                            default:
                                currentblock.Data[104 + playerdat.dungeon_level * 4] = si;
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

            currentblock.Data[6 + (playerdat.dungeon_level * 4)] = si;
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
        static int ProcessSCDEvent(UWBlock currentblock, int eventOffset)
        {
            if (
                (currentblock.Data[eventOffset + 2] == playerdat.dungeon_level)
                || (currentblock.Data[eventOffset + 2] == 0xFF)
                || (currentblock.Data[eventOffset + 2] - 246 == playerdat.CurrentWorld)
                )
            {
                //DoEvent = true;
                var var1 = 1;
                var var2 = 1;

                if (currentblock.Data[eventOffset + 4] >= 0)
                {
                    if (currentblock.Data[eventOffset + 4] <0xC)
                    {
                        Debug.Print($"Running SCD function {currentblock.Data[eventOffset + 4]}");
                        //var2 = function result.
                        if (currentblock.Data[eventOffset + 3] != 0)
                        {
                            Debug.Print($"{eventOffset} needs to be deleted!");
                            return 4;
                        }
                        else
                        {
                            return var2;
                        }
                    }
                    else
                    {
                        return 6;
                    }
                }
                else
                {
                    return 5;
                }
            }
            else
            {
                return 5;
            }
        }
    }//end class
}//end namespace