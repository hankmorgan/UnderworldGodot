using System.Diagnostics;

namespace Underworld
{
    public class an_oscillator_trap : trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            Debug.Print("THIS IS UNTESTED OscillateTrap!");
            int var22;
            int var38 = 0;
            var var36 = 0x3F;
            var var3A = 0xF;

            var tile =UWTileMap.current_tilemap.Tiles[triggerX, triggerY];
            var var23 = trapObj.xpos>>1;

            var var30_floortexture = 0xF;

            var var32 = 0x3F;
            var var34_newheight = 0x3F;
            var si = trapObj.xpos & 0x1;
            if (si ==0)
            {
                si --;
            }

            switch (var23)
            {
                case 0://ovr166_CEB
                    {
                        var22 = tile.floorHeight;
                        var36 = 0xF;
                        var34_newheight = tile.floorHeight + si;
                        var38 = var34_newheight;
                        if ((tile.tileType ==0) || (tile.tileType !=0 && si>=0))
                        {
                            if (tile.tileType==1)
                            {
                                if (var34_newheight == 0x10)
                                {
                                    var3A = 0;
                                }
                            }
                        }
                        else
                        {
                            var3A = 1;
                            var34_newheight = 0xF;
                        }
                        
                        if (trapObj.owner==0x10)
                        {
                            var36 = 0x3F;
                        }
                        break;    
                    }                    
                case 1://ovr166_D50
                    var22 = tile.floorTexture;
                    var30_floortexture = var22 + si;
                    var38 = var30_floortexture;
                    break;
                case 2://ovr166_D67
                    var22 = tile.wallTexture;
                    var32 = var22 + si;
                    var38 = var32;
                    break;
                case 3:
                    break;
            }//end switch

            if ((trapObj.quality <= var38) && (trapObj.owner>=var38))
            {
                Debug.Print($"updating {triggerX},{triggerY}");
                TileInfo.ChangeTile(
                    StartTileX: triggerX,
                    StartTileY: triggerY, 
                    newHeight: var34_newheight,   
                    newFloor: var30_floortexture,                  
                    HeightAdjustFlag: 4);
                // tile.floorHeight = (short)var34_newheight;
                // tile.floorTexture = (short)var30_floortexture;
            }

            if (si==1)
            {
                if (trapObj.owner == var38)
                {
                    if (trapObj.owner<=var36)
                    {
                        trapObj.xpos &= 0x6;
                    }
                }
            }
            else
            {
                if (trapObj.quality == var38)
                {
                    if (trapObj.owner < var36)
                    {
                        var tmp = trapObj.xpos & 0x6;
                        tmp++;
                        trapObj.xpos = (short)tmp;
                        if (playerdat.dungeon_level == 0x44)
                        {
                            SpecificOscillation();
                        }
                    }
                }
            }
        }

        static void SpecificOscillation()
        {
            var tile = UWTileMap.current_tilemap.Tiles[24,2];
            if (tile.floorHeight!=0)
            {
                for (int i=0; i<5;i++)
                {
                    var tileToChange = UWTileMap.current_tilemap.Tiles[24+i,2];
                    tileToChange.wallTexture = 0x17;
                    tileToChange.floorTexture = 4;
                    tileToChange.tileType = 1;
                    tileToChange.Redraw = true;
                    main.DoRedraw = true;
                }
                var anotherTileToChange = UWTileMap.current_tilemap.Tiles[2,0x15];
                anotherTileToChange.wallTexture = 0x14;
                anotherTileToChange.floorTexture = 4;
                anotherTileToChange.tileType = 1;
                anotherTileToChange.Redraw = true;
                main.DoRedraw = true;
            }
        }
    }//end class
}//end namespace