namespace Underworld
{
    public class wand : objectInstance
    {

        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (!WorldObject)
            {
                 var spell = MagicEnchantment.GetSpellEnchantment(obj, playerdat.InventoryObjects);
                if (spell != null)
                {
                    MagicEnchantment.CastObjectSpell(obj, spell);
                    if (spell.LinkedSpellObject != null)
                    {
                        var hasRanOut = MagicEnchantment.DecrementSpellQuality(
                            objList: playerdat.InventoryObjects,
                            WorldObject: WorldObject,
                            parentObject: obj,
                            spell: spell);
                        if (hasRanOut)
                        {
                            uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_with_a_loud_snap_the_wand_cracks_));
                            obj.item_id+=4;
                            uimanager.UpdateInventoryDisplay();
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }//end class
}//end namespace