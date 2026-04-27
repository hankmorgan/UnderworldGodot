using Godot;

namespace Underworld
{
    /// <summary>
    /// For managing the two dragons on the UW1 Hud.
    /// </summary>
    public partial class uimanager : Node2D
    {
        static ImageTexture[] DragonFrames = new ImageTexture[32];


        [ExportGroup("Dragons")]
        //Health and manaflask
        [Export] public TextureRect[] DragonHeads = new TextureRect[2];
        [Export] public TextureRect[] DragonTails = new TextureRect[2];


        private void InitDragons()
        {
            if (UWClass._RES != UWClass.GAME_UW2)
            {
                //prepare the images by taking the images from dragons.gr and inserting them into a set of blank frames.
                var dragongr = new GRLoader(GRLoader.DRAGONS_GR, GRLoader.GRShaderMode.UIShader);
                
                var blankcanvas = Godot.Image.CreateEmpty(34,23,false, Godot.Image.Format.Rgba8);

                var img = ArtLoader.InsertImage(ToInsert:dragongr.LoadImageAt(30).GetImage(), background: blankcanvas, offX: -6, offY: 9);
                //img.SavePng("c:\\temp\\dragon1.png");
                
                img = ArtLoader.InsertImage(ToInsert:dragongr.LoadImageAt(31).GetImage(), background: blankcanvas, offX: -4, offY: 9);
                //img.SavePng("c:\\temp\\dragon2.png");
            }
        }     
     

    }//end class
}//end namespace