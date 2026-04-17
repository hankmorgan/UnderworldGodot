using System.Diagnostics;

namespace Underworld
{
    public class special_effects: UWClass
    {
        /// <summary>
        /// 2 = sound effect (specified by param)
        /// 4 = screenshake  (intensity specified by param)
        /// 5,6,7,8 = flash colour (specified by param in bands of 64)
        /// </summary>
        /// <param name="effecttype"></param>
        /// <param name="effectparam"></param>
        public static void SpecialEffect(int effecttype, int effectparam)
        {
            switch (effecttype)
            {
                case 2://sound effect
                    // Dispatch per UW2's PlaySoundEffect (uw2_asm.asm:77819 —
                    // seg016_1E73 sub cmp 0x63; jbe SP-path else UW-path):
                    //   id <= 99 → SOUND/SP{id:00}.VOC (UW2) or UW.AD-TVFX (UW1)
                    //   id >= 100 → SOUND/UW{id-100:00}.VOC (guardian laughter
                    //               etc.) — UW2 only; UW1 SFX is TVFX-only.
                    Debug.Print($"Playsound effect id {effectparam}");
                    if (_RES == GAME_UW2)
                    {
                        //Plays a guardian laugh sound.
                        UWsoundeffects.PlaySoundEffectAtCoordinate((byte)(effectparam + 0x64), 0 ,0, 0);// Sound number being >=100 tells the game to load uw##.voc files
                    }
                    else
                    {
                        Debug.Print("Usage of special effect in UW1. Should not occur?");
                    }
                    break;
                case 4://screenshake
                    Debug.Print($"screenshake left/right with duration {effectparam}");
                    break;
                case 5:
                    uimanager.FlashColour((byte)(64*3 + effectparam), uimanager.CutsSmall);break;
                case 6:
                    uimanager.FlashColour((byte)(64*2 + effectparam), uimanager.CutsSmall);break;
                case 7:
                    uimanager.FlashColour((byte)(64 + effectparam), uimanager.CutsSmall);break;
                case 8:
                    uimanager.FlashColour((byte)(effectparam), uimanager.CutsSmall);break;
            }
        }
    }
}//end namespace