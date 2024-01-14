namespace Underworld
{

    /// <summary>
    /// Player data relating to combat
    /// </summary>
    public partial class playerdat: Loader
    {
        /// <summary>
        /// Does the player have their weapon drawn
        /// </summary>
        public static int play_drawn
        {
            get
            {
                if (_RES==GAME_UW2)
                    {
                        return GetAt(0x61) & 0x1;
                    }
                else
                    {
                        return GetAt(0x60) & 0x1;
                    }
                
            }
        }
    }//end class
}//end namespace