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
                    Debug.Print($"Playsound effect id {0x64+effectparam}");
                    //vocLoader.ReadStreamFile()
                    //main.instance.audioplayer.Play()
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