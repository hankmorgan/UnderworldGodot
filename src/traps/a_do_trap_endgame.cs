namespace Underworld
{
    /// <summary>
    /// Trap which ends the game when the player passes through the moongate in UW1.
    /// </summary>
    public class a_do_trap_endgame : hack_trap
    {
        public static void Activate()
        {
            //TODO : I cannot test this sequence on the actual level yet because destroying the talismans is not yet simulated and it is not possible to have a save game from within the void.
            cutsplayer.PlayCutscene(1, uimanager.VictoryScreen);
        }
    }
}