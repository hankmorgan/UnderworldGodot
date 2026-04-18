using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using Godot;
using Underworld.Sfx;

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
        public const byte SoundEffectPortcullis = 0x14;
        public const byte SoundEffectSpellFailure = 0x16;   
        public const byte SoundEffectLight = 0x20; 
        public const byte SoundEffectSpellRing1 = 0x2A;     
        public const byte SoundEffectSpellRing2 = 0x2c;
        public const byte SoundEffectFail = 0x2D;

        /// <summary>
        /// The sound is played at the location of the avatar.
        /// </summary>
        /// <param name="effectno"></param>
        /// <param name="arg2_volume"></param>
        /// <param name="arg4_panning"></param>
        public static void PlaySoundEffectAtAvatar(byte effectno, byte arg2_volume, byte arg4_panning)        
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
                        PlayVocFile(effectno,arg2_volume, arg4_panning);
                      }
                }
                else
                {
                    //UW1
                    Sfx.SoundEffects.Play(effectno);
                }
            }
        }


        /// <summary>
        /// Loads and plays a .voc file from \SOUNDS folder.
        /// </summary>
        /// <param name="effectno"></param>
        /// <param name="Volume"></param>
        /// <param name="Panning"></param>
        private static void PlayVocFile(byte effectno, int Volume, int Panning)
        {
            if ((effectno == 90) || (effectno == 91))
            {
                //TODO foot step sounds from NPCS, needs special handling.
                return;
            }
            string filepath;
            if (effectno >= 100)
            {
                //guardian laughter
                filepath = System.IO.Path.Combine(
                    BasePath, "SOUND",
                    $"UW{effectno - 100:0#}.VOC");
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

        public static void PlaySoundEffectAtObject(byte effectNo, uwObject obj, int arg6)
        {            
            PlaySoundEffectAtCoordinate(effectNo, (obj.tileX<<3) + obj.xpos, (obj.tileY<<3) + obj.xpos, arg6);
        }

        /// <summary>
        /// Plays a sound at the specified coordinates and calculates the volume and panning of the sound based on position relative to the player.
        /// </summary>
        /// <param name="effectNo"></param>
        /// <param name="xCoordinate"></param>
        /// <param name="yCoordinate"></param>
        /// <param name="arg6_velocityoffset"></param>
        public static void PlaySoundEffectAtCoordinate(byte effectNo, int xCoordinate, int yCoordinate, int arg6_velocityoffset)
        {
            SoundEntry effectData;
            bool isFootStep=false;
            byte varA_panning; 
            byte var8_volume;
            if (playerdat.SoundEffectsEnabled)
            {
                if (effectNo >= 100)
                {
                    var8_volume = 0x7F;
                    varA_panning = 0x40;
                }
                else
                {
                    if (effectNo == 90)
                    {//footstep
                        effectData =  SoundEffects.SoundDat[1];
                    }
                    else if (effectNo == 91)
                    {
                        effectData =  SoundEffects.SoundDat[2];
                    }
                    else
                    {
                        effectData = SoundEffects.SoundDat[effectNo];
                    }

                    CalculateSoundFallOff(xCoordinate, yCoordinate, (byte)(effectData.Velocity + arg6_velocityoffset), out varA_panning, out var8_volume);
                    if (var8_volume != 0)
                    {
                        if ((effectNo == 90) )
                        {
                            isFootStep = true;
                            effectNo = 1;
                        }
                        else if (effectNo == 91)
                        {
                            isFootStep = true;
                            effectNo = 2;
                        }
                        else
                        {
                            isFootStep = false;
                        }
                    }

                    if (isFootStep)
                    {
                        Sfx.SoundEffects.Play(effectNo, varA_panning, var8_volume);
                    }
                    else
                    {
                        PlayVocFile(effectNo, var8_volume, varA_panning);
                    }
                }                

            }
        }


        /// <summary>
        /// Based on values in sounds.dat work out how loud the sound should be at a distance.
        /// </summary>
        /// <param name="XCoordinate"></param>
        /// <param name="YCoordinate"></param>
        /// <param name="Velocity"></param>
        /// <param name="arg6_panning"></param>
        /// <returns></returns>
        public static void CalculateSoundFallOff(int XCoordinate, int YCoordinate, byte Velocity, out byte arg6_panning, out byte arg8_volume)
        {
            // arg6 = 0;
            // arg8 =  0x7F;
            var var10_heading = (short)(0x4000 - (((playerdat.playerObject.heading<<5) + playerdat.playerObject.npc_heading)<<8))& 0xFFFF;
            int YComponent_var18 = 0; int XComponent_var1A = 0;
            motion.GetVectorForDirection(var10_heading, ref YComponent_var18, ref XComponent_var1A);
            var xDiffVar2 = XCoordinate - ((playerdat.playerObject.npc_xhome<<3) + playerdat.playerObject.xpos);
            var yDiffVar4 = YCoordinate - ((playerdat.playerObject.npc_yhome<<3) + playerdat.playerObject.ypos);
            var distVarE = (int)(Math.Sqrt(xDiffVar2*xDiffVar2 + yDiffVar4*yDiffVar4));
            if (distVarE != 0)
            {//1E73:0E72
                var var14X = (int)((xDiffVar2 * 0x7F)/distVarE);
                var var16Y = (int)((yDiffVar4 * 0x7F)/distVarE);

                XComponent_var1A >>= 8;
                YComponent_var18 >>= 8;

                var var12 = (((XComponent_var1A * var14X) - (YComponent_var18 * var16Y)) >> 8); 
                arg6_panning = (byte)(0x40 - var12);
                if (arg6_panning > 0x7F)
                {
                    arg6_panning = 0x7F;
                }
                else if (arg6_panning <0)
                {
                    arg6_panning = 0;
                }
            }
            else
            {
                arg6_panning = 0x40;
            }

            if (distVarE > 0x30)
            {
                arg8_volume = 0;
            }
            else
            {
                if (distVarE >= 8)
                {
                    arg8_volume = (byte)((Velocity * (0x30-distVarE))/0x28);
                }
                else
                {
                    //dist<8
                    arg8_volume = Velocity;
                }
            }
        }
    }
}