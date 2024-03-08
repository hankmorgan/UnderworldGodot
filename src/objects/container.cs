using System.Collections.Generic;
using System.Diagnostics;

namespace Underworld
{
    public class container : objectInstance
    {    
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (WorldObject)
            {
                SpillWorldContainer(obj);
                return true;
            }
            else
            {
                //container used in inventory. Browse into it.
                if (obj.classindex <= 0xB)
                {
                    //set to opened version by setting bit 0 to 1.
                    obj.item_id |= 0x1;
                }
                uimanager.OpenedContainerIndex = obj.index;
                uimanager.SetOpenedContainer(obj.index, uwObject.GetObjectSprite(obj));
                uimanager.BackPackStart = 0;
                DisplayContainerObjects(obj);
                return true;
            }
        }


        /// <summary>
        /// Splills the contents of a container onto the tile
        /// </summary>
        /// <param name="obj"></param>
        public static void SpillWorldContainer(uwObject obj)
        {
            //container used in the world
            if (UWTileMap.ValidTile(obj.tileX, obj.tileY))
            {
                var tile = UWTileMap.current_tilemap.Tiles[obj.tileX, obj.tileY];
                if (tile != null)
                {
                    //add the contents of the container to the tile.
                    if ((obj.majorclass == 2) && (obj.minorclass == 0))
                    {
                        if ((obj.classindex & 1) == 0)
                        {
                            obj.item_id |= 0x1;// set it to an opened version.
                            if (obj.instance != null)
                            {
                                if (obj.instance.uwnode != null)
                                {
                                    var nd = (uwMeshInstance3D)obj.instance.uwnode.GetChild(0);
                                    nd.Mesh.SurfaceSetMaterial(0, ObjectCreator.grObjects.GetMaterial(obj.item_id));
                                }
                            }
                        }
                    }
                    int nextobj = obj.link;
                    obj.link = 0;
                    while (nextobj != 0)
                    {
                        var objToSpill = UWTileMap.current_tilemap.LevelObjects[nextobj];
                        Debug.Print($"Spilling {objToSpill.a_name}");
                        objToSpill.tileX = obj.tileX;
                        objToSpill.tileY = obj.tileY;
                        GetRandomXYZForTile(tile, out int newxpos, out int newypos, out int newzpos);
                        objToSpill.xpos = (short)newxpos;//obj.xpos;
                        objToSpill.ypos = (short)newypos;///obj.ypos;
                        objToSpill.zpos = (short)newzpos; //obj.zpos;
                        objToSpill.owner = 0; //clear owner
                        ObjectCreator.RenderObject(objToSpill, UWTileMap.current_tilemap);
                        nextobj = objToSpill.next;
                        //insert to object list
                        objToSpill.next = tile.indexObjectList;
                        tile.indexObjectList = objToSpill.index;
                    }
                }
            }
        }


        /// <summary>
        /// Displays the range (start to start+count) of container objects on the paper doll backback slots
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        public static void DisplayContainerObjects(uwObject obj, int start = 0, int count = 8)
        {
            int occupiedslots = 0;
            var objects = GetObjects(
                ContainerIndex: obj.index, 
                objList: playerdat.InventoryObjects,
                OccupiedSlots: out occupiedslots,
                start:start,
                count:count
                );

            if (objects==null)
            {
                //empty container
                for (int o=0;o<8;o++)
                {
                    uimanager.SetBackPackIndex(o, null);
                }
            }
            else
            {
                for (int o = 0; o <= objects.GetUpperBound(0); o++)
                {
                    if (objects[o] != -1)
                    {
                        //render object at this slot
                        var objFound = playerdat.InventoryObjects[objects[o]];
                        uimanager.SetBackPackIndex(o, objFound);
                    }
                    else
                    {
                        uimanager.SetBackPackIndex(o, null);
                    }
                }
            }

            uimanager.EnableDisable(uimanager.instance.ArrowUp, start !=0);
            uimanager.EnableDisable(uimanager.instance.ArrowDown, occupiedslots == 8);

            uimanager.UpdateInventoryDisplay();
        }

        /// <summary>
        /// Closes the container on the paperdoll
        /// </summary>
        /// <param name="obj"></param>
        public static int Close(int index, uwObject[] objList)
        {
            var obj = objList[index];
            if (obj == null) { return -1; }
            if (obj.classindex <= 0xB)
                {//return to closed version of the container.
                    obj.item_id &= 0x1fe;                        
                }
            //Check the paperdoll
            for (int p = 0; p < 19; p++)
            {
                if (playerdat.GetInventorySlotListHead(p) == obj.index)
                { //object is on the paperdoll. I can close and return to the top level
                    uimanager.RefreshSlot(p);
                    uimanager.OpenedContainerIndex = -1;//clear slot graphics
                    uimanager.SetOpenedContainer(obj.index, -1);
                    //Draw the paperdoll inventory.
                    for (int i = 0; i < 8; i++)
                    {
                        uimanager.SetBackPackArt(i, uwObject.GetObjectSprite(playerdat.BackPackObject(i)), uwObject.GetObjectQuantity(playerdat.BackPackObject(i)));
                        uimanager.SetBackPackIndex(i, playerdat.BackPackObject(i));
                    }
                    uimanager.EnableDisable(uimanager.instance.ArrowUp, false);
                    uimanager.EnableDisable(uimanager.instance.ArrowDown, false);
                    return -1;
                }
            }
            foreach (var objToCheck in playerdat.InventoryObjects)
            {//if this far down then I need to find the container that the closing container sits in
                if (objToCheck!=null)
                {
                    var result = objectsearch.GetContainingObject(
                        ListHead:objToCheck.index, 
                        ToFind: uimanager.OpenedContainerIndex,
                        objList: playerdat.InventoryObjects);
                    if (result!=-1)
                    {//container found. Browse into it by using it
                        Use(objList[result],false);
                        return result;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns an array of object indices at the specified offset and length
        /// </summary>
        /// <param name="ContainerIndex"></param>
        /// <param name="objList"></param>
        /// <param name="OccupiedSlots">No of objects still in this list after start+count</param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int[] GetObjects(int ContainerIndex, uwObject[] objList, out int OccupiedSlots, int start = 0, int count = 8)
        {
            var Objects = ListObjects(ContainerIndex, objList);            
            OccupiedSlots = 0;
            if (Objects==null)
                {
                return null;
                }
            var output = new int[count];
            int i = 0;
            for (int o = start; o < start + count; o++)
            {
                if (o <= Objects.GetUpperBound(0))
                {
                    if (i < count)
                    {
                        output[i++] = Objects[o];
                        OccupiedSlots++;
                    }
                    else
                    {
                        output[i++] = -1;
                    }
                }
                else
                {
                    output[i++] = -1;
                }
            }            
            return output;
        }

        /// <summary>
        /// Returns a list of all objects in the container main level
        /// </summary>
        /// <param name="Container"></param>
        /// <returns></returns>
        public static int[] ListObjects(int ContainerIndex, uwObject[] objList)
        {
            var Container = objList[ContainerIndex];
            if (Container == null)
            {
                return null;
            }
            if (Container.link != 0)
            {
                var OutputList = new List<int>();
                var nextObj = objList[Container.link];
                while (nextObj != null)
                {
                    OutputList.Add(nextObj.index);
                    if (nextObj.next != 0)
                    {
                        nextObj = objList[nextObj.next];
                    }
                    else
                    {
                        nextObj = null;
                    }
                }
                return OutputList.ToArray();
            }
            return null;
        }

        static void GetRandomXYZForTile(TileInfo tile, out int xpos, out int ypos, out int zpos)
        {
            switch (tile.tileType)
            {
                case UWTileMap.TILE_DIAG_NE:
                    zpos = tile.floorHeight << 2;
                    xpos = Rng.r.Next(1, 8);
                    ypos = Rng.r.Next(7-xpos, 8); //(i >= 7 - j)
                    return;
                case UWTileMap.TILE_DIAG_SE:
                    zpos = tile.floorHeight << 2;
                    xpos = Rng.r.Next(1, 8);
                    ypos = Rng.r.Next(1, xpos); // (i >= j)
                    return;
                case UWTileMap.TILE_DIAG_NW:
                    zpos = tile.floorHeight << 2;
                    xpos = Rng.r.Next(1, 8);
                    ypos = Rng.r.Next(xpos, 8); // ((i <= j)
                    return;
                case UWTileMap.TILE_DIAG_SW:
                    zpos = tile.floorHeight << 2;
                    xpos = Rng.r.Next(1, 8);
                    ypos = Rng.r.Next(0, 8-xpos); // (7 - i >= j)
                    return;
                case UWTileMap.TILE_SLOPE_S:
                    xpos = Rng.r.Next(0,8);
                    ypos = Rng.r.Next(0,8);
                    zpos = (8-ypos) + (tile.floorHeight << 2);
                    return;
                case UWTileMap.TILE_SLOPE_N:
                    xpos = Rng.r.Next(0,8);
                    ypos = Rng.r.Next(0,8);
                    zpos = (ypos) + (tile.floorHeight << 2);
                    return;
                case UWTileMap.TILE_SLOPE_E:
                    xpos = Rng.r.Next(0,8);
                    ypos = Rng.r.Next(0,8);
                    zpos = (xpos) + (tile.floorHeight << 2);
                    return;
                case UWTileMap.TILE_SLOPE_W:
                    xpos = Rng.r.Next(0,8);
                    ypos = Rng.r.Next(0,8);
                    zpos = (8-xpos) + (tile.floorHeight << 2);
                    return;
                default:
                case UWTileMap.TILE_OPEN:
                case UWTileMap.TILE_SOLID:
                    xpos = Rng.r.Next(0,8);
                    ypos = Rng.r.Next(0,8);
                    zpos = tile.floorHeight << 2;
                    return;
            }
        }

    }//end class
}//end namespace