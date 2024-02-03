using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Trade")]
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
            }
            else
            {
                var obj = TileMap.current_tilemap.LevelObjects[item_index];
                playerTrade[slotno].Texture = grObjects.LoadImageAt(obj.item_id);
                playerTrade[slotno].Material = grObjects.GetMaterial(obj.item_id);
            }
        }

        /// <summary>
        /// Sets the art, item index, for the specified slot. 
        /// </summary>
        /// <param name="slotno"></param>
        /// <param name="itemid"></param>
        public static void SetNPCTradeSlot(int slotno, int item_index = -1, bool selected = true)
        {
            if (item_index == -1)
            {
                //clear
                NPCTrade[slotno].Texture = null;
                NPCItemIDs[slotno] = -1;
                NPCItemSelected[slotno] = false; //force selected off
            }
            else
            {
                var obj = TileMap.current_tilemap.LevelObjects[item_index];
                NPCTrade[slotno].Texture = grObjects.LoadImageAt(obj.item_id);
                NPCTrade[slotno].Material = grObjects.GetMaterial(obj.item_id);
            }
        }
    }
}