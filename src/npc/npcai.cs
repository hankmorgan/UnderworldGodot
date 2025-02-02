using System.Diagnostics;


namespace Underworld
{
    /// <summary>
    /// Class for managing the ai processing loop for npcs
    /// </summary>
    public partial class npc : objectInstance
    {

        static int MaxAnimFrame;
        static int dseg_67d6_224E;//needs to be set in 1413:ABF
        static int IsNPCActive_dseg_67d6_2234;
        static int dseg_67d6_225E;
        static int HasCurrobjHeadingChanged_dseg_67d6_2242;
        static int dseg_67d6_2269;
        static int dseg_67d6_226F;
        static int dseg_67d6_2246;

        static int dseg_67d6_B4;

        static int dseg_67d6_226E;

        static int currObj_XHome;
        static int currObj_YHome;
        static int currObj_Zpos;
        static int currObjXCoordinate;
        static int currObjYCoordinate;
        static int currObjQualityX;
        static int currObjOwnerY;
        static int currObjProjectileHeading;
        static int currObjTotalHeading;
        static int currObjUnk13;
        static int currObjHeight;

        static uwObject currentGoalTarget;
        static int currentGTargXHome;
        static int currentGTargYHome;
        static int currentGTargHeight;

        static int currentGTargXCoord;
        static int currentGTargYCoord;

        static int currentGTargXVector;
        static int currentGTargYVector;

        static int currentGTargSquaredDistanceByTiles;
        static int currentGTargSquaredDistanceByCoordinates;

        static int zposofGTARG;

        static uwObject collisionObject = new uwObject(); //tmp

        /// <summary>
        /// Initial stage of processing AI. Handles the execution of attacks, NPCs reacting to combat and then jumps into handling of goals.
        /// </summary>
        /// <param name="critter"></param>
        public static void NPCInitialProcess(uwObject critter)
        {
            var distancesquaredtoplayer = ((critter.npc_yhome - playerdat.tileY) ^ 2)
                                            + ((critter.npc_xhome - playerdat.tileX) ^ 2);

            var n = (npc)critter.instance;

            if (
                (distancesquaredtoplayer <= 0x64)
                ||
                (critter.npc_goal == (byte)npc_goals.npc_goal_follow)
                )
            {
                
                var CalcedFacing = GetCritterAnimationGlobalsForCurrObj(critter);

                if (critterObjectDat.isFlier(critter.item_id))
                {
                    //special setup for flying ai                    
                }
                else
                {
                    if (critterObjectDat.isSwimmer(critter.item_id))
                    {
                        //special setup for swimming ai

                    }
                    else
                    {
                        //default ai (26ba in uw2,  285C in uw1 )

                    }
                }

                if ((commonObjDat.scaleresistances(critter.item_id) & 8) != 0)
                {
                    //Set values in motion arrays (defined in previous section. )

                }

                //set some globals
                dseg_67d6_224E = 0;
                IsNPCActive_dseg_67d6_2234 = 1;
                dseg_67d6_225E = 0;
                HasCurrobjHeadingChanged_dseg_67d6_2242 = 0;
                dseg_67d6_2269 = 0;
                dseg_67d6_226F = 0;
                dseg_67d6_2246 = 0;

                if (critter.npc_animation != 1)
                {
                    if (critter.npc_animation != 0)
                    {
                        if (critter.UnkBit_0X15_Bit7 == 1)
                        {
                            dseg_67d6_B4 |= (1 << critter.UnkBits_0x16_0_F);
                            critter.UnkBit_0X15_Bit7 = 0;
                        }
                    }
                }

                if (
                    (critter.UnkBit_0X15_Bit6 == 0)
                    ||
                    (critter.UnkBit_0X13_Bit0to6 != 0)
                    ||
                    (critter.Projectile_Pitch != 0x10)
                )
                {
                    //motion
                    var ProjectileHeading = critter.ProjectileHeading;
                    motion.InitMotionParams(critter);
                    motion.seg006_1413_9F5(critter);
                    motion.CalculateMotion_seg006_1413_D6A(critter);
                    motion.ApplyProjectileMotion_seg030_2BB7_689(critter);
                    if (ProjectileHeading != critter.ProjectileHeading)
                    {
                        HasCurrobjHeadingChanged_dseg_67d6_2242 = 1;
                    }
                }

                if ((commonObjDat.scaleresistances(critter.item_id) & 8) != 0)
                {
                    //update values in motion arrays
                }

                //set some other values
                currObj_XHome = critter.npc_xhome;
                currObj_YHome = critter.npc_yhome;
                currObj_Zpos = critter.zpos;
                currObjXCoordinate = (currObj_XHome << 3) + critter.xpos;
                currObjYCoordinate = (currObj_YHome << 3) + critter.ypos;
                currObjQualityX = critter.quality;
                currObjOwnerY = critter.owner;
                currObjProjectileHeading = critter.ProjectileHeading;
                currObjTotalHeading = (critter.heading << 5) + critter.npc_heading;
                currObjUnk13 = critter.UnkBit_0X13_Bit0to6;
                currObjHeight = commonObjDat.height(critter.item_id);

                //GetCritterAnimationGlobalsForCurrObj(critter);

                //finally process goals.
                if ((critter.npc_goal == 0xB) || (critter.npc_goal == 3))
                {
                    NpcBehaviours(critter);

                    ///remove animation update from here since frames are actually udpated as part of ai.
                    // if (critter.AnimationFrame>=MaxAnimFrame)
                    // {
                    //     critter.AnimationFrame=0;
                    // }
                    // else
                    // {
                    //     critter.AnimationFrame++;
                    // }
                    // n.SetAnimSprite(critter.npc_animation,critter.AnimationFrame, CalcedFacing);//temp keep anims running
                }
                else
                {
                    if (
                    ((critter.npc_animation == 7) && (_RES == GAME_UW2))
                    ||
                    ((critter.npc_animation == 0xC) && (_RES != GAME_UW2))
                    )
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
                            n.SetAnimSprite(critter.npc_animation, critter.AnimationFrame, CalcedFacing);
                        }
                    }
                    else
                    {//check and process combat anims and other goals
                        switch (critter.npc_animation)
                        {
                            case 3://weapon swings
                            case 4:
                            case 5:
                                {
                                    if (critter.AnimationFrame == 0)
                                    {
                                        if (critter.npc_gtarg == 1)
                                        {
                                            //TODO set music if not playing any combat theme
                                            //todo set combat music timer
                                        }
                                    }
                                    if (critter.AnimationFrame == 3)
                                    {
                                        //apply attack
                                        Debug.Print("NPC makes attack");
                                    }
                                    if (critter.AnimationFrame >= MaxAnimFrame)
                                    {
                                        critter.npc_animation = 2;
                                        critter.AnimationFrame = 0;
                                    }
                                    else
                                    {
                                        critter.AnimationFrame++;
                                    }
                                    n.SetAnimSprite(critter.npc_animation, critter.AnimationFrame, CalcedFacing);

                                    break;
                                }
                            case 6://ranged attack/ magic attack
                                {
                                    if (critter.AnimationFrame == 3)
                                    {
                                        if (critter.npc_spellindex == 0)
                                        {//ranged weapon
                                         //var rangedweapon = critterObjectDat.weaponloot(critter.item_id,0);
                                         //get pitch to gtarg
                                         //launch missile using rangedwweapon                                        
                                        }
                                        else
                                        {//magic spell
                                         //get pitch to gtarg
                                            var effectid = critterObjectDat.spell(critter.item_id, critter.npc_spellindex);
                                            SpellCasting.CastSpellFromObject(
                                                spellno: effectid,
                                                obj: critter,
                                                playerCast: false);
                                        }
                                    }

                                    if (critter.AnimationFrame >= MaxAnimFrame)
                                    {
                                        critter.npc_animation = 2;
                                        critter.AnimationFrame = 0;
                                    }
                                    else
                                    {
                                        critter.AnimationFrame++;
                                    }
                                    n.SetAnimSprite(critter.npc_animation, critter.AnimationFrame, CalcedFacing);
                                    break;
                                }
                            default:
                                {
                                    NpcBehaviours(critter);
                                    //temp
                                    // if (critter.AnimationFrame>=MaxAnimFrame)
                                    // {
                                    //     critter.AnimationFrame=0;
                                    // }
                                    // else
                                    // {
                                    //     critter.AnimationFrame++;
                                    // }
                                    n.SetAnimSprite(critter.npc_animation, critter.AnimationFrame, CalcedFacing);//temp keep anims running
                                    break;
                                }
                        }
                    }
                    //update unk0xA, probably a refresh rate
                    critter.UnkBit_0XA_Bit0123 = (short)((critter.UnkBit_0XA_Bit0123 + critter.Projectile_Speed) % 16);
                }
            }
            else
            {//distance >0x64 and goal is not follow. do a calculation                
                critter.UnkBit_0XA_Bit0123 = (short)((critter.UnkBit_0XA_Bit0123 + 8) % 16);
            }
        }

        /// <summary>
        /// Handles high level goal processing, critter footsteps and determines if an npc needs to become hostile to the player and so on
        /// </summary>
        static void NpcBehaviours(uwObject critter)
        {
            dseg_67d6_226E = 0;//global related to goto
            critter.UnkBit_0x18_5 = 0;
            critter.UnkBit_0x18_6 = 0;
            bool gtargAvailable = false;

            if ((critter.npc_goal != 0xB) && (critter.npc_goal != 0xF))
            {
                if ((critter.npc_animation == 1) && ((critter.AnimationFrame & 0x1) == 1))
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
                //passisve test
                if (critterObjectDat.unkPassivenessProperty(critter.item_id) == false)
                {
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
                        if (playerdat.LastDamagedNPCTime + 512 < playerdat.game_time)
                        {
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
                                    SetGoalAndGtarg(critter, gtarg, 5);
                                    SetNPCTargetDestination(critter, playerdat.LastDamagedNPCTileX, playerdat.LastDamagedNPCTileY, playerdat.LastDamagedNPCZpos);
                                }
                            }
                        }
                    }
                    if (critter.ProjectileSourceID > 0)//last hit
                    {
                        if
                            (
                            (critter.ProjectileSourceID != 1)
                            ||
                            (
                                (critter.ProjectileSourceID == 1) && (critter.IsAlly == 1)
                            )
                            )
                        {
                            bool tmp = false;
                            if (critter.IsAlly == 1)
                            {
                                tmp = true;
                            }
                            else
                            {
                                uwObject lasthitobject;
                                if (critter.ProjectileSourceID == 1)
                                {
                                    lasthitobject = playerdat.playerObject;
                                }
                                else
                                {
                                    lasthitobject = UWTileMap.current_tilemap.LevelObjects[critter.ProjectileSourceID];
                                }
                                tmp = (lasthitobject.IsAlly == 1);
                            }
                            if (tmp)
                            {
                                if (critter.npc_gtarg != critter.ProjectileSourceID)
                                {
                                    critter.npc_gtarg = (byte)critter.ProjectileSourceID;
                                }

                                if (GetDistancesToGTarg(critter))
                                {
                                    int newGoal;
                                    //int newGtarg  = critter.ProjectileSourceID;;
                                    gtargAvailable = true;
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
            }
            //processgoals here
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
                    //and so on
                case 2:
                    NPCWander(critter);
                    break;
            }

        }


        public static void NpcGoto(uwObject critter, int tileX, int tileY, int Height)
        {

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
            if (critter.npc_gtarg == 1)
            {
                currentGoalTarget = playerdat.playerObject;
            }
            else
            {
                currentGoalTarget = UWTileMap.current_tilemap.LevelObjects[critter.npc_gtarg];
            }
            if (currentGoalTarget.npc_hp <= 0)
            {
                return false;
            }
            else
            {
                currentGTargXHome = currentGoalTarget.npc_xhome;
                currentGTargYHome = currentGoalTarget.npc_yhome;
                currentGTargHeight = currentGoalTarget.zpos >> 3;
                currentGTargXCoord = currentGoalTarget.xpos + (currentGoalTarget.npc_xhome << 3);
                currentGTargYCoord = currentGoalTarget.ypos + (currentGoalTarget.npc_yhome << 3);
                currentGTargXVector = currentGTargXCoord - currObjXCoordinate;
                currentGTargYVector = currentGTargYCoord - currObjYCoordinate;
                zposofGTARG = currentGoalTarget.zpos >> 3;
                currentGTargSquaredDistanceByTiles = (currentGTargXHome - currObj_XHome) ^ 2 + (currentGTargYHome - currObj_YHome) ^ 2;
                currentGTargSquaredDistanceByCoordinates = (currentGTargXCoord - currObjXCoordinate) ^ 2 + (currentGTargYCoord - currObjYCoordinate) ^ 2;
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
            if (IsNPCActive_dseg_67d6_2234 != 0)
            {
                if (critter.npc_attitude == 0)
                {
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
                                SetNPCTargetDestination(critter, gtarg_x, gtarg_y, zposofGTARG);
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
                                        NPC_Goto(critter, gtarg_x, gtarg_y, zposofGTARG);
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
                        NPCWander(critter);
                        return;
                    case 0:
                    case 7:
                        critter.Projectile_Speed = 6;
                        critter.UnkBit_0X13_Bit0to6 = 0;
                        if (_RES == GAME_UW2)
                        {
                            UpdateAnimation(critter, 0, false);
                        }
                        else
                        {
                            UpdateAnimation(critter, 32, false);
                        }
                        return;
                    default:
                        NPC_Goal8(critter);
                        return;
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
            xHomeFound = currentGTargXHome; yHomeFound = currentGTargYHome;

            var xvector = currentGTargXHome - critter.tileX;
            var yvector = currentGTargXHome - critter.tileY;

            var si_dist = xvector ^ 2 + yvector ^ 2;
            var tmp_critter = (critterObjectDat.combatdetectionrange(critter.item_id) * critterObjectDat.maybestealth(critter.item_id)) / 16;
            var tmp_gtarg = (critterObjectDat.combatdetectionrange(currentGoalTarget.item_id) * critterObjectDat.maybestealth(currentGoalTarget.item_id)) / 16;

            var score = (tmp_critter * tmp_gtarg) / 4;

            if (score < si_dist)
            {
                tmp_critter = (critterObjectDat.theftdetectionrange(critter.item_id) * critterObjectDat.unk_1D_uppernibble(critter.item_id)) / 16;
                tmp_gtarg = (critterObjectDat.theftdetectionrange(currentGoalTarget.item_id) * critterObjectDat.unk_1D_uppernibble(currentGoalTarget.item_id)) / 16;
                var var8 = (tmp_critter * tmp_gtarg);
                if (si_dist < var8)
                {
                    var HeadingToTarget_var3 = pathfind.GetVectorHeading(xvector, yvector);
                    var var5 = (8 + (HeadingToTarget_var3 - critter.heading)) % 8;
                    if ((var5 == 0) || (var5 == 1) || (var5 == 7))
                    {
                        var result = pathfind.TestBetweenPoints(
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
                    critter.UnkBit_0x19_0_likelyincombat = 1;
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
            var heading_var3 = pathfind.GetVectorHeading(vectorX, vectorY);

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
                return Seg007_17A2_1CE5(vectorX, vectorY);
            }
        }

        /// <summary>
        /// Placeholder. Function only applies when npcs casting spells
        /// </summary>
        /// <param name="vectorX"></param>
        /// <param name="vectorY"></param>
        /// <returns></returns>
        static bool Seg007_17A2_1CE5(int vectorX, int vectorY)
        {
            return true;
        }

        static void NPC_Goto(uwObject critter, int targetX, int targetY, int targetZ)
        {
            //placeholder
            var Bit7Cleared_Var5 = false;
            var MaybeIsFlier_Var6 = false;

            SetNPCTargetDestination(
                critter: critter, 
                newTargetX: targetX, 
                newTargetY: targetY, 
                newHeight: targetZ);

            if (critter.UnkBit_0x18_5 == 1)
            {
                if (critter.UnkBit_0X15_Bit7 == 1)
                {
                    dseg_67d6_B4 |= (1<<critter.UnkBits_0x16_0_F);
                    critter.UnkBit_0X15_Bit7 = 0;
                }
            }

            var xDiff = targetX-currObj_XHome;
            var yDiff = targetY-currObj_YHome;

            if ((xDiff==0) && (xDiff==0))
            {
                if (critter.UnkBit_0X15_Bit7==1)
                {
                    dseg_67d6_B4 |= (1<<critter.UnkBits_0x16_0_F);
                    critter.UnkBit_0X15_Bit7 = 0;
                }
                if (critter.npc_goal == 1)
                {
                    SetGoalAndGtarg(critter, 8, 0);
                }
                else
                {
                    if (IsNPCActive_dseg_67d6_2234==1)
                    {
                        critter.UnkBit_0X13_Bit7 = 1;
                        critter.UnkBit_0X15_Bit6 = 1;
                        critter.npc_animation = 0;
                        critter.AnimationFrame = 0;
                        return;
                    }
                }
            }
            if (IsNPCActive_dseg_67d6_2234==0)
            {
                critter.Projectile_Speed = 1;
                if(critter.UnkBit_0X15_Bit7 == 1)
                {
                    //table look up into seg057_625F (possibly a path list)
                    var tmpSeg57_x = -1;
                    var tmpSeg57_y = -1;
                    var indexSeg57 = critter.UnkBits_0x16_0_F;  
                    if (tmpSeg57_x == critter.npc_xhome)
                    {
                        if (tmpSeg57_y == critter.npc_yhome)
                        {
                            SomethingWithSeg57(critter, indexSeg57);
                        }
                    }
                }
            }
            else
            {
                if (dseg_67d6_224E != 0)
                {//seg006_1413_31DC
                    if (HasCurrobjHeadingChanged_dseg_67d6_2242 == 0)
                    {
                        if (critter.UnkBit_0x18_6 == 0)
                        {
                            if (dseg_67d6_2269 !=0)
                            {
                                if (dseg_67d6_226F==0)
                                {
                                    //get collision object
                                    var siOtherItemId = collisionObject.item_id;
                                    if (collisionObject.majorclass == 1)
                                    {
                                        goto seg006_1413_3290;
                                    }
                                    if (collisionObject.item_id ==0x7F)
                                    {
                                        goto seg006_1413_3290;
                                    }
                                    if (critter.npc_goal != 5)
                                    {
                                        goto seg006_1413_3290;
                                    }
                                    if (collisionObject.npc_goal!=5)
                                    {
                                        goto seg006_1413_3290;
                                    }
                                    else
                                    {
                                        goto seg006_1413_32E5;
                                    }

                                    seg006_1413_3290:
                                    if (
                                        (collisionObject.OneF0Class == 0x14)
                                        &&
                                        (collisionObject.classindex>=8)
                                        )
                                    {
                                        if (critterObjectDat.isFlier(critter.item_id))
                                        {
                                            critter.Projectile_Pitch = 14;
                                            dseg_67d6_224E = 0;
                                            dseg_67d6_2246 = 1;
                                            MaybeIsFlier_Var6 = true;
                                            goto seg006_1413_32E5;
                                        }
                                        else
                                        {
                                            goto seg006_1413_32D5;
                                        }
                                    }
                                    else
                                    {
                                        goto seg006_1413_32D5;
                                    }

                                }
                                else
                                {
                                    critter.npc_animation = 0;
                                    critter.AnimationFrame = 0;
                                    if (Rng.r.Next(0,4)==0)
                                    {
                                        goto seg006_1413_32D5;
                                    }
                                    else
                                    {
                                        //seg006_1413:323b
                                        //push collision object
                                        NPCTryToOpenDoor(critter, null);
                                    }
                                }
                            }
                        goto seg006_1413_32E5;

                        seg006_1413_32D5:
                            critter.UnkBit_0x18_6 = 1;

                        seg006_1413_32E5:
                            if (dseg_67d6_224E !=0)
                            {
                                if (critter.UnkBit_0X15_Bit7 ==1)
                                {
                                    dseg_67d6_B4 |= (1<<critter.UnkBits_0x16_0_F);
                                    critter.UnkBit_0X15_Bit7 = 0;
                                }
                               critter.UnkBit_0x18_7 = 0;
                               Bit7Cleared_Var5 = true;
                            }
                        }
                    }
                }
                //seg006_1413_332C:
                //resume here

            }



        }

        static void SomethingWithSeg57(uwObject critter, int tableindex)
        {
            //placeholder
        }

        static void NPCTryToOpenDoor(uwObject critter, uwObject doorobject)
        {
            //playerholder
        }

        static void NPCWander(uwObject critter)
        {
            var newAnim_var2 = 1;
            int NewHeading = critter.ProjectileHeading;//not sure if it is initialised to this value. possibly vanilla behaviour was to use uninitialised memory in certain circumstances
            int PitchVarA;
            int si;
            if (critter.UnkBit_0X15_Bit7 == 1)
            {
                var tmp = 1 << critter.UnkBits_0x16_0_F;
                dseg_67d6_B4 |= tmp;
                critter.UnkBit_0X15_Bit7 = 0;                
            }

            var tileVar8 = UWTileMap.current_tilemap.Tiles[currObj_XHome,currObj_YHome];

            if (IsNPCActive_dseg_67d6_2234 != 0)
            {
                if (critter.npc_attitude == 0)
                {
                    if (Rng.r.Next(0,2)==1)
                    {
                        StandStillGoal(critter);
                        return;
                    }
                }
                //seg007_17A2_15D
                if (critterObjectDat.isFlier(critter.item_id))
                {
                    PitchVarA = 0xE;
                    si = 3;
                    if (currObj_Zpos<=0xE)
                    {
                        if (currObj_Zpos < tileVar8.floorHeight + 2)
                        {
                            PitchVarA = 0x10;
                        }
                        else
                        {
                            si = 5;
                        }
                        var tmp = Rng.r.Next(0,si) + PitchVarA;
                        tmp = tmp & 0x1F;
                        tmp = tmp << 3;
                        critter.Projectile_Pitch = (short)tmp;
                    }
                    //seg007_17A2_1C2://TODO include UW1 logic
                    if (_RES==GAME_UW2)
                    {                       
                        if (critter.npc_animation == 0)
                        {
                            if (critterObjectDat.unk_1F_lowernibble(critter.item_id) <= Rng.r.Next(0,0x10))
                            {
                                if (critter.AnimationFrame == MaxAnimFrame+1)
                                {
                                    newAnim_var2 = -1;
                                }
                            }
                            else
                            {
                                newAnim_var2 = -1;
                            }
                            //seg007_17A2_202:
                        }
                        else
                        {
                            if (critterObjectDat.unk_1F_lowernibble(critter.item_id) < Rng.r.Next(0,0x10))
                            {
                                if (critter.AnimationFrame == MaxAnimFrame+1)
                                {
                                    newAnim_var2 = 0;
                                }
                            }
                        }
                        //seg007_17A2_241: uw2
                        if (critter.npc_animation != newAnim_var2)
                        {
                            if (newAnim_var2!=-1)
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
                        if (critter.npc_animation == 32)
                        {
                            if (critterObjectDat.unk_1F_lowernibble(critter.item_id) > Rng.r.Next(0,0x10))
                            {
                                if (critter.AnimationFrame==3)
                                {
                                    critter.npc_animation = 44;
                                }
                            }
                        }
                        else
                        {
                            if (critterObjectDat.unk_1F_lowernibble(critter.item_id) < Rng.r.Next(0,0x10))
                            {
                                if (critter.AnimationFrame == 3)
                                {
                                    critter.npc_animation = 32;
                                }
                                else
                                {
                                    critter.npc_animation = 44;
                                }
                            }
                        }
                    }

                    //seg007_17A2_2A9: uw2
                    //seg007_1798_193: uw1
                    if 
                    (
                        (_RES==GAME_UW2 && critter.npc_animation == 1)
                        ||
                        (_RES!=GAME_UW2 && critter.npc_animation == 44)
                    )
                    {
                        if (dseg_67d6_224E != 0)
                        {
                            if (HasCurrobjHeadingChanged_dseg_67d6_2242 == 0)
                            {
                                //seg007_17A2_2D1
                                var deflection = Rng.r.Next(0,2);
                                deflection--;
                                deflection = deflection<<6;

                                var newheading = critter.ProjectileHeading;
                                newheading = (short)(newheading + deflection);
                                newheading = (short)(newheading + 256);
                                newheading = (short)(newheading % 256);
                                critter.ProjectileHeading = newheading;
                                critter.heading = (short)((newheading>>5) & 7);
                                critter.UnkBit_0X13_Bit0to6 = 0;
                                return;
                            }
                            //seg007_17A2_34C
                           
                            if (Rng.r.Next(0,0x40)>=critterObjectDat.unk_1F_lowernibble(critter.item_id) + 8)
                            {
                                var tmp = Rng.r.Next(0,0x40);
                                tmp = tmp + critter.ProjectileHeading + 224;
                                tmp = tmp % 0x100;
                                NewHeading = (short)tmp;
                            }
                            else
                            {
                                NewHeading = critter.ProjectileHeading;
                            }
                            
                            if (HasCurrobjHeadingChanged_dseg_67d6_2242==0)
                            {
                                NewHeading = VectorsToPlayer(NewHeading, 0xA);
                            }
                        }
                    }
                    else
                    {
                        //animation is not 1 in uw2 (probably 0)
                        //animation is not 44 in uw1 (probably 32)

                        if (critterObjectDat.unk_1F_lowernibble(critter.item_id)>Rng.r.Next(0,0x80))
                        {
                            var tmp = Rng.r.Next(0,0x40);
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
                    critter.ProjectileHeading = (short)NewHeading;
                    critter.heading = (short)((NewHeading >> 5) & 7);
                    critter.npc_heading = ((short)(NewHeading & 0x1F));
                    bool frameadvance = false;
                    if 
                    (
                        (_RES==GAME_UW2 && critter.npc_animation == 0)
                        ||
                        (_RES!=GAME_UW2 && critter.npc_animation == 32)
                    )
                    {
                        critter.UnkBit_0X15_Bit7 = 1;
                        critter.UnkBit_0X13_Bit0to6 = 0;
                        critter.Projectile_Speed = 6;
                        frameadvance = Rng.r.Next(0,2) == 0;
                    }
                    else
                    {
                        frameadvance = true;
                        critter.UnkBit_0X15_Bit6 = 0;
                        critter.UnkBit_0X15_Bit7 = 0;                        
                        critter.UnkBit_0X13_Bit0to6 = (short)critterObjectDat.unk_b_0_7(critter.item_id);
                        critter.Projectile_Speed = 4;

                    }
                    if (frameadvance)
                    {
                        if (_RES==GAME_UW2)
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
                    MaybeFaceGtarg(critter);
                }
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

        static void NPC_Goal8(uwObject critter)
        {
            //placeholder
        }

        static int GetCritterAnimationGlobalsForCurrObj(uwObject critter)
        {
            short CalcedFacing = CalculateFacingAngleToNPC(critter);
            string animname = CritterArt.GetAnimName(critter.npc_animation, CalcedFacing);
            var crit = CritterArt.GetCritter(critter.item_id & 0x3F);
            if (crit.Animations.ContainsKey(animname))
            {
                var anim = crit.Animations[animname];
                MaxAnimFrame = anim.maxNoOfFrames;
            }
            // else
            // {
            //     return;//no animation data.
            // }
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
            var playerXCoordinate = playerdat.xpos + (playerdat.playerObject.npc_xhome<<3);
            var playerYCoordinate = playerdat.ypos + (playerdat.playerObject.npc_yhome<<3);
            var xvector = playerXCoordinate - currentGTargXCoord;
            var yvector = playerYCoordinate - currentGTargYCoord;
            if ((xvector^2 + yvector^2) > (maybedist^2))
            {
                var HeadingVarB = ((pathfind.GetVectorHeading(xvector,yvector) + 4) % 8)<<5;
                HeadingVarB += 0x100;
                HeadingVarB -= headingArg0;
                var varD = HeadingVarB % 0x100;
                if ((varD>=0x40) || (varD<=0xC0))
                {
                    if (varD>=0x60)
                    {
                        if (varD>=0x80)
                        {
                            if (varD<=0xA0)
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
        /// Seems to turn the critter towards the player
        /// </summary>
        /// <param name="critter"></param>
        static void MaybeFaceGtarg(uwObject critter)
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
                    if ((currentGTargXVector^2 + currentGTargYVector^2) < 0x90)
                    {
                        var heading = pathfind.GetVectorHeading(currentGTargXVector, currentGTargYVector);
                        critter.UnkBit_0X13_Bit0to6 = 0;
                        critter.Projectile_Speed = 6;
                        if (_RES == GAME_UW2)
                        {
                            UpdateAnimation(critter,0,false);
                        }
                        else
                        {
                            critter.npc_animation = 32;
                            if (Rng.r.Next(0,2)==1)
                            {
                                critter.AnimationFrame = (byte)((critter.AnimationFrame + 1) & 3);
                            }
                        }
                        critter.heading = (short)heading;
                        critter.npc_heading = 0;
                    }
                }
        }
    } //end class
}//end namespace