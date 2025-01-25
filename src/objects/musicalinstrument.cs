using System.Diagnostics;

namespace Underworld
{
    public class musicalinstrument : objectInstance
    {
        public static string notesplayed;
        public static bool PlayingInstrument;
        public static int InstrumentClassIndex;
        public static bool use(uwObject obj, bool WorldObject)
        {
            if (!WorldObject)
            {
                uimanager.instance.scroll.Clear();
                InstrumentClassIndex = obj.classindex;
                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_play_the_instrument_));
                notesplayed = "";
                PlayingInstrument = true;
                return true;
            }
            else
            {
                return false;
            }

        }

        public static void StopPlaying()
        {
            PlayingInstrument = false;
            uimanager.instance.scroll.Clear();
            uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_put_the_instrument_down_));
            if (_RES == GAME_UW1)
            {
                //process notes to see if cup of wonder spawned (if at right position)
                Debug.Print($"{notesplayed}");
                if (InstrumentClassIndex == 4)//is a flute
                {
                    if (!playerdat.GotCupOfWonder)
                    {
                        if (playerdat.dungeon_level == 3)
                        {
                            if ((playerdat.tileX >= 23) && (playerdat.tileX <= 27) && (playerdat.tileY >= 43) && (playerdat.tileY <= 45))
                            {
                                if (notesplayed == "354237875")
                                {
                                    ObjectCreator.SpawnObjectInHand(174);
                                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 136));
                                    playerdat.GotCupOfWonder = true;
                                }
                            }
                        }

                    }
                }
            }
        }
    }//end class
}//end namespace