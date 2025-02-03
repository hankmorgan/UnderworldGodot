namespace Underworld
{
    public partial class motion : UWClass
    {
        /// <summary>
        /// A struct based inplementation of an array of motion params that UW uses for projectile calcs
        /// </summary>
        public struct motionarray
        {
            public int x_0;
            public int y_2;
            public int z_4;

            public int unk_a;
            public int unk_10;
            public int speed_12;
            public int pitch_13;
            public int unk_14;

            public int unk_16;
            public int mass_18;
            public int unk_1a;
            public int hp_1b;
            public int scaleresistances_1C;
            public int heading_1E;
            public int unk_1d;
            public int index_20;
            public int radius_22;
            public int height_23;
            public int unk_24;
            public int unk_25;
        }

        //static motionarray MotionParams;

        static int MaybeSpecialMotionObjectDatFlag_dseg_67d6_26D2;

        public static bool MotionProcessing(uwObject projectile)
        {

            //Check if object is still "alive"
            if (projectile.npc_hp == 0)
            {
                if (commonObjDat.qualityclass(projectile.item_id) < 3)
                {
                    if (ObjectRemover.DeleteObjectFromTile(projectile.tileX, projectile.tileY, projectile.index))
                    {
                        return false;
                    }
                    else
                    {
                        projectile.npc_hp = 1;
                    }
                }
            }

            if (commonObjDat.maybeMagicObjectFlag(projectile.item_id))
            {
                MaybeSpecialMotionObjectDatFlag_dseg_67d6_26D2 = 0x1000;
            }
            else
            {
                MaybeSpecialMotionObjectDatFlag_dseg_67d6_26D2 = 0;
            }

            motionarray MotionParams = new();

            InitMotionParams(projectile, MotionParams);

            CalculateMotion_TopLevel(projectile, MotionParams,MaybeSpecialMotionObjectDatFlag_dseg_67d6_26D2);

            //store current x/y homes in globals


            var result = ApplyProjectileMotion(projectile, MotionParams);
            if (result)
            {
                if (_RES == GAME_UW2)
                {
                    switch (projectile.item_id)
                    {
                        case 0x1B://homing dart
                            break;
                        case 0x1E://satellite
                            break;
                    }
                }
            }
            return result;
        }



        public static void seg006_1413_9F5(uwObject projectile)
        {

        }

        public static bool ApplyProjectileMotion(uwObject projectile, motionarray MotionParams)
        {//seg030_2BB7_689
            return false;
        }

    }//end class
}//end namespace