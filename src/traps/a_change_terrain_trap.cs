namespace Underworld
{
    public class a_change_terrain_trap : trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            var newtype = (trapObj.heading<<1) + (trapObj.quality & 0x1);
            if (newtype==0xF){newtype = 0xA;}

            TileInfo.ChangeTile(
                StartTileX: triggerX, StartTileY: triggerY, 
                newWallTexture: trapObj.owner, 
                newFloorTexture: trapObj.quality>>1,
                newHeight: trapObj.zpos>>3,
                newType: newtype,
                DimX: trapObj.xpos, DimY: trapObj.ypos);

            // var newFloorTexture = trapObj.quality >> 1;
            // var newTileType = trapObj.quality & 0x1; // codes seems to indicate heading has an impact but not in game+ (trapObj.heading<<1);
            // var newWallTexture = trapObj.owner;
            // if (newTileType==0xF){newTileType = 0xA;}
            // var X = triggerX;            
            // var EndX = triggerX + trapObj.xpos;
            // var EndY = triggerY + trapObj.ypos;
            // while (X <= EndX)
            // {
            //     var Y = triggerY;
            //     while (Y <= EndY)
            //     {
            //         if (UWTileMap.ValidTile(X, Y))
            //         {
            //             Debug.Print($"Changing terrain:{trapObj.index} Changing tile {X},{Y} to a {newTileType}");
            //             // UWTileMap.RemoveTile(X, Y);
            //             var tileToChange = UWTileMap.current_tilemap.Tiles[X, Y];
            //             tileToChange.tileType = (short)newTileType;
            //             if (_RES==GAME_UW2)
            //             {
            //                 tileToChange.floorTexture = (short)newFloorTexture; 
            //                 if (newWallTexture<63)
            //                 {
            //                     tileToChange.wallTexture = newWallTexture;
            //                 }                              
            //             }
            //             else
            //             {
            //                 if (newFloorTexture<0xB)
            //                 {
            //                     tileToChange.floorTexture =  (short)newFloorTexture;   
            //                 }
            //                 if (newWallTexture< 48)
            //                 {
            //                     tileToChange.wallTexture = newWallTexture;
            //                 }
            //             }
                                            
            //             var newHeight = trapObj.zpos>>3;
            //             if ((newHeight>=0) && (newHeight<=0xE))
            //             {
            //                 tileToChange.floorHeight = (short)newHeight;
            //             }
            //             for (int i = 0; i <= tileToChange.VisibleFaces.GetUpperBound(0); i++)
            //             {
            //                 tileToChange.VisibleFaces[i] = true;
            //             }
            //             if (_RES==GAME_UW2)
            //             {
                            
            //             }
            //             main.DoRedraw =true;
            //             tileToChange.Redraw = true;
            //             //UWTileMap.SetTileWallFacesUW(X,Y);
            //             //UWTileMap.SetTileMapWallFacesUW();
            //             //tileMapRender.RenderTile(tileMapRender.worldnode, X, Y, tileToChange);
            //         }
            //         Y++;
            //     }
            //     X++;
            // }
        }
    }//end class
}//end namespace