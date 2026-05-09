using System.Diagnostics;

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
            {
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
                        //uimanager.AddToMessageScroll($"Display Cutscene {cutsno}", colour: 2);
                        uimanager.EnableDisable(uimanager.instance.uwviewport,false); 
                        cutsplayer.PlayCutscene(CutsceneNo: 0x100 + (ObjectUsed.link & 0x1FF), callBackMethod: cutsplayer.RestoreViewPort, useSingleRedChannel: true);
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

                    var magicenchantment = MagicEnchantment.GetSpellEnchantment(ObjectUsed, objList);
                    if (magicenchantment == null)
                    {
                        if (_RES != GAME_UW2)
                        {
                            //check if has special flags. If not then read it
                            ReadObject(ObjectUsed);
                        }
                        else
                        {
                            if (ObjectUsed.item_id == 0x139)//map piece
                            {
                                //TODO
                                Debug.Print("MAP PIECE LOOKAT");
                            }
                            else
                            {
                                if ((ObjectUsed.enchantment == 1) && (ObjectUsed.majorclass == 5))
                                {
                                    return false;
                                }
                                else
                                {
                                    if ((ObjectUsed.link & 0x1FF) == 6)
                                    {
                                        playerdat.SetQuest(106,1);//You have looked at Mors Gothris spellbook.
                                    }
                                    ReadObject(ObjectUsed);
                                }
                            }
                        }                        
                    }
                    else
                    {
                        look.PrintLookDescription(obj: ObjectUsed, objList: objList, lorecheckresult: look.LoreCheck(ObjectUsed));
                    }
                }
                return true;
            }
        }


        /// <summary>
        /// Look at the flags on the object and read it's contents as appropiate.
        /// </summary>
        /// <param name="ObjectUsed"></param>
        private static void ReadObject(uwObject ObjectUsed)
        {
            if (ObjectUsed.flags1 == 0)
            {
                if (ObjectUsed.link > 512)
                {
                    //just display the text
                    uimanager.AddToMessageScroll(GameStrings.GetString(3, ObjectUsed.link & 0x1FF));
                }
                else
                {
                    //display the text with the prefix You read the item name
                    uimanager.AddToMessageScroll($"You read the {GameStrings.GetSimpleObjectNameUW(ObjectUsed.item_id)}");
                    uimanager.AddToMessageScroll(GameStrings.GetString(3, ObjectUsed.link & 0x1FF));
                }
            }
            else
            {
                uimanager.EnableDisable(uimanager.instance.uwviewport,false); 
                cutsplayer.PlayCutscene(CutsceneNo: 0x100 + (ObjectUsed.link & 0x1FF), callBackMethod: cutsplayer.RestoreViewPort, useSingleRedChannel: true);
            }
        }

    }//end class
}//end namespace