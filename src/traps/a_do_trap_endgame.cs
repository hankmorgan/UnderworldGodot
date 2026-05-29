namespace Underworld
{
    /// <summary>
    /// Trap which ends the game when the player passes through the moongate in UW1.
    /// </summary>
    public class a_do_trap_endgame : hack_trap
    {
        public static void Activate()
        {    
            cutsplayer.PlayCutscene(1, uimanager.VictoryScreen);
        }
    }
}