using System.Diagnostics;
using System.IO;
using Godot;
using Underworld.Sfx;

namespace Underworld
{
    /// <summary>
    /// Wrapper for calling appropriate Sound Effects in game. Entry points
    /// mirror the UW1/UW2 asm:
    ///   PlaySoundEffectAtAvatar         — non-positional (UW1 seg014_B1B / UW2 1DB2)
    ///   PlaySoundEffectAtCoordinate     — positional (UW1 seg014_8AE / UW2 1C50)
    ///   PlaySoundEffectAtObject         — object-based wrapper (UW1 seg014_B9C / UW2 1E7E)
    /// </summary>
    public class UWsoundeffects : UWClass
    {
        public const byte SoundEffectHit1 = 0x3;
        public const byte SoundEffectHit2 = 0x4;
        public const byte SoundEffectBowTwang = 0x9;
        public const byte SoundEffectDoor = 0xB;
        public const byte SoundEffectLanding = 0xF;
        public const byte SoundEffectSpellNotReady = 0xB; // spell timers are not yet implemented.
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
        /// The sink every sound eventually flows through. pan is 0..0x7F
        /// (0x40 = centre); velocityOffset is a signed-byte delta added to
        /// the SOUNDS.DAT[+2] base velocity before clamping.
        /// Source: UW1 seg014_B1B (UW1_asm.asm:64927-65021) for UW1 path,
        ///         UW2 PlaySoundEffect_1DB2 (uw2_asm.asm:82585-82751) for UW2 path.
        /// </summary>
        public static void PlaySoundEffectAtAvatar(byte effectno, byte pan, byte velocityOffset)
        {
            if ((effectno == 90) || (effectno == 91))
            {
                // TODO footstep sounds from NPCs, needs special handling
                return;
            }
            if (!playerdat.SoundEffectsEnabled) return;

            if (_RES == GAME_UW2)
            {
                PlayUw2(effectno, pan, velocityOffset);
            }
            else
            {
                // UW1 OPL/TVFX path — mono hardware, pan is ignored by
                // the TVFX backend (authentic AdLib behaviour). Volume
                // attenuation flows through the backend's velocityOffset
                // hook. See src/audio/sfx/godot/SoundEffects.cs.
                Sfx.SoundEffects.Play(effectno, pan: pan, velocityOffset: velocityOffset);
            }
        }

        /// <summary>
        /// Object-based positional emit. Source: UW1 seg014_B9C
        /// (UW1_asm.asm:65027-65091), UW2 PlaySoundEffectAtObject_1E7E
        /// (uw2_asm.asm:82757-82863). Extracts packed (tile&lt;&lt;3 | fine)
        /// coords from the object and forwards.
        /// </summary>
        public static void PlaySoundEffectAtObject(byte effectNo, uwObject obj, int volDelta)
        {
            // Packed coords: (tile << 3) | fine. 8 units per tile.
            // uw2_asm.asm:82814-82848 pattern.
            int packedX = (obj.tileX << 3) | (obj.xpos & 0x7);
            int packedY = (obj.tileY << 3) | (obj.ypos & 0x7);
            PlaySoundEffectAtCoordinate(effectNo, packedX, packedY, volDelta);
        }

        /// <summary>
        /// Positional emit by packed coord. Source: UW1 seg014_8AE
        /// (UW1_asm.asm:64454-64921), UW2 PlaySoundEffect_1C50
        /// (uw2_asm.asm:82356-82579).
        ///
        /// Special-cases:
        ///   effectNo >= 100  — guardian laughter, played full-volume centre-pan.
        ///   effectNo == 90   — NPC walk footstep, redirects to SOUNDS.DAT entry 1.
        ///   effectNo == 91   — NPC run footstep, redirects to SOUNDS.DAT entry 2.
        /// (uw2_asm.asm:82405-82425 in PlaySoundEffect_1C50.)
        /// </summary>
        public static void PlaySoundEffectAtCoordinate(byte effectNo, int packedX, int packedY, int volDelta)
        {
            if (!playerdat.SoundEffectsEnabled) return;

            // Guardian laughter bypasses distance attenuation entirely.
            if (effectNo >= 100)
            {
                PlaySoundEffectAtAvatar(effectNo, pan: 0x40, velocityOffset: 0);
                return;
            }

            // Footstep id redirection — uw2_asm.asm:82405-82425.
            byte lookupId = effectNo switch
            {
                90 => 1,
                91 => 2,
                _ => effectNo,
            };

            var r = CalculateSoundFallOff(packedX, packedY, lookupId, volDelta);
            if (r.Culled) return;

            // Convert absolute vol back into a velocityOffset for the sink,
            // which adds it to SOUNDS.DAT[+2] before clamping. Round-trip is
            // exact: baseVel + (r.Vol - baseVel) = r.Vol.
            byte baseVel = LookupBaseVelocity(lookupId);
            sbyte velocityOffset = (sbyte)(r.Vol - baseVel);
            PlaySoundEffectAtAvatar(lookupId, pan: r.Pan, velocityOffset: (byte)velocityOffset);
        }

        /// <summary>
        /// Compute (vol, pan, culled) from source position, player state,
        /// and a SOUNDS.DAT entry id. Pure pass-through to
        /// <see cref="PositionalAudio.Sample"/> with the coord + heading
        /// extraction wired to the port's player object.
        /// </summary>
        public static SoundFalloff CalculateSoundFallOff(int packedX, int packedY, int effectNo, int volDelta)
        {
            var player = playerdat.playerObject;

            // Player coords: (npc_xhome << 3) + xpos, matching
            // uw2_asm.asm:79391-79421 which reads obj+0x16 tile-home bits
            // shifted up 3, then adds obj+2 fine-fraction bits.
            int playerX = (player.npc_xhome << 3) + (player.xpos & 0x7);
            int playerY = (player.npc_yhome << 3) + (player.ypos & 0x7);

            // 8-bit heading. Per UW2 uw2_asm.asm:79427-79441:
            //   heading8 = (tileOctant << 5) | subAngle.
            byte heading8 = (byte)(((player.heading & 0x07) << 5) | (player.npc_heading & 0x1F));

            int baseVelocity = LookupBaseVelocity((byte)effectNo);

            return PositionalAudio.Sample(
                srcX: packedX, srcY: packedY,
                playerX: playerX, playerY: playerY,
                heading8: heading8,
                baseVelocity: baseVelocity,
                volDelta: (sbyte)volDelta);
        }

        /// <summary>
        /// SOUNDS.DAT[+2] per-sound base velocity. Returns 0x7F as a
        /// conservative default when the SFX subsystem hasn't loaded
        /// its entry table. Source: UW1_asm.asm:64622, uw2_asm.asm:82447.
        /// </summary>
        private static byte LookupBaseVelocity(byte effectNo)
        {
            var table = Sfx.SoundEffects.SoundDat;
            return effectNo < table.Length ? table[effectNo].Velocity : (byte)0x7F;
        }

        /// <summary>
        /// UW2 VOC digital-sample playback path. Loads the VOC, bakes the
        /// mono sample into stereo-interleaved int16 using Miles AIL2's
        /// pan_graph × V / 16129 curve (external/AIL2/DMASOUND.ASM:287-294
        /// and :993-1006), wraps in an AudioStreamWav, plays through
        /// DigitalAudioPlayer. Falls back to the MIDI sink on missing VOC
        /// (uw2_asm.asm:82573 analogue).
        /// </summary>
        private static void PlayUw2(byte effectno, byte pan, byte velocityOffset)
        {
            if (effectno == 0xFF) return;

            // Effective vol = clamp(baseVel + signed velocityOffset, 0..0x7F).
            // Matches UW1_asm.asm:64836-64849 / uw2_asm.asm:82447-82638.
            byte baseVel = LookupBaseVelocity(effectno);
            int effective = baseVel + (sbyte)velocityOffset;
            if (effective < 0) effective = 0;
            if (effective > 0x7F) effective = 0x7F;
            byte vol = (byte)effective;

            // vocLoader.Load prepends BasePath/SOUND/ internally
            // (src/loaders/vocloader.cs:55).
            string vocName = effectno >= 100
                ? $"UW{effectno - 100:0#}.VOC"   // guardian laughter
                : $"SP{effectno:0#}.VOC";
            string fullPath = Path.Combine(BasePath, "SOUND", vocName);

            if (File.Exists(fullPath))
            {
                Debug.Print($"Playing sound {vocName} vol={vol:X2} pan={pan:X2}");
                AudioSample sound = vocLoader.Load(vocName);
                if (sound != null && sound.AudioBuffer != null)
                {
                    // AudioSample.AudioBuffer is signed 8-bit PCM already
                    // (vocLoader subtracts 128 at src/loaders/vocloader.cs:96,114).
                    short[] mono = VocToPcm16Mono(sound);
                    short[] stereo = StereoPanBake.Apply(mono, vol, pan);

                    var wav = new AudioStreamWav();
                    wav.Format = AudioStreamWav.FormatEnum.Format16Bits;
                    wav.Stereo = true;
                    wav.MixRate = sound.SampleRate;
                    wav.Data = Int16ToBytesLE(stereo);

                    main.instance.DigitalAudioPlayer.Stream = wav;
                    main.instance.DigitalAudioPlayer.Play();
                }
            }
            else
            {
                // Missing VOC — fall back to MIDI sink.
                Sfx.SoundEffects.Play(effectno, pan: pan, velocityOffset: velocityOffset);
            }
        }

        private static short[] VocToPcm16Mono(AudioSample sound)
        {
            // AudioSample.AudioBuffer stores signed 8-bit PCM (already
            // centred on 0; see src/loaders/vocloader.cs:96,114). Interpret
            // each byte as an sbyte and promote to int16 by shift-left 8.
            byte[] src = sound.AudioBuffer;
            var dst = new short[src.Length];
            for (int i = 0; i < src.Length; i++)
                dst[i] = (short)((sbyte)src[i] << 8);
            return dst;
        }

        private static byte[] Int16ToBytesLE(short[] samples)
        {
            var bytes = new byte[samples.Length * 2];
            for (int i = 0, j = 0; i < samples.Length; i++)
            {
                short s = samples[i];
                bytes[j++] = (byte)(s & 0xFF);
                bytes[j++] = (byte)((s >> 8) & 0xFF);
            }
            return bytes;
        }
    }
}
