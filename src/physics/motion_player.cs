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
        public static short dseg_67d6_22A2;


        public const short MaybeBaseForwardSpeed_1_dseg_67d6_CE = 0x3AC;
        public const short MaybeBaseSlideSpeed_2_dseg_67d6_CC = 0xEB;
        public const short MaybeBaseBackwardsSpeed_3_dseg_67d6_CA = 0xBC;
        public static short MaybePlayerActualForwardSpeed_1_dseg_67d6_22A6;
        public static short MaybePlayerActualSlideSpeed_2_dseg_67d6_22A8;
        public static short MaybePlayerActualBackwardsSpeed_3_dseg_67d6_22AA;


        static short dseg_67d6_22A0;
        static short dseg_67d6_22AC;
        static short dseg_67d6_22AE = -1;//tmp


        public static short MotionWeightRelated_dseg_67d6_C8;

        public static short MotionRelated_dseg_67d6_775 = 0xF;

        static short SomeTileOrTerrainDatInfo_seg_67d6_D4;

        public static short PlayerHeadingMinor_dseg_8294;
        public static short PlayerHeadingMajor_dseg_67d6_8296;

        public static short PreviousTileState_dseg_67d6_22B4 = 0;


        public static short RelatedToSwimDmg_dseg_67d6_33CE = 0;
        public static byte RelatedToClockIncrement_67d6_742;

        public static bool dseg_67d6_33c6;

        static sbyte[] dseg_67d6_743 = new sbyte[] { 1, 3, 4, 3, 1, -3, 0, 0, 1, 3, 4, 3, 1, -3, 0, 0 };
        static sbyte[] dseg_67d6_753 = new sbyte[] { 0, 0, -1, -2, -3 - 4, -5, -6, -6, -4, -3, -2, -1, 0, 0, 0, -4};


        /// <summary>
        /// Moves the player by processing inputs and forces acting on the player
        /// </summary>
        /// <param name="ClockIncrement"></param>
        public static void PlayerMotion(short ClockIncrement)
        {
            UWMotionParamArray.instance = motion.playerMotionParams;
            dseg_67d6_33D4 = 0;
            dseg_67d6_33D2 = 0;
            dseg_67d6_33D0 = 0;
            playerMotionParams.radius_22 = (byte)commonObjDat.radius(playerdat.playerObject.item_id);
            playerMotionParams.height_23 = (byte)commonObjDat.height(playerdat.playerObject.item_id);
            // var x_init = playerMotionParams.x_0;
            // var y_init = playerMotionParams.y_2;
            PlayerMotionInitialCalculation_seg008_1B09_7B2(ClockIncrement);

            CalculateMotion(
                projectile: playerdat.playerObject,
                MotionParams: playerMotionParams,
                SpecialMotionHandler: UWMotionParamArray.PlayerMotionHandler_dseg_67d6_26AA);

            ApplyPlayerMotion(playerdat.playerObject);

            playerdat.heading_major = PlayerHeadingMajor_dseg_67d6_8296 >> 8;//this hack fixes turning but the heading value here is actually direction of motion so the camera turns during backwards and sideways motion

            playerdat.PositionPlayerObject();

            // if ((x_init != playerMotionParams.x_0) || (y_init != playerMotionParams.y_2))
            // {
            //     Debug.Print($"Move from {x_init},{y_init} to {playerMotionParams.x_0},{playerMotionParams.y_2}   ({playerdat.playerObject.tileX}, {playerdat.playerObject.tileY})  {playerdat.playerObject.xpos},{playerdat.playerObject.ypos} ");
            // }

            if ((playerMotionParams.tilestate25 & 0x10) == 0)
            {
                //THIS SECTION MAY BE SOMETHING TO DO WITH MOMENTUM OR SWIMMING
                if ((MaybePlayerActualForwardSpeed_1_dseg_67d6_22A6 >> 2) < playerMotionParams.unk_14)
                {
                    if (MotionInputPressed == 1)
                    {
                        var cl = (playerMotionParams.unk_14 << 2) / (MaybePlayerActualForwardSpeed_1_dseg_67d6_22A6 >> 1) - 1;
                        if (cl < 2)
                        {
                            cl = 2;
                        }
                        dseg_67d6_33c6 = false;

                        RelatedToSwimDmg_dseg_67d6_33CE = (short)(dseg_67d6_743[RelatedToClockIncrement_67d6_742 >> 4] * cl);
                    }
                }
                if (MotionInputPressed == 7)
                {
                    RelatedToSwimDmg_dseg_67d6_33CE = -32;
                    dseg_67d6_33D2 = -256;
                    dseg_67d6_33c6 = true;
                    MotionInputPressed = 0;
                }
                if ((MotionInputPressed == 9) || (MotionInputPressed == 0xA))
                {
                    dseg_67d6_33c6 = true;
                    RelatedToSwimDmg_dseg_67d6_33CE = (short)(dseg_67d6_753[RelatedToClockIncrement_67d6_742 >> 4] << 1);
                }
            }
            MotionInputPressed = 0;
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


            if (playerMotionParams.unk_10_Z == 0)
            {
                CalculateMotionFromCommand_seg008_1B09_108E(MotionInputPressed, ClockIncrement, out var2);
                if (playerMotionParams.unk_10_Z == 0)
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
                    if (playerMotionParams.unk_14 <= MaybePlayerActualForwardSpeed_1_dseg_67d6_22A6)
                    {
                        if (playerMotionParams.unk_14 < 0)
                        {
                            playerMotionParams.unk_14 = 0;
                        }
                    }
                    else
                    {
                        playerMotionParams.unk_14 = MaybePlayerActualForwardSpeed_1_dseg_67d6_22A6;
                    }
                }
            }


            //seg008_1B09_83E:
            if (playerMotionParams.unk_10_Z != 0)
            {
                PlayerHeadingMinor_dseg_8294 += (short)(((ClockIncrement * MotionRelated_dseg_67d6_775) * (PlayerMotionHeading_77E / 4)) / 4);
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
                    ((((int)playerMotionParams.unk_c_X | (int)playerMotionParams.unk_e_Y | (int)playerMotionParams.unk_10_Z) == 0))
                            && ((playerdat.TileState & 0x84) == 0)
                    )
                {
                    playerMotionParams.unk_17 = 0x80;
                }
                else
                {
                    if (playerMotionParams.unk_10_Z != 0)
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
                if (((int)playerMotionParams.unk_c_X | (int)playerMotionParams.unk_e_Y | (int)playerMotionParams.unk_10_Z) == 0)
                {
                    playerMotionParams.unk_17 = 0x80;
                }
            }


            //seg008_1B09_A9D:
            //UW1 and UW2 realign here.
            if (playerMotionParams.unk_14 == 0)
            {
                PlayerHeadingMajor_dseg_67d6_8296 = PlayerHeadingMinor_dseg_8294;
            }

            //seg008_1B09_AAA:
            playerMotionParams.heading_1E = PlayerHeadingMajor_dseg_67d6_8296;
            setAt(UWMotionParamArray.PlayerMotionHandler_dseg_67d6_26AA, 0, 16, 0x0);

            if ((playerdat.MagicalMotionAbilities & 0x14) != 0)
            {
                //seg008_1B09_ABD:
                //player is flying or levitating
                setAt(UWMotionParamArray.PlayerMotionHandler_dseg_67d6_26AA, 0, 16, 0x1000);
                playerMotionParams.unk_17 = 0x80;
            }

            //seg008_1B09_AC8:
            playerMotionParams.unk_26_falldamage = 0;
            dseg_67d6_22A0 = playerMotionParams.unk_10_Z;
        }

        static void ApplyPlayerMotion(uwObject playerObj)
        {
            var di_zpos = playerObj.zpos;

            if (dseg_67d6_22AE != -1)
            {
                PlayerHeadingMajor_dseg_67d6_8296 = dseg_67d6_22AC;
                playerMotionParams.unk_14 = dseg_67d6_22AE;
            }

            playerObj.xpos = (short)((playerMotionParams.x_0 >> 5) & 0x7);
            playerObj.ypos = (short)((playerMotionParams.y_2 >> 5) & 0x7);

            //Debug.Print($"player high precision x,y = {playerMotionParams.x_0 & 0x1F},{playerMotionParams.y_2 & 0x1F}" );

            //TODO update playerObj.goal with value based on system clock

            var newTileX = playerMotionParams.x_0 >> 8;
            var newTileY = playerMotionParams.y_2 >> 8;

            if ((newTileX != playerObj.tileX) || (newTileY != playerObj.tileY))
            {
                //player has changed tiles
                playerdat.PlacePlayerInTile(newTileX: newTileX, newTileY: newTileY, previousTileX: playerObj.tileX, previousTileY: playerObj.tileY);
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
                if (playerMotionParams.heading_1E == PlayerHeadingMajor_dseg_67d6_8296)
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
            if (playerMotionParams.heading_1E != PlayerHeadingMajor_dseg_67d6_8296)
            {
                //this may be uw2 specific
                PlayerHeadingMajor_dseg_67d6_8296 = playerMotionParams.heading_1E;

                var si = playerMotionParams.heading_1E - (dseg_67d6_D0 << 0xE);
                if ((playerMotionParams.unk_17 & 0x80) != 0)
                {
                    if (SomeTileOrTerrainDatInfo_seg_67d6_D4 == -1)
                    {
                        if (Math.Abs(PlayerHeadingMinor_dseg_8294 - si) >= 0x600)
                        {
                            //seg008_1B09_EC7
                            if (PlayerHeadingMinor_dseg_8294 - si >= 0x7FFF)
                            {
                                //seg008_1B09_ED9: 
                                PlayerHeadingMinor_dseg_8294 += 0x600;
                            }
                            else
                            {
                                PlayerHeadingMinor_dseg_8294 -= 0x600;
                            }
                        }
                        else
                        {
                            //seg008_1B09_EC1:
                            PlayerHeadingMinor_dseg_8294 = (short)si;
                        }
                    }
                }

            }
            //seg008_1B09_EDF:
            playerObj.heading = (short)((PlayerHeadingMinor_dseg_8294 >> 0xD) & 0x7);
            playerObj.npc_heading = (short)((PlayerHeadingMinor_dseg_8294 >> 8) & 0x1F);

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
                var di = PlayerHeadingMinor_dseg_8294;
                switch (inputcmd)
                {
                    case 0:
                        arg4 = 0; break;
                    case 1://walk,run,turn
                        {
                            PlayerHeadingMinor_dseg_8294 += (short)((MotionRelated_dseg_67d6_775 * ClockIncrement) * (PlayerMotionHeading_77E / 4));
                            di = PlayerHeadingMinor_dseg_8294;
                            PlayerHeadingMajor_dseg_67d6_8296 = di;
                            arg4 = (short)(((PlayerMotionWalk_77C >> 2) * MaybePlayerActualForwardSpeed_1_dseg_67d6_22A6) / 0x20);
                            dseg_67d6_D0 = 0;
                            break;
                        }
                    case 6:
                    case 7://jumps
                        {
                            if (inputcmd == 6)
                            {
                                if (playerMotionParams.unk_a_pitch == 0)
                                {
                                    if (playerMotionParams.unk_10_Z == 0)
                                    {
                                        if (playerMotionParams.unk_14 == 0)
                                        {
                                            //seg008_1B09_1158:
                                            di = PlayerHeadingMinor_dseg_8294;
                                            PlayerHeadingMajor_dseg_67d6_8296 = di;
                                            arg4 = (short)(MaybePlayerActualForwardSpeed_1_dseg_67d6_22A6 / 2);
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
                                playerMotionParams.unk_10_Z = -4;
                            }
                            else
                            {
                                playerMotionParams.unk_10_Z = -2;
                            }

                            break;
                        }
                    case 8://walk backwards
                        {
                            di = (short)(di - 0x8000);
                            arg4 = MaybePlayerActualBackwardsSpeed_3_dseg_67d6_22AA;
                            dseg_67d6_D0 = -2;
                            PlayerHeadingMajor_dseg_67d6_8296 = di;
                            break;
                        }
                    case 9://slide left
                        {
                            di = (short)(di - 0x4000);
                            arg4 = MaybePlayerActualSlideSpeed_2_dseg_67d6_22A8;
                            dseg_67d6_D0 = -1;
                            PlayerHeadingMajor_dseg_67d6_8296 = di;
                            break;
                        }
                    case 0xA://slide right
                        {
                            di = (short)(di + 0x4000);
                            arg4 = MaybePlayerActualSlideSpeed_2_dseg_67d6_22A8;
                            dseg_67d6_D0 = 1;
                            PlayerHeadingMajor_dseg_67d6_8296 = di;
                            break;
                        }
                    case 0xC://flying up?
                        {
                            dseg_67d6_D0 = 0;
                            playerMotionParams.unk_a_pitch = 0x8D;
                            playerMotionParams.unk_10_Z = 0;
                            PlayerHeadingMajor_dseg_67d6_8296 = di;
                            break;
                        }
                    case 0xD://flying down?
                        {
                            dseg_67d6_D0 = 0;
                            playerMotionParams.unk_a_pitch = -141;
                            playerMotionParams.unk_10_Z = 0;
                            PlayerHeadingMajor_dseg_67d6_8296 = di;
                            break;
                        }
                    default:
                        {
                            PlayerHeadingMajor_dseg_67d6_8296 = di;
                            break;
                        }
                }
            }
        }

        public static void ProcessPlayerTileState(short tilestate, int arg2)
        {
            var InWater = false;
            if ((motion.PreviousTileState_dseg_67d6_22B4 != tilestate) || (arg2 == 0))
            {
                InWater = false;
                var NewMotionState = 0;
                motion.PreviousTileState_dseg_67d6_22B4 = tilestate;

                //Test if in water (or water current?)
                if ((tilestate & 0x22) == 0)
                {
                    //seg008_1B09_83:
                    //not in water
                    if ((tilestate & 4) != 0)
                    {
                        //in lava
                        NewMotionState = 2;
                    }
                    else
                    {
                        if ((tilestate & 8) != 0)
                        {
                            //in lava
                            NewMotionState = 3;
                        }
                        else
                        {
                            if ((tilestate & 0x10) != 0)
                            {
                                //in the air?
                                if ((playerdat.MagicalMotionAbilities & 0x4) != 0)
                                {
                                    NewMotionState = 4;
                                }
                                else
                                {
                                    if ((playerdat.MagicalMotionAbilities & 0x10) != 0)
                                    {
                                        NewMotionState = 5;
                                    }
                                    else
                                    {
                                        if ((playerdat.MagicalMotionAbilities & 0x2) != 0)
                                        {
                                            NewMotionState = 6;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //seg008_1B09_CB:
                    UpdateMotionStateAndSwimming(NewMotionState);
                    if (InWater == false)
                    {
                        playerdat.SwimCounter = 0;
                    }
                }
                else
                {
                    //in water
                    //seg008_1B09_6B
                    //test for waterwalking
                    if ((playerdat.MagicalMotionAbilities & 0x8) == 0)
                    {
                        //waterwalk not active
                        StartSwimming(tilestate);
                        InWater = true;
                        NewMotionState = 1;
                    }

                }
            }

            //seg008_1B09_E8
            if ((tilestate & 0x10) != 0)
            {
                //when jumping?
                if ((playerdat.MagicalMotionAbilities & 0x14) != 0)
                {
                    //flying or levitating
                    motion.playerMotionParams.unk_10_Z = 0;
                    if (Math.Abs(motion.playerMotionParams.unk_a_pitch) <= 0xA)
                    {
                        motion.playerMotionParams.unk_a_pitch = 0;
                    }
                    else
                    {
                        motion.playerMotionParams.unk_a_pitch = (short)((motion.playerMotionParams.unk_a_pitch << 2) / 5);
                    }
                }
                else
                {
                    if (motion.playerMotionParams.unk_10_Z == 0)
                    {
                        motion.playerMotionParams.unk_10_Z = -4;
                    }
                    if ((playerdat.MagicalMotionAbilities & 0x2) == 0) // slowfall
                    {
                        if (motion.playerMotionParams.unk_a_pitch <= -94)
                        {
                            motion.playerMotionParams.unk_a_pitch = -94;
                            if (motion.playerMotionParams.unk_14 > 0x14)
                            {
                                motion.playerMotionParams.unk_14 = (short)(motion.playerMotionParams.unk_14 / 2);
                            }
                            else
                            {
                                motion.playerMotionParams.unk_14 = 0;
                            }
                        }
                    }
                }
            }
        }


        public static void RefreshPlayerTileState()
        {
            ProcessPlayerTileState(motion.playerMotionParams.tilestate25, 1);
            motion.Examine_dseg_D3 = 1;
        }

        public static void UpdateMotionStateAndSwimming(int arg0)
        {
            //todo, check if UW1 has the same array values
            var tilestatetable_var8 = new short[] { 0x14, 0x6, 0xE, 0x14, 0x1, 0xE, 0x4 }; //likely speeds?
            var tilestatestranslation_var10 = new short[] { 0, 1, 2, 4, 8, 8, 0 };
            //             ; State   | table value
            //             ; normal  |     0
            //             ; swim    |     1
            //             ; lava    |     2
            //             ; snow/ice|     4
            //             ; levitate|     8
            //             ; fly     |     8
            //             ; slowfall|     0

            if (arg0 != -1)
            {
                playerdat.TileState = (playerdat.TileState & 0xE0) + tilestatestranslation_var10[arg0];
                playerdat.RelatedToMotionState = (playerdat.RelatedToMotionState & 0xF8) | arg0;
            }
            else
            {
                arg0 = playerdat.RelatedToMotionState & 0x7;
            }
            int si = 0;
            if (arg0 != 1)
            {
                //not swimming?
                si = tilestatetable_var8[arg0];

            }
            else
            {
                si = 4 + (playerdat.Swimming / 2);
            }
            //seg008_1B09_12F0: 
            motion.MaybePlayerActualForwardSpeed_1_dseg_67d6_22A6 = (short)((motion.MaybeBaseForwardSpeed_1_dseg_67d6_CE * si) / 0x14);
            motion.MaybePlayerActualSlideSpeed_2_dseg_67d6_22A8 = (short)((motion.MaybeBaseSlideSpeed_2_dseg_67d6_CC * si) / 0x14);
            motion.MaybePlayerActualBackwardsSpeed_3_dseg_67d6_22AA = (short)((motion.MaybeBaseBackwardsSpeed_3_dseg_67d6_CA * si) / 0x14);

            if (arg0 >= 4)
            {
                motion.dseg_67d6_22A2 = motion.MotionRelated_dseg_67d6_775;
            }
            else
            {
                motion.dseg_67d6_22A2 = (short)((motion.MotionRelated_dseg_67d6_775 * tilestatetable_var8[arg0]) / 0x14);
            }
            //seg008_1B09_1342

            if ((playerdat.WeightMax != 0) && ((playerdat.WeightCarried << 1) <= playerdat.WeightMax))
            {
                motion.MotionWeightRelated_dseg_67d6_C8 = (short)(0x60 - ((playerdat.WeightCarried * 0x60) / (playerdat.WeightMax << 1)));
            }
            else
            {
                motion.MotionWeightRelated_dseg_67d6_C8 = 0x60;
            }
        }



        /// <summary>
        /// Initiates the act of swimming in water.
        /// </summary>
        /// <param name="tilestate"></param>
        /// <returns></returns>
        static bool StartSwimming(int tilestate)
        {
            bool result = false;
            if ((tilestate & 0x2) != 0)
            {
                result = true;
                playerdat.SwimCounter = 0x60;
                playerdat.PutWeaponAway();
            }
            else
            {
                playerdat.SwimCounter = 0x10;
            }
            return result;
        }

        public static void WalkOnSpecialTerrain()
        {
            //Debug.Print("Walkonspecialterrain todo");
        }

    }//end class
}//end namespace