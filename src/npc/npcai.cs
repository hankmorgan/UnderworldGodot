using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    /// <summary>
    /// Class for managing the ai processing loop for npcs
    /// </summary>
    public partial class npc : objectInstance
    {

        public static short ANIMATION_IDLE
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return 0;
                }
                else
                {
                    return 32;
                }
            }
        }


        public static short ANIMATION_WALK
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return 1;
                }
                else
                {
                    return 44;
                }
            }
        }


        public static short ANIMATION_DEATH
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return 7;
                }
                else
                {
                    return 0xC;
                }
            }
        }

        public static short ANIMATION_COMBAT_IDLE
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return 2;
                }
                else
                {
                    return 0;
                }
            }
        }


        public static short ANIMATION_COMBAT_BASH
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return 3;
                }
                else
                {
                    return 1;
                }
            }
        }

        public static short ANIMATION_COMBAT_SLASH
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return 4;
                }
                else
                {
                    return 2;
                }
            }
        }

        public static short ANIMATION_COMBAT_STAB
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return 5;
                }
                else
                {
                    return 3;
                }
            }
        }

        public static short ANIMATION_MAGICATTACK
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return 6;
                }
                else
                {
                    return 0xD;
                }
            }
        }

        public static short ANIMATION_RANGEDATTACK
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return 6;//appears to use same animation as magic attack in UW2. Only 2 npc types in the game actually do magic attacks!
                }
                else
                {
                    return 5;
                }
            }
        }

        /// <summary>
        /// The animation frame on which the NPC makes a melee attack
        /// </summary>
        static int COMBAT_HITFRAME
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return 3;
                }
                else
                {
                    return 4;
                }
            }
        }

        public static byte[] SpecialMotionHandler;

        public static int MaxAnimFrame;
        public static bool RelatedToMotionCollision_dseg_67d6_224E;//needs to be set in 1413:ABF
        public static bool IsNPCActive_dseg_67d6_2234;
        static bool dseg_67d6_225E;
        static bool HasCurrobjHeadingChanged_dseg_67d6_2242;
        public static bool dseg_67d6_2269;
        public static bool RelatedToColliding_dseg_67d6_226F;
        static bool FlyingPitchingRelated_dseg_67d6_2246;

        static int BitFieldForPathing_dseg_67d6_B4 = 0xFFFF;

        static bool dseg_67d6_226E;

        static int currObj_XHome;
        static int currObj_YHome;
        static int currObj_FloorHeight;
        static int currObjXCoordinate;
        static int currObjYCoordinate;
        static int currObjQualityX;
        static int currObjOwnerY;
        static int currObjProjectileHeading;
        static int currObjTotalHeading;
        static int currObjUnkBit_0X13_Bit0to6;
        public static int currObjHeight;

        static uwObject currentGoalTarget;
        static int currentGTargXHome;
        static int currentGTargYHome;

        static int currentGTargXCoord;
        static int currentGTargYCoord;

        static int currentGTargXVector;
        static int currentGTargYVector;

        static int currentGTargSquaredDistanceByTiles;
        static int currentGTargSquaredDistanceByCoordinates;

        static int currentGTargFloorHeight;

        public static uwObject collisionObject; //tmp

        /// <summary>
        /// Initial stage of processing AI. Handles the execution of attacks, NPCs reacting to combat and then jumps into handling of goals.
        /// </summary>
        /// <param name="critter"></param>
        public static void NPCInitialProcess(uwObject critter)
        {
            currObj_XHome = critter.npc_xhome;
            currObj_YHome = critter.npc_yhome;
            var distancesquaredtoplayer = Math.Pow(critter.npc_yhome - playerdat.tileY, 2)
                                            + Math.Pow(critter.npc_xhome - playerdat.tileX, 2);

            //var n = (npc)critter.instance;
            if (
                (distancesquaredtoplayer <= 0x64)
                ||
                (critter.npc_goal == (byte)npc_goals.npc_goal_follow)
                )
            {
                //seg007_17A2_21C9:
                //var CalcedFacing = GetCritterAnimationGlobalsForCurrObj(critter);

                if (critterObjectDat.isFlier(critter.item_id))
                {
                    //special setup for flying ai    
                    SpecialMotionHandler = UWMotionParamArray.DSEG_26C6_FlyingNPCMotionHandler;
                }
                else
                {
                    if (critterObjectDat.isSwimmer(critter.item_id))
                    {
                        //special setup for swimming ai
                        SpecialMotionHandler = UWMotionParamArray.DSEG_26DE_SwimmingNPCMotionHandler;
                    }
                    else
                    {
                        //default ai (26ba in uw2,  285C in uw1 ) //seg007_17A2_2207
                        SpecialMotionHandler = UWMotionParamArray.DSEG_26BA_LandNPCMotionHandler;
                    }
                }

                if ((commonObjDat.scaleresistances(critter.item_id) & 8) != 0)
                {
                    //Set values in motion arrays (defined in previous section. )
                    //seg007_17A2_222B
                }

                //set some globals
                RelatedToMotionCollision_dseg_67d6_224E = false;
                IsNPCActive_dseg_67d6_2234 = true;
                dseg_67d6_225E = false;
                HasCurrobjHeadingChanged_dseg_67d6_2242 = false;
                dseg_67d6_2269 = false;
                RelatedToColliding_dseg_67d6_226F = false;
                FlyingPitchingRelated_dseg_67d6_2246 = false;
                //seg007_17A2_2268: 
                if (critter.npc_animation != ANIMATION_WALK)
                {//seg007_17A2:227A
                    if (critter.npc_animation != ANIMATION_IDLE)
                    {
                        if (critter.UnkBit_0X15_Bit7 == 1)
                        {
                            BitFieldForPathing_dseg_67d6_B4 |= (1 << critter.PathFindIndex_0x16_0_F);
                            critter.UnkBit_0X15_Bit7 = 0;
                        }
                    }
                }
                //seg007_17A2_22AB:
                if (
                    (critter.UnkBit_0X15_6 == 0)
                    ||
                    (critter.UnkBit_0X13_Bit0to6 != 0)
                    ||
                    (critter.Projectile_Pitch != 0x10)
                )
                {
                    //NeedsToMove_seg007_17A2_22DA
                    var ProjectileHeading = critter.ProjectileHeading;
                    UWMotionParamArray MotionParams = new();
                    MotionParams.unk_17 = 0x80;
                    motion.InitMotionParams(critter, MotionParams);
                    var NPCMotionCalcArray = new byte[0x20];
                    UWMotionParamArray.LikelyNPCTileStates_222C = motion.InitMotionCalcForNPC(critter, NPCMotionCalcArray);
                    motion.CalculateMotion_TopLevel(
                       projectile: critter,
                       MotionParams: MotionParams,
                       SpecialMotionHandler: SpecialMotionHandler);
                    motion.ApplyProjectileMotion(critter, MotionParams);
                    objectInstance.Reposition(critter);
                    if (ProjectileHeading != critter.ProjectileHeading)
                    {
                        HasCurrobjHeadingChanged_dseg_67d6_2242 = true;
                    }
                }
                //seg007_17A2_2360:
                if ((commonObjDat.scaleresistances(critter.item_id) & 8) != 0)
                {
                    //update values in motion arrays.
                    //TODO confirm the correct bits are being changed.
                    SpecialMotionHandler[2] |= 0x20;
                    SpecialMotionHandler[6] |= 0x20;
                    SpecialMotionHandler[0] &= 0xDF;
                }
                //seg007_17A2_2392:
                //To update globals after motion has taken place
                currObj_XHome = critter.npc_xhome;
                currObj_YHome = critter.npc_yhome;
                currObj_FloorHeight = critter.zpos >> 3;
                currObjXCoordinate = (currObj_XHome << 3) + critter.xpos;
                currObjYCoordinate = (currObj_YHome << 3) + critter.ypos;
                currObjQualityX = critter.quality;
                currObjOwnerY = critter.owner;
                currObjProjectileHeading = critter.ProjectileHeading;
                currObjTotalHeading = (critter.heading << 5) + critter.npc_heading;
                currObjUnkBit_0X13_Bit0to6 = critter.UnkBit_0X13_Bit0to6;
                currObjHeight = commonObjDat.height(critter.item_id);

                GetCritterAnimationGlobalsForCurrObj(critter);

                //finally process goals.
                if ((critter.npc_goal == 0xB) || (critter.npc_goal == 3))
                {
                    NpcBehaviours(critter);
                }
                else
                {
                    //seg007_17A2_246A
                    if (critter.npc_animation == ANIMATION_DEATH)
                    {
                        //death animation
                        if (critter.AnimationFrame >= MaxAnimFrame)
                        {
                            //Special death cases
                            SpecialDeathCases(critter, 1); //mode 1
                            DropRemainsAndLoot(critter);
                            //remove from tile and free object
                            ObjectRemover.DeleteObjectFromTile(critter.tileX, critter.tileY, critter.index, true);
                            return;
                        }
                        else
                        {
                            critter.AnimationFrame++;
                        }
                    }
                    else
                    {
                        //seg007_17A2_2575:    check and process combat anims and other goals                     
                        //Check for melee animation
                        if (
                            (critter.npc_animation == ANIMATION_COMBAT_BASH)
                            ||
                            (critter.npc_animation == ANIMATION_COMBAT_SLASH)
                            ||
                            (critter.npc_animation == ANIMATION_COMBAT_STAB)
                        )
                        {
                            if (critter.AnimationFrame == 0)
                            {
                                if (critter.npc_gtarg == 1)
                                {
                                    //TODO set music if not playing any combat theme
                                    //todo set combat music timer
                                }
                            }
                            if (critter.AnimationFrame == COMBAT_HITFRAME)
                            {
                                //apply attack
                                Debug.Print("NPC makes attack");
                            }
                            if (critter.AnimationFrame >= MaxAnimFrame)
                            {
                                critter.npc_animation = ANIMATION_COMBAT_IDLE;
                                critter.AnimationFrame = 0;
                            }
                            else
                            {
                                critter.AnimationFrame++;
                            }
                        }
                        else
                        {
                            //Check for ranged attacks
                            if ((critter.npc_animation == ANIMATION_RANGEDATTACK) || (critter.npc_animation == ANIMATION_MAGICATTACK))
                            {
                                if (critter.AnimationFrame == COMBAT_HITFRAME)
                                {
                                    if (critter.npc_spellindex == 0)
                                    {
                                        //launch missile using rangedwweapon data in critter.dat 
                                        var si_ammoitemid = critterObjectDat.WeaponLootTableLookup(critter.item_id);
                                        motion.MissilePitch = GetPitchToGTarg(critter, rangedObjectDat.ammotype(si_ammoitemid), 1);
                                        motion.NPCMissileLaunch(critter, si_ammoitemid, rangedObjectDat.ammotype(si_ammoitemid));
                                    }
                                    else
                                    {
                                        //magic spell
                                        //get pitch to gtarg
                                        motion.MissilePitch = GetPitchToGTarg(critter, 0, 0);
                                        var effectid = critterObjectDat.spell(critter.item_id, critter.npc_spellindex - 1); //minus 1 to offset lookup array.
                                        SpellCasting.CastSpellFromObject(
                                            spellno: effectid,
                                            caster: critter);
                                    }
                                }

                                if (critter.AnimationFrame >= MaxAnimFrame)
                                {
                                    critter.npc_animation = ANIMATION_COMBAT_IDLE;
                                    critter.AnimationFrame = 0;
                                }
                                else
                                {
                                    critter.AnimationFrame++;
                                }
                            }
                            else
                            {
                                //Not making a melee/ranged attack. Process goals and behaviours instead.
                                //17A2:27F0
                                NpcBehaviours(critter);
                            }
                        }
                    }
                    //update next frame, probably a refresh rate
                    critter.NextFrame_0XA_Bit0123 = (short)((critter.NextFrame_0XA_Bit0123 + critter.Projectile_Speed) % 16);
                }
            }
            else
            {//distance >0x64 and goal is not follow. do a calculation                
                critter.NextFrame_0XA_Bit0123 = (short)((critter.NextFrame_0XA_Bit0123 + 8) % 16);
            }
        }

        /// <summary>
        /// Handles high level goal processing, critter footsteps and determines if an npc needs to become hostile to the player and so on
        /// </summary>
        static void NpcBehaviours(uwObject critter)
        {
            dseg_67d6_226E = false;//global related to goto
            critter.UnkBit_0x18_5 = 0;
            critter.UnkBit_0X15_6 = 0;
            bool gtargFound = false;

            if ((critter.npc_goal != 0xB) && (critter.npc_goal != 0xF))
            {//seg007_17A2_2886:
                if ((critter.npc_animation == ANIMATION_WALK) && ((critter.AnimationFrame & 0x1) == 1))
                {
                    int soundeffect = -1;
                    switch (critterObjectDat.category(critter.item_id))
                    {
                        case 1://humanoids
                            {
                                if (critter.AnimationFrame == 1)
                                {
                                    soundeffect = 0x5A;//uw2 value?
                                }
                                else if (critter.AnimationFrame == 3)
                                {
                                    soundeffect = 0x5B;//uw2 value?
                                }
                                break;
                            }
                        case 2://fliers
                            {
                                soundeffect = 0x17;//uw2 value?
                                break;
                            }
                        case 3://swimmers
                            {
                                soundeffect = 0x27;//uw2 value?
                                break;
                            }
                        case 4://creepycrawlies
                            {
                                soundeffect = 0xE;//uw2 value?
                                break;
                            }
                        case 5://
                            {
                                soundeffect = 0xD;//uw2 value?
                                break;
                            }
                        case 6://
                            {
                                if (critter.AnimationFrame == 1)
                                {
                                    soundeffect = 0x2F;//uw2 value?
                                }
                                else if (critter.AnimationFrame == 3)
                                {
                                    soundeffect = 0x30;//uw2 value?
                                }
                                break;
                            }
                        case 7://
                            {
                                if ((critter.AnimationFrame == 1) || (critter.AnimationFrame == 3))
                                {
                                    soundeffect = 0x1D;//uw2 value?
                                }
                                break;
                            }
                    }
                    if (soundeffect != -1)
                    {
                        //play sound effect at critter x,y coordinate
                    }
                }//end sound block
                //seg007_17A2_29A5:  passive test?
                if (critterObjectDat.unkPassivenessProperty(critter.item_id) == false)
                {
                    //seg007_17A2_29B8:
                    if (
                        (
                            (critter.IsAlly == 0)
                            &&
                            (critter.index == playerdat.LastDamagedNPCIndex)
                            &&
                            (critterObjectDat.generaltype(critter.item_id) == playerdat.LastDamagedNPCType)
                            &&
                            (critter.UnkBit_0XA_Bit7 == 0)
                        )
                        ||
                        (
                            critter.IsAlly == 1
                        )
                    )
                    {
                        if (playerdat.LastDamagedNPCTime + 512 >= playerdat.ClockValue)
                        {
                            //seg007_17A2_2A0D:
                            //do detection
                            var dist = System.Math.Abs(critter.tileX - playerdat.LastDamagedNPCTileX) + System.Math.Abs(critter.tileY - playerdat.LastDamagedNPCTileY);
                            if (dist < critterObjectDat.combatdetectionrange(critter.item_id))
                            {//npc has detected hostility to themselves or to an ally
                                critter.npc_attitude = 0;
                                critter.UnkBit_0x19_0_likelyincombat = 1;
                                if ((critter.npc_goal != 9) && (critter.npc_goal != 6))
                                {
                                    var gtarg = 1;
                                    if (critter.IsAlly == 1)
                                    {//critter is allied with the player? set them to attack the players target
                                        gtarg = playerdat.LastDamagedNPCIndex;
                                    }
                                    SetGoalAndGtarg(critter, 5, gtarg);
                                    SetNPCTargetDestination(critter, playerdat.LastDamagedNPCTileX, playerdat.LastDamagedNPCTileY, playerdat.LastDamagedNPCZpos);
                                }
                            }
                        }
                    }
                    //seg007_17A2_2AEA:
                    if (critter.ProjectileSourceID > 0)
                    {
                        //HasLastHitNPC_seg007_17A2_2AF8:                        
                        var run2b48 = false;
                        if (critter.ProjectileSourceID == 1 && critter.IsAlly == 0)
                        {
                            run2b48 = true;
                        }
                        else
                        {
                            if (critter.IsAlly != 0)
                            {
                                run2b48 = true;
                            }
                            else
                            {
                                var LastHitChar = UWTileMap.current_tilemap.LevelObjects[critter.ProjectileSourceID];
                                if (LastHitChar.IsAlly != 0)
                                {
                                    run2b48 = true;
                                }
                            }
                        }

                        if (run2b48)
                        {
                            //seg007_17A2_2B48
                            if (critter.npc_gtarg != critter.ProjectileSourceID)
                            {
                                critter.npc_gtarg = (byte)critter.ProjectileSourceID;
                            }

                            if (GetDistancesToGTarg(critter))
                            {
                                int newGoal;
                                //int newGtarg  = critter.ProjectileSourceID;;
                                gtargFound = true;
                                if (critter.ProjectileSourceID == 1)
                                {//player has attacked
                                    critter.npc_attitude = 0;
                                    SetNPCTargetDestination(critter, playerdat.tileX, playerdat.tileY, playerdat.zpos);
                                    critter.UnkBit_0x19_0_likelyincombat = 1;
                                }

                                //now eval distances to decide attack types
                                if
                                    (
                                    currentGTargSquaredDistanceByTiles <= 2
                                    ||
                                        (
                                        currentGTargSquaredDistanceByTiles > 2
                                        && critterObjectDat.isCaster(critter.item_id)
                                        && (UWTileMap.current_tilemap.Tiles[critter.tileX, critter.tileY].noMagic == 0)
                                        )
                                    )
                                {//npc is close, or is a caster who is at range and can cast
                                    if (critter.UnkBit_0x19_5 == 0)
                                    {
                                        if (critter.UnkBit_0x19_4 == 0)
                                        {
                                            if (ShouldNPCWithdraw(critter))
                                            {
                                                newGoal = 6;
                                            }
                                            else
                                            {
                                                newGoal = 5;
                                            }
                                        }
                                        else
                                        {
                                            newGoal = 9;
                                        }
                                    }
                                    else
                                    {
                                        newGoal = 5;
                                    }
                                }
                                else
                                {//non caster at range/ caster who is unable to cast spell
                                    critter.UnkBit_0x19_5 = 1;
                                    newGoal = 5;
                                }

                                SetGoalAndGtarg(critter, newGoal, critter.ProjectileSourceID);
                                critter.ProjectileSourceID = 0;
                                critter.AccumulatedDamage = 0;
                            }
                        }
                    }
                }
            }
            //ProcessGoals_seg007_17A2_2CE7:  processgoals here
            switch (critter.npc_goal)
            {
                case 0://standstill
                case 7:
                    StandStillGoal(critter);
                    break;
                case 1: //goto
                    NPC_Goto(
                        critter: critter,
                        targetX: currObjQualityX,
                        targetY: currObjOwnerY,
                        targetZ: UWTileMap.current_tilemap.Tiles[currObjQualityX, currObjOwnerY].floorHeight);
                    break;
                case 2:
                    NPCWanderUpdate(critter);
                    break;
                case 3://move/follow (skip to NPC_Move below)
                    break;
                case 4:
                    StandStillGoal(critter);
                    break;
                case 5:
                    if (!gtargFound)
                    {
                        if (!GetDistancesToGTarg(critter))
                        {
                            ResetGoalAndTarget(critter);
                            break;
                        }
                    }
                    NPC_Goal5_Attack(critter);
                    break;
                case 6:
                    if (!gtargFound)
                    {
                        if (!GetDistancesToGTarg(critter))
                        {
                            ResetGoalAndTarget(critter);
                            break;
                        }
                    }
                    NPC_Goal6(critter);
                    break;
                case 8:
                    NPC_Goal8(critter);
                    break;
                case 9:
                    if (!gtargFound)
                    {
                        if (!GetDistancesToGTarg(critter))
                        {
                            ResetGoalAndTarget(critter);
                            break;
                        }
                    }
                    NPC_Goal9(critter);
                    break;
                case 10: //talkto
                    NPCGoal_TalkTo(critter);
                    break;
                case 12://stand at location
                    NPCGoalC_StandAtLocation(critter);
                    break;
                default:
                    Debug.Print($"unhandled goal {critter.npc_goal} for {critter.a_name}");
                    break;
            }

            NPC_Move(critter);
        }


        static void ResetGoalAndTarget(uwObject critter)
        {
            //; if npc_level is set then
            //;        change npc_goal to npc_level aimed at avatar,
            //;        clear npc_level
            //; else
            //;    set goal to 2 (Wander)
            if (critter.npc_level == 0)
            {
                critter.npc_goal = 2;
                critter.npc_gtarg = 0;
            }
            else
            {
                critter.npc_goal = critter.npc_level;
                critter.npc_gtarg = 1;
                critter.npc_level = 0;
            }

        }

        /// <summary>
        /// Updates critter headings. seg007_17A2_1B9E
        /// </summary>
        /// <param name="critter"></param>
        static void NPC_Move(uwObject critter)
        {
            var HeadingVar1 = critter.ProjectileHeading;
            var FullHeadingVar2 = (critter.heading << 5) + critter.npc_heading;

            var cl_heading = (0x100 + (FullHeadingVar2 & 0xFF) - currObjTotalHeading) % 0x100;

            //seg007_17A2_1BDF
            if ((cl_heading >= 0x20) && (cl_heading <= 0xE0))
            {
                if (cl_heading >= 0x80)
                {
                    FullHeadingVar2 = (currObjTotalHeading + 0xE0) % 0x100;
                }
                else
                {
                    FullHeadingVar2 = (currObjTotalHeading + 0x20) % 0x100;
                }
            }

            //seg007_17A2_1C06:
            critter.heading = (short)((FullHeadingVar2 >> 5) & 0x7);
            critter.npc_heading = (short)(FullHeadingVar2 & 0x1F);
            if (HasCurrobjHeadingChanged_dseg_67d6_2242)
            {
                critter.ProjectileHeading = (ushort)currObjProjectileHeading;
            }
            else
            {
                if (currObjUnkBit_0X13_Bit0to6 > 1)
                {
                    if (critter.UnkBit_0X13_Bit0to6 > 1)
                    {
                        cl_heading = (0x100 + (HeadingVar1 & 0xFF) - currObjProjectileHeading) % 0x100;

                        if (cl_heading < 0x20)
                        {
                            critter.ProjectileHeading = HeadingVar1;
                        }
                        else
                        {
                            if (cl_heading <= 0xE0)
                            {
                                if (cl_heading >= 0x40)
                                {
                                    if (cl_heading <= 0xC0)
                                    {
                                        critter.UnkBit_0X13_Bit0to6 = 0;
                                        critter.ProjectileHeading = (ushort)currObjProjectileHeading;
                                    }
                                    else
                                    {
                                        critter.ProjectileHeading = (ushort)((currObjProjectileHeading + 0xE0) % 0x100);
                                    }
                                }
                                else
                                {
                                    critter.ProjectileHeading = (ushort)((currObjProjectileHeading + 0x20) % 0x100);
                                }
                            }
                            else
                            {
                                critter.ProjectileHeading = HeadingVar1;
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Changes the goal and gtarg for the npc
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="goal"></param>
        /// <param name="target"></param>
        public static void SetGoalAndGtarg(uwObject critter, int goal, int target)
        {
            if (critter.npc_goal == 4)
            {//back up goal for some unknown reason
                critter.npc_level = critter.npc_goal;
            }
            critter.npc_goal = (byte)goal;
            critter.npc_gtarg = (byte)target;
        }

        public static void SetNPCTargetDestination(uwObject critter, int newTargetX, int newTargetY, int newHeight)
        {
            if (
                (critter.TargetTileX != newTargetX)
                ||
                (critter.TargetTileY != newTargetY)
                ||
                (critter.TargetZHeight != newHeight)
            )
            {
                critter.TargetTileX = (short)newTargetX;
                critter.TargetTileY = (short)newTargetY;
                critter.TargetZHeight = (short)newHeight;
                critter.UnkBit_0x18_5 = 1;
                critter.UnkBit_0x18_6 = 0;
            }
        }


        /// <summary>
        /// Calcs distances from critter to it's target
        /// </summary>
        /// <param name="critter"></param>
        /// <returns>true if target hp>0</returns>
        public static bool GetDistancesToGTarg(uwObject critter)
        {
            currentGoalTarget = UWTileMap.current_tilemap.LevelObjects[critter.npc_gtarg];
            if (currentGoalTarget.npc_hp <= 0)
            {
                return false;
            }
            else
            {
                currentGTargXHome = currentGoalTarget.npc_xhome;
                currentGTargYHome = currentGoalTarget.npc_yhome;

                currentGTargXCoord = currentGoalTarget.xpos + (currentGoalTarget.npc_xhome << 3);
                currentGTargYCoord = currentGoalTarget.ypos + (currentGoalTarget.npc_yhome << 3);
                currentGTargXVector = currentGTargXCoord - currObjXCoordinate;
                currentGTargYVector = currentGTargYCoord - currObjYCoordinate;
                currentGTargFloorHeight = currentGoalTarget.zpos >> 3;
                currentGTargSquaredDistanceByTiles = (int)(Math.Pow(currentGTargXHome - currObj_XHome, 2) + Math.Pow(currentGTargYHome - currObj_YHome, 2));
                currentGTargSquaredDistanceByCoordinates = (int)(Math.Pow(currentGTargXCoord - currObjXCoordinate, 2) + Math.Pow(currentGTargYCoord - currObjYCoordinate, 2));
                return true;
            }
        }

        static bool ShouldNPCWithdraw(uwObject critter)
        {//, int maxhp, int currhp, int unk1C, int accumulateddmg)
            var maxhp = critterObjectDat.avghit(critter.item_id);
            var critter1C = critterObjectDat.maybemorale(critter.item_id);

            if ((maxhp * 3) >> 2 >= critter.npc_hp) //.75 of max hp
            {
                if (maxhp >> 3 <= critter.npc_hp)//maxhp/8
                {
                    if ((maxhp >> 1) < critter.AccumulatedDamage)//max/2
                    {
                        return true;
                    }
                    else
                    {
                        if (maxhp != 0)
                        {
                            var hpcalc = Rng.r.Next(0, 4) + ((critter.npc_hp << 4) / maxhp);
                            var moralcalc = 15 - critter1C;
                            if (moralcalc >= hpcalc)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }



        /// <summary>
        /// Processes standing still (UW2 version)
        /// </summary>
        /// <param name="critter"></param>
        public static void StandStillGoal(uwObject critter)
        {
            int gtarg_x = -1; int gtarg_y = -1;
            if (IsNPCActive_dseg_67d6_2234)
            {
                if (critter.npc_attitude == 0)
                {
                    critter.npc_gtarg = 1;
                    GetDistancesToGTarg(critter);
                    if (critter.UnkBit_0x19_0_likelyincombat == 0)
                    {
                        if (critter.UnkBit_0x19_1 != 0)
                        {
                            if (Rng.r.Next(0, 16) <= critterObjectDat.unk_1F_uppernibble(critter.item_id))
                            {
                                TurnTowardsTarget(critter, 0);
                            }
                            else
                            {
                                critter.UnkBit_0x19_1 = 0;
                            }
                        }

                        if (Rng.r.Next(0, 16) < critterObjectDat.unk_1F_uppernibble(critter.item_id))
                        {
                            var result = SearchForGoalTarget(critter, ref gtarg_x, ref gtarg_y);
                            if (result == 0)
                            {
                                critter.UnkBit_0x19_0_likelyincombat = 1;
                                SetNPCTargetDestination(critter, gtarg_x, gtarg_y, currentGTargFloorHeight);
                                SetGoalAndGtarg(critter, 5, 1);
                                return;
                            }
                            else
                            {
                                if (result == 2)
                                {
                                    critter.UnkBit_0x19_1 = 1;
                                    if (Rng.r.Next(0, 2) == 1)
                                    {
                                        NPC_Goto(critter, gtarg_x, gtarg_y, currentGTargFloorHeight);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        SetGoalAndGtarg(critter, 5, 1);
                        return;
                    }
                }

                //check goal again.
                switch (critter.npc_goal)
                {
                    case 2:
                        NPCWanderUpdate(critter);
                        return;
                    case 0:
                    case 7:
                        critter.Projectile_Speed = 6;
                        critter.UnkBit_0X13_Bit0to6 = 0;
                        UpdateAnimation(critter, ANIMATION_IDLE, false);
                        return;
                    default:
                        NPC_Goal8(critter);
                        return;
                }
            }
        }


        static void AttackGoalSearchForTarget(uwObject critter, ref int xhome, ref int yhome, int arg4)
        {
            if ((xhome != critter.TargetTileX) && (yhome != critter.TargetTileY))
            {
                if (Rng.r.Next(8) == 0)
                {
                    var result = SearchForGoalTarget(critter, ref xhome, ref yhome);
                    switch (result)
                    {
                        case 0:
                            {//npc is aware of target
                                SetNPCTargetDestination(critter, xhome, yhome, currentGTargFloorHeight);
                                break;
                            }
                        case 1:
                            {//npc loses target?
                                critter.UnkBit_0x19_0_likelyincombat = 0;
                                critter.UnkBit_0x19_1 = 1;
                                ResetGoalAndTarget(critter);
                                break;
                            }
                        case 2:
                            {//no change to detection.
                                if (Rng.r.Next(2) == 1)
                                {
                                    SetNPCTargetDestination(critter, xhome, yhome, currentGTargFloorHeight);
                                }
                                else
                                {
                                    critter.UnkBit_0x19_0_likelyincombat = 0;
                                    critter.UnkBit_0x19_1 = 0;
                                    ResetGoalAndTarget(critter);
                                }
                                break;
                            }
                        default:
                            Debug.Print("unknown detection state. Exiting AttackGoalSearchForTarget()");
                            return;//not a valid case
                    }

                }
            }
            //seg007_17A2_D3D:
            if ((xhome == currObj_YHome) && (yhome == currObj_YHome) && (Math.Abs(currObj_FloorHeight - currentGTargFloorHeight) < 4))
            {
                //close to target
                return;
            }
            else
            {
                //seg007_17A2_D68:
                //This chain of logic is fairly complex. Bet I made a mistake here.
                if ((arg4 <= 1) || ((arg4 > 1) && (arg4 * arg4 >= currentGTargSquaredDistanceByTiles)))
                {//seg007_17A2_D80:
                    if ((((arg4 * arg4) << 3) << 3) >= currentGTargSquaredDistanceByCoordinates)
                    {
                        if (arg4 > 1)
                        {
                            return;
                        }
                        else
                        {
                            if (Math.Abs(currentGTargFloorHeight - currObj_FloorHeight) < 4)
                            {
                                return;
                            }
                        }
                    }
                }
                //seg007_17A2_DBD:
                NPC_Goto(critter, xhome, yhome, currentGTargFloorHeight);
                if (critter.UnkBit_0x18_6 != 0)
                {
                    critter.UnkBit_0x19_1 = 0;
                }
            }

        }

        /// <summary>
        ///; ax = 0, npc finds and is aware of the player who is close, optionally set npc_hunger bit 0 based on x and y vector
        ///; ax = 1, npc loses and becames unaware of the player who has gone far away clear npc_hunger bit 0
        ///; ax = 2, npc makes no change to their detection state because the player is in the middle distance no change to hunger bit 0
        /// </summary>
        /// <param name="xHomeFound"></param>
        /// <param name="yHomeFound"></param>
        /// <returns></returns>
        static int SearchForGoalTarget(uwObject critter, ref int xHomeFound, ref int yHomeFound)
        {
            var gtargObject = UWTileMap.current_tilemap.LevelObjects[critter.npc_gtarg];
            xHomeFound = currentGTargXHome; yHomeFound = currentGTargYHome;

            var xvector = currentGTargXHome - currObj_XHome;
            var yvector = currentGTargYHome - currObj_YHome;

            var si_dist = (int)Math.Pow(xvector, 2) + Math.Pow(yvector, 2);
            var score = (int)Math.Pow((critterObjectDat.combatdetectionrange(critter.item_id) * critterObjectDat.maybestealth(gtargObject.item_id)) / 16, 2);

            if (score / 4 < si_dist)
            {
                var var8 = (int)Math.Pow((critterObjectDat.theftdetectionrange(critter.item_id) * critterObjectDat.StealthScore2(gtargObject.item_id)) / 16, 2);

                if (si_dist < var8)
                {
                    var HeadingToTarget_var3 = Pathfind.GetVectorHeading(xvector, yvector);
                    var var5 = (8 + (HeadingToTarget_var3 - critter.heading)) % 8;
                    if ((var5 == 0) || (var5 == 1) || (var5 == 7))
                    {
                        var result = Pathfind.TestBetweenPoints(
                            currObjXCoordinate, currObjYCoordinate, critter.zpos + commonObjDat.height(critter.item_id),
                            currentGTargXCoord, currentGTargYCoord, currentGoalTarget.zpos + commonObjDat.height(currentGoalTarget.item_id)
                            );
                        if (result)
                        {
                            critter.UnkBit_0x19_0_likelyincombat = 1;
                            return 0;
                        }
                    }
                }

                if ((score << 2) < si_dist)
                {
                    critter.UnkBit_0x19_0_likelyincombat = 0;
                    return 1;
                }
                else
                {
                    return 2;
                }

            }
            else
            {
                return 0;
            }
        }



        /// <summary>
        /// Turns the heading of the critter towards it's gtarg if not facing it.
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="arg0"></param>
        /// <returns></returns>
        static bool TurnTowardsTarget(uwObject critter, int arg0)
        {
            var vectorX = currentGTargXCoord - currObjXCoordinate;
            var vectorY = currentGTargYCoord - currObjYCoordinate;
            var heading_var3 = Pathfind.GetVectorHeading(vectorX, vectorY);

            var relativeHeading_var5 = (8 + (heading_var3 - critter.heading)) % 8;
            if (arg0 == 0)
            {
                if (relativeHeading_var5 == 0)
                {
                    int newHeading;
                    if (relativeHeading_var5 > 4)
                    {
                        newHeading = critter.heading + 1;
                    }
                    else
                    {
                        newHeading = critter.heading - 1;
                    }
                    critter.heading = (short)(newHeading & 7);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return TurnTowardsVectorSeg007_17A2_1CE5(critter, (short)vectorX, (short)vectorY);
            }
        }

        /// <summary>
        /// Function only applies when npcs casting spells
        /// </summary>
        /// <param name="vectorX"></param>
        /// <param name="vectorY"></param>
        /// <returns></returns>
        static bool TurnTowardsVectorSeg007_17A2_1CE5(uwObject critter, short vectorX, short vectorY)
        {
            bool result = false;

            var totalHeading_var7 = (short)((critter.heading << 5) + critter.npc_heading);

            var si_distance = (short)Math.Sqrt(Math.Pow(currentGTargXVector, 2) + Math.Pow(currentGTargYVector, 2));

            var xVar10 = (int)vectorX;
            var xVarE_Sign = (short)(xVar10 >> 0xF);

            var yVar14 = (int)vectorY;
            var yVar12_Sign = (short)(yVar14 >> 0xF);

            if (si_distance == 0)
            {
                return true;
            }
            else
            {
                //seg007_17A2_1D58:
                short varC = 0; short di = 0;
                if ((yVar12_Sign == 0) && (yVar14 == si_distance))
                {
                    varC = 0x7FFF;
                }
                else
                {
                    //seg007_17A2_1D6B:
                    //THIS SECTION IS possibly WRONG! 
                    if ((yVar14 < 1) && (-yVar14 == si_distance))
                    {
                        varC = -32768;
                    }
                    else
                    {
                        varC = (short)((vectorY << 0xF) / si_distance);
                    }
                }

                //seg007_17A2_1DA1:
                if ((xVarE_Sign == 0) && (xVar10 == si_distance))
                {
                    di = 0x7FFF;
                }
                else
                {
                    if ((xVar10 < 1) && (-xVar10 == si_distance))
                    {
                        di = -32768;
                    }
                    else
                    {
                        di = (short)((vectorX << 0xF) / si_distance);
                    }
                }

                //seg007_17A2_1DE5
                var tangent_var6 = motion.MaybeGetTangent_seg021_22FD_EFB(varC, di);

                var var8 = (0x40 - (tangent_var6 >> 8)) & 0xFF;
                var var9 = var8 - totalHeading_var7;
                var HeadingVarA = 0;
                //seg007_17A2_1DFE:
                if ((var9 >= 0x20) && (var9 <= 0xE0))
                {
                    if (var9 >= 0x80)
                    {
                        HeadingVarA = (totalHeading_var7 + 0xE0) % 0x100;
                    }
                    else
                    {
                        HeadingVarA = (totalHeading_var7 + 0x20) % 0x100;
                    }
                }
                else
                {
                    //seg007_17A2_1E16:
                    HeadingVarA = var8;
                    result = true;
                }

                critter.heading = (short)(HeadingVarA >> 5);
                critter.npc_heading = (short)(HeadingVarA & 0x1F);

                return result;
            }
        }


        /// <summary>
        /// NPC will wait for the player to face them from a close range and initiates conversation.
        /// </summary>
        /// <param name="critter"></param>
        static void NPCGoal_TalkTo(uwObject critter)
        {
            if (IsNPCActive_dseg_67d6_2234)
            {
                if ((critter.npc_attitude == 0) && (critter.npc_goal != 4))
                {
                    SetGoalAndGtarg(critter, 4, 1);//critter becomes hostile
                }
                else
                {
                    critter.npc_gtarg = 1;
                    GetDistancesToGTarg(critter);
                    var si_dist = (currentGTargXVector * currentGTargXVector) + (currentGTargYVector * currentGTargYVector);
                    int targetX = 0;
                    int targetY = 0;
                    var gtargFound = SearchForGoalTarget(critter, ref targetX, ref targetY);
                    critter.Projectile_Speed = 6;
                    critter.UnkBit_0X13_Bit0to6 = 0;
                    UpdateAnimation(critter, ANIMATION_IDLE, false);
                    if (gtargFound != 1)
                    {
                        if (si_dist < 0x190)
                        {
                            //player is relatively close, turn to face the player
                            var heading = Pathfind.GetVectorHeading(currentGTargXVector, currentGTargYVector);
                            ChangeNpcHeadings(critter, heading);
                            if (si_dist < 0x90)
                            {
                                var RelativeHeading = (playerdat.playerObject.heading + 8 - heading) & 0x7;
                                if ((RelativeHeading >= 3) && (RelativeHeading <= 5))
                                {
                                    //player is facing the npc.
                                    talk.Talk(critter, true);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Goal 12. NPC will attempt to stand at the specified spot. If not there will pathfind to that location.
        /// NPCGoalC_seg007_17A2_17F1
        /// </summary>
        static void NPCGoalC_StandAtLocation(uwObject critter)
        {
            if (IsNPCActive_dseg_67d6_2234)
            {//seg007_17A2_1802
                var xDist = currObjQualityX - currObj_XHome;
                var yDist = currObjOwnerY - currObj_YHome;
                if ((critter.npc_attitude == 0) && (critter.npc_goal != (int)npc_goals.npc_goal_wander_4))
                {//seg007_17A2:183B
                    SetGoalAndGtarg(critter, 4, 1);
                }
                else
                {//seg007_17A2_1842:
                    if ((xDist == 0) && (yDist == 0))
                    {//seg007_17A2_1882:
                        critter.Projectile_Speed = 6;
                        critter.UnkBit_0X13_Bit0to6 = 0;
                        UpdateAnimation(critter, ANIMATION_IDLE, false);
                    }
                    else
                    {
                        //seg007_17A2_184E:  needs to return to that lcoation.
                        var tile = UWTileMap.current_tilemap.Tiles[currObjQualityX, currObjOwnerY];
                        NPC_Goto(critter: critter, targetX: currObjQualityX, targetY: currObjOwnerY, targetZ: tile.floorHeight);
                    }
                }
            }
        }

        /// <summary>
        /// Directs the NPC to travel towards a target location
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="targetX"></param>
        /// <param name="targetY"></param>
        /// <param name="targetZ"></param>
        static void NPC_Goto(uwObject critter, int targetX, int targetY, int targetZ)
        {
            var Bit7Cleared_Var5 = false;
            var MaybeIsFlier_Var6 = false;

            SetNPCTargetDestination(
                critter: critter,
                newTargetX: targetX,
                newTargetY: targetY,
                newHeight: targetZ);

            if (critter.UnkBit_0x18_5 == 1)
            {
                //seg006_1413_303E
                if (critter.UnkBit_0X15_Bit7 == 1)
                {
                    BitFieldForPathing_dseg_67d6_B4 |= (1 << critter.PathFindIndex_0x16_0_F);
                    critter.UnkBit_0X15_Bit7 = 0;
                }
            }
            //seg006_1413_3068:
            var xDiff = targetX - currObj_XHome;
            var yDiff = targetY - currObj_YHome;

            if ((xDiff == 0) && (yDiff == 0))
            {
                if (critter.UnkBit_0X15_Bit7 == 1)
                {
                    BitFieldForPathing_dseg_67d6_B4 |= (1 << critter.PathFindIndex_0x16_0_F);
                    critter.UnkBit_0X15_Bit7 = 0;
                }
                //seg006_1413_30BC:
                if (critter.npc_goal == 1)
                {
                    SetGoalAndGtarg(critter, 8, 0);
                }
                else
                {
                    //seg006_1413_30DD:
                    if (IsNPCActive_dseg_67d6_2234)
                    {
                        //seg006_1413_30F6:
                        critter.UnkBit_0X13_Bit7 = 1;
                        critter.UnkBit_0X15_6 = 1;
                        critter.npc_animation = ANIMATION_IDLE;
                        critter.AnimationFrame = 0;
                        return;
                    }
                }
            }
            if (IsNPCActive_dseg_67d6_2234 == false)//seg006_1413_3122
            {
                //Seg006_1413_312E
                critter.Projectile_Speed = 1;
                if (critter.UnkBit_0X15_Bit7 == 1)
                {
                    //seg006_1413_3155:
                    //table look up into seg057_625F (possibly a path list)
                    var tmpSeg57_x = -1;
                    var tmpSeg57_y = -1;
                    var indexSeg57 = critter.PathFindIndex_0x16_0_F;
                    if (tmpSeg57_x == critter.npc_xhome)
                    {
                        if (tmpSeg57_y == critter.npc_yhome)
                        {
                            //seg006_1413_31B5:
                            //MaybeReadPath(critter, indexSeg57);
                            UpdatePathFlag_seg006_1413_2ABB(PathFind57_Turning.PathFind57Records[indexSeg57]);
                        }
                    }
                }
                return;
            }
            else
            {
                //Seg006_1413_31D2
                if (RelatedToMotionCollision_dseg_67d6_224E)
                {//seg006_1413_31DC
                    if (HasCurrobjHeadingChanged_dseg_67d6_2242 == false)
                    {
                        //seg006_1413_31E8: 
                        if (critter.UnkBit_0x18_6 == 0)
                        {
                            //seg006_1413_31FF
                            if (dseg_67d6_2269)
                            {
                                //seg006_1413_3209
                                if (RelatedToColliding_dseg_67d6_226F == false)
                                {
                                    //seg006_1413_324E:                                    
                                    if
                                     (!(
                                        (collisionObject.majorclass == 1)
                                        &&
                                        (collisionObject.item_id == 0x7F)
                                        &&
                                        (critter.npc_goal == 5)
                                        )
                                     )
                                    {
                                        //seg006_1413_3290:
                                        //when not colliding with the avatar with a goal of 5.
                                        if (
                                            (collisionObject.item_id >> 4 == 0x14)
                                            && (collisionObject.classindex >= 8)
                                            && critterObjectDat.isFlier(critter.item_id)
                                            )
                                        {
                                            //when a flier collides with an open door.
                                            //seg006_1413:32B5
                                            critter.Projectile_Pitch = 14;
                                            RelatedToMotionCollision_dseg_67d6_224E = false;
                                            FlyingPitchingRelated_dseg_67d6_2246 = true;
                                        }
                                        else
                                        {
                                            //seg006_1413_32D5
                                            critter.UnkBit_0x18_6 = 1;
                                        }
                                    }
                                }
                                else
                                {
                                    //seg006_1413:3210
                                    critter.npc_animation = ANIMATION_IDLE;
                                    critter.AnimationFrame = 0;
                                    if (Rng.r.Next(4) == 0)
                                    {
                                        //seg006_1413_32D5: (again)
                                        critter.UnkBit_0x18_6 = 1;
                                    }
                                    else
                                    {
                                        //seg006_1413_323B: 
                                        NPCTryToOpenDoor(critter, collisionObject);
                                    }
                                }
                            }

                            //seg006_1413_32E5:

                            if (RelatedToMotionCollision_dseg_67d6_224E)
                            {
                                if (critter.UnkBit_0X15_Bit7 != 0)
                                {
                                    BitFieldForPathing_dseg_67d6_B4 |= 1 << critter.PathFindIndex_0x16_0_F;
                                    critter.UnkBit_0X15_Bit7 = 0;
                                }
                                //seg006_1413_331A: 
                                critter.UnkBit_0x18_7 = 0;
                                Bit7Cleared_Var5 = true;
                            }
                        }
                    }
                }


                //seg006_1413_332C:
                if (critter.UnkBit_0X15_Bit7 == 0)
                {
                    //seg006_1413_3384
                    if ((critter.UnkBit_0x18_5 == 0) && (critter.UnkBit_0x18_7 != 0))
                    {
                        //seg006_1413:33A8
                        var var3 = Pathfind.GetVectorHeading(xDiff, yDiff);
                        ChangeNpcHeadings(critter, var3);
                        if (critterObjectDat.isFlier(critter.item_id))
                        {
                            //seg006_1413_33D8:
                            FlyingCritterPitching_seg006_1413_36B8(critter, targetX, targetY);
                        }
                    }
                    else
                    {
                        //seg006_1413_33EB:
                        if ((critter.UnkBit_0x18_5 == 0) && (critter.UnkBit_0x18_6 == 1))
                        {
                            //seg006_1413:340F
                            if (Rng.r.Next(8) == 0)
                            {
                                critter.UnkBit_0x18_6 = 0;
                            }
                            //seg006_1413_342C:
                            NPCWanderUpdate(critter);
                            return;
                        }
                        else
                        {
                            bool goto43D2;
                            //seg006_1413_3434
                            if (Bit7Cleared_Var5 == false)
                            {
                                goto43D2 = (Pathfind.PathAlongStraightLine_seg006_1413_205B(critter, currObj_XHome, currObj_YHome, targetX, targetY) != 1);
                            }
                            else
                            {
                                goto43D2 = true;
                            }
                            if (goto43D2)
                            {
                                //seg006_1413_34D2
                                int BitFieldIndex_var4 = 0;
                                if (FindSetIndexOfBitField(ref BitFieldIndex_var4))
                                {
                                    //seg006_1413_34E3: 
                                    if (Pathfind.PathFindBetweenTiles(
                                        critter: critter,
                                        currTileX_arg0: currObj_XHome, currTileY_arg2: currObj_YHome, CurrFloorHeight_arg4: critter.zpos >> 3,
                                        TargetTileX_arg6: targetX, TargetTileY_arg8: targetY, TargetFloorHeight_argA: targetZ,
                                        LikelyRangeArgC: GetCritterRange_seg007_17A2_30D1(critter)))
                                    {
                                        //seg006_1413_351A:
                                        var mask = ~(1 << BitFieldIndex_var4);
                                        BitFieldForPathing_dseg_67d6_B4 &= mask; //unset the bit at var4

                                        Pathfind.UpdateSeg57TurningValues(PathFind57_Turning.PathFind57Records[BitFieldIndex_var4]);

                                        critter.UnkBit_0x18_6 = 0;
                                        critter.UnkBit_0X15_Bit7 = 1;
                                        critter.PathFindIndex_0x16_0_F = (short)BitFieldIndex_var4;
                                        //Lookup Record
                                        TurnTowardsPath_seg006_1413_2BF5(critter, PathFind57_Turning.PathFind57Records[critter.PathFindIndex_0x16_0_F]);
                                    }
                                    else
                                    {
                                        //seg006_1413_359B:
                                        critter.UnkBit_0x18_6 = 1;
                                        critter.UnkBit_0x18_7 = 0;
                                        NPCWanderUpdate(critter);
                                        return;
                                    }
                                }
                                else
                                {
                                    //seg006_1413_359B:
                                    critter.UnkBit_0x18_6 = 1;
                                    critter.UnkBit_0x18_7 = 0;
                                    NPCWanderUpdate(critter);
                                    return;
                                }
                            }
                            else
                            {
                                //seg006_1413_3470:
                                critter.UnkBit_0x18_7 = 1;
                                var var3 = Pathfind.GetVectorHeading(xDiff, yDiff);
                                ChangeNpcHeadings(critter, var3);
                                critter.UnkBit_0x18_6 = 0;
                                if (critter.UnkBit_0X15_Bit7 == 1)
                                {
                                    //seg006_1413_34B5:
                                    BitFieldForPathing_dseg_67d6_B4 |= 1 << critter.PathFindIndex_0x16_0_F;
                                    critter.UnkBit_0X15_Bit7 = 0;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //seg006_1413_334C:
                    if (TurnTowardsPath_seg006_1413_2BF5(critter, PathFind57_Turning.PathFind57Records[critter.PathFindIndex_0x16_0_F]) == false)
                    {
                        BitFieldForPathing_dseg_67d6_B4 |= 1 << critter.PathFindIndex_0x16_0_F;
                        critter.UnkBit_0X15_Bit7 = 0;
                    }
                }

                //seg006_1413_35C1:
                if (dseg_67d6_226E == false)
                {//THis is probably uw2 logic. Todo check if the same happens in uw1
                    //seg006_1413_35CD:
                    critter.UnkBit_0X15_6 = 0;
                    if (critter.npc_animation != ANIMATION_WALK)
                    {
                        critter.npc_animation = ANIMATION_WALK;
                        GetCritterAnimationGlobalsForCurrObj(critter);
                        if (critter.AnimationFrame > MaxAnimFrame)
                        {
                            critter.AnimationFrame = 0;
                        }
                    }
                    else
                    {
                        //already in walk animation;
                        //seg006_1413_3620:
                        var tmp = critter.AnimationFrame + 1;
                        critter.AnimationFrame = (byte)(tmp % MaxAnimFrame);
                    }
                    //seg006_1413_3657:
                    //set frames as in above branches   
                    int var8;
                    //seg006_1413_3657:
                    if (MaybeIsFlier_Var6 == false)
                    {
                        //seg006_1413_3664
                        if (critter.npc_goal == 5)
                        {
                            var8 = critterObjectDat.speed(critter.item_id);
                        }
                        else
                        {
                            var8 = critterObjectDat.unk_b(critter.item_id);
                        }
                    }
                    else
                    {
                        //seg006_1413:365D
                        var8 = 0;
                    }
                    critter.UnkBit_0X13_Bit0to6 = (short)var8;
                    critter.Projectile_Speed = 4;
                }
            }
        }


        /// <summary>
        /// Seems to get a range that the NPC will use in pathfinding
        /// </summary>
        /// <param name="critter"></param>
        /// <returns></returns>
        static int GetCritterRange_seg007_17A2_30D1(uwObject critter)
        {
            if (
                (critter.npc_attitude == 0)
                &&
                (critterObjectDat.avghit(critter.item_id) != 0)
                &&
                (critter.doordir == 0)
                )
            {
                //seg007_17A2_3105: 
                return ((critter.npc_hp << 2) / critterObjectDat.avghit(critter.item_id)) + (critterObjectDat.maybemorale(critter.item_id) / 4);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Finds the first set bit in the global
        /// </summary>
        /// <param name="FieldIndex"></param>
        /// <returns></returns>
        static bool FindSetIndexOfBitField(ref int FieldIndex)
        {
            if (BitFieldForPathing_dseg_67d6_B4 != 0)
            {
                var dl = 0;
                while (dl < 0x10)
                {
                    if ((BitFieldForPathing_dseg_67d6_B4 & (1 << dl)) != 0)
                    {
                        FieldIndex = dl;
                        return true;
                    }
                    dl++;
                }
            }
            return false;
        }




        static void ChangeNpcHeadings(uwObject critter, int heading)
        {
            critter.ProjectileHeading = (ushort)(heading << 5);
            critter.heading = (short)heading;
            critter.npc_heading = 0;
        }

        static bool TurnTowardsPath_seg006_1413_2BF5(uwObject critter, PathFind57_Turning path57Record)
        {
            var tileX_var5 = path57Record.X0;
            var tileY_var6 = path57Record.Y1;

            if (CheckIfAtOrNearTargetTile(
                flagArg0: path57Record.unk2_7,
                xhome_arg2: currObj_XHome, yhome_arg4: currObj_YHome,
                xpos_arg6: currObjXCoordinate & 0x7, ypos_arg8: currObjYCoordinate & 0x7,
                PathXArgA: tileX_var5, PathYArgC: tileY_var6))
            {
                if (!UpdatePathFlag_seg006_1413_2ABB(path57Record))
                {
                    return false;
                }
            }
            else
            {
                //seg006_1413_2C5C:
                tileX_var5 = currObj_XHome;
                tileY_var6 = currObj_YHome;
            }

            //seg006_1413_2C68:
            if (path57Record.unk2_7 == 0)
            {
                //seg006_1413_2C8A:
                if (critterObjectDat.isFlier(critter.item_id))
                {
                    FlyingCritterPitching_seg006_1413_36B8(critter, critter.TargetTileX, critter.TargetTileY);
                }

                //seg006_1413_2CBD:
                var xVar2 = path57Record.X0 << 3;
                if (path57Record.X0 == tileX_var5)
                {
                    xVar2 += 4;
                }
                else
                {
                    if (path57Record.X0 < tileX_var5)
                    {
                        xVar2 += 7;
                    }
                }

                var yVar4 = path57Record.Y1 << 3;

                if (path57Record.Y1 == tileY_var6)
                {
                    yVar4 += 4;
                }
                else
                {
                    if (path57Record.Y1 < tileY_var6)
                    {
                        yVar4 += 7;
                    }
                }

                //seg006_1413_2D16:

                var headingvar7 = Pathfind.GetVectorHeading(xVar2 - currObjXCoordinate, yVar4 - currObjYCoordinate);
                ChangeNpcHeadings(critter, headingvar7);
            }
            else
            {
                //seg006_1413_2C79:
                TurnTowardsPath_Adjusted_seg006_1413_2D3F(critter, path57Record);
            }
            return true;
        }

        static void FlyingCritterPitching_seg006_1413_36B8(uwObject critter, int DestinationTileX, int DestinationTileY)
        {
            var ChangeInPitch_varD = 0;

            if (FlyingPitchingRelated_dseg_67d6_2246 == false)
            {
                if (UWTileMap.ValidTile(DestinationTileX, DestinationTileY))
                {
                    var DestinationTile = UWTileMap.current_tilemap.Tiles[DestinationTileX, DestinationTileY];
                    var CurrObjTile = UWTileMap.current_tilemap.Tiles[critter.tileX, critter.tileY];

                    var CurrObjZ_var9 = critter.zpos;
                    var destinationTileZ_varA = 0x14 + (DestinationTile.floorHeight << 3);
                    var CurrObjTileZ_varB = 0x14 + (CurrObjTile.floorHeight << 3);

                    if (destinationTileZ_varA > 0x78)
                    {
                        destinationTileZ_varA = 0x78;
                    }

                    if (CurrObjTileZ_varB > 0x78)
                    {
                        CurrObjTileZ_varB = 0x78;
                    }

                    //seg006_1413_3749:
                    if (CurrObjZ_var9 >= CurrObjTileZ_varB - 12)
                    {
                        //seg006_1413_3760:
                        if ((dseg_67d6_225E) && (CurrObjZ_var9 < 0x78))
                        {
                            ChangeInPitch_varD = 2;
                        }
                        else
                        {
                            //seg006_1413_376D:
                            if (CurrObjZ_var9 >= destinationTileZ_varA - 8)
                            {
                                if ((CurrObjZ_var9 <= 0x78) && (CurrObjZ_var9 <= destinationTileZ_varA + 8))
                                {
                                    ChangeInPitch_varD = Rng.r.Next(0, 3) - 1;  //either -1,0,+1
                                }
                                else
                                {
                                    ChangeInPitch_varD = -2;
                                }
                            }
                            else
                            {
                                ChangeInPitch_varD = 2;
                            }
                        }

                        //seg006_1413_37B1:
                        if (CurrObjZ_var9 < CurrObjTileZ_varB - 4)
                        {
                            ChangeInPitch_varD++;
                        }

                        if (CurrObjZ_var9 > CurrObjTileZ_varB + 12)
                        {
                            ChangeInPitch_varD--;
                        }
                    }
                    else
                    {
                        ChangeInPitch_varD = 2;
                    }

                    //seg006_1413_37D9:
                    critter.Projectile_Pitch = (short)(0x10 + ChangeInPitch_varD);
                }
            }
        }

        static void TurnTowardsPath_Adjusted_seg006_1413_2D3F(uwObject critter, PathFind57_Turning path57Record)
        {
            var xVar2 = (path57Record.X0 << 3) - 2;
            if (path57Record.X0 == currObj_XHome)
            {
                xVar2 += 6;
            }
            else
            {
                if (path57Record.X0 < currObj_XHome)
                {
                    xVar2 += 0xB;
                }
            }

            //seg006_1413_2D76:
            var yvar4 = (path57Record.Y1 << 3) - 2;
            if (path57Record.Y1 == currObj_YHome)
            {
                yvar4 += 6;
            }
            else
            {
                if (path57Record.Y1 < currObj_YHome)
                {
                    yvar4 += 0xB;
                }
            }

            //seg006_1413_2DA9:
            if (Math.Abs(xVar2 - currObjXCoordinate) + Math.Abs(yvar4 - currObjYCoordinate) >= 3)
            {
                //seg006_1413_2E9E:
                var heading = Pathfind.GetVectorHeading(xVar2 - currObjXCoordinate, yvar4 - currObjYCoordinate);
                ChangeNpcHeadings(critter, heading);
            }
            else
            {
                //seg006_1413_2DCD:
                //This may be buggy.
                var offset = path57Record.PathingOffsetIndex4;
                xVar2 += Pathfind.PathXOffsetTable[offset];
                yvar4 += Pathfind.PathYOffsetTable[offset];

                var heading = Pathfind.GetVectorHeading(critter.npc_xhome - xVar2, critter.npc_yhome - yvar4);
                critter.Projectile_Speed = 1;
                critter.Projectile_Pitch = 16;
                critter.UnkBit_0X13_Bit0to6 = 0xB;

                ChangeNpcHeadings(critter, heading);
            }

        }

        static bool UpdatePathFlag_seg006_1413_2ABB(PathFind57_Turning path57Record)
        {
            if (path57Record.unk2_0_6_PathingIndex < path57Record.UNK3)
            {
                //seg006_1413_2AD4:
                //I guarantee this will be buggy when I get to start testing!
                var index = path57Record.PathingOffsetIndex4;
                path57Record.X0 += Pathfind.PathXOffsetTable[index];
                path57Record.Y1 += Pathfind.PathYOffsetTable[index];

                if ((path57Record.PathingOffsetIndex8 & 0x1) == 0)
                {
                    path57Record.unk2_7 = 0;
                }
                else
                {
                    path57Record.unk2_7 = 1;
                }
                path57Record.unk2_0_6_PathingIndex++;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check if the tileX/Y being tested is the targetTile (or next tile in path) to get to or very close to it.
        /// </summary>
        /// <param name="flagArg0">If true require exact location, if false close enough should do?</param>
        /// <param name="xhome_arg2"></param>
        /// <param name="yhome_arg4"></param>
        /// <param name="xpos_arg6"></param>
        /// <param name="ypos_arg8"></param>
        /// <param name="PathXArgA"></param>
        /// <param name="PathYArgC"></param>
        /// <returns></returns>
        static bool CheckIfAtOrNearTargetTile(int flagArg0, int xhome_arg2, int yhome_arg4, int xpos_arg6, int ypos_arg8, int PathXArgA, int PathYArgC)
        {
            var di_x = xhome_arg2;
            var si_y = yhome_arg4;
            var bx_pathX = PathXArgA;
            var dx_pathY = PathYArgC;

            if (flagArg0 == 0)
            {
                if (xpos_arg6 >= 6 && bx_pathX > di_x)
                {
                    di_x++;
                }
                else
                {
                    //seg006_1413_2BBF:
                    if ((xpos_arg6 <= 1) && (bx_pathX < di_x))
                    {
                        di_x--;
                    }
                }

                //seg006_1413_2BCA: 
                if ((ypos_arg8 >= 6) && (dx_pathY > si_y))
                {
                    si_y++;
                }
                else
                {
                    //seg006_1413_2BD7:
                    if ((ypos_arg8 <= 1) && (dx_pathY < si_y))
                    {
                        si_y--;
                    }
                }
            }

            //seg006_1413_2BE2:
            //true if path/destination are the same tilex/y
            return (si_y == dx_pathY) && (di_x == bx_pathX);
        }

        static void NPCTryToOpenDoor(uwObject critter, uwObject doorobject)
        {
            if (doorobject.classindex != 7)
            {
                if ((_RES == GAME_UW2) && (playerdat.dungeon_level == 0xA))
                {
                    return;//stops goblins from opening doors in the prison tower.
                }
                if (critterObjectDat.dooropenskill(critter.item_id) != 0)
                {
                    use.Use(doorobject, critter, UWTileMap.current_tilemap.LevelObjects, true);
                }
                if (doorobject.OneF0Class == 0x14)
                {
                    if (doorobject.classindex < 8)
                    {
                        //door is closed
                        if ((critterObjectDat.dooropenskill(critter.item_id) != 0) && (Rng.r.Next(2) != 0))
                        {
                            //seg006_1413_3887: 
                            door.CharacterDoorLockAndKeyInteraction(critter, doorobject, -critterObjectDat.dooropenskill(critter.item_id));
                        }
                        else
                        {
                            if (Rng.r.Next(4) == 0)
                            {
                                //NPC will try and bash door
                                //seg006_1413_38BA:
                                damage.DamageObject(objToDamage: doorobject,
                                    basedamage: Rng.r.Next(critterObjectDat.attackdamage(critter.item_id, 0)),
                                    damagetype: 4,
                                    objList: UWTileMap.current_tilemap.LevelObjects, WorldObject: true,
                                    hitCoordinate: Vector3.Zero,
                                    damagesource: critter.index,
                                    ignoreVector: true);
                            }
                        }
                    }
                }
            }
        }

        static void NPCWanderUpdate(uwObject critter)
        {
            var newAnim_var2 = 1;
            int NewHeading = critter.ProjectileHeading;//not sure if it is initialised to this value. possibly vanilla behaviour was to use uninitialised memory in certain circumstances
            int PitchVarA;
            int si;
            if (critter.UnkBit_0X15_Bit7 == 1)
            {//seg007_17A2_E4:
                var tmp = 1 << critter.PathFindIndex_0x16_0_F;
                BitFieldForPathing_dseg_67d6_B4 |= tmp;
                critter.UnkBit_0X15_Bit7 = 0;
            }
            //seg007_17A2_FE:
            var tileVar8 = UWTileMap.current_tilemap.Tiles[currObj_XHome, currObj_YHome];

            if (IsNPCActive_dseg_67d6_2234)
            {//seg007_17A2_134:
                if (critter.npc_attitude == 0)
                {
                    if (Rng.r.Next(0, 2) == 1)
                    {
                        StandStillGoal(critter);
                        return;
                    }
                }
                //seg007_17A2_15D
                if (critterObjectDat.isFlier(critter.item_id))
                {//POSSIBLY this if else is wrong!
                    PitchVarA = 0xE;
                    si = 3;
                    if (currObj_FloorHeight <= 0xE)
                    {
                        if (currObj_FloorHeight < tileVar8.floorHeight + 2)
                        {
                            PitchVarA = 0x10;
                        }
                        else
                        {
                            si = 5;
                        }
                        var tmp = Rng.r.Next(0, si) + PitchVarA;
                        tmp = tmp & 0x1F;
                        tmp = tmp << 3;
                        critter.Projectile_Pitch = (short)tmp;
                    }
                }
                //seg007_17A2_1C2://TODO include UW1 logic
                if (_RES == GAME_UW2)
                {
                    if (critter.npc_animation == ANIMATION_IDLE)
                    {//Seg007_17A2_1CD:
                        if (critterObjectDat.unk_1F_lowernibble(critter.item_id) <= Rng.r.Next(0, 0x10))
                        {//seg007_17A2:01EA
                            if (critter.AnimationFrame != MaxAnimFrame + 1)
                            {
                                newAnim_var2 = -1;
                            }
                        }
                        else
                        {//seg007_17A2_202
                            newAnim_var2 = -1;
                        }
                        //seg007_17A2_202:
                    }
                    else
                    {//seg007_17A2_208:
                        if (critterObjectDat.unk_1F_lowernibble(critter.item_id) < Rng.r.Next(0, 0x10))
                        {
                            if (critter.AnimationFrame == MaxAnimFrame + 1)
                            {
                                newAnim_var2 = 0;
                            }
                        }
                    }
                    //seg007_17A2_241: uw2
                    if (critter.npc_animation != newAnim_var2)
                    {
                        if (newAnim_var2 != -1)
                        {
                            critter.npc_animation = (short)newAnim_var2;
                            GetCritterAnimationGlobalsForCurrObj(critter);
                            if (critter.AnimationFrame > MaxAnimFrame)
                            {
                                critter.AnimationFrame = (byte)MaxAnimFrame;
                            }
                        }
                    }
                }
                else
                {
                    //todo
                    //from X to 
                    //seg007_1798_193: uw1
                    if (critter.npc_animation == ANIMATION_IDLE)
                    {
                        if (critterObjectDat.unk_1F_lowernibble(critter.item_id) > Rng.r.Next(0, 0x10))
                        {
                            if (critter.AnimationFrame == 3)
                            {
                                critter.npc_animation = ANIMATION_WALK;
                            }
                        }
                    }
                    else
                    {
                        if (critterObjectDat.unk_1F_lowernibble(critter.item_id) < Rng.r.Next(0, 0x10))
                        {
                            if (critter.AnimationFrame == 3)
                            {
                                critter.npc_animation = ANIMATION_IDLE;
                            }
                            else
                            {
                                critter.npc_animation = ANIMATION_WALK;
                            }
                        }
                    }
                }

                //seg007_17A2_2A9: uw2
                //seg007_1798_193: uw1
                if (critter.npc_animation == ANIMATION_WALK)
                {
                    if (RelatedToMotionCollision_dseg_67d6_224E != false)
                    {
                        if (HasCurrobjHeadingChanged_dseg_67d6_2242 == false)
                        {
                            //seg007_17A2_2D1
                            var deflection = Rng.r.Next(0, 2);
                            deflection--;
                            deflection = deflection << 6;

                            var newheading = critter.ProjectileHeading;
                            newheading = (ushort)(newheading + deflection);
                            newheading = (ushort)(newheading + 256);
                            newheading = (ushort)(newheading % 256);
                            critter.ProjectileHeading = newheading;
                            critter.heading = (short)((newheading >> 5) & 7);
                            critter.UnkBit_0X13_Bit0to6 = 0;
                            return;
                        }
                        //seg007_17A2_34C

                        if (Rng.r.Next(0, 0x40) >= critterObjectDat.unk_1F_lowernibble(critter.item_id) + 8)
                        {
                            NewHeading = critter.ProjectileHeading;
                        }
                        else
                        {
                            NewHeading = (Rng.r.Next(0, 0x40) + critter.ProjectileHeading + 224) % 0x100;
                        }

                        if (HasCurrobjHeadingChanged_dseg_67d6_2242 == false)
                        {
                            NewHeading = VectorsToPlayer(NewHeading, 0xA);
                        }
                    }
                }
                else
                {
                    //animation is not 1 in uw2 (probably 0)
                    //animation is not 44 in uw1 (probably 32)
                    //seg007_17A2_3BF:
                    if (critterObjectDat.unk_1F_lowernibble(critter.item_id) > Rng.r.Next(0, 0x80))
                    {
                        var tmp = Rng.r.Next(0, 0x40);
                        tmp = tmp + critter.ProjectileHeading;
                        tmp = tmp + 224;
                        tmp = tmp % 0x100;
                        NewHeading = tmp;
                    }
                    else
                    {
                        //uninitialised heading here???
                    }

                }

                //seg007_17A2_3FF:
                critter.ProjectileHeading = (ushort)NewHeading;
                critter.heading = (short)((NewHeading >> 5) & 7);
                critter.npc_heading = ((short)(NewHeading & 0x1F));
                bool frameadvance = false;
                if (critter.npc_animation == ANIMATION_IDLE)
                {//seg007_17A2:044E
                    critter.UnkBit_0X15_6 = 1;
                    critter.UnkBit_0X13_Bit0to6 = 0;
                    critter.Projectile_Speed = 6;
                    frameadvance = Rng.r.Next(0, 2) == 0;
                }
                else
                {//seg007_17A2_489:
                    frameadvance = true;
                    critter.UnkBit_0X15_6 = 0;
                    critter.UnkBit_0X15_Bit7 = 0;
                    critter.UnkBit_0X13_Bit0to6 = (short)critterObjectDat.unk_b_0_7(critter.item_id);
                    critter.Projectile_Speed = 4;

                }
                if (frameadvance)
                {
                    if (_RES == GAME_UW2)
                    {
                        var frame = critter.AnimationFrame;
                        frame++;
                        frame = (byte)(frame % (MaxAnimFrame + 1));
                        critter.AnimationFrame = frame;
                    }
                    else
                    {
                        var frame = critter.AnimationFrame;
                        frame++;
                        frame = (byte)(frame & 3);
                        critter.AnimationFrame = frame;
                    }
                }
                ReactToPlayerPresence(critter);
            }
            else
            {
                critter.Projectile_Speed = 1;
            }
        }

        static void UpdateAnimation(uwObject critter, int NewAnimation_arg0, bool DoNotIdle)
        {
            if (critter.npc_animation == NewAnimation_arg0)
            {
                if (!DoNotIdle)
                {//random 50:50 chance to not advance
                    if (Rng.r.Next(0, 2) == 0)
                    {
                        return;
                    }
                }
                //advance
                var frame = critter.AnimationFrame + 1;
                frame = frame % (MaxAnimFrame + 1);
                critter.AnimationFrame = (byte)frame;
            }
            else
            {
                //reset
                critter.npc_animation = (short)NewAnimation_arg0;
                critter.AnimationFrame = 0;
            }
        }


        /// <summary>
        /// Combat goal
        /// </summary>
        /// <param name="critter"></param>
        static void NPC_Goal5_Attack(uwObject critter)
        {
            if (IsNPCActive_dseg_67d6_2234)
            {
                var RangeAttackStarted = false;
                var var4 = 4;
                var siDist = (currentGTargXVector * currentGTargXVector) + (currentGTargYVector * currentGTargYVector);
                var xDist = currObjQualityX - currObj_XHome;
                var yDist = currObjOwnerY - currObj_YHome;
                var diDistFromHome = (xDist * xDist) + (yDist * yDist);
                if (critter.npc_gtarg == 1)
                {
                    critter.npc_attitude = 0;
                }

                if ((siDist < 0x64) || ((siDist >= 0x64) && (currObj_XHome == currentGTargXHome) && (currObj_YHome == currentGTargYHome)))
                {
                    //seg007_17A2_81D: 
                    if (Math.Abs(currObj_FloorHeight - currentGTargFloorHeight) < 4)
                    {
                        //seg007_17A2_844:
                        ChooseMeleeAttackToMake(critter, siDist);
                        return;
                    }
                    else
                    {
                        if (critterObjectDat.isFlier(critter.item_id))
                        {
                            //seg007_17A2_844:
                            ChooseMeleeAttackToMake(critter, siDist);
                            return;
                        }
                    }
                }

                //seg007_17A2_84F:
                if (critterObjectDat.NPCPower_0x2DBits1To7(critter.item_id) <= 0)
                {
                    //Check if NPC Weapon loot is a ranged weapon
                    //seg007_17A2_87F:
                    if ((critterObjectDat.weaponloot(critter.item_id, 0) >> 4) == 1)
                    {
                        //seg007_17A2_895:
                        RangeAttackStarted = NPCStartRangedAttack(critter);
                    }
                }
                else
                {
                    if (!TryToDoMagicAttack(critter))
                    {
                        //seg007_17A2_86E
                        if (critterObjectDat.isCaster(critter.item_id))
                        {
                            RangeAttackStarted = NPCStartMagicAttack(critter);
                        }
                    }
                }

                //seg007_17A2_89D:
                if (RangeAttackStarted)
                {
                    //seg007_17A2_8AD:
                    if (
                        (critter.npc_animation != ANIMATION_MAGICATTACK)
                        &&
                        (critter.npc_animation != ANIMATION_RANGEDATTACK)
                        &&
                            (
                                (_RES == GAME_UW2 && critter.npc_animation != 3)
                                ||
                                (_RES != GAME_UW2 && critter.npc_animation != 1)
                            )
                        )
                    {
                        UpdateAnimation(critter, ANIMATION_COMBAT_IDLE, true);
                        critter.Projectile_Speed = 4;
                        critter.UnkBit_0X13_Bit0to6 = 0;
                    }
                }
                else
                {
                    //seg007_17A2_906
                    if (siDist > 0x100)
                    {
                        if (critter.npc_level == 4)
                        {
                            if (critter.UnkBit_0x19_5 == 0)
                            {
                                if (Math.Pow(critterObjectDat.maybemorale(critter.item_id), 2) < diDistFromHome)
                                {
                                    //seg007_17A2_953:
                                    //NPC Gives up.
                                    critter.UnkBit_0x19_0_likelyincombat = 0;
                                    critter.UnkBit_0x19_1 = 0;
                                    SetGoalAndGtarg(critter, 4, 0);
                                    return;
                                }
                            }
                        }
                    }

                    //seg007_17A2_97D: 
                    if (critterObjectDat.isCaster(critter.item_id))
                    {
                        AttackGoalSearchForTarget(critter, ref currentGTargXHome, ref currentGTargYHome, var4);
                    }
                    else
                    {
                        AttackGoalSearchForTarget(critter, ref currentGTargXHome, ref currentGTargYHome, 1);
                    }
                }
            }
        }


        /// <summary>
        /// Possibly attack at a distance or flee from player.
        /// </summary>
        /// <param name="critter"></param>
        static void NPC_Goal6(uwObject critter)
        {
            if (IsNPCActive_dseg_67d6_2234)
            {
                //seg007_17A2_1179:

                var headingvar1 = Pathfind.GetVectorHeading(currentGTargXVector, currentGTargYVector);
                var zDiffVar4 = currentGoalTarget.zpos - critter.zpos;
                var HeadingVar3 = 0;
                if (critterObjectDat.isFlier(critter.item_id))
                {
                    //seg007_17A2_11B7:
                    var var6 = 0;
                    if (critter.zpos <= 0x6E)
                    {
                        var6 = 2;
                    }
                    //seg007_17A2_11D1
                    critter.Projectile_Pitch = (short)(var6 + Rng.r.Next(5) + 13);
                }

                if (currentGTargSquaredDistanceByTiles <= 3 && zDiffVar4 < 0x10)
                {
                    //seg007_17A2_1214:
                    if (Rng.r.Next(0x100) >= (critterObjectDat.maybemorale(critter.item_id) >> 3))
                    {
                        if ((RelatedToMotionCollision_dseg_67d6_224E == false) || HasCurrobjHeadingChanged_dseg_67d6_2242)
                        {
                            //seg007_17A2_126E:
                            critter.ProjectileHeading = (ushort)(((headingvar1 + 4) % 8) << 5);
                            critter.heading = (short)headingvar1;
                            critter.npc_heading = 0;
                            UpdateAnimation(critter, 1, true);
                            critter.UnkBit_0X13_Bit0to6 = (short)((critterObjectDat.unk_b(critter.item_id) + 1) / 2);
                            return;
                        }
                    }

                    //seg007_17A2_1242:
                    critter.UnkBit_0x19_4 = 1;
                    SetGoalAndGtarg(critter, 9, critter.npc_gtarg);
                    return;
                }
                else
                {
                    //seg007_17A2_12ED:
                    if ((RelatedToMotionCollision_dseg_67d6_224E) && (HasCurrobjHeadingChanged_dseg_67d6_2242 == false))
                    {
                        //seg007_17A2_1303 
                        if (currentGTargSquaredDistanceByTiles >= 9)
                        {
                            //seg007_17A2_137D
                            HeadingVar3 = Rng.r.Next(0x20) + (((critter.ProjectileHeading >> 5) + ((Rng.r.Next(2) << 1) - 1) + 8) % 5) << 5;
                            //goto 17a2:1444
                        }
                        else
                        {
                            //seg007_17A2_137D
                            if (critter.npc_goal == 9)
                            {
                                critter.UnkBit_0X13_Bit0to6 = 0;
                                ChangeNpcHeadings(critter, headingvar1);
                                critter.Projectile_Speed = 4;
                                UpdateAnimation(critter, 2, true);
                                return;
                            }
                            else
                            {
                                critter.UnkBit_0x19_4 = 1;
                                SetGoalAndGtarg(critter, 9, critter.npc_gtarg);
                                return;
                            }
                        }
                    }
                    else
                    {
                        //seg007_17A2_13BD:
                        if (TryToDoMagicAttack(critter))
                        {
                            return;
                        }
                        else
                        {
                            //seg007_17A2_13C8
                            var rngvar2 = Rng.r.Next(0x40);
                            if (rngvar2 >= critterObjectDat.unk_1F_lowernibble(critter.item_id) + 8)
                            {
                                //seg007_17A2_1414:
                                HeadingVar3 = critter.ProjectileHeading;
                            }
                            else
                            {
                                HeadingVar3 = (Rng.r.Next(0x40) + critter.ProjectileHeading + 0xE0) % 0x100;
                            }
                            if (!HasCurrobjHeadingChanged_dseg_67d6_2242)
                            {
                                HeadingVar3 = VectorsToPlayer(HeadingVar3, 0x18);
                            }
                            //goto 17a2:1444
                        }
                    }
                }

                //seg007_17A2_1444
                //rejoin here
                critter.ProjectileHeading = (ushort)HeadingVar3;
                critter.heading = (short)(HeadingVar3 >> 5);
                critter.npc_heading = (short)(HeadingVar3 & 0x1F);
                if (currentGTargSquaredDistanceByTiles >= 0x40)
                {
                    //seg007_17A2_14A2:
                    critter.UnkBit_0X13_Bit0to6 = (short)critterObjectDat.unk_b(critter.item_id);
                }
                else
                {
                    //seg007_17A2_1493
                    critter.UnkBit_0X13_Bit0to6 = (short)critterObjectDat.speed(critter.item_id);
                }
                UpdateAnimation(critter, 1, true);
                critter.Projectile_Speed = 4;
            }
        }

        /// <summary>
        /// NPC Wanders around until it gets too far from home.
        /// </summary>
        /// <param name="critter"></param>
        static void NPC_Goal8(uwObject critter)
        {
            if (IsNPCActive_dseg_67d6_2234)
            {
                if ((critter.npc_attitude == 0) && (critter.npc_goal != 4))
                {
                    SetGoalAndGtarg(critter, 4, 1);
                }
                else
                {
                    var xDiff = Math.Pow(currObjQualityX - currObj_XHome, 2);
                    var yDiff = Math.Pow(currObjOwnerY - currObj_YHome, 2);
                    if (xDiff + yDiff <= critterObjectDat.maybeNPCTravelRange(critter.item_id))
                    {
                        NPCWanderUpdate(critter);
                    }
                    else
                    {
                        var tile = UWTileMap.current_tilemap.Tiles[currObjQualityX, currObjOwnerY];
                        NPC_Goto(critter, currObjQualityX, currObjOwnerY, tile.floorHeight);
                    }
                }
            }
        }


        /// <summary>
        /// Try and do ranged or melee attacks based on range.
        /// </summary>
        /// <param name="critter"></param>
        static void NPC_Goal9(uwObject critter)
        {
            if (IsNPCActive_dseg_67d6_2234)
            {
                var si_dist = (currentGTargXVector * currentGTargXVector) + (currentGTargYVector * currentGTargYVector);
                if ((si_dist < 0x90) || ((currObj_XHome == currentGTargXHome) && (currObj_YHome == currentGTargYHome)))
                {
                    //seg007_17A2_10C7
                    ChooseMeleeAttackToMake(critter, si_dist);
                }
                else
                {
                    if (currentGTargSquaredDistanceByTiles <= 4)
                    {
                        //seg007_17A2_111C: 
                        var heading_var1 = Pathfind.GetVectorHeading(currentGTargXVector, currentGTargYVector);
                        critter.UnkBit_0X13_Bit0to6 = 0;
                        ChangeNpcHeadings(critter, heading_var1);
                        critter.Projectile_Speed = 4;
                        UpdateAnimation(critter, 2, true);
                    }
                    else
                    {
                        if (!TryToDoMagicAttack(critter))
                        {
                            //seg007_17A2_10E3
                            if (critterObjectDat.NPCPower_0x2DBits1To7(critter.item_id) > 0)
                            {
                                NPCStartMagicAttack(critter);
                            }
                            else
                            {
                                if (critterObjectDat.WeaponLootHighNibble(critter.item_id) == 1)
                                {
                                    NPCStartRangedAttack(critter);
                                }
                                else
                                {
                                    NPC_Goal6(critter);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static int GetCritterAnimationGlobalsForCurrObj(uwObject critter)
        {
            short CalcedFacing = CalculateFacingAngleToNPC(critter);
            string animname = CritterArt.GetAnimName(critter.npc_animation, CalcedFacing);
            var crit = CritterArt.GetCritter(critter.item_id & 0x3F);
            if (crit.Animations.ContainsKey(animname))
            {
                var anim = crit.Animations[animname];
                MaxAnimFrame = anim.maxNoOfFrames;
            }
            return CalcedFacing;
        }


        /// <summary>
        /// Does something with getting a heading towards the player??
        /// </summary>
        /// <param name="headingArg0"></param>
        /// <param name="maybedist"></param>
        /// <returns></returns>
        static int VectorsToPlayer(int headingArg0, int maybedist)
        {
            var playerXCoordinate = playerdat.xpos + (playerdat.playerObject.npc_xhome << 3);
            var playerYCoordinate = playerdat.ypos + (playerdat.playerObject.npc_yhome << 3);
            var xvector = playerXCoordinate - currentGTargXCoord;
            var yvector = playerYCoordinate - currentGTargYCoord;
            if (Math.Pow(xvector, 2) + Math.Pow(yvector, 2) > Math.Pow(maybedist, 2))
            {
                var HeadingVarB = ((Pathfind.GetVectorHeading(xvector, yvector) + 4) % 8) << 5;
                HeadingVarB += 0x100;
                HeadingVarB -= headingArg0;
                var varD = HeadingVarB % 0x100;
                if ((varD >= 0x40) || (varD <= 0xC0))
                {
                    if (varD >= 0x60)
                    {
                        if (varD >= 0x80)
                        {
                            if (varD <= 0xA0)
                            {
                                return (headingArg0 + 0xE0) % 0x100;
                            }
                            else
                            {
                                return (HeadingVarB + 0x20) % 0x100;
                            }
                        }
                        else
                        {
                            return (HeadingVarB + 0x20) % 0x100;
                        }
                    }
                    else
                    {
                        return (HeadingVarB + 0xE0) % 0x100;
                    }
                }
                else
                {
                    return headingArg0;
                }
            }
            else
            {
                return headingArg0;
            }
        }


        /// <summary>
        /// Seems to turn the critter towards the player when player is close to them or when the play has their weapon drawn
        /// </summary>
        /// <param name="critter"></param>
        static void ReactToPlayerPresence(uwObject critter)
        {
            if
                (
                (playerdat.play_drawn == 1)
                ||
                (critter.UnkBit_0X13_Bit0to6 == 0)
                )
            {
                critter.npc_gtarg = 1;
                GetDistancesToGTarg(critter);
                if (Math.Pow(currentGTargXVector, 2) + Math.Pow(currentGTargYVector, 2) < 0x90)
                {
                    var heading = Pathfind.GetVectorHeading(currentGTargXVector, currentGTargYVector);
                    critter.UnkBit_0X13_Bit0to6 = 0;
                    critter.Projectile_Speed = 6;
                    if (_RES == GAME_UW2)
                    {
                        UpdateAnimation(critter, ANIMATION_IDLE, false);
                    }
                    else
                    {
                        critter.npc_animation = ANIMATION_IDLE;
                        if (Rng.r.Next(0, 2) == 1)
                        {
                            critter.AnimationFrame = (byte)((critter.AnimationFrame + 1) & 3);
                        }
                    }
                    critter.heading = (short)heading;
                    critter.npc_heading = 0;
                }
            }
        }

        /// <summary>
        /// Critter checks distances to player and chooses melee attack based on defined probabilities
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="DistArg0"></param>
        static void ChooseMeleeAttackToMake(uwObject critter, int DistArg0)
        {
            var headingvar2 = Pathfind.GetVectorHeading(currentGTargXVector, currentGTargYVector);
            ChangeNpcHeadings(critter, headingvar2);
            critter.UnkBit_0X15_6 = 0;

            var animVar4 = 0x20;
            var AttackTypeVar6 = 0;

            if (DistArg0 >= 0x31)
            {
                if (DistArg0 <= 0x51)
                {
                    //seg007_17A2_A61:
                    if (Rng.r.Next(0x40) >= critterObjectDat.dexterity(critter.item_id))
                    {
                        animVar4 = 2;
                        AttackTypeVar6 = 0;
                    }
                    else
                    {
                        headingvar2 = Rng.r.Next(8);
                        animVar4 = 2;
                        AttackTypeVar6 = 1;
                    }
                }
                else
                {
                    //seg007_17A2_A55:
                    animVar4 = 1;
                    AttackTypeVar6 = 2;
                }
            }
            else
            {
                if (Rng.r.Next(4) != 0)
                {
                    //seg007_17A2_A33:
                    headingvar2 = (headingvar2 + 4) % 8;
                    animVar4 = 1;
                    AttackTypeVar6 = 2;
                }
                else
                {
                    //seg007_17A2_9F5:
                    headingvar2 = ((headingvar2 + (Rng.r.Next(2) << 1) - 1) + 8) % 8;
                    animVar4 = 2;
                    AttackTypeVar6 = (critterObjectDat.unk_b(critter.item_id) << 1) / 3;
                }
            }

            //seg007_17A2_A9D:
            critter.ProjectileHeading = (ushort)(headingvar2 << 5);
            critter.UnkBit_0X13_Bit0to6 = (short)AttackTypeVar6;
            if (critter.npc_animation != animVar4)
            {
                critter.npc_animation = (short)animVar4;
                critter.AnimationFrame = 0;
            }
            else
            {
                critter.AnimationFrame = (byte)((critter.AnimationFrame + 1) % MaxAnimFrame);
            }

            if (critterObjectDat.isFlier(critter.item_id))
            {
                var zposDiffVar1 = currentGoalTarget.zpos + 14 - critter.zpos;
                if (zposDiffVar1 <= 1)
                {
                    //seg007_17A2_B6C:
                    if (zposDiffVar1 >= -1)
                    {
                        //seg007_17A2_B80:
                        critter.Projectile_Pitch = (short)(Rng.r.Next(3) + 0xF);
                    }
                    else
                    {
                        critter.Projectile_Pitch = 0xE;
                    }
                }
                else
                {
                    critter.Projectile_Pitch = 0x12;
                }
            }

            //seg007_17A2_BA8: 
            if (DistArg0 <= 0x100)
            {
                if (Rng.r.Next(4) != 0)
                {
                    //seg007_17A2_C35
                    if (critter.Swing < 0xF)
                    {
                        critter.Swing++;
                    }
                }
                else
                {
                    var rngSI = Rng.r.Next(0x64);
                    var attackNoVar8 = 0;
                    while ((critterObjectDat.attackprobability(critter.item_id, attackNoVar8) >= rngSI) && (attackNoVar8 < 2))
                    {
                        rngSI -= critterObjectDat.attackprobability(critter.item_id, attackNoVar8);//reduce probability threshold for next check.
                        attackNoVar8++;
                    }
                    if (_RES == GAME_UW2)
                    {
                        critter.npc_animation = (short)(attackNoVar8 + 3);
                    }
                    else
                    {
                        critter.npc_animation = (short)(attackNoVar8 + 1);
                    }

                    critter.AnimationFrame = 0;
                }
            }

            //seg007_17A2_C68:
            critter.Projectile_Speed = 4;
        }

        /// <summary>
        /// Tries to do a special? magic attack
        /// </summary>
        /// <param name="critter"></param>
        /// <returns></returns>
        static bool TryToDoMagicAttack(uwObject critter)
        {
            if (critterObjectDat.spell2C(critter.item_id) == -1)
            {
                return false;
            }

            if (Rng.r.Next(0x100) >= critterObjectDat.NPCPower_0x2DBits1To7(critter.item_id))
            {
                return false;
            }

            if (UWTileMap.current_tilemap.Tiles[currObj_XHome, currObj_YHome].noMagic != 0)
            {
                return false;
            }

            if (_RES != GAME_UW2)
            {
                //check for members of a mage faction casting spells in tybals lair while the Orb is not destroyed.
                if (playerdat.dungeon_level == 7)
                {
                    if (playerdat.isOrbDestroyed == false)
                    {
                        if (critterObjectDat.generaltype(critter.item_id) == 0x13)
                        {
                            return false;
                        }
                    }
                }
            }

            critter.UnkBit_0X13_Bit0to6 = 0;
            critter.npc_animation = ANIMATION_MAGICATTACK;
            critter.AnimationFrame = 0;
            critter.npc_spellindex = 3;
            return true;
        }

        /// <summary>
        /// Starts the magic attack animation
        /// </summary>
        /// <param name="critter"></param>
        /// <returns></returns>
        static bool NPCStartMagicAttack(uwObject critter)
        {
            if (UWTileMap.current_tilemap.Tiles[currObj_XHome, currObj_YHome].noMagic != 0)
            {
                return false;
            }

            if (_RES != GAME_UW2)
            {
                //check for members of a mage faction casting spells in tybals lair while the Orb is not destroyed.
                if (playerdat.dungeon_level == 7)
                {
                    if (playerdat.isOrbDestroyed == false)
                    {
                        if (critterObjectDat.generaltype(critter.item_id) == 0x13)
                        {
                            return false;
                        }
                    }
                }
            }

            if (currentGTargSquaredDistanceByTiles >= 0x40)
            {
                return false;
            }
            if (!Pathfind.TestBetweenPoints(
                srcX: currObjXCoordinate, srcY: currObjYCoordinate, srcZ: critter.zpos + commonObjDat.height(critter.item_id),
                dstX: currentGTargXCoord, dstY: currentGTargYCoord, dstZ: currentGoalTarget.zpos + commonObjDat.height(currentGoalTarget.item_id)))
            {
                return false;
            }

            if (!TurnTowardsTarget(critter, 1))
            {
                return false;
            }

            if (critterObjectDat.NPCPower_0x2DBits1To7(critter.item_id) <= Rng.r.Next(0x80))
            {
                return true;
            }
            else
            {
                critter.UnkBit_0X13_Bit0to6 = 0;
                critter.npc_animation = ANIMATION_MAGICATTACK;
                critter.AnimationFrame = 0;
                if (Rng.r.Next(0x10) >= 0xB)
                {
                    critter.npc_spellindex = 2;
                }
                else
                {
                    critter.npc_spellindex = 1;
                }
                return true;
            }
        }

        /// <summary>
        /// Starts the ranged attack animation.
        /// </summary>
        /// <param name="critter"></param>
        /// <returns></returns>
        static bool NPCStartRangedAttack(uwObject critter)
        {
            if (currentGTargSquaredDistanceByTiles < 0x10)
            {
                if (Pathfind.TestBetweenPoints(
                    srcX: currObjXCoordinate, srcY: currObjYCoordinate, srcZ: critter.zpos + commonObjDat.height(critter.item_id),
                    dstX: currentGTargXCoord, dstY: currentGTargYCoord, dstZ: currentGoalTarget.zpos + commonObjDat.height(currentGoalTarget.item_id)))
                {
                    if (TurnTowardsTarget(critter, 1))
                    {
                        if (Rng.r.Next(0xc0) <= critterObjectDat.dexterity(critter.item_id))
                        {
                            critter.UnkBit_0X13_Bit0to6 = 0;
                            critter.npc_animation = ANIMATION_RANGEDATTACK;//todo update animation value for UW1
                            critter.AnimationFrame = 0;
                        }
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Calculates the pitch to apply to thrown missiles or magic spells to hit the target.
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="arg0"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        static int GetPitchToGTarg(uwObject critter, int arg0, int arg2)
        {
            GetDistancesToGTarg(critter);
            var di_ZDist = currentGoalTarget.zpos - critter.zpos;
            var siDist = (int)Math.Sqrt(currentGTargSquaredDistanceByCoordinates);
            if (siDist != 0)
            {
                var var2Pitch = (di_ZDist << 2) / siDist;
                if (var2Pitch > 0xF)
                {
                    var2Pitch = 0xF;
                }
                if (var2Pitch < -15)
                {
                    var2Pitch = -15;
                }
                if ((arg2 != 0) && (arg0 != 0))
                {
                    var2Pitch = var2Pitch + ((siDist * 3) / arg0);
                }
                return var2Pitch;
            }
            else
            {
                if (di_ZDist <= 0)
                {
                    return -15;
                }
                else
                {
                    return 15;
                }
            }
        }
    } //end class
}//end namespace