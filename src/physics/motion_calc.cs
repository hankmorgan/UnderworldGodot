namespace Underworld
{    
    public partial class motion:UWClass
    {

        public static bool CalculateMotion_TopLevel(uwObject projectile, motionarray MotionParams, int MaybeMagicObjectFlag)
        {//seg006_1413_D6A
            MotionParams.speed_12 = projectile.Projectile_Speed<<4;
            CalculateMotion(projectile, MotionParams, MaybeMagicObjectFlag);
            return true;
        }


        static void CalculateMotion(uwObject projectile, motionarray MotionParams, int MaybeMagicObjectFlag)
        {
            //TODO
        }

    }//end class
}//end namespace