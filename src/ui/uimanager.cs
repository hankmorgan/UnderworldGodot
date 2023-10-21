using Godot;

namespace Underworld
{
    public partial class uimanager:Node2D
    {
        public static uimanager instance;

        [Export]
        public Camera3D cam;

        [Export] public mouseCursor mousecursor;
        [Export] public CanvasLayer uw1UI;
        [Export] public CanvasLayer uw2UI;

        [Export] public TextureRect mainwindowUW1;
        [Export] public TextureRect mainwindowUW2;

        [Export] public Label messageScrollUW1;
        [Export] public Label messageScrollUW2;

        [Export] public TextureRect placeholderuw1;
        [Export] public TextureRect placeholderuw2;

        public static bool Fullscreen = false;
    
        public static GRLoader grCursors; //= new GRLoader(GRLoader.CURSORS_GR, GRLoader.GRShaderMode.UIShader);
		public static GRLoader grObjects; //= new GRLoader(GRLoader.OBJECTS_GR, GRLoader.GRShaderMode.UIShader);

        public static BytLoader byt;
        static uimanager()
        {
            grCursors = new GRLoader(GRLoader.CURSORS_GR, GRLoader.GRShaderMode.UIShader);
            grObjects = new GRLoader(GRLoader.OBJECTS_GR, GRLoader.GRShaderMode.UIShader);
            byt = new BytLoader();
        }

        public void InitUI()
        {
            instance = this;
            mousecursor.InitCursor(); 
            EnableDisable(placeholderuw1,false);
            EnableDisable(placeholderuw2,false);
            
            EnableDisable(uw1UI, UWClass._RES == UWClass.GAME_UW1);
            EnableDisable(uw2UI, UWClass._RES != UWClass.GAME_UW1);  

            //EnableDisable(mainwindowUW1, UWClass._RES == UWClass.GAME_UW1);
            //EnableDisable(mainwindowUW2, UWClass._RES != UWClass.GAME_UW1);  

            //EnableDisable(messageScrollUW1, UWClass._RES == UWClass.GAME_UW1);
            //EnableDisable(messageScrollUW2, UWClass._RES != UWClass.GAME_UW1);  

            var view = cam.GetViewport();
            switch(UWClass._RES)  
            {
                case UWClass.GAME_UW2:
                    mainwindowUW2.Texture = byt.LoadImageAt(BytLoader.UW2ThreeDWin_BYT,true);
                    if (!Fullscreen)
                    {
                        //view.Set("size", new Vector2(10,20));
                    }
                    break;
                default:
                    mainwindowUW1.Texture = byt.LoadImageAt(BytLoader.MAIN_BYT,true);
                    if (!Fullscreen)
                    {
                        //view.Set("size", new Vector2(10,20));
                    }
                    break;
            }

        } 

        static void EnableDisable (Control ctrl, bool state)
        {
            if (ctrl!=null)
            {
                ctrl.Visible=state;
            }
        }     

        static void EnableDisable (CanvasLayer ctrl, bool state)
        {
            if (ctrl!=null)
            {
                ctrl.Visible=state;
            }
        }   
    }
}