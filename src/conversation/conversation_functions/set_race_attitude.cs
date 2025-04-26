using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        /// <summary>
        /// Selects what items the NPC offers in trade.
        /// </summary>
        public static void set_race_attitude(uwObject talker)
        {
            var range = at(at(stackptr-1));
            if (range<=0)
            {
                range = 1;
            }
            var newAttitute = at(at(stackptr-2));
            var race = at(at(stackptr-3));
            Debug.Print($"set_race_attitude to {newAttitute} in range {range} for race {race}");
            for (var tileX = talker.tileX - range; tileX <= talker.tileX + range; tileX++)
            {
                for (var tileY = talker.tileY - range; tileY <= talker.tileY + range; tileY++)
                {
                    if (UWTileMap.ValidTile(tileX,tileY))
                    {
                        var next = UWTileMap.current_tilemap.Tiles[tileX,tileY].indexObjectList;
                        while (next!=0)
                        {
                            var obj = UWTileMap.current_tilemap.LevelObjects[next];
                            if(obj.majorclass == 1)
                            {//npc
                                if (obj.item_id == talker.item_id)
                                {
                                    if (obj.UnkBit_0XA_Bit7 == 0 ) //  .MobileUnk_0xA & 0x80) >> 7) == 0)
                                    {
                                        if (critterObjectDat.faction(obj.item_id) == race)
                                        {
                                            obj.npc_attitude = newAttitute;
                                        }
                                    }
                                }
                            }

                            next = obj.next;                            
                        }
                    }
                }
            }
        }
    }//end class
}//end namespace