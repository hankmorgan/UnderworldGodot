using System.Diagnostics;

namespace Underworld
{
    public class a_change_terrain_trap : trap
    {
        public static void Activate(uwObject triggerObj, uwObject trapObj)
        {
            
            var newFloorTexture = trapObj.quality >> 1;
            var newTileType = trapObj.quality & 0x1; // codes seems to indicate heading has an impact but not in game+ (trapObj.heading<<1);
            var newWallTexture = trapObj.owner;
            if (newTileType==0xF){newTileType = 0xA;}
            var X = triggerObj.quality;            
            var EndX = triggerObj.quality + trapObj.xpos;
            var EndY = triggerObj.owner + trapObj.ypos;
            while (X <= EndX)
            {
                var Y = triggerObj.owner;
                while (Y <= EndY)
                {
                    if (UWTileMap.ValidTile(X, Y))
                    {
                        Debug.Print($"Changing terrain:{trapObj.index} Changing tile {X},{Y} to a {newTileType}");
                        // UWTileMap.RemoveTile(X, Y);
                        var tileToChange = UWTileMap.current_tilemap.Tiles[X, Y];
                        tileToChange.tileType = (short)newTileType;
                        if (_RES==GAME_UW2)
                        {
                            tileToChange.floorTexture = (short)newFloorTexture; 
                            if (newWallTexture<63)
                            {
                                tileToChange.wallTexture = newWallTexture;
                            }                              
                        }
                        else
                        {
                            if (newFloorTexture<0xB)
                            {
                                tileToChange.floorTexture =  (short)newFloorTexture;   
                            }
                            if (newWallTexture< 48)
                            {
                                tileToChange.wallTexture = newWallTexture;
                            }
                        }
                                            
                        if (trapObj.zpos == 120)
                        {//If at this height use the trigger zpos for height instead.
                            tileToChange.floorHeight = (short)(triggerObj.zpos >> 2);
                        }
                        else
                        {
                            tileToChange.floorHeight = (short)(trapObj.zpos >> 2);
                        }
                        for (int i = 0; i <= tileToChange.VisibleFaces.GetUpperBound(0); i++)
                        {
                            tileToChange.VisibleFaces[i] = true;
                        }
                        if (_RES==GAME_UW2)
                        {
                            
                        }
                        trigger.DoRedraw =true;
                        tileToChange.Redraw = true;
                        //UWTileMap.SetTileWallFacesUW(X,Y);
                        //UWTileMap.SetTileMapWallFacesUW();
                        //tileMapRender.RenderTile(tileMapRender.worldnode, X, Y, tileToChange);
                    }
                    Y++;
                }
                X++;
            }
        }
    }//end class
}//end namespace