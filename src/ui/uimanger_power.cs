using System;
using Godot;
using Godot.NativeInterop;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("PowerGem")]
        [Export]
        public TextureRect PowerGemUW1;
        [Export]
        public TextureRect PowerGemUW2;


        /// <summary>
        /// Frames 0-9 of the power gem
        /// </summary>
        public static int PowerLevelFrame=0;

        /// <summary>
        /// Looping frame number when at max power
        /// </summary>
        public static int PowerFrameAdjust = 0;
        public static TextureRect PowerGem
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return instance.PowerGemUW2;
                }
                else
                {
                    return instance.PowerGemUW1;
                }
            }
        }

        public static void InitPower()
        {
            PowerGem.Texture = grPower.LoadImageAt(0);
        }

        public static void ChangePower(int powerlevel)
        {
            if (powerlevel<9)
            {
                PowerLevelFrame = powerlevel;
                PowerFrameAdjust = 0;
                SetPowerFrame(PowerLevelFrame);
            }
            else
            {//cycle frames at max level
                PowerLevelFrame = 9;
                PowerFrameAdjust++;
            
                if ((PowerFrameAdjust>4)&& (UWClass._RES!=UWClass.GAME_UW2))
                {
                    PowerFrameAdjust = 0;
                } 
                if ((PowerFrameAdjust>1)&& (UWClass._RES==UWClass.GAME_UW2))
                {
                    PowerFrameAdjust = 0;
                } 
                                      
                SetPowerFrame(PowerLevelFrame + PowerFrameAdjust);                       
            }                        
        }

        public static void SetPowerFrame(int frameno)
        {
            if (UWClass._RES==UWClass.GAME_UW2)
            {
                frameno = Math.Min(10,frameno);
            }
            PowerGem.Texture = grPower.LoadImageAt(frameno); 
        }

        public static void ResetPower()
        {
            SetPowerFrame(0);
            PowerLevelFrame = 0;
            PowerFrameAdjust = 0;
        }
    }//end class
}//end namespace