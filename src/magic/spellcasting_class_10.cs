namespace Underworld
{
    public partial class SpellCasting : UWClass
    {
        public static void CastClass10_ManaBoost(int minorclass)
        {
            //check mana rules for the academy test.
            if (_RES == GAME_UW2)
            {
                if (worlds.GetWorldNo(playerdat.dungeon_level) == 5)//Academy
                {
                    var academylevel = 1 + playerdat.dungeon_level % 8;
                    if ((academylevel > 1) && (academylevel < 8))
                    {
                        return;
                    }
                    if (academylevel == 8)
                    {
                        if (playerdat.tileX < 25)
                        {
                            return;
                        }
                    }
                }
            }
            //Apply mana boost
            if (minorclass < 0)
            {//boost mana by minus minus minor class. Not clear when this could happen...
                playerdat.play_mana = System.Math.Min(playerdat.play_mana + minorclass, playerdat.max_mana);
            }
            else
            {
                var increase = 1 + ((playerdat.max_mana * (minorclass + Rng.r.Next(0, 4))) >> 4);
                playerdat.play_mana = System.Math.Min(playerdat.play_mana + increase, playerdat.max_mana);
            }
        }
    }
}