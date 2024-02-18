using System.Diagnostics;
using System.Diagnostics.Tracing;
using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        /// <summary>
        /// The currently displayed backpack objects.
        /// </summary>
        public static int[] BackPackIndices = new int[8];

        /// <summary>
        /// The container currently opened on the paperdoll.
        /// </summary>
        public static int OpenedContainerIndex = -1;

        private static void InteractWithEmptySlot()
        {
            //there is no object in the slot.
            switch (InteractionMode)
            {
                case InteractionModes.ModePickup:
                case InteractionModes.ModeTalk:
                    {
                        PickupToEmptySlot(playerdat.ObjectInHand);
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles clicking on an empty inventory slot with an object in hand
        /// </summary>
        public static void PickupToEmptySlot(int ObjectToPickup)
        {
            if (ObjectToPickup != -1)
            {
                switch (CurrentSlot)
                {
                    case -1:
                        {
                            MoveObjectInHandOutOfOpenedContainer(ObjectToPickup);
                            break;
                        }
                    case >= 0 and <= 10:
                        {//paperdoll
                            if (CurrentSlot == 0) //HELM
                                {
                                    //try the eat interation.
                                    if (playerdat.ObjectInHand!=1)
                                    {
                                        var objInHand = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];
                                        if (objInHand.majorclass==2 && objInHand.minorclass==3)
                                        {
                                            food.Use(objInHand, true);
                                            return;
                                        }
                                        else
                                        {
                                            if (food.SpecialFoodCases(
                                                obj: objInHand, 
                                                UsedFromInventory: false))
                                            {//if any food special case occurs exit sub
                                                return;
                                            }
                                        }
                                    }
                                }
                            var index = playerdat.AddObjectToPlayerInventory(ObjectToPickup, false);
                            playerdat.SetInventorySlotListHead(CurrentSlot, index);
                            //redraw                
                            RefreshSlot(CurrentSlot);
                            if (ObjectToPickup == playerdat.ObjectInHand)
                            {
                                playerdat.ObjectInHand = -1;
                                instance.mousecursor.ResetCursor();
                            }

                            break;
                        }
                    case >= 11 and <= 18:
                        //backpack
                        if (uimanager.OpenedContainerIndex < 0)
                        {//backpackslot
                            var index = playerdat.AddObjectToPlayerInventory(ObjectToPickup, false);
                            playerdat.SetInventorySlotListHead(CurrentSlot, index);

                            //redraw
                            BackPackIndices[CurrentSlot - 11] = index;
                            RefreshSlot(CurrentSlot);
                            if (ObjectToPickup == playerdat.ObjectInHand)
                            {
                                playerdat.ObjectInHand = -1;
                                instance.mousecursor.ResetCursor();
                            }
                        }
                        else
                        {
                            int occupiedslots;
                            var containerobjects = container.GetObjects(
                                ContainerIndex: uimanager.OpenedContainerIndex,
                                objList: playerdat.InventoryObjects,
                                OccupiedSlots: out occupiedslots,
                                start: 0,
                                count: 512
                                );
                            //find the object that is previous to the current slot.
                            var start = BackPackStart + CurrentSlot - 11 - 1;
                            var previousObjectIndex = -1;
                            while (start >= 0)
                            {
                                if (containerobjects[start] != -1)
                                {
                                    previousObjectIndex = containerobjects[start];
                                    break;
                                }
                                start--;
                            }

                            //in a container 
                            var index = playerdat.AddObjectToPlayerInventory(ObjectToPickup, false);
                            var newobj = playerdat.InventoryObjects[index];

                            if (previousObjectIndex != -1)
                            {
                                //add as a next to that object
                                var prev = playerdat.InventoryObjects[previousObjectIndex];
                                newobj.next = prev.next;
                                prev.next = newobj.index;
                            }
                            else
                            {
                                //add to the container link
                                var opened = playerdat.InventoryObjects[uimanager.OpenedContainerIndex];
                                newobj.next = opened.link;
                                opened.link = index;
                            }

                            //redraw
                            BackPackIndices[CurrentSlot - 11] = index;
                            UpdateInventoryDisplay();
                            if (ObjectToPickup == playerdat.ObjectInHand)
                            {
                                playerdat.ObjectInHand = -1;
                                instance.mousecursor.ResetCursor();
                            }
                        }
                        break;
                }

            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotname"></param>
        /// <param name="objAtSlot"></param>
        /// <param name="isLeftClick"></param>
        private static void InteractWithObjectInSlot(string slotname, int objAtSlot, bool isLeftClick)
        {
            switch (InteractionMode)
            {
                case InteractionModes.ModeUse:
                    if (slotname != "OpenedContainer")
                    {
                        use.Use(
                            index: objAtSlot,
                            objList: playerdat.InventoryObjects,
                            WorldObject: false);
                    }
                    else
                    {
                        //close up opened container.
                        container.Close(
                            index: objAtSlot,
                            objList: playerdat.InventoryObjects);
                    }
                    break;
                case InteractionModes.ModeLook:
                    if (slotname != "OpenedContainer")
                    {
                        if (isLeftClick)
                        {
                            look.LookAt(objAtSlot, playerdat.InventoryObjects, false);
                        }
                        else
                        {
                            //use
                            use.Use(
                                index: objAtSlot,
                                objList: playerdat.InventoryObjects,
                                WorldObject: false);
                        }

                    }
                    else
                    {
                        //close up opened container.
                        container.Close(
                            index: objAtSlot,
                            objList: playerdat.InventoryObjects);
                    }

                    break;

                case InteractionModes.ModeTalk://same as pickup except no use
                case InteractionModes.ModePickup:
                    if (slotname == "OpenedContainer")
                    {
                        if ((playerdat.ObjectInHand != -1) && (!isLeftClick))
                        {
                            MoveObjectInHandOutOfOpenedContainer(playerdat.ObjectInHand);
                        }
                        else
                        {
                            //close up opened container when no obj in hand or when leftclicking
                            container.Close(
                                index: objAtSlot,
                                objList: playerdat.InventoryObjects);
                        }
                    }
                    else
                    {
                        if (playerdat.ObjectInHand != -1)
                        {
                            //do a use interaction on the object already there. 
                            uimanager.UseObjectsTogether(playerdat.ObjectInHand, objAtSlot);
                        }
                        else
                        {
                            if (InteractionMode != InteractionModes.ModeTalk)
                            {
                                if (isLeftClick)
                                {
                                    //try and pickup
                                    PickupObjectFromSlot(objAtSlot);
                                }
                                else
                                {
                                    //use the object at that slot
                                    use.Use(
                                        index: objAtSlot,
                                        objList: playerdat.InventoryObjects,
                                        WorldObject: false);
                                }
                            }
                            else
                            {//click on a slot in talk mode
                                if (isLeftClick)
                                {

                                    //left click pickup in conversation
                                    var obj = playerdat.InventoryObjects[objAtSlot];
                                    if ((obj.majorclass == 2) && (obj.minorclass == 0))
                                    {
                                        AddToConvoScroll(GameStrings.GetString(1, GameStrings.str_you_cannot_barter_a_container__instead_remove_the_contents_you_want_to_trade_), 2);
                                    }
                                    else
                                    {
                                        //try and pickup if not a container
                                        PickupObjectFromSlot(objAtSlot);
                                    }



                                }
                                else
                                {//check if container.
                                    var obj = playerdat.InventoryObjects[objAtSlot];
                                    if ((obj.majorclass == 2) && (obj.minorclass == 0) && (obj.classindex != 0xF))
                                    {//containers, browse into
                                        use.Use(
                                            index: objAtSlot,
                                            objList: playerdat.InventoryObjects,
                                            WorldObject: false);
                                    }
                                    else
                                    {
                                        if ((obj.majorclass == 2) && (obj.minorclass == 0) && (obj.classindex == 0xF))
                                        {//runebag. ignore.

                                        }
                                        else
                                        {//all other objects look at
                                            look.GeneralLookDescription(obj: obj, OutputConvoScroll: true);
                                        }
                                    }

                                }

                            }

                        }
                    }
                    break;
                default:
                    Debug.Print("Unimplemented inventory use verb-object combination"); break;
            }
        }


        /// <summary>
        /// Gets the object at the named inventory slot
        /// </summary>
        /// <param name="slotname"></param>
        /// <returns></returns>
        private static int GetObjAtSlot(string slotname)
        {
            var obj = -1;
            switch (slotname)
            {
                case "Helm": { obj = playerdat.Helm; CurrentSlot = 0; break; }
                case "Armour": { obj = playerdat.ChestArmour; CurrentSlot = 1; break; }
                case "Gloves": { obj = playerdat.Gloves; CurrentSlot = 2; break; }
                case "Leggings": { obj = playerdat.Leggings; CurrentSlot = 3; break; }
                case "Boots": { obj = playerdat.Boots; CurrentSlot = 4; break; }

                case "RightShoulder": { obj = playerdat.RightShoulder; CurrentSlot = 5; break; }
                case "LeftShoulder": { obj = playerdat.LeftShoulder; CurrentSlot = 6; break; }
                case "RightHand": { obj = playerdat.RightHand; CurrentSlot = 7; break; }
                case "LeftHand": { obj = playerdat.LeftHand; CurrentSlot = 8; break; }

                case "RightRing": { obj = playerdat.RightRing; CurrentSlot = 9; break; }
                case "LeftRing": { obj = playerdat.LeftRing; CurrentSlot = 10; break; }

                case "Back0": { obj = GetBackPackIndex(0); CurrentSlot = 11; break; }
                case "Back1": { obj = GetBackPackIndex(1); CurrentSlot = 12; break; }
                case "Back2": { obj = GetBackPackIndex(2); CurrentSlot = 13; break; }
                case "Back3": { obj = GetBackPackIndex(3); CurrentSlot = 14; break; }
                case "Back4": { obj = GetBackPackIndex(4); CurrentSlot = 15; break; }
                case "Back5": { obj = GetBackPackIndex(5); CurrentSlot = 16; break; }
                case "Back6": { obj = GetBackPackIndex(6); CurrentSlot = 17; break; }
                case "Back7": { obj = GetBackPackIndex(7); CurrentSlot = 18; break; }
                case "OpenedContainer": { obj = uimanager.OpenedContainerIndex; CurrentSlot = -1; break; }
                default:
                    CurrentSlot = -1;
                    Debug.Print("Unimplemented inventory slot"); break;
            }

            return obj;
        }



        /// <summary>
        /// Stores an array listing the currently displayed backpack objects indices
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="obj"></param>
        public static void SetBackPackIndex(int slot, uwObject obj)
        {
            if (obj != null)
            {
                BackPackIndices[slot] = obj.index;
            }
            else
            {
                BackPackIndices[slot] = -1;
            }
        }

        /// <summary>
        /// Get the index of the object at the currently displayed backpack slot
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static int GetBackPackIndex(int slot)
        {
            return BackPackIndices[slot];
        }


        /// <summary>
        /// Uses a srcobject from world on a target object in inventory
        /// </summary>
        /// <param name="srcObj"></param>
        /// <param name="targetObj"></param>
        public static void UseObjectsTogether(int srcObj, int targetObj)
        {
            var target = playerdat.InventoryObjects[targetObj];
            var source = UWTileMap.current_tilemap.LevelObjects[srcObj];

            if ((target.majorclass == 2) && (target.minorclass == 0) && (target.classindex != 0xF))
            {
                //containers excluding the runebag.
                //add object to container
                var Added = playerdat.AddObjectToPlayerInventory(srcObj, false);
                var AddedObj = playerdat.InventoryObjects[Added];
                AddedObj.next = target.link;
                target.link = Added;
                playerdat.ObjectInHand = -1;
                uimanager.instance.mousecursor.ResetCursor();
                return;
            }

            if ((target.majorclass == 2) && (target.minorclass == 0) && (target.classindex == 0xF))
            {
                if (source.majorclass == 3)
                {
                    if (
                        (source.minorclass == 2) && (source.classindex >= 8)
                        ||
                         (source.minorclass == 3)
                    )
                    {
                        //the runebag. add to runes if source is a rune.
                        int runeid = source.item_id - 232;
                        playerdat.SetRune(runeid, true);
                        playerdat.ObjectInHand = -1; uimanager.instance.mousecursor.ResetCursor();
                    }
                }
                return;
            }

            //try item combinations
            if (objectCombination.TryObjectCombination(target, source))
            {
                return;
            }

            //swap objects otherwise
            var backup = playerdat.ObjectInHand;
            PickupObjectFromSlot(targetObj);
            uimanager.PickupToEmptySlot(backup);
            uimanager.UpdateInventoryDisplay();
        }

        public static void PickupObjectFromSlot(int objAtSlot)
        {
            var newIndex = playerdat.AddInventoryObjectToWorld(
                    objIndex: objAtSlot,
                    updateUI: true,
                    RemoveNext: false);
            var pickObject = UWTileMap.current_tilemap.LevelObjects[newIndex];
            playerdat.ObjectInHand = newIndex;
            uimanager.instance.mousecursor.SetCursorArt(pickObject.item_id);
        }


        public static void MoveObjectInHandOutOfOpenedContainer(int ObjectToPickup)
        {
            //the opened container
            if (uimanager.OpenedContainerParent == -1)
            {
                //on paperdoll. try and add to there
                var freeslot = uimanager.FreePaperDollSlot;
                if (freeslot != -1)
                {
                    var index = playerdat.AddObjectToPlayerInventory(ObjectToPickup, false);
                    playerdat.SetInventorySlotListHead(freeslot, index);
                    if (ObjectToPickup == playerdat.ObjectInHand)
                    {
                        playerdat.ObjectInHand = -1;
                        uimanager.instance.mousecursor.ResetCursor();
                    }

                }
                else
                {
                    Debug.Print("No room on paperdoll");
                }
            }
            else
            {
                //the container that contains the opened container.
                var index = playerdat.AddObjectToPlayerInventory(playerdat.ObjectInHand, false);
                var newobj = playerdat.InventoryObjects[index];
                var container = playerdat.InventoryObjects[uimanager.OpenedContainerParent];
                newobj.next = container.link;
                container.link = index;
                playerdat.ObjectInHand = -1;
                uimanager.instance.mousecursor.ResetCursor();
            }
        }


        // /// <summary>
        // /// Finds and changes the backpack display array for the specific object indices
        // /// </summary>
        // /// <param name="index"></param>
        // /// <param name="newValue"></param>
        // static void FindAndChangeBackpackIndex(int index, int newValue = -1)
        // {
        //     for (int i = 0; i < BackPackIndices.GetUpperBound(0); i++)
        //     {
        //         if (BackPackIndices[i] == index)
        //         {
        //             BackPackIndices[i] = newValue;
        //         }
        //     }
        // }

    }//end class
}//end namespace