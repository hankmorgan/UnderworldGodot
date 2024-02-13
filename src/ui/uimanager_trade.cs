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
        public static int GetPlayerTradeSlot(int slotno, bool ignoreSelected = true)
        {
            if ((PlayerItemSelected[slotno]) | (!ignoreSelected))
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
        /// <param name="ignoreSelected"></param>
        /// <returns></returns>
        public static int GetNPCTradeSlot(int slotno, bool ignoreSelected = true)
        {
            if ((NPCItemSelected[slotno]) | (!ignoreSelected))
            {
                return NPCItemIDs[slotno];
            }
            return -1;
        }
        private void _on_player_trade_selected(InputEvent @event, long extra_arg_0)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {                
                //toggle selected
                if (PlayerItemSelected[extra_arg_0])
                {
                    PlayerTradeOff(extra_arg_0);
                }
                else
                {
                    if (PlayerItemIDs[extra_arg_0]!=-1)
                    {//check if occupied.
                        PlayerTradeOn(extra_arg_0);
                    }
                    else
                    {
                        PlayerTradeOff(extra_arg_0);
                    }
                }
            }
        }

        private static void PlayerTradeOn(long slotNo)
        {
            playerTradeSelected[slotNo].Texture = SelectionCross;
            PlayerItemSelected[slotNo] = true;
        }


        private static void PlayerTradeOff(long slotNo)
        {
            playerTradeSelected[slotNo].Texture = null;
            PlayerItemSelected[slotNo] = false;
        }


        private void _on_npc_trade_selected(InputEvent @event, long extra_arg_0)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {                
                //toggle selected
                if (NPCItemSelected[extra_arg_0])
                {
                    NpcTradeOff(extra_arg_0);
                }
                else
                {
                    if (NPCItemIDs[extra_arg_0]!=-1)
                    {//check if occupied.
                        NPCTradeOn(extra_arg_0);
                    }
                    else
                    {
                        NpcTradeOff(extra_arg_0);
                    }
                }
            }
        }

        private static void NpcTradeOff(long slotNo)
        {
            npcTradeSelected[slotNo].Texture = null;
            NPCItemSelected[slotNo] = false;
        }


        private static void NPCTradeOn(long slotNo)
        {
            npcTradeSelected[slotNo].Texture = SelectionCross;
            NPCItemSelected[slotNo] = true;
        }

        private void _on_player_trade_input(InputEvent @event, long extra_arg_0)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
            {  
                if (eventMouseButton.ButtonIndex == MouseButton.Left)
                {//left click item manipulation
                    if (PlayerItemIDs[extra_arg_0]== -1)
                    {//no item currently in slot.
                        if (playerdat.ObjectInHand!=-1)
                        {//drop available item in slot
                            SetPlayerTradeSlot((int)extra_arg_0,playerdat.ObjectInHand,true);
                            playerdat.ObjectInHand = -1;
                            mousecursor.ResetCursor();
                        }
                    }
                    else
                    {
                        if (playerdat.ObjectInHand == -1)
                        {//take item from slot into a free hand
                            playerdat.ObjectInHand = PlayerItemIDs[extra_arg_0];
                            var obj = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];
                            mousecursor.SetCursorArt(obj.item_id);
                            SetPlayerTradeSlot((int)extra_arg_0, -1, false);
                        }
                        else
                        {
                            //swap objects
                            var swap = playerdat.ObjectInHand;
                            playerdat.ObjectInHand = PlayerItemIDs[extra_arg_0];
                            var obj = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];
                            mousecursor.SetCursorArt(obj.item_id);
                            SetPlayerTradeSlot((int)extra_arg_0, swap, true);
                        }
                    }
                }
                else
                {//right click look at
                    if (PlayerItemIDs[extra_arg_0]!=-1)
                    {
                        var obj = UWTileMap.current_tilemap.LevelObjects[PlayerItemIDs[extra_arg_0]];
                        if (obj!=null)
                        {
                            look.GeneralLookDescription(obj: obj, OutputConvoScroll: true);
                        }  
                    }
                }                
            }
        }


        private void _on_npc_trade_input(InputEvent @event, long extra_arg_0)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {  
                //AddToMessageScroll($"NPC {extra_arg_0}");
                if (NPCItemIDs[extra_arg_0]!=-1)
                {
                    var obj = UWTileMap.current_tilemap.LevelObjects[NPCItemIDs[extra_arg_0]];
                    if (obj!=null)
                    {
                        look.GeneralLookDescription(obj: obj, OutputConvoScroll: true);
                    }                    
                }
            }
        }
    }//end class
}//end namespace