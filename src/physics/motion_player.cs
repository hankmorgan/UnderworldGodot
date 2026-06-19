using System;
using System.Diagnostics;


namespace Underworld
{
    /// <summary>
    /// Player Motion.
    /// </summary>
    public partial class motion : Loader
    {
        public static short[] PlayerCardinalHeadingLookupTable = { 0, 16384, -32768, -16384 };

        public static UWMotionParamArray playerMotionParams = new UWMotionParamArray();

        //These 2 globals are used to determine the heading/velocity for the player.
        public static short PlayerMotionWalk_77C = 0;
        public static short PlayerMotionHeading_77E = 0;

        /// <summary>
        /// What key is inputed. Prompts the game to process player motion.
        /// </summary>
        public static short MotionInputPressed = 0;


        static short dseg_67d6_D0 = 0;
        public static bool PlayerMotionUpdateRequired_dseg_D3 = false;


        //Camera Globals -> these should move over the the camera classes.
        public static short PlayerCameraYaw_dseg_8294;
        public static short PlayerCameraPitch_dseg_67d6_33D6 = 0; //unimplemented
        public static short PlayerCameraRoll_dseg_67d6_33D8 = 0; //unimplemented


        //Camera bob adjustments, 
        public static short CameraBobZAdjust_dseg_67d6_33CE = 0;
        public static short CameraYawModifier_dseg_67d6_33D0;//changes the camera angle that is set by PlayerHeadingMinor8294
        public static short CameraPitchModifier_dseg_67d6_33D2;//changes the camera angle that is set by PlayerHeadingRelated33D6        
        public static short CameraRollModifier_dseg_67d6_33D4;//changes the camera angle that is set by PlayerHeadingRelated33D8, 

        //These are used in shaking and are used to calculate the camera modifiers
        public static byte Shake20_Duration_73F;
        public static byte Shake40_Duration_740;
        public static byte Shake80_Duration_741; //not used in UW1

        //Used to calulate the angle for npc sprites
        public static short CameraYawHeadingRelated_2B52=0;
        public static short CameraPointer2C=0;


        //These are used to backup the player motion values when subject to sliding
        static short copyofheading1E_dseg_67d6_229A;
        static short copyofunk14_dseg_67d6_229C;
        static short copyofplayerheading_dseg_67d6_22AC;

        public static short dseg_67d6_22A2;

        public const short BaseForwardSpeed_1_dseg_67d6_CE = 0x3AC;
        public const short BaseSlideSpeed_2_dseg_67d6_CC = 0xEB;
        public const short BaseBackwardsSpeed_3_dseg_67d6_CA = 0xBC;
        public static short PlayerActualForwardSpeed_1_dseg_67d6_22A6 = 0x3AC;
        public static short PlayerActualSlideSpeed_2_dseg_67d6_22A8 = 0xEB;
        public static short PlayerActualBackwardsSpeed_3_dseg_67d6_22AA = 0xBC;


        static short dseg_67d6_22A0;//.possibly unused.

        static short dseg_67d6_22AE;//tmp

        public static short MotionWeightRelated_dseg_67d6_C8;

        public static short MotionTurnStep_dseg_67d6_775 = 0xF;

        /// <summary>
        /// Value is > -1 when on a sliding tile.
        /// </summary>
        static short TypeOfSlidingFloor_seg_67d6_D4;

        public static short PlayerMotionYaw_dseg_67d6_8296;//set by the camera yaw at the start of motion input

        public static short PreviousTileState_dseg_67d6_22B4 = 0;

        public static byte RelatedToClockIncrement_67d6_742;

        public static bool CameraIsBobbing_dseg_67d6_33c6;

        public static bool ICYFloor_dseg_229E = false;

        static sbyte[] dseg_67d6_743 = new sbyte[] { 1, 3, 4, 3, 1, -3, 0, 0, 1, 3, 4, 3, 1, -3, 0, 0 };
        static sbyte[] dseg_67d6_753 = new sbyte[] { 0, 0, -1, -2, -3 - 4, -5, -6, -6, -4, -3, -2, -1, 0, 0, 0, -4 };

        /// <summary>
        /// Moves the player by processing the inputs and forces acting on the player
        /// </summary>
        /// <param name="ClockIncrement"></param>
        public static void PlayerMotion(short ClockIncrement)
        {
            UWMotionParamArray.instance = motion.playerMotionParams;//in case we step on a jump trap...
            
            //These values are used in camera bobbing/shaking
            CameraRollModifier_dseg_67d6_33D4 = 0;
            CameraPitchModifier_dseg_67d6_33D2 = 0;
            CameraYawModifier_dseg_67d6_33D0 = 0;

            // var x_init = playerMotionParams.x_0;
            // var y_init = playerMotionParams.y_2;
            playerMotionParams.radius_22 = (byte)commonObjDat.radius(playerdat.playerObject.item_id);
            playerMotionParams.height_23 = (byte)commonObjDat.height(playerdat.playerObject.item_id);
            var initial = PlayerMotionYaw_dseg_67d6_8296;
            PlayerMotionInitialCalculation_seg008_1B09_7B2(ClockIncrement);

            CalculateMotion(
                projectile: playerdat.playerObject,
                MotionParams: playerMotionParams,
                SpecialMotionHandler: MotionHandler.PlayerMotionHandler);

            ApplyPlayerMotion(playerdat.playerObject);
            // if (initial != PlayerMotionYaw_dseg_67d6_8296)
            // {
            //     Debug.Print($"{initial} -> {PlayerMotionYaw_dseg_67d6_8296}");
            // }

            // playerdat.heading_major = PlayerHeadingMajor_dseg_67d6_8296 >> 8;//this hack fixes turning but the heading value here is actually direction of motion so the camera turns during backwards and sideways motion

            //Debug.Print($"playerpos is now {playerMotionParams.z_4}");
            // if ((x_init != playerMotionParams.x_0) || (y_init != playerMotionParams.y_2))
            // {
            //     Debug.Print($"Motion is {x_init - playerMotionParams.x_0},{y_init - playerMotionParams.y_2}");
            // }


            if ((playerMotionParams.tilestate25 & 0x10) == 0)
            {
                //THIS SECTION SETS UP WALKING CAMERA BOB
                if ((PlayerActualForwardSpeed_1_dseg_67d6_22A6 >> 2) < playerMotionParams.momentum_14)
                {
                    if (MotionInputPressed == 1)
                    {
                        var cl = (playerMotionParams.momentum_14 << 2) / (PlayerActualForwardSpeed_1_dseg_67d6_22A6 >> 1) - 1;
                        if (cl < 2)
                        {
                            cl = 2;
                        }
                        CameraIsBobbing_dseg_67d6_33c6 = true;

                        CameraBobZAdjust_dseg_67d6_33CE = (short)(dseg_67d6_743[RelatedToClockIncrement_67d6_742 >> 4] * cl);
                    }
                }
                if (MotionInputPressed == 7)
                {
                    CameraBobZAdjust_dseg_67d6_33CE = -32;
                    CameraPitchModifier_dseg_67d6_33D2 = -256;
                    CameraIsBobbing_dseg_67d6_33c6 = true;
                    MotionInputPressed = 0;
                }
                if ((MotionInputPressed == 9) || (MotionInputPressed == 0xA))
                {//strafe motion.
                    CameraIsBobbing_dseg_67d6_33c6 = true;
                    CameraBobZAdjust_dseg_67d6_33CE = (short)(dseg_67d6_753[RelatedToClockIncrement_67d6_742 >> 4] << 1);
                }
            }
            MotionInputPressed = 0;
        }
        static void PlayerMotionInitialCalculation_seg008_1B09_7B2(short ClockIncrement)
        {

            short di = 0;
            short var2 = 0;
            playerMotionParams.unk_16_relatedtoPitch = 5;
            playerMotionParams.unk_17 = 0;
            byte var4 = 0;


            copyofheading1E_dseg_67d6_229A = playerMotionParams.heading_1E;
            copyofunk14_dseg_67d6_229C = playerMotionParams.momentum_14;


            if (playerMotionParams.gravity_10_Z == 0)
            {
                CalculateMotionFromCommand_seg008_1B09_108E(MotionInputPressed, ClockIncrement, out var2);
                if (playerMotionParams.gravity_10_Z == 0)
                {
                    //seg008_1B09_7F6
                    di = (short)(var2 - playerMotionParams.momentum_14);

                    if (Math.Abs(di) > MotionWeightRelated_dseg_67d6_C8)
                    {
                        if (di > 0)
                        {
                            di = (short)(1 * MotionWeightRelated_dseg_67d6_C8);//this global is wrong!
                        }
                        else
                        {
                            di = (short)(-1 * MotionWeightRelated_dseg_67d6_C8);
                        }

                    }
                    //seg008_1B09_81C
                    playerMotionParams.momentum_14 += di;
                    if (playerMotionParams.momentum_14 <= PlayerActualForwardSpeed_1_dseg_67d6_22A6)
                    {
                        if (playerMotionParams.momentum_14 < 0)
                        {
                            playerMotionParams.momentum_14 = 0;
                        }
                    }
                    else
                    {
                        playerMotionParams.momentum_14 = PlayerActualForwardSpeed_1_dseg_67d6_22A6;
                    }
                }
            }


            //seg008_1B09_83E:
            if (playerMotionParams.gravity_10_Z != 0)
            {
                PlayerCameraYaw_dseg_8294 += (short)(((ClockIncrement * MotionTurnStep_dseg_67d6_775) * (PlayerMotionHeading_77E / 4)) / 4);
            }


            if (_RES == GAME_UW2)
            {
                short HeadingVar6;
                short MaybeMovementVectorVar8;
                var tile = UWTileMap.current_tilemap.Tiles[playerdat.playerObject.tileX, playerdat.playerObject.tileY];
                //seg008_1B09_862:

                if ((playerdat.TileState & 0x4) != 0)
                {
                    //seg008_870 -> ice/snow                    
                    var terrainVarA = TerrainDatLoader.GetTerrainDataBit345(tile);  // (short)((TerrainDatLoader.Terrain[tile.floorTexture] & 0x38) >> 3);

                    if (terrainVarA != 7)
                    {
                        //seg008_89F
                        ICYFloor_dseg_229E = true;


                        if ((tile.tileType >= 6) && (tile.tileType <= 9))
                        {
                            //seg008_8C8
                            TypeOfSlidingFloor_seg_67d6_D4 = (short)(tile.tileType - 6);
                            var4 = 0x2F;
                        }
                        //seg008_8CD
                        playerMotionParams.unk_16_relatedtoPitch = (byte)(0xF - terrainVarA);
                        terrainVarA = (short)(((terrainVarA << 3) + 8) - ((copyofunk14_dseg_67d6_229C / 0x2F) << 2));

                        if (copyofunk14_dseg_67d6_229C >= 0x2F)
                        {
                            //seg008_8FE
                            if (playerMotionParams.momentum_14 > copyofunk14_dseg_67d6_229C)
                            {
                                if (TypeOfSlidingFloor_seg_67d6_D4 == -1)
                                {
                                    terrainVarA += 8;
                                }
                                else
                                {
                                    terrainVarA += 4;
                                }
                            }
                        }
                        else
                        {
                            //seg008_8F8
                            terrainVarA += 0x10;
                        }
                        HeadingVar6 = 0;
                        //seg008_918
                        ApplyWaterCurrentIceSliding(
                            TypeOfMotionArg0: 2,
                            HeadingArg2: copyofheading1E_dseg_67d6_229A,
                            arg4: copyofunk14_dseg_67d6_229C,
                            HeadingArg6: PlayerMotionYaw_dseg_67d6_8296,
                            INunk14_arg8: playerMotionParams.momentum_14,
                            argA: terrainVarA,
                            newHeading1E_argC: out HeadingVar6,
                            newUnk14_argE: out MaybeMovementVectorVar8, test: HeadingVar6);

                        PlayerMotionYaw_dseg_67d6_8296 = HeadingVar6;
                        playerMotionParams.momentum_14 = MaybeMovementVectorVar8;

                        if (copyofunk14_dseg_67d6_229C + MotionWeightRelated_dseg_67d6_C8 <= playerMotionParams.momentum_14)
                        {
                            if ((Rng.r.Next(0x7fff) & 0x3) == 0)
                            {
                                //Seg008_96C
                                SetScreenShake(0x80, 4);
                                //--> seg008_9D6
                            }
                            else
                            {
                                // seg008_95F
                                if (copyofunk14_dseg_67d6_229C - MotionWeightRelated_dseg_67d6_C8 >= playerMotionParams.momentum_14)
                                {
                                    //Seg008_96C
                                    SetScreenShake(0x80, 4);
                                    //--> seg008_9D6
                                }
                            }
                        }
                        else
                        {
                            // seg008_95F
                            if (copyofunk14_dseg_67d6_229C - MotionWeightRelated_dseg_67d6_C8 >= playerMotionParams.momentum_14)
                            {
                                //Seg008_96C
                                SetScreenShake(0x80, 4);
                                //--> seg008_9D6
                            }
                        }
                    }
                    else
                    {
                        //Seg008_87B
                        TypeOfSlidingFloor_seg_67d6_D4 = -1;
                        ICYFloor_dseg_229E = false;
                        //-->Seg008_9D6
                    }
                }
                else
                {
                    //Set 1B09_988 
                    if ((playerdat.TileState & 0x1) == 0)
                    {
                        //seg008_9D0
                        TypeOfSlidingFloor_seg_67d6_D4 = -1;
                    }
                    else
                    {
                        //seg008_997
                        var si = TypeOfSlidingFloor_seg_67d6_D4;
                        //SomeTileOrTerrainDatInfo_seg_67d6_D4 = (short)(((TerrainDatLoader.Terrain[tile.floorTexture] & 0x38) >> 3) - 1);
                        TypeOfSlidingFloor_seg_67d6_D4 = (short)(TerrainDatLoader.GetTerrainDataBit345(tile) - 1);
                        if (TypeOfSlidingFloor_seg_67d6_D4 == -1)
                        {
                            TypeOfSlidingFloor_seg_67d6_D4 = si;
                        }
                        //seg008_9C9
                        var4 = 0x8D;
                    }

                }
                //Seg008_1B08_9D6
                dseg_67d6_22AE = -1;
                if (TypeOfSlidingFloor_seg_67d6_D4 != -1)
                {
                    //seg008_9E3
                    short si = 0;
                    if (TypeOfSlidingFloor_seg_67d6_D4 <= 3)
                    {
                        //seg008_9EC
                        if (TypeOfSlidingFloor_seg_67d6_D4 > 1)
                        {
                            si = 0x4000;
                        }
                        if ((TypeOfSlidingFloor_seg_67d6_D4 & 0x1) == 0)
                        {
                            si = (short)(si + 0x8000);
                        }
                    }

                    //seg008_A05
                    HeadingVar6 = var4; //note. I had to initialise var4 with 0..
                    copyofheading1E_dseg_67d6_229A = PlayerMotionYaw_dseg_67d6_8296;
                    copyofunk14_dseg_67d6_229C = playerMotionParams.momentum_14;
                    ApplyWaterCurrentIceSliding(
                        TypeOfMotionArg0: 3,
                        HeadingArg2: PlayerMotionYaw_dseg_67d6_8296,
                        arg4: playerMotionParams.momentum_14,
                        HeadingArg6: si,
                        INunk14_arg8: HeadingVar6,
                        argA: 0x20,
                        newHeading1E_argC: out copyofheading1E_dseg_67d6_229A,
                        newUnk14_argE: out copyofunk14_dseg_67d6_229C, test: copyofheading1E_dseg_67d6_229A);

                    if (var4 != 0x2F)
                    {
                        //seg008_A40
                        copyofplayerheading_dseg_67d6_22AC = PlayerMotionYaw_dseg_67d6_8296;
                        dseg_67d6_22AE = playerMotionParams.momentum_14;
                    }
                    //seg008_A46
                    if (copyofunk14_dseg_67d6_229C > PlayerActualForwardSpeed_1_dseg_67d6_22A6)
                    {
                        //seg008_A52
                        copyofunk14_dseg_67d6_229C = PlayerActualForwardSpeed_1_dseg_67d6_22A6;
                    }
                    //seg008_A55
                    PlayerMotionYaw_dseg_67d6_8296 = copyofheading1E_dseg_67d6_229A;
                    playerMotionParams.momentum_14 = copyofunk14_dseg_67d6_229C;
                }
                // rejoin at Seg008_A61 
            }//end uw2 specific code.

            //Seg008_a61
            playerMotionParams.speed_12 = (sbyte)ClockIncrement;//too low a value breaks some motion. too high a value makes turning too fast.

            if (_RES != GAME_UW2)
            {
                //seg008_67C UW1 dissassmbly
                playerMotionParams.unk_16_relatedtoPitch = 5;
                playerMotionParams.unk_17 = 0;
            }

            if (_RES == GAME_UW2)
            {
                //Seg008_A61
                if (
                    ((((int)playerMotionParams.unk_c_X | (int)playerMotionParams.unk_e_Y | (int)playerMotionParams.gravity_10_Z) == 0))
                            && ((playerdat.TileState & 0x84) == 0)
                    )
                {
                    playerMotionParams.unk_17 = 0x80;
                }
                else
                {
                    if (playerMotionParams.gravity_10_Z != 0)
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
                if (((int)playerMotionParams.unk_c_X | (int)playerMotionParams.unk_e_Y | (int)playerMotionParams.gravity_10_Z) == 0)
                {
                    playerMotionParams.unk_17 = 0x80;
                }
            }



            //seg008_1B09_A9D:
            //UW1 and UW2 realign here.
            if (playerMotionParams.momentum_14 == 0)
            {
                PlayerMotionYaw_dseg_67d6_8296 = PlayerCameraYaw_dseg_8294;
            }

            //seg008_1B09_AAA:
            playerMotionParams.heading_1E = PlayerMotionYaw_dseg_67d6_8296;
            setAt(MotionHandler.PlayerMotionHandler.handlerdata, 0, 16, 0x0);  //setAt(UWMotionParamArray.PlayerMotionHandler_dseg_67d6_26AA, 0, 16, 0x0);

            if ((playerdat.MagicalMotionAbilities & 0x14) != 0)
            {
                //seg008_1B09_ABD:
                //player is flying or levitating
                setAt(MotionHandler.PlayerMotionHandler.handlerdata, 0, 16, 0x1000); //UWMotionParamArray.PlayerMotionHandler_dseg_67d6_26AA
                playerMotionParams.unk_17 = 0x80;
            }

            //seg008_1B09_AC8:
            playerMotionParams.unk_26_falldamage = 0;
            if (_RES == GAME_UW2)
            {
                dseg_67d6_22A0 = playerMotionParams.gravity_10_Z;
            }

        }

        static void ApplyPlayerMotion(uwObject playerObj)
        {
            var di_zpos = playerObj.zpos;
            if (_RES == GAME_UW2)
            {
                if (dseg_67d6_22AE != -1)
                {
                    PlayerMotionYaw_dseg_67d6_8296 = copyofplayerheading_dseg_67d6_22AC;
                    playerMotionParams.momentum_14 = dseg_67d6_22AE;
                }
            }


            playerObj.xpos = (short)((playerMotionParams.x_0 >> 5) & 0x7);
            playerObj.ypos = (short)((playerMotionParams.y_2 >> 5) & 0x7);

            //Addition. change of zpos
            playerObj.zpos = (short)(playerMotionParams.z_4 >> 3);

            //Debug.Print($"player high precision x,y = {playerMotionParams.x_0 & 0x1F},{playerMotionParams.y_2 & 0x1F}" );

            //TODO update playerObj.goal with value based on system clock

            var newTileX = playerMotionParams.x_0 >> 8;
            var newTileY = playerMotionParams.y_2 >> 8;

            if ((newTileX != playerObj.tileX) || (newTileY != playerObj.tileY))
            {
                //player has changed tiles
                playerdat.PlacePlayerInTile(
                    newTileX: newTileX, newTileY: newTileY,
                    previousTileX: playerObj.tileX, previousTileY: playerObj.tileY);

                playerObj.npc_xhome = (short)(playerMotionParams.x_0 >> 8);
                playerObj.npc_yhome = (short)(playerMotionParams.y_2 >> 8);

                //TODO Check if player is no longer lost

                playerdat.PlayerStatusUpdate();//to force lighting refreshes when player has changed tile.
            }
            else
            {
                //player is in same tile
                if (di_zpos != (playerMotionParams.z_4 >> 3))
                {
                    if (_RES == GAME_UW2)
                    {
                        //player has changed height in the same tile. Check if pressure triggers in the tile need to be ran
                        trigger.PressureTriggerZChange(
                        obj: playerObj,
                        tile: UWTileMap.current_tilemap.Tiles[playerObj.tileX, playerObj.tileY],
                        zParam: playerMotionParams.z_4 >> 3);
                    }
                }
            }


            if (_RES == GAME_UW2)
            {
                //seg008_1B09_E5B
                if (playerMotionParams.unk_26_falldamage != 0)
                {
                    if (playerMotionParams.heading_1E == PlayerMotionYaw_dseg_67d6_8296)
                    {
                        if ((playerdat.TileState & 0x4) == 0)
                        {
                            if (dseg_67d6_22AE == -1)
                            {
                                playerMotionParams.momentum_14 = 0;
                            }
                        }
                    }
                }
            }
            else
            {
                //Seg008_A49 in UW1
                if (playerMotionParams.unk_26_falldamage != 0)
                {
                    if (playerMotionParams.heading_1E == PlayerMotionYaw_dseg_67d6_8296)
                    {
                        playerMotionParams.momentum_14 = 0;
                    }
                }
            }


            //seg008_1B09_E8A:
            if (playerMotionParams.heading_1E != PlayerMotionYaw_dseg_67d6_8296)
            {
                //this may be uw2 specific
                PlayerMotionYaw_dseg_67d6_8296 = playerMotionParams.heading_1E;

                var si = playerMotionParams.heading_1E - (dseg_67d6_D0 << 0xE);
                if ((playerMotionParams.unk_17 & 0x80) != 0)
                {
                    if (TypeOfSlidingFloor_seg_67d6_D4 == -1)
                    {
                        if (Math.Abs(PlayerCameraYaw_dseg_8294 - si) >= 0x600)
                        {
                            //seg008_1B09_EC7
                            if (PlayerCameraYaw_dseg_8294 - si >= 0x7FFF)
                            {
                                //seg008_1B09_ED9: 
                                PlayerCameraYaw_dseg_8294 += 0x600;
                            }
                            else
                            {
                                PlayerCameraYaw_dseg_8294 -= 0x600;
                            }
                        }
                        else
                        {
                            //seg008_1B09_EC1:
                            PlayerCameraYaw_dseg_8294 = (short)si;
                        }
                    }
                }

            }
            //seg008_1B09_EDF:  (SEG_008_AAD in UW1 version)
            playerObj.heading = (short)((PlayerCameraYaw_dseg_8294 >> 0xD) & 0x7);
            playerObj.npc_heading = (short)((PlayerCameraYaw_dseg_8294 >> 8) & 0x1F);

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
                        //play landing sound effect
                        UWsoundeffects.PlaySoundEffectAtAvatar(UWsoundeffects.SoundEffectLanding, 0x40, (byte)((FallDamage << 2) - 60));
                    }
                }
                //seg008_1B09_FDB:
                playerMotionParams.unk_26_falldamage = 0;
            }

            //seg008_1B09_FE1:
            ProcessPlayerTileState(playerMotionParams.tilestate25, 0);

            if (_RES == GAME_UW2)
            {
                //uw2 only. Checks if player is sliding and as such motion needs to be processed again at the next interval.=
                if ((playerdat.TileState & 0x4) == 0)
                {//Seg008_DDB
                    if ((playerdat.TileState & 0x1) == 0)
                    {
                        PlayerMotionUpdateRequired_dseg_D3 = false;
                    }
                    else
                    {
                        if (TypeOfSlidingFloor_seg_67d6_D4 == -1)
                        {
                            PlayerMotionUpdateRequired_dseg_D3 = false;
                        }
                        else
                        {
                            PlayerMotionUpdateRequired_dseg_D3 = true; //subject to water current/ice sliding
                        }
                    }
                }
                else
                {
                    PlayerMotionUpdateRequired_dseg_D3 = true;
                }
            }
            else
            {
                PlayerMotionUpdateRequired_dseg_D3 = false;
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
            if (playerdat.ParalyseTimer != 0)
            {
                arg4 = 0;
            }
            else
            {
                var di = PlayerCameraYaw_dseg_8294;
                switch (inputcmd)
                {
                    case 0:
                        arg4 = 0; break;
                    case 1://walk,run,turn
                        {
                            PlayerCameraYaw_dseg_8294 += (short)((((MotionTurnStep_dseg_67d6_775 * ClockIncrement)) * (PlayerMotionHeading_77E / 4)) / 4);
                            di = PlayerCameraYaw_dseg_8294;
                            PlayerMotionYaw_dseg_67d6_8296 = PlayerCameraYaw_dseg_8294;
                            arg4 = (short)(((PlayerMotionWalk_77C >> 2) * PlayerActualForwardSpeed_1_dseg_67d6_22A6) / 0x20);
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
                                    if (playerMotionParams.gravity_10_Z == 0)
                                    {
                                        if (playerMotionParams.momentum_14 == 0)
                                        {
                                            //seg008_1B09_1158:
                                            di = PlayerCameraYaw_dseg_8294;
                                            PlayerMotionYaw_dseg_67d6_8296 = di;
                                            arg4 = (short)(PlayerActualForwardSpeed_1_dseg_67d6_22A6 / 2);
                                            playerMotionParams.momentum_14 = arg4;
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
                                playerMotionParams.gravity_10_Z = -4;
                            }
                            else
                            {
                                playerMotionParams.gravity_10_Z = -2;
                            }

                            break;
                        }
                    case 8://walk backwards
                        {
                            di = (short)(di - 0x8000);
                            arg4 = PlayerActualBackwardsSpeed_3_dseg_67d6_22AA;
                            dseg_67d6_D0 = -2;
                            PlayerMotionYaw_dseg_67d6_8296 = di;
                            break;
                        }
                    case 9://slide left
                        {
                            di = (short)(di - 0x4000);
                            arg4 = PlayerActualSlideSpeed_2_dseg_67d6_22A8;
                            dseg_67d6_D0 = -1;
                            PlayerMotionYaw_dseg_67d6_8296 = di;
                            break;
                        }
                    case 0xA://slide right
                        {
                            di = (short)(di + 0x4000);
                            arg4 = PlayerActualSlideSpeed_2_dseg_67d6_22A8;
                            dseg_67d6_D0 = 1;
                            PlayerMotionYaw_dseg_67d6_8296 = di;
                            break;
                        }
                    case 0xC://flying up?
                        {
                            dseg_67d6_D0 = 0;
                            playerMotionParams.unk_a_pitch = 0x8D;
                            playerMotionParams.gravity_10_Z = 0;
                            PlayerMotionYaw_dseg_67d6_8296 = di;
                            break;
                        }
                    case 0xD://flying down?
                        {
                            dseg_67d6_D0 = 0;
                            playerMotionParams.unk_a_pitch = -141;
                            playerMotionParams.gravity_10_Z = 0;
                            PlayerMotionYaw_dseg_67d6_8296 = di;
                            break;
                        }
                    default:
                        {
                            PlayerMotionYaw_dseg_67d6_8296 = di;
                            break;
                        }
                }
            }
        }

        public static void ProcessPlayerTileState(short tilestate, int arg2)
        {
            var InWater = false;
            if ((motion.PreviousTileState_dseg_67d6_22B4 != tilestate) || (arg2 != 0))
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
                //seg008_1B09_CB:
                UpdateMotionStateAndSwimming(NewMotionState);
                if (InWater == false)
                {
                    playerdat.SwimCounter = 0;
                }
            }

            //seg008_1B09_E8
            if ((tilestate & 0x10) != 0)
            {
                //when jumping?
                if ((playerdat.MagicalMotionAbilities & 0x14) != 0)
                {
                    //flying or levitating
                    motion.playerMotionParams.gravity_10_Z = 0;
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
                    if (motion.playerMotionParams.gravity_10_Z == 0)
                    {
                        motion.playerMotionParams.gravity_10_Z = -4;
                    }
                    if ((playerdat.MagicalMotionAbilities & 0x2) != 0) // slowfall
                    {
                        if (motion.playerMotionParams.unk_a_pitch <= -94)
                        {
                            motion.playerMotionParams.unk_a_pitch = -94;
                            if (motion.playerMotionParams.momentum_14 > 0x14)
                            {
                                motion.playerMotionParams.momentum_14 = (short)(motion.playerMotionParams.momentum_14 / 2);
                            }
                            else
                            {
                                motion.playerMotionParams.momentum_14 = 0;
                            }
                        }
                    }
                }
            }
        }

        public static void RefreshPlayerTileState()
        {
            ProcessPlayerTileState(motion.playerMotionParams.tilestate25, 1);
            PlayerMotionUpdateRequired_dseg_D3 = true;
        }

        public static void UpdateMotionStateAndSwimming(int arg0)
        {
            short[] tilestatetable_var8;
            if (_RES == GAME_UW2)
            {
                tilestatetable_var8 = new short[] { 0x14, 0x6, 0xE, 0x14, 0x1, 0xE, 0x4 }; //likely speeds?
            }
            else
            {
                tilestatetable_var8 = new short[] { 0xA, 0x3, 0x5, 0xA, 0x1, 0x7, 0x2 };
            }
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
            if ((arg0 != 1))
            {
                //not swimming?/UW1
                si = tilestatetable_var8[arg0];
            }
            else
            {
                //swimming speed adjustment (UW2 only)
                si = 4 + (playerdat.Swimming / 2);
            }
            //seg008_1B09_12F0:             
            if (_RES == GAME_UW2)
            {
                //slow down player when swimming
                motion.PlayerActualForwardSpeed_1_dseg_67d6_22A6 = (short)((motion.BaseForwardSpeed_1_dseg_67d6_CE * si) / 0x14);
                motion.PlayerActualSlideSpeed_2_dseg_67d6_22A8 = (short)((motion.BaseSlideSpeed_2_dseg_67d6_CC * si) / 0x14);
                motion.PlayerActualBackwardsSpeed_3_dseg_67d6_22AA = (short)((motion.BaseBackwardsSpeed_3_dseg_67d6_CA * si) / 0x14);
            }
            else
            {
                //TODO: confirm for certain there is no speed penalty when the player is swimming.
                si = tilestatetable_var8[arg0];
                motion.PlayerActualForwardSpeed_1_dseg_67d6_22A6 = (short)((motion.BaseForwardSpeed_1_dseg_67d6_CE * si) / 0xA);
                motion.PlayerActualSlideSpeed_2_dseg_67d6_22A8 = (short)((motion.BaseSlideSpeed_2_dseg_67d6_CC * si) / 0xA);
                motion.PlayerActualBackwardsSpeed_3_dseg_67d6_22AA = (short)((motion.BaseBackwardsSpeed_3_dseg_67d6_CA * si) / 0xA);
            }


            if (arg0 >= 4)
            {
                motion.dseg_67d6_22A2 = motion.MotionTurnStep_dseg_67d6_775;
            }
            else
            {
                motion.dseg_67d6_22A2 = (short)((motion.MotionTurnStep_dseg_67d6_775 * tilestatetable_var8[arg0]) / 0x14);
            }
            //seg008_1B09_1342

            if ((playerdat.WeightMax != 0) && ((playerdat.WeightCarried << 1) > playerdat.WeightMax))
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
            if ((tilestate & 0x2) != 0)
            {
                playerdat.SwimCounter = 0x60;
                playerdat.PutWeaponAway();
                return true;
            }
            else
            {
                playerdat.SwimCounter = 0x10;
                return false;
            }
        }

        /// <summary>
        /// Sets up the type and duration of screenshake to be applied on the next iteration of WalkOnSurfaceType
        /// </summary>
        /// <param name="TypeOfShake"></param>
        /// <param name="duration"></param>
        public static void SetScreenShake(byte TypeOfShake, byte duration)
        {
            switch (TypeOfShake)
            {
                case 0x20:
                    Shake20_Duration_73F = duration;
                    break;
                case 0x40:
                    Shake40_Duration_740 = duration;
                    break;
                case 0x80:
                    if (_RES == GAME_UW2)
                    {
                        //UW2 only form of shaking
                        Shake80_Duration_741 = duration;
                        break;
                    }
                    return;
                default:
                    Debug.Print($"invalid TypeOfShake {TypeOfShake}");
                    return;//not valid. do not apply
            }
            playerdat.TileState |= TypeOfShake;
        }

        /// <summary>
        /// Handles walking on lava, ice and water. Does calcs for screen shaking adjustments.
        /// </summary>
        public static void WalkOnSurfaceType()
        {
            //Seg35_A0C
            var var1 = 1;
            var var2_incrementrelated = 1;
            if (playerdat.TileState != 0)
            {
                CameraIsBobbing_dseg_67d6_33c6 = true; //probably means the camera will need to be adjusted.
                CameraRollModifier_dseg_67d6_33D4 = 0;//possibly the camera adjustments
                CameraPitchModifier_dseg_67d6_33D2 = 0;
                CameraYawModifier_dseg_67d6_33D0 = 0; // this moves the camera forward.

                if ((playerdat.TileState & 0x11) != 0)
                {
                    //seg35_A59
                    CameraBobZAdjust_dseg_67d6_33CE = (short)-playerdat.SwimCounter;
                    if (playerdat.SwimCounter > 0x50)
                    {
                        //seg35_A6E
                        var1 = (playerMotionParams.momentum_14 << 2) / (PlayerActualForwardSpeed_1_dseg_67d6_22A6 >> 1) - 3;
                        if (var1 < 1)
                        {
                            var1 = 1;
                        }
                        var2_incrementrelated = RelatedToClockIncrement_67d6_742 >> 4;
                        if (playerMotionParams.momentum_14 != 0)
                        {
                            //Seg31AB_AAD                            
                            CameraRollModifier_dseg_67d6_33D4 = (short)((CameraBobArray[var2_incrementrelated] * var1) << 6);
                        }
                        else
                        {
                            CameraRollModifier_dseg_67d6_33D4 = (short)((Rng.r.Next(0x7FFF) & 0x1FF) - 256);
                        }

                        //seg35_AC5
                        CameraBobZAdjust_dseg_67d6_33CE += (short)((CameraBobArray[(var2_incrementrelated + 2) & 0xF] << 1) * var1);

                        CameraYawModifier_dseg_67d6_33D0 = (short)(((Rng.r.Next(0x7FFF) & 0x7F) - 64) * var1);
                        CameraPitchModifier_dseg_67d6_33D2 = (short)(((Rng.r.Next(0x7FFF) & 0x7F) - 64) * var1);

                        //Debug.Print($"swimbob {CameraBobZAdjust_dseg_67d6_33CE},{dseg_67d6_33D4_modifiescamera2A}");
                        //Debug.Print($"swim adjust by {dseg_67d6_33D0_modifiescamera2c},{dseg_67d6_33D2_modifiescamera28},{dseg_67d6_33D4_modifiescamera2A}");
                    }
                }
                //seg35_b14
                if ((playerdat.TileState & 0x2) != 0)
                {
                    //Seg35_B1F
                    //On lava
                    if (Rng.r.Next(5) == 0)
                    {
                        //seg35_B30
                        if (playerdat.DragonSkinBoots == false)
                        {
                            damage.DamageObject(objToDamage: playerdat.playerObject, basedamage: 1, damagetype: 8, objList: UWTileMap.current_tilemap.LevelObjects, WorldObject: true, damagesource: 0);
                        }

                        if (_RES == GAME_UW2)
                        {
                            //Handle player steping on lava for baking mud.
                            //Seg35_B4A
                            if (playerdat.GetXClock(3) == 3)
                            {
                                //seg35_B55
                                playerdat.SetXClock(3, 4);
                                uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x14E));//the oily mud bakes on your skin.
                            }
                        }
                    }

                }

                //Seg35_B64
                if ((playerdat.TileState & 8) != 0)
                {
                    //seg35_B74
                    CameraBobZAdjust_dseg_67d6_33CE = (short)(Math.Abs(0x10 - (RelatedToClockIncrement_67d6_742 >> 3)) * 3);
                }

                //Seg35_B88
                if ((playerdat.TileState & 0xE0) != 0) //checking for screenshaking
                {
                    //seg35_B99
                    if ((playerdat.TileState & 0x40) != 0)
                    {
                        //shake40,
                        var tmp = Shake40_Duration_740;
                        Shake40_Duration_740--;
                        if (tmp == 0)
                        {
                            //seg35_BAB
                            //end shake
                            playerdat.TileState = playerdat.TileState & 0xBF;//clear bit 6
                        }
                        //seg35_BBE
                        var1 = Shake40_Duration_740 / 0xA;
                        if (var1 > 8)
                        {
                            //seg35_BD2
                            var1 = 8;
                        }
                    }
                    //Seg35_BD6
                    if ((playerdat.TileState & 0x20) != 0)
                    {
                        var tmp = Shake20_Duration_73F;
                        Shake20_Duration_73F--;
                        if (tmp == 0)
                        {
                            //seg35_BF2
                            //turn off shake
                            playerdat.TileState = playerdat.TileState & 0xDF;//clear bit 5
                        }
                        //seg35_BFF
                        var2_incrementrelated = Shake20_Duration_73F / 0xA;
                        if (var2_incrementrelated > 3)
                        {
                            var2_incrementrelated = 3;
                        }
                    }


                    //Seg35_C17                    
                    if ((playerdat.TileState & 0x80) != 0)
                    {
                        var tmp = Shake80_Duration_741;
                        Shake80_Duration_741--;
                        if (tmp == 0)
                        {
                            //seg35_C2D
                            //turn off shake
                            playerdat.TileState = playerdat.TileState & 0x7F;
                        }
                        //Seg_C40
                        var2_incrementrelated = 0;
                        CameraYawModifier_dseg_67d6_33D0 = (short)(((Rng.r.Next(0x7FFF) & 0x1FF) - 256) << 2);
                    }
                    //seg35_C56

                    var1 += var2_incrementrelated;
                    CameraYawModifier_dseg_67d6_33D0 = (short)(var1 * ((Rng.r.Next(0x7FFF) & 0xFF) - 128));
                    CameraPitchModifier_dseg_67d6_33D2 = (short)(var1 * ((Rng.r.Next(0x7FFF) & 0x7F) - 64));
                    CameraRollModifier_dseg_67d6_33D4 = (short)(var1 * ((Rng.r.Next(0x7FFF) & 0x1FF) - 256));
                    //Debug.Print($"Screenshake by {CameraYawModifier_dseg_67d6_33D0},{CameraPitchModifier_dseg_67d6_33D2},{CameraRollModifier_dseg_67d6_33D4}");
                }
            }
            else
            {
                //Seg35_A24
                CameraIsBobbing_dseg_67d6_33c6 = false;
                CameraBobZAdjust_dseg_67d6_33CE = 0;
            }
        }

        static void ApplyWaterCurrentIceSliding(short TypeOfMotionArg0, short HeadingArg2, short arg4, short HeadingArg6, short INunk14_arg8, short argA, out short newHeading1E_argC, out short newUnk14_argE, short test)
        {
            if (argA > 0)
            {
                //seg008_5DD
                if (argA > 0x40)
                {
                    //seg008_5E3
                    argA = 0x40;
                }

                //Seg008_5E8
                var xvar2 = 0; var yvar4 = 0;
                GetVectorForDirection((ushort)HeadingArg2, ref yvar4, ref xvar2);

                var xvar6 = 0; var yvar8 = 0;
                GetVectorForDirection((ushort)HeadingArg6, ref yvar8, ref xvar6);

                xvar2 = xvar2 / 0x800;
                yvar4 = yvar4 / 0x800;
                xvar6 = xvar6 / 0x800;
                yvar8 = yvar8 / 0x800;

                xvar2 = xvar2 * arg4;
                yvar4 = yvar4 * arg4;

                xvar6 = xvar6 * INunk14_arg8;
                yvar8 = yvar8 * INunk14_arg8;


                var varE = 0; var di = 0;
                switch (TypeOfMotionArg0)
                {
                    case 2:
                        {
                            //seg008_668
                            var varA = xvar6 - xvar2;
                            var varC = yvar8 - yvar4;

                            varE = xvar2 + ((argA * varA) / 0x40);
                            di = yvar4 + ((argA * varC) / 0x40);
                            break;
                        }
                    case 3:
                        {
                            //seg008_6C3
                            varE = xvar6 + xvar2;
                            di = yvar8 + yvar4;
                            break;
                        }
                }

                //seg008_6D4
                var var12 = varE / 0x10;
                var var16 = di / 0x10;
                newUnk14_argE = (short)(Math.Sqrt((var12 * var12) + (var16 * var16)));
                if (newUnk14_argE <= 1)
                {
                    //seg008_72F
                    newUnk14_argE = 0;
                }
                if (newUnk14_argE == 0)
                {
                    //Seg008_7A6
                    newHeading1E_argC = HeadingArg2;
                }
                else
                {
                    //Seg008_73F
                    var12 = var12 * 0x7FFF;
                    var16 = var16 * 0x7FFF;

                    var12 = var12 / newUnk14_argE;
                    var16 = var16 / newUnk14_argE;

                    varE = var12;
                    di = var16;

                    // if (di == -27336)
                    // {
                    //     Debug.Print("HERE");//varE =-18284
                    // }
                    newHeading1E_argC = MaybeGetTangent_seg021_22FD_EFB((short)varE, (short)di);
                    //Debug.Print($"tangent vare{varE.ToString("X")}, sr di {di.ToString("X")} = {newHeading1E_argC.ToString("X")}");
                }
            }
            else
            {
                //seg008_5D5
                newUnk14_argE = arg4;
                //->Seg008_7A6
                newHeading1E_argC = HeadingArg2;
            }
        }

        /// <summary>
        /// Runs all player motions until globals come to zero, Used by a sleep update that occurs due to drunkeness
        /// </summary>
        public static void RunPlayerMotions()
        {
            motion.MotionInputPressed = 0;
            while (true)
            {
                if (motion.playerMotionParams.momentum_14 == 0)
                {
                    if (motion.playerMotionParams.unk_a_pitch == 0)
                    {
                        if (motion.playerMotionParams.gravity_10_Z == 0)
                        {
                            if (motion.playerMotionParams.unk_e_Y == 0)
                            {
                                if (motion.playerMotionParams.unk_c_X == 0)
                                {
                                    if (motion.PlayerMotionUpdateRequired_dseg_D3 == false)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                motion.PlayerMotion(0x40);
            }
        }
    }//end class
}//end namespace