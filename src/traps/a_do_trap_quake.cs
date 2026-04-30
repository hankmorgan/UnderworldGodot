namespace Underworld
{
    /// <summary>
    /// Trap which causes an earthway to shakescreen and/or launch the player in air.
    /// </summary>
    public class a_do_trap_quake : hack_trap
    {
        public static void Activate(uwObject trapObj)
        {
            DoTrapQuake(quaketype: trapObj.quality - 59, intensity: trapObj.owner);
        }

        static void DoTrapQuake(int quaketype, int intensity)
        {
            if ((quaketype & 1) != 0)
            {
                motion.SetScreenShake(TypeOfShake: 0x40, duration: 0x1E);
            }
            if ((quaketype & 2) != 0)
            {
                BouncePlayer(intensity);
            }
        }

        static void BouncePlayer(int intensity)
        {
            if (motion.playerMotionParams.unk_10_Z != -4)
            {
                motion.playerMotionParams.unk_10_Z = -2;
            }
            motion.playerMotionParams.unk_a_pitch = (short)((intensity * 0x2F) / 4);
            motion.playerMotionParams.unk_6_x = (short)(motion.playerMotionParams.unk_6_x / 2);
            motion.playerMotionParams.unk_8_y = (short)(motion.playerMotionParams.unk_8_y / 2);
        }
    } //end class
}//end namespace