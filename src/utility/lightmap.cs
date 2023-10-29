using Godot;

namespace Underworld
{
    public class lightmap: Palette
    {
        public override ImageTexture toImage(int ColourBandSize = 1)
        {
            int ImageHeight=1, NoOfColours=256;  ColourBandSize=1;//force =1 
            var output = ArtLoader.Image(
                databuffer: this.red, 
                dataOffSet: 0, 
                width: NoOfColours * ColourBandSize, 
                height: ImageHeight, 
                palette: this, 
                useAlphaChannel: true, 
                useSingleRedChannel: true);
            return output;
        }
    } //end class
}//end namespace