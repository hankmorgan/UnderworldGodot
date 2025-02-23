using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Class for removing instances of objects from object index chains
    /// </summary>
    public class ObjectRemover : UWClass
    {
        static int RemoveTrapFlags;
        static long TrapTriggerContainerListHead = 0;

        static int GlobalCullingRange = 0;
        public static void RemoveTrapChain(uwObject trapObj, long ptrListHead)
        {
            //var triggertile = UWTileMap.current_tilemap.Tiles[triggerX, triggerY];
            Debug.Print($"Try and remove trap {trapObj.a_name} {trapObj.index}");
            RemoveTrapFlags = trapObj.flags_full;
            if (RemoveTrapFlags != 0)
            {
                //removes this trap and all links to it in the gameworld
                //go throught all the tiles and object chains
                for (int x = 0; (x < 64 && RemoveTrapFlags > 0); x++)
                {
                    for (int y = 0; (y < 64 && RemoveTrapFlags > 0); y++)
                    {
                        var tile = UWTileMap.current_tilemap.Tiles[x, y];
                        if (tile.indexObjectList != 0)
                        {
                            RemoveTriggersPointingAtTrap(
                                listhead: tile.indexObjectList,
                                trapObjIndex: trapObj.index,
                                objList: UWTileMap.current_tilemap.LevelObjects,
                                tile: tile);
                        }
                    }
                }
            }
            //Now clear its chain.
            trapObj = FindObjectListHeadObject(ptrListHead, trapObj, 1);
            if (trapObj != null)
            {
                RemoveObjectAndChainFromLists(trapObj, TrapTriggerContainerListHead);
            }
        }


        static uwObject FindObjectListHeadObject(long ptrListHead, uwObject toFind, int searchmode)
        {
            int LinkNext = GetLinkNext(ptrListHead);

            if (LinkNext != 0)
            {
                TrapTriggerContainerListHead = ptrListHead;
                while (LinkNext != 0)
                {
                    var NextObject = UWTileMap.current_tilemap.LevelObjects[LinkNext];
                    if (NextObject.index == toFind.index)
                    {
                        TrapTriggerContainerListHead = ptrListHead;
                        return NextObject;
                    }
                    else
                    {
                        if (searchmode != 0)
                        {
                            if ((NextObject.is_quant == 0) && (NextObject.link != 0))
                            {
                                var recurseResult = FindObjectListHeadObject(NextObject.PTR + 6, toFind, 1);
                                if (recurseResult == null)
                                {
                                    LinkNext = GetLinkNext(NextObject.PTR + 4);
                                }
                                else
                                {
                                    return recurseResult;
                                }
                            }
                        }
                        LinkNext = GetLinkNext(NextObject.PTR + 4);
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        static int GetLinkNext(long ptr)
        {
            return (int)((Loader.getAt(UWTileMap.current_tilemap.lev_ark_block.Data, ptr, 16) >> 6) & 0x3FF);
        }

        static void RemoveObjectAndChainFromLists(uwObject toRemove, long ptrListHead)
        {
            if (toRemove.is_quant == 0 && toRemove.link != 0)
            {
                ClearLinkedList(toRemove.link);
            }
            if (ptrListHead != 0)
            {
                if (RemoveSingleObjectFromList(toRemove, ptrListHead))
                {
                    ObjectFreeLists.ReleaseFreeObject(toRemove);
                }
            }
            else
            {
                ObjectFreeLists.ReleaseFreeObject(toRemove);
            }

        }




        static void ClearLinkedList(int ptrListHead)
        {
            // Recurse up the linked list removing objects. incl special handling for traps/triggers
            var linknext = GetLinkNext(ptrListHead);
            if (linknext != 0)
            {
                var HeadObject = UWTileMap.current_tilemap.LevelObjects[linknext];

                //var next = LinkedObject.next;
                if (HeadObject.next != 0)
                {
                    ClearLinkedList(HeadObject.PTR + 4);
                }
                if (HeadObject.IsTrap || HeadObject.IsTrigger)
                {
                    RemoveTrapTriggerChain(HeadObject, ptrListHead);
                }
                else
                {
                    if ((HeadObject.is_quant == 0) && (HeadObject.link != 0))
                    {
                        ClearLinkedList(HeadObject.PTR + 6);
                    }
                    if (RemoveSingleObjectFromList(HeadObject, ptrListHead))
                    {
                        ObjectFreeLists.ReleaseFreeObject(HeadObject);
                    }
                }
            }
        }


        static bool RemoveSingleObjectFromList(uwObject toRemove, long ptrListHead)
        {
            if (toRemove != null)
            {//assumes ptrListHead is not 0
                var linknext = GetLinkNext(ptrListHead);
                while (linknext != 0)
                {
                    var NextObject = UWTileMap.current_tilemap.LevelObjects[linknext];
                    if (NextObject != null)
                    {
                        if (NextObject.index == toRemove.index)
                        {
                            //Clear the links.
                            var newLink = toRemove.next;
                            int tmp = (int)Loader.getAt(UWTileMap.current_tilemap.lev_ark_block.Data, ptrListHead, 16);
                            tmp = tmp & 0x3F;
                            tmp = tmp | (newLink << 6);
                            Loader.setAt(UWTileMap.current_tilemap.lev_ark_block.Data, ptrListHead, 16, tmp);
                            return true;
                        }
                        else
                        {
                            linknext = GetLinkNext(NextObject.PTR + 4);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }
        //Does the remove

        public static void RemoveTrapTriggerChain(uwObject traptrigger, long ptrListHead)
        {
            if (traptrigger.IsTrap)
            {
                RemoveTrapChain(traptrigger, ptrListHead);
            }
            else
            {//is a trigger
                RemoveTriggerChain(traptrigger, ptrListHead);
            }
        }

        public static void RemoveTriggerChain(uwObject TriggerHeadObj, long ptrListHead)
        {
            if (TriggerHeadObj.link != 0)
            {
                var linkedObject = UWTileMap.current_tilemap.LevelObjects[TriggerHeadObj.link];
                if (linkedObject.flags_full == 1)
                {
                    if (UWTileMap.ValidTile(TriggerHeadObj.quality, TriggerHeadObj.owner))
                    {
                        var tile = UWTileMap.current_tilemap.Tiles[TriggerHeadObj.quality, TriggerHeadObj.owner];
                        RemoveTrapChain(linkedObject, tile.Ptr + 2);
                    }
                }
                else
                {
                    if (RemoveSingleObjectFromList(TriggerHeadObj, ptrListHead))
                    {
                        linkedObject.flags_full--;
                        ObjectFreeLists.ReleaseFreeObject(TriggerHeadObj);
                    }
                }
                //handle timer triggers
                if (triggerObjectDat.triggertype(TriggerHeadObj.item_id) == (int)triggerObjectDat.triggertypes.TIMER)
                {
                    Debug.Print("Special handling for deleting timer triggers");
                }
            }
        }



        static void RemoveTriggersPointingAtTrap(int listhead, int trapObjIndex, uwObject[] objList, TileInfo tile = null)
        {
            if (listhead == 0) { return; }

            var triggerObj = objList[listhead];
            var trapObj = objList[trapObjIndex];

            if (tile != null)
            {//special case for start of tile list.
                if (tile.indexObjectList == trapObjIndex)
                {
                    tile.indexObjectList = trapObj.next;
                    trapObj.next = 0;
                    return;
                }
            }

            while (triggerObj != null)
            {
                uwObject nextObj = null;
                if (triggerObj.next != 0)
                {
                    nextObj = objList[triggerObj.next];
                }
                if (triggerObj.IsTrigger)
                {
                    if (triggerObj.IsTrigger)
                    {
                        if (triggerObj.link == trapObj.index)
                        {
                            if (triggerObjectDat.triggertype(triggerObj.item_id) == (int)triggerObjectDat.triggertypes.TIMER)
                            {
                                Debug.Print("Special handling for deleting timer triggers");
                            }
                            Debug.Print($"Breaking link for trigger {triggerObj.index} {triggerObj.a_name}");
                            RemoveObjectFromLinkedList(listhead, triggerObj.index, objList, triggerObj.PTR + 6);
                            ObjectFreeLists.ReleaseFreeObject(triggerObj);
                            triggerObj.link = 0;
                            RemoveTrapFlags--;
                        }
                    }
                }
                if (triggerObj.is_quant == 0 && triggerObj.link > 0)
                {//try recursive
                    RemoveTriggersPointingAtTrap(triggerObj.link, trapObjIndex, objList);
                }
                triggerObj = nextObj;
            }
        }


        public static bool RemoveObjectFromLinkedList(int listhead, int toRemove, uwObject[] objlist, long HeadOffset)
        {
            if (listhead == toRemove)
            {//Handle case where this starts at an arbitary data location and links directly to the object.
                var ObjToRemove = objlist[toRemove];
                var databuffer = ObjToRemove.DataBuffer;
                var tmp = (int)Loader.getAt(databuffer, HeadOffset, 16);
                tmp = tmp & 0x3F;//clear the link or next.

                if (ObjToRemove != null)
                {
                    tmp = tmp | (ObjToRemove.next << 6);//insert the next as the new item at the head.
                    ObjToRemove.next = 0;
                }
                Loader.setAt(databuffer, HeadOffset, 16, tmp);
                return true;
            }
            //var obj = objlist[toRemove];
            var next = listhead;
            while (next != 0)
            {
                var nextObject = objlist[next];
                var headObject = objlist[listhead];
                if (nextObject.index == toRemove)
                {
                    headObject.next = nextObject.next;
                    nextObject.next = 0;
                    return true;
                }
                listhead = next;//move the listhead on.
                next = objlist[next].next;//get the next object
            }
            return false;
        }

        /// <summary>
        /// Deletes the specified object from the tile (searches first level only, does not remove from containers)
        /// </summary>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <param name="indexToDelete"></param>
        public static bool DeleteObjectFromTile(int tileX, int tileY, short indexToDelete, bool RemoveFromWorld = true)
        {
            if (!UWTileMap.ValidTile(tileX, tileY))
            {
                return false;//not on map.
            }
            var objList = UWTileMap.current_tilemap.LevelObjects;
            if (indexToDelete != 0)
            {
                var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                var objectToDelete = objList[indexToDelete];
                if (objectToDelete != null)
                {
                    if (tile.indexObjectList == indexToDelete)
                    {
                        tile.indexObjectList = objectToDelete.next;
                        objectToDelete.next = 0;
                        if (RemoveFromWorld)
                        {
                            ObjectFreeLists.ReleaseFreeObject(objectToDelete);
                        }
                        return true;
                    }
                    else
                    {
                        //search
                        var next = tile.indexObjectList;

                        while (next != 0)
                        {
                            var nextObject = objList[next];
                            if (nextObject.next == indexToDelete)
                            {
                                nextObject.next = objectToDelete.next;
                                objectToDelete.next = 0;
                                if (RemoveFromWorld)
                                {
                                    ObjectFreeLists.ReleaseFreeObject(objectToDelete);
                                }
                                return true;
                            }
                            next = nextObject.next;
                        }
                        Debug.Print($"Was unable to find {indexToDelete} to delete it in {tileX},{tileY}");
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Decides if an object can be culled if it or all of it's contents meets certain criteria, (rng check, object flags)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="CullingRange"></param>
        /// <returns>True if object can be culled, false to preserve</returns>
        public static bool ObjectCulling(uwObject obj, int CullingRange)
        {
            if (obj == null)
            {
                return false;
            }
            if (CullingRange != 0)
            {
                CullingRange += Rng.r.Next(3);
            }
            GlobalCullingRange = CullingRange;
            if (!CheckDoNotCull(obj))
            {
                if ((obj.is_quant != 0) && (obj.link != 0))
                {//object is a container that needs it's contents to be tested 
                    if (CallBacks.RunCodeOnObjectsInChain(CheckDoNotCull, obj, UWTileMap.current_tilemap.LevelObjects))
                    {
                        return false;
                    }
                }
                if (Rng.r.Next(0xA) < GlobalCullingRange)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Checks if the object should be culled.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if the object is to be preserved, false to cull</returns>
        static bool CheckDoNotCull(uwObject obj)
        {            
            if (obj.doordir == 1)
            {
                return true;
            }
            else
            {
                int si;
                if ((obj.is_quant == 1) && ((obj.link & 0x200) == 0))
                {
                    si = obj.link - 1;
                }
                else
                {
                    si = 0;
                }

                if ((commonObjDat.cullingpriority(obj.item_id) + (si / 2)) <= GlobalCullingRange)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }


    }//end class
}//end namespace
