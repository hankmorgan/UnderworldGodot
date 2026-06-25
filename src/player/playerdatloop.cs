using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    //for handling loop updates for the player.
    public partial class playerdat : Loader
    {
        public static int previousLightLevel = -1;
        public static bool previousMazeNavigation = false;

        static double playertimer;
        static int playerUpdateCounter;

        //static int PreviousClockValue;
        static int secondcounter = 0;

        static byte FootSteps_77A = 0xFF;
        static uint FootStepTimerA_19DF;
        static uint FootStepTimerB_19E5;
        static uint FootstepSoundIndex_dseg_79B = 0;
        static uint WaterSoundPanIndex_dseg_79B = 0;
        static uint dseg_79C = 0;//used in footsteps

        static byte[] FootstepSoundPanning = new byte[] { 0x38, 0x48 };
        static byte[] FootStepSoundEffectsUW2 = new byte[] { 0x1, 0x2, 0x2F, 0x30, 0x1D, 0x1D };

        public static int NoOfTilesDiscovered = 0;
        public static void PlayerTimedLoop(double delta)
        {
            if ((!uimanager.blockmouseinput) && (uimanager.InGame))
            {
                playertimer += delta;

                XMIMusic.RefreshMusic();//checks the playing themes and updates accordingly
                //every frame
                //Compass updates
                if (_RES == GAME_UW2)
                {
                    if (CurrentWorld != 8)
                    {
                        uimanager.UpdateCompass();
                    }
                }
                else
                {
                    if (dungeon_level != 9)
                    {
                        uimanager.UpdateCompass();
                    }
                }

                //Use accumulated damage to see if the dragons need to start cowering.
                if ((_RES != GAME_UW2) && (playerObject.AccumulatedDamage > 0))
                {
                    if (
                        (playerObject.AccumulatedDamage << 2 > max_hp)
                        ||
                        (play_hp < 0x10)
                        )
                    {
                        uimanager.StartDragonAnimation(2);
                    }
                }

                if (_RES != GAME_UW2)
                {
                    if ((dungeon_level == 9) && (playertimer > 0.2f)) //todo need to figure out the correct timing on this.
                    {
                        if ((Rng.r.Next(0x7FFF) & 0x1F) == 0)
                        {
                            Etherealvoid.EtherealVoidEndGameSpecialEffects();
                        }
                    }
                }

                playerObject.AccumulatedDamage = 0;//clear accumulated damage

                if (playertimer >= 1f)
                {//every second
                    if (ParalyseTimer > 0)
                    {
                        ParalyseTimer--;
                        Debug.Print($"Paralyse timer: {ParalyseTimer}");
                    }                   

                    var secondelasped = (int)(playertimer / 1);
                    playertimer = 0f;
                    for (int s = 0; s < secondelasped; s++)
                    {
                        secondcounter++;
                        //ClockValue += 0x40; //not sure what the exact rate should be here. for the moment assuming this is 1 second of time in game clock terms


                        //if ((ClockValue % 2048) < PreviousClockValue)//every 20 seconds
                        if (secondcounter >= 20)
                        {
                            secondcounter = 0;
                            playerUpdateCounter++;

                            UpdateLightStability(playerUpdateCounter);

                            for (int effectindex = 0; effectindex < 3; effectindex++)
                            {
                                if (effectindex < ActiveSpellEffectCount)
                                {
                                    var stability = GetEffectStability(effectindex);
                                    stability--;
                                    if (stability == 1)
                                    {
                                        var effectclass = playerdat.GetEffectClass(effectindex);
                                        var majorclass = effectclass & 0xF;
                                        var minorclass = effectclass >> 4;
                                        if ((majorclass == 1) && (((minorclass & 0x3F) == 3) || ((minorclass & 0x3F) == 5)))
                                        {
                                            var neweffectid = (2 << 4) + majorclass;
                                            //Fly or levitate.
                                            playerdat.SetSpellEffect(index: effectindex, effectid: neweffectid, stability: playerdat.GetEffectStability(effectindex));
                                        }
                                        else
                                        {
                                            playerdat.CancelEffect(effectindex);
                                        }
                                        effectindex = 0;//restart loop                                        
                                    }
                                    else
                                    {
                                        SetEffectStability(effectindex, stability);
                                    }
                                }
                            }

                            if (shrooms > 0)
                            {
                                shrooms--;
                            }

                            if (ManaRegenEnchantment)
                            {
                                //play_mana = Math.Min(play_mana + 1, max_mana);
                                ManaRegenChange(1);
                            }
                            if (HealthRegenEnchantment)
                            {
                                //play_hp = Math.Min(play_hp + 1, max_hp);
                                HPRegenerationChange(1);
                            }

                            if (playerdat.SwimCounter > 0x50)
                            {
                                SwimmingSkillCheck();
                            }


                            if (_RES == GAME_UW2)
                            {
                                if (GetQuest(50) == 1)
                                {//the keep is crashing
                                    if (GetQuest(54) == 0)//player has not returned after the crash.
                                    {
                                        special_effects.SpecialEffect(effecttype: 4, effectparam: 0x2C);
                                        if (!FreezeTimeEnchantment)
                                        {
                                            SetQuest(questno: 134, newValue: GetQuest(134) - 1);
                                            if (GetQuest(134) - 1 == 0)
                                            {
                                                killorn.KilornIsCrashing(false);
                                                //Apply raw damage to player in order to kill them for spending too much time in kilorn before it crashed
                                                damage.DamageObject(
                                                    objToDamage: playerObject,
                                                    basedamage: 0xFF,
                                                    damagetype: 0,
                                                    objList: UWTileMap.current_tilemap.LevelObjects,
                                                    WorldObject: true,
                                                    damagesource: 0);
                                            }
                                        }
                                    }
                                }
                            }

                            if (DreamingInVoid)
                            {
                                //check if dreaming in void and count down
                                //Debug.Print("Dreaming in void. count down dream plant value");
                                if (DreamPlantCounter > 0)
                                {
                                    DreamPlantCounter--;
                                    if (DreamPlantCounter == 0)
                                    {
                                        sleep.AwakenFromTheVoid();
                                    }
                                }
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
                                    //play_mana = Math.Min(play_mana + manaskillcheck, max_mana);
                                    ManaRegenChange(play_mana + manaskillcheck);
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
                                    a_create_object_trap.FindAndRunCreateObjectTraps();
                                }

                                Debug.Print("Do something with Npc hunger");

                                Debug.Print("Increment values at  bx+3a(fatigue), bx+3b(related to food health regen), bx+3c (unknown)");
                                play_fatigue = (byte)Math.Min(play_fatigue + 1, 0xFF);

                                foodhealthbonus = (byte)Math.Min(foodhealthbonus + 1, 0xFF);

                                var hpskillcheck = (int)SkillCheck(STR, 10);
                                if (hpskillcheck > 0)
                                {//regular health regen
                                    HPRegenerationChange(hpskillcheck);
                                }
                            }//end every 5 mins update
                            if ((playerUpdateCounter % 60) == 0)//every 20 mins
                            {
                                Debug.Print("TODO Update 'day' value");
                                Debug.Print("TODO something with scd.ark");
                                playerUpdateCounter = 0;
                            }//end every 20 mins update

                            PlayerStatusUpdate();
                        }//end every 20 seconds update
                        //PreviousClockValue = ClockValue % 2048;
                    }//end seconds for loop                 
                }  //every second  
                //check for player death.
                if (play_hp <= 0)
                {
                    Debug.Print("player has died");
                    PlayerDeath();
                }
            }//end ingame check
        }

        /// <summary>
        /// Regenerates HP
        /// </summary>
        /// <param name="regeneration"></param>
        public static void HPRegenerationChange(int regeneration)
        {
            if (regeneration >= 0)
            {//when positive apply a random bonus
                regeneration = 1 + (((Rng.r.Next(4) + regeneration) * max_hp) >> 4);
            }
            else
            {//when negative exact amount regen
                regeneration = -regeneration;
            }
            play_hp = Math.Min(play_hp + regeneration, max_hp);
        }

        public static void ManaRegenChange(int regeneration)
        {
            //check mana rules for the academy test.
            if (_RES == GAME_UW2)
            {
                if (worlds.GetWorldNo(dungeon_level) == 5)//Academy
                {
                    var academylevel = 1 + dungeon_level % 8;
                    if ((academylevel > 1) && (academylevel < 8))
                    {
                        return;
                    }
                    if (academylevel == 8)
                    {
                        if (playerObject.tileX < 25)
                        {
                            return;
                        }
                    }
                }
            }
            //TODO: see about similar logic for UW1 tybals lair

            //Apply mana boost
            if (regeneration < 0)
            {//boost mana by minus minus minor class. Not clear when this could happen...
                play_mana = Math.Min(play_mana - regeneration, max_mana);
            }
            else
            {
                var increase = 1 + ((max_mana * (regeneration + Rng.r.Next(0, 4))) >> 4); //This formula may be wrong and is restoring too much mana.
                play_mana = Math.Min(play_mana + increase, max_mana);
            }
        }



        // /// <summary>
        // /// Screenshakes and damage when killorn is crashing
        // /// </summary>
        // public static void KillornKeepEvent()
        // {
        //     //ovr135_250
        //     //Debug.Print("Killorn is crashing!!");
        //     special_effects.SpecialEffect(effecttype: 4, effectparam: 0x2C);
        // }

        /// <summary>
        /// Does a skill check every 20s when in water to see if damage is taken while swimming
        /// </summary>
        public static void SwimmingSkillCheck()
        {
            var checkvalue = 0;
            if (playerdat.WeightMax != 0)
            {
                checkvalue = (playerdat.WeightCarried << 5) / playerdat.WeightMax;
            }
            var result = SkillCheck(skillValue: playerdat.Swimming, targetValue: checkvalue, debug: true);
            if (result <= 0)
            {
                //fail or crit fail, dice roll and increase the swim counter.
                var roll = Rng.DiceRoll(NoOfLoops: (int)(3 - result), diceRange: 4);
                playerdat.SwimCounter += (byte)roll;  //this is what the vanilla code does but what will stop it from overflowing and stop applying damage.
            }
            if (playerdat.SwimCounter > 0x78)
            {
                result = SkillCheck(skillValue: playerdat.Swimming, targetValue: checkvalue, debug: true);
                if (result != SkillCheckResult.CritSucess)
                {
                    //apply damage
                    var damagerange = 2 - result;
                    if (_RES == GAME_UW2)
                    {
                        uimanager.FlashColour(colour: 0x50, targetControl: uimanager.CutsSmall);
                    }
                    else
                    {
                        uimanager.FlashColour(colour: 0xC6, targetControl: uimanager.CutsSmall);
                    }
                    damage.DamageObject(objToDamage: playerdat.playerObject, basedamage: Rng.DiceRoll(2, (int)(2 + damagerange)), damagetype: 0, objList: UWTileMap.current_tilemap.LevelObjects, WorldObject: true, damagesource: 0);
                }
            }
        }

        public static void UpdateLightStability(int updatecounter)
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
            StealthCalculationScoreQuietness = 13 - (Sneak / 3);
            StealthCalculationScoreVisibility = 15 - (Sneak / 5);

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
            DragonSkinBoots = false;
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
            }


            RefreshLighting();//either brightest physical light or brightest magical light

            //Handle game specific items
            if (_RES != GAME_UW2)
            {
                ApplyMazeNavigation();//handles tybals maze
            }
            else
            {
                FlyingInVoid();//handles flying in the ethereal void
            }


            if ((!AutomapEnabled) && (_RES == GAME_UW2))
            {
                //Do a test here to see if the player has entered a previously visible tile. If so renable automap, not sure where in the game the getting lost mechanic occurs but possibly caused by some teleportation effects.             
                if (UWTileMap.ValidTile(playerdat.playerObject.tileX, playerdat.playerObject.tileY))
                {
                    if (automap.automaps[dungeon_level - 1].tiles[playerObject.tileX, playerObject.tileY].visited)
                    {
                        Debug.Print("Visiting a previously discovered tile. You remember where you are and automap is reenabled");
                        AutomapEnabled = true;
                    }
                }
            }
            if (automap.CanMap(dungeon_level) && (AutomapEnabled))
            {
                UpdateAutomap();//update the visited status of nearby tiles
            }

            motion.RefreshPlayerTileState();

            motion.UpdateMotionStateAndSwimming(-1);
        }


        public static void PutWeaponAway()
        {
            if (playerdat.play_drawn == 1)
            {
                Debug.Print("put away weapon");
                playerdat.play_drawn = 0; //ensure weapon is not drawn.
                //XMIMusic.ChangeTheme(XMIMusic.PickLevelThemeMusic(0)); //in future this needs to take into account combat state.
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    uimanager.instance.InteractionButtonsUW2[(int)(uimanager.InteractionModes.ModeAttack)].Texture = uimanager.instance.UW2InteractionBtnsOff[(int)(uimanager.InteractionModes.ModeAttack)];
                }
                else
                {
                    uimanager.instance.InteractionButtonsUW1[(int)(uimanager.InteractionModes.ModeAttack)].Texture = uimanager.grLfti.LoadImageAt((int)(uimanager.InteractionModes.ModeAttack) * 2, false);
                }
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
            for (int slot = 0; slot < 10; slot++)
            {
                var objindex = uimanager.GetPaperDollObjAtSlot(slot);
                if (objindex != -1)
                {
                    var obj = InventoryObjects[objindex];
                    if (obj != null)
                    {
                        bool isValid = true;

                        if (!uimanager.ValidObjectForSlot(slot, obj))
                        {
                            isValid = false;
                        }

                        //filter for other scenarios that are not valid.  
                        //Rings not in the ring slot.     
                        if ((obj.majorclass == 0) && (obj.minorclass == 3))
                        {
                            if (_RES == GAME_UW2)
                            {
                                if (((obj.classindex >= 7) && (obj.classindex <= 0xA)) || obj.classindex == 5)//rings
                                {
                                    if ((slot != 9) && (slot != 10))
                                    {
                                        isValid = false;
                                    }
                                }
                            }
                            else
                            {
                                if (((obj.classindex >= 7) && (obj.classindex <= 0xA)) || obj.classindex == 6)//rings
                                {
                                    if ((slot != 9) && (slot != 10))
                                    {
                                        isValid = false;
                                    }
                                }
                            }
                        }


                        if ((slot >= 5) && (slot <= 8))
                        {
                            //shoulders and arms. do not cast effects from here.
                            // if (uimanager.DominantHandSlot == slot)
                            // {//Check for weapon in dominant hand. 
                            //     if (
                            //         !
                            //     (
                            //         ((obj.majorclass == 0) && (obj.minorclass == 0))
                            //         ||
                            //         ((obj.majorclass == 0 && obj.minorclass == 1) && (obj.classindex >= 8 && obj.classindex <= 10))
                            //     )
                            //     )
                            //     {
                            isValid = false;
                            //     }
                            // }
                        }



                        if ((slot == 4) && (_RES != GAME_UW2))
                        {
                            if (obj.item_id == 0x2F)//dragon skin boots
                            {
                                playerdat.DragonSkinBoots = true;
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
                                    PaperDollSlot: slot
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
            for (int i = 0; i < 4; i++)
            {
                var bit = (StealthBonus >> i) & 1;
                if (bit == 1)
                {//bit is set
                    switch (i)
                    {
                        case 1:
                            if (StealthCalculationScoreQuietness <= 0x10)
                            {
                                StealthCalculationScoreQuietness = 0;
                            }
                            else
                            {
                                StealthCalculationScoreQuietness = StealthCalculationScoreQuietness - 0x10;
                            }
                            break;
                        case 2:
                            if (StealthCalculationScoreVisibility <= 5)
                            {
                                StealthCalculationScoreVisibility = 0;
                            }
                            else
                            {
                                StealthCalculationScoreVisibility = StealthCalculationScoreVisibility - 5;
                            }
                            break;
                        case 3:
                            {
                                if (StealthCalculationScoreVisibility <= 0x10)
                                {
                                    StealthCalculationScoreVisibility = 0;
                                }
                                else
                                {
                                    StealthCalculationScoreVisibility = StealthCalculationScoreVisibility - 0x10;
                                }
                                break;
                            }
                    }
                }
            }
        }

        public static void RefreshLighting()
        {
            RenderingServer.GlobalShaderParameterSet("cutoffdistance", shade.GetViewingDistance(lightlevel));
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
                if (UWTileMap.ValidTile(playerObject.tileX, playerObject.tileY))
                {
                    int tileLightFlag = UWTileMap.current_tilemap.Tiles[playerObject.tileX, playerObject.tileY].lightFlag;
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
            return;
            //depending on light level. need to confirm if below math is okay
            NoOfTilesDiscovered = 0;
            var range = 1 + (lightlevel / 2);
            automap.MarkRangeOfTilesVisited(
                range: range,
                cX: playerObject.tileX,
                cY: playerObject.tileY,
                dungeon_level: dungeon_level
                );

            //This should cause an exp gain but the effect is limited because the automap is currently not doing the same map revealing as vanilla.
            if (NoOfTilesDiscovered > 0)
            {
                int gain = 0;
                if (_RES == GAME_UW2)
                {
                    var world = 1 + (dungeon_level / 8);
                    gain = (NoOfTilesDiscovered * world) / 0xA;
                }
                else
                {
                    gain = (NoOfTilesDiscovered * dungeon_level) / 0xA;
                }
                if (gain != 0)
                {
                    ChangeExperience(gain);
                }
            }
        }

        /// <summary>
        /// Changes the colour of the maze tiles in Tybals Lair in UW1 when the maze navigation crown is work
        /// </summary>
        static void ApplyMazeNavigation()
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
                        var material = tileMapRender.mapTexturesWalls.GetMaterial(52, UWTileMap.current_tilemap.texture_map);
                        material.SetShaderParameter("texture_albedo", (Texture)tileMapRender.mapTexturesFloors.LoadImageAt(222));
                    }
                    else
                    {
                        //remove effect
                        //set tiles to us texture 224
                        var material = tileMapRender.mapTexturesWalls.GetMaterial(52, UWTileMap.current_tilemap.texture_map);
                        material.SetShaderParameter("texture_albedo", (Texture)tileMapRender.mapTexturesFloors.LoadImageAt(224));
                    }
                }
            }
            previousMazeNavigation = MazeNavigation;
        }


        /// <summary>
        /// Forces fly mode when the player is dreaming in the void
        /// </summary>
        static void FlyingInVoid()
        {
            if (DreamPlantCounter != 0)
            {
                if (DreamingInVoid)
                {
                    MagicalMotionAbilities |= 0x10;   //Set bit 4 for flying
                }
            }
        }


        /// <summary>
        /// Calculates and sets the values used in CritterObject.Dat to represent how visible and quiet the player is.
        /// </summary>
        /// <param name="EasyMove"></param>
        public static void ApplyPlayerSneakScore(bool EasyMove = false)
        {
            var cl = playerdat.StealthCalculationScoreQuietness;
            if (EasyMove)
            {
                cl += 4;
            }
            else
            {
                if (motion.playerMotionParams.momentum_14 != 0)
                {
                    cl += ((motion.playerMotionParams.momentum_14 / 0xA) / motion.PlayerActualForwardSpeed_1_dseg_67d6_22A6) - 5;  //could this work out to be negative?
                }
                else
                {
                    cl = 0; //standing still?
                }
            }

            if (playerdat.TileState != 0)
            {
                cl += 4;
            }

            if (cl >= 0)
            {
                if (cl > 0xF)
                {
                    cl = 0xF;
                }
            }
            else
            {
                cl = 0;
            }

            if (critterObjectDat.StealthQuietness(127) <= cl)
            {
                playerdat.PlayerQuietness = cl;
            }
            else
            {
                if (SneakSoundCooldown_79D == 0)
                {
                    playerdat.PlayerQuietness--;
                }
            }

            SneakSoundCooldown_79D = (SneakSoundCooldown_79D + 1) % 8;
            playerdat.PlayerVisibility = playerdat.StealthCalculationScoreVisibility;
            //Debug.Print($"Stealth sound {PlayerQuietness} visibility {PlayerVisibility}");
        }

        public static void FootSteps(bool EasyMove = false)
        {
            if ((TileState & 0x1) != 0)
            {
                //lava
                //seg35_74E
                if (FootSteps_77A != 0xFF)
                {
                    if (FootStepTimerB_19E5 + 0x1800 <= main.GlobalPITTimer) //the globalpittimer is probably a bit slow leading to gaps in the sound.
                    {
                        //do something in seg016_1FFD(FootStepGlobal77A)
                        FootSteps_77A = 0xFF;
                    }
                }
                //seg35_784
                if (FootSteps_77A == 0xFF)
                {
                    FootStepTimerB_19E5 = main.GlobalPITTimer;
                    Debug.Print("playing Watersound");
                    UWsoundeffects.PlaySoundEffectAtAvatar(
                        effectno: 0,
                        pan: 0x40,
                        velocityOffset: 0);//plays the water edge sound
                    FootSteps_77A = 0;//always appears to be zero/;//unknown value Set from LoadBasicSound or Seg016_1FFD
                }

                if (_RES == GAME_UW2)
                {
                    //the following sound is uw2 only. water splashes
                    //seg035_7AE
                    dseg_79C = (dseg_79C + 1) % 0xF;
                    if (dseg_79C == 0)
                    {
                        WaterSoundPanIndex_dseg_79B = (uint)motion.SBB((int)WaterSoundPanIndex_dseg_79B);
                        if (motion.playerMotionParams.momentum_14 != 0)
                        {
                            if (main.GlobalPITTimer > FootStepTimerA_19DF)//note change of timer from B to A. This is in the original code..
                            {
                                UWsoundeffects.PlaySoundEffectAtAvatar(
                                    effectno: 0x1A,
                                    pan: FootstepSoundPanning[WaterSoundPanIndex_dseg_79B],
                                    velocityOffset: 0);
                            }
                        }
                    }
                }
            }
            else
            {
                //tilestate&1 == 0 (not in water)
                if (FootSteps_77A != 0xFF)
                {
                    //do something with seg016_1FFD // If I had to guess there probably needs to be handling to turn off the running sound effect of water?
                    FootSteps_77A = 0xFF;
                }
                if ((TileState & 0x8) == 0)
                {
                    int OnSnowOrIce = 0;
                    if (_RES == GAME_UW2)
                    {
                        if ((TileState & 0x4) == 0)
                        {
                            OnSnowOrIce = 0;
                        }
                        else
                        {
                            if (motion.ICYFloor_dseg_229E)
                            {
                                OnSnowOrIce = 2;
                            }
                            else
                            {
                                OnSnowOrIce = 1;
                            }
                        }
                    }

                    if ((motion.playerMotionParams.tilestate25 & 0x10) == 0)
                    {
                        if (EasyMove == false)
                        {
                            if (motion.playerMotionParams.momentum_14 > 0x2F)
                            {
                                if (main.GlobalPITTimer > FootStepTimerA_19DF)
                                {
                                    //seg035_8F0
                                    if (_RES == GAME_UW2)
                                    {
                                        UWsoundeffects.PlaySoundEffectAtAvatar(
                                            effectno: FootStepSoundEffectsUW2[FootstepSoundIndex_dseg_79B + (OnSnowOrIce << 1)],
                                            pan: FootstepSoundPanning[FootstepSoundIndex_dseg_79B],
                                            velocityOffset: (byte)(0xF + (motion.playerMotionParams.momentum_14 >> 5)));
                                    }
                                    else
                                    {
                                        //UW1 uses a toggle
                                        if (FootstepSoundIndex_dseg_79B == 0)
                                        {
                                            UWsoundeffects.PlaySoundEffectAtAvatar(
                                                effectno: 2,
                                                pan: 0x38,
                                                velocityOffset: (byte)(0xF + (motion.playerMotionParams.momentum_14 >> 5)));
                                        }
                                        else
                                        {
                                            UWsoundeffects.PlaySoundEffectAtAvatar(
                                                effectno: 1,
                                                pan: 0x48,
                                                velocityOffset: (byte)(0xF + (motion.playerMotionParams.momentum_14 >> 5)));
                                        }
                                    }
                                    //toggle to other foot
                                    FootstepSoundIndex_dseg_79B = (uint)motion.SBB((int)FootstepSoundIndex_dseg_79B);
                                    //set the time for the next footsteps.
                                    var nextinterval = 0x40 + (0x1770 / (1 + (motion.playerMotionParams.momentum_14 >> 2)));
                                    if (nextinterval > 0xC8)
                                    {
                                        nextinterval = 0xC8;
                                    }
                                    FootStepTimerA_19DF = (uint)(main.GlobalPITTimer + nextinterval);
                                }

                            }
                        }
                        else
                        {
                            Debug.Print("unimplemented easymove footstep sounds.");
                        }
                    }
                }
            }
        }

    }//end class
}//end namespace