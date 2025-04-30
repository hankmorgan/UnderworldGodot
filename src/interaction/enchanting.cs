using System;

namespace Underworld
{
    /// <summary>
    /// Class for managing the logic around applying enchantments to objects.
    /// </summary>
    public class enchanting : UWClass
    {



        /// <summary>
        /// Applies an enchantment or recharges an enchanted wand.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        /// <param name="WorldObject"></param>
        public static void EnchantObject(int index, uwObject[] objList, bool WorldObject)
        {
            var failed = true;
            bool MakeAttemptVar8;
            var objToEnchant = objList[index];

            if (objToEnchant == null) { return; }

            var ExistingEnchantment = MagicEnchantment.GetSpellEnchantment(objToEnchant, objList);

            if (ExistingEnchantment != null)
            {   //is already enchanted
                var var2SpellMajorClass = ExistingEnchantment.SpellMajorClass;
                if ((ExistingEnchantment.IsFlagBit2Set) && (ExistingEnchantment.SpellMajorClass == 0))
                {
                    ExistingEnchantment.SpellMajorClass = RunicMagic.SpellList[ExistingEnchantment.SpellMinorClass].SpellMajorClass;
                    ExistingEnchantment.SpellMinorClass = RunicMagic.SpellList[ExistingEnchantment.SpellMinorClass].SpellMinorClass;
                }
                else
                {

                    if ((ExistingEnchantment.IsFlagBit2Set) && (CanObjectBeEnchanted(objToEnchant, ExistingEnchantment.SpellMajorClass, ExistingEnchantment.SpellMinorClass)))
                    {
                        //can object be enchanted.
                        //EnchantObjectwithEffectId andupdate failed variable
                        failed = ChargeSpellObjectwithEffectId(objToEnchant, objList, ExistingEnchantment.SpellMinorClass, playerdat.tileX, playerdat.tileY, WorldObject);
                        if (failed)//if true object has been destroyed.
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (ExistingEnchantment.IsFlagBit2Set)
                        {
                            //Failenchantment
                            FailEnchantment(objToEnchant, objList, WorldObject, playerdat.tileX, playerdat.tileY);
                            return;
                        }
                        else
                        {
                            if (ExistingEnchantment.SpellMajorClass == 0xC)
                            {
                                MakeAttemptVar8 = false;
                                if (objToEnchant.majorclass == 0)
                                {
                                    int diDifficulty = 0;
                                    int siSkill = 0;
                                    //ovr157_921
                                    if (
                                        (objToEnchant.minorclass > 1)
                                        ||
                                        (objToEnchant.minorclass <= 1 && ExistingEnchantment.SpellMinorClass >= 8)
                                        ||
                                        (objToEnchant.minorclass <= 1 && ExistingEnchantment.SpellMinorClass < 8 && (ExistingEnchantment.SpellMinorClass & 0xFFFB) >= 3)
                                    )
                                    {
                                        //ovr157_978:
                                        if ((objToEnchant.minorclass >= 2) && (ExistingEnchantment.SpellMinorClass & 0xFFF7) < 7)
                                        {
                                            MakeAttemptVar8 = true;
                                            diDifficulty = ExistingEnchantment.SpellMinorClass & 0xFFF7;
                                            siSkill = playerdat.play_level + (playerdat.Casting / 0xB) - 10;
                                        }
                                    }
                                    else
                                    {
                                        //ovr157_943:
                                        MakeAttemptVar8 = true;
                                        diDifficulty = ExistingEnchantment.SpellMinorClass & 0xFFF8;
                                        siSkill = (playerdat.Casting / 0xB) + (playerdat.play_level - 8) / 4;
                                    }
                                    if (MakeAttemptVar8)
                                    {
                                        if (diDifficulty <= siSkill)
                                        {
                                            failed = false;
                                            objToEnchant.link = (short)((objToEnchant.link & 0x1F0) | ((ExistingEnchantment.SpellMinorClass + 1) & 0xF) | 0x200);//increase the power of the existing enchantment
                                        }
                                        else
                                        {
                                            //blowup
                                            //Fail enchantment
                                            FailEnchantment(objToEnchant, objList, WorldObject, playerdat.tileX, playerdat.tileY);
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //ovr157_a01, not applicable 
                            }
                        }
                    }
                }
            }
            else
            {  //is not enchanted
                if (objToEnchant.majorclass == 0)
                {
                    uwObject linkedspell = null;
                    if ((objToEnchant.is_quant == 0) && (objToEnchant.link != 0))
                    {
                        linkedspell = objList[objToEnchant.link];
                    }

                    if (linkedspell == null)
                    {
                        objToEnchant.link = 201;// or maybe 102
                        objToEnchant.enchantment = 1;
                        objToEnchant.flags = (short)(objToEnchant.flags & 0xB); //clear flag 2
                        objToEnchant.is_quant = 1; //?why?
                        failed = false;
                        switch (objToEnchant.minorclass)
                        {
                            case 0:
                            case 1://weapons
                                {
                                    var newlink = ((((objToEnchant.link & 0xF) | 0x2C0) | ((Rng.r.Next(2) << 2) & 0xF)) & 0x1f0) | 0x200;
                                    objToEnchant.link = (short)newlink;
                                    break;
                                }
                            case 2://armour
                            case 3:
                                {
                                    var newlink = ((((objToEnchant.link & 0xF) | 0x2C0) | (((Rng.r.Next(2) << 3) / 3) & 0xF)) & 0x1f0) | 0x200;
                                    objToEnchant.link = (short)newlink;
                                    break;
                                }

                        }
                    }
                }
            }

            //check SkillCheckResult
            if (failed)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x12C));
            }
            else
            {
                uimanager.AddToMessageScroll($"{GameStrings.GetString(1, 0x12D)}{GameStrings.GetSimpleObjectNameUW(objToEnchant.item_id)}");
            }
        }

        /// <summary>
        /// Checks if the object can be enchanted.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="spellmajorclass"></param>
        /// <param name="spellminorclass"></param>
        /// <returns></returns>
        static bool CanObjectBeEnchanted(uwObject obj, int spellmajorclass, int spellminorclass)
        {
            if ((spellmajorclass == 7) && (spellminorclass == 0xE))
            {//Can't enchant using enchant spell
                return false;
            }
            if (spellmajorclass == 0xD && (spellminorclass >= 0xC && spellmajorclass <= 0xF))
            {//skip mana boost spells
                return false;
            }

            switch (obj.OneF0Class)
            {
                case 0xB://food
                case 0xE://potions
                case 0x13://books
                    return false;
                default:
                    return true;
            }
        }


        /// <summary>
        /// Tries to charge a linked spell object with a new charge.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="objList"></param>
        /// <param name="effectid"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="WorldObject"></param>
        /// <returns></returns>
        static bool ChargeSpellObjectwithEffectId(uwObject obj, uwObject[] objList, int effectid, int x, int y, bool WorldObject)
        {
            int var1;
            if (effectid >= 64)
            {
                var1 = 4;
            }
            else
            {
                var1 = 1 + (effectid / 8);
            }

            var var2 = 8 + ((1 + playerdat.play_level) / 2) - var1;


            var spellobject = objectsearch.FindMatchInObjectChain(
                ListHeadIndex: obj.link, majorclass: 4, minorclass: 2,
                classindex: 0, objList: objList, SkipLinks: true);
            if (spellobject == null)
            {
                return true;//enchanting has failed.
            }

            var var3 = spellobject.quality / Math.Abs(var2);

            var si_difficulty = (var3 + (16 - var2) << 10) / (var3 + 24);

            var rng = Rng.r.Next(0x3ff);

            if (rng < si_difficulty)
            {
                //fail
                EnchantFailureExplosion(obj, x, y);
                ObjectRemover.RemoveObjectFromLinkedList(obj.link, spellobject.index, objList, obj.PTR + 6);
                if (WorldObject)
                {
                    ObjectFreeLists.ReleaseFreeObject(spellobject);
                }

                obj.item_id = damage.GetObjectTypeDebris(obj, 0);
                if (WorldObject)
                {
                    objectInstance.RedrawFull(obj);
                }
                else
                {
                    uimanager.UpdateInventoryDisplay();
                }
                return true;
            }
            else
            {
                //sucess
                var newCharge = -1 - playerdat.Casting / 0xF;
                MagicObjectChargeUpdate(
                    obj: obj,
                    objList: objList,
                    WorldObject: WorldObject,
                    ChargeChangeFactor: newCharge);
                return false;
            }
        }

        /// <summary>
        /// Handles failing the enchantment and destroying the target object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="objList"></param>
        /// <param name="WorldObject"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        static void FailEnchantment(uwObject obj, uwObject[] objList, bool WorldObject, int x, int y)
        {
            EnchantFailureExplosion(obj, x, y);
            if (WorldObject)
            {
                damage.DamageObject(
                    objToDamage: obj,
                    basedamage: 0xFF,
                    damagetype: 0,
                    objList: objList,
                    WorldObject: WorldObject,
                    damagesource: 1);
            }
            else
            {
                //Apply equipment damage
                //Find object inventory slot
                damage.DamageEquipment(uimanager.CurrentSlot, 0xFF, 0, 2, 1);

                uimanager.UpdateInventoryDisplay();
            }

        }

        /// <summary>
        /// Handles an explosion when enchantment fails.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        static void EnchantFailureExplosion(uwObject obj, int x, int y)
        {
            var tile = UWTileMap.current_tilemap.Tiles[x, y];
            animo.SpawnAnimoInTile(
                subclassindex: 2, xpos: 3, ypos: 3, zpos: (short)(tile.floorHeight << 3), tileX: x, tileY: y);
            uimanager.AddToMessageScroll($"{GameStrings.GetString(1, 0x13E)}{GameStrings.GetSimpleObjectNameUW(obj.item_id)}{GameStrings.GetString(1, 0x13F)}");//your attempt to enchant X destroys it in a blaze of flame
            damage.DamageObjectsInTile(playerdat.tileX, playerdat.tileY, 1, 1);
        }

        /// <summary>
        /// Increases the charge of a spell linked to obj
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="objList"></param>
        /// <param name="WorldObject"></param>
        /// <param name="ChargeChangeFactor"></param>
        /// <returns>the final charge applied</returns>
        static int MagicObjectChargeUpdate(uwObject obj, uwObject[] objList, bool WorldObject, int ChargeChangeFactor)
        {
            if ((obj.is_quant == 0) && (obj.link != 0))
            {
                var spellobject = objectsearch.FindMatchInObjectChain(
                    ListHeadIndex: obj.link, majorclass: 4, minorclass: 2,
                    classindex: 0, objList: objList, SkipLinks: true);
                if (spellobject != null)
                {
                    int newCharge = 0;
                    if (spellobject.flags2 != 0)
                    {
                        newCharge = spellobject.quality - ChargeChangeFactor;
                        if (newCharge >= 0)
                        {
                            if (newCharge >= 0x40)
                            {
                                newCharge = 0x3F;
                            }
                            spellobject.quality = (short)newCharge;
                        }
                        else
                        {
                            if (Rng.r.Next(0xA) < 4)
                            {
                                ObjectRemover.RemoveObjectFromLinkedList(obj.link, spellobject.index, objList, obj.PTR + 6);
                                if (WorldObject)
                                {
                                    ObjectFreeLists.ReleaseFreeObject(spellobject);
                                }
                            }
                        }
                    }

                    if (newCharge <= 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return newCharge;
                    }
                }
                else
                {
                    return 0;//no spell object found
                }
            }
            else
            {
                return 0; //is a quant or has no link
            }
        }
    }//end class
}//end namespace