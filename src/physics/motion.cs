namespace Underworld
{
    public partial class motion : Loader
    {
        static int MaybeSpecialMotionObjectDatFlag_dseg_67d6_26D2;

        static motion()
        {
            for (short i=0; i<=collisionTable.GetUpperBound(0);i++)
            {//initialise collision records
                collisionTable[i] = new CollisionRecord(i);
            }
        }
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

            UWMotionParamArray MotionParams = new();

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

        public static bool ApplyProjectileMotion(uwObject projectile, UWMotionParamArray MotionParams)
        {//seg030_2BB7_689
            return false;
        }

    }//end class


    
}//end namespace