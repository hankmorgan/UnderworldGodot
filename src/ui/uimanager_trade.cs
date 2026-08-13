using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Trade")]
        [Export]
        public Texture2D cross;
        [Export]
        public Texture2D cross2;

        static Texture2D SelectionCross
        {
            get
            {
                if (UWClass._RES==UWClass.GAME_UW2)
                {
                    return instance.cross2;
                }
                return instance.cross;
            }
        }
        //uw1      
        [Export]
        public TextureRect[] PlayerTradeSlotUW1 = new TextureRect[4];
        [Export]
        public TextureRect[] NPCTradeSlotUW1 = new TextureRect[4];
        [Export]
        public TextureRect[] PlayerTradeSelectedUW1 = new TextureRect[4];
        [Export]
        public TextureRect[] NPCTradeSelectedUW1 = new TextureRect[4];

        //uw2
        [Export]
        public TextureRect[] PlayerTradeSlotUW2 = new TextureRect[6];
        [Export]
        public TextureRect[] NPCTradeSlotUW2 = new TextureRect[6];
        [Export]
        public TextureRect[] PlayerTradeSelectedUW2 = new TextureRect[6];
        [Export]
        public TextureRect[] NPCTradeSelectedUW2 = new TextureRect[6];


        static int[] PlayerItemIDs = new int[6];
        static bool[] PlayerItemSelected = new bool[6];
        static int[] NPCItemIDs = new int[6];
        static bool[] NPCItemSelected = new bool[6];


        public static int NoOfTradeSlots
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return 6;
                }
                else
                {
                    return 4;
                }
            }
        }

        public static TextureRect[] playerTrade
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return instance.PlayerTradeSlotUW2;
                }
                else
                {
                    return instance.PlayerTradeSlotUW1;
                }
            }
        }

        public static TextureRect[] playerTradeSelected
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return instance.PlayerTradeSelectedUW2;
                }
                else
                {
                    return instance.PlayerTradeSelectedUW1;
                }
            }
        }

        public static TextureRect[] NPCTrade
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return instance.NPCTradeSlotUW2;
                }
                else
                {
                    return instance.NPCTradeSlotUW1;
                }
            }
        }

        public static TextureRect[] npcTradeSelected
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return instance.NPCTradeSelectedUW2;
                }
                else
                {
                    return instance.NPCTradeSelectedUW1;
                }
            }
        }

        /// <summary>
        /// Sets the art, item index, for the specified slot. 
        /// </summary>
        /// <param name="slotno"></param>
        /// <param name="itemid"></param>
        public static void SetPlayerTradeSlot(int slotno, int item_index = -1, bool selected = true)
        {
            if (item_index == -1)
            {
                //clear
                playerTrade[slotno].Texture = null;
                PlayerItemIDs[slotno] = -1;
                PlayerItemSelected[slotno] = false; //force selected off
                PlayerTradeOff(slotno);
            }
            else
            {
                var obj = UWTileMap.current_tilemap.LevelObjects[item_index];
                PlayerItemIDs[slotno] = obj.index;
                playerTrade[slotno].Texture = grObjects.LoadImageAt(obj.item_id);
                playerTrade[slotno].Material = grObjects.GetMaterial(obj.item_id);
                PlayerItemSelected[slotno] = selected;
                if (selected)
                {
                    PlayerTradeOn(slotno);
                }      
                else
                {
                    PlayerTradeOff(slotno);
                }          
            }
        }

        /// <summary>
        /// Gets the item id if it is selected at that slot
        /// </summary>
        /// <param name="slotno"></param>
        /// <returns></returns>
        public static int GetPlayerTradeSlot(int slotno, bool OnlySelected = true)
        {
            if ((PlayerItemSelected[slotno]) | (!OnlySelected))
            {
                return PlayerItemIDs[slotno];
            }
            return -1;
        }

        /// <summary>
        /// Sets the art, item index, for the specified slot. 
        /// </summary>
        /// <param name="slotno"></param>
        /// <param name="itemid"></param>
        public static void SetNPCTradeSlot(int slotno, int item_index = -1, bool selected = false)
        {
            if (item_index == -1)
            {
                //clear
                NPCTrade[slotno].Texture = null;
                NPCItemIDs[slotno] = -1;
                NPCItemSelected[slotno] = false; //force selected off
                NpcTradeOff(slotno);
            }
            else
            {
                var obj = UWTileMap.current_tilemap.LevelObjects[item_index];
                NPCItemIDs[slotno] = obj.index;
                NPCTrade[slotno].Texture = grObjects.LoadImageAt(obj.item_id);
                NPCTrade[slotno].Material = grObjects.GetMaterial(obj.item_id);
                NPCItemSelected[slotno]  = selected;                
                if (selected)
                {
                    NPCTradeOn(slotno);
                }
                else
                {
                    NpcTradeOff(slotno);
                }
            }
        }

        /// <summary>
        /// Gets the item if selected at the npc trade slot
        /// </summary>
        /// <param name="slotno"></param>
        /// <param name="OnlySelected"></param>
        /// <returns></returns>
        public static int GetNPCTradeSlot(int slotno, bool OnlySelected = true)
        {
            if ((NPCItemSelected[slotno]) | (!OnlySelected))
            {
                return NPCItemIDs[slotno];
            }
            return -1;
        }
        private void _on_player_trade_selected(InputEvent @event, long extra_arg_0)
        {
            HandleTradeSlotClick(@event, extra_arg_0, isPlayerSide: true);
        }

        private void _on_npc_trade_selected(InputEvent @event, long extra_arg_0)
        {
            HandleTradeSlotClick(@event, extra_arg_0, isPlayerSide: false);
        }

        private void _on_player_trade_input(InputEvent @event, long extra_arg_0)
        {
            HandleTradeSlotClick(@event, extra_arg_0, isPlayerSide: true);
        }

        private void _on_npc_trade_input(InputEvent @event, long extra_arg_0)
        {
            HandleTradeSlotClick(@event, extra_arg_0, isPlayerSide: false);
        }

        /// <summary>
        /// Trade slots: left release = select (or place held item on player side),
        /// right release = look.
        /// </summary>
        void HandleTradeSlotClick(InputEvent @event, long slotNo, bool isPlayerSide)
        {
            if (@event is not InputEventMouseButton eventMouseButton
                || eventMouseButton.Pressed
                || ((eventMouseButton.ButtonIndex != MouseButton.Left)
                    && (eventMouseButton.ButtonIndex != MouseButton.Right)))
            {
                return;
            }

            int[] itemIds = isPlayerSide ? PlayerItemIDs : NPCItemIDs;
            if (eventMouseButton.ButtonIndex == MouseButton.Right)
            {
                LookAtTradeSlot(itemIds, slotNo, useLoreCheck: isPlayerSide);
                return;
            }

            // Left release
            if (isPlayerSide && playerdat.ObjectInHand != -1)
            {
                // Place / swap held item into the player trade slot.
                if (itemIds[slotNo] == -1)
                {
                    SetPlayerTradeSlot((int)slotNo, playerdat.ObjectInHand, true);
                    playerdat.ObjectInHand = -1;
                    mousecursor.SetCursorToCursor();
                }
                else
                {
                    var swap = playerdat.ObjectInHand;
                    playerdat.ObjectInHand = itemIds[slotNo];
                    var obj = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];
                    mousecursor.SetCursorToObject(obj.item_id);
                    SetPlayerTradeSlot((int)slotNo, swap, true);
                }
                return;
            }

            ToggleTradeSlotSelection(slotNo, isPlayerSide);
        }

        static void LookAtTradeSlot(int[] itemIds, long slotNo, bool useLoreCheck)
        {
            if (itemIds[slotNo] == -1)
            {
                return;
            }
            var obj = UWTileMap.current_tilemap.LevelObjects[itemIds[slotNo]];
            if (obj == null)
            {
                return;
            }
            look.PrintLookDescription(
                obj: obj,
                objList: UWTileMap.current_tilemap.LevelObjects,
                OutputConvo: true,
                lorecheckresult: useLoreCheck ? look.LoreCheck(obj) : 0);
        }

        void ToggleTradeSlotSelection(long slotNo, bool isPlayerSide)
        {
            if (isPlayerSide)
            {
                if (PlayerItemIDs[slotNo] == -1)
                {
                    PlayerTradeOff(slotNo);
                    return;
                }
                if (PlayerItemSelected[slotNo])
                    PlayerTradeOff(slotNo);
                else
                    PlayerTradeOn(slotNo);
            }
            else
            {
                if (NPCItemIDs[slotNo] == -1)
                {
                    NpcTradeOff(slotNo);
                    return;
                }
                if (NPCItemSelected[slotNo])
                    NpcTradeOff(slotNo);
                else
                    NPCTradeOn(slotNo);
            }
        }

        public static void PlayerTradeOn(long slotNo)
        {
            playerTradeSelected[slotNo].Texture = SelectionCross;
            PlayerItemSelected[slotNo] = true;
        }


        public static void PlayerTradeOff(long slotNo)
        {
            playerTradeSelected[slotNo].Texture = null;
            PlayerItemSelected[slotNo] = false;
        }

        public static void NpcTradeOff(long slotNo)
        {
            npcTradeSelected[slotNo].Texture = null;
            NPCItemSelected[slotNo] = false;
        }


        public static void NPCTradeOn(long slotNo)
        {
            npcTradeSelected[slotNo].Texture = SelectionCross;
            NPCItemSelected[slotNo] = true;
        }
    }//end class
}//end namespace