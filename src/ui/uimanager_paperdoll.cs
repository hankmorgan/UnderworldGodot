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
        [Export] public TextureRect Chains;
        [Export] public TextureRect BackpackBG;

        public static int CurrentSlot;
        public static int BackPackStart = 0;

        public static int DominantHandSlot
        {
            get
            {
                if (playerdat.isLefty)
                    {
                        return 8;
                    }
                else
                    {
                        return 7;
                    }
            }
        }

        public void InitPaperdoll()
        {
            EnableDisable(PanelInventory,true);
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

                offset = new Vector2(10, -12);
                ArrowUp.Position += offset;
                ArrowDown.Position +=offset;
                BackpackBG.Position = new Vector2(-148,314);
                BackpackBG.Size = new Vector2(300,164);                
            }

            ArrowUp.Texture = grButtons.LoadImageAt(27);
            ArrowDown.Texture = grButtons.LoadImageAt(28);
            EnableDisable(ArrowUp, false);
            EnableDisable(ArrowDown, false);
            EnableDisable(Chains, true);
            EnableDisable(SelectedRunes[0],true);
            EnableDisable(SelectedRunes[1],true);
            EnableDisable(SelectedRunes[2],true);
            EnableDisable(BackpackBG,false);
            BackpackBG.Texture = grInv.LoadImageAt(6);
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
                        if (BackPackIndices[slotno - 11] != -1)
                        {
                            objAtSlot = playerdat.InventoryObjects[BackPackIndices[slotno - 11]];
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
                EnableDisable(instance.OpenedContainer, false);
                instance.OpenedContainer.Texture = null;      
                EnableDisable(instance.BackpackBG,false);          
            }
            else
            {
                EnableDisable(instance.OpenedContainer, true);
                instance.OpenedContainer.Texture = grObjects.LoadImageAt(SpriteNo);
                instance.OpenedContainer.Material = grObjects.GetMaterial(SpriteNo);
                EnableDisable(instance.BackpackBG,true);
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
                if (BackPackIndices[i] != -1)
                {
                    var objFound = playerdat.InventoryObjects[BackPackIndices[i]];
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
            if (@event is InputEventMouseButton eventMouseButton 
                && eventMouseButton.Pressed 
                &&  (((eventMouseButton.ButtonIndex == MouseButton.Left) || (eventMouseButton.ButtonIndex == MouseButton.Right)))
            )
            {
                if (playerdat.ParalyseTimer>0)
                {
                    return; //block input while paralysed
                }
                if (playerdat.DreamingInVoid)
                {
                    return;// to prevent inventory use while in the void.
                }
                bool isLeftClick = (eventMouseButton.ButtonIndex == MouseButton.Left);
                if (MessageScrollIsTemporary)
                {//avoids bug involved in clicking on an object while a temp message is displayed
                    return;
                }
                //LEFT CLICK ACTIONS
                Debug.Print($"->{extra_arg_0}");
                var objIndexAtSlot = GetObjAtSlot(extra_arg_0);

                //Do action appropiate to the interaction mode verb. use 
                if (objIndexAtSlot > 0)
                { //there is an object in that slot.
                    uwObject obj = playerdat.InventoryObjects[objIndexAtSlot];
                    switch(UsageMode)
                    {
                        case 0: //default
                            InteractWithObjectInSlot(
                                slotname: extra_arg_0,
                                objAtSlot: obj,
                                isLeftClick: isLeftClick);
                            break;
                        case 1://object select to use on another. eg doorkey
                            useon.UseOn(
                                ObjectUsed: playerdat.InventoryObjects[objIndexAtSlot], 
                                srcObject: useon.CurrentItemBeingUsed, 
                                WorldObject: false);
                            break;
                        case 2://spell casting
                            if (SpellCasting.currentSpell.SpellMajorClass != 5)
                                {//as long as it's not a project(?) try and cast on the object
                                    SpellCasting.CastCurrentSpellOnRayCastTarget(
                                        index: objIndexAtSlot, 
                                        objList: playerdat.InventoryObjects, 
                                        hitCoordinate: Vector3.Zero,
                                        WorldObject:false);
                                }
                            break;
                    }
                }
                else
                {
                    switch(UsageMode)
                    {
                        case 0:
                            InteractWithEmptySlot(); break;
                    }
                 
                }
                CurrentSlot = -1;
            }
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
                    case 1://move down
                        BackPackStart += 4;
                        container.DisplayContainerObjects(
                            obj: playerdat.InventoryObjects[OpenedContainerIndex],
                            start: BackPackStart);
                        break;
                    case -1: //move up
                        BackPackStart -= 4;
                        if (BackPackStart < 0) { BackPackStart = 0; }
                        container.DisplayContainerObjects(
                            obj: playerdat.InventoryObjects[OpenedContainerIndex],
                            start: BackPackStart);
                        break;
                }
            }
        }


        /// <summary>
        /// The object containing the opened container. Returns -1 parent is at top level. Assumes container is opened.
        /// </summary>
        public static int OpenedContainerParent
        {
            get
            {
                if (OpenedContainerIndex == -1)
                {
                    return -1;
                }
                for (int i = 0; i < 19; i++)
                {
                    if (playerdat.GetInventorySlotListHead(i) == OpenedContainerIndex)
                    {
                        return -1;
                    }
                }
                //try and find in the rest of the inventory
                foreach (var objToCheck in playerdat.InventoryObjects)
                {//if this far down then I need to find the container that the closing container sits in
                    if (objToCheck != null)
                    {
                        var result = objectsearch.GetContainingObject(
                            ListHead: objToCheck.index,
                            ToFind: OpenedContainerIndex,
                            objList: playerdat.InventoryObjects);
                        if (result != -1)
                        {//container found. Browse into it by using it
                            return result;
                        }
                    }
                }
                return -1; //not found. assume paperdoll
            }
        }


        /// <summary>
        /// Gets a free paperdoll slot
        /// </summary>
        public static int FreePaperDollSlot
        {
            get
            {
                for (int i = 11; i < 19; i++)
                {
                    if (playerdat.GetInventorySlotListHead(i) == 0)
                    {
                        return i;
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// Finds a slot on the inventory (top level) that has no objects in it. Excludes paperdoll
        /// </summary>
        /// <returns></returns>
        public static int FreeGeneralUseSlot
        {
            get
            {
                for (int i = 11; i <= 18; i++)
                {//try backpack first
                    if (playerdat.GetInventorySlotListHead(i) == 0)
                    {
                        return i;
                    }
                }

                for (int i = 5; i <=8; i++)
                {//then arms and shoulders
                    if (playerdat.GetInventorySlotListHead(i) == 0)
                    {
                        return i;
                    }
                }
                return -1; //nothing found
            }
        }

    }//end class
} //end namespace