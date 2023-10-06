using Godot;

namespace Underworld
{
    public class lightmap: Palette
    {
        public override ImageTexture toImage(int ColourBandSize = 1)
        {
            int ImageHeight=1, NoOfColours=256;  ColourBandSize=1;//force =1 
            // byte[] imgData=new byte[256];
            // for (int i=0;i<imgData.GetUpperBound(0);i++)
            // {
            //     imgData[i] = this.red[i];
            // }
            //BuildPaletteImgData(ColourBandSize, out ImageHeight, out NoOfColours, out imgData);
            var output = ArtLoader.Image(this.red, 0, NoOfColours * ColourBandSize, ImageHeight, "name here", this, true, true);
            return output;
        }
    } //end class
}//end namespace