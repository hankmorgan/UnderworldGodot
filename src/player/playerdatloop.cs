using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    //for handling loop updates for the player.
    public partial class playerdat : Loader
    {
        static int previousLightLevel;
        public static bool previousMazeNavigation = false;

        static double playertimer;
        static int playerUpdateCounter;

        static int PreviousClockValue;
        public static void PlayerTimedLoop(double delta)
        {
            if ((!main.blockmouseinput) && (uimanager.InGame))
            {
                playertimer += delta;
                if (playertimer >= 1f)
                {
                    var secondelasped = (int)(playertimer / 1);
                    playertimer = 0f;
                    for (int s = 0; s < secondelasped; s++)
                    {
                        ClockValue += 0x40; //not sure what the exact rate should be here. for the moment assuming this is 1 second of time

                        if ((ClockValue % 20) < PreviousClockValue)//every 20 seconds
                        {
                            playerUpdateCounter++;

                            UpdateLightStability(playerUpdateCounter);

                            for (int i = 0; i < 3; i++)
                            {
                                if (i < ActiveSpellEffectCount)
                                {
                                    var stability = GetEffectStability(i);
                                    stability--;
                                    if (stability == 1)
                                    {
                                        CancelEffect(i);
                                        break; //breaking here since cancelling effects changes the order of the list. too annoying a problem to solve now.
                                    }
                                    else
                                    {
                                        SetEffectStability(i, stability);
                                    }
                                }
                            }

                            if (shrooms > 0)
                            {
                                shrooms--;
                            }

                            if (ManaRegenEnchantment)
                            {
                                play_mana = Math.Min(play_mana + 1, max_mana);
                            }
                            if (HealthRegenEnchantment)
                            {
                                play_hp = Math.Min(play_hp + 1, max_hp);
                            }

                            SwimmingSkillCheck();

                            if (_RES == GAME_UW2)
                            {
                                if (GetQuest(50) == 1)
                                {//the keep is crashing
                                    KillornKeepEvent();
                                }
                            }

                            if (DreamingInVoid)
                            {//TODO check if dreaming in void and count down
                                Debug.Print("Dreaming in void. count down dream plant value");
                            }

                            if ((playerUpdateCounter % 3) == 0)
                            {//every 60 seconds
                                if (play_poison > 0)
                                {
                                    Debug.Print("TODO apply poison damage");
                                    var dam = play_poison >> 1;
                                    play_poison = (byte)Math.Max(play_poison - 1, 0);
                                    play_hp = Math.Max(play_hp - dam, 0);
                                }

                                var manaskillcheck = (int)SkillCheck(ManaSkill, 10);
                                if (manaskillcheck > 0)
                                {
                                    play_mana = Math.Min(play_mana + manaskillcheck, max_mana);
                                }
                            }//end every 60 seconds update
                            if ((playerUpdateCounter % 30) == 0)
                            {//every 5 minutes
                                var hungerchange = -3 - Rng.r.Next(0, 3);
                                play_hunger = (byte)Math.Max(play_hunger + hungerchange, 0);

                                if (intoxication > 0)
                                {
                                    intoxication--;
                                }

                                if ((Rng.r.Next(0, 0xffff) & 0x3) == 0)
                                {
                                    Debug.Print("Find and trigger create object traps that are on map!");
                                }

                                Debug.Print("Do something with Npc hunger");

                                Debug.Print("Increment values at  bx+3a(fatigue), bx+3b(related to food health regen), bx+3c (unknown)");
                                play_fatigue = (byte)Math.Min(play_fatigue + 1, 0xFF);
                                
                                maybefoodhealthbonus = (byte)Math.Min(maybefoodhealthbonus + 1, 0xFF);

                                var hpskillcheck = (int)SkillCheck(STR, 10);
                                if (hpskillcheck > 0)
                                {//regular health regen
                                    play_hp = Math.Min(play_hp + hpskillcheck, max_hp);
                                }
                            }//end every 5 mins update
                            if ((playerUpdateCounter % 60) == 0)//every 20 mins
                            {
                                Debug.Print("TODO Update 'day' value");
                                Debug.Print("Do something with scd.ark");
                                playerUpdateCounter = 0;
                            }//end every 20 mins update

                            PlayerStatusUpdate();
                        }//end every 20 seconds update
                        PreviousClockValue = ClockValue % 20;
                    }//end seconds for loop                 
                }  //every second              
            }//end ingame check
        }


        /// <summary>
        /// Screenshakes and damage when killorn is crashing
        /// </summary>
        public static void KillornKeepEvent()
        {
            Debug.Print("Killorn is crashing!!");
        }

        /// <summary>
        /// Does a skill check every 20s when in water to see if damage is taken while swimming
        /// </summary>
        public static void SwimmingSkillCheck()
        {
            //TODO
        }

        static void UpdateLightStability(int updatecounter)
        {
            //loop each lit light source.
            //when updatecounter % lightsource class index. decrease light quality by 1 point
            for (int slot = 5; slot <= 8; slot++)
            {
                var objInSlot = GetInventorySlotObject(slot);
                if (objInSlot != null)
                {
                    if ((objInSlot.majorclass == 2) && (objInSlot.minorclass == 1) && (objInSlot.classindex >= 4 && objInSlot.classindex <= 7))
                    {
                        //a lit light source
                        if (updatecounter % objInSlot.classindex == 0)
                        {
                            objInSlot.quality = (short)Math.Max(objInSlot.quality - 1, 1);
                            if (objInSlot.quality == 1)
                            {
                                light.LightOff(objInSlot);
                                uimanager.UpdateInventoryDisplay();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// resets the player variables ahead of a status update
        /// </summary>
        static void ResetPlayer()
        {
            previousLightLevel = lightlevel;
            lightlevel = 0;
            Palette.ColourTone = 0;
            Palette.CurrentPalette = 0;

            MagicalMotionAbilities = 0;
            for (int i = 0; i <= 3; i++)
            {
                LocationalArmourValues[i] = 0;
                LocationalProtectionValues[i] = 0;
            }
            StealthScore1 = 13 - (Sneak / 3);
            StealthScore2 = 15 - (Sneak / 5);

            PlayerDamageTypeScale = 0;
            ValourBonus = 0;
            PoisonedWeapon = false;

            FreezeTimeEnchantment = false;
            RoamingSightEnchantment = false;
            SpeedEnchantment = false;
            TelekenesisEnchantment = false;
            HealthRegenEnchantment = false;
            ManaRegenEnchantment = false;
            MazeNavigation = false;
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

            if (shrooms != 0)
            {
                Palette.CurrentPalette = Rng.r.Next(1, 4);
                //shrooms--; //eventually this should be on a timer
            }


            RefreshLighting();//either brightest physical light or brightest magical light
            ApplyMazeNavigation();//handles tybals maze
            if (automap.CanMap(dungeon_level))
            {
                UpdateAutomap();//update the visited status of nearby tiles
            }
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
                    //Debug.Print($"Player has spell effect {major},{minor} of {stability} ");
                    SpellCasting.CastEnchantedItemSpell(
                        majorclass: major,
                        minorclass: minor,
                        TriggeredByInventoryEvent: false,
                        DamageResistance: ref DamageResistance,
                        StealthBonus: ref StealthBonus,
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
            for (int i = 0; i <= LocationalArmourValues.GetUpperBound(0); i++)
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
            for (int i = 0; i < 3; i++)
            {
                var bit = (StealthBonus >> i) & 1;
                if (bit == 1)
                {//bit is set
                    switch (i)
                    {
                        case 0:
                            StealthScore1 = Math.Max(StealthScore1 - 0x10, 0); break;
                        case 1:
                            StealthScore2 = Math.Max(StealthScore2 - 5, 0); break;
                        case 2:
                            StealthScore2 = Math.Max(StealthScore2 - 0x10, 0); break;
                    }
                }
            }
        }

        public static void RefreshLighting()
        {
            Godot.RenderingServer.GlobalShaderParameterSet("cutoffdistance", shade.GetViewingDistance(lightlevel));
            if (previousLightLevel != lightlevel)
            {
                UpdateAutomap();//refresh automap visibility
            }
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
                var dungeon_ambientlight = DlDat.GetAmbientLight(dungeon_level - 1);
                var remainder = dungeon_ambientlight % 10;
                var dlFlag = 0;
                if (dungeon_ambientlight >= 10)
                {
                    dlFlag = 1;
                }
                if (UWTileMap.ValidTile(tileX, tileY))
                {
                    int tileLightFlag = UWTileMap.current_tilemap.Tiles[tileX, tileY].lightFlag;
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
            }
            return lightlevel;
        }


        /// <summary>
        /// Updates a range of tiles in the automap around the player
        /// </summary>
        public static void UpdateAutomap()
        {
            //depending on light level. need to confirm if below math is okay
            var range = 1 + (lightlevel / 2);
            automap.MarkRangeOfTilesVisited(
                range: range,
                cX: tileX,
                cY: tileY,
                dungeon_level: dungeon_level
                );
        }

        static void ApplyMazeNavigation()
        {
            if (_RES != GAME_UW2)
            {
                if (dungeon_level == 7)
                {
                    if (previousMazeNavigation != MazeNavigation)
                    {
                        //change in stage
                        if (MazeNavigation)
                        {
                            //apply effect
                            //set tiles to use texture 222
                            var material = tileMapRender.mapTextures.GetMaterial(52, UWTileMap.current_tilemap.texture_map);
                            material.SetShaderParameter("texture_albedo", (Texture)tileMapRender.mapTextures.LoadImageAt(222));
                        }
                        else
                        {
                            //remove effect
                            //set tiles to us texture 224
                            var material = tileMapRender.mapTextures.GetMaterial(52, UWTileMap.current_tilemap.texture_map);
                            material.SetShaderParameter("texture_albedo", (Texture)tileMapRender.mapTextures.LoadImageAt(224));
                        }
                    }
                }
            }
            previousMazeNavigation = MazeNavigation;
        }

    }//end class
}//end namespace