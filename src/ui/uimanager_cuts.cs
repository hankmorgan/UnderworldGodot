using System.ComponentModel;
using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Cutscenes")]
        [Export] TextureRect CutsSmallUW1;
        [Export] TextureRect CutsSmallUW2;

        public static void InitCuts()
        {
            EnableDisable(CutsSmall,false);
        }

        public static TextureRect CutsSmall
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return instance.CutsSmallUW2;
                }
                else
                {
                    return instance.CutsSmallUW1;
                }
            }
        }
    }//end class
}//end namespace