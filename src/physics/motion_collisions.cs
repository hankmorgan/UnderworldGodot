using System;
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// code and data relating to collisions
    /// </summary>
    public partial class motion : Loader
    {
        public static CollisionRecord[] collisionTable = new CollisionRecord[32];

        /// <summary>
        /// Does/calls applicable collisions
        /// </summary>
        public static void DoCollision_seg031_2CFA_D1F(UWMotionParamArray MotionParams)
        {
            UWMotionParamArray.dseg_67d6_26A4 = 0;
            var projectile = UWTileMap.current_tilemap.LevelObjects[MotionCalcArray.MotionArrayObjectIndexA_base];
            int soundeffect;

            var si_mass = commonObjDat.mass(projectile.item_id);
            if (UWMotionParamArray.Gravity_Related_dseg_67d6_41F <= 4)
            {//UWMotionParamArray.Gravity_Related_dseg_67d6_41F
                MotionParams.speed_12 = 0;
            }
            else
            {//2CFA:0D5B
                var var2 = Math.Abs(MotionCalcArray.z4 - UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419);
                var2 = var2 * UWMotionParamArray.dseg_67d6_412;

                var2 = (var2 << 4) / (UWMotionParamArray.Gravity_Related_dseg_67d6_41F / 4);
                MotionParams.speed_12 -= (byte)var2;
            }

            //seg031_2CFA_D9A:
            MotionCalcArray.z4 = UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419;
            UWMotionParamArray.RelatedToMotionZ_dseg_67d6_402 = 0;


            if (
                (UWMotionParamArray.ACollisionIndex_dseg_67d6_416 == -1)
                &&
                ((MotionCalcArray.UnkC_terrain_base & 0x3) == 1)
                &&
                (MotionCalcArray.Radius8_base + MotionCalcArray.Unk10_base >= MotionCalcArray.z4)
                &&
                (MotionParams.unk_a_pitch <= 0)
                )
            {//seg031_2CFA_DDE: 
                ZeroiseMotionValues_seg031_2CFA_7BF(MotionParams);
                MotionParams.tilestate25 = 2;

                if (si_mass >= 0x14)
                {
                    if (si_mass >= 0x64)
                    {
                        soundeffect = 5;
                    }
                    else
                    {
                        soundeffect = 0x19;
                    }
                }
                else
                {
                    soundeffect = 0x14;
                }
                Debug.Print($"play sound effect {soundeffect} at {MotionParams.x_0 >> 5} {MotionParams.y_2 >> 5}");
            }
            else
            {//seg031_2CFA_E28:
                soundeffect = (Math.Abs(MotionParams.unk_a_pitch) / 0xA) + ((si_mass - 600) / 32) - 40;
                Debug.Print($"play sound effect {soundeffect} at {MotionParams.x_0 >> 5} {MotionParams.y_2 >> 5}");

                var di_collisionresult = CollideObjects_seg030_2BB7_1CE(MotionParams, UWMotionParamArray.ACollisionIndex_dseg_67d6_416, MotionCalcArray.MotionArrayObjectIndexA_base);

                //resume here.
                if ((di_collisionresult & 0x18) != 0)
                {
                    //seg031_2CFA_E87:
                    if ((di_collisionresult & 0x10) == 0)
                    {//seg031_2CFA_E94: 
                        UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414 = (short)(UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E + 1);
                    }
                    else
                    {
                        //seg031_2CFA_E8E:
                        ZeroiseMotionValues_seg031_2CFA_7BF(MotionParams);
                    }
                }
                else
                {//seg031_2CFA_E9E:
                    if ((di_collisionresult & 0x4) == 0)
                    {
                        //seg031_2CFA_10FB: 
                        if (UWMotionParamArray.ACollisionIndex_dseg_67d6_416 != -1)
                        {
                            var Dst = UWMotionParamArray.ACollisionIndex_dseg_67d6_416;
                            MotionCalcArray.Unk14_collisoncount_base--;
                            for (int i = 0; i < 6; i++)
                            {
                                CollisionRecord.Collisions_dseg_2520[(Dst * 6) + i] = CollisionRecord.Collisions_dseg_2520[(MotionCalcArray.Unk14_collisoncount_base * 6) + i];
                            }
                        }
                    }
                    else
                    {
                        int var3;
                        int var6;
                        if (MotionParams.unk_a_pitch > 0)
                        {
                            var3 = 0;
                        }
                        else
                        {
                            var3 = 1;
                        }

                        var6 = MotionParams.unk_14;

                        if (
                            (MotionParams.index_20 == 1) && ((playerdat.MagicalMotionAbilities & 0x20) == 0x20)
                            ||
                            (MotionParams.unk_16 == 0xF)
                        )
                        {
                            //Bouncing_seg031_2CFA_EE2:
                            MotionParams.unk_a_pitch = (short)-MotionParams.unk_a_pitch;
                        }
                        else
                        {
                            //seg031_2CFA_EF0:
                            MotionParams.unk_a_pitch = (short)-(MotionParams.unk_a_pitch / 0xF);
                            MotionParams.unk_26 = (short)Math.Abs((0xF - MotionParams.unk_16) * MotionParams.unk_a_pitch);
                            MotionParams.unk_a_pitch = (short)(MotionParams.unk_a_pitch * MotionParams.unk_16);

                            if (MotionParams.unk_16 == 0)
                            {
                                MotionParams.unk_14 = 0;
                            }
                            else
                            {
                                MotionParams.unk_14 -= (short)(MotionParams.unk_14 * (0xF - MotionParams.unk_16) / 0x1E);
                            }
                        }

                        //seg031_2CFA_F63:
                        if (
                            (MotionParams.index_20 != 1)
                            &&
                            ((UWMotionParamArray.LikelyIsMagicProjectile_dseg_67d6_26B8 & 0x1000) == 0x1000)
                        )
                        {
                            //seg031_2CFA_F7B:
                            if (MotionParams.speed_12 > 1)
                            {
                                MotionParams.speed_12--;
                            }
                        }
                        else
                        {
                            //seg031_2CFA_F8E
                            if (var3 != 0)
                            {
                                //seg031_2CFA_FA5:
                                if (MotionParams.unk_a_pitch < 0x8D)
                                {
                                    MotionParams.unk_a_pitch = 0;
                                    MotionParams.unk_10 = 0;
                                    if (UWMotionParamArray.ACollisionIndex_dseg_67d6_416 == -1)
                                    {
                                        //seg031_2CFA_102A
                                        if (MotionCalcArray.Unk10_base + MotionCalcArray.Radius8_base < MotionCalcArray.z4)
                                        {
                                            //seg031_2CFA_1054:
                                            var ProjectileObject = UWTileMap.current_tilemap.LevelObjects[MotionParams.index_20];
                                            if (ProjectileObject.majorclass == 1)
                                            {
                                                //NPC
                                                //seg031_2CFA_107A: 
                                                if ((MotionCalcArray.UnkE_base & 0x10) == 0)
                                                {
                                                    //seg031_2CFA_108C
                                                    if ((MotionCalcArray.UnkE_base & 0x20) == 0)
                                                    {//seg031_2CFA_109E:
                                                        if ((MotionCalcArray.UnkE_base & 0x40) == 0)
                                                        {
                                                            MotionParams.tilestate25 = 1;
                                                        }
                                                        else
                                                        {
                                                            //seg031_2CFA_10AA:
                                                            MotionParams.tilestate25 = 8;
                                                        }
                                                    }
                                                    else
                                                    {//seg031_2CFA_1098
                                                        MotionParams.tilestate25 = 4;
                                                    }
                                                }
                                                else
                                                {
                                                    MotionParams.tilestate25 = 2;
                                                }
                                            }
                                            else
                                            {
                                                //Not an npc
                                                //seg031_2CFA_1075:
                                                MaybeReflection_seg031_2CFA_CC6(MotionParams);
                                            }
                                        }
                                        else
                                        {
                                            //seg031_2CFA_1040:
                                            MotionParams.tilestate25 = (byte)(1 << (MotionCalcArray.UnkC_terrain_base & 3));
                                        }
                                    }
                                    else
                                    {
                                        //seg031_2CFA_FBD:
                                        var collision = collisionTable[UWMotionParamArray.ACollisionIndex_dseg_67d6_416];
                                        var CollisionObject = UWTileMap.current_tilemap.LevelObjects[collision.link];
                                        if (commonObjDat.UnknownFlag3_1(CollisionObject.item_id))
                                        {
                                            //seg031_2CFA_FFD:
                                            MotionParams.tilestate25 = 1;
                                        }
                                        else
                                        {
                                            //seg031_2CFA_1000:
                                            if (CollisionObject.majorclass == 1)
                                            {
                                                MotionParams.tilestate25 = 1;
                                            }
                                            else
                                            {
                                                MaybeReflection_seg031_2CFA_CC6(MotionParams);
                                            }
                                        }
                                    }
                                    //seg031_2CFA_10B8:
                                    if (UWMotionParamArray.dseg_67d6_26A4 == 0)
                                    {
                                        MotionParams.speed_12 = 0;
                                    }
                                }
                            }
                        }

                        //seg031_2CFA_10CA:

                        if (
                            (((MotionCalcArray.UnkC_terrain_base & 3) == 3) && ((MotionCalcArray.UnkC_terrain_base & 0xC & 4) != 4))
                            ||
                            (((MotionCalcArray.UnkC_terrain_base & 3) == 3) && ((MotionCalcArray.UnkC_terrain_base & 0xC & 4) == 4) && ((MotionCalcArray.UnkE_base & 0x40) != 0) && ((MotionCalcArray.UnkE_base & 0x800) == 0x800))
                            ||
                            (((MotionCalcArray.UnkC_terrain_base & 3) != 3) && ((MotionCalcArray.UnkE_base & 0x40) != 0) && ((MotionCalcArray.UnkE_base & 0x800) == 0x800))
                            )
                        {
                            MotionParams.unk_14 = (short)var6;
                        }
                    }

                    //seg031_2CFA_112C:
                    seg031_2CFA_78A(MotionParams, 0);


                }
            }
        }


        static void SetCollisionTarget_seg031_2CFA_10E(UWMotionParamArray MotionParams, int arg0)
        {
            UWMotionParamArray.dseg_67d6_26A8 = 1;
            int var1 = 0;
            SortCollisions_seg028_2941_ED8();

            UWMotionParamArray.ACollisionIndex_dseg_67d6_416 = -1;
            UWMotionParamArray.dseg_67d6_41D = 0x7f;

            if (MotionParams.unk_a_pitch > 0)
            {
                //seg031_2CFA_136:
                if (
                    (MotionCalcArray.Unk14_collisoncount_base > 0)
                    &&
                    (MotionCalcArray.Unk16_base >= 0)
                    &&
                    (MotionCalcArray.Unk16_base + MotionCalcArray.Unk15_base < MotionCalcArray.Unk14_collisoncount_base)
                )
                {
                    //seg031_2CFA_15A:
                    UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 = (ushort)(collisionTable[MotionCalcArray.Unk15_base + MotionCalcArray.Unk16_base].zpos - MotionParams.height_23);
                    UWMotionParamArray.ACollisionIndex_dseg_67d6_416 = (sbyte)(MotionCalcArray.Unk15_base + MotionCalcArray.Unk16_base);
                }
                else
                {
                    //seg031_2CFA_18D:
                    UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 = (ushort)(0x80 - MotionParams.height_23);
                }

                //seg031_2CFA_19F:
                UWMotionParamArray.dseg_67d6_26A8 = 0;
                //Jump when (z4 + radius)<calc11
                if (MotionCalcArray.z4 + MotionParams.radius_22 < MotionCalcArray.Unk11_base)
                {//seg031_2CFA_1C3
                    UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 = MotionCalcArray.Unk11_base;
                    UWMotionParamArray.dseg_67d6_26A8 = 1;
                }
            }
            else
            {
                //seg031_2CFA_133
                if (MotionParams.unk_a_pitch == 0)
                {//seg031_2CFA_1E0

                    UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 = MotionCalcArray.Unk10_base;

                    //Jump when z4+param[24]<Calc11
                    if (MotionCalcArray.z4 + MotionParams.unk_24 < MotionCalcArray.Unk11_base)
                    {
                        //seg031_2CFA_209:
                        UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 = MotionCalcArray.Unk10_base;
                    }
                    else
                    {
                        //seg031_2CFA_204:
                        UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 = MotionCalcArray.Unk11_base;
                    }

                    //seg031_2CFA_20C:

                    //when calc15 == 0
                    //AND calc16>0
                    //AND calc16<=calc14
                    if (
                        (MotionCalcArray.Unk15_base == 0)
                        &&
                        (MotionCalcArray.Unk16_base > 0)
                        &&
                        (MotionCalcArray.Unk16_base <= MotionCalcArray.Unk14_collisoncount_base)
                    )
                    {
                        var1 = 1;
                    }
                    else
                    {
                        var1 = 0;
                    }

                    var CollisionIndex_var_4 = 0;
                    while (MotionCalcArray.Unk14_collisoncount_base > CollisionIndex_var_4)//oob error
                    {
                        //seg031_2CFA_23A
                        var collision_Var4 = collisionTable[CollisionIndex_var_4];
                        var obj = UWTileMap.current_tilemap.LevelObjects[collision_Var4.link];
                        if (commonObjDat.Unk6_0(obj.item_id))
                        {
                            if (MotionCalcArray.Unk16_base <= CollisionIndex_var_4)
                            {//seg031_2CFA_281:
                                if (collision_Var4.zpos - 1 < UWMotionParamArray.dseg_67d6_41D)
                                {
                                    UWMotionParamArray.dseg_67d6_41D = collision_Var4.zpos - 1;
                                }
                                //seg031_2CFA_2AC
                                // when calc15+calc16>var4
                                if (MotionCalcArray.Unk15 + MotionCalcArray.Unk16_base > CollisionIndex_var_4)
                                {
                                    //seg031_2CFA_2C0:
                                    if (collision_Var4.height > UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419)
                                    {
                                        //collisionrecordheight<(0x80-paramheight)
                                        if (collision_Var4.height < (0x80 - MotionParams.height_23))
                                        {
                                            UWMotionParamArray.ACollisionIndex_dseg_67d6_416 = (sbyte)CollisionIndex_var_4;
                                            UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 = collision_Var4.height;
                                        }
                                    }
                                }
                            }
                            else
                            {//seg031_2CFA_319
                                if (var1 != 0)
                                {
                                    if (collision_Var4.height < UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419)
                                    {
                                        if (UWMotionParamArray.ACollisionIndex_dseg_67d6_416 == -1)
                                        {
                                            UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 = collision_Var4.height;
                                            UWMotionParamArray.ACollisionIndex_dseg_67d6_416 = (sbyte)CollisionIndex_var_4;
                                        }
                                        else
                                        {//seg031_2CFA_340:
                                            var currentcollision = collisionTable[UWMotionParamArray.ACollisionIndex_dseg_67d6_416];
                                            if ((currentcollision.quality & 0x10) == 0)
                                            {
                                                UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 = collision_Var4.height;
                                                UWMotionParamArray.ACollisionIndex_dseg_67d6_416 = (sbyte)CollisionIndex_var_4;
                                            }
                                            else
                                            {
                                                //seg031_2CFA_353
                                                if (collision_Var4.quality == 0)
                                                {
                                                    UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 = collision_Var4.height;
                                                    UWMotionParamArray.ACollisionIndex_dseg_67d6_416 = (sbyte)CollisionIndex_var_4;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        CollisionIndex_var_4++;
                    }


                }
                else
                {
                    //seg031_2CFA_1DD
                    UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 = MotionCalcArray.Unk11_base;
                    if (MotionCalcArray.Unk16_base > 0)
                    {
                        if (MotionCalcArray.Unk16_base < MotionCalcArray.Unk14_collisoncount_base)
                        {
                            //when colision[calc16-1]>Global419
                            if (collisionTable[MotionCalcArray.Unk16_base - 1].height > UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419)
                            {
                                //seg031_2CFA_3C5
                                UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 = collisionTable[MotionCalcArray.Unk16_base - 1].height;

                                UWMotionParamArray.ACollisionIndex_dseg_67d6_416 = (sbyte)(MotionCalcArray.Unk16_base - 1);
                            }
                        }
                    }
                    UWMotionParamArray.dseg_67d6_26A8 = 0;
                }
            }

            //return path
            //seg031_2CFA_3E6
            if (UWMotionParamArray.ACollisionIndex_dseg_67d6_416 != -1)
            {
                var obj = UWTileMap.current_tilemap.LevelObjects[collisionTable[UWMotionParamArray.ACollisionIndex_dseg_67d6_416].link];
                UWMotionParamArray.CollisionItemID_dseg_67d6_417 = obj.item_id;
            }
        }


        /// <summary>
        /// Sorts the collision table by height and zpos.
        /// </summary>
        public static void SortCollisions_seg028_2941_ED8()
        {
            int var1;
            var HeightIsZero_Var4 = 0;
            if (MotionCalcArray.Height9 == 0)
            {//seg028_2941_EEC
                HeightIsZero_Var4 = 1;
            }

            var var3 = 0;


            //seg028_2941_F58:
            while (MotionCalcArray.Unk14_collisoncount > var3)
            {
                var1 = MotionCalcArray.Unk14_collisoncount - 2;
                while (var1 >= var3)
                {
                    if (collisionTable[var1].height > collisionTable[var1 + 1].height)
                    {
                        SwapCollisionRecord(var1);
                    }
                    var1--;
                }
                var1 = var3;
                if (collisionTable[var1].height > MotionCalcArray.z4)
                {
                    var3++;
                }
                else
                {
                    break;
                }
            }

            var1 = var3;

            //seg028_2941_FB4:
            while (true)
            {
                if (MotionCalcArray.Unk14_collisoncount <= var1)
                {//seg028_2941_FC0
                    MotionCalcArray.Unk16 = (byte)var3;
                    MotionCalcArray.Unk15 = 0;
                    while (MotionCalcArray.Unk15 < MotionCalcArray.Unk14_collisoncount)
                    {
                        if ((MotionCalcArray.z4 + MotionCalcArray.Height9 + HeightIsZero_Var4) > collisionTable[MotionCalcArray.Unk15].zpos)
                        {
                            MotionCalcArray.Unk15++;
                        }
                        else
                        {
                            return;
                        }
                    }
                    return;
                }
                else
                {//seg028_2941_F6C:

                    var var2 = MotionCalcArray.Unk14_collisoncount - 2;

                    while (var2 >= var1)
                    {
                        if (collisionTable[var2].zpos > collisionTable[var2 + 1].zpos)
                        {
                            SwapCollisionRecord(var2);
                        }
                        var2--;
                    }
                }
                //seg028_2941_FB1
                var1++;
            }
        }



        /// <summary>
        /// Swaps collision table record at index with the record at index+1
        /// </summary>
        /// <param name="index"></param>
        static void SwapCollisionRecord(int index)
        {
            byte[] copy = new byte[6];
            for (int i = 0; i < 6; i++)
            {//backup
                copy[i] = CollisionRecord.Collisions_dseg_2520[(index * 6) + i];
            }

            for (int i = 0; i < 6; i++)
            {//replace
                CollisionRecord.Collisions_dseg_2520[(index * 6) + i] = CollisionRecord.Collisions_dseg_2520[((index + 1) * 6) + i];
            }

            for (int i = 0; i < 6; i++)
            {//restore
                CollisionRecord.Collisions_dseg_2520[((index + 1) * 6) + i] = copy[i];
            }
        }

        /// <summary>
        /// Looks like this adds the object as a possible candidate for collision checking.
        /// </summary>
        /// <param name="MotionParams"></param>
        /// <param name="CollidingObject"></param>
        /// <param name="ListHead"></param>
        /// <param name="xArg6"></param>
        /// <param name="yArg8"></param>
        /// <param name="isNPCArgA"></param>
        static void CreateCollisonRecord_Seg028_2941_A78(uwObject CollidingObject, long ListHead, int xArg6, int yArg8, bool isNPCArgA)
        {
            if (MotionCalcArray.Unk14_collisoncount <= 8)
            {
                int XCoordVar1;
                int YCoordVar2;
                int XVar3;
                int YVar4;
                int cx_radius;
                if (commonObjDat.radius(CollidingObject.item_id) == 4)
                {
                    XCoordVar1 = xArg6 << 3;
                    XVar3 = 7 + xArg6 << 3;

                    YCoordVar2 = yArg8 << 3;
                    YVar4 = 7 + yArg8 << 3;
                }
                else
                {
                    XCoordVar1 = CollidingObject.xpos + (xArg6 << 3);
                    YCoordVar2 = CollidingObject.ypos + (yArg8 << 3);
                    cx_radius = commonObjDat.radius(CollidingObject.item_id);
                    if ((CollidingObject.majorclass == 1) && (cx_radius > 0) && (isNPCArgA))
                    {
                        cx_radius--;
                    }
                    XVar3 = XCoordVar1 + cx_radius;
                    YVar4 = YCoordVar2 + cx_radius;
                    XCoordVar1 -= cx_radius;
                    YCoordVar2 -= cx_radius;
                    if (
                        (XVar3 >= UWMotionParamArray.XposMinusRad)
                        &&
                        (XCoordVar1 <= UWMotionParamArray.XposPlusRad)
                        &&
                        (YVar4 >= UWMotionParamArray.YposMinusRad)
                        &&
                        (YCoordVar2 <= UWMotionParamArray.YposMinusRad)
                    )
                    {
                        //seg028_2941_B73
                        MotionCalcArray.Unk14_collisoncount++;
                        var si_ptr = MotionCalcArray.Unk14_collisoncount;// * 6;
                        collisionTable[si_ptr].zpos = (byte)CollidingObject.zpos;
                        //DataLoader.setAt(motion.Collisions_dseg_2520, si_ptr + 1, 8, CollidingObject.zpos);
                        collisionTable[si_ptr].height = (byte)commonObjDat.height(CollidingObject.item_id);
                        //DataLoader.setAt(motion.Collisions_dseg_2520, si_ptr, 8, commonObjDat.height(CollidingObject.item_id));
                        if (commonObjDat.height(CollidingObject.item_id) == 0)
                        {
                            //motion.Collisions_dseg_2520[0]++;
                            collisionTable[si_ptr].height++;
                        }

                        //seg028_2941_BB3:
                        collisionTable[si_ptr].link = (short)ListHead;
                        // var temp = DataLoader.getAt(motion.Collisions_dseg_2520, si_ptr + 2, 16);
                        // temp = temp & 0x3F;
                        // temp = (uint)(temp | (ListHead << 6));

                        collisionTable[si_ptr].quality = 9;
                        // temp = temp & 0xC0;
                        // temp = temp | 9;
                        //DataLoader.setAt(motion.Collisions_dseg_2520, si_ptr + 2, 16, (int)temp);

                        //seg028_2941_BCF
                        if (UWMotionParamArray.xpos_dseg_67d6_2585 >= XCoordVar1)
                        {
                            if (UWMotionParamArray.xpos_dseg_67d6_2585 <= XVar3)
                            {
                                if (UWMotionParamArray.ypos_dseg_67d6_251C >= YCoordVar2)
                                {
                                    if (UWMotionParamArray.ypos_dseg_67d6_251C <= YVar4)
                                    {
                                        collisionTable[si_ptr].quality |= 0x10;
                                        // var tmp = DataLoader.getAt(motion.Collisions_dseg_2520, si_ptr + 2, 16);
                                        // var tochange = tmp & 0x3F;
                                        // tochange = tochange | 0x10;
                                        // tmp = tmp & 0xC0;
                                        // tmp = tmp | tochange;
                                        // DataLoader.setAt(motion.Collisions_dseg_2520, si_ptr + 2, 16, (int)tmp);
                                    }
                                }
                            }
                        }

                        //seg028_2941_BF9:
                        collisionTable[si_ptr].unkxyvalue = (short)(xArg6 + (yArg8 << 6));
                        //DataLoader.setAt(motion.Collisions_dseg_2520, si_ptr + 4, 16, xArg6 + (yArg8 << 6));
                    }
                }
            }
        }

        /// <summary>
        /// Looks like this checks the tile for possible objects to collide with.
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="arg2"></param>
        static void ScanForCollisions_seg028_2941_C0E(int arg0, int arg2)
        {
            var isNPC_var14 = false;
            var tile_var4 = UWTileMap.current_tilemap.Tiles[MotionCalcArray.x0 >> 3, MotionCalcArray.y2 >> 3];
            if (MotionCalcArray.MotionArrayObjectIndexA != 0)
            {
                var obj = UWTileMap.current_tilemap.LevelObjects[MotionCalcArray.MotionArrayObjectIndexA];
                if (obj.majorclass == 1)
                {
                    isNPC_var14 = true;
                }
            }

            MotionCalcArray.Unk14_collisoncount = 0;
            UWMotionParamArray.xpos_dseg_67d6_2585 = MotionCalcArray.x0 & 0x7;
            UWMotionParamArray.ypos_dseg_67d6_251C = MotionCalcArray.y2 & 0x7;

            int Radius_var13 = 0;
            if ((arg0 != 0) && (MotionCalcArray.Height9 == 0))
            {
                Radius_var13 = 0;
                if ((isNPC_var14) && (Radius_var13 > 0))
                {
                    Radius_var13--;
                }
            }
            else
            {
                Radius_var13 = MotionCalcArray.Radius8;
            }

            //seg028_2941_CAF
            UWMotionParamArray.XposPlusRad = UWMotionParamArray.xpos_dseg_67d6_2585 + Radius_var13;
            UWMotionParamArray.YposPlusRad = UWMotionParamArray.ypos_dseg_67d6_251C + Radius_var13;
            UWMotionParamArray.XposMinusRad = UWMotionParamArray.xpos_dseg_67d6_2585 - Radius_var13;
            UWMotionParamArray.YposMinusRad = UWMotionParamArray.ypos_dseg_67d6_251C - Radius_var13;

            //seg028_2941_CD3:
            var varD = (UWMotionParamArray.XposMinusRad - 11) / 8;
            var varE = (UWMotionParamArray.YposMinusRad - 11) / 8;

            //seg028_2941_CF0:
            var varF = (UWMotionParamArray.XposPlusRad + 4) / 8;
            var var10 = (UWMotionParamArray.YposPlusRad + 4) / 8;

            var var11_maybeX = varD;

            //seg028_2941_E64:
            while (var11_maybeX <= varF)
            {
                //seg028_2941_D13:
                var var12_maybeY = varE;
                while (var12_maybeY <= var10)
                {
                    //seg028_2941_D1C:
                    var si = 0;

                    var di = var11_maybeX + (var12_maybeY << 6);
                    var tilePtr = (int)(tile_var4.Ptr + (di<<2));
                    var tileAtPTR = UWTileMap.GetTileByPTR(tilePtr);
                    int next = tileAtPTR.indexObjectList;
                    //seg028_2941_E36:
                    //loop the object list on the tile.
                    var indexByte = tileAtPTR.indexObjectList;
                    while (next != 0 && si <= 0x40)
                    {
                        var obj = UWTileMap.current_tilemap.LevelObjects[next];
                        if (next != MotionCalcArray.MotionArrayObjectIndexA)
                        {//maybe check for colliding.
                            if (
                                (!isNPC_var14)
                                ||
                                (isNPC_var14 && (commonObjDat.UnknownFlag3_2(obj.item_id) == false))
                            )
                            {
                                //seg028_2941_DA1
                                if (
                                    (commonObjDat.height(obj.item_id) != 0)
                                    ||
                                    (commonObjDat.height(obj.item_id) == 0) && (!obj.IsStatic)
                                )
                                {
                                    //seg028_2941_DB7:
                                    if (
                                        (obj.IsStatic)
                                        ||
                                        ((!obj.IsStatic) && (obj.majorclass != 1))
                                        ||
                                        ((!obj.IsStatic) && (obj.majorclass == 1) && (obj.UnkBit_0X15_Bit7 == 0))
                                        )
                                    {
                                        if (
                                            (arg2 == 0)
                                            ||
                                            ((arg2 != 0) && commonObjDat.ActivatedByCollision(obj.item_id))
                                            )
                                        {
                                            CreateCollisonRecord_Seg028_2941_A78( obj, indexByte, var11_maybeX, var12_maybeY, isNPC_var14);
                                        }
                                    }
                                }
                            }
                        }
                        indexByte = obj.index;//seg028_2941_E1E
                        next = obj.next;
                        si++;
                    }
                    if (si > 0x40)
                    {
                        return;
                    }
                    else
                    {
                        var12_maybeY++;
                    }
                }
                //seg028_2941_E61:
                var11_maybeX++;
            }
        }




        static int CollideObjects_seg030_2BB7_1CE(UWMotionParamArray MotionParams, int si_CollisionIndex_Arg0, int MotionObjectArg2)
        {
            uwObject CollidedObject_VarA;
            bool varB;
            int var2_collideditemid;
            if (si_CollisionIndex_Arg0 != -1)
            {
                //seg030_2BB7_1E0:
                var Collision = collisionTable[si_CollisionIndex_Arg0];
                if (((Collision.quality & 0x20) != 0))
                {
                    return 2;
                }
                //seg030_2BB7_1F6:
                Collision.quality |= 0x20;//make sure it collides only once?
            }

            //seg030_2BB7_220:
            UWMotionParamArray.dseg_67d6_25BF_X = MotionCalcArray.x0_base >> 3;
            UWMotionParamArray.dseg_67d6_25C0_Y = MotionCalcArray.y2_base >> 3;

            var MotionObject = UWTileMap.current_tilemap.LevelObjects[MotionObjectArg2];
            var diMotionObject_itemid = MotionObject.item_id;

            if (si_CollisionIndex_Arg0 == -1)
            {//seg030_2BB7_252:
                CollidedObject_VarA = null;
                varB = true;
                var2_collideditemid = -1;
            }
            else
            {
                //seg030_2BB7_268:
                var Collision = collisionTable[si_CollisionIndex_Arg0];
                CollidedObject_VarA = UWTileMap.current_tilemap.LevelObjects[Collision.link];
                var temp = Collision.unkxyvalue & 0x3F;

                UWMotionParamArray.UnknownX_dseg_67d6_25BD = (temp + UWMotionParamArray.dseg_67d6_25BF_X) & 0x3F;
                temp = UWMotionParamArray.UnknownX_dseg_67d6_25BD - UWMotionParamArray.dseg_67d6_25BF_X;

                //seg030_2BB7_2B4
                UWMotionParamArray.UnknownY_dseg_67d6_25BE = (UWMotionParamArray.dseg_67d6_25C0_Y + ((Collision.unkxyvalue - temp) / 0x40)) & 0x3F;

                var2_collideditemid = CollidedObject_VarA.item_id;
                varB = commonObjDat.Unk6_0(var2_collideditemid);

                if (!MotionObject.IsStatic)
                {
                    //mobile
                    //seg030_2BB7_30C:
                    if ((MotionObject.majorclass != 1)
                        && (MotionObject.UnkBit_0X15_Bit7 != 0)
                        && (diMotionObject_itemid != 0x1D)
                        && (diMotionObject_itemid != 0x13F))
                    {
                        return 2;
                    }
                    else
                    {
                        //an npc, or bit is zero, or  a fireball or a npc
                        //seg030_2BB7_33E:
                        if (MotionObject.majorclass != 1)
                        {
                            //seg030_2BB7_348:
                            MotionObject.UnkBit_0X15_Bit7 = 1;
                        }
                    }
                }
            }

            //seg030_2BB7_357
            if (var2_collideditemid != -1)
            {
                if (commonObjDat.ActivatedByCollision(var2_collideditemid))
                {
                    //seg030_2BB7_374
                    Debug.Print("Use activated by collision!");
                    UWMotionParamArray.dseg_67d6_25BC = 0;
                    use.Use(
                        index: CollidedObject_VarA.index,
                        objList: UWTileMap.current_tilemap.LevelObjects,
                        WorldObject: true);//this line will probably break a lot until I make use a more vanilla compliant Use() function.
                }
                else
                {
                    //seg030_2BB7_3A1
                    if (CollidedObject_VarA.OneF0Class == 0x1A)
                    {
                        //object is a trigger
                        return trigger.RunTrigger(CollidedObject_VarA.index, MotionObject, CollidedObject_VarA, (int)triggerObjectDat.triggertypes.MOVE, UWTileMap.current_tilemap.LevelObjects);
                    }
                }
            }
            //seg030_2BB7_3CE:
            if (varB == false)
            {
                return 2;
            }
            else
            {//seg030_2bb7_d39
                if (commonObjDat.ActivatedByCollision(diMotionObject_itemid))
                {//seg030_2bb7_3F7
                    UWMotionParamArray.dseg_67d6_25BC = 1;
                    use.Use(
                        index: MotionObject.index,
                        objList: UWTileMap.current_tilemap.LevelObjects,
                        WorldObject: true);//this line will probably break a lot until I make use a more vanilla compliant Use() function.
                    if (UWTileMap.ValidTile(CollidedObject_VarA.tileX, CollidedObject_VarA.tileY))
                    {//Seg031_2bb7_421
                        return BounceOtherObject_seg030_2BB7_8(MotionParams, CollidedObject_VarA);
                    }
                    else
                    {
                        return 0x10;
                    }
                }
                else
                {//Seg031_2bb7_421
                    return BounceOtherObject_seg030_2BB7_8(MotionParams, CollidedObject_VarA);
                }
            }
        }


        /// <summary>
        /// Applies motion to the object hit based on the motion params of the object hit with
        /// </summary>
        /// <param name="MotionParams"></param>
        /// <param name="toBounce"></param>
        /// <returns></returns>
        static int BounceOtherObject_seg030_2BB7_8(UWMotionParamArray MotionParams, uwObject toBounce)
        {
            if (toBounce != null)
            {
                var newMotionParams = new UWMotionParamArray();
                InitMotionParams(toBounce, newMotionParams);
                if (newMotionParams.mass_18 != 0)
                {
                    var di_mass = (newMotionParams.mass_18 << 6) / newMotionParams.mass_18;
                    if (di_mass > 0x80)
                    {
                        di_mass = 0x80;
                    }
                    newMotionParams.heading_1E = MotionParams.heading_1E;
                    newMotionParams.unk_14 = 0xEB;
                    newMotionParams.unk_a_pitch = (short)(MotionParams.unk_a_pitch * di_mass);

                    //vanilla behaviour here is to retore currobj details

                    ApplyProjectileMotion(toBounce, newMotionParams);
                    objectInstance.Reposition(toBounce);//finally move!
                }
                else
                {
                    if (newMotionParams.unk_14 != 0)
                    {
                        newMotionParams.unk_14 = 0;
                    }
                }
            }

            return 4;
        }


        static void MaybeReflection_seg031_2CFA_CC6(UWMotionParamArray MotionParams)
        {
            MotionParams.unk_a_pitch = 0x8C;
            MotionParams.unk_10 = -4;
            if (MotionParams.unk_14 <= 0xEB)
            {
                MotionParams.unk_14 = 0xEB;
            }
            MotionParams.tilestate25 = 0x10;
            if ((Rng.r.Next(0x7fff) & 0x3) != 0)
            {
                MotionParams.heading_1E -= 0x3000;
                MotionParams.heading_1E += (short)Rng.r.Next(0x6000);
            }
            UWMotionParamArray.dseg_67d6_26A4 = 1;
        }

        static void seg031_2CFA_78A(UWMotionParamArray MotionParams, int arg0)
        {
            MotionParams.speed_12 -= (byte)(UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414 * UWMotionParamArray.dseg_67d6_412);
            if (MotionParams.speed_12 > 0)
            {
                if (InitialMotionCalc_seg031_2CFA_412(MotionParams, false, arg0))
                {
                    return;
                }
            }
            UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414 = (short)(UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E + 1);
        }

        /// <summary>
        /// Returns a value that is used with GetTileState.
        /// </summary>
        /// <param name="MotionParams"></param>
        /// <returns></returns>
        static int GetCollisionHeightState_seg031_2CFA_13B2(UWMotionParamArray MotionParams)
        {//funciton returns wrong result.
            var var2 = 0;
            var var3 = 0;
            var var4 = SBB(UWMotionParamArray.dseg_67d6_26BC & 0x80);
            var var6 = 0;
            UWMotionParamArray.dseg_67d6_26A5 = 0;

            ProcessMotionTileHeights_seg028_2941_385(MotionParams.unk_24);
            ScanForCollisions_seg028_2941_C0E( 0, 0);
            SetCollisionTarget_seg031_2CFA_10E(MotionParams, 0);
            var si_result = MotionCalcArray.UnkC_terrain_base | MotionCalcArray.UnkE_base;
            var var5 = SBB(UWMotionParamArray.dseg_67d6_26BC);

            if (MotionCalcArray.Unk14_collisoncount_base > 0)
            {
                var di = MotionCalcArray.Unk16_base;
                //seg031_2CFA_1463
                while (MotionCalcArray.Unk16_base + MotionCalcArray.Unk15_base > di)
                {
                    var2 = CollideObjects_seg030_2BB7_1CE(MotionParams, di, MotionCalcArray.MotionArrayObjectIndexA_base);
                    if ((var2 & 4) != 0)
                    {
                        si_result |= 0x400;
                    }
                    //seg031_2CFA_1440: 
                    if ((var2 & 0x18) == 0)
                    {
                        if ((var2 & 0x10) == 0)
                        {//seg031_2CFA_1455
                            si_result |= 0x8000;
                        }
                        else
                        {//seg031_2CFA_144E
                            si_result |= 0x4000;
                        }
                        return si_result;
                    }
                    //seg031_2CFA_1462
                    di++;
                }
            }

            //seg031_2CFA_1473: After while loop

            if (MotionCalcArray.z4 == UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419)
            {//seg031_2CFA_166F
                if (UWMotionParamArray.ACollisionIndex_dseg_67d6_416 != -1)
                {
                    //seg031_2CFA_1676:
                    if (commonObjDat.UnknownFlag3_1(UWMotionParamArray.CollisionItemID_dseg_67d6_417))
                    {
                        //seg031_2CFA_168E
                        if (var4 != 0)
                        {
                            si_result |= 0x80;
                        }
                        si_result &= 0xfffB;
                    }
                }
            }
            else
            {//seg031_2CFA_1483:
                if (var5 != 0 && UWMotionParamArray.ACollisionIndex_dseg_67d6_416 == -1)
                {
                    //seg031_2CFA_1493:
                    if (Math.Abs(MotionCalcArray.z4 - UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419) > MotionParams.unk_24)
                    {
                        var3 = 0;
                    }
                    else
                    {
                        var3 = 1;
                    }

                    if
                        (
                            (var3 != 0)
                            ||
                            (var3 == 0 && MotionParams.unk_a_pitch == 0 && ((MotionCalcArray.UnkE_base & 0x800) == 0) && (MotionCalcArray.UnkC_terrain_base & 4) == 0)
                        )
                    {
                        var3 = 1;
                    }
                    else
                    {
                        var3 = 0;
                    }
                }
                else
                {
                    //seg031_2CFA_14DF
                    if (var4 != 0)
                    {
                        //seg031_2CFA_14EF:
                        if (commonObjDat.UnknownFlag3_1(UWMotionParamArray.CollisionItemID_dseg_67d6_417))
                        {
                            var3 = 1;
                        }
                        else
                        {
                            var3 = 0;
                        }

                        //seg031_2CFA_1504:
                        if (var3 != 0)
                        {
                            //seg031_2CFA_1511:
                            var collision = collisionTable[UWMotionParamArray.ACollisionIndex_dseg_67d6_416];
                            if (Math.Abs(MotionCalcArray.z4 - collision.height) > MotionParams.unk_24)
                            {
                                var3 = 1;
                            }
                            else
                            {
                                var3 = 0;
                            }
                            if (var3 != 0)
                            {
                                si_result |= 0x80;
                            }
                        }
                    }
                }

                //seg031_2CFA_154F
                if (
                    (var3 != 0)
                    &&
                    (UWMotionParamArray.dseg_67d6_26A8 != 0)
                    &&
                    (var6 == 0)
                )
                {
                    var3 = 1;
                }
                else
                {
                    var3 = 0;
                }

                //seg031_2CFA_156F:. GOING WRONG HERE.
                if ((var3 != 0) && (UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 >= UWMotionParamArray.dseg_67d6_41D))
                {
                    //seg031_2CFA_157E
                    si_result &= 0xFEFF;
                }
                else
                {
                    //seg031_2CFA_1584
                    if (var3 == 0)
                    {
                        if (MotionCalcArray.Unk11_base > MotionCalcArray.z4)
                        {
                            si_result |= 0x100;
                        }
                    }
                }

                //seg031_2CFA_159F:
                if ((var3 != 0) && (UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 + MotionParams.height_23 > 0x7F))
                {
                    //seg031_2CFA_15B9:
                    var3 = 0;
                    UWMotionParamArray.dseg_67d6_26A5 = 1;
                    si_result |= 0x200;
                }
                else
                {
                    //seg031_2CFA_15C8
                    if (var3 != 0)
                    {
                        //seg031_2CFA_15CE:
                        if (UWMotionParamArray.ACollisionIndex_dseg_67d6_416 == -1)
                        {
                            if (UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 + MotionParams.height_23 > UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419)
                            {
                                var3 = 0;
                                UWMotionParamArray.dseg_67d6_26A5 = 1;
                                si_result |= 0x400;
                            }
                        }
                    }
                }

                //seg031_2CFA_15F7: 
                if (
                    (var3 == 0)
                    ||
                    ((var3 != 0) && ((si_result & 0x400) != 0))
                    )
                {
                    if (var3 != 0)
                    {
                        if (si_result != -1)
                        {
                            if (commonObjDat.UnknownFlag3_1(UWMotionParamArray.ACollisionIndex_dseg_67d6_416))
                            {
                                if (var4 == 0)
                                {
                                    var3 = 0;
                                }
                            }
                        }
                    }
                }
                //seg031_2CFA_1634:
                if (var3 != 0)
                {
                    if (Math.Abs(UWMotionParamArray.CollisionHeightRelated_dseg_67d6_419 - MotionCalcArray.Unk10_base) > MotionParams.radius_22)
                    {
                        si_result &= 0xFFFB;
                    }
                    else
                    {
                        si_result |= 4;
                    }
                }
            }

            //seg031_2CFA_169C after the 2nd big if split.
            if ((UWMotionParamArray.dseg_67d6_26A5 != 0) && (var3 == 0))
            {
                SortCollisions_seg028_2941_ED8();
                si_result &= 0xFBFF;
                var di = 0;
                while (MotionCalcArray.Unk15_base > di)
                {
                    var2 = CollideObjects_seg030_2BB7_1CE(MotionParams, di, MotionCalcArray.MotionArrayObjectIndexA_base);
                    if ((var2 & 0x4) != 0)
                    {
                        si_result |= 0x400;
                    }
                    di++;
                }
            }

            //seg031_2CFA_16DB:
            if (((si_result & 0x80) != 0) && (var4 != 0))
            {
                var collision = collisionTable[UWMotionParamArray.ACollisionIndex_dseg_67d6_416];
                if (
                   ((collision.quality & 0x10) != 0)
                    ||
                    ((collision.quality & 0x10) == 0) && ((MotionCalcArray.UnkC_terrain_base & 4) == 0)
                    )
                {
                    si_result &= 0xF7FF;
                }
            }

            //seg031_2CFA_170A
            if (
                ((MotionCalcArray.UnkE_base & 0x100) != 0)
                &&
                (var5 != 0)
                &&
                (MotionParams.unk_a_pitch == 0)
                )
            {
                if (MotionParams.unk_24 + MotionCalcArray.z4 >= MotionCalcArray.Unk11_base)
                {
                    MotionCalcArray.z4 = MotionCalcArray.Unk11_base;
                    if (MotionCalcArray.z4 != MotionCalcArray.Unk10_base)
                    {
                        si_result &= 0xFFFB;
                    }
                    else
                    {
                        si_result |= 4;
                    }
                }
            }

            //next join seg031_2CFA_1766:

            if ((si_result & 0xFC) == 0)
            {
                si_result |= 0x1000;
            }

            if ((si_result & 0x80) == 0)
            {
                if (MotionCalcArray.z4 - MotionParams.radius_22 > MotionCalcArray.Unk11_base)
                {
                    si_result |= 0x1000;
                }
            }
            return si_result;
        }


        static int AMotionCollisionFunction_seg006_1413_ABF(int arg0)
        {
            Debug.Print("TODO 1413_ABF()");
            return 0;
        }

        static uwObject ProbablyThrownObjectCollision_seg030_2BB7_10BC(uwObject projectile, int tileX, int tileY, int arg8)
        {
            //Crap. this function creates a new calcarray. I will need to make my static array an instance that can change.
            return projectile;
        }

    }//end class
}//end namespace