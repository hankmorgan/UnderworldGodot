using System.Diagnostics;

namespace Underworld
{
    public class a_bridge_trap : trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            var var30 = triggerX;
            var var32 = triggerY;
            var var36 = 0;
            var var38 = 0;
            int var34;

            trapObj.heading = (short)(trapObj.heading & 0x6);
            if (trapObj.heading < 4)
            {
                var34 = 1;  // (1<<1)-1
            }
            else
            {
                var34 = -1; //(0<<1) - 1;
            }

            if ((trapObj.heading & 2) == 0)
            {
                var38 = var34;
                
            }
            else
            {
                var36 = var34;
            }

            var34 = trapObj.quality;

            while (var34>0)
            {                
                switch (trapObj.owner>>4)
                {
                    case 1:
                        {
                            var foundBridge = CreateBridge(
                                tileX: var30, 
                                tileY: var32, 
                                heading: trapObj.heading, 
                                zpos: trapObj.zpos);
                            if (foundBridge != null)
                            {
                                if ((foundBridge.flags_full != (trapObj.owner & 0xF)) || (trapObj.instance == null))
                                {
                                    foundBridge.flags_full = (short)(trapObj.owner & 0xF);
                                    objectInstance.RedrawFull(foundBridge);//either redraw an existing bridge with a new texture or spawns a new one.
                                }
                            }
                            break;
                        }
                    case 2://remove
                        DestroyBridge(
                            tileX: var30, 
                            tileY: var32, 
                            heading: trapObj.heading, 
                            zpos: trapObj.zpos);
                        break;
                }



                //decrement and move to next tile
                var30 = var30 + var36;
                var32 = var32 + var38;
                var34--;
            }

        }

        static uwObject CreateBridge(int tileX, int tileY, int heading, int zpos)
        {
            Debug.Print ($"Create bridge at {tileX},{tileY}");
            if (UWTileMap.ValidTile(tileX,tileY))
            {
                var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                if (tile.indexObjectList!=0)
                {
                    var next = tile.indexObjectList;
                    while (next != 0)
                    {
                        var obj = UWTileMap.current_tilemap.LevelObjects[next];
                        if (obj.item_id == 0x164)
                        {
                            if ((obj.heading == heading) && (obj.zpos == zpos))
                            {   //found a matching bridge already.
                                return obj;
                            }
                        }                      
                        
                        next = obj.next;
                    }
                }
                //not found any matching bridge add a new bridge to the tile
                var newBridgeIndex = ObjectCreator.PrepareNewObject(0x164,ObjectFreeLists.ObjectListType.StaticList);
                var newBridge = UWTileMap.current_tilemap.LevelObjects[newBridgeIndex];                 
                newBridge.next = tile.indexObjectList;//insert into object list
                tile.indexObjectList = (short)newBridgeIndex; 
                newBridge.invis = 0;
                newBridge.xpos = 3; newBridge.ypos = 3;
                newBridge.zpos = (short)zpos; 
                newBridge.heading = (short)heading;
                newBridge.tileX = tileX; newBridge.tileY = tileY;
                return newBridge;
            }
            
            return null;
        }

        static void DestroyBridge(int tileX, int tileY, int heading, int zpos)
        {
            Debug.Print ($"Destroy bridge at {tileX},{tileY}");
            if (UWTileMap.ValidTile(tileX,tileY))
            {
                var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                if (tile.indexObjectList!=0)
                {
                    var next = tile.indexObjectList;
                    while (next != 0)
                    {
                        var obj = UWTileMap.current_tilemap.LevelObjects[next];
                        if (obj.item_id == 0x164)
                        {
                            if ((obj.heading == heading) && (obj.zpos == zpos))
                            {   //found a matching bridge.
                                ObjectRemover.DeleteObjectFromTile(
                                    tileX: tileX, 
                                    tileY: tileY, 
                                    indexToDelete: obj.index, 
                                    RemoveFromWorld: true);
                                return;
                            }
                        }                      
                        
                        next = obj.next;
                    }
                }
            }
        }
    }//end class
}//end namespace