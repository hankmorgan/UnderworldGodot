using System.Diagnostics;
using Godot;

namespace Underworld
{
    public partial class uimanager:Node2D
    {
        public static uimanager instance;

        [Export]
        public Camera3D cam;
        [Export] public Node3D freelook;

        // [Export] public SubViewportContainer uwviewport;
        // [Export] public SubViewport uwsubviewport;

        [Export] public mouseCursor mousecursor;
        [Export] public CanvasLayer uw1UI;
        [Export] public CanvasLayer uw2UI;

        [Export] public TextureRect mainwindowUW1;
        [Export] public TextureRect mainwindowUW2;

        [Export] public Label messageScrollUW1;
        [Export] public Label messageScrollUW2;

        [Export] public TextureRect placeholderuw1;
        [Export] public TextureRect placeholderuw2;

        //Array to store the interaction mode mo
        [Export] public Godot.TextureButton[] InteractionButtonsUW1 = new Godot.TextureButton[6];
        //[Export] public Godot.TextureButton[] InteractionButtonsUW2 = new Godot.TextureButton[6];
 
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
            
            switch(UWClass._RES)  
            {
                case UWClass.GAME_UW2:
                    mainwindowUW2.Texture = byt.LoadImageAt(BytLoader.UW2ThreeDWin_BYT,true);
                    if (!Fullscreen)
                    {
                        // uwviewport.SetSize(new Vector2(840f,512f));
                        // uwviewport.Position = new Vector2(62f,62f);
                        // uwsubviewport.Size = new Vector2I(840,512);
                    }
                    break;
                default:
                    mainwindowUW1.Texture = byt.LoadImageAt(BytLoader.MAIN_BYT,true);
                    if (!Fullscreen)
                    {
                        // uwviewport.SetSize(new Vector2(700f,456f));
                        // uwviewport.Position = new Vector2(200f,72f);
                        // uwsubviewport.Size = new Vector2I(700,456);
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


        /// <summary>
        /// The Options button is pressed
        /// </summary>
        /// <param name="viewPort"></param>
        /// <param name="inputEvent"></param>
        public void OnOptionsButtonUW1_Pressed(InputEvent inputEvent)
        {
        int toggle=0;
           for (int i=0; i<instance.InteractionButtonsUW1.GetUpperBound(0);i++)
           {
                if(i==toggle)
                {
                    instance.InteractionButtonsUW1[i].SetPressedNoSignal(false);
                }
           }
        } 

        
    }
}