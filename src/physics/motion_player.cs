using System;
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Player Motion.
    /// </summary>
    public partial class motion : Loader
    {
        public static UWMotionParamArray playerMotionParams = new UWMotionParamArray();

        public static short PlayerHeadingRelated_dseg_67d6_33D6 = 0;

        //These 2 globals are used to determine the heading/velocity for the player.
        public static short PlayerMotionWalk_77C = 0;
        public static short PlayerMotionHeading_77E = 0;
        public static short MotionInputPressed = 0;


        //Player Vectors (to be identified)
        static short dseg_67d6_D0 = 0;
        public static short Examine_dseg_D3 = 0;

        //globals used in player motion calcs
        //Possibly some of these are actually part of the player motion params. to do map them out.
        static short dseg_67d6_33D4;
        static short dseg_67d6_33D2;
        static short dseg_67d6_33D0;

        static short dseg_67d6_229A;
        static short dseg_67d6_229C;

        static short dseg_67d6_22AA;
        static short dseg_67d6_22A6;
        static short dseg_67d6_22A8;

        static short dseg_67d6_22A0;

        static short dseg_67d6_22AC;

        static short dseg_67d6_22AE = -1;//tmp


        static short MotionWeightRelated_dseg_67d6_C8;

        static short MotionRelated_dseg_67d6_775 = 0xF;

        static short SomeTileOrTerrainDatInfo_seg_67d6_D4;

        public static void PlayerMotion(short ClockIncrement)
        {
            dseg_67d6_33D4 = 0;
            dseg_67d6_33D2 = 0;
            dseg_67d6_33D0 = 0;
            playerMotionParams.radius_22 = (byte)commonObjDat.radius(playerdat.playerObject.item_id);
            playerMotionParams.height_23 = (byte)commonObjDat.height(playerdat.playerObject.item_id);

            PlayerMotionInitialCalculation_seg008_1B09_7B2(ClockIncrement);

            CalculateMotion(
                projectile: playerdat.playerObject,
                MotionParams: playerMotionParams,
                SpecialMotionHandler: UWMotionParamArray.PlayerMotionHandler_dseg_67d6_26AA);

            ApplyPlayerMotion(playerdat.playerObject);

            playerdat.PositionPlayerObject();


        }


        static void PlayerMotionInitialCalculation_seg008_1B09_7B2(short ClockIncrement)
        {
            //todo
            short di = 0;
            short var2 = 0;
            playerMotionParams.speed_12 = (byte)ClockIncrement;
            playerMotionParams.unk_16_relatedtoPitch = 5;
            playerMotionParams.unk_17 = 0;

            dseg_67d6_229A = playerMotionParams.heading_1E;
            dseg_67d6_229C = playerMotionParams.unk_14;


            if (playerMotionParams.unk_10 == 0)
            {
                CalculateMotionFromCommand_seg008_1B09_108E(MotionInputPressed, ClockIncrement, out var2);
                if (playerMotionParams.unk_10 == 0)
                {
                    //seg008_1B09_7F6
                    di = (short)(var2 - playerMotionParams.unk_14);

                    if (Math.Abs(di) > MotionWeightRelated_dseg_67d6_C8)
                    {
                        if (di > 0)
                        {
                            di = (short)(1 * MotionWeightRelated_dseg_67d6_C8);
                        }
                        else
                        {
                            di = (short)(-1 * MotionWeightRelated_dseg_67d6_C8);
                        }

                    }
                    //seg008_1B09_81C
                    playerMotionParams.unk_14 += di;
                    if (playerMotionParams.unk_14 <= dseg_67d6_22A6)
                    {
                        if (playerMotionParams.unk_14 < 0)
                        {
                            playerMotionParams.unk_14 = 0;
                        }
                    }
                    else
                    {
                        playerMotionParams.unk_14 = dseg_67d6_22A6;
                    }
                }
            }


            //seg008_1B09_83E:
            if (playerMotionParams.unk_10 != 0)
            {
                playerdat.heading_minor += ((ClockIncrement * MotionRelated_dseg_67d6_775) * (PlayerMotionHeading_77E / 4)) / 4;
            }


            if (_RES == GAME_UW2)
            {
                //seg008_1B09_862:
                //TODO UW2 specific code for ice and water currents


                //
            }



            if (_RES == GAME_UW2)
            {
                if (
                    ((((int)playerMotionParams.unk_c_X | (int)playerMotionParams.unk_e | (int)playerMotionParams.unk_10) == 0))
                            && ((playerdat.TileState & 0x84) == 0)
                    )
                {
                    playerMotionParams.unk_17 = 0x80;
                }
                else
                {
                    if (playerMotionParams.unk_10 != 0)
                    {
                        if ((playerdat.TileState & 0x84) != 0)
                        {
                            playerMotionParams.unk_16_relatedtoPitch = 5;
                        }
                    }
                }
            }
            else
            {
                if (((int)playerMotionParams.unk_c_X | (int)playerMotionParams.unk_e | (int)playerMotionParams.unk_10) == 0)
                {
                    playerMotionParams.unk_17 = 0x80;
                }
            }


            //seg008_1B09_A9D:
            //UW1 and UW2 realign here.
            if (playerMotionParams.unk_14 == 0)
            {
                //seg008_1B09_AAA:
                playerdat.heading_major = playerdat.heading_minor;
            }

            if ((playerdat.MagicalMotionAbilities & 0x14) != 0)
            {
                //seg008_1B09_ABD:
                //player is flying or levitating
                setAt(UWMotionParamArray.PlayerMotionHandler_dseg_67d6_26AA, 0, 16, 0x1000);
                playerMotionParams.unk_17 = 0x80;
            }

            //seg008_1B09_AC8:
            playerMotionParams.unk_26_falldamage = 0;
            dseg_67d6_22A0 = playerMotionParams.unk_10;
        }

        static void ApplyPlayerMotion(uwObject playerObj)
        {
            var di_zpos = playerObj.zpos;

            if (dseg_67d6_22AE != -1)
            {
                playerdat.heading_major = dseg_67d6_22AC;
                playerMotionParams.unk_14 = dseg_67d6_22AE;
            }

            playerObj.xpos = (short)((playerMotionParams.x_0 >> 5) & 0x7);
            playerObj.ypos = (short)((playerMotionParams.y_2 >> 5) & 0x7);
            //TODO update playerObj.goal with value based on system clock

            var newTileX = playerMotionParams.x_0 >> 8;
            var newTileY = playerMotionParams.y_2 >> 8;

            if ((newTileX != playerdat.tileX) || (newTileY != playerdat.tileY))
            {
                //player has changed tiles
                playerdat.PlacePlayerInTile(newTileX, newTileY, playerdat.tileX, playerdat.tileY);
                playerObj.npc_xhome = (short)(playerMotionParams.x_0 >> 8);
                playerObj.npc_yhome = (short)(playerMotionParams.y_2 >> 8);

                //TODO Check if player is no longer lost
            }
            else
            {
                //player is in same tile
                if (di_zpos != playerMotionParams.z_4)
                {
                    //player has changed height in the same tile.
                    //TODO run pressure triggers in UW2
                }
            }

            //seg008_1B09_E5B
            if (playerMotionParams.unk_26_falldamage != 0)
            {
                if (playerMotionParams.heading_1E == playerdat.heading_major)
                {
                    if ((playerdat.TileState & 0x4) == 0)
                    {
                        if (dseg_67d6_22AE == -1)
                        {
                            playerMotionParams.unk_14 = 0;
                        }
                    }
                }
            }

            //seg008_1B09_E8A:
            if (playerMotionParams.heading_1E != playerdat.heading_major)
            {
                //this may be uw2 specific
                playerdat.heading_major = playerMotionParams.heading_1E;

                var si = playerMotionParams.heading_1E - (dseg_67d6_D0 << 0xE);
                if ((playerMotionParams.unk_17 & 0x80) != 0)
                {
                    if (SomeTileOrTerrainDatInfo_seg_67d6_D4 == -1)
                    {
                        if (Math.Abs(playerdat.heading_minor - si) >= 0x600)
                        {
                            //seg008_1B09_EC7
                            if (playerdat.heading_minor - si >= 0x7FFF)
                            {
                                //seg008_1B09_ED9: 
                                playerdat.heading_minor += 0x600;
                            }
                            else
                            {
                                playerdat.heading_minor -= 0x600;
                            }
                        }
                        else
                        {
                            //seg008_1B09_EC1:
                            playerdat.heading_minor = si;
                        }
                    }
                }

            }
            //seg008_1B09_EDF:
            playerObj.heading = (short)((playerdat.heading_minor >> 0xD) & 0x7);
            playerObj.npc_heading = (short)((playerdat.heading_minor >> 8) & 0x1F);

            if (playerMotionParams.unk_26_falldamage != 0)
            {
                if (playerMotionParams.unk_16_relatedtoPitch > 0)
                {
                    //seg008_1B09_F2F:
                    var FallDamage = playerMotionParams.unk_26_falldamage >> 8;
                    if (_RES == GAME_UW2)
                    {
                        if (worlds.GetWorldNo(playerdat.dungeon_level) == 8)
                        {
                            FallDamage = 0;
                        }
                    }
                    if (playerMotionParams.unk_a_pitch != 0)
                    {
                        FallDamage = FallDamage << 1;
                    }

                    //seg008_1B09_F56:
                    var skillCheckResult = playerdat.SkillCheck(playerdat.Acrobat, FallDamage << 1);
                    if ((int)skillCheckResult > 0)
                    {
                        //passed check
                        FallDamage = ((0x1E - playerdat.Acrobat) * FallDamage) / 0x1E;
                    }
                    if (FallDamage > 3)
                    {
                        damage.DamageObject(playerObj, FallDamage, 0, UWTileMap.current_tilemap.LevelObjects, true, 0);
                    }
                    if (!((FallDamage <= 1) && ((playerMotionParams.tilestate25 & 0x10) == 0)))
                    {
                        //seg008_1B09_FBF:
                        //TODO play landing sound effect
                    }
                }
                //seg008_1B09_FDB:
                playerMotionParams.unk_26_falldamage = 0;
            }

            //seg008_1B09_FE1:
            ProcessPlayerTileState(playerMotionParams.tilestate25, 0);

            //likely uw2 only
            if ((playerdat.TileState & 0x4) == 0)
            {
                if ((playerdat.TileState & 0x1) == 0)
                {
                    Examine_dseg_D3 = 0;
                }
                else
                {
                    if (SomeTileOrTerrainDatInfo_seg_67d6_D4 == -1)
                    {
                        Examine_dseg_D3 = 0;
                    }
                    else
                    {
                        Examine_dseg_D3 = 1;
                    }
                }
            }
            else
            {
                Examine_dseg_D3 = 1;
            }


        }

        /// <summary>
        /// Translates an input command into motion inputs.
        /// </summary>
        /// <param name="inputcmd"></param>
        /// <param name="ClockIncrement"></param>
        /// <param name="arg4"></param>
        static void CalculateMotionFromCommand_seg008_1B09_108E(short inputcmd, int ClockIncrement, out short arg4)
        {
            arg4 = 0;
            //todo
            if (playerdat.ParalyseTimer != 0)
            {
                arg4 = 0;
            }
            else
            {
                var di = playerdat.heading_minor;
                switch (inputcmd)
                {
                    case 0:
                        arg4 = 0; break;
                    case 1://walk,run,turn
                        {
                            playerdat.heading_minor += (MotionRelated_dseg_67d6_775 * ClockIncrement) * (PlayerMotionHeading_77E / 4);
                            di = playerdat.heading_minor;
                            playerdat.heading_major = di;
                            arg4 = (short)(((PlayerMotionWalk_77C >> 2) * dseg_67d6_22A6) / 0x20);
                            break;
                        }
                    case 6:
                    case 7://jumps
                        {
                            if (inputcmd == 6)
                            {
                                if (playerMotionParams.unk_a_pitch == 0)
                                {
                                    if (playerMotionParams.unk_10 == 0)
                                    {
                                        if (playerMotionParams.unk_14 == 0)
                                        {
                                            //seg008_1B09_1158:
                                            di = playerdat.heading_minor;
                                            playerdat.heading_major = di;
                                            arg4 = (short)(dseg_67d6_22A6 / 2);
                                            playerMotionParams.unk_14 = arg4;
                                            dseg_67d6_D0 = 0;
                                        }
                                    }
                                }
                            }
                            //case 7
                            playerMotionParams.unk_a_pitch = 0x263;
                            if (playerMotionParams.z_4 > 0x280)
                            {
                                playerMotionParams.unk_a_pitch = (short)((playerMotionParams.unk_a_pitch * 5) / 6);
                                if (playerMotionParams.z_4 > 0x2C0)
                                {
                                    playerMotionParams.unk_a_pitch = (short)((playerMotionParams.unk_a_pitch << 1) / 3);
                                }
                            }
                            if ((playerdat.MagicalMotionAbilities & 0x1) == 0) //leaping
                            {
                                playerMotionParams.unk_10 = -4;
                            }
                            else
                            {
                                playerMotionParams.unk_10 = -2;
                            }

                            break;
                        }
                    case 8://walk backwards
                        {
                            di = di - 0x8000;
                            arg4 = dseg_67d6_22AA;
                            dseg_67d6_D0 = -2;
                            playerdat.heading_major = di;
                            break;
                        }
                    case 9://slide
                    case 0xA:
                        {
                            di = di - 0x4000;
                            arg4 = dseg_67d6_22A8;
                            dseg_67d6_D0 = -1;
                            playerdat.heading_major = di;
                            break;
                        }
                    case 0xC://flying up?
                        {
                            dseg_67d6_D0 = 0;
                            playerMotionParams.unk_a_pitch = 0x8D;
                            playerMotionParams.unk_10 = 0;
                            playerdat.heading_major = di;
                            break;
                        }
                    case 0xD://flying down?
                        {
                            dseg_67d6_D0 = 0;
                            playerMotionParams.unk_a_pitch = -141;
                            playerMotionParams.unk_10 = 0;
                            playerdat.heading_major = di;
                            break;
                        }
                    default:
                        {
                            playerdat.heading_major = di;
                            break;
                        }
                }
            }
        }

        static void ProcessPlayerTileState(int newTileState_arg0, int arg2)
        {
            //TODO
        }

    }//end class
}//end namespace