using Godot;

namespace Underworld
{
    public class messageScroll: UWClass
    {
        public static Label MessageScroll
        {
            get
            {
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        return uimanager.instance.messageScrollUW2;
                    default:
                        return uimanager.instance.messageScrollUW1;
                }
            }
        }

        public static void AddString(string stringToAdd)
        {
            MessageScroll.Text = stringToAdd;
        }

    }
}//end namespace