using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Paperdoll")]
        [Export] public TextureRect Body;
        [Export] public TextureRect Helm;
        [Export] public TextureRect Armour;
        [Export] public TextureRect ArmourInput;
        [Export] public TextureRect Leggings;
        [Export] public TextureRect LeggingsInput;
        [Export] public TextureRect Boots;
        [Export] public TextureRect Gloves;
        [Export] public TextureRect GlovesInput1;
        [Export] public TextureRect GlovesInput2;
        [Export] public TextureRect RightShoulder;
        [Export] public Label RightShoulderQTY;
        [Export] public TextureRect LeftShoulder;
        [Export] public Label LeftShoulderQTY;
        [Export] public TextureRect RightHand;
        [Export] public Label RightHandQTY;
        [Export] public TextureRect LeftHand;
        [Export] public Label LeftHandQTY;
        [Export] public TextureRect RightRing;
        [Export] public TextureRect RightRingInput;
        [Export] public TextureRect LeftRing;
        [Export] public TextureRect LeftRingInput;
        [Export] public TextureRect[] Backpack = new TextureRect[8];
        [Export] public Label[] BackpackQTY = new Label[8];
        [Export] public TextureRect ArrowUp;
        [Export] public TextureRect ArrowDown;
        [Export] public TextureRect OpenedContainer;
        public static int CurrentSlot;
        public static int BackPackStart = 0;


        public void InitPaperdoll()
        {
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                //Move paperdoll
                var offset = new Vector2(0, -12);
                PanelInventoryArt.Size = new Vector2(316, 448);
                PanelStatsArt.Size = new Vector2(316, 448);
                PanelRuneBagArt.Size = new Vector2(316, 448);

                // PanelInventory.Position += offset;
                // PanelStats.Position +=offset;
                // PanelRuneBag.Position +=offset;

                offset = new Vector2(-4, -12);
                Body.Position += offset;
                Helm.Position += offset;
                Boots.Position += offset;
                Gloves.Position += offset;
                Leggings.Position += offset;
                Armour.Position += offset;
                RightRing.Position += offset;
                LeftRing.Position += offset;
                RightRingInput.Position += offset;
                LeftRingInput.Position += offset;
                GlovesInput1.Position += offset;
                GlovesInput2.Position += offset;
                ArmourInput.Position += offset;
                LeggingsInput.Position += offset;

                offset = new Vector2(-4, -4);
                for (int i = 0; i < 4; i++)
                {
                    Backpack[i].Position += offset;
                }
                offset = new Vector2(-4, -2);
                for (int i = 4; i < 8; i++)
                {
                    Backpack[i].Position += offset;
                }

                offset = new Vector2(8, 44);
                for (int i = 0; i < 3; i++)
                {
                    SelectedRunes[i].Position += offset;
                }
            }

            EnableDisable(ArrowUp, false);
            EnableDisable(ArrowDown, false);
        }

        /// <summary>
        /// Sets the body image for the pc
        /// </summary>
        /// <param name="body"></param>
        /// <param name="isFemale"></param>
        public static void SetBody(int body, bool isFemale)
        {
            int MaleOrFemale = 0;
            if (isFemale)
            {
                MaleOrFemale = 1;
            }
            if (grBody == null)
            {
                grBody = new GRLoader(GRLoader.BODIES_GR, GRLoader.GRShaderMode.UIShader);
            }
            instance.Body.Texture = grBody.LoadImageAt(body + (5 * MaleOrFemale));
        }

        /// <summary>
		/// Redraws the specified slot
		/// </summary>
		/// <param name="slotno"></param>
		public static void RefreshSlot(int slotno)
        {
            switch (slotno)
            {
                case 0: SetHelm(playerdat.isFemale, wearable.GetSpriteIndex(playerdat.HelmObject)); break;
                case 1: SetArmour(playerdat.isFemale, wearable.GetSpriteIndex(playerdat.ChestArmourObject)); break;
                case 2: SetGloves(playerdat.isFemale, wearable.GetSpriteIndex(playerdat.GlovesObject)); break;
                case 3: SetLeggings(playerdat.isFemale, wearable.GetSpriteIndex(playerdat.LeggingsObject)); break;
                case 4: SetBoots(playerdat.isFemale, wearable.GetSpriteIndex(playerdat.BootsObject)); break;
                //Set arms and shoulders
                case 5: SetRightShoulder(uwObject.GetObjectSprite(playerdat.RightShoulderObject)); break;
                case 6: SetLeftShoulder(uwObject.GetObjectSprite(playerdat.LeftShoulderObject)); break;
                case 7: SetRightHand(uwObject.GetObjectSprite(playerdat.RightHandObject)); break;
                case 8: SetLeftHand(uwObject.GetObjectSprite(playerdat.LeftHandObject)); break;
                //set rings
                case 9: SetRightRing(ring.GetSpriteIndex(playerdat.RightRingObject)); break;
                case 10: SetLeftRing(ring.GetSpriteIndex(playerdat.LeftRingObject)); break;
                default:
                    if ((slotno >= 11) && (slotno <= 18))
                    {
                        uwObject objAtSlot = null;
                        if (playerdat.BackPackIndices[slotno - 11] != -1)
                        {
                            objAtSlot = playerdat.InventoryObjects[playerdat.BackPackIndices[slotno - 11]];
                        }
                        SetBackPackArt(
                            slot: slotno - 11,
                            SpriteNo: uwObject.GetObjectSprite(objAtSlot),
                            qty: uwObject.GetObjectQuantity(objAtSlot));
                    }
                    break;
            }
        }


        /// <summary>
		/// Sets the sprite for the helmet
		/// </summary>
		/// <param name="isFemale"></param>
		/// <param name="SpriteNo"></param>
		public static void SetHelm(bool isFemale, int SpriteNo = -1)
        {
            if (SpriteNo == -1)
            { //clear the slot
                instance.Helm.Texture = null;
            }
            else
            {
                instance.Helm.Texture = grArmour(isFemale).LoadImageAt(SpriteNo);
            }
        }


        /// <summary>
        /// Sets the sprite for the armour
        /// </summary>
        /// <param name="isFemale"></param>
        /// <param name="SpriteNo"></param>
        public static void SetArmour(bool isFemale, int SpriteNo = -1)
        {
            if (SpriteNo == -1)
            { //clear the slot
                instance.Armour.Texture = null;
            }
            else
            {
                instance.Armour.Texture = grArmour(isFemale).LoadImageAt(SpriteNo);
            }
        }


        /// <summary>
        /// Sets the sprite for the leggings
        /// </summary>
        /// <param name="isFemale"></param>
        /// <param name="SpriteNo"></param>
        public static void SetLeggings(bool isFemale, int SpriteNo = -1)
        {
            if (SpriteNo == -1)
            { //clear the slot
                instance.Leggings.Texture = null;
            }
            else
            {
                instance.Leggings.Texture = grArmour(isFemale).LoadImageAt(SpriteNo);
            }
        }


        /// <summary>
        /// Sets the sprite for the boots
        /// </summary>
        /// <param name="isFemale"></param>
        /// <param name="SpriteNo"></param>
        public static void SetBoots(bool isFemale, int SpriteNo = -1)
        {
            if (SpriteNo == -1)
            { //clear the slot
                instance.Boots.Texture = null;
            }
            else
            {
                instance.Boots.Texture = grArmour(isFemale).LoadImageAt(SpriteNo);
            }
        }


        /// <summary>
        /// Sets the sprite for the gloves
        /// </summary>
        /// <param name="isFemale"></param>
        /// <param name="SpriteNo"></param>
        public static void SetGloves(bool isFemale, int SpriteNo = -1)
        {
            if (SpriteNo == -1)
            { //clear the slot
                instance.Gloves.Texture = null;
            }
            else
            {
                instance.Gloves.Texture = grArmour(isFemale).LoadImageAt(SpriteNo);
            }
        }

        /// <summary>
        /// Sets the sprite in the right shoulder
        /// </summary>
        /// <param name="SpriteNo"></param>
        public static void SetRightShoulder(int SpriteNo = -1, int qty = 1)
        {
            if (SpriteNo == -1)
            { //clear the slot
                instance.RightShoulder.Texture = null;
            }
            else
            {
                instance.RightShoulder.Texture = grObjects.LoadImageAt(SpriteNo);
                instance.RightShoulder.Material = grObjects.GetMaterial(SpriteNo);
            }
            var pQty = "";
            if (qty > 1)
            {
                pQty = qty.ToString();
            }
            instance.RightShoulderQTY.Text = pQty;
        }


        /// <summary>
        /// Sets the sprite in the left shoulder
        /// </summary>
        /// <param name="SpriteNo"></param>
        public static void SetLeftShoulder(int SpriteNo = -1, int qty = 1)
        {
            if (SpriteNo == -1)
            { //clear the slot
                instance.LeftShoulder.Texture = null;
            }
            else
            {
                instance.LeftShoulder.Texture = grObjects.LoadImageAt(SpriteNo);
                instance.LeftShoulder.Material = grObjects.GetMaterial(SpriteNo);
            }
            var pQty = "";
            if (qty > 1)
            {
                pQty = qty.ToString();
            }
            instance.LeftShoulderQTY.Text = pQty;
        }


        /// <summary>
        /// Sets the sprite in the right hand
        /// </summary>
        /// <param name="SpriteNo"></param>
        public static void SetRightHand(int SpriteNo = -1, int qty = 1)
        {
            if (SpriteNo == -1)
            { //clear the slot
                instance.RightHand.Texture = null;
            }
            else
            {
                instance.RightHand.Texture = grObjects.LoadImageAt(SpriteNo);
                instance.RightHand.Material = grObjects.GetMaterial(SpriteNo);
            }
            var pQty = "";
            if (qty > 1)
            {
                pQty = qty.ToString();
            }
            instance.RightHandQTY.Text = pQty;
        }


        /// <summary>
        /// Sets the sprite in the left hand
        /// </summary>
        /// <param name="SpriteNo"></param>
        public static void SetLeftHand(int SpriteNo = -1, int qty = 1)
        {
            if (SpriteNo == -1)
            { //clear the slot
                instance.LeftHand.Texture = null;
            }
            else
            {
                instance.LeftHand.Texture = grObjects.LoadImageAt(SpriteNo);
                instance.LeftHand.Material = grObjects.GetMaterial(SpriteNo);
            }
            var pQty = "";
            if (qty > 1)
            {
                pQty = qty.ToString();
            }
            instance.LeftHandQTY.Text = pQty;
        }


        /// <summary>
        /// Sets the sprite in the ring slot
        /// </summary>
        /// <param name="SpriteNo"></param>
        public static void SetRightRing(int SpriteNo = -1)
        {
            if (SpriteNo == -1)
            { //clear the slot
                instance.RightRing.Texture = null;
            }
            else
            {
                instance.RightRing.Texture = grObjects.LoadImageAt(SpriteNo);
                instance.RightRing.Material = grObjects.GetMaterial(SpriteNo);
            }
        }


        /// <summary>
        /// Sets the sprite in the ring slot
        /// </summary>
        /// <param name="SpriteNo"></param>
        public static void SetLeftRing(int SpriteNo = -1)
        {
            if (SpriteNo == -1)
            { //clear the slot
                instance.LeftRing.Texture = null;
            }
            else
            {
                instance.LeftRing.Texture = grObjects.LoadImageAt(SpriteNo);
                instance.LeftRing.Material = grObjects.GetMaterial(SpriteNo);
            }
        }

        /// <summary>
        /// Sets the sprite in the backpack slot
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="SpriteNo"></param>
        public static void SetBackPackArt(int slot, int SpriteNo = -1, int qty = 1)
        {
            if (SpriteNo == -1)
            { //clear the slot
                instance.Backpack[slot].Texture = null;
            }
            else
            {
                instance.Backpack[slot].Texture = grObjects.LoadImageAt(SpriteNo);
                instance.Backpack[slot].Material = grObjects.GetMaterial(SpriteNo);
            }

            //Set the qty label
            var pQty = "";
            if (qty > 1)
            {
                pQty = qty.ToString();
            }
            instance.BackpackQTY[slot].Text = pQty;
        }



        /// <summary>
        /// Sets the open container gui image
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="SpriteNo"></param>
        public static void SetOpenedContainer(int slot, int SpriteNo = -1)
        {
            if (SpriteNo == -1)
            { //clear the slot
                instance.OpenedContainer.Texture = null;
            }
            else
            {
                instance.OpenedContainer.Texture = grObjects.LoadImageAt(SpriteNo);
                instance.OpenedContainer.Material = grObjects.GetMaterial(SpriteNo);
            }
        }




        /// <summary>
        /// Returns the gender specific grArmour data
        /// </summary>
        /// <param name="isFemale"></param>
        /// <returns></returns>
        public static GRLoader grArmour(bool isFemale)
        {
            if (isFemale)
            {
                if (grArmour_F == null)
                {
                    grArmour_F = new GRLoader(GRLoader.ARMOR_F_GR, GRLoader.GRShaderMode.UIShader);
                }
                return grArmour_F;
            }
            else
            {
                if (grArmour_M == null)
                {
                    grArmour_M = new GRLoader(GRLoader.ARMOR_M_GR, GRLoader.GRShaderMode.UIShader);
                }
                return grArmour_M;
            }
        }


        public static void UpdateInventoryDisplay()
        {
            SetHelm(playerdat.isFemale, wearable.GetSpriteIndex(playerdat.HelmObject));
            SetArmour(playerdat.isFemale, wearable.GetSpriteIndex(playerdat.ChestArmourObject));
            SetGloves(playerdat.isFemale, wearable.GetSpriteIndex(playerdat.GlovesObject));
            SetLeggings(playerdat.isFemale, wearable.GetSpriteIndex(playerdat.LeggingsObject));
            SetBoots(playerdat.isFemale, wearable.GetSpriteIndex(playerdat.BootsObject));
            //Set arms and shoulders
            SetRightShoulder(uwObject.GetObjectSprite(playerdat.RightShoulderObject), uwObject.GetObjectQuantity(playerdat.RightShoulderObject));
            SetLeftShoulder(uwObject.GetObjectSprite(playerdat.LeftShoulderObject), uwObject.GetObjectQuantity(playerdat.LeftShoulderObject));
            SetRightHand(uwObject.GetObjectSprite(playerdat.RightHandObject), uwObject.GetObjectQuantity(playerdat.RightHandObject));
            SetLeftHand(uwObject.GetObjectSprite(playerdat.LeftHandObject), uwObject.GetObjectQuantity(playerdat.LeftHandObject));
            //set rings
            SetRightRing(ring.GetSpriteIndex(playerdat.RightRingObject));
            SetLeftRing(ring.GetSpriteIndex(playerdat.LeftRingObject));
            //backback
            for (int i = 0; i < 8; i++)
            {
                if (playerdat.BackPackIndices[i] != -1)
                {
                    var objFound = playerdat.InventoryObjects[playerdat.BackPackIndices[i]];
                    SetBackPackArt(i, uwObject.GetObjectSprite(objFound), uwObject.GetObjectQuantity(objFound));
                }
                else
                {
                    SetBackPackArt(i, -1);
                }
            }
        }


        /// <summary>
		/// Handles click events on the paperdoll
		/// </summary>
		/// <param name="event"></param>
		/// <param name="extra_arg_0"></param>
		private void _paperdoll_gui_input(InputEvent @event, string extra_arg_0)
        {
            // Replace with function body.
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                Debug.Print($"->{extra_arg_0}");
                var obj = GetObjAtSlot(extra_arg_0);

                //Do action appropiate to the interaction mode verb. use 
                if (obj > 0)
                { //there is an object in that slot.
                    InteractWithObjectInSlot(extra_arg_0, obj);
                }
                else
                {
                    InteractWithEmptySlot();
                }
                CurrentSlot = -1;
            }
        }

        private static void InteractWithEmptySlot()
        {
            //there is no object in the slot.
            switch (InteractionMode)
            {
                case InteractionModes.ModePickup:
                    {
                        PickupToEmptySlot();
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles clicking on an empty inventory slot with an object in hand
        /// </summary>
        private static void PickupToEmptySlot()
        {
            if (playerdat.ObjectInHand != -1)
            {
                switch (CurrentSlot)
                {
                    case -1:
                        {
                            MoveObjectInHandOutOfOpenedContainer();
                            break;
                        }
                    case >= 0 and <= 10:
                        {//paperdoll
                            var index = playerdat.AddObjectToPlayerInventory(playerdat.ObjectInHand,false);
                            playerdat.SetInventorySlotListHead(CurrentSlot, index);
                            //redraw                            
                            RefreshSlot(CurrentSlot);
                            playerdat.ObjectInHand = -1;
                            instance.mousecursor.ResetCursor();
                            break;
                        }
                    case > 10 and <= 18:
                        //backpack
                        if (playerdat.OpenedContainer < 0)
                        {//backpackslot
                            var index = playerdat.AddObjectToPlayerInventory(playerdat.ObjectInHand,false);
                            playerdat.SetInventorySlotListHead(CurrentSlot, index);

                            //redraw
                            playerdat.BackPackIndices[CurrentSlot - 11] = index;
                            RefreshSlot(CurrentSlot);
                            playerdat.ObjectInHand = -1;
                            instance.mousecursor.ResetCursor();
                        }
                        else
                        {
                            //in a container 
                            var index = playerdat.AddObjectToPlayerInventory(playerdat.ObjectInHand,false);
                            var newobj = playerdat.InventoryObjects[index];
                            //add to linked list, for the moment it will be at the head but later on should be at the end (next of the object to it's left)
                            var opened = playerdat.InventoryObjects[playerdat.OpenedContainer];
                            newobj.next = opened.link;
                            opened.link = index;

                            //redraw
                            playerdat.BackPackIndices[CurrentSlot - 11] = index;
                            RefreshSlot(CurrentSlot);
                            playerdat.ObjectInHand = -1;
                            instance.mousecursor.ResetCursor();

                        }
                        break;
                }

            }
        }

        private static void MoveObjectInHandOutOfOpenedContainer()
        {
            //the opened container
            if (playerdat.OpenedContainerParent == -1)
            {
                //on paperdoll. try and add to there
                var freeslot = playerdat.FreePaperDollSlot;
                if (freeslot != -1)
                {
                    var index = playerdat.AddObjectToPlayerInventory(playerdat.ObjectInHand,false);
                    playerdat.SetInventorySlotListHead(freeslot, index);
                    playerdat.ObjectInHand = -1;
                    instance.mousecursor.ResetCursor();
                }
                else
                {
                    Debug.Print("No room on paperdoll");
                }
            }
            else
            {
                //the container that contains the opened container.
                var index = playerdat.AddObjectToPlayerInventory(playerdat.ObjectInHand,false);
                var newobj = playerdat.InventoryObjects[index];
                var container = playerdat.InventoryObjects[playerdat.OpenedContainerParent];
                newobj.next = container.link;
                container.link = index;
                playerdat.ObjectInHand = -1;
                instance.mousecursor.ResetCursor();
            }
        }


        private static void InteractWithObjectInSlot(string extra_arg_0, int obj)
        {
            switch (InteractionMode)
            {
                case InteractionModes.ModeUse:
                    if (extra_arg_0 != "OpenedContainer")
                    {
                        use.Use(
                            index: obj,
                            objList: playerdat.InventoryObjects,
                            WorldObject: false);
                    }
                    else
                    {
                        //close up opened container.
                        container.Close(
                            index: obj,
                            objList: playerdat.InventoryObjects);
                    }
                    break;
                case InteractionModes.ModeLook:
                    look.LookAt(obj, playerdat.InventoryObjects, false); break;
                case InteractionModes.ModePickup:
                    if (extra_arg_0 == "OpenedContainer")
                    {
                        if (playerdat.ObjectInHand != -1)
                        {
                            MoveObjectInHandOutOfOpenedContainer();
                        }
                        else
                        {
                            //close up opened container.
                            container.Close(
                                index: obj,
                                objList: playerdat.InventoryObjects);
                        }
                    }
                    else
                    {
                        if (playerdat.ObjectInHand != -1)
                        {
                            //do a use interaction on the object already there. 
                            playerdat.UseObjectsTogether(playerdat.ObjectInHand, obj);
                            // use.Use(
                            //     index: obj,
                            //     objList: playerdat.InventoryObjects,
                            //     WorldObject: false);
                        }
                        else
                        {
                            //try and pickup
                            var newIndex = playerdat.AddInventoryObjectToWorld(obj, true, false);
                            var pickObject = UWTileMap.current_tilemap.LevelObjects[newIndex];
                            playerdat.ObjectInHand = newIndex;
                            uimanager.instance.mousecursor.SetCursorArt(pickObject.item_id);
                        }

                    }
                    break;
                default:
                    Debug.Print("Unimplemented inventory use verb-object combination"); break;
            }
        }


        private static int GetObjAtSlot(string extra_arg_0)
        {
            var obj = -1;
            switch (extra_arg_0)
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

        /// <summary>
        /// Moves the backpack paper doll display up or down
        /// </summary>
        /// <param name="event"></param>
        /// <param name="extra_arg_0">+1 move up, -1 move down</param>
        private void _on_updown_arrow_gui_input(InputEvent @event, long extra_arg_0)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                switch (extra_arg_0)
                {
                    case -1://move down
                        BackPackStart += 4;
                        container.DisplayContainerObjects(
                            obj: playerdat.InventoryObjects[playerdat.OpenedContainer],
                            start: BackPackStart);
                        break;
                    case 1: //move up
                        BackPackStart -= 4;
                        if (BackPackStart < 0) { BackPackStart = 0; }
                        container.DisplayContainerObjects(
                            obj: playerdat.InventoryObjects[playerdat.OpenedContainer],
                            start: BackPackStart);
                        break;
                }
            }
        }

        private void _on_stats_updown_gui_input(InputEvent @event, long extra_arg_0)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                switch (extra_arg_0)
                {
                    case -1:
                        StatsOffset = Math.Max(0, StatsOffset - 1);
                        PrintStatsDisplay();
                        break;
                    case 1:
                        StatsOffset = Math.Min(15, StatsOffset + 1);
                        PrintStatsDisplay();
                        break;
                }
            }
        }

    }//end class
} //end namespace