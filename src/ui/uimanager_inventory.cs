using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {

        private static void InteractWithEmptySlot()
        {
            //there is no object in the slot.
            switch (InteractionMode)
            {
                case InteractionModes.ModePickup:
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
                            playerdat.MoveObjectInHandOutOfOpenedContainer(ObjectToPickup);
                            break;
                        }
                    case >= 0 and <= 10:
                        {//paperdoll
                            var index = playerdat.AddObjectToPlayerInventory(ObjectToPickup, false);
                            playerdat.SetInventorySlotListHead(CurrentSlot, index);
                            //redraw                
                            RefreshSlot(CurrentSlot);
                            if (ObjectToPickup==playerdat.ObjectInHand)
                            {
                                playerdat.ObjectInHand = -1;
                                instance.mousecursor.ResetCursor();
                            }

                            break;
                        }
                    case >= 11 and <= 18:
                        //backpack
                        if (playerdat.OpenedContainer < 0)
                        {//backpackslot
                            var index = playerdat.AddObjectToPlayerInventory(ObjectToPickup, false);
                            playerdat.SetInventorySlotListHead(CurrentSlot, index);

                            //redraw
                            playerdat.BackPackIndices[CurrentSlot - 11] = index;
                            RefreshSlot(CurrentSlot);
                            if (ObjectToPickup==playerdat.ObjectInHand)
                            {                               
                                playerdat.ObjectInHand = -1;
                                instance.mousecursor.ResetCursor();                                
                            }
                        }
                        else
                        {
                            //in a container 
                            var index = playerdat.AddObjectToPlayerInventory(ObjectToPickup, false);
                            var newobj = playerdat.InventoryObjects[index];
                            //add to linked list, for the moment it will be at the head but later on should be at the end (next of the object to it's left)
                            var opened = playerdat.InventoryObjects[playerdat.OpenedContainer];
                            newobj.next = opened.link;
                            opened.link = index;

                            //redraw
                            playerdat.BackPackIndices[CurrentSlot - 11] = index;
                            RefreshSlot(CurrentSlot);
                            if (ObjectToPickup==playerdat.ObjectInHand)
                            {                                
                                playerdat.ObjectInHand = -1;
                                instance.mousecursor.ResetCursor(); 
                            }
                        }
                        break;
                }

            }
        }

        


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
                    look.LookAt(objAtSlot, playerdat.InventoryObjects, false); break;
                case InteractionModes.ModePickup:
                    if (slotname == "OpenedContainer")
                    {
                        if ((playerdat.ObjectInHand != -1) && (!isLeftClick))
                        {
                            playerdat.MoveObjectInHandOutOfOpenedContainer(playerdat.ObjectInHand);
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
                            playerdat.UseObjectsTogether(playerdat.ObjectInHand, objAtSlot);
                        }
                        else
                        {
                            if (isLeftClick)
                            {//use the object at that slot
                                use.Use(
                                    index: objAtSlot,
                                    objList: playerdat.InventoryObjects,
                                    WorldObject: false);
                            }
                            else
                            {
                                //try and pickup
                                playerdat.PickupObjectFromSlot(objAtSlot);
                            }
                        }
                    }
                    break;
                default:
                    Debug.Print("Unimplemented inventory use verb-object combination"); break;
            }
        }



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

                case "Back0": { obj = playerdat.GetBackPackIndex(0); CurrentSlot = 11; break; }
                case "Back1": { obj = playerdat.GetBackPackIndex(1); CurrentSlot = 12; break; }
                case "Back2": { obj = playerdat.GetBackPackIndex(2); CurrentSlot = 13; break; }
                case "Back3": { obj = playerdat.GetBackPackIndex(3); CurrentSlot = 14; break; }
                case "Back4": { obj = playerdat.GetBackPackIndex(4); CurrentSlot = 15; break; }
                case "Back5": { obj = playerdat.GetBackPackIndex(5); CurrentSlot = 16; break; }
                case "Back6": { obj = playerdat.GetBackPackIndex(6); CurrentSlot = 17; break; }
                case "Back7": { obj = playerdat.GetBackPackIndex(7); CurrentSlot = 18; break; }
                case "OpenedContainer": { obj = playerdat.OpenedContainer; CurrentSlot = -1; break; }
                default:
                    CurrentSlot = -1;
                    Debug.Print("Unimplemented inventory slot"); break;
            }

            return obj;
        }




    }
}