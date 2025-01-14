using System.Diagnostics;

namespace Underworld
{
    //Player dat inventory
    public partial class playerdat : Loader
    {

        public static int WeightCarried
        {
            get
            {
                if(_RES==GAME_UW2)
                {
                    return GetAt16(0x4B);
                }
                else
                {
                    return GetAt16(0x4C);
                }
            }
        }

        /// <summary>
        /// Reference to the object the player is using currently in their hand
        /// To be consistent his object MUST always be a world object. When picking inventory objects add it to world
        /// </summary>
        public static int ObjectInHand// = -1;
        {
            get
            {
                return _ObjectInHand;
            }
            set
            {
                if (value != -1)
                {
                    if (_ObjectInHand != -1)
                    {
                        Debug.Print("Dropping object that was already in hand before taking new object.");
                        var tile = UWTileMap.current_tilemap.Tiles[playerdat.tileX,playerdat.tileY];
					    UWTileMap.GetRandomXYZForTile(tile, out int newxpos, out int newypos, out int newzpos);
					    var dropcoordinate = uwObject.GetCoordinate(playerdat.tileX, playerdat.tileY, newxpos, newypos, newzpos);

                        //already holding something. Drop that item to the ground first so it does not get lost
                        pickup.Drop(
                            index: _ObjectInHand,
                            objList: UWTileMap.current_tilemap.LevelObjects,
                            dropPosition: dropcoordinate,
                            tileX: playerdat.tileX,
                            tileY: playerdat.tileY, 
                            DoSpecialCases:false);
                    }
                }
                _ObjectInHand = value;
            }
        }


        static int _ObjectInHand = -1;


        public static uwObject[] InventoryObjects = new uwObject[512];
        //public static byte[] InventoryBuffer = new byte[512 * 8];

        /// <summary>
        /// The last item in the inventory
        /// </summary>
        //public static int LastItemIndex;

        /// <summary>
        /// Gets the next memory slot in the inventory that does not have an object.
        /// </summary>
        public static int NextFreeInventorySlot
        {
            get
            {
                for (int i = 1; i <= InventoryObjects.GetUpperBound(0); i++)
                {//start at 1 since there is no object 0
                    if (InventoryObjects[i] == null)
                    {
                        return i;
                    }
                }
                return -1; //no free slot
            }
        }


        /// <summary>
        /// Looks up all inventory slots list indices. starting at helm and ending with backpacks
        /// </summary>
        /// <param name="slot"></param>
        public static int GetInventorySlotListHead(int slot)
        {
            int startOffset = 0xF8;
            if (_RES == GAME_UW2)
            {
                startOffset = 0x3A3;
            }
            return GetAt16(startOffset + slot * 2) >> 6;
        }

        public static int GetInventorySlotListHeadOffset(int slot)
        {
            int startOffset = 0xF8;
            if (_RES == GAME_UW2)
            {
                startOffset = 0x3A3;
            }
            return startOffset + slot * 2;
        }

        public static uwObject GetInventorySlotObject(int slot)
        {
            int index = GetInventorySlotListHead(slot);
            return InventoryObjects[index];
        }

        public static void SetInventorySlotListHead(int slot, int value)
        {
            value = value << 6;
            int startOffset = 0xF8;
            if (_RES == GAME_UW2)
            {
                startOffset = 0x3A3;
            }
            var currentValue = GetAt16(startOffset + slot * 2);
            currentValue &= 0x3f; //Preserve existing value
            var valueToSet = value | currentValue;
            SetAt16(startOffset + slot * 2, valueToSet);
        }

        /// <summary>
        /// Object index for the item at the helm slot
        /// </summary>
        public static int Helm
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3A3) >> 6;
                    default:
                        return GetAt16(0xF8) >> 6;
                }
            }
            set
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        SetAt16(0x3A3, value << 6); break;
                    default:
                        SetAt16(0xF8, value << 6); break;
                }
            }
        }


        /// <summary>
        /// Direct reference to the object at the helm slot
        /// </summary>
        public static uwObject HelmObject
        {
            get
            {
                if (Helm != 0)
                {
                    return InventoryObjects[Helm];
                }
                return null;
            }
        }

        public static int ChestArmour
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3A5) >> 6;
                    default:
                        return GetAt16(0xFA) >> 6;
                }
            }
        }


        /// <summary>
        /// Direct reference to the object at the chest armour slot
        /// </summary>
        public static uwObject ChestArmourObject
        {
            get
            {
                if (ChestArmour != 0)
                {
                    return InventoryObjects[ChestArmour];
                }
                return null;
            }
        }

        public static int Gloves
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3A7) >> 6;
                    default:
                        return GetAt16(0xFC) >> 6;
                }
            }
        }


        /// <summary>
        /// Direct reference to the object at the gloves slot
        /// </summary>
        public static uwObject GlovesObject
        {
            get
            {
                if (Gloves != 0)
                {
                    return InventoryObjects[Gloves];
                }
                return null;
            }
        }

        public static int Leggings
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3A9) >> 6;
                    default:
                        return GetAt16(0xFE) >> 6;
                }
            }
        }


        /// <summary>
        /// Direct reference to the object at the leggings slot
        /// </summary>
        public static uwObject LeggingsObject
        {
            get
            {
                if (Leggings != 0)
                {
                    return InventoryObjects[Leggings];
                }
                return null;
            }
        }

        public static int Boots
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3AB) >> 6;
                    default:
                        return GetAt16(0x100) >> 6;
                }
            }
        }


        /// <summary>
        /// Direct reference to the object at the boots slot
        /// </summary>
        public static uwObject BootsObject
        {
            get
            {
                if (Boots != 0)
                {
                    return InventoryObjects[Boots];
                }
                return null;
            }
        }


        public static int RightShoulder
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3AD) >> 6;
                    default:
                        return GetAt16(0x102) >> 6;
                }
            }
        }


        /// <summary>
        /// Direct reference to the object at the boots slot
        /// </summary>
        public static uwObject RightShoulderObject
        {
            get
            {
                if (RightShoulder != 0)
                {
                    return InventoryObjects[RightShoulder];
                }
                return null;
            }
        }

        public static int LeftShoulder
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3AF) >> 6;
                    default:
                        return GetAt16(0x104) >> 6;
                }
            }
        }

        /// <summary>
        /// Direct reference to the object at the boots slot
        /// </summary>
        public static uwObject LeftShoulderObject
        {
            get
            {
                if (LeftShoulder != 0)
                {
                    return InventoryObjects[LeftShoulder];
                }
                return null;
            }
        }

        public static int RightHand
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3B1) >> 6;
                    default:
                        return GetAt16(0x106) >> 6;
                }
            }
        }

        /// <summary>
        /// Direct reference to the object at the boots slot
        /// </summary>
        public static uwObject RightHandObject
        {
            get
            {
                if (RightHand != 0)
                {
                    return InventoryObjects[RightHand];
                }
                return null;
            }
        }

        public static int LeftHand
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3B3) >> 6;
                    default:
                        return GetAt16(0x108) >> 6;
                }
            }
        }

        /// <summary>
        /// Direct reference to the object at the boots slot
        /// </summary>
        public static uwObject LeftHandObject
        {
            get
            {
                if (LeftHand != 0)
                {
                    return InventoryObjects[LeftHand];
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the object in the player primary hand.
        /// </summary>
        /// <returns></returns>

        public static uwObject PrimaryHandObject
        {
            get
            {
                if (playerdat.isLefty)
                {
                    return playerdat.LeftHandObject;
                }
                else
                {
                    return playerdat.RightHandObject;
                }
            }
        }


        public static int RightRing
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3B5) >> 6;
                    default:
                        return GetAt16(0x10A) >> 6;
                }
            }
        }

        public static uwObject RightRingObject
        {
            get
            {
                if (RightRing != 0)
                {
                    return InventoryObjects[RightRing];
                }
                return null;
            }
        }
        public static int LeftRing
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3B7) >> 6;
                    default:
                        return GetAt16(0x10C) >> 6;
                }
            }
        }
        public static uwObject LeftRingObject
        {
            get
            {
                if (LeftRing != 0)
                {
                    return InventoryObjects[LeftRing];
                }
                return null;
            }
        }

        public static int BackPack(int slot)
        {
            switch (_RES)
            {
                case GAME_UW2:
                    return GetAt16(0x3B9 + slot * 2) >> 6;
                default:
                    return GetAt16(0x10E + slot * 2) >> 6;
            }
        }

        public static uwObject BackPackObject(int slot)
        {
            if (BackPack(slot) != 0)
            {
                return InventoryObjects[BackPack(slot)];
            }
            return null;
        }








        /// <summary>
        /// Removes the object data at index from the player inventory.
        /// </summary>
        /// <param name="index"></param>
        public static void RemoveFromInventory(int index, bool updateUI = true, bool ClearLink = true)
        {
            var obj = InventoryObjects[index];
            var next = obj.next;

            if (ClearLink)
            {
                // //Find the object chain the object is in and clear the list head index        
                var LinkOffset = GetItemLinkOffset(index);
                if (LinkOffset != -1)
                {
                    if (LinkOffset < InventoryPtr)
                    {
                        //offset is directly on paper doll. it should have no next. rare issue in Uw1 where a paperdoll object still has a next.
                        next = 0;
                    }

                    //This clears either the next or link to the object and replaces it with the objects next value
                    var data = (int)getAt(pdat, (LinkOffset), 16);
                    data &= 0x3f; //Clear link/next
                    data |= (next << 6); //Or in the obj.next for the object.
                    setAt(pdat, LinkOffset, 16, data);
                }
                else
                {
                    Debug.Print($"No link offset found for {index}. If you are not changing levels this should not happen");
                }
            }

            InventoryObjects[index] = null;

            if (updateUI)
            {
                if (uimanager.CurrentSlot >= 11)
                {
                    uimanager.BackPackIndices[uimanager.CurrentSlot - 11] = -1;
                }
                uimanager.UpdateInventoryDisplay();
            }
        }


        /// <summary>
        /// Finds the byte that links to the object at index. Returns either a next or link that points to the object
        /// </summary>
        /// <param name="IndexToFind"></param>
        /// <returns></returns>
        public static int GetItemLinkOffset(int IndexToFind)
        {
            //first check the paperdolls
            for (int slot = 0; slot <= 18; slot++)
            {
                if (GetInventorySlotListHead(slot) == IndexToFind)
                {
                    //found. 
                    //object is on the paperdoll at that slot. return that slot offset
                    return GetInventorySlotListHeadOffset(slot);
                }
            }

            //otherwise object is on an internal inventory list.
            //Find direct container link first.
            foreach (var objToCheck in InventoryObjects)
            {//if this far down then I need to find the container that the closing container sits in
                if (objToCheck != null)
                {
                    var result = objectsearch.GetContainingObject(
                        ListHead: objToCheck.index,
                        ToFind: IndexToFind,
                        objList: InventoryObjects);
                    if (result != -1)
                    {//container found.
                     //Get either the container link or browse the next chain to match the object
                        if (objToCheck.link == IndexToFind) //check the first object in the container
                        {
                            return objToCheck.PTR + 6;
                        }
                        else
                        {
                            var NextObj = InventoryObjects[objToCheck.link]; //get the first object in the container
                            while (NextObj.next != 0)
                            {
                                if (NextObj.next == IndexToFind)
                                {
                                    return NextObj.PTR + 4;
                                }
                                else
                                {
                                    NextObj = InventoryObjects[NextObj.next];
                                }
                            }
                        }
                    }
                }
            }
            return -1; //not found!
        }


        /// <summary>
        /// Adds an object from the inventory from inventory to the world
        /// </summary>
        /// <param name="objIndex"></param>
        /// <param name="updateUI"></param>
        /// <param name="RemoveNext"></param>
        /// <returns></returns>
        public static int AddInventoryObjectToWorld(int objIndex, bool updateUI, bool RemoveNext, bool DestroyInventoryObject = true, bool ClearLink = true)
        {
            var oldObj = InventoryObjects[objIndex];

            // recursively add linked/next objects to the world first. Updating oldobj so that its link/next is pointing to the world
            if (oldObj.link != 0 && oldObj.is_quant == 0)
            {
                oldObj.link = (short)AddInventoryObjectToWorld(
                    objIndex: oldObj.link,
                    updateUI: false,
                    RemoveNext: true,
                    ClearLink: false
                    );
                Debug.Print($"Link is now {oldObj.link}");
            }
            if (oldObj.next != 0 && RemoveNext)
            {
                oldObj.next = (short)AddInventoryObjectToWorld(
                    objIndex: oldObj.next,
                    updateUI: false,
                    RemoveNext: true,
                    ClearLink: false);
                Debug.Print($"Next is now {oldObj.next}");
            }

            //Now get the index to store at
            //UWTileMap.current_tilemap.StaticFreeListPtr--;
            //Debug.Print($"Allocating {UWTileMap.current_tilemap.StaticFreeListObject} (pointer decremented)");

            var newIndex = ObjectFreeLists.GetAvailableObjectSlot();

            //(short)UWTileMap.current_tilemap.StaticFreeListObject;
            var NewObj = UWTileMap.current_tilemap.LevelObjects[newIndex];
            //copy data to that index.
            for (int i = 0; i < 8; i++)
            {
                NewObj.DataBuffer[NewObj.PTR + i] = oldObj.DataBuffer[oldObj.PTR + i];
            }
            //special cases
            if ((NewObj.majorclass == 2) && (NewObj.minorclass == 1) && (NewObj.classindex >= 4) && (NewObj.classindex <= 7))
            {
                //light source that is turned on.
                light.LightOff(NewObj);
            }
            if (DestroyInventoryObject)
            {
                //remove inventory obj
                RemoveFromInventory(
                    index: objIndex,
                    updateUI: updateUI,
                    ClearLink: ClearLink);
            }
            return newIndex;
        }



        /// <summary>
        /// Recursively adds object data to player inventory and returns the index it is at.
        /// </summary>
        /// <param name="objIndex"></param>
        /// <param name="ObjectToAddTo"></param>
        /// <returns></returns>
        public static short AddObjectToPlayerInventory(int objIndex, bool IncludeNext)
        {
            var oldObj = UWTileMap.current_tilemap.LevelObjects[objIndex];

            // recursively add linked/next objects
            if (oldObj.link != 0 && oldObj.is_quant == 0)
            {
                if (oldObj.item_id == 288)//a_spell. stop here.
                {
                    //do nothing
                    Debug.Print("picked up a_spell");
                    //TODO add handling of traps/triggers
                }
                else
                {
                    oldObj.link = AddObjectToPlayerInventory(oldObj.link, true);
                    Debug.Print($"link is now {oldObj.link}");
                }

            }
            if (oldObj.next != 0 && IncludeNext)
            {
                oldObj.next = AddObjectToPlayerInventory(oldObj.next, true);
                Debug.Print($"Next is now {oldObj.next}");
            }

            var newIndex = MoveObjectToInventoryData(objIndex);
            return (short)newIndex;
        }

        /// <summary>
        /// Justs adds object at objIndex in the gameworld to the inventory data at the next free slot.
        /// </summary>
        /// <param name="objIndex"></param>
        /// <returns></returns>
        private static int MoveObjectToInventoryData(int objIndex)
        {
            var obj = UWTileMap.current_tilemap.LevelObjects[objIndex];
            // if (objIndex >= 256)
            // {
            //     //Add to static free list                
            //     Debug.Print($"Freeing {objIndex} (pointer incremented)");
            //     UWTileMap.current_tilemap.StaticFreeListObject = objIndex;
            //     UWTileMap.current_tilemap.StaticFreeListPtr++;
            // }
            // else
            // {
            //     // TODO mobile              //Add to mobile free list
            //     // UWTileMap.current_tilemap.MobileFreeListPtr++;
            //     // UWTileMap.current_tilemap.MobileFreeListObject = objIndex;
            // }
            var newIndex = NextFreeInventorySlot; //++LastItemIndex;
            Debug.Print($"Moving object at index {objIndex} to {newIndex}");
            InventoryObjects[newIndex] = new uwObject
            {
                //isInventory = true,
                IsStatic = true,
                index = (short)(newIndex),
                PTR = InventoryPtr + (newIndex - 1) * 8,
                DataBuffer = pdat
            };
            var NewObj = InventoryObjects[newIndex];

            //copy data to this offset.... and wipe the old
            for (int i = 0; i < 8; i++)
            {
                InventoryObjects[newIndex].DataBuffer[NewObj.PTR + i] = obj.DataBuffer[obj.PTR + i];
                obj.DataBuffer[obj.PTR + i] = 0;
            }

            //Destroy the world object
            ObjectFreeLists.ReleaseFreeObject(obj);
            return newIndex;
        }



        /// <summary>
        /// Future proof. Will test if player can carry the weight of this object based on it's stack size.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool CanCarryWeight(uwObject obj)
        {
            Debug.Print("unimplemented weight check");
            return true;
        }

        /// <summary>
        /// Weight check based on a single instance of an item
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static bool CanCarryWeight(int item_id)
        {
            Debug.Print("unimplemented weight check");
            return true;
        }

    } //end class
} //end namespace