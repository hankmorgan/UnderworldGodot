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
        public static bool UseTriggerHasBeenActivated = false;
        public static bool Use(uwObject ObjectUsed, uwObject UsingObjectOrCharacter, uwObject[] objList, bool WorldObject = true)
        {            
            SpellHasBeenCast = false;
            UseTriggerHasBeenActivated = false;

            //if (index == -1) { return false; }
            trap.ObjectThatStartedChain = ObjectUsed.index;
            bool result = false;
            if (ObjectUsed != null)
            {
                //var obj = objList[index];
                Debug.Print($"Object {ObjectUsed.majorclass}-{ObjectUsed.minorclass}-{ObjectUsed.classindex} {ObjectUsed.a_name}");
                switch (ObjectUsed.majorclass)
                {
                    case 0://weapons
                        {
                            result = UseMajorClass0(ObjectUsed, UsingObjectOrCharacter, WorldObject);
                            break;
                        }
                    case 1://npcs
                        {
                            talk.Talk(ObjectUsed, WorldObject);
                            return true;
                        }
                    case 2:
                        {
                            result = UseMajorClass2(ObjectUsed, WorldObject);
                            break;
                        }
                    case 3:
                        {
                            result = UseMajorClass3(ObjectUsed, WorldObject);
                            break;
                        }
                    case 4:
                        {
                            result = UseMajorClass4(ObjectUsed, WorldObject);
                            break;
                        }
                    case 5:
                        {
                            result = UseMajorClass5(ObjectUsed, WorldObject);
                            break;
                        }
                    case 7:
                        {
                            result = UseMajorClass7(ObjectUsed, WorldObject);
                            break;
                        }
                }
                //Check for use trigger on this action and try activate if so.
                if (!UseTriggerHasBeenActivated)
                {
                    trigger.TriggerObjectLink
                    (
                        character: 1,
                        ObjectUsed: ObjectUsed,
                        triggerType: (int)triggerObjectDat.triggertypes.USE,
                        triggerX: ObjectUsed.tileX,
                        triggerY: ObjectUsed.tileY,
                        objList: objList
                    );
                }
                //check for a spell
                if (!SpellHasBeenCast)
                {
                    if (ObjectUsed != null)
                    {
                        if (ObjectUsed.link != 0)
                        {
                            switch (ObjectUsed.majorclass)
                            {
                                case 0:
                                case 1:
                                case 5:
                                case 6:
                                case 7://do not cast spell from these objects major classes
                                    break;
                                default:
                                    {
                                        if ((ObjectUsed.majorclass == 2) && (ObjectUsed.minorclass == 0))
                                        {
                                            //do not normally cast spells from containers. Check if directly contains a spell. Eg the Cornucopia.
                                            var linkedspell = objectsearch.FindMatchInObjectChain(
                                                ListHeadIndex: ObjectUsed.link, 
                                                majorclass: 4, minorclass: 2, classindex: 0, 
                                                objList: objList, 
                                                SkipNext: false, 
                                                SkipLinks: true );
                                            if (linkedspell != null)
                                            {
                                                result = UseItemCastSpell(objList: objList, WorldObject: WorldObject, result: result, obj: ObjectUsed) | result; 
                                            }
                                        
                                        }
                                        else
                                        {
                                            result = UseItemCastSpell(objList: objList, WorldObject: WorldObject, result: result, obj: ObjectUsed) | result;
                                        }
                                        break;
                                    }
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

        private static bool UseMajorClass0(uwObject ObjectUsed, uwObject UsingObjectOrCharacter, bool WorldObject)
        {
            if ((ObjectUsed.minorclass == 0) && (!WorldObject))
            {
                //weapons on paperdoll.
                if (playerdat.isLefty)
                {
                    if (uimanager.CurrentSlot == 8)
                    {
                        if (uimanager.InteractionMode != uimanager.InteractionModes.ModeAttack)
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
                        if (uimanager.InteractionMode != uimanager.InteractionModes.ModeAttack)
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
            else
            {
                if (WorldObject)
                {
                    if (ObjectUsed.minorclass == 1)
                    {
                        //projectile has hit the UsingObjectOrCharacter.
                        if (UsingObjectOrCharacter!=null)
                        {
                            combat.MissileImpact(ObjectUsed, UsingObjectOrCharacter);
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


        public static bool UseMajorClass3(uwObject ObjectUsed, bool WorldObject)
        {
            switch (ObjectUsed.minorclass)
            {
                case 1:
                    {//some misc items                    
                        switch (ObjectUsed.classindex)
                        {
                            case 7://anvil
                                return anvil.Use(ObjectUsed, WorldObject);
                            case 8:
                                return pole.Use(ObjectUsed, WorldObject);
                        }
                        break;
                    }
                case 2:
                    {
                        if (_RES != GAME_UW2)
                        {
                            switch (ObjectUsed.classindex)
                            {
                                case 7://key of infinity
                                    return key_of_infinity.Use(ObjectUsed, WorldObject);
                            }
                        }
                        else
                        {
                            switch (ObjectUsed.classindex)
                            {
                                case > 0 and <= 7://a range of potions.
                                    return potion.Use(ObjectUsed, WorldObject);
                            }
                        }
                        break;
                    }
            }
            return false;
        }

        public static bool UseMajorClass2(uwObject ObjectUsed, bool WorldObject)
        {
            switch (ObjectUsed.minorclass)
            {
                case 0:
                    {
                        if (ObjectUsed.classindex < 0xF)
                        {//containers
                            return container.Use(ObjectUsed, WorldObject);
                        }
                        else
                        {
                            //runebag
                            return runebag.Use(ObjectUsed, WorldObject);
                        }
                    }
                case 1:
                    {
                        if (ObjectUsed.classindex <= 7)
                        {//lights
                            return light.Use(ObjectUsed, WorldObject);
                        }
                        else
                        {
                            //wands
                            if (ObjectUsed.classindex <= 0xB)
                            {
                                return wand.Use(ObjectUsed, WorldObject);
                            }
                        }
                        break;
                    }
                case 2:
                    {
                        switch (ObjectUsed.classindex)
                        {
                            case 0x1://Storage crystal in UW2, gold coin in uw1
                                {
                                    if (_RES == GAME_UW2)
                                    {
                                        return storage_crystal.Use(ObjectUsed, WorldObject);
                                    }
                                    break;
                                }
                            case 0xF:
                                {//picketwatch in uw2, gold nugget in uw1
                                    if (_RES == GAME_UW2)
                                    {
                                        return pocketwatch.Use(ObjectUsed, WorldObject);
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case 3://food (including some potions)
                    {
                        return food.Use(ObjectUsed, WorldObject);
                    }
            }
            return false;
        }

        public static bool UseMajorClass4(uwObject ObjectUsed, bool WorldObject)
        {
            switch (ObjectUsed.minorclass)
            {
                case 0: //keys up to 0xE
                    {
                        if (_RES == GAME_UW2)
                        {
                            switch (ObjectUsed.classindex)
                            {
                                case 0:
                                case 1:
                                    //LOCKPICK and curious implement.
                                    return lockpick.Use(ObjectUsed, WorldObject);
                                case > 1 and <= 0xE://keys
                                    return doorkey.Use(ObjectUsed, WorldObject);
                            }
                        }
                        else
                        {
                            switch (ObjectUsed.classindex)
                            {
                                case 0:
                                case > 1 and < 0xE://keys
                                    return doorkey.Use(ObjectUsed, WorldObject);
                                case 1://lockpick
                                    return lockpick.Use(ObjectUsed, WorldObject);
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        if (_RES != GAME_UW2)
                        {
                            switch(ObjectUsed.classindex)
                            {
                                case 4://exploding book
                                    return explodingbook.Use(ObjectUsed, WorldObject);
                                case 0xB:
                                    return rotwormstew.Use(ObjectUsed, WorldObject);

                            }
                        }
                        else
                        {//uw2
                            switch (ObjectUsed.classindex)
                            {
                                case 4://dream plant
                                    return food.SpecialFoodCases(ObjectUsed, WorldObject);
                                case >= 8 and <= 0xF:
                                    return smallblackrockgem.Use(ObjectUsed, WorldObject);
                            }
                        }
                        break;
                    }
                case 2:
                    {
                        switch (ObjectUsed.classindex)
                        {
                            case 1://bedroll
                                sleep.Sleep(1);
                                return true;
                            case 2:
                                {
                                    if (_RES != GAME_UW2)
                                    {
                                        return silverseed.use(ObjectUsed, WorldObject);//plant silver seed
                                    }
                                    break;
                                }
                            case 3:
                            case 4://musical instruments
                                {
                                    return musicalinstrument.use(ObjectUsed, WorldObject);
                                }
                            case 5://leeches
                                return leech.use(ObjectUsed, WorldObject);
                            case 8://rock hammer
                                return rockhammer.Use(ObjectUsed, WorldObject);
                            case 7: //spike and forcefield in uw2
                                if (_RES != GAME_UW2)
                                {
                                    return spike.Use(ObjectUsed, WorldObject);
                                }
                                break;
                            case 0xB://Fishing pole
                                return fishingpole.use(ObjectUsed, WorldObject);
                            case 0xD://oilflask
                                return oilflask.Use(ObjectUsed, WorldObject);
                            case 0xE://foutain
                                return fountain.Use(ObjectUsed);
                        }

                        break;
                    }
                case 3: //readables (up to index 8)
                    {
                        if (_RES != GAME_UW2)
                        {
                            switch (ObjectUsed.classindex)
                            {
                                case 0xB:
                                    return map.Use(ObjectUsed, WorldObject);
                                default:
                                    return Readable.Use(ObjectUsed, WorldObject);
                            }
                        }
                        else
                        {//uw2
                            switch (ObjectUsed.classindex)
                            {
                                case 0x9://a_bit of a map 
                                    return false;
                                case 0xA://a_map 
                                    return map.Use(ObjectUsed, WorldObject);
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
                                    return Readable.Use(ObjectUsed, WorldObject);
                            }
                        }
                    }
            }
            return false;
        }

        public static bool UseMajorClass5(uwObject ObjectUsed, bool WorldObject)
        {
            switch (ObjectUsed.minorclass)
            {
                case 0: // Doors
                    {
                        return door.Use(ObjectUsed);

                    }
                case 1: //3d models
                    {
                        switch (ObjectUsed.classindex)
                        {
                            case 7://a_shrine
                                {
                                    return shrine.Use(ObjectUsed);                                
                                }
                            case 0xB://barrel
                            case 0xD://chest
                            case 0xE://nightstand
                                {
                                    return container.Use(ObjectUsed, WorldObject);
                                }
                        }
                        break;
                    }
                case 2: //misc objects including readables
                    {
                        switch (ObjectUsed.classindex)
                        {
                            case 1:
                            case 2: //rotary switches                                
                                return buttonrotary.Use(ObjectUsed);
                            case 5://gravestone
                                return gravestone.Use(ObjectUsed);
                            case 6: // a readable sign. interaction is also a look
                                return writing.LookAt(ObjectUsed);
                            case 7://bed (UW2 only object)
                                sleep.Sleep(2);
                                return true;
                            case 0xE://tmap
                            case 0xF:
                                return tmap.LookAt(ObjectUsed);
                            default:
                                return true;
                        }
                    }
                case 3: //buttons
                    {
                        return button.Use(ObjectUsed);
                    }
            }
            return false;
        }

        public static bool UseMajorClass7(uwObject ObjectUsed, bool WorldObject)
        {
            switch (ObjectUsed.classindex)
            {
                case 9://fountain animo.
                    return fountain.FountainAnimoUse(ObjectUsed);
            }
            return false;
        }

    }//end class
}//end namespace