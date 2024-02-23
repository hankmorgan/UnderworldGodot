using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {

        [ExportGroup("Automap")]
        //Automap
        [Export] public Panel AutomapPanel;
        [Export] public TextureRect AutomapImage;
        public static bool InAutomap = false;

        /// <summary>
        /// Closes the automap window
        /// </summary>
        /// <param name="event"></param>
        private void CloseAutomap(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                EnableDisable(AutomapPanel, false);
                InAutomap = false;
            }
        }
    }//end class
}//end namespace