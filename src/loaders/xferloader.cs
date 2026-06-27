using System.IO;
using Godot;

namespace Underworld
{

    /// <summary>
    /// Loads data from xfer.dat
    /// </summary>
    public class XferLoader : ArtLoader
    {
        public static ImageTexture GetXFERImageData()
        {
            var path = System.IO.Path.Combine(BasePath,"DATA","XFER.DAT");
            if (System.IO.Path.Exists(path))
            {
                //set up a temp palette so that the output image matches the colour indices
                var palette = new Palette();
                for (short i = 0; i<256; i++)
                {
                    palette.red[i] = (byte)i;
                    palette.blue[i] = (byte)i;
                    palette.green[i] = (byte)i;
                }
                var imgdata = File.ReadAllBytes(path);
                return Image(databuffer: imgdata, dataOffSet: 0, width: 256, height: 5, palette: palette, useAlphaChannel: false, useSingleRedChannel: true);
            }
            return null;
        }
    }
}