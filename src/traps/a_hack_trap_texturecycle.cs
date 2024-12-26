namespace Underworld
{
    /// <summary>
    /// Changes floor and wall textures across a range of tiles
    /// </summary>
    public class a_hack_trap_texturecycle : hack_trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            TextureCycler(
                triggerX: triggerX, 
                triggerY: triggerY, 
                X: trapObj.xpos<<1, 
                Y: trapObj.ypos<<1, 
                owner_startTexture: trapObj.owner, 
                zpos_maxTexture: trapObj.zpos, 
                heading_changewall: trapObj.heading);
        }

        /// <summary>
        /// Changes floor and wall textures. (Make tank frame rate, look into directly changing the tile model material)
        /// </summary>
        /// <param name="triggerX"></param>
        /// <param name="triggerY"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="owner_startTexture"></param>
        /// <param name="zpos_maxTexture"></param>
        /// <param name="heading_changewall"></param>
        static void TextureCycler(int triggerX, int triggerY, int X, int Y, int owner_startTexture, int zpos_maxTexture, int heading_changewall)        
        {
            int Xvar6 = triggerX + X;
            int Yvar8 = triggerY + Y;

            while (triggerX<=Xvar6)
            {
                var di = triggerY;
                while (di<=Yvar8)
                {
                    var tile = UWTileMap.current_tilemap.Tiles[triggerX, di];
                    var newfloor = (int)tile.floorTexture;
                    if (newfloor<=zpos_maxTexture)
                    {
                        if (newfloor>=owner_startTexture)
                        {
                            newfloor++;
                            if (newfloor>zpos_maxTexture)
                            {
                                newfloor = owner_startTexture;
                            }
                        }
                    }
                    var newwall = 63;
                    if (heading_changewall!=0)
                    {
                        newwall = tile.wallTexture;
                        if (newwall<=zpos_maxTexture)
                        {
                            if (newwall>=owner_startTexture)
                            {
                                newwall++;
                                if (newwall>zpos_maxTexture)
                                {
                                    newwall = owner_startTexture;
                                }
                            }
                        }
                    }


                    TileInfo.ChangeTile(
                        StartTileX: triggerX, 
                        StartTileY: di, 
                        newFloorTexture:newfloor,
                        newWallTexture:newwall);

                    di++;
                }//end whileY
                triggerX++;
            }//end whileX
        }

    }//end class
}//end namespace