using System;
using System.Diagnostics;

namespace Underworld
{
    //for handling loop updates for the player.
    public partial class playerdat : Loader
    {
        /// <summary>
        /// resets the player variables ahead of a status update
        /// </summary>
        static void ResetPlayer()
        {
            lightlevel = 0;
            Palette.ColourTone = 0;
            
            MagicalMotionAbilities = 0;
            for (int i=0; i<=3;i++)
            {
                LocationalArmourValues[i] = 0;
                LocationalProtectionValues[i] = 0;
            }       
            StealthScore1 = 13 - (Sneak/3);
            StealthScore2 = 15 - (Sneak/5);

            PlayerDamageTypeScale = 0;
            ValourBonus = 0;
            PoisonedWeapon = false;

            FreezeTimeEnchantment = false;
            RoamingSightEnchantment = false;
            SpeedEnchantment = false;
            TelekenesisEnchantment = false;
            HealthRegenEnchantment = false;
            ManaRegenEnchantment = false;
        }

        public static void PlayerStatusUpdate(bool CastOnEquip = false)
        {
            var DamageResistance = 0;
            var StealthBonus = 0;
            ResetPlayer();
            InitArmourValues();

            //Get brightest physcial light
            lightlevel = BrightestNonMagicalLight();          

            //cast active spell effects
            CastActiveSpellEffects(ref DamageResistance, ref StealthBonus);

            ApplyEquipmentEffects(CastOnEquip, ref DamageResistance, ref StealthBonus);

            //Apply the max damage resistance from enchantments/active spell effects
            ApplyDamageResistance(DamageResistance);
            ApplyStealthBonus(StealthBonus);

            RefreshLighting();//either brightest physical light or brightest magical light
        }


        /// <summary>
        /// Casts the 3 spell effects stored in the player data.
        /// </summary>
        /// <param name="DamageResistance"></param>
        /// <returns></returns>
        private static void CastActiveSpellEffects(ref int DamageResistance, ref int StealthBonus)
        {
            for (int i = 0; i < 3; i++)
            {
                if (i < ActiveSpellEffectCount)
                {
                    var stability = GetEffectStability(i);
                    var effectclass = GetEffectClass(i);
                    var major = effectclass & 0xF;
                    var minor = effectclass >> 4;
                    Debug.Print($"Player has spell effect {major},{minor} of {stability} ");
                    SpellCasting.CastEnchantedItemSpell(
                        majorclass: major,
                        minorclass: minor,
                        TriggeredByInventoryEvent: false,
                        DamageResistance: ref DamageResistance,
                        StealthBonus : ref StealthBonus,
                        PaperDollSlot: -1);
                    uimanager.SetSpellIcon(i, major, minor);
                }
                else
                {
                    uimanager.ClearSpellIcon(i);
                }
            }
        }

        /// <summary>
        /// Casts the spell effects from equipment on the paperdoll
        /// </summary>
        /// <param name="CastOnEquip"></param>
        /// <param name="DamageResistance"></param>
        /// <returns></returns>
        private static void ApplyEquipmentEffects(bool CastOnEquip, ref int DamageResistance, ref int StealthBonus)
        {
            //apply spell effects from inventory objects
            for (int i = 0; i < 10; i++)
            {
                var objindex = uimanager.GetPaperDollObjAtSlot(i);
                if (objindex != -1)
                {
                    var obj = InventoryObjects[objindex];
                    if (obj != null)
                    {
                        bool isValid = true;

                        if (!uimanager.ValidObjectForSlot(i, obj))
                        {
                            isValid = false;
                        }
                        if (uimanager.DominantHandSlot == i)
                        {//Check for weapon in dominant hand.
                            if (
                                !
                            (
                                ((obj.majorclass == 0) && (obj.minorclass == 0))
                                ||
                                ((obj.majorclass == 0 && obj.minorclass == 1) && (obj.classindex >= 8 && obj.classindex <= 10))
                            )
                            )
                            {
                                isValid = false;
                            }
                        }

                        if (isValid)
                        {
                            var spell = MagicEnchantment.GetSpellEnchantment(obj: obj, InventoryObjects);
                            if (spell != null)
                            {
                                SpellCasting.CastEnchantedItemSpell(
                                    majorclass: spell.SpellMajorClass,
                                    minorclass: spell.SpellMinorClass,
                                    TriggeredByInventoryEvent: CastOnEquip,
                                    DamageResistance: ref DamageResistance,
                                    StealthBonus: ref StealthBonus,
                                    PaperDollSlot: i
                                    );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Inints armour protection values
        /// </summary>
        private static void InitArmourValues()
        {
            //Get armour protections to store in LocationalArmourValues
            for (int i = 0; i <= 4; i++)
            {//check each inventory slot (helm, armour, leggings, boots and gloves)
                var objindex = uimanager.GetPaperDollObjAtSlot(i);
                var obj = InventoryObjects[objindex];
                if (obj != null)
                {
                    if (uimanager.ValidObjectForSlot(i, obj))
                    {//apply protection if object is valid
                        var protection = armourObjectDat.protection(obj.item_id);
                        switch (i)
                        {//apply protection to the appropiate body location
                            case 0://helm
                                LocationalArmourValues[3] += protection; break;
                            case 1: //armour
                                LocationalArmourValues[0] += protection; break;
                            case 2: // gloves
                                LocationalArmourValues[1] += protection; break;
                            case 3: //leggings                                
                            case 4: //boots
                                LocationalArmourValues[2] += protection; break;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Applies the bonuses from things like iron flesh, resist blows to the player. 
        /// Only the highest value applies
        /// </summary>
        /// <param name="DamageResistance"></param>
        public static void ApplyDamageResistance(int DamageResistance)
        {
            for (int i = 0; i<= LocationalArmourValues.GetUpperBound(0); i++)
            {
                 LocationalArmourValues[i] += DamageResistance;
            }
        }


        /// <summary>
        /// Provides an adjustment to 2 stealth scores which are presumably used later on in stealth detection when player is moving
        /// </summary>
        /// <param name="StealthBonus"></param>
        public static void ApplyStealthBonus(int StealthBonus)
        {
            for (int i = 0; i<3; i++)
            {
                var bit = (StealthBonus>>i) & 1;
                if (bit == 1)
                {//bit is set
                    switch (i)
                    {
                        case 0:
                            StealthScore1 = Math.Max(StealthScore1 - 0x10, 0); break;
                        case 1:
                            StealthScore2 = Math.Max(StealthScore2 - 5 , 0); break;
                        case 2:
                            StealthScore2 = Math.Max(StealthScore2 - 0x10 , 0); break;
                    }
                }
            }
        }

        public static void RefreshLighting()
        {
            Godot.RenderingServer.GlobalShaderParameterSet("cutoffdistance", shade.GetViewingDistance(playerdat.lightlevel));
           // Godot.RenderingServer.GlobalShaderParameterSet("shades", shade.shadesdata[playerdat.lightlevel].GetImage());
        }

        /// <summary>
        /// Calculates the brightest non magical or ambient level light source that surrounds the player
        /// </summary>
        /// <returns></returns>
        public static int BrightestNonMagicalLight()
        {
            int lightlevel = 0; //darkness
            //5,6,7,8
            for (int i = 5; i <= 8; i++)
            {
                var obj = GetInventorySlotObject(i);
                if (obj != null)
                {
                    if ((obj.majorclass == 2) && (obj.minorclass == 1) && (obj.classindex >= 4) && (obj.classindex <= 7))
                    {   //object is a lit light
                        var level = lightsourceObjectDat.brightness(obj.item_id);
                        if (level > lightlevel)
                        {
                            lightlevel = level;
                        }
                    }
                }
            }
            //If uw2 check for dungeon light level
            if (_RES == GAME_UW2)
            {
                var dungeon_ambientlight = DlDat.GetAmbientLight(playerdat.dungeon_level - 1);
                var remainder = dungeon_ambientlight % 10;
                var dlFlag = 0;
                if (dungeon_ambientlight >= 10)
                {
                    dlFlag = 1;
                }
                int tileLightFlag = UWTileMap.current_tilemap.Tiles[playerdat.tileX, playerdat.tileY].lightFlag;
                if ((tileLightFlag ^ dlFlag) == 1)
                {
                    dungeon_ambientlight = remainder;
                }
                else
                {
                    dungeon_ambientlight = -1;
                }
                if (dungeon_ambientlight > lightlevel)
                {
                    lightlevel = dungeon_ambientlight;
                }
            }
            return lightlevel;
        }
    }//end class
}//end namespace