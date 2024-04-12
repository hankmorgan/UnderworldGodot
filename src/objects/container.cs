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
            //check for pickup trigger first
            trigger.PickupTrigger(UWTileMap.current_tilemap.LevelObjects, obj);

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
                        UWTileMap.GetRandomXYZForTile(tile, out int newxpos, out int newypos, out int newzpos);
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
                start: start,
                count: count
                );

            if (objects == null)
            {
                //empty container
                for (int o = 0; o < 8; o++)
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

            uimanager.EnableDisable(uimanager.instance.ArrowUp, start != 0);
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
                    uimanager.EnableDisable(uimanager.instance.OpenedContainer, false);
                    return -1;
                }
            }
            foreach (var objToCheck in playerdat.InventoryObjects)
            {//if this far down then I need to find the container that the closing container sits in
                if (objToCheck != null)
                {
                    var result = objectsearch.GetContainingObject(
                        ListHead: objToCheck.index,
                        ToFind: uimanager.OpenedContainerIndex,
                        objList: playerdat.InventoryObjects);
                    if (result != -1)
                    {//container found. Browse into it by using it
                        Use(objList[result], false);
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
            if (Objects == null)
            {
                //return an empty list
                var emptylist = new int[count];
                for (int o = start; o < start + count; o++)
                {
                    emptylist[o] = -1;
                }
                return emptylist;
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



        public static bool TestContainerCanHold(uwObject containerobject, uwObject objectToAdd, bool printReason = true)
        {//assume containerobject = in player object list, objecttoadd in level objects list
            MessageDisplay.MessageDisplayMode printmode = MessageDisplay.MessageDisplayMode.NormalMode;
            if (ConversationVM.InConversation)
            {
                printmode = MessageDisplay.MessageDisplayMode.TemporaryMessage;
            }
            bool WeightTest = false;
            bool ContainerCanHold = false;
            bool WrongTypeMessage = false;
            var mask = containerObjectDat.objectmask(containerobject.item_id);
            if (mask <= -1)
            {
                ContainerCanHold = true;
            }
            else
            {
                if ((mask >= 0) && (mask < 512))
                {//check for exact match. I don't think this actually happens with any container but just incase...
                    if (objectToAdd.item_id == mask)
                    {
                        if (printReason)
                        {
                            if (ConversationVM.InConversation)
                            {
                                uimanager.AddToMessageScroll(
                                    stringToAdd: GameStrings.GetString(1, GameStrings.str_that_item_does_not_fit_),
                                    mode: printmode);
                            }
                        }
                        ContainerCanHold = false;
                    }
                    else
                    {
                        ContainerCanHold = true;
                    }
                }
                else
                {//check category against mask-512;
                    //mask = mask - 512s;
                    switch (mask)
                    {
                        case 512://rune bag, this will probably never hit but included this case for completedness
                            ContainerCanHold = runestone.IsRunestone(objectToAdd.item_id);
                            if ((!ContainerCanHold) && (printReason))
                            {
                                uimanager.AddToMessageScroll(
                                    stringToAdd: GameStrings.GetString(1, GameStrings.str_you_can_only_put_runes_in_the_rune_bag_),
                                    mode:printmode);
                            }
                            break;
                        case 513://quivers, accepts missiles and wands
                            {
                                if (
                                    ((objectToAdd.majorclass == 0) && (objectToAdd.minorclass == 3) && (objectToAdd.classindex <= 3))
                                    ||
                                    ((objectToAdd.majorclass == 0) && (objectToAdd.minorclass == 3) && (objectToAdd.classindex >= 8))
                                )
                                {//a wand or ammo
                                    ContainerCanHold = true;
                                }
                                else
                                {
                                    ContainerCanHold = false;
                                    WrongTypeMessage = true;
                                }
                                break;
                            }
                        case 514://scrolls
                            {
                                if (
                                    ((objectToAdd.majorclass == 4) && (objectToAdd.minorclass == 3) && (_RES != GAME_UW2))
                                    ||
                                    ((objectToAdd.majorclass == 4) && (objectToAdd.minorclass == 3) && (objectToAdd.classindex <= 0xA) && (_RES == GAME_UW2))
                                    )
                                {
                                    //a book/scroll
                                    ContainerCanHold = true;
                                }
                                else
                                {
                                    ContainerCanHold = false;
                                    WrongTypeMessage = true;
                                }
                                break;
                            }
                        case 515://bowl/food
                            {
                                if (food.IsFood(objectToAdd))
                                {
                                    ContainerCanHold = true;
                                }
                                else
                                {
                                    ContainerCanHold = false;
                                    WrongTypeMessage = true;
                                }
                                break;
                            }
                        case 516://keyring
                            {
                                if ((objectToAdd.majorclass == 4) && (objectToAdd.minorclass == 0))
                                {
                                    ContainerCanHold = true;
                                }
                                else
                                {
                                    ContainerCanHold = false;
                                    WrongTypeMessage = true;
                                }
                                break;
                            }
                    }
                }
            }

            if (!ContainerCanHold && WrongTypeMessage)
            {
                uimanager.AddToMessageScroll(
                    stringToAdd: GameStrings.GetString(1, GameStrings.str_that_item_does_not_fit_),
                    mode: printmode);
            }

            //TODO test total weight.
            if (ContainerCanHold)
            {
                var containerMass = GetTotalMass(containerobject, playerdat.InventoryObjects, true);
                var itemToAddMass = GetTotalMass(objectToAdd, UWTileMap.current_tilemap.LevelObjects, true);
                if (containerMass + itemToAddMass > containerObjectDat.capacity(containerobject.item_id))
                {
                    WeightTest = false;
                    if (printReason)
                    {
                        uimanager.AddToMessageScroll(
                            stringToAdd: $"The {GameStrings.GetSimpleObjectNameUW(containerobject.item_id)} is too full",
                            mode:printmode);
                    }
                }
                else
                {
                    WeightTest = true;
                }
            }

            return (ContainerCanHold && WeightTest);
        }


        /// <summary>
        /// Gets the total mass of the object chain.
        /// </summary>
        /// <param name="objectToWeight"></param>
        /// <param name="objList"></param>
        /// <param name="IgnoreNext">tells the loop to not look at the next object.</param>
        /// <returns></returns>
        public static int GetTotalMass(uwObject objectToWeight, uwObject[] objList, bool IgnoreNext)
        {
            var selfweight = objectToWeight.ObjectQuantity * commonObjDat.mass(objectToWeight.item_id);

            if (!IgnoreNext)
            {
                var nextIndex = objectToWeight.next;
                while (nextIndex != 0)
                {
                    var nextObject = objList[nextIndex];
                    selfweight += GetTotalMass(nextObject, objList, true);
                    nextIndex = nextObject.next;
                }
            }

            if (objectToWeight.is_quant == 0)
            {
                if (objectToWeight.link > 0)
                {
                    var linked = objList[objectToWeight.link];
                    selfweight += GetTotalMass(linked, objList, false);
                }
            }
            return selfweight;
        }

    }//end class
}//end namespace