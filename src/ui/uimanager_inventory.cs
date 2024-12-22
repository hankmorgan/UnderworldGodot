using System.Diagnostics;
using Godot;
using Peaky.Coroutines;
using System.Collections;

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
            bool CheckForEnchantments = false;
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
                                if (playerdat.ObjectInHand != 1)
                                {
                                    if (TryEatInteraction())
                                    {
                                        return; //food has been consumed. exit function.
                                    }
                                }
                            }

                            //Test if object is valid for slot
                            if (!ValidObjectForSlot(CurrentSlot, UWTileMap.current_tilemap.LevelObjects[ObjectToPickup]))
                            {
                                return;
                            }
                            var index = playerdat.AddObjectToPlayerInventory(ObjectToPickup, false);
                            playerdat.SetInventorySlotListHead(CurrentSlot, index);
                            //redraw                
                            RefreshSlot(CurrentSlot);
                            if (ObjectToPickup == playerdat.ObjectInHand)
                            {
                                playerdat.ObjectInHand = -1;
                                instance.mousecursor.SetCursorToCursor();
                            }
                            //the object added to the paper doll may have a (curse) enchantment that may need to be activated
                            CheckForEnchantments = true;
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
                                instance.mousecursor.SetCursorToCursor();
                            }
                        }
                        else
                        {//into a container directly. test if it can hold the object
                            if (container.TestContainerCanHold(
                                containerobject: playerdat.InventoryObjects[OpenedContainerIndex],
                                objectToAdd: UWTileMap.current_tilemap.LevelObjects[ObjectToPickup],
                                printReason: true))
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
                                    instance.mousecursor.SetCursorToCursor();
                                }
                            }
                        }
                        break;
                }
                //Update player state
                playerdat.PlayerStatusUpdate(CastOnEquip: CheckForEnchantments);
            }
        }


        /// <summary>
        /// Handles dropping food items on the players head
        /// </summary>
        /// <returns></returns>
        private static bool TryEatInteraction()
        {
            var objInHand = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];
            if (objInHand.majorclass == 2 && objInHand.minorclass == 3)
            {
                food.Use(objInHand, true);
                return true;
            }
            else
            {
                if (food.SpecialFoodCases(
                    obj: objInHand,
                    UsedFromInventory: false))
                {//if any food special case occurs exit sub
                    return true;
                }
            }
            return false;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotname"></param>
        /// <param name="objAtSlot"></param>
        /// <param name="isLeftClick"></param>
        private static void InteractWithObjectInSlot(string slotname, int objAtSlot, bool isLeftClick)
        {
            if ((MessageDisplay.WaitingForTypedInput) || (MessageDisplay.WaitingForYesOrNo))
            {//stop while typing in progress
                return;
            }
            if (InteractionMode == InteractionModes.ModeAttack)
            {
                if (combat.stage != combat.CombatStages.Ready)
                {
                    return;
                }
            }

            if (CurrentSlot == 0) //HELM
            {
                //try the eat interation.
                if (playerdat.ObjectInHand != 1)
                {
                    if (TryEatInteraction())
                    {
                        return; //food has been consumed. exit function.
                    }
                }
            }
            
            switch (InteractionMode)
            {
                case InteractionModes.ModeAttack:
                case InteractionModes.ModeUse:
                    if (slotname != "OpenedContainer")
                    {
                        if (isLeftClick)
                        {
                            use.Use(
                                index: objAtSlot,
                                objList: playerdat.InventoryObjects,
                                WorldObject: false);
                        }
                        else
                        {
                            //do pickup
                            //try and pickup
                            uimanager.InteractionModeToggle(InteractionModes.ModePickup);
                            PickupObjectFromSlot(objAtSlot);
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
                case InteractionModes.ModeLook:
                    if (slotname != "OpenedContainer")
                    {
                        if (isLeftClick)
                        {
                            look.LookAt(objAtSlot, playerdat.InventoryObjects, false);
                        }
                        else
                        {
                            uimanager.InteractionModeToggle(InteractionModes.ModeUse);
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
                            UseObjectsTogether(playerdat.ObjectInHand, objAtSlot);
                        }
                        else
                        {
                            if (!ConversationVM.InConversation)
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
                            {//click on a slot in a conversation
                                if (isLeftClick)
                                {
                                    //left click pickup in conversation
                                    var obj = playerdat.InventoryObjects[objAtSlot];
                                    if ((obj.majorclass == 2) && (obj.minorclass == 0))
                                    {
                                        AddToMessageScroll(
                                            stringToAdd: GameStrings.GetString(1, GameStrings.str_you_cannot_barter_a_container__instead_remove_the_contents_you_want_to_trade_),
                                            option: 2,
                                            mode: MessageDisplay.MessageDisplayMode.TemporaryMessage
                                            );
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
                                            look.PrintLookDescription(
                                                obj: obj,
                                                objList: playerdat.InventoryObjects,
                                                OutputConvo: true,
                                                lorecheckresult: look.LoreCheck(obj));
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



        public static int GetPaperDollObjAtSlot(int slotindex)
        {
            var obj = -1;
            switch (slotindex)
            {
                case 0: { obj = playerdat.Helm; break; }
                case 1: { obj = playerdat.ChestArmour; break; }
                case 2: { obj = playerdat.Gloves; break; }
                case 3: { obj = playerdat.Leggings; break; }
                case 4: { obj = playerdat.Boots; break; }

                case 5: { obj = playerdat.RightShoulder; break; }
                case 6: { obj = playerdat.LeftShoulder; break; }
                case 7: { obj = playerdat.RightHand; break; }
                case 8: { obj = playerdat.LeftHand; break; }

                case 9: { obj = playerdat.RightRing; break; }
                case 10: { obj = playerdat.LeftRing; break; }
            }
            return obj;
        }
        /// <summary>
        /// Gets the object at the named inventory slot
        /// </summary>
        /// <param name="slotname"></param>
        /// <returns></returns>
        public static int GetObjAtSlot(string slotname)
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
            if ((MessageDisplay.WaitingForTypedInput) || (MessageDisplay.WaitingForYesOrNo))
            {//stop while typing in progress
                return;
            }
            var target = playerdat.InventoryObjects[targetObj];
            var source = UWTileMap.current_tilemap.LevelObjects[srcObj];

            if ((target.majorclass == 2) && (target.minorclass == 0) && (target.classindex != 0xF))
            {
                //containers excluding the runebag.
                //add object to container
                if (container.TestContainerCanHold(target, source))
                {
                    var Added = playerdat.AddObjectToPlayerInventory(srcObj, false);
                    var AddedObj = playerdat.InventoryObjects[Added];
                    AddedObj.next = target.link;
                    target.link = Added;
                    playerdat.ObjectInHand = -1;
                    uimanager.instance.mousecursor.SetCursorToCursor();
                }
                playerdat.PlayerStatusUpdate();
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
                        playerdat.ObjectInHand = -1; uimanager.instance.mousecursor.SetCursorToCursor();
                    }
                }
                return;
            }

            if (target.item_id == source.item_id)
            {
                if ((commonObjDat.stackable(target.item_id) & 0x1) == 0)
                {//and is stackable.
                    //average out the quality between the stacks
                    target.quality = (short)(((target.quality * target.ObjectQuantity) + (source.quality * source.ObjectQuantity)) / (target.ObjectQuantity + source.ObjectQuantity));

                    //change the quantities.
                    target.link += source.link;
                    target.is_quant = 1;

                    UpdateInventoryDisplay();
                    //destroy the source.
                    ObjectFreeLists.ReleaseFreeObject(source); //object in hand
                    playerdat.ObjectInHand = -1; uimanager.instance.mousecursor.SetCursorToCursor();
                    return;
                }
            }
            //try item combinations
            if (objectCombination.TryObjectCombination(target, source))
            {
                return;
            }

            //swap objects otherwise if object is valid for slot
            if (ValidObjectForSlot(CurrentSlot, source))
            {
                var backup = playerdat.ObjectInHand;
                PickupObjectFromSlot(targetObj);
                PickupToEmptySlot(backup);
                UpdateInventoryDisplay();
            }


            //Update player state
            playerdat.PlayerStatusUpdate();
        }

        public static void PickupObjectFromSlot(int objAtSlot)
        {
            if ((MessageDisplay.WaitingForTypedInput) || (MessageDisplay.WaitingForYesOrNo))
            {//stop while another in progress
                return;
            }
            var obj = playerdat.InventoryObjects[objAtSlot];

            if (obj.ObjectQuantity > 1)
            {//object is a quantity, prompty for pickup size and then complete pickup
                //prompt for quantity in coroutine.
                _ = Peaky.Coroutines.Coroutine.Run(
                        DoPickupQty(objAtSlot: objAtSlot, currslot: CurrentSlot),
                        main.instance
                    );
            }
            else
            {
                //just pick it up
                DoPickup(objAtSlot);
            }
        }



        /// <summary>
        /// Does the pickup from inventory.
        /// </summary>
        /// <param name="objAtSlot"></param>
        /// <param name="DestroyInventoryObject"></param>
        /// <returns></returns>
        public static int DoPickup(int objAtSlot, bool DestroyInventoryObject = true)
        {
            var newIndex = playerdat.AddInventoryObjectToWorld(
                    objIndex: objAtSlot,
                    updateUI: true,
                    RemoveNext: false,
                    DestroyInventoryObject: DestroyInventoryObject,
                    ClearLink: true
                    );
            var pickObject = UWTileMap.current_tilemap.LevelObjects[newIndex];
            pickObject.next=0;
            playerdat.ObjectInHand = newIndex;
            instance.mousecursor.SetCursorToObject(pickObject.item_id);
            playerdat.PlayerStatusUpdate();//check if any enchantments need to be updated
            return newIndex;
        }

        public static void MoveObjectInHandOutOfOpenedContainer(int ObjectToPickup)
        {
            //the opened container
            if (OpenedContainerParent == -1)
            {
                //on paperdoll. try and add to there
                var freeslot = FreePaperDollSlot;
                if (freeslot != -1)
                {
                    var index = playerdat.AddObjectToPlayerInventory(ObjectToPickup, false);
                    playerdat.SetInventorySlotListHead(freeslot, index);
                    if (ObjectToPickup == playerdat.ObjectInHand)
                    {
                        playerdat.ObjectInHand = -1;
                        instance.mousecursor.SetCursorToCursor();
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
                var container = playerdat.InventoryObjects[OpenedContainerParent];
                newobj.next = container.link;
                container.link = index;
                playerdat.ObjectInHand = -1;
                instance.mousecursor.SetCursorToCursor();
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


        /// <summary>
        /// Handles pickup up stacks of objects which need to be split
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerator DoPickupQty(int objAtSlot, int currslot)
        {
            MessageScrollLine[] linesToRestore = BackupLines(instance.scroll.Lines, 5);
            var obj = playerdat.InventoryObjects[objAtSlot];
            MessageDisplay.WaitingForTypedInput = true;

            instance.TypedInput.Text = obj.ObjectQuantity.ToString();
            instance.scroll.Clear();
            AddToMessageScroll("Move how many? {TYPEDINPUT}|", mode: MessageDisplay.MessageDisplayMode.TypedInput);

            while (MessageDisplay.WaitingForTypedInput)
            {
                yield return new WaitOneFrame();
            }
            //restore currentslot as this gets wiped once the calling function has ended
            CurrentSlot = currslot;

            var response = instance.TypedInput.Text;
            if (int.TryParse(response, out int result))
            {
                if (result > 0)
                {
                    if (obj.ObjectQuantity <= result)
                    {//at least all of the stack is selected                        
                        DoPickup(objAtSlot);
                    }
                    else
                    {
                        //if <quantity selected, split objects, pickup object of that quantity.
                        var newObjIndex = ObjectCreator.SpawnObjectInHand(obj.item_id); //spawning in hand is very handy here
                        var newObj = UWTileMap.current_tilemap.LevelObjects[newObjIndex];
                        newObj.link = (short)result;
                        newObj.quality = obj.quality;
                        newObj.owner = obj.owner;
                        //TODO. see if other object properties need copying.                    
                        obj.link = (short)(obj.link - result);//reduce the other object.
                    }
                }
            }
            UpdateInventoryDisplay();
            if (ConversationVM.InConversation)
            {
                //restore lines.
                for (int i = 0; i <= linesToRestore.GetUpperBound(0); i++)
                {
                    instance.scroll.Lines[i] = new MessageScrollLine(linesToRestore[i].OptionNo, linesToRestore[i].LineText);
                }
                instance.scroll.UpdateMessageDisplay();
            }
            CurrentSlot = -1; //cleanup/needed?
            yield return true;
        }

        public static bool ValidObjectForSlot(int slot, uwObject obj)
        {//TODO: this should respect objects.dat settings
            //test game specific special objects
            switch (UWClass._RES)
            {
                case UWClass.GAME_UW2:
                    {
                        switch (obj.item_id)
                        {
                            case 47://	a_pair of swamp boots&pairs of swamp boots
                                return slot == 4;
                            case 51://	fraznium gauntlets&pairs of fraznium gauntlets
                                return slot == 2;
                            case 52://	a_fraznium circlet
                                return slot == 0;
                            case 53://	a_Guardian signet ring
                            case 55://	a_copper ring
                                return (slot == 9) || (slot == 10);
                        }
                        break;
                    }

                default:
                    {
                        switch (obj.item_id)
                        {
                            case 47: //a_pair of dragon skin boots&pairs of dragon skin boots
                                return slot == 4;
                            case 54://an_iron ring
                                return (slot == 9) || (slot == 10);
                        }
                        break;
                    }
            }
            //standard rules
            switch (slot)
            {
                case 0: //helm. 
                    return
                        obj.majorclass == 0
                        &&
                        (
                            (obj.minorclass == 2) && (obj.classindex >= 0xC)
                            ||
                            (obj.minorclass == 3) && (obj.classindex <= 2)
                        );
                case 1://body armour
                    return
                        obj.majorclass == 0
                        &&
                        obj.minorclass == 2
                        &&
                        (obj.classindex >= 0 || obj.classindex <= 2);
                case 2://gloves
                    return
                        obj.majorclass == 0
                        &&
                        obj.minorclass == 2
                        &&
                        (obj.classindex >= 7 || obj.classindex <= 8);
                case 3://leggings
                    return
                        obj.majorclass == 0
                        &&
                        obj.minorclass == 2
                        &&
                        (obj.classindex >= 3 || obj.classindex <= 5);
                case 4://boots
                    return
                        obj.majorclass == 0
                        &&
                        obj.minorclass == 2
                        &&
                        (obj.classindex >= 9 || obj.classindex <= 0xB);
                case 9:
                case 10:
                    return
                        obj.majorclass == 0
                        &&
                        obj.minorclass == 2
                        &&
                        (obj.classindex >= 9 || obj.classindex <= 0xB);
            }

            return true;
        }
    }//end class
}//end namespace