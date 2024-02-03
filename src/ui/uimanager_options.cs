using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        private ImageTexture[] UW2OptBtnsOff;
		private ImageTexture[] UW2OptBtnsOn;
        private void InitOptions()
        {
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
        }        
    }//end class
}//end namespace