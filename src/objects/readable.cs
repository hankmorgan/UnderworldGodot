namespace Underworld
{
    public class Readable : objectInstance
    {

        /// <summary>
        /// The use interaction. Reads the object unless there is a special effect or magic to be cast
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool Use(uwObject obj, uwObject[] objlist, bool WorldObject)
        {
            if (_RES == GAME_UW2)
            {//TODO implement the UW2 logic.
                var magicenchantment = MagicEnchantment.GetSpellEnchantment(obj, objlist);
                if ((magicenchantment != null) && (!WorldObject))
                {
                    //uimanager.AddToMessageScroll($"Cast a spell from this object", colour: 2);
                    MagicEnchantment.CastObjectSpell(obj, magicenchantment);
                    ObjectCreator.Consume(obj,true);
                    return true;
                }
                else
                {
                    return LookAt(obj, objlist, WorldObject);
                }
            }
            else
            {
                if (obj.enchantment == 0 || (obj.enchantment == 1 && obj.majorclass == 5))
                {
                    if (obj.flags1 == 0)
                    {
                        if ((obj.link & 0x1FF) < 0x100)
                        {
                            return LookAt(obj, objlist, WorldObject);//default read
                        }
                        else
                        {
                            //rotworm stew and only rotworm stew
                            return rotwormstew.MixRotwormStew();
                        }
                    }
                    else
                    {
                        var cutsno = (obj.link & 0x1ff) + 0x100;
                        uimanager.AddToMessageScroll($"Display Cutscene {cutsno}", colour: 2);
                        return true;
                    }
                }
                else
                {
                    var magicenchantment = MagicEnchantment.GetSpellEnchantment(obj, objlist);
                    if (magicenchantment!=null)
                    {
                        //uimanager.AddToMessageScroll($"Cast a spell from this object", colour: 2);
                        MagicEnchantment.CastObjectSpell(obj, magicenchantment);
                        ObjectCreator.Consume(obj,true);
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
        public static bool LookAt(uwObject obj, uwObject[] objList, bool WorldObject)
        {
            if (WorldObject)
            {
                look.GenericLookDescription(obj.item_id, 1);
                return true;
            }
            else
            {
                if (obj.is_quant == 1)
                {
                    look.PrintLookDescription(obj: obj, objList: objList, lorecheckresult: look.LoreCheck(obj));
                    var magicenchantment = MagicEnchantment.GetSpellEnchantment(obj, objList);
                    if (magicenchantment == null)
                    {//check if enchanted. If not then read it
                        uimanager.AddToMessageScroll(GameStrings.GetString(3, obj.link - 0x200));
                    }
                }
                return true;
            }
        }
    }//end class
}//end namespace