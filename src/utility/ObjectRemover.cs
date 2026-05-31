using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace Underworld
{

    public class ObjectRemover : UWClass
    {

        static int RemoveTrapFlags = 0;

        /// <summary>
        /// Vanilla remove object behaviour. Remove object will obey object culling rules and clean linked lists/object overlays if needed.
        /// The problem I keep running into with this is that in vanilla code I'm dealing with direct ptrs to memory where as in c# I can only really reference the arrays. 
        /// At a few points in the process. I need to be able to change variable values outside of the arrays. I have writers block on this object removal code....
        /// </summary>
        /// <param name="ListHeadPTR"></param>
        /// <param name="objToRemove"></param>
        /// <param name="ForceCull"></param>
        /// <returns></returns>
        public static uwObject RemoveObject_experimental(uwObject[] objectlist, byte[]buffer, int ListHeadPTR, uwObject objToRemove, bool ForceCull)
        {
            if (!ForceCull)
            {
                if (!ObjectRemover_OLD.ObjectCullingTest(objToRemove, 0xA))
                {
                    return objToRemove;
                }
            }

            if (objToRemove.majorclass == 7)
            {
                //remove the animation overlay
                AnimationOverlay.RemoveAnimationOverlay(objToRemove.index);
            }
            if (ListHeadPTR == 0)
            {
                //construct a temporary buffer to use with ClearLinkedList
                byte[] tmp = new byte[2];
                Loader.setAt16(tmp, 0, objToRemove.index << 6);

                Debug.Print("unimplemented/untested object removal scenario. ListHeadPtr is 0");
                ClearLinkedList(
                    objectlist: objectlist,
                    objectdata: objToRemove.DataBuffer,
                    listheaddata: tmp,
                    ptrListHead: 0);
            }
            else
            {
                //ObjectRemover_OLD.RemoveObjectAndChainFromLists(objToRemove, ListHeadPTR);
                //RemoveObjectAndChainFromLists(ListHeadValue, ObjtoRemove)//to implement
                RemoveObjectAndChainFromLists(
                    objectlist: objectlist, 
                    objectdata: buffer, 
                    listheaddata: buffer, 
                    ptrListHead: ListHeadPTR, 
                    toDelete: objToRemove);
            }
            return null;
        }


        static void RemoveObjectAndChainFromLists(uwObject[]objectlist, byte[]objectdata, byte[]listheaddata, long ptrListHead, uwObject toDelete)
        {
            if (toDelete.is_quant == 0)
            {
                if (toDelete.link>0)
                {
                    ClearLinkedList(
                        objectlist: objectlist, 
                        objectdata: objectdata, 
                        listheaddata: listheaddata, 
                        ptrListHead: toDelete.PTR+6);
                }
            }
            
            if (UnlinkObjectFromLinkedList(
                objectlist: objectlist, 
                listheaddata: listheaddata, 
                objectdata: objectdata, 
                ptrListHead: ptrListHead, 
                toUnlink: toDelete))
            {
                ObjectFreeLists.ReleaseFreeObject(toDelete);
            }
        }

        static void ClearLinkedList(uwObject[] objectlist, byte[] objectdata, byte[] listheaddata, long ptrListHead)
        {
            var ListHeadObject = GetLinkNextObject(
                objectList: objectlist,
                buffer: listheaddata,
                ptr: ptrListHead);

            if (ListHeadObject != null)
            {
                if (ListHeadObject.next != 0)
                {
                    ClearLinkedList(
                        objectlist: objectlist, 
                        objectdata: ListHeadObject.DataBuffer, 
                        listheaddata: ListHeadObject.DataBuffer, 
                        ptrListHead: ListHeadObject.PTR + 4); //clear the list of the objects next
                }
                if (ListHeadObject.majorclass == 6)
                {
                    //trap/trigger
                    //Debug.Print("REMOVE TRAP TRIGGER CHAIN");
                    RemoveTrapTriggerChain(
                        objectlist: objectlist, 
                        listheaddata: listheaddata, 
                        objectdata: objectdata, 
                        ptrListHead: ptrListHead, 
                        toUnlink: ListHeadObject );
                }
                else
                {
                    if (ListHeadObject.is_quant == 0)
                    {
                        if (ListHeadObject.link != 0)
                        {
                            //clear the link chain
                            ClearLinkedList(objectlist, ListHeadObject.DataBuffer, ListHeadObject.DataBuffer, ListHeadObject.PTR + 6);
                        }
                    }
                    //unlink from this chain
                    if (UnlinkObjectFromLinkedList(
                        objectlist: objectlist,
                        listheaddata: listheaddata,
                        objectdata: ListHeadObject.DataBuffer,
                        ptrListHead: ptrListHead,
                        toUnlink: ListHeadObject))
                    {
                        ObjectFreeLists.ReleaseFreeObject(ListHeadObject);
                    }
                }
            }

        }

        static void RemoveTrapTriggerChain(uwObject[] objectlist, byte[] listheaddata, byte[] objectdata, long ptrListHead, uwObject toUnlink)
        {
            if (toUnlink.minorclass > 1)//triggers
            {
                RemoveTriggerChain(
                    objectlist: objectlist,
                    listheaddata: listheaddata,
                    objectdata: objectdata,
                    ptrListHead: ptrListHead,
                    toUnlink: toUnlink);
            }
            else
            {
                //traps
                RemoveTrapChain(
                    objectlist: objectlist,
                    listheaddata: listheaddata,
                    objectdata: objectdata,
                    ptrListHead: ptrListHead,
                    toUnlink: toUnlink);
            }
        }

        static void RemoveTriggerChain(uwObject[] objectlist, byte[] listheaddata, byte[] objectdata, long ptrListHead, uwObject toUnlink)
        {
            var linkedObject = GetLinkNextObject(objectlist, objectdata, toUnlink.PTR + 6);
            if (linkedObject != null)
            {
                if (linkedObject.flags == 1)
                {
                    if (UnlinkObjectFromLinkedList(objectlist, listheaddata, objectdata, ptrListHead, toUnlink))
                    {
                        linkedObject.flags--;
                        ObjectFreeLists.ReleaseFreeObject(toUnlink);
                    }
                }
                else
                {
                    var tile = UWTileMap.current_tilemap.Tiles[toUnlink.owner, toUnlink.quality];
                    RemoveTrapChain(
                        objectlist: objectlist,
                        listheaddata: toUnlink.DataBuffer,
                        objectdata: toUnlink.DataBuffer,
                        ptrListHead: tile.Ptr + 2,
                        toUnlink: linkedObject);
                }

                if (_RES == GAME_UW2)
                {
                    if (triggerObjectDat.triggertype(toUnlink.item_id) == (int)triggerObjectDat.triggertypes.TIMER)
                    {
                        Debug.Print("handle removal of timer trigger!");//the timer trigger has to be moved in the list of timers. Unknown when in game this would happen/?
                    }
                }
            }
        }

        static void RemoveTrapChain(uwObject[] objectlist, byte[] listheaddata, byte[] objectdata, long ptrListHead, uwObject toUnlink)
        {
            RemoveTrapFlags = toUnlink.flags;
            if (RemoveTrapFlags != 0)
            {
                foreach (var tile in UWTileMap.current_tilemap.Tiles)
                {
                    if (RemoveTrapFlags == 0)
                    {

                    }
                    else
                    {
                        if (tile.indexObjectList > 0)
                        {
                            RemoveTriggersPointAtTrap(objectlist, listheaddata, objectdata, tile.Ptr + 2, toUnlink);
                        }
                    }
                }
            }
        }

        static void RemoveTriggersPointAtTrap(uwObject[] objectlist, byte[] listheaddata, byte[] objectdata, long ptrListHead, uwObject toRemove)
        {
            var HeadObject = GetLinkNextObject(
                objectList: objectlist,
                buffer: listheaddata,
                ptr: ptrListHead);
            while (HeadObject != null)
            {
                var NextObject = GetLinkNextObject(objectlist, objectdata, HeadObject.PTR + 4); //get next now.

                if (HeadObject.majorclass == 6)
                {
                    if (HeadObject.minorclass > 2)
                    {
                        //object is a trigger
                        if (HeadObject.link == toRemove.index)
                        {
                            if (_RES == GAME_UW2)
                            {
                                if (triggerObjectDat.triggertype(HeadObject.item_id) == (int)triggerObjectDat.triggertypes.TIMER)
                                {
                                    Debug.Print("Handle removal of timer trigger!");
                                }
                            }
                            if (UnlinkObjectFromLinkedList(
                                objectlist: objectlist,
                                listheaddata: HeadObject.DataBuffer,
                                objectdata: objectdata,
                                ptrListHead: HeadObject.PTR + 6,
                                toUnlink: toRemove))
                            {
                                ObjectFreeLists.ReleaseFreeObject(HeadObject);
                                RemoveTrapFlags--;
                            }
                        }
                    }
                }

                //check the link again
                if (HeadObject.is_quant == 0)
                {
                    if (HeadObject.link > 0)
                    {
                        RemoveTriggersPointAtTrap(
                            objectlist: objectlist,
                            listheaddata: objectdata,
                            objectdata: objectdata,
                            ptrListHead: HeadObject.PTR + 6,
                            toRemove: toRemove);
                    }
                }
                HeadObject = NextObject;
            }
        }

        static bool UnlinkObjectFromLinkedList(uwObject[] objectlist, byte[] listheaddata, byte[] objectdata, long ptrListHead, uwObject toUnlink)
        {
            if (toUnlink != null)
            {
                var nextObject = GetLinkNextObject(objectList: objectlist, buffer: listheaddata, ptr: ptrListHead);
                if (nextObject == toUnlink)
                {
                    //initial case where the head object is to one to remove.
                    //Set the listhead "next" to the objects next and clear the objects next
                    var tmp = (int)DataLoader.getAt16(listheaddata, ptrListHead) & 0x3F;
                    tmp = (tmp | (toUnlink.next << 6));
                    DataLoader.setAt16(listheaddata, (int)ptrListHead, tmp);
                    toUnlink.next = 0;
                    return true;
                }

                while (nextObject != null)
                {
                    if (nextObject == toUnlink)
                    {
                        var tmp = (int)DataLoader.getAt16(objectdata, ptrListHead) & 0x3F;
                        tmp = (tmp | (toUnlink.next << 6));
                        DataLoader.setAt16(objectdata, (int)ptrListHead, tmp);
                        toUnlink.next = 0;
                        return true;
                    }
                    ptrListHead = nextObject.PTR + 4;
                    nextObject = GetLinkNextObject(objectList: objectlist, buffer: objectdata, ptr: ptrListHead); //get the next object.
                }
                return false;
            }
            else
            {
                return true;
            }
        }


        static uwObject GetLinkNextObject(uwObject[] objectList, byte[] buffer, long ptr)
        {
            var index = GetLinkNext(buffer, ptr);
            if (index != 0)
            {
                return objectList[index];
            }
            else
            {
                return null;
            }

        }

        static int GetLinkNext(byte[] buffer, long ptr)
        {
            return (int)((Loader.getAt(buffer, ptr, 16) >> 6) & 0x3FF);
        }

    }//end class


    /// <summary>
    /// Class for removing instances of objects from object index chains
    /// </summary>
    public class ObjectRemover_OLD : UWClass
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
                                ToRemove: trapObj.index,
                                ptrListHead: tile.indexObjectList);
                            // RemoveTriggersPointingAtTrap(
                            //     listhead: tile.indexObjectList,
                            //     trapObjIndex: trapObj.index,
                            //     objList: UWTileMap.current_tilemap.LevelObjects,
                            //     tile: tile);
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

        static uwObject GetLinkNextObject(long ptr)
        {
            var index = (int)((Loader.getAt(UWTileMap.current_tilemap.lev_ark_block.Data, ptr, 16) >> 6) & 0x3FF);
            if (index > 0)
            {
                return UWTileMap.current_tilemap.LevelObjects[index];
            }
            else
            {
                return null;
            }
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


        static void RemoveTriggersPointingAtTrap(int ToRemove, long ptrListHead)
        {
            var Obj = GetLinkNextObject(ptrListHead);
            while (Obj != null)
            {
                //get the next of the object
                var NextObject = GetLinkNextObject(Obj.PTR + 4); //get the next object.

                if (Obj.IsTrigger)
                {
                    if (Obj.link == ToRemove)
                    {
                        if (_RES == GAME_UW2)
                        {
                            if (triggerObjectDat.triggertype(Obj.item_id) == 0xA)
                            {
                                Debug.Print("TODO special handling for deleting timer triggers.");
                            }
                        }
                        //unlink object and replace with it's next
                        int tmp = (int)(DataLoader.getAt16(UWTileMap.current_tilemap.lev_ark_block.Data, ptrListHead) & 0x3F);//clear the next/link
                        tmp = (tmp | (Obj.next << 6));
                        DataLoader.setAt16(UWTileMap.current_tilemap.lev_ark_block.Data, (int)ptrListHead, (int)tmp);
                        Obj.link = 0;
                        ObjectFreeLists.ReleaseFreeObject(Obj);

                    }
                }

                //ovr166_1C3F;
                if (Obj.is_quant == 0)
                {
                    //recurse on it's linked, this means it's not been removed by the previous loop.
                    if (Obj.link > 0)
                    {
                        RemoveTriggersPointingAtTrap(ToRemove, Obj.PTR + 6);
                    }
                }

                ptrListHead = Obj.PTR + 4; //set to the int16 that contains the next value.
                Obj = NextObject;
            }
        }



        // static void RemoveTriggersPointingAtTrap(int listhead, int trapObjIndex, uwObject[] objList, TileInfo tile = null)
        // {
        //     if (listhead == 0) { return; }

        //     var triggerObj = objList[listhead];
        //     var trapObj = objList[trapObjIndex];

        //     if (tile != null)
        //     {//special case for start of tile list.
        //         if (tile.indexObjectList == trapObjIndex)
        //         {
        //             tile.indexObjectList = trapObj.next;
        //             trapObj.next = 0;
        //             return;
        //         }
        //     }

        //     while (triggerObj != null)
        //     {
        //         uwObject nextObj = null;
        //         if (triggerObj.next != 0)
        //         {
        //             nextObj = objList[triggerObj.next];
        //         }
        //         if (triggerObj.IsTrigger)
        //         {
        //             if (triggerObj.IsTrigger)
        //             {
        //                 if (triggerObj.link == trapObj.index)
        //                 {
        //                     if (triggerObjectDat.triggertype(triggerObj.item_id) == (int)triggerObjectDat.triggertypes.TIMER)
        //                     {
        //                         Debug.Print("Special handling for deleting timer triggers");
        //                     }
        //                     Debug.Print($"Breaking link for trigger {triggerObj.index} {triggerObj.a_name}");
        //                     //This is bugging out and may cause list corruption.
        //                     RemoveObjectFromLinkedList(listhead: listhead, toRemove: triggerObj.index, objlist: objList, OffsetToListHeadConnection: objList[listhead].PTR + 4);
        //                     //replacing instead with a simplified break to the link
        //                     //triggerObj.link = 0;
        //                     ObjectFreeLists.ReleaseFreeObject(triggerObj);
        //                     if (UWTileMap.ValidTile(triggerObj.tileX, triggerObj.tileY))
        //                     {
        //                         //triggers is on map.
        //                         ObjectRemover_OLD.DeleteObjectFromTile_DEPRECIATED(triggerObj.tileX, triggerObj.tileY, triggerObj.index, true);
        //                     }

        //                     //triggerObj.link = 0;
        //                     RemoveTrapFlags--;
        //                 }
        //             }
        //         }
        //         if (triggerObj.is_quant == 0 && triggerObj.link > 1)
        //         {//try recursive
        //             RemoveTriggersPointingAtTrap(triggerObj.link, trapObjIndex, objList);
        //         }
        //         triggerObj = nextObj;
        //     }
        // }


        public static bool RemoveObjectFromLinkedList(int listhead, int toRemove, uwObject[] objlist, long OffsetToListHeadConnection)
        {
            if (listhead == toRemove)
            {//Handle case where this starts at an arbitary data location and links directly to the object.
                var ObjToRemove = objlist[toRemove];
                var databuffer = ObjToRemove.DataBuffer;
                var tmp = (int)Loader.getAt(databuffer, OffsetToListHeadConnection, 16);
                tmp = tmp & 0x3F;//clear the link or next.

                if (ObjToRemove != null)
                {
                    tmp = tmp | (ObjToRemove.next << 6);//insert the next as the new item at the head.
                    ObjToRemove.next = 0;
                }
                Loader.setAt(databuffer, OffsetToListHeadConnection, 16, tmp);
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
                if (next == objlist[next].next)
                {
                    Debug.Print("Infinite loop in RemoveObjectFromLinkedList()");
                    return false;
                }
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
        public static bool DeleteObjectFromTile_DEPRECIATED(int tileX, int tileY, short indexToDelete, bool RemoveFromWorld = true, bool forceDelete = false)
        {
            List<int> tested = new();
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
                        uwObject PreviousObject = null;
                        while (next != 0)
                        {
                            if (tested.Contains(next))
                            {
                                Debug.Print($"Likely loop in object chain. Index {next} has already been tested in DeleteObjectFromTile.");
                                if (PreviousObject != null)
                                {
                                    Debug.Print($"Fixing loop by setting the next of {PreviousObject.a_name} {PreviousObject.index} to 0. Objects may be missing from tile at next reload!");
                                    PreviousObject.next = 0;
                                    return false;
                                }
                            }
                            tested.Add(next);//to track for infinite loops.
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
                            PreviousObject = nextObject;
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
        public static bool ObjectCullingTest(uwObject obj, int CullingRange)
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
            if (ObjectCullingRngTest(obj) == false)
            {
                if ((obj.is_quant == 0) && (obj.link != 0))
                {//object is a container that needs it's contents to be tested 
                    if (CallBacks.RunCodeOnObjectsInChain(ObjectCullingRngTest, obj, UWTileMap.current_tilemap.LevelObjects))
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
        static bool ObjectCullingRngTest(uwObject obj)
        {
            if (obj.doordir == 1)
            {
                return true;
            }
            else
            {
                int si;
                if (obj.is_quant == 0)
                {
                    si = 0;
                }
                else
                {
                    if ((obj.link & 0x200) != 0)
                    {
                        si = 0;
                    }
                    else
                    {
                        si = obj.link - 1;
                    }
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
