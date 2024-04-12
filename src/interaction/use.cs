using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the use verb
    /// </summary>
    public class use : UWClass
    {
        /// <summary>
        /// Global flag in case the object uses fires the spell prematurely.
        /// </summary>
        public static bool SpellHasBeenCast = false;
        public static bool Use(int index, uwObject[] objList, bool WorldObject = true)
        {
            SpellHasBeenCast = false;
            if (useon.CurrentItemBeingUsed != null)
            {
                return useon.UseOn(index, objList, useon.CurrentItemBeingUsed, WorldObject);
            }
            // if (playerdat.ObjectInHand != -1)
            // {
            //     return useon.UseOn(index, objList, WorldObject);
            // }
            if (index == -1) { return false; }
            trap.ObjectThatStartedChain = index;
            bool result = false;
            if (index <= objList.GetUpperBound(0))
            {
                var obj = objList[index];
                Debug.Print($"Object {obj.majorclass}-{obj.minorclass}-{obj.classindex} {obj.a_name}");
                switch (obj.majorclass)
                {
                    case 0://weapons
                        {
                            result = UseMajorClass1(WorldObject, obj);
                            break;
                        }
                    case 1://npcs
                        {
                            talk.Talk(index, objList, WorldObject);
                            return true;
                        }
                    case 2:
                        {
                            result = UseMajorClass2(obj, objList, WorldObject);
                            break;
                        }
                    case 3:
                        {
                            result = UseMajorClass3(obj, objList, WorldObject);
                            break;
                        }
                    case 4:
                        {
                            result = UseMajorClass4(obj, objList, WorldObject);
                            break;
                        }
                    case 5:
                        {
                            result = UseMajorClass5(obj, objList, WorldObject);
                            break;
                        }
                    case 7:
                        {
                            result = UseMajorClass7(obj, objList, WorldObject);
                            break;
                        }
                }
                //Check for use trigger on this action and try activate if so.
                if ((obj.is_quant == 0) && (obj.link != 0))
                {
                    trigger.UseTrigger(
                        srcObject: obj,
                        triggerIndex: obj.link,
                        objList: objList);
                }
                //check for a spell
                if (!SpellHasBeenCast)
                {
                    if (obj != null)
                    {
                        switch(obj.majorclass)
                        {
                            case 0:
                            case 1:
                            case 5:
                            case 6:
                            case 7://do not cast spell from these objects major classes
                                break;
                            default:
                            {
                                if ((obj.majorclass==2) && (obj.minorclass == 0))
                                {
                                    //do not cast spells from containers. (except maybe the cornicopia?)
                                }   
                                else
                                    {
                                        result = UseItemCastSpell(objList, WorldObject, result, obj);
                                    }
                                    break;
                            }
                        }

                    }
                }
            }
            if (!result)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_cannot_use_that_));
            }

            return result;
        }

        private static bool UseMajorClass1(bool WorldObject, uwObject obj)
        {
            if ((obj.minorclass == 0) && (!WorldObject))
            {
                //weapons on paperdoll.
                if (playerdat.isLefty)
                {
                    if (uimanager.CurrentSlot == 8)
                    {
                        if (uimanager.InteractionMode!=uimanager.InteractionModes.ModeAttack)
                        {
                            uimanager.InteractionModeToggle(uimanager.InteractionModes.ModeAttack);
                        }
                        else
                        {
                            uimanager.InteractionModeToggle(uimanager.InteractionModes.ModeUse);
                        }                       
                        return true;
                    }
                }
                else
                {
                    if (uimanager.CurrentSlot == 7)
                    {
                        if (uimanager.InteractionMode!=uimanager.InteractionModes.ModeAttack)
                        {
                            uimanager.InteractionModeToggle(uimanager.InteractionModes.ModeAttack);
                        }
                        else
                        {
                            uimanager.InteractionModeToggle(uimanager.InteractionModes.ModeUse);
                        }    
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// General catch all spell cast. Allows casting of spells from objects that are not normally associated with magic. Eg the piece of wood of lightning
        /// </summary>
        /// <param name="objList"></param>
        /// <param name="WorldObject"></param>
        /// <param name="result"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool UseItemCastSpell(uwObject[] objList, bool WorldObject, bool result, uwObject obj)
        {
            var spell = MagicEnchantment.GetSpellEnchantment(obj, objList);
            if (spell != null)
            {
                MagicEnchantment.CastObjectSpell(obj, spell);
                if (spell.LinkedSpellObject != null)
                {
                    MagicEnchantment.DecrementSpellQuality(
                        objList: objList,
                        WorldObject: WorldObject,
                        parentObject: obj,
                        spell: spell);

                }
                return true;
            }
            return false;
        }


        public static bool UseMajorClass3(uwObject obj, uwObject[] objList, bool WorldObject)
        {
            switch (obj.minorclass)
            {
                case 1:
                    {//some misc items                    
                        switch (obj.classindex)
                        {
                            case 7://anvil
                                return anvil.Use(obj, WorldObject);
                            case 8:
                                return pole.Use(obj,WorldObject);
                        }
                        break;
                    }
                case 2:
                    {
                        if (_RES!=GAME_UW2)
                        {
                            switch (obj.classindex)
                            {
                                case 7://key of infinity
                                    return key_of_infinity.Use(obj, WorldObject);
                            }
                        }
                        else
                        {
                            switch(obj.classindex)
                            {
                                case >0 and <=7://a range of potions.
                                    return potion.Use(obj,objList, WorldObject);                                
                            }
                        }
                        break;
                    }
            }
            return false;
        }

        public static bool UseMajorClass2(uwObject obj, uwObject[] objList, bool WorldObject)
        {
            switch (obj.minorclass)
            {
                case 0:
                    {
                        if (obj.classindex < 0xF)
                        {//containers
                            return container.Use(obj, WorldObject);
                        }
                        else
                        {
                            //runebag
                            return runebag.Use(obj, WorldObject);
                        }
                    }
                case 1:
                    {
                        if (obj.classindex <= 7)
                        {//lights
                            return light.Use(obj, WorldObject);
                        }
                        else
                        {
                            //wands
                            if (obj.classindex<=0xB)
                                {
                                    return wand.Use(obj, WorldObject);
                                }
                        }
                        break;
                    }
                case 2:
                    {
                        switch (obj.classindex)
                        {
                            case 0x1://Storage crystal in UW2, gold coin in uw1
                                {
                                    if (_RES==GAME_UW2)
                                    {
                                        return storage_crystal.Use(obj, WorldObject);
                                    }
                                    break;
                                }
                            case 0xF:
                                {//picketwatch in uw2, gold nugget in uw1
                                    if (_RES == GAME_UW2)
                                    {
                                        return pocketwatch.Use(obj, WorldObject);
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case 3://food (including some potions)
                    {
                        return food.Use(obj, WorldObject);
                    }
            }
            return false;
        }

        public static bool UseMajorClass4(uwObject obj, uwObject[] objList, bool WorldObject)
        {
            switch (obj.minorclass)
            {
                case 0: //keys up to 0xE
                    {
                        if (_RES == GAME_UW2)
                        {
                            switch (obj.classindex)
                            {
                                case 0:
                                case 1:
                                    //LOCKPICK and curious implement.
                                    return lockpick.Use(obj, WorldObject);
                                case > 1 and <= 0xE://keys
                                    return doorkey.Use(obj, WorldObject);
                            }
                        }
                        else
                        {
                            switch (obj.classindex)
                            {
                                case 0:
                                case > 1 and < 0xE://keys
                                    return doorkey.Use(obj, WorldObject);
                                case 1://lockpick
                                    return lockpick.Use(obj, WorldObject);
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        if (_RES != GAME_UW2)
                        {
                            if (obj.classindex == 0xB)
                            {
                                return rotwormstew.Use(obj, WorldObject);
                            }
                        }
                        else
                        {//uw2
                            switch (obj.classindex)
                            {
                                case >= 8 and <= 0xF:
                                    return smallblackrockgem.Use(obj, WorldObject);
                            }
                        }
                        break;
                    }
                case 2:
                    {
                        switch (obj.classindex)
                        {
                            case 2:
                                {
                                    if (_RES != GAME_UW2)
                                    {
                                        return silverseed.use(obj, WorldObject);//plant silver seed
                                    }
                                    break;
                                }
                            case 3:
                            case 4://musical instruments
                                {
                                    return musicalinstrument.use(obj, WorldObject);
                                }
                            case 5://leeches
                                return leech.use(obj, WorldObject);
                            case 8://rock hammer
                                return rockhammer.Use(obj, WorldObject);
                            case 7: //spike and forcefield in uw2
                                if (_RES!=GAME_UW2)
                                {
                                    return spike.Use(obj, WorldObject);
                                }
                                break;
                            case 0xB://Fishing pole
                                return fishingpole.use(obj, WorldObject);
                            case 0xD://oilflask
                                return oilflask.Use(obj, WorldObject);
                        }

                        break;
                    }
                case 3: //readables (up to index 8)
                    {
                        if (_RES != GAME_UW2)
                        {
                            switch (obj.classindex)
                            {
                                case 0xB:
                                    return map.Use(obj, WorldObject);
                                default:
                                    return Readable.Use(obj, objList, WorldObject);
                            }
                        }
                        else
                        {//uw2
                            switch (obj.classindex)
                            {
                                case 0x9://a_bit of a map 
                                    return false;
                                case 0xA://a_map 
                                    return map.Use(obj, WorldObject);
                                case 0xB://a_dead plant 
                                    return false;
                                case 0xC://a_dead plant 
                                    return false;
                                case 0xD://a_bottle 
                                    return false;
                                case 0xE://a_stick 
                                    return false;
                                case 0xF://a_resilient sphere 
                                    return false;
                                default:
                                    return Readable.Use(obj, objList, WorldObject);
                            }
                        }
                    }
            }
            return false;
        }

        public static bool UseMajorClass5(uwObject obj, uwObject[] objList, bool WorldObject)
        {
            switch (obj.minorclass)
            {
                case 0: // Doors
                    {
                        return door.Use(obj);

                    }
                case 1: //3d models
                    {
                        switch (obj.classindex)
                        {
                            case 7://a_shrine
                                {
                                    return shrine.Use(obj);
                                }
                            case 0xB://barrel
                            case 0xD://chest
                            case 0xE://nightstand
                            {
                                return container.Use(obj, WorldObject);
                            }                            
                        }
                        break;
                    }
                case 2: //misc objects including readables
                    {
                        switch (obj.classindex)
                        {
                            case 1:
                            case 2: //rotary switches                                
                                return buttonrotary.Use(obj);
                            case 5://gravestone
                                return gravestone.Use(obj);
                            case 6: // a readable sign. interaction is also a look
                                return writing.LookAt(obj);
                            case 0xE://tmap
                            case 0xF:
                                return tmap.LookAt(obj);
                            default:
                                return true;
                        }
                    }
                case 3: //buttons
                    {
                        return button.Use(obj);
                    }
            }
            return false;
        }

        public static bool UseMajorClass7(uwObject obj, uwObject[] objList, bool WorldObject)
        {
            switch (obj.classindex)
            {
                case 9://fountain animo.
                    return fountain.FountainAnimoUse(obj);
            }
            return false;
        }

    }//end class
}//end namespace