namespace Underworld
{
    public class Readable : objectInstance
    {

        /// <summary>
        /// The use interaction. Reads the object unless there is a special effect or magic to be cast
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool Use(uwObject ObjectUsed, bool WorldObject)
        {
            uwObject[] objList;
            if (WorldObject)
            {
                objList = UWTileMap.current_tilemap.LevelObjects;
            }
            else
            {
                objList = playerdat.InventoryObjects;
            }
            if (_RES == GAME_UW2)
            {//TODO implement the UW2 logic.
                var magicenchantment = MagicEnchantment.GetSpellEnchantment(ObjectUsed, objList);
                if ((magicenchantment != null) && (!WorldObject))
                {
                    //uimanager.AddToMessageScroll($"Cast a spell from this object", colour: 2);
                    MagicEnchantment.CastObjectSpell(ObjectUsed, magicenchantment);
                    ObjectCreator.Consume(ObjectUsed, true);
                    return true;
                }
                else
                {
                    return LookAt(ObjectUsed, WorldObject);
                }
            }
            else
            {
                if (ObjectUsed.enchantment == 0 || (ObjectUsed.enchantment == 1 && ObjectUsed.majorclass == 5))
                {
                    if (ObjectUsed.flags1 == 0)
                    {
                        if ((ObjectUsed.link & 0x1FF) < 0x100)
                        {
                            return LookAt(ObjectUsed, WorldObject);//default read
                        }
                        else
                        {
                            //rotworm stew and only rotworm stew
                            return rotwormstew.MixRotwormStew();
                        }
                    }
                    else
                    {
                        var cutsno = (ObjectUsed.link & 0x1ff) + 0x100;
                        uimanager.AddToMessageScroll($"Display Cutscene {cutsno}", colour: 2);
                        return true;
                    }
                }
                else
                {
                    var magicenchantment = MagicEnchantment.GetSpellEnchantment(ObjectUsed, objList);
                    if (magicenchantment != null)
                    {
                        //uimanager.AddToMessageScroll($"Cast a spell from this object", colour: 2);
                        MagicEnchantment.CastObjectSpell(ObjectUsed, magicenchantment);
                        ObjectCreator.Consume(ObjectUsed, true);
                    }
                    return true;
                }
            }
        }

        /// <summary>
        /// The read interation.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool LookAt(uwObject ObjectUsed, bool WorldObject)
        {
            if (WorldObject)
            {
                look.GenericLookDescription(ObjectUsed.item_id, 1);
                return true;
            }
            else
            {
                if (ObjectUsed.is_quant == 1)
                {
                    uwObject[] objList;
                    if (WorldObject)
                    {
                        objList = UWTileMap.current_tilemap.LevelObjects;
                    }
                    else
                    {
                        objList = playerdat.InventoryObjects;
                    }
                    look.PrintLookDescription(obj: ObjectUsed, objList: objList, lorecheckresult: look.LoreCheck(ObjectUsed));
                    var magicenchantment = MagicEnchantment.GetSpellEnchantment(ObjectUsed, objList);
                    if (magicenchantment == null)
                    {//check if enchanted. If not then read it
                        uimanager.AddToMessageScroll(GameStrings.GetString(3, ObjectUsed.link - 0x200));
                    }
                }
                return true;
            }
        }
    }//end class
}//end namespace