using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        /// <summary>
        /// Selects what items the NPC offers in trade.
        /// </summary>
        public static void setup_to_barter(uwObject talker)
        {
            var skippedprimaryweapon = false;
            if (talker.LootSpawnedFlag == 0)
                {
                    npc.spawnloot(talker);
                }
            var nextObj = talker.link;
            var objectcount = 0;
            var slotindex = 0;
            while (nextObj!=0)
            {
                bool skip = false;
                objectcount++;
                if (objectcount>=40)
                {
                    break;
                }
                var obj = TileMap.current_tilemap.LevelObjects[nextObj];
                if (!skippedprimaryweapon)
                    {
                        if (obj.OneF0Class==0)
                        {   //item is a weapon
                            skippedprimaryweapon = true;
                            skip=true;
                        }
                    }
                if (commonObjDat.monetaryvalue(obj.item_id)==0)
                    {
                        skip = true;
                    }
                if (Rng.r.Next(0,7)<5)
                {
                    skip=true;
                }

                if (!skip)
                {
                    Debug.Print($"Slot{slotindex} {obj._name}");
                    slotindex++;
                }
                nextObj = obj.next;
            }
        }

    }//end class
}//end class