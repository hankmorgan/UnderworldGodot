namespace Underworld
{
    public class leech : objectInstance
    {

        public static bool use(uwObject obj, bool WorldObject)
        {
            if (WorldObject)
            {
                return false;
            }
            else
            {
                LeechDamage();
                if (playerdat.play_poison>0)
                {                
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_the_leeches_remove_the_poison_as_well_as_some_of_your_skin_and_blood_));
                    //reduce poison
                    playerdat.play_poison = 0;
                }

                ObjectCreator.Consume(obj, !WorldObject);

                //flash screen
                if (_RES==GAME_UW2)
                {
                    uimanager.FlashColour(0x30, uimanager.Cuts3DWin,0.2f);
                }
                else
                {
                    uimanager.FlashColour(0xA8, uimanager.Cuts3DWin,0.2f);
                }               

                return true;
            }
        }

        private static void LeechDamage()
        {
            //reduce hp to no less than 3 (technically a cursed object spell is supposed to be cast here)
            var dmg = Rng.DiceRoll(2, 8);
            if (playerdat.play_hp - dmg < 3)
            {
                playerdat.play_hp = 3;
            }
            else
            {
                playerdat.play_hp = playerdat.play_hp - dmg;
            }
        }
    }
}