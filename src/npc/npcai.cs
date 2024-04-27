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

        static int dseg_67d6_224E;
        static int IsNPCActive_dseg_67d6_2234;
        static int dseg_67d6_225E;
        static int HasCurrobjHeadingChanged_dseg_67d6_2242;
        static int dseg_67d6_2269;
        static int dseg_67d6_226F;
        static int dseg_67d6_2246;

        static int dseg_67d6_B4;

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
                    //todo
                    var ProjectileHeading = critter.ProjectileHeading;
                    //InitMotionParams()
                    //call    seg006_1413_9F5 
                    // CalculateMotion_seg006_1413_D6A
                    //ApplyProjectileMotion_seg030_2BB7_689
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
                    NpcBehaviours();
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
                            //SpecialDeathCases(1); //mode 1
                            //Drop npc loot (spawn if missing)
                            //Drop corpse
                            //remove from tile and free object
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
                                    NpcBehaviours();
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



        static void NpcBehaviours()
        {

        }
    } //end class
}//end namespace