namespace Underworld
{

    /// <summary>
    /// Trap that moves NPCs around the castle map based on time of day and XClock value.
    /// </summary>
    public class a_hack_trap_castleschedule : trap
    {
        /// <summary>
        /// Castle default tileX for NPCs starting at nystrul (130);
        /// </summary>
        static short[]CastleX = new short[]{42,
                                        36,
                                        21,
                                        37,
                                        22,
                                        25,
                                        27,
                                        44,
                                        43,
                                        22,
                                        21,
                                        24,
                                        26,
                                        25
                                        };

        static short[]CastleY = new short[]{43,
                                        51,
                                        42,
                                        35,
                                        51,
                                        43,
                                        36,
                                        48,
                                        49,
                                        37,
                                        34,
                                        39,
                                        48,
                                        34
                                        };

        public static void Activate()
        {
            var di_xclock = playerdat.GetXClock(1);
            if (playerdat.GetQuest(109) == 0)
            {
                return;// player has not yet spoken to LB
            }
            if (di_xclock >= 0x10)
            {
                return; //at end game.
            }

            for (int si_whoami = 0x82; si_whoami < 0x8F; si_whoami++)//note vanilla starts with a guard (0x81) but due to the way I've handled the arrays I'm skipping that npc for now
            {
                var run = true;
                switch (si_whoami)
                {
                    case 0x8E:// Lord british
                        {
                            if (playerdat.GetQuest(112) == 1)
                            {//Lord british and avatar is fighting in the castle with friendlies (LB needs to appear at the jail to let avatar out.)
                                run = false;
                            }
                            break;
                        }
                    case 0x8D:// Lady Tori
                        {
                            if (di_xclock >= 8)
                            {   //Spoiler. Tori has been murdered at this point
                                run = false;
                            }
                            break;
                        }
                    case 0x8B://Nelson
                    case 0x8C://Patterson
                        {
                            if (di_xclock >= 0xB)
                            {
                                //Spoiler. Patterson is the traitor and kills Nelson
                                run = false;
                            }
                            break;
                        }

                }
                if (run)
                {
                    CallBacks.RunCodeOnNPCS(DoCastleschedule, si_whoami, null, false);
                }
            }
        }


        static void DoCastleschedule(uwObject critter, int[] paramsarray)
        {
            short newX; short newY;
            GetCastleDestination(
                critter: critter,
                NewTileX: out newX,
                NewTileY: out newY);
            critter.quality = newX;
            critter.owner = newY;
            critter.npc_goal = (byte)npc.npc_goals.npc_goal_goto_1;

            //TODO: Check if current location is in front of player

            //TODO: Check if destination is in front of player.

            //TODO: If player cannot see destination or current location. Do the move.
            
            npc.moveNPCToTile(
                critter: critter,
                destTileX: newX, destTileY: newY);

        }

        static void GetCastleDestination(uwObject critter, out short NewTileX, out short NewTileY)
        {
            NewTileX = (short)critter.tileX;
            NewTileY = (short)critter.tileY;

            var var2_hour = playerdat.TwelveHourClock;
            var2_hour++;
            var2_hour = var2_hour >> 1;
            var2_hour = var2_hour + 0xB;

            var varC_stage = var2_hour % 0xC; // can be 0 to 11

            var di_location = Rng.r.Next(6);//pick a random location.

            var rng_result = Rng.r.Next(3);
            if ((rng_result == 0) || ((rng_result != 0) && (varC_stage <= 2)))
            {
                switch (varC_stage)
                {
                    case 0: //case0
                    case 1:
                    case 2: 
                    case 11:
                        di_location = 0;//go to default location
                        break;
                    case 3: //case3
                    case 5:
                    case 9:
                        di_location = 3;   //go to throne room.                 
                        break;
                    case 6://case 6
                    case 7:
                    case 8:
                        di_location = 1;    //go to the lobby.
                        break;

                }
            }
            
            //special cases for miranda and xclock, Lord British and strike, and Nystrul and xclock after murders.
            if (
                (critter.npc_whoami == 0x88 && playerdat.GetXClock(1)!=0)
                ||
                (critter.npc_whoami == 0x8E && playerdat.GetQuest(115)==1)
                ||
                (critter.npc_whoami == 0x82 && playerdat.GetXClock(1)>=0xC)
                )
                {
                    di_location = 0;//stay in place
                }
            

            if (di_location == 0)
            {//when 0 go to the NPC default hangout location.
                if (critter.npc_whoami==0xA8)
                {//special case for Syria
                    NewTileX = 0x2A; //guardroom/training room
                    NewTileY = 0x24;
                }
                else
                {
                    NewTileX = CastleX[critter.npc_whoami-130];
                    NewTileY = CastleY[critter.npc_whoami-130];
                }
            }
            else
            {
                int locX1=0; int locX2=0; int locY1=0; int locY2=0;
                switch(di_location)
                {
                    case 1://castle lobby
                        locX1 = 0x1B;
                        locY1 = 0x22;
                        locX2 = 0x23;
                        locY2 = 0x25;
                        break;
                    case 2://fountain area
                        locX1 = 0x1E;
                        locY1 = 0x27;
                        locX2 = 0x20;
                        locY2 = 0x2B;
                        break;
                    case 3://Throne room
                        locX1 = 0x1D;
                        locY1 = 0x2D;
                        locX2 = 0x21;
                        locY2 = 0x33;
                        break;
                    case 4://kitchens
                        //Although the kitchens have co-ordinates defined NPCs will not actually move here
                    default:
                        return;//remain at current position as set at start of function.
                }

                //Find random spot within the bounds of the locations.
                NewTileX = (short)(locX1 + Rng.r.Next(locX2-locX1));
                NewTileY = (short)(locY1 + Rng.r.Next(locY2-locY1));
                if ((di_location == 2) && (NewTileX == 0x1F) && (NewTileY == 0x29))
                {//fountain, special case for the fountain pedestal location.
                    NewTileX++;
                }
            }
        }

    }//end class
}//end namespace