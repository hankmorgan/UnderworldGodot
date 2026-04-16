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
                    // Trap-script SFX ids: original code passes effectparam directly
                    // here. The 0x64 offset that earlier debug code printed appears
                    // to be a mis-trace — SOUNDS.DAT only has 24 entries so an
                    // 0x64+ id would never resolve. Trigger by raw id and log if
                    // it falls outside the SOUNDS.DAT range.
                    Debug.Print($"Playsound effect raw id {effectparam}");
                    Sfx.SoundEffects.Play(effectparam);
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