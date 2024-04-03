using System;
using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void find_inv(uwObject talker)
        {
            var Character_arg0 = at(at(stackptr - 1));
            var ItemID_arg1 = at(at(stackptr - 2));
            Debug.Print($"Find_Inv({Character_arg0},{ItemID_arg1});");

            //arg0 when 0 search npc, else search player
            //arg1 == itemid to find.
            //returns item index;
            int majorclass; int minorclass; int classindex;
            if (ItemID_arg1 >= 1000)
            {
                var tmp = ItemID_arg1 - 1000;
                majorclass = tmp >> 2;
                minorclass = tmp & 0x3;
                classindex = -1;
            }
            else
            {
                majorclass = ItemID_arg1 >> 6;
                minorclass = (ItemID_arg1 & 0x30) >> 4;
                classindex = ItemID_arg1 & 0xF;
            }

            if (Character_arg0 == 0)
            {
                //search npc.
                if (talker.LootSpawnedFlag == 0)
                {
                    npc.spawnloot(talker);
                }
                var result = objectsearch.FindMatchInObjectChain(
                    ListHeadIndex: talker.link,
                    majorclass: majorclass,
                    minorclass: minorclass,
                    classindex: classindex,
                    objList: UWTileMap.current_tilemap.LevelObjects,
                    SkipNext: true);
                if (result != null)
                {
                    result_register = result.index;
                    return;
                }
            }
            else
            {
                //search player
                var result = objectsearch.FindMatchInObjectList(
                    majorclass: majorclass,
                    minorclass: minorclass,
                    classindex: classindex,
                    objList: playerdat.InventoryObjects);
                if (result != null)
                {
                    result_register = result.index;
                    return;
                }
            }
            result_register = 0;//not found
        }
    }//end class
}//end namespace