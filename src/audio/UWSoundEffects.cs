using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;

namespace Underworld
{
    /// <summary>
    /// Wrapper for calling appropriate Sound Effects in Game.
    /// </summary>
    public class UWsoundeffects : UWClass
    {

        public const byte SoundEffectHit1 = 0x3;
        public const byte SoundEffectHit2 = 0x4;
        public const byte SoundEffectBowTwang = 0x9;
        public const byte SoundEffectDoor = 0xB;
        public const byte SoundEffectLanding = 0xF;
        public const byte SoundEffectSpellNotReady = 0xB;//spell timers are not yet implemented.
        public const byte SoundEffectSpell = 0x10;
        public const byte SoundEffectKlang = 0x11;
        public const byte SoundEffectRumble = 0x12;
        public const byte SoundEffectLockPick = 0x13;
        public const byte SoundEffectSpellFailure = 0x16;   
        public const byte SoundEffectLight = 0x20; 
        public const byte SoundEffectSpellRing1 = 0x2A;     
        public const byte SoundEffectSpellRing2 = 0x2c;
        public const byte SoundEffectFail = 0x2D;

        /// <summary>
        /// The sound is played at the location of the avatar.
        /// </summary>
        /// <param name="effectno"></param>
        /// <param name="arg2"></param>
        /// <param name="arg4"></param>
        public static void PlaySoundEffectAtAvatar(byte effectno, byte arg2, byte arg4)        
        {
            if ((effectno == 90)|| (effectno==91))
                    {
                        //TODO foot step sounds from NPCS, needs special handling.
                        return;
                    }
            if (playerdat.SoundEffectsEnabled)
            {
                //Only UW2 voc support so far
                if (_RES == GAME_UW2)
                {
                    if (effectno != 0xFF)
                    {
                        if ((effectno == 90)|| (effectno==91))
                        {
                            //TODO foot step sounds from NPCS, needs special handling.
                            return;
                        }
                        string filepath;
                        if (effectno>=100)
                        {
                            //guardian laughter
                            filepath = System.IO.Path.Combine(
                                BasePath, "SOUND",
                                $"UW{effectno-100:0#}.VOC");
                        }
                        else
                        {
                            filepath = System.IO.Path.Combine(
                                BasePath, "SOUND",
                                $"SP{effectno:0#}.VOC");
                        }
                        
                        if (File.Exists(filepath))
                        {
                            Debug.Print($"Playing sound {filepath}");
                            var sound = vocLoader.Load(
                                    System.IO.Path.Combine(
                                        BasePath, "SOUND",
                                        $"SP{effectno:0#}.VOC"));
                            if (sound.AudioBuffer != null)
                            {                                
                                //TODO: only one audio player is set up so far. Integrate with better sound output methods
                                //TODO: calculations on sound falloff need to be made.
                                main.instance.DigitalAudioPlayer.Stream = sound.toWav();
                                main.instance.DigitalAudioPlayer.Play();
                            }
                        }
                        else
                        {
                            //fallback to midi sound if not .voc file.
                            Sfx.SoundEffects.Play(effectno);
                        }

                    }
                }
                else
                {
                    //UW1
                    Sfx.SoundEffects.Play(effectno);
                }
            }
        }

        public static void PlaySoundEffectAtObject(byte effectNo, uwObject obj, int arg6)
        {            
            PlaySoundEffectAtCoordinate(effectNo, obj.tileX<<3 + obj.xpos, obj.tileY<<3 + obj.xpos, arg6);
        }

        public static void PlaySoundEffectAtCoordinate(byte effectNo, int x, int y, int arg6)
        {
            var var8 = CalculateSoundFallOff(x, y, 0, 0);
            //TODO.. all this stuff, for the moment play at the avatars position.
            PlaySoundEffectAtAvatar(effectNo, 0x40, 0);
        }


        /// <summary>
        /// Based on values in sounds.dat work out how loud the sound should be at a distance.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="SoundDatByte"></param>
        /// <param name="arg6"></param>
        /// <returns></returns>
        public static int CalculateSoundFallOff(int x, int y, int SoundDatByte, int arg6)
        {
            return 0x7F;
        }
    }
}