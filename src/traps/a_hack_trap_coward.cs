namespace Underworld
{
    /// <summary>
    /// Trap that handles the avatar or a dueling npc fleeing from an arena in the Pits of Carnage.
    /// </summary>
    public class a_hack_trap_coward : trap
    {
        public static bool Activate(int character)
        {   
            if (character == 0)
            {//Avatar has fled.
                if (playerdat.IsFightingInPit)
                {
                    playerdat.AvatarIsACoward();
                }
                return true;
            }
            else
            {
                //NPC. Not yet implemented.
                return false;
            }
        }
    }//end class
}//end namespace