using Godot;

namespace Underworld
{
    public class ArtLoader : Loader
    {
        /// <summary>
        /// Load an approximation of xfer.dat transparency
        /// </summary>
        //public bool xfer;  //TODO this should mean that the file uses a shader that has xfer transparency applied

        public const byte BitMapHeaderSize = 28;

        /// <summary>
        /// The complete image file 
        /// </summary>
        protected byte[] ImageFileData;

        /// <summary>
        /// The palette no to use with this file.
        /// </summary>
        public short PaletteNo = 0;

        /// <summary>
        /// Instructs the image() function to crop out based on pixel 0
        /// </summary>
        public bool UseCropping = false;


        public const float SpriteScale = 0.024f;  //height of 1px of a sprite

        public const float NPCSpriteScale = 0.012f;

        /// <summary>
        /// Loads the image file into the buffer
        /// </summary>
        /// <returns><c>true</c>, if image file was loaded, <c>false</c> otherwise.</returns>
        public virtual bool LoadImageFile()
        {
            if (ReadStreamFile(filePath, out ImageFileData))
            {//data read
                DataLoaded = true;
            }
            else
            {
                DataLoaded = false;
            }
            return DataLoaded;
        }

        /// <summary>
        /// Loads the image at index.
        /// </summary>
        /// <returns>The <see cref="UnityEngine.Texture2D"/>.</returns>
        /// <param name="index">Index.</param>
        public virtual ImageTexture LoadImageAt(int index)
        {
            return null; // new ImageTexture() Texture2D(1, 1);
        }

        /// <summary>
        /// Loads the image at index.
        /// </summary>
        /// <returns>The <see cref="UnityEngine.Texture2D"/>.</returns>
        /// <param name="index">Index.</param>
        public virtual ImageTexture LoadImageAt(int index, bool Alpha)
        {
            return null;// new Texture2D(1, 1);
        }


        /// <summary>
        /// Generates the image from the specified data buffer position
        /// </summary>
        /// <param name="databuffer">Databuffer.</param>
        /// <param name="dataOffSet">Data off set.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="imageName">Image name.</param>
        /// <param name="palette">Pal.</param>
        /// <param name="useAlphaChannel">If set to <c>true</c> alpha.</param>
        public static ImageTexture Image(
            byte[] databuffer,
            long dataOffSet,
            int width, int height,
            Palette palette,
            bool useAlphaChannel,
            bool useSingleRedChannel,
            bool crop)
        {
            Godot.Image.Format imgformat;
            if (useSingleRedChannel)
            {
                imgformat = Godot.Image.Format.R8;
            }
            else
            {
                if (useAlphaChannel)
                {
                    imgformat = Godot.Image.Format.Rgba8;
                }
                else
                {
                    imgformat = Godot.Image.Format.Rgb8;
                }
            }

            var img = Godot.Image.Create(width, height, false, imgformat);
            for (int iRow = 0; iRow < height; iRow++)
            {
                int iCol = 0;
                for (int j = iRow * width; j < (iRow * width) + width; j++)
                {
                    byte pixel = (byte)getAt(databuffer, dataOffSet + j, 8);
                    img.SetPixel(iCol, iRow, palette.ColorAtIndex(pixel, useAlphaChannel, useSingleRedChannel));
                    iCol++;
                }
            }

            if (crop)
            {
                var bound = GetBoundingBox(databuffer, (int)dataOffSet, width, height);
                return CropImage(img, bound);
            }
            else
            {
                var tex = new ImageTexture();
                tex.SetImage(img);
                return tex;
            }
        }


        /// <summary>
        /// Creates a collison mask around the image data.
        /// </summary>
        /// <param name="databuffer"></param>
        /// <param name="dataOffSet"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="crop"></param>
        /// <returns></returns>
        public static Godot.Collections.Array<Godot.Vector2[]> CreateCollisionMask(
            byte[] databuffer,
            long dataOffSet,
            int width, int height,
            bool crop = false
           )
        {

            var bmp = new Bitmap();
            bmp.Create(new Vector2I(width, height));

            for (int iRow = 0; iRow < height; iRow++)
            {
                int iCol = 0;
                for (int j = iRow * width; j < (iRow * width) + width; j++)
                {
                    byte pixel = (byte)getAt(databuffer, dataOffSet + j, 8);
                    bmp.SetBit(iCol, iRow, pixel != 0);
                    iCol++;
                }
            }
            if (crop)
            {
                var bound = GetBoundingBox(databuffer, (int)dataOffSet, width, height);
                return bmp.OpaqueToPolygons(bound);
            }
            else
            {
                return bmp.OpaqueToPolygons(new Rect2I(0, 0, width, height));
            }




            // if (crop)
            // {

            //     return CropImage(img, bound);
            // }
            // else
            // {
            //     var tex = new ImageTexture();
            //     tex.SetImage(img);
            //     return tex;
            // }
        }


        public static Rect2I GetBoundingBox(byte[] buf, int dataoffset, int width, int height)
        {//https://stackoverflow.com/questions/32191887/creating-a-bounding-box-in-image-with-transparency

            //search upper bound
            bool found = false;
            int x0 = 0; int x1 = width; int y1 = 0; int y0 = height;
            for (int row = 0; row < height && !found; row++) //row
            {
                for (int col = 0; col < width && !found; col++) //column
                {
                    int idx = dataoffset + (row * width + col);
                    if (!(buf[idx] == 0)) //not transparent
                    {
                        //BoundingBox.top = row;
                        y0 = row;
                        found = true;
                    }
                }
            }

            //search lower bound
            found = false;
            for (int row = height - 1; row >= 0 && !found; row--) //row
            {
                for (int col = width - 1; col >= 0 && !found; col--) //column
                {
                    int idx = dataoffset + (row * width + col);
                    if (!(buf[idx] == 0)) //not transparent           
                    {
                        //BoundingBox.bottom = row;
                        y1 = row;
                        found = true;
                    }
                }
            }


            //search left bound
            found = false;
            for (int col = 0; col < width && !found; col++) //row
            {
                for (int row = 0; row < height && !found; row++) //column
                {
                    int idx = dataoffset + (row * width + col);
                    if (!(buf[idx] == 0)) //not transparent           
                    {
                        //BoundingBox.left = col;
                        x0 = col;
                        found = true;
                    }
                }
            }


            //search right bound
            found = false;
            for (int col = width - 1; col >= 0 && !found; col--) //row
            {
                for (int row = height - 1; row >= 0 && !found; row--) //column
                {
                    int idx = dataoffset + (row * width + col);
                    if (!(buf[idx] == 0)) //not transparent           
                    {
                        //BoundingBox.right = col;
                        x1 = col;
                        found = true;
                    }
                }
            }
            return new Rect2I(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
        }


        public static ImageTexture CropImage(Image src, Rect2I rect)
        {
            var img = Godot.Image.Create(rect.Size.X, rect.Size.Y, false, src.GetFormat());
            img.BlitRect(src, rect, Vector2I.Zero);
            var tex = new ImageTexture();
            tex.SetImage(img);
            //img.SavePng("c:\\temp\\img\\" + System.Guid.NewGuid() + ".png");
            return tex;
        }

    }//class artloader

} //namespace