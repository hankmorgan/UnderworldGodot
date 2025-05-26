using System;

namespace Underworld
{
    /// <summary>
    /// Trap that launches player or NPC into the air
    /// </summary>
    public class a_jump_trap : trap
    {
        public static void Activate(uwObject trapObj)
        {
            if (Math.Abs(UWMotionParamArray.instance.unk_6_x) + Math.Abs(UWMotionParamArray.instance.unk_8_y) >= trapObj.heading * 0x2F)
            {
                if (UWMotionParamArray.instance.unk_10_Z != -4)
                {
                    UWMotionParamArray.instance.unk_10_Z = -2;
                }
            }

            if (UWMotionParamArray.instance.index_20 == 1)
            {
                UWMotionParamArray.instance.unk_a_pitch = (short)((trapObj.quality * 0x2F) / 2);
            }
            else
            {
                UWMotionParamArray.instance.unk_a_pitch = (short)((trapObj.quality * 0x8D) / 4);
            }

            if (trapObj.owner == 0)
            {
                UWMotionParamArray.instance.unk_6_x = (short)(UWMotionParamArray.instance.unk_6_x / 2);
                UWMotionParamArray.instance.unk_8_y = (short)(UWMotionParamArray.instance.unk_8_y /2);
            }
            else
            {
                UWMotionParamArray.instance.unk_6_x = 0;
                UWMotionParamArray.instance.unk_8_y = 0;
            }
        }


    }//end class
}//end namespace