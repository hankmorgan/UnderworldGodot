using System;
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Class for managing the spawning of npc loot objects
    /// </summary>
    public partial class npc : objectInstance
    {
        public static void loot(uwObject critter)
        {
            DropValuableObjectLoot(critter);

            //TODO set the loot dropped flag on the NPC.
        }

        /// <summary>
        /// Drops coins and gems into then npc inventory.
        /// </summary>
        /// <param name="critter"></param>
        public static void DropValuableObjectLoot(uwObject critter)
        {
            //The game world chooses which valuable item type is dropped in the valuables object class (starting at coin)
            var world = worlds.GetWorldNo(playerdat.dungeon_level);
            
            var Critter_var_5 = critterObjectDat.Unk26_F(critter.item_id);
            var Critter_var_6 = critterObjectDat.Unk26_R4(critter.item_id);
            
            var r = new Random();
            if ((r.Next(0, 16) > Critter_var_6) && (false))
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
                /
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
                    Debug.Print($"Spawning {var_3_qty} {GameStrings.GetObjectNounUW(coinitemid + valuable_type_var_1)}");
                }
            }
        }
    } //end class
}//end namespace