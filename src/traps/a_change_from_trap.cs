using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Trap that changes a range of tiles from this trap to the trap linked to this.
    /// </summary>
    public class a_change_from_trap : trap
    {
        static int[] ChangeFromTrapDataVar26 = new int[] { 0x3F, 0xF, 0xA, 0xF };

        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            if (trapObj.link == 0) { return; }

            var RangeX_var6 = trapObj.xpos;
            var RangeY_var8 = trapObj.ypos;
            int[] TrapData_var12 = new int[4]; //= trapObj.quality; var12,var10,varE,varC
            TrapData_var12[0] = trapObj.quality;
            TrapData_var12[1] = ((trapObj.zpos & 0x10) >> 1) + trapObj.heading;
            TrapData_var12[2] = trapObj.owner;
            TrapData_var12[3] = trapObj.zpos & 0xF;

            var linkedObject = UWTileMap.current_tilemap.LevelObjects[trapObj.link];
            int[] Template_var1a = new int[4];//var1A, var18, var16, var14
            Template_var1a[0] = linkedObject.quality;
            Template_var1a[1] = ((linkedObject.zpos & 0x10) >> 1) + linkedObject.heading;
            Template_var1a[2] = linkedObject.owner;
            Template_var1a[3] = linkedObject.zpos & 0xF;

            int StartX_var2;
            int StartY_var4;

            if (RangeX_var6 != 0)
            {
                StartX_var2 = triggerX;
                StartY_var4 = triggerY;
                if (RangeY_var8 == 0)
                {
                    RangeY_var8 = RangeX_var6;
                }
            }
            else
            {
                StartX_var2 = 1;
                StartY_var4 = 1;
                RangeX_var6 = 0x3E;
                RangeY_var8 = 0x3E;
            }
            Debug.Print ($"Changing from Tiles {StartX_var2},{StartY_var4} to {StartX_var2 + RangeX_var6},{StartY_var4+RangeY_var8}");

            var TileX_varA = StartX_var2;
            while (StartX_var2 + RangeX_var6 > TileX_varA)
            {
                var TileY_di = StartY_var4;
                while (StartY_var4+RangeY_var8 > TileY_di)
                {   
                    var tile = UWTileMap.current_tilemap.Tiles[TileX_varA, TileY_di];
                    int[] NewTileData_var22 = new int[4];
                    NewTileData_var22[0] = tile.wallTexture;
                    NewTileData_var22[1] = tile.floorTexture;
                    NewTileData_var22[2] = tile.tileType;
                    NewTileData_var22[3] = tile.floorHeight;
                    var var2b = 1;
                    for (int si = 0; si<4; si++)
                    {
                        if (Template_var1a[si] < ChangeFromTrapDataVar26[si])
                        {
                            if ((TrapData_var12[si] != NewTileData_var22[si]) && (TrapData_var12[si] != ChangeFromTrapDataVar26[si]))
                            {
                                var2b = var2b & 0x0;
                            }
                            else
                            {
                                var2b = var2b & 0x1;
                            }
                            NewTileData_var22[si] = Template_var1a[si];
                        }
                    }
                    if (var2b!=0)
                    {
                        TileInfo.ChangeTile(
                            StartTileX: TileX_varA, 
                            StartTileY: TileY_di, 
                            newWallTexture: NewTileData_var22[0], 
                            newFloorTexture: NewTileData_var22[1], 
                            newType: NewTileData_var22[2],
                            newHeight: NewTileData_var22[3] 
                           );
                    }
                    TileY_di++;
                }
                TileX_varA++;
            }


        }
    }//end class
}//end namespace