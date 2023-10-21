using Godot;

namespace Underworld
{
    /// <summary>
    /// Base class for ui texture rects.
    /// </summary>
    public partial class uwTextureRect : TextureRect
    {
        //Reference to the UI manager
        [Export] public uimanager uwUI;

    }
}//end namespace