using System;

namespace Underworld
{
    public partial class motion : UWClass
    {

        static int LikelyIsMagicProjectile_dseg_67d6_26B8;
        static int MotionParam0x25_dseg_67d6_26A9;
        static int CalculateMotionGlobal_dseg_67d6_25DB;

        static int CalculateMotionGlobal_dseg_67d6_26B6;

        public static bool CalculateMotion_TopLevel(uwObject projectile, motionarray MotionParams, int MaybeMagicObjectFlag)
        {//seg006_1413_D6A
            MotionParams.speed_12 = projectile.Projectile_Speed << 4;
            CalculateMotion(projectile, MotionParams, MaybeMagicObjectFlag);
            return true;
        }


        static void CalculateMotion(uwObject projectile, motionarray MotionParams, int MaybeMagicObjectFlag)
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

        static bool seg031_2CFA_412(uwObject projectile, motionarray MotionParams, int arg0, int arg2)
        {
            short var2 = 0; short var4 = 0;
            SomethingProjectileHeading_seg021_22FD_EAE(MotionParams.heading_1E, ref var2, ref var4);
            MotionParams.unk_6 = (var2 * MotionParams.unk_14) >> 0xF;
            MotionParams.unk_8 = (var4 * MotionParams.unk_14) >> 0xF;

            return false;
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

    }//end class
}//end namespace