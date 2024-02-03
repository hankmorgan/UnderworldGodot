using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Messagescroll")]
        [Export] public Label messageScrollUW1;
        [Export] public Label messageScrollUW2;

        public static Label MessageScroll
        {
            get
            {
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        return instance.messageScrollUW2;
                    default:
                        return instance.messageScrollUW1;
                }
            }
        }

        public static void AddToMessageScroll(string stringToAdd)
        {
            MessageScroll.Text = stringToAdd;
        }

    }
}//end namespace