namespace Underworld
{
    /// <summary>
    /// Trap which resets the collapsing platforms in Scintillus academy
    /// </summary>
    public class a_hack_trap_platformreset : hack_trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            var di_x = triggerX;
            while (di_x<=0x3F)
            {
                var si_y = triggerY;
                while (si_y<=0x3F)
                {
                    var tile = UWTileMap.current_tilemap.Tiles[di_x, si_y];
                    if (tile.tileType!=0)
                    {//tile is not solid
                        var pressureTrigger = objectsearch.FindMatchInObjectListChainNextObjectsOnly(
                            ListHeadIndex: tile.indexObjectList, 
                            majorclass: 6, 
                            minorclass: 2, 
                            classindex: 4, 
                            objList: UWTileMap.current_tilemap.LevelObjects);
                        if (pressureTrigger==null)
                        {//find an alternative trigger
                            pressureTrigger = objectsearch.FindMatchInObjectListChainNextObjectsOnly(
                                ListHeadIndex: tile.indexObjectList, 
                                majorclass: 6, 
                                minorclass: 3, 
                                classindex: 4, 
                                objList: UWTileMap.current_tilemap.LevelObjects);
                        }
                        if (pressureTrigger!=null)
                        {
                            if (pressureTrigger.zpos>>3 != tile.floorHeight)
                            {//found a pressure trigger that is not at the floor height
                                var x = pressureTrigger.quality - di_x;
                                var y = pressureTrigger.owner - si_y;

                                int newFloor;
                                if (trapObj.owner==0)
                                {
                                    newFloor=0x1F;//no texture change
                                }
                                else
                                {
                                    if (x<0 || y<0)
                                    {
                                        newFloor=1;
                                    }
                                    else
                                    {
                                        newFloor=0;
                                    }
                                    newFloor = newFloor<<1;
                                    if (y == 0)
                                    {
                                        newFloor = newFloor | 1;
                                    }
                                }

                                TileInfo.ChangeTile(
                                    StartTileX: di_x,
                                    StartTileY:si_y,
                                    newFloorTexture: newFloor,
                                    newHeight: pressureTrigger.zpos>>3);

                            }
                        }
                    }
                    else
                    {//tile is solid, stop on axis.
                        if (si_y==triggerY)
                        {
                            di_x=0x3F;
                        }
                        si_y = 0x3F;
                    }
                    si_y++;
                }//end while Y
                di_x++;
            }//end while X
        }//end activate
    }//end class
}//end namespace