using Godot;

namespace Underworld
{
    public partial class InteractionModeButton : TextureButton
    {

        [Export]
        public uimanager.InteractionModes index;

        public override void _Toggled(bool buttonPressed)
        {
            if (!ConversationVM.InConversation)
            {
                uimanager.InteractionModeToggle(index);
            }
        }
    }
}//end namespace