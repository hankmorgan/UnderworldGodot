using Godot;

namespace Underworld
{
    /// <summary>
    /// For managing the "eyes" ui element for showing enemy NPC health.
    /// </summary>
    public partial class uimanager : Node2D
    {
        [ExportGroup("Eyes")]
        [Export] public TextureRect Eyes;


        public static int EyeLevel = 0; //ha.
        public static int CurrentEyeLevel = 0;
        static double EyeTimer;
        /// <summary>
        /// Controls the timing of when the element returns to the default "off" positiong
        /// </summary>
        static double TimeLastReset;
        private void InitEyes()
        {
            grEyes = new GRLoader(GRLoader.EYES_GR, GRLoader.GRShaderMode.UIShader);
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                Eyes.Position = new Vector2(440, 16);
            }
            else
            {
                Eyes.Position = new Vector2(512, 16);
            }
        }

        /// <summary>
        /// Calculates the target image to display in the eyeball slot to indicate enemy npc health.
        /// </summary>
        /// <param name="_hp"></param>
        /// <param name="_maxhp"></param>
        public static void SetEyeLevel(int _hp, int _maxhp)
        {
            TimeLastReset = 0f;
            if (_maxhp == 0)
            {
                EyeLevel = 0;
                return;
            }
            //This is not quite vanilla as I'm not 100% on how the division below leads to what image to load.
            EyeLevel = (int)(_hp*3)/_maxhp;
            if (EyeLevel>=3)
            {
                EyeLevel = 2;
            }
            EyeLevel = 3 - EyeLevel;
            EyeLevel = int.Min(grEyes.ImageCache.GetUpperBound(0), 4 + EyeLevel);
        }

        private void _ProcessEyeAnims(double delta)
        {
            if (InGame)
            {
                EyeTimer+=delta;
                TimeLastReset+=delta;
                if (EyeTimer>0.2f)
                {
                    EyeTimer = 0;
                    if (CurrentEyeLevel != EyeLevel)
                    {                        
                        if (CurrentEyeLevel > EyeLevel)
                        {
                            //step up
                            CurrentEyeLevel--; 
                        }
                        else
                        {
                            //step down
                            CurrentEyeLevel++;                            
                        }
                        instance.Eyes.Texture = grEyes.LoadImageAt(CurrentEyeLevel);
                    }
                }
                if (TimeLastReset>=10f)
                {
                    TimeLastReset = 0f;
                    EyeLevel = 0;
                }
            }
        }

    }//end class
}//end namespace