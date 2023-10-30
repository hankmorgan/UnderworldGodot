using System.Diagnostics;
using Godot;

namespace Underworld
{
    public partial class InteractionModeButton : TextureButton
    {

        [Export]
        public uimanager.InteractionModes index;

        static int eventcount=0;
        public override void _Toggled(bool buttonPressed)
        {

            Debug.Print($"Press event {eventcount++} ");
            uimanager.instance.InteractionModeToggle(index);
        }
    }
}//end namespace