namespace Underworld
{
    public class potion : objectInstance
    {
        public static bool Use(uwObject ObjectUsed, bool WorldObject)
        {
            if (!WorldObject)
            {
                QuaffPotion(ObjectUsed, true);
                return true;
            }
            return false;
        }


        public static void QuaffPotion(uwObject potion, bool UsedFromInventory)
        {
            uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_quaff_the_potion_in_one_gulp_));

            uwObject[] objList;

            MagicEnchantment spell;
            if (UsedFromInventory)
            {
                spell = MagicEnchantment.GetSpellEnchantment(potion, playerdat.InventoryObjects);
                objList = playerdat.InventoryObjects;
            }
            else
            {
                spell = MagicEnchantment.GetSpellEnchantment(potion, UWTileMap.current_tilemap.LevelObjects);
                objList = UWTileMap.current_tilemap.LevelObjects;
            }

            if (spell != null)
            {
                MagicEnchantment.CastObjectSpell(potion, spell);
            }
            else
            {//no spell was attached. See if a damage trap is present.
                if (potion.is_quant==0)
                {
                    if (potion.link!=0)
                    {                       
                        var dmgtrap = objectsearch.FindMatchInObjectChain(
                            ListHeadIndex: potion.link, 
                            majorclass: 6, 
                            minorclass: 0, 
                            classindex: 0, 
                            objList: objList);
                        if (dmgtrap!=null)
                        {
                            use.UseTriggerHasBeenActivated = true;//stop later infinite loop
                            a_damage_trap.ApplyDamageTrap(dmgtrap.owner>0, dmgtrap.quality);
                        }
                    }
                }
            }
            ObjectCreator.Consume(potion, UsedFromInventory);
        }

    }//end class
}//end namespace