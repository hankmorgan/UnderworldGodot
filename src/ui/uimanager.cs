using Godot;

namespace Underworld
{
    public partial class uimanager:Node2D
    {
        [Export] public mouseCursor mousecursor;

        public GRLoader grCursors; //= new GRLoader(GRLoader.CURSORS_GR, GRLoader.GRShaderMode.UIShader);
		public GRLoader grObjects; //= new GRLoader(GRLoader.OBJECTS_GR, GRLoader.GRShaderMode.UIShader);

        public void InitUI()
        {
            grCursors = new GRLoader(GRLoader.CURSORS_GR, GRLoader.GRShaderMode.UIShader);
            grObjects = new GRLoader(GRLoader.OBJECTS_GR, GRLoader.GRShaderMode.UIShader);

            mousecursor.InitCursor();
        }
    }
}