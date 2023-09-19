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
        public Color ColorAtPixel(byte pixel, bool Alpha)
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
            
            uint rgba;
            rgba = (uint)(red[pixel]<<24 | green[pixel]<<16 | blue[pixel]<<8 | alphabyte);
            return new Color(rgba);

           // return new Color(red[pixel], green[pixel], blue[pixel], alpha);
        }

        public Color ColorAtPixelAlpha(byte pixel, byte alpha)
        {
            return new Color(red[pixel], green[pixel], blue[pixel], alpha);
        }



    }//end class
    
}//end namespace