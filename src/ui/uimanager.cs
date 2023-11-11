using System.Diagnostics;
using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        public static uimanager instance;


        public enum InteractionModes
        {
            ModeOptions = 0,
            ModeTalk = 1,
            ModePickup = 2,
            ModeLook = 3,
            ModeAttack = 4,
            ModeUse = 5
        };

        public static InteractionModes InteractionMode = InteractionModes.ModeUse;

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
        [Export] public Godot.TextureButton[] InteractionButtonsUW2 = new Godot.TextureButton[6];

        public static bool Fullscreen = false;

        public static GRLoader grCursors;
        public static GRLoader grObjects;

        public static GRLoader grLfti;

        public static GRLoader grOptBtns;
        private ImageTexture[] UW2OptBtnsOff;
        private ImageTexture[] UW2OptBtnsOn;

        public static GRLoader grBody;
        public static GRLoader grArmour_F;
        public static GRLoader grArmour_M;

        [Export] public TextureRect BodyUW1;
        [Export] public TextureRect HelmUW1;
        [Export] public TextureRect ArmourUW1;
        [Export] public TextureRect LeggingsUW1;
        [Export] public TextureRect BootsUW1;
        [Export] public TextureRect GlovesUW1;
        public TextureRect RingLeftUW1;
        public TextureRect RingRightUW2;



        public static BytLoader byt;


        static uimanager()
        {

        }

        public void InitUI()
        {
            instance = this;

            grCursors = new GRLoader(GRLoader.CURSORS_GR, GRLoader.GRShaderMode.UIShader);
            grObjects = new GRLoader(GRLoader.OBJECTS_GR, GRLoader.GRShaderMode.UIShader);
            grLfti = new GRLoader(GRLoader.LFTI_GR, GRLoader.GRShaderMode.UIShader);
            grArmour_F = new GRLoader(GRLoader.ARMOR_F_GR, GRLoader.GRShaderMode.UIShader);
            grArmour_M = new GRLoader(GRLoader.ARMOR_M_GR, GRLoader.GRShaderMode.UIShader);

            // for (int i=0; i<grArmour_F.ImageCache.GetUpperBound(0);i++)
            // {
            //    grArmour_F.LoadImageAt(i).GetImage().SavePng(System.IO.Path.Combine("c:\\temp",$"armourf_{i}.png"));
            // }
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                UW2OptBtnsOff = new ImageTexture[6];
                UW2OptBtnsOn = new ImageTexture[6];
                grOptBtns = new GRLoader(GRLoader.OPTBTNS_GR, GRLoader.GRShaderMode.UIShader);
                var Off = grOptBtns.LoadImageAt(0).GetImage();
                var On = grOptBtns.LoadImageAt(1).GetImage();
                UW2OptBtnsOff[4] = ArtLoader.CropImage(Off, new Rect2I(0, 0, 25, 14)); //attack button off
                UW2OptBtnsOn[4] = ArtLoader.CropImage(On, new Rect2I(0, 0, 25, 14)); //attack button on

                UW2OptBtnsOff[5] = ArtLoader.CropImage(Off, new Rect2I(26, 0, 25, 14)); //use button off
                UW2OptBtnsOn[5] = ArtLoader.CropImage(On, new Rect2I(26, 0, 25, 14)); //use button on

                UW2OptBtnsOff[2] = ArtLoader.CropImage(Off, new Rect2I(52, 0, 25, 14)); //pickup button off
                UW2OptBtnsOn[2] = ArtLoader.CropImage(On, new Rect2I(52, 0, 25, 14)); //pickup button on

                UW2OptBtnsOff[1] = ArtLoader.CropImage(Off, new Rect2I(0, 15, 25, 14)); //talk button off
                UW2OptBtnsOn[1] = ArtLoader.CropImage(On, new Rect2I(0, 15, 25, 14)); //talk button on

                UW2OptBtnsOff[3] = ArtLoader.CropImage(Off, new Rect2I(26, 15, 25, 14)); //look button off
                UW2OptBtnsOn[3] = ArtLoader.CropImage(On, new Rect2I(26, 15, 25, 14)); //look button on

                UW2OptBtnsOff[0] = ArtLoader.CropImage(Off, new Rect2I(52, 15, 25, 14)); //options button off
                UW2OptBtnsOn[0] = ArtLoader.CropImage(On, new Rect2I(52, 15, 25, 14)); //option button on

            }
            byt = new BytLoader();

            mousecursor.InitCursor();
            EnableDisable(placeholderuw1, false);
            EnableDisable(placeholderuw2, false);

            EnableDisable(uw1UI, UWClass._RES == UWClass.GAME_UW1);
            EnableDisable(uw2UI, UWClass._RES != UWClass.GAME_UW1);

            //EnableDisable(mainwindowUW1, UWClass._RES == UWClass.GAME_UW1);
            //EnableDisable(mainwindowUW2, UWClass._RES != UWClass.GAME_UW1);  

            //EnableDisable(messageScrollUW1, UWClass._RES == UWClass.GAME_UW1);
            //EnableDisable(messageScrollUW2, UWClass._RES != UWClass.GAME_UW1);  

            switch (UWClass._RES)
            {
                case UWClass.GAME_UW2:
                    mainwindowUW2.Texture = byt.LoadImageAt(BytLoader.UW2ThreeDWin_BYT, true);
                    if (!Fullscreen)
                    {
                        // uwviewport.SetSize(new Vector2(840f,512f));
                        // uwviewport.Position = new Vector2(62f,62f);
                        // uwsubviewport.Size = new Vector2I(840,512);
                    }

                    for (int i = 0; i <= InteractionButtonsUW2.GetUpperBound(0); i++)
                    {
                        InteractionButtonsUW2[i].TexturePressed = UW2OptBtnsOn[i]; // grLfti.LoadImageAt(i*2 + 1,false);
                        InteractionButtonsUW2[i].TextureNormal = UW2OptBtnsOff[i]; //grLfti.LoadImageAt(i*2,false);  
                        InteractionButtonsUW2[i].SetPressedNoSignal((i == (int)InteractionMode));
                    }

                    break;
                default:
                    mainwindowUW1.Texture = byt.LoadImageAt(BytLoader.MAIN_BYT, true);
                    if (!Fullscreen)
                    {
                        // uwviewport.SetSize(new Vector2(700f,456f));
                        // uwviewport.Position = new Vector2(200f,72f);
                        // uwsubviewport.Size = new Vector2I(700,456);
                    }
                    //grLfti.ExportImages("c:\\temp\\lfti\\");
                    for (int i = 0; i <= InteractionButtonsUW1.GetUpperBound(0); i++)
                    {
                        InteractionButtonsUW1[i].TexturePressed = grLfti.LoadImageAt(i * 2 + 1, false);
                        InteractionButtonsUW1[i].TextureNormal = grLfti.LoadImageAt(i * 2, false);
                        InteractionButtonsUW1[i].SetPressedNoSignal((i == (int)InteractionMode));
                    }
                    break;
            }

        }

        static void EnableDisable(Control ctrl, bool state)
        {
            if (ctrl != null)
            {
                ctrl.Visible = state;
            }
        }

        static void EnableDisable(CanvasLayer ctrl, bool state)
        {
            if (ctrl != null)
            {
                ctrl.Visible = state;
            }
        }


        /// <summary>
        /// The Options button is pressed
        /// </summary>
        /// <param name="viewPort"></param>
        /// <param name="inputEvent"></param>
        //event delegates

        public void InteractionModeToggle(InteractionModes index)
        {
            Debug.Print($"Press {index}");

            if (UWClass._RES == UWClass.GAME_UW2)
            {

                for (int i = 0; i <= instance.InteractionButtonsUW2.GetUpperBound(0); i++)
                {
                    InteractionButtonsUW2[i].SetPressedNoSignal(i == (int)(index));
                    if (i == (int)(index))
                    {
                        InteractionMode = index;
                    }
                }
            }
            else
            {
                for (int i = 0; i <= instance.InteractionButtonsUW1.GetUpperBound(0); i++)
                {
                    InteractionButtonsUW1[i].SetPressedNoSignal(i == (int)(index));
                    if (i == (int)(index))
                    {
                        InteractionMode = index;
                    }
                }
            }
        }

        public static void SetBody(int body, int isFemale)
        {
            if (grBody==null)
            {
                grBody = new GRLoader(GRLoader.BODIES_GR,GRLoader.GRShaderMode.UIShader);
            }
            switch (UWClass._RES)
            {
                case UWClass.GAME_UW2:
                    return;
                default:
                   instance.BodyUW1.Texture = grBody.LoadImageAt(body + (5*isFemale));
                   return;
            }
        }

        public static void SetHelm(bool isFemale, int SpriteNo=-1)
        {              
            if (SpriteNo == -1)
            { //clear the slot
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        return;
                    default:
                        instance.HelmUW1.Texture = null;
                    return;
                }
            }
            else
            {
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        return;
                    default:
                        instance.HelmUW1.Texture = grArmour(isFemale).LoadImageAt(SpriteNo);
                    return;
                }
            }
        }

        public static void SetArmour(int spriteNo=-1)
        {            
            if (spriteNo == -1)
            { //clear the slot
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        return;
                    default:
                        instance.ArmourUW1.Texture = null;
                    return;
                }
            }
            else
            {
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        return;
                    default:
                        instance.ArmourUW1.Texture = null;
                    return;
                }
            }
        }

        public static void SetLeggings(int spriteNo=-1)
        {            
            if (spriteNo == -1)
            { //clear the slot
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        return;
                    default:
                        instance.LeggingsUW1.Texture = null;
                    return;
                }
            }
            else
            {
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        return;
                    default:
                        instance.LeggingsUW1.Texture = null;
                    return;
                }
            }
        }

        public static void SetBoots(int spriteNo=-1)
        {            
            if (spriteNo == -1)
            { //clear the slot
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        return;
                    default:
                        instance.BootsUW1.Texture = null;
                    return;
                }
            }
            else
            {
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        return;
                    default:
                        instance.BootsUW1.Texture = null;
                    return;
                }
            }
        }

        public static void SetGloves(int spriteNo=-1)
        {            
            if (spriteNo == -1)
            { //clear the slot
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        return;
                    default:
                        instance.GlovesUW1.Texture = null;
                    return;
                }
            }
            else
            {
                switch (UWClass._RES)
                {
                    case UWClass.GAME_UW2:
                        return;
                    default:
                        instance.GlovesUW1.Texture = null;
                    return;
                }
            }
        }

        public static GRLoader grArmour(bool isFemale)
        {
            if (isFemale)
            {
                if(grArmour_F==null)
                {
                    grArmour_F = new GRLoader(GRLoader.ARMOR_F_GR,GRLoader.GRShaderMode.UIShader);
                }
                return grArmour_F;
            }
            else
            {
                if(grArmour_M==null)
                {
                    grArmour_M = new GRLoader(GRLoader.ARMOR_M_GR,GRLoader.GRShaderMode.UIShader);
                }
                return grArmour_M;
            }
        }

    } //end class
}   //end namespace