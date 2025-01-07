using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void transform_talker(uwObject talker)
        {
            Debug.Print("transform talker");
            var ArgA = GetConvoStackValueAtPtr(stackptr - 1);
            var newIsPowerful = GetConvoStackValueAtPtr(stackptr - 2);
            var newWhoAMI = GetConvoStackValueAtPtr(stackptr - 3);
            var newItemID = GetConvoStackValueAtPtr(stackptr - 4);

            var tile = UWTileMap.current_tilemap.Tiles[talker.npc_xhome, talker.npc_yhome];

            if (newItemID != -1)
            {
                newItemID = (newItemID & 0x30) + (newItemID & 0xF) + 0x40;
                talker.item_id = newItemID;
            }

            if (newWhoAMI != -1)
            {
                talker.npc_whoami = (short)newWhoAMI;
            }

            if (newIsPowerful != -1)
            {
                talker.IsPowerfull = (short)newIsPowerful;
            }

            if (ArgA != -1)
            {
                talker.UnkBit_0XA_Bit456 = (short)ArgA;
            }

            if (!critterObjectDat.isFlier(talker.item_id))
            {
                talker.zpos = (short)(tile.floorHeight<<3);
            }
            objectInstance.Reposition(talker);
        }


    }//end class
}//end namespace