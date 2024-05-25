using System.Diagnostics;


namespace Underworld
{
    /// <summary>
    /// Class for managing the ai processing loop for npcs
    /// </summary>
    public partial class npc : objectInstance
    {

        static int dseg_67d6_224E;
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
                int MaxAnimFrame;
                short CalcedFacing = CalculateFacingAngleToNPC(critter);
                string animname = CritterArt.GetAnimName(critter.npc_animation, CalcedFacing);
                var crit = CritterArt.GetCritter(critter.item_id & 0x3F);
                if (crit.Animations.ContainsKey(animname))
                {
                    var anim = crit.Animations[animname];
                    MaxAnimFrame = anim.maxNoOfFrames;
                }
                else
                {
                    return;//no animation data.
                }


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
                            dseg_67d6_B4 |= (1 << critter.MobileUnk_0x16_0_F);
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
                currObjXCoordinate = (currObj_XHome<<3) + critter.xpos;
                currObjYCoordinate = (currObj_YHome<<3) + critter.ypos;
                currObjQualityX = critter.quality;
                currObjOwnerY = critter.owner;
                currObjProjectileHeading = critter.ProjectileHeading;
                currObjTotalHeading = (critter.heading<<5) + critter.npc_heading;
                currObjUnk13 = critter.UnkBit_0X13_Bit0to6;
                currObjHeight = commonObjDat.height(critter.item_id);
                
                //finally process goals.
                if ((critter.npc_goal == 0xB) || (critter.npc_goal == 3))
                {
                    NpcBehaviours(critter);
                    if (critter.AnimationFrame>=MaxAnimFrame)
                    {
                        critter.AnimationFrame=0;
                    }
                    else
                    {
                        critter.AnimationFrame++;
                    }
                    n.SetAnimSprite(critter.npc_animation,critter.AnimationFrame, CalcedFacing);//temp keep anims running
                }
                else
                {
                    if (
                    ((critter.npc_animation==7) && (_RES==GAME_UW2))
                    ||
                    ((critter.npc_animation==0xC) && (_RES!=GAME_UW2))
                    )
                    {
                        //death animation
                        if (critter.AnimationFrame>=MaxAnimFrame)
                        {
                            //Special death cases
                            SpecialDeathCases(critter, 1); //mode 1
                            DropRemainsAndLoot(critter);
                            
                            //remove from tile and free object
                            ObjectCreator.DeleteObjectFromTile(critter.tileX, critter.tileY, critter.index, true);
                            return;
                        }
                        else
                        {
                            critter.AnimationFrame++;
                            n.SetAnimSprite(critter.npc_animation,critter.AnimationFrame, CalcedFacing);   
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
                                    if (critter.AnimationFrame ==0)
                                    {
                                        if (critter.npc_gtarg==1)
                                        {
                                             //TODO set music if not playing any combat theme
                                             //todo set combat music timer
                                        }                                       
                                    }
                                    if (critter.AnimationFrame==3)
                                    {
                                        //apply attack
                                        Debug.Print("NPC makes attack");
                                    }
                                    if (critter.AnimationFrame>=MaxAnimFrame)
                                    {
                                        critter.npc_animation = 2;
                                        critter.AnimationFrame = 0;
                                    }
                                    else
                                    {
                                        critter.AnimationFrame++;
                                    }
                                    n.SetAnimSprite(critter.npc_animation,critter.AnimationFrame, CalcedFacing);

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

                                    if (critter.AnimationFrame>=MaxAnimFrame)
                                    {
                                        critter.npc_animation = 2;
                                        critter.AnimationFrame = 0;
                                    }
                                    else
                                    {
                                        critter.AnimationFrame++;
                                    }
                                    n.SetAnimSprite(critter.npc_animation,critter.AnimationFrame, CalcedFacing);
                                    break;
                                }
                            default:
                                {                                    
                                    NpcBehaviours(critter);
                                    //temp
                                    if (critter.AnimationFrame>=MaxAnimFrame)
                                    {
                                        critter.AnimationFrame=0;
                                    }
                                    else
                                    {
                                        critter.AnimationFrame++;
                                    }
                                    n.SetAnimSprite(critter.npc_animation,critter.AnimationFrame, CalcedFacing);//temp keep anims running
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
                if ((critter.npc_animation == 1) && ((critter.AnimationFrame & 0x1) ==1))
                {
                    int soundeffect = -1;
                    switch(critterObjectDat.category(critter.item_id))
                    {
                        case 1://humanoids
                            {
                                if (critter.AnimationFrame==1)
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
                                if (critter.AnimationFrame==1)
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
                                if ((critter.AnimationFrame==1) || (critter.AnimationFrame == 3))
                                {
                                    soundeffect = 0x1D;//uw2 value?
                                }
                                break;
                            }
                    }
                    if (soundeffect!=-1)
                    {
                        //play sound effect at critter x,y coordinate
                    }
                }//end sound block
                //passisve test
                if (critterObjectDat.unkPassivenessProperty(critter.item_id)==false)
                {
                    if ( 
                        (
                            (critter.UnkBit_0x19_6_MaybeAlly == 0)
                            && 
                            (critter.index == playerdat.LastDamagedNPCIndex)
                            && 
                            (critterObjectDat.generaltype(critter.item_id) == playerdat.LastDamagedNPCType)
                            &&  
                            (critter.UnkBit_0XA_Bit7 == 0)
                        )
                        ||
                        (
                            critter.UnkBit_0x19_6_MaybeAlly == 1
                        )
                    )
                    {
                        if (playerdat.LastDamagedNPCTime + 512 < playerdat.game_time)
                        {
                            //do detection
                            var dist = System.Math.Abs(critter.tileX-playerdat.LastDamagedNPCTileX) + System.Math.Abs(critter.tileY-playerdat.LastDamagedNPCTileY);
                            if (dist < critterObjectDat.combatdetectionrange(critter.item_id))
                            {//npc has detected hostility to themselves or to an ally
                                critter.npc_attitude = 0;
                                critter.UnkBit_0x19_0_likelyincombat = 1;
                                if ((critter.npc_goal!=9) && (critter.npc_goal!=6))
                                {   
                                    var gtarg = 1;
                                    if (critter.UnkBit_0x19_6_MaybeAlly == 1)
                                    {//critter is allied with the player? set them to attack the players target
                                        gtarg = playerdat.LastDamagedNPCIndex;
                                    }
                                    SetGoalAndGtarg(critter, gtarg, 5);
                                    SetNPCTargetDestination(critter, playerdat.LastDamagedNPCTileX, playerdat.LastDamagedNPCTileY,playerdat.LastDamagedNPCZpos);
                                }
                            }
                        }
                    }
                    if (critter.ProjectileSourceID>0)//last hit
                    {
                        if 
                            (
                            (critter.ProjectileSourceID!=1)
                            ||
                            (
                                (critter.ProjectileSourceID==1) && (critter.UnkBit_0x19_6_MaybeAlly==1)
                            )
                            )
                        {
                            bool tmp=false;
                            if (critter.UnkBit_0x19_6_MaybeAlly==1)
                            {
                                tmp = true;
                            }
                            else
                            {
                                uwObject lasthitobject;
                                if (critter.ProjectileSourceID==1)
                                {
                                    lasthitobject = playerdat.playerObject;
                                }
                                else
                                {
                                    lasthitobject= UWTileMap.current_tilemap.LevelObjects[critter.ProjectileSourceID];
                                }
                                tmp = (lasthitobject.UnkBit_0x19_6_MaybeAlly==1);
                            }
                            if (tmp)
                            {
                                if(critter.npc_gtarg != critter.ProjectileSourceID)
                                {
                                    critter.npc_gtarg = (byte)critter.ProjectileSourceID;
                                }

                                if (GetDistancesToGTarg(critter))
                                {
                                    int newGoal;
                                    //int newGtarg  = critter.ProjectileSourceID;;
                                    gtargAvailable = true;
                                    if (critter.ProjectileSourceID==1)
                                    {//player has attacked
                                        critter.npc_attitude=0;
                                        SetNPCTargetDestination(critter,playerdat.tileX, playerdat.tileY, playerdat.zpos);
                                        critter.UnkBit_0x19_0_likelyincombat = 1;
                                    }

                                    //now eval distances to decide attack types
                                    if 
                                        (
                                        currentGTargSquaredDistanceByTiles<=2
                                        ||
                                            (
                                            currentGTargSquaredDistanceByTiles>2
                                            && critterObjectDat.isCaster(critter.item_id) 
                                            && (UWTileMap.current_tilemap.Tiles[critter.tileX, critter.tileY].noMagic==0)
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
            switch(critter.npc_goal)
            {
                case 0://standstill
                case 7:
                    break;
                case 1: //goto
                    break;
                    //and so on
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
            if (critter.npc_gtarg==1)
            {
                currentGoalTarget = playerdat.playerObject;
            }
            else
            {
                currentGoalTarget = UWTileMap.current_tilemap.LevelObjects[critter.npc_gtarg];
            }
            if (currentGoalTarget.npc_hp<=0)
            {
                return false;
            }  
            else
            {
                currentGTargXHome = currentGoalTarget.npc_xhome;
                currentGTargYHome = currentGoalTarget.npc_yhome;
                currentGTargHeight = currentGoalTarget.zpos>>3;
                currentGTargXCoord = currentGoalTarget.xpos + (currentGoalTarget.npc_xhome<<3);
                currentGTargYCoord = currentGoalTarget.ypos + (currentGoalTarget.npc_yhome<<3);
                currentGTargXVector = currentGTargXCoord - currObjXCoordinate;
                currentGTargYVector = currentGTargYCoord - currObjYCoordinate;
                zposofGTARG = currentGoalTarget.zpos>>3;
                currentGTargSquaredDistanceByTiles = (currentGTargXHome - currObj_XHome)^2 +  (currentGTargYHome - currObj_YHome)^2;
                currentGTargSquaredDistanceByCoordinates = (currentGTargXCoord - currObjXCoordinate)^2 +  (currentGTargYCoord - currObjYCoordinate)^2;
                return true;
            }             
        }

        static bool ShouldNPCWithdraw(uwObject critter)        
        {//, int maxhp, int currhp, int unk1C, int accumulateddmg)
            var maxhp = critterObjectDat.avghit(critter.item_id);
            var critter1C = critterObjectDat.maybemorale(critter.item_id);            
            
            if ((maxhp*3)>>2>= critter.npc_hp) //.75 of max hp
            {
                if (maxhp>>3 <=critter.npc_hp)//maxhp/8
                {
                    if ((maxhp>>1) < critter.AccumulatedDamage)//max/2
                    {
                        return true;
                    }
                    else
                    {
                        if (maxhp!=0)
                        {
                            var hpcalc = Rng.r.Next(0,4) + ((critter.npc_hp<<4)/maxhp);
                            var moralcalc = 15 - critter1C;
                            if (moralcalc>=hpcalc)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static void StandStillGoal(uwObject critter)
        {
            int gtarg_x=-1; int gtarg_y=-1;
            if (IsNPCActive_dseg_67d6_2234 != 0)
            {
                if (critter.npc_attitude==0)
                {
                    GetDistancesToGTarg(critter);
                    if (critter.UnkBit_0x19_0_likelyincombat==0)
                    {
                        if (critter.UnkBit_0x19_1 !=0)
                        {
                            if (Rng.r.Next(0,16)<=critterObjectDat.unk_1F(critter.item_id))
                            {
                                TurnTowardsTarget(0);
                            }
                            else
                            {
                                critter.UnkBit_0x19_1 = 0;
                            }
                        }

                        if (Rng.r.Next(0,16)<critterObjectDat.unk_1F(critter.item_id))
                        {
                            var result = SearchForGoalTarget(critter, ref gtarg_x, ref gtarg_y);
                            if (result == 0)
                            {
                                critter.UnkBit_0x19_0_likelyincombat = 1;
                                SetNPCTargetDestination(critter, gtarg_x, gtarg_y, zposofGTARG);
                                SetGoalAndGtarg(critter, 5,1);
                                return;
                            }
                            else
                            {
                                if (result == 2)
                                {
                                    critter.UnkBit_0x19_1 = 1;
                                    if (Rng.r.Next(0,2) == 1)
                                    {
                                        NPC_Goto(gtarg_x, gtarg_y, zposofGTARG);
                                        return;
                                    }
                                }
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
                                ChangeAnimation(critter, 0,0);
                                return;
                            default:
                                NPC_Goal8(critter);
                                return;
                        }
                    }
                    else
                    {
                        SetGoalAndGtarg(critter, 5, 1);
                        return;
                    }
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
            //placeholder
            xHomeFound=currentGTargXHome; yHomeFound=currentGTargYHome;

            var xvector = currentGTargXHome - critter.tileX;
            var yvector = currentGTargXHome - critter.tileY;

            var si_dist = xvector^2 + yvector^2;
            var tmp_critter = (critterObjectDat.combatdetectionrange(critter.item_id) * critterObjectDat.unk_1D_lowernibble(critter.item_id))/16;
            var tmp_gtarg = (critterObjectDat.combatdetectionrange(currentGoalTarget.item_id) * critterObjectDat.unk_1D_lowernibble(currentGoalTarget.item_id))/16;
            
            var score = (tmp_critter * tmp_gtarg)/4;

            if (score<si_dist)
            {
                tmp_critter = (critterObjectDat.theftdetectionrange(critter.item_id) * critterObjectDat.unk_1D_uppernibble(critter.item_id))/16;
                tmp_gtarg = (critterObjectDat.theftdetectionrange(currentGoalTarget.item_id) * critterObjectDat.unk_1D_uppernibble(currentGoalTarget.item_id))/16;
                var var8 = (tmp_critter * tmp_gtarg);
                if (si_dist<var8)
                {
                    var HeadingToTarget_var3 = pathfind.GetVectorHeading(xvector, yvector);
                    var var5 =  (8 + (HeadingToTarget_var3-critter.heading)) % 8;
                    if ((var5 == 0) || (var5 == 1) || (var5 == 7))
                    {
                        var result = pathfind.TestBetweenPoints(
                            currObjXCoordinate,currObjYCoordinate, critter.zpos+ commonObjDat.height(critter.item_id),
                            currentGTargXCoord,currentGTargYCoord, currentGoalTarget.zpos+ commonObjDat.height(currentGoalTarget.item_id)
                            );     
                        if (result)
                        {
                            critter.UnkBit_0x19_0_likelyincombat = 1;
                            return 0;
                        }                   
                    }
                }

                if ((score<<2) < si_dist)
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

        static void TurnTowardsTarget(int arg0)
        {
//placeholder
        }
        static void NPC_Goto(int targetX, int targetY, int targetZ)
        {
//placeholder
        }
        
        static void NPCWander(uwObject critter)
        {
//placeholder
        }

        static void ChangeAnimation(uwObject critter, int arg0, int arg2)
        {
            //placeholder
        }

        static void NPC_Goal8(uwObject critter)
        {
            //placeholder
        }
    } //end class
}//end namespace