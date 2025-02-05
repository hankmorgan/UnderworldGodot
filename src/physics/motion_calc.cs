using System;
namespace Underworld
{
    public partial class motion : UWClass
    {

        static int LikelyIsMagicProjectile_dseg_67d6_26B8;
        static int MotionParam0x25_dseg_67d6_26A9;
        static int CalculateMotionGlobal_dseg_67d6_25DB;

        static int CalculateMotionGlobal_dseg_67d6_26B6;

        public static bool CalculateMotion_TopLevel(uwObject projectile, UWMotionParamArray MotionParams, int MaybeMagicObjectFlag)
        {//seg006_1413_D6A
            MotionParams.speed_12 = projectile.Projectile_Speed << 4;
            CalculateMotion(projectile, MotionParams, MaybeMagicObjectFlag);
            return true;
        }


        static void CalculateMotion(uwObject projectile, UWMotionParamArray MotionParams, int MaybeMagicObjectFlag)
        {
            //TODO
            LikelyIsMagicProjectile_dseg_67d6_26B8 = MaybeMagicObjectFlag;
            MotionParam0x25_dseg_67d6_26A9 = MotionParams.unk_25;
            CalculateMotionGlobal_dseg_67d6_25DB = 0;
            CalculateMotionGlobal_dseg_67d6_26B6 = 0;

            if (seg031_2CFA_412(projectile, MotionParams, 1, 1))
            {
                //do more processing at seg031_2CFA_69
            }
        }

        static bool seg031_2CFA_412(uwObject projectile, UWMotionParamArray MotionParams, int arg0, int arg2)
        {
            short var2 = 0; short var4 = 0;
            short var8 = 0;
            SomethingProjectileHeading_seg021_22FD_EAE(MotionParams.heading_1E, ref var2, ref var4);
            MotionParams.unk_6 = (var2 * MotionParams.unk_14) >> 0xF;
            MotionParams.unk_8 = (var4 * MotionParams.unk_14) >> 0xF;

            //possibly the following are translation vectors
            MotionParams.unk_6 = MotionParams.unk_6 + (MotionParams.unk_c * MotionParams.speed_12);
            MotionParams.unk_8 = MotionParams.unk_6 + (MotionParams.unk_d * MotionParams.speed_12);
            MotionParams.unk_a = MotionParams.unk_a + (MotionParams.unk_10 * MotionParams.speed_12);

            if ((MotionParams.unk_6 | MotionParams.unk_8 | MotionParams.unk_a) == 0)
            {
                return false;
            }
            else
            {
                //seg031_2CFA_4D2
                if (arg0 != 0)
                {
                    CopyMotionValsToAnotherArray_seg031_2CFA_93(MotionParams);
                }
                if (Math.Abs(MotionParams.unk_6) <= Math.Abs(MotionParams.unk_8))
                {
                    UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer = 1;
                }
                else
                {
                    UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer = 0;
                }
                UWMotionParamArray.dseg_67d6_40C_indexer = (UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer +1 )/2;

                if (MotionParams.GetParam6(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer) <=0)
                {
                   UWMotionParamArray.dseg_67d6_404[UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer] = -8192;
                }
                else
                {
                    UWMotionParamArray.dseg_67d6_404[UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer] = +8192;
                }

                if (MotionParams.GetParam6(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer) == 0)
                {
                    //seg031_2CFA_5D8:
                    UWMotionParamArray.dseg_67d6_404[UWMotionParamArray.dseg_67d6_40C_indexer] = 1;
                    var8 = 0;
                    UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E = 0;
                    UWMotionParamArray.dseg_67d6_410 = 0;
                    UWMotionParamArray.dseg_67d6_412 = MotionParams.speed_12;
                }
                else
                {   
                    //seg031_2CFA_54F
                    var tmp = (UWMotionParamArray.dseg_67d6_404[UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer] / 0x100) * MotionParams.GetParam6(UWMotionParamArray.dseg_67d6_40C_indexer);
                    tmp = tmp / MotionParams.GetParam6(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer); 
                    tmp = tmp << 8;

                    UWMotionParamArray.dseg_67d6_404[UWMotionParamArray.dseg_67d6_40C_indexer] = tmp;

                    var8 = (short)(MotionParams.GetParam6(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer) * MotionParams.speed_12);

                    UWMotionParamArray.MAYBEcollisionOrGravity_dseg_67d6_40E = Math.Abs(var8) >> 0xD;

                    UWMotionParamArray.dseg_67d6_410 = Math.Abs(var8) & 0x1FFF;

                    UWMotionParamArray.dseg_67d6_412 = Math.Abs(0x2000/MotionParams.GetParam6(UWMotionParamArray.MotionGlobal_dseg_67d6_40A_indexer));
                }

                UWMotionParamArray.GravityCollisionRelated_dseg_67d6_414 = 0;

                if (
                    (UWMotionParamArray.MotionArrayObjectIndex_dseg_67d6_25CE != 1)
                    ||
                    ((UWMotionParamArray.MotionArrayObjectIndex_dseg_67d6_25CE == 1) && MotionParams.unk_a !=0)
                    )
                {
                    if (arg2 != 0)
                    {
                        RelatedToMotion_seg028_2941_385(MotionParams.unk_24);//unk24 is 0 in normal projectile processing
                        seg028_2941_C0E(0,0);
                    }
                }

                //resume at seg031_2CFA_648:

            }      
            return false;//temp      
        }


        static void SomethingProjectileHeading_seg021_22FD_EAE(int heading, ref short Result_arg2, ref short Result_arg4)
        {
            HeadingLookupCalc(heading, out short ax, out short bx);
            Result_arg2 = ax;
            Result_arg4 = bx;            
        }


        /// <summary>
        /// Does a lookup of a table of values to calculate something.
        /// Below code is a fairly direct copy of the assembly code
        /// </summary>
        /// <param name="bx"></param>
        /// <param name="Result_AX"></param>
        /// <param name="Result_BX"></param>
        /// <returns></returns>
        static int HeadingLookupCalc(int bx, out short Result_AX, out short Result_BX)
        {
            var cx = bx & 0xff;
            bx = bx >> 8;
            var bp = HeadingLookupTable[bx];
            var ax = HeadingLookupTable[bx + 1];
            ax = (short)(ax - bp);
            ax = (short)(ax * cx);
            if (ax < 0)
            {
                ax = (short)(ax >> 8);
                ax = (short)-Math.Abs(ax);
            }
            else
            {
                ax = (short)(ax >> 8);
            }
            ax = (short)(ax + bp);
            Result_AX = ax;

            bp = HeadingLookupTable[64 + bx];
            ax = HeadingLookupTable[65 + bx];
            ax = (short)(ax - bp);
            ax = (short)(ax * cx);

            bx = ax >> 8;
            if (ax < 0)
            {
                bx = -Math.Abs(bx);
            }
            Result_BX = (short)bx;
            return Result_AX;
        }

        static void CopyMotionValsToAnotherArray_seg031_2CFA_93(UWMotionParamArray MotionParams)
        {
            //store some globals
            UWMotionParamArray.relatedtoheadinginMotion_dseg_67d6_25CA = MotionParams.heading_1E;
            UWMotionParamArray.Likely_RadiusInMotion_dseg_67d6_25CC = MotionParams.radius_22;
            UWMotionParamArray.Likely_HeightInMotion_dseg_67d6_25CD = MotionParams.height_23;
            UWMotionParamArray.MotionArrayObjectIndex_dseg_67d6_25CE = MotionParams.index_20;
            
            MotionParams.dseg_67d6_3FC_ptr_to_25C4_maybemotion.Unk0_x = MotionParams.x_0 >> 5;
            MotionParams.dseg_67d6_3FC_ptr_to_25C4_maybemotion.Unk2_y = MotionParams.y_2 >> 5;
            MotionParams.dseg_67d6_3FC_ptr_to_25C4_maybemotion.Unk4_z = MotionParams.z_4 >> 3;

            UWMotionParamArray.RelatedToMotionX_dseg_67d6_3FE = (MotionParams.x_0 & 0x1F) << 8;
            UWMotionParamArray.RelatedToMotionY_dseg_67d6_400 = (MotionParams.y_2 & 0x1F) << 8;
            UWMotionParamArray.RelatedToMotionZ_dseg_67d6_402 = (MotionParams.z_4 & 7) << 8;

        }


        static void RelatedToMotion_seg028_2941_385(int arg0)
        {
            //?
        }

        static void seg028_2941_C0E(int arg0, int arg2)
        {
            //?
        }

    }//end class
}//end namespace