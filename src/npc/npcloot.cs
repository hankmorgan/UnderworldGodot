using System;
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Class for managing the spawning of npc loot objects
    /// </summary>
    public partial class npc : objectInstance
    {
        static bool DebugSpawnAllLoot=true;

        /// <summary>
        /// Generates the npc's inventory on demand. If lootspawned flag is set the npc already has an inventory
        /// </summary>
        /// <param name="critter"></param>
        public static void spawnloot(uwObject critter)
        {
            if (critter.LootSpawnedFlag==0)
            {
                ValuableObjectLoot(critter);
                FoodLoot(critter);
                WeaponLoot(critter);
                OtherLoot(critter);
            }
            critter.LootSpawnedFlag = 1;
        }

        /// <summary>
        /// Drops coins and gems into then npc inventory.
        /// </summary>
        /// <param name="critter"></param>
        public static void ValuableObjectLoot(uwObject critter)
        {
            //The game world chooses which valuable item type is dropped in the valuables object class (starting at coin)
            var world = worlds.GetWorldNo(playerdat.dungeon_level);
            
            var Critter_var_5 = critterObjectDat.Unk26_F(critter.item_id);
            var Critter_var_6 = critterObjectDat.Unk26_R4(critter.item_id);
            
            var r = new Random();
            if ((r.Next(0, 16) > Critter_var_6) && (!DebugSpawnAllLoot))
            {//Do not spawn a valuable.
                return;
            }
            else
            { //spawn valuable
                int offset=0; if (_RES!=GAME_UW2){offset=3;}
                var valuable_type_var_1 = r.Next(0, 37 + offset - world * 3) - (30 + offset - world * 3);
                if (valuable_type_var_1 < 0) { valuable_type_var_1 = 0; }
                if ((valuable_type_var_1 == 1) && (_RES==GAME_UW2)) { valuable_type_var_1 = 0; }//make sure no storage crystal is dropped in UW2

                //now use var_1 to look up the objects monetary value within objects.dat (starting at coin)
                var coinitemid = 0xA0;
                var base_objectvalue_var_2 = commonObjDat.monetaryvalue(coinitemid + valuable_type_var_1);
                
                if (base_objectvalue_var_2 == 0) { base_objectvalue_var_2 = 1; }

                //Adjust the base value based on ranges, presumably to create a probability
                if (base_objectvalue_var_2 >= 12)
                {
                    base_objectvalue_var_2 = 0xBC + (base_objectvalue_var_2 << 3);
                }
                else
                {
                    if ((base_objectvalue_var_2 >= 8) && base_objectvalue_var_2 < 12)
                    {
                        base_objectvalue_var_2 = 0xEC + base_objectvalue_var_2 << 2;
                    }
                    else
                    {
                        if ((base_objectvalue_var_2 >= 4) && (base_objectvalue_var_2 < 8))
                        {
                            base_objectvalue_var_2 = 0xFC + (valuable_type_var_1 << 1);
                        }
                    }
                }

                var var_3_qty = 0;
                
                if (Critter_var_5 < base_objectvalue_var_2)
                {
                    //do a rng roll to check if a single instance of the valuable is spawned.
                    if ((Critter_var_5 << 2) < r.Next(0, base_objectvalue_var_2))
                    {
                        var_3_qty = 1;
                    }
                }
                else
                {
                    // spawn a qty of the object based on a 4Dx dice roll.
                    var qty_range_var_4 = ((Critter_var_5 << 2) / base_objectvalue_var_2) << 1;

                    //do 4D dice roll.

                    qty_range_var_4 = Rng.DiceRoll(4, qty_range_var_4);

                    var_3_qty = qty_range_var_4 >> 2;
                }

                if (var_3_qty > 0)
                {
                    //Create obj
                    uwObject obj = spawnLootObject(critter, valuable_type_var_1 + coinitemid);
                    //Apply the new qty of valuables to the result
                    obj.link = (short)var_3_qty;
                    Debug.Print($"Spawning {var_3_qty} {GameStrings.GetObjectNounUW(coinitemid + valuable_type_var_1)}");
                }
            }
        }

        /// <summary>
        /// Spawns the final loot object on the critters inventory
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="itemid"></param>
        /// <returns></returns>
        private static uwObject spawnLootObject(uwObject critter, int itemid)
        {
            var slot = ObjectCreator.PrepareNewObject(itemid);
            //add to critter object list
            var obj = TileMap.current_tilemap.LevelObjects[slot];
            //Insert at the head of the critters inventory.
            obj.next = critter.link;
            critter.link = (short)slot;
            return obj;
        }

        public static void FoodLoot(uwObject critter)
        {
            if (Rng.r.Next(0,16)<critterObjectDat.foodloot_probability(critter.item_id) || (DebugSpawnAllLoot))
            {
                var item_id = critterObjectDat.foodloot_item(critter.item_id) + 0xB0;
                var obj = spawnLootObject(critter, item_id);
                Debug.Print($"Spawning {GameStrings.GetObjectNounUW(item_id)}");
            }
        }

        /// <summary>
        /// Drops loot that has a quality and qty to it. Probably weapons.
        /// </summary>
        /// <param name="critter"></param>
        public static void WeaponLoot(uwObject critter)
        {
            for (int i=0; i<2;i++)
            {
                int item_id = critterObjectDat.weaponloot(critter.item_id, i);
                int quality=0;
                int qty=0;
                if (item_id!=-1)
                {
                    //Create object now. Set properties later

                    //Now calculate quality 50:50 chance of rnd(0-64) or a figure based on the current dungeon level
                    quality = RandomLootQuality();
                    if ((item_id & 0x30) >> 4 == 1)
                    {
                        if (rangedObjectDat.RangedWeaponType(item_id) == 0xC0)
                        {
                            //item is ammo. it needs a qty.
                            qty = 4 + (Rng.r.Next(0, 8));                         
                        }
                    }

                    var obj = spawnLootObject(critter, item_id);
                    if (qty>0)
                    {
                        obj.link = (short)qty; 
                    }                              
                    Debug.Print($"Spawning {qty} {GameStrings.GetObjectNounUW(item_id)} of quality {quality}");
                }
            }
           
        }

        /// <summary>
        /// Loot spawns for NPCs have random quality. 50:50 either based on rng(0-63) or based on level no
        /// </summary>
        /// <returns></returns>
        private static int RandomLootQuality()
        {
            int quality;
            if (Rng.r.Next(0, 2) == 0)
            {
                quality = Rng.r.Next(1, 0x40);
            }
            else
            {
                quality = Rng.r.Next(0, playerdat.dungeon_level << 2) + (playerdat.dungeon_level << 2);
                quality = quality & 0x3F;
            }

            return quality;
        }

        /// <summary>
        /// Drops other loot objects based on rng
        /// </summary>
        /// <param name="critter"></param>
        public static void OtherLoot(uwObject critter)
        {
            for (int i=0;i<2;i++)
            {
                if ((Rng.r.Next(0,16) <critterObjectDat.otherloot_probability(critter.item_id, i)) || (DebugSpawnAllLoot))
                {
                    var item_id = critterObjectDat.otherloot_item(critter.item_id, i);
                    var quality = RandomLootQuality();
                    if (commonObjDat.qualitytype(item_id)==0xF)
                        {
                            quality = 0x40;
                        }
                    var obj = spawnLootObject(critter, item_id);
                    Debug.Print($"Spawning {GameStrings.GetObjectNounUW(item_id)} of quality {quality}");
                }
            }
        }
    } //end class
}//end namespace