using Godot;

namespace Underworld
{

    public class Palette : UWClass
    {

        public byte[] red = new byte[256];
        public byte[] blue = new byte[256];
        public byte[] green = new byte[256];

        /// <summary>
        /// Returns the color for the specified palette index. 
        /// </summary>
        /// <returns>The at pixel.</returns>
        /// <param name="pixel">Pixel.</param>
        /// <param name="Alpha">If set to <c>true</c> alpha.</param>
        public Color ColorAtPixel(byte pixel, bool Alpha, bool useGreyScale)
        {
            byte alphabyte;
            if (Alpha == true)
            {
                if (pixel != 0) //Alpha
                {
                    alphabyte = 255;
                }
                else
                {
                    alphabyte = 0;
                }
            }
            else
            {
                alphabyte = 0;
            }

            if (useGreyScale)
            {
                return new Color(
                    g: pixel/255f,
                    r: pixel/255f,
                    b: pixel/255f,
                    a: alphabyte/255f                
                );
            }
            else
            {
                return new Color(
                        g: green[pixel]/255f,
                        r: red[pixel]/255f,
                        b: blue[pixel]/255f,
                        a: alphabyte/255f                
                    );
            }



            // uint rgba;
            // rgba = (uint)(red[pixel]<<24 | green[pixel]<<16 | blue[pixel]<<8 | alphabyte);
            // return new Color(rgba);
           // return new Color(red[pixel], green[pixel], blue[pixel], alpha);
        }

        public Color ColorAtPixelAlpha(byte pixel, byte alpha)
        {
            return new Color(red[pixel], green[pixel], blue[pixel], alpha);
        }


        public ImageTexture toImage()
        {
            int height = 1;
            int width = 256;
            byte[] imgData = new byte[height * width];
            for (int i = 0; i < imgData.GetUpperBound(0); i++)
            {
                imgData[i] = (byte)i;
            }
            var output= ArtLoader.Image(imgData, 0, width, height, "name here", this, true,false);
            return output;
        }


    }//end class
    
}//end namespace