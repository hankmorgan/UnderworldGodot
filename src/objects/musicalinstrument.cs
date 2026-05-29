using System.Diagnostics;
using Godot;

namespace Underworld
{
    public class musicalinstrument : objectInstance
    {
        public static string notesplayed;
        public static byte channel = 8;
        public static bool PlayingInstrument;
        public static byte PlayingNote;//what note is currently playing
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
                byte timbre;
                switch (obj.item_id)
                {
                    case 291://mandolin
                        timbre = 24; break;
                    case 292://flute
                    default:
                        timbre = 73; break;

                }
                //change the timbre
                MusicStreamPlayer.Instance.SendMidiMsg((uint)(0xC0 | channel | timbre << 8));
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
                            if ((playerdat.playerObject.tileX >= 23) && (playerdat.playerObject.tileX <= 27) && (playerdat.playerObject.tileY >= 43) && (playerdat.playerObject.tileY <= 45))
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

        /// <summary>
        /// Plays the note based on keyboard input 0-9
        /// </summary>
        /// <param name="keyinput"></param>
        public static void PlayMusicalNote(InputEventKey keyinput)
        {
            if (PlayingNote == 0)//check if no note is already playing.
            {
                notesplayed += keyinput.AsText();
                main.playingnotetimer = 0;
                PlayingNote = (byte)(60 + int.Parse(keyinput.AsText().ToString())); //taking 60 as middle C or button 0
                //Send note On
                MusicStreamPlayer.Instance.SendMidiMsg((uint)(0x90 | channel | (PlayingNote << 8) | (0x70 << 16)));
            }
        }


        /// <summary>
        /// Sends a midi note off command to stop the currently running musical note.
        /// </summary>
        public static void StopMusicalNote()
        {
            //Send Midi note off
            MusicStreamPlayer.Instance.SendMidiMsg((uint)(0x80 | musicalinstrument.channel | (musicalinstrument.PlayingNote << 8) | (0x1 << 16)));
            PlayingNote = 0;
        }

    }//end class
}//end namespace