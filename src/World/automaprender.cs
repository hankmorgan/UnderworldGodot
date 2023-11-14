using Godot;
namespace Underworld
{
    public class AutomapRender : UWClass
    {
        const int TileSize = 10;

        //Colours
        public static Color[] Background = new Color[1];

        public static ImageTexture MapImage(int levelno)
        {
            var output = Image.Create(64 * TileSize, 64 * TileSize, false, Image.Format.Rgba8);

            //Init the tile map as a blank map first
            for (int i = 0; i < TileMap.TileMapSizeX; i++)
            {
                for (int j = TileMap.TileMapSizeY; j > 0; j--)
                {                    
                    DrawSolidTile(output, i, j, TileSize, TileSize, Background);
                }
            }
            ///TODO MORE



            return null;
        }

        private static void DrawSolidTile(Image OutputTile, int TileX, int TileY, int TileWidth, int TileHeight, Color[] InputColour)
        {
            for (int i = 0; i < TileWidth; i++)
            {
                for (int j = 0; j < TileHeight; j++)
                {
                    OutputTile.SetPixel(i + TileX * TileWidth, j + TileY * TileHeight, PickColour(InputColour));
                }
            }
        }





        /// <summary>
        /// Picks a random colour from the array of colours
        /// </summary>
        /// <returns>The colour selected</returns>
        /// <param name="Selection">Selection.</param>
        static Color PickColour(Color[] Selection)
        {
            var r = new System.Random();            
            return Selection[r.Next(0, Selection.GetUpperBound(0) + 1)];
        }


    } //end class
}//end namespace