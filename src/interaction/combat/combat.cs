using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    /// <summary>
    /// Class for combat calculations.
    /// </summary>
    public partial class combat : UWClass
    {
        public static int CombatHitTileX;
        public static int CombatHitTileY;
        public enum CombatStages
        {
            Ready = 0,
            Charging = 1,
            Release = 2,
            Swinging = 3,
            Striking = 4,
            Resetting = 5
        }
        public static uwObject currentweapon;  //if null then using fist

        public static int OnHitSpell = 0;

        public static int CurrentAttackSwingType = 0;

        public static int currentWeaponItemID
        {
            get
            {
                if (currentweapon == null)
                {
                    return 15;
                }
                else
                {
                    return currentweapon.item_id;
                }
            }
        }

        public static int CurrentWeaponBaseDamage(int attacktype)
        {
            if (currentweapon != null)
            {
                switch (attacktype)
                {
                    case 0://stab
                        return weaponObjectDat.stab(currentweapon.item_id);
                    case 1://slash
                        return weaponObjectDat.slash(currentweapon.item_id);
                    case 2://bash
                        return weaponObjectDat.bash(currentweapon.item_id);
                }
            }
            return 0;//no weapon
        }


        /// <summary>
        /// Gets the skill used for the current melee weapon
        /// </summary>
        public static int currentMeleeWeaponSkillNo
        {
            get
            {
                if (currentweapon == null)
                {
                    return 2;
                }
                else
                {
                    var tmp = weaponObjectDat.skill(currentweapon.item_id);
                    if (tmp >= 6)//check if a missile weapon or a fist
                    {
                        return 2; //return unarmed
                    }
                    return tmp;
                }
            }
        }

        public static CombatStages stage = 0;
        public static double combattimer = 0.0;

        /// <summary>
        /// tracks if a jewelled dagger is being used in order to ensure the listener in the sewers can be killed with it
        /// </summary>
        public static bool JeweledDagger = false;

        /// <summary>
        /// Item ID for Fist object
        /// </summary>
        const int fist = 15;

        public static int WeaponCharge = 0;
        public static int FinalAttackCharge = 0;

        public static int AttackAccuracy = 0;
        public static int AttackDamage = 0;
        public static int AttackScoreFlankingBonus = 0;
        public static bool AttackWasACrit = false;

        public static int BodyPartHit;


        /// <summary>
        /// Get how fast the charge builds up for the weapon
        /// </summary>
        static int ChargeSpeed
        {
            get
            {
                if (currentweapon == null)
                {
                    return weaponObjectDat.chargespeed(fist);
                }
                else
                {
                    return weaponObjectDat.chargespeed(currentweapon.item_id);
                }
            }
        }

        static int mincharge
        {
            get
            {
                if (currentweapon == null)
                {
                    return weaponObjectDat.mincharge(fist);
                }
                else
                {
                    return weaponObjectDat.mincharge(currentweapon.item_id);
                }
            }
        }


        static int maxcharge
        {
            get
            {
                if (currentweapon == null)
                {
                    return weaponObjectDat.maxcharge(fist);
                }
                else
                {
                    return weaponObjectDat.maxcharge(currentweapon.item_id);
                }
            }
        }

        /// <summary>
        /// Builds up the accumulated charge for the weapon
        /// </summary>
        /// <param name="delta"></param>
        public static void CombatChargingLoop(double delta)
        {
            switch (stage)
            {
                case CombatStages.Ready:
                    stage = CombatStages.Charging; //begin charging. start weapon swing pull back anim
                    IncreaseCharge(delta);
                    break;
                case CombatStages.Charging: //building up charge
                    IncreaseCharge(delta);
                    break;
            }
        }

        /// <summary>
        /// Increases the charge by weapon speed score every 16 units of a timer
        /// </summary>
        /// <param name="delta"></param>
        private static void IncreaseCharge(double delta)
        {
            combattimer += delta;
            if (combattimer > 0.0625) // = should be every 16 units between a previuosly stored timer in vanilla but I'm unsure what time units they are. assuming 1 second. Feels a bit fast
            {
                WeaponCharge = Math.Min(WeaponCharge + ChargeSpeed, 100);
                var frame = 1 + (WeaponCharge / 12);
                //Debug.Print($"{frame} at {WeaponCharge}");
                uimanager.ChangePower(frame);
                combattimer = 0f;
            }
        }

        /// <summary>
        /// Ends the combat attack
        /// </summary>
        public static void EndCombatLoop()
        {
            WeaponCharge = 0;
            stage = CombatStages.Ready;
            uimanager.ResetPower();
            combattimer = 0;
        }


        /// <summary>
        /// Identifies what type of weapon is in use
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns>0= not a weapon/using fist, 1 = melee,  2= ranged</returns>
        public static int isWeapon(uwObject weapon)
        {
            if (weapon == null)
            {
                return 0;
            }
            if (weapon.majorclass == 0)
            {
                switch (weapon.minorclass)
                {
                    case 0:
                        return 1;
                    case 1:
                        {
                            switch (weapon.classindex)
                            {
                                case 8://sling
                                case 9://bow
                                case 10://crossbow
                                case 15://jeweled bow
                                    return 2;
                            }
                            break;
                        }
                }
            }
            return 0;
        }


        /// <summary>
        /// Processes the various stages of combat
        /// </summary>
        /// <param name="delta"></param>
        public static void CombatInputHandler(double delta)
        {
            if (uimanager.InteractionMode == uimanager.InteractionModes.ModeAttack)
            {
                if ((playerdat.ObjectInHand != -1)
                || (useon.CurrentItemBeingUsed != null)
                || (SpellCasting.currentSpell != null)
                || (main.blockmouseinput))
                {
                    return;
                }
                bool MouseHeldDown = Input.IsMouseButtonPressed(MouseButton.Right);
                switch (stage)
                {
                    case CombatStages.Ready:
                        if (MouseHeldDown)
                        {
                            OnHitSpell = 0;
                            JeweledDagger = false;
                            AttackAccuracy = 0;
                            AttackDamage = 0;
                            AttackScoreFlankingBonus = 0;
                            AttackWasACrit = false;
                            //currentAnimationStrikeType = WeaponAnimStrikeOffset;
                            CurrentAttackSwingType = Rng.r.Next(0, 4);
                            stage = CombatStages.Charging;
                        }
                        else
                        {
                            //return to normal
                            uimanager.currentWeaponAnim = WeaponAnimGroup + WeaponAnimHandednessOffset + 6;
                        }
                        break;
                    case CombatStages.Charging:
                        {
                            if (MouseHeldDown)
                            {
                                CombatChargingLoop(delta);
                                uimanager.currentWeaponAnim = WeaponAnimGroup + WeaponAnimStrikeOffset + WeaponAnimHandednessOffset;
                            }
                            else
                            {
                                stage = CombatStages.Release;
                            }
                            break;
                        }
                    case CombatStages.Release:
                        {
                            if (uimanager.IsMouseInViewPort())
                            {
                                if (WeaponCharge >= mincharge)
                                {
                                    //start return swing   swing                            
                                    Debug.Print($"Releasing attack at charge {WeaponCharge}");
                                    stage = CombatStages.Swinging;
                                    combattimer = 0;
                                    uimanager.currentWeaponAnim = WeaponAnimGroup + WeaponAnimStrikeOffset + WeaponAnimHandednessOffset + 1;
                                }
                                else
                                {//cancel. not enough charge built up
                                    stage = CombatStages.Resetting;
                                }
                            }
                            else
                            {
                                Debug.Print("Swing outside the window. Cancelling");
                                EndCombatLoop();//don't swing when outside the window
                            }
                            break;
                        }
                    case CombatStages.Swinging:
                        {
                            //repeat until swing anim sequence is completed. then go to strike
                            combattimer += delta;
                            if (combattimer >= 0.6f)
                            {
                                Debug.Print("Swing completed");
                                stage = CombatStages.Striking;
                            }
                            break;
                        }
                    case CombatStages.Striking:
                        {
                            //weapon has struck do combat calcs  (if melee) 
                            ProcessAttackHit();

                            //when done start reset
                            stage = CombatStages.Resetting;
                            combattimer = 0;
                            break;
                        }
                    case CombatStages.Resetting:
                        {
                            //do weapon put away anim until time   
                            combattimer += delta;
                            if (combattimer >= 0.2f)
                            {
                                uimanager.currentWeaponAnim = WeaponAnimGroup + WeaponAnimHandednessOffset + 6;
                                Debug.Print("Resetting");
                                EndCombatLoop();//resetting. when done return to ready    
                            }
                            break;
                        }
                }
            }
            else
            {// check if we need to reset
                if (stage != CombatStages.Ready)
                {
                    EndCombatLoop();
                }
            }
        }


        /// <summary>
        /// checks for target hit and apply combat calcs
        /// </summary>
        /// <returns></returns>
        static bool ProcessAttackHit()
        {
            var position = mouseCursor.CursorPosition;
            if (!uimanager.IsMouseInViewPort())
            {
                position = mouseCursor.CursorPositionSub;//use mouselook position
            }
            var result = uimanager.DoRayCast(position, 2f, out Vector3 rayorigin);
            if (result != null)
            {
                if (result.ContainsKey("collider") && result.ContainsKey("normal") && result.ContainsKey("position"))
                {
                    var obj = (StaticBody3D)result["collider"];
                    var normal = (Vector3)result["normal"];
                    var hitCoordinateEnd = (Vector3)result["position"];
                    var hitCoordinate = rayorigin.Lerp(hitCoordinateEnd, 0.9f);

                    Debug.Print(obj.Name);
                    string[] vals = obj.Name.ToString().Split("_");
                    switch (vals[0].ToUpper())
                    {
                        case "TILE":
                        case "WALL":
                        case "CEILING"://hit a wall/surface
                            Debug.Print("hit a wall. do selfdamage to the weapon");
                            animo.SpawnAnimoAtPoint(0xB, hitCoordinate);
                            return false;
                        default:
                            if (int.TryParse(vals[0], out int index))
                            {
                                var hitobject = UWTileMap.current_tilemap.LevelObjects[index];
                                if (hitobject != null)
                                {
                                    Debug.Print($"{hitobject.a_name}");
                                    return PlayerHitsUWObject(hitobject, hitCoordinate);
                                }
                            }
                            break;
                    }
                }
            }
            return false; //miss attack
        }


        /// <summary>
        /// Do the attack calcs for the player hitting an object
        /// </summary>
        /// <param name="objHit"></param>
        /// <returns></returns>
        static bool PlayerHitsUWObject(uwObject objHit, Godot.Vector3 hitCoordinate)
        {
            //calc final attack charge based on the % of charge built up in the weapon
            FinalAttackCharge = mincharge + ((maxcharge - mincharge) * WeaponCharge) / 100; //this is kept later for damage calcs.

            //do attack calcs
            CalcPlayerAttackScores();

            //execute attack
            if (PlayerExecuteAttack(objHit, hitCoordinate))
            {
                if (_RES == GAME_UW2)
                {
                    int currWeaponType = 0;
                    if (currentweapon != null)
                    {
                        currWeaponType = isWeapon(currentweapon);
                    }
                    //post apply spell effect if applicable
                    switch (OnHitSpell)
                    {
                        case 1:
                            {
                                //Debug.Print("Lifestealer");
                                if (currWeaponType > 0)
                                {
                                    playerdat.HPRegenerationChange(-AttackDamage);
                                }
                                break;
                            }
                        case 2:
                            {//Debug.Print("Undeadbane"); 
                                if (currWeaponType > 0)
                                {
                                    SpellCasting.SmiteUndead(objHit.index, UWTileMap.current_tilemap.LevelObjects, hitCoordinate, playerdat.playerObject);
                                }                                
                                break;
                            }
                        case 3: 
                            {
                                //Debug.Print("Firedoom"); 
                                animo.SpawnAnimoAtPoint(2, hitCoordinate);//explosion
                                //Do damage in area of tile.
                                damage.DamageObjectsInTile(objHit.tileX, objHit.tileY, 0, 1);                                
                                break;
                            }
                        case 4:
                            {
                                //Debug.Print("stonestrike"); 
                                SpellCasting.Paralyse(objHit.index, UWTileMap.current_tilemap.LevelObjects, playerdat.playerObject);
                                break;
                            }
                        case 5:
                        case 6: 
                            {
                                if (objHit.OneF0Class == 0x14)
                                {
                                    //is door
                                    SpellCasting.Unlock(objHit.index, UWTileMap.current_tilemap.LevelObjects);
                                }
                            }
                        Debug.Print("Entry"); break;
                        //case 7: Debug.Print("unknownspecial 7"); break;
                        //case 8: Debug.Print("unknownspecial 8"); break;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Calculates the player attack accuracy and base damage scores
        /// </summary>
        static void CalcPlayerAttackScores()
        {
            var weapon = currentweapon;
            var weaponskill = playerdat.GetSkillValue(currentMeleeWeaponSkillNo);

            if (_RES == GAME_UW2)
            {
                if (currentWeaponItemID == 10)
                {
                    JeweledDagger = true;
                }
            }

            AttackAccuracy = weaponskill + (playerdat.Attack >> 1) + (playerdat.DEX / 7) + playerdat.ValourBonus;
            if (playerdat.difficuly == 1)
            {   //easy dificulty
                AttackAccuracy += 7;
            }


            //base damage calcs
            CalcBasicWeaponDamage();
            CalcAttackEnchantment();
            Debug.Print($"Final scores accuracy {AttackAccuracy} basedamage {AttackDamage}");
        }

        /// <summary>
        /// Gets the damage and accuracy bonuses for the attack
        /// </summary>
        private static void CalcAttackEnchantment()
        {
            //Then get weapon enchantments
            if (currentweapon != null)
            {
                var enchant = MagicEnchantment.GetSpellEnchantment(currentweapon, playerdat.InventoryObjects);
                if (enchant != null)
                {
                    if (!enchant.IsFlagBit2Set)
                    {
                        Debug.Print($"Weapon has enchantment {enchant.NameEnchantment(currentweapon, playerdat.InventoryObjects, 3)} {enchant.SpellMajorClass} {enchant.SpellMinorClass}");
                        if (enchant.SpellMajorClass == 12)
                        {//accuracy or damage bonuses
                            switch (_RES)
                            {
                                case GAME_UW2:
                                    {
                                        if (enchant.SpellMinorClass <= 7)
                                        {
                                            Debug.Print("check me. Possibly bugged enchantment behaviour in uw2 where attack and accuracy are the wrong way around!");
                                            if (enchant.SpellMinorClass < 4)
                                            {//this is possibly a bug in uw2 since the accuracy enchantments come to here.
                                                AttackDamage += (1 + enchant.SpellMinorClass << 1);
                                            }
                                            else
                                            {
                                                AttackAccuracy += ((enchant.SpellMinorClass << 1) - 7);
                                            }
                                        }
                                        else
                                        {
                                            OnHitSpell = enchant.SpellMinorClass - 7;//eg lifestealer, firedoom, stone strike door unlocking
                                            if (OnHitSpell == 8)
                                            {//unknown special spell.
                                                AttackDamage += 5;
                                            }
                                        }
                                        break;
                                    }
                                default:
                                    {

                                        if ((enchant.SpellMinorClass & 8) == 0)
                                        {
                                            AttackAccuracy += (1 + enchant.SpellMinorClass & 0x7);
                                        }
                                        else
                                        {
                                            AttackDamage += (1 + enchant.SpellMinorClass & 0x7);
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialises the basic damage for a weapon attack
        /// </summary>
        private static void CalcBasicWeaponDamage()
        {
            if (currentMeleeWeaponSkillNo == 2)
            {
                //do unarmed calcs for base damage
                AttackDamage = 4 + (playerdat.STR / 6) + (playerdat.Unarmed << 1) / 5;
            }
            else
            {
                //var attacktype = Rng.r.Next(0, 3);
                //do weapon based calcs, using a random attack type now.
                AttackDamage = (playerdat.STR / 9) + CurrentWeaponBaseDamage(CurrentAttackSwingType);
            }
        }

        static bool PlayerExecuteAttack(uwObject critter, Godot.Vector3 hitCoordinate)
        {
            if (checkAttackHit())
            {
                if (CalcAttackResults(critter) == 0)
                {//attack roll has hit
                    AttackScoreFlankingBonus = CalcFlankingBonus(critter);
                    Debug.Print("Hit");
                    AttackerAppliesFinalDamage(
                        objHit: critter,
                        damageType: 4,                       
                        MissileAttack: false);
                }
                else
                {//attack roll has missed
                    Debug.Print("Miss");
                }
                return true;
            }
            else
            {
                return false;//swing and a miss
            }
        }


        /// <summary>
        /// Finalises the attack results
        /// </summary>
        /// <param name="objHit"></param>
        /// <returns>0 if attack hit</returns>
        static int CalcAttackResults(uwObject objHit)
        {
            if (objHit.majorclass == 1)
            {//npc
                var defencescore = critterObjectDat.defence(objHit.item_id);
                var attackscore = AttackAccuracy + AttackScoreFlankingBonus;
                var result = playerdat.SkillCheck(attackscore, defencescore);
                if (playerdat.PoisonedWeapon)
                {
                    if (checkforPoisonableWeapon())
                    {
                        if (critterObjectDat.maybepoisonvulnerability(objHit.item_id) != 0)
                        {
                            AttackDamage += ((playerdat.Casting + 30) / 40);
                        }
                    }
                }
                AttackWasACrit = false;
                if (result == playerdat.SkillCheckResult.CritSucess)
                {
                    AttackWasACrit = true;
                    var critbonus = (48 + Rng.r.Next(30)) >> 5;
                    AttackDamage = AttackDamage * critbonus;
                    return 0;
                }
                else
                {
                    if (result == playerdat.SkillCheckResult.CritFail)
                    {
                        if (critterObjectDat.damagesWeaponOnCritMiss(objHit.item_id))
                        {
                            var weaponselfdamage = Rng.DiceRoll(2, 3);
                            Debug.Print($"Damage primary weapon by {weaponselfdamage}");
                        }
                    }
                    return 1 - (int)result;

                }
            }
            else
            {
                //did not hit an npc                
                if (objHit.OneF0Class == 0x14)
                {//doors
                    var hitroll = Rng.r.Next(0, 0xC);
                    var checkvalue = (objHit.item_id & 0x7) << 1;
                    if (hitroll < checkvalue)
                    {
                        var equipdam = Rng.DiceRoll(2, 4);
                        Debug.Print($"Do weapon self damage of {equipdam}");
                    }
                    return 0; //0 is a hit!
                }
                else
                {
                    return 0;
                }
            }
        }


        static int AttackerAppliesFinalDamage(uwObject objHit, int damageType, bool MissileAttack = false)
        {
            if (AttackDamage < 2)
            {
                AttackDamage = 2;
            }
            var damagequotient = AttackDamage / 6;
            var damageremainder = AttackDamage % 6;
            AttackDamage = 0;
            if (damagequotient != 0)
            {
                AttackDamage = Rng.DiceRoll(damagequotient, 6);
            }
            if (damageremainder != 0)
            {
                AttackDamage += Rng.DiceRoll(1, damageremainder);
            }

            var finaldamage = (AttackDamage * FinalAttackCharge) >> 7;
            finaldamage += AttackScoreFlankingBonus;

            //TODO figure out correct sounds
            Debug.Print("Player Weapon Hit sound");
            if (!MissileAttack)
            {
                //Do blood spatters.
                Debug.Print("Spatter blood");

                // if (objHit.majorclass == 1)
                // {
                //     //********************//
                //     Debug.Print("Force critter hostile for debug purposes");
                //     objHit.npc_attitude = 0;
                //     //********************//
                //     if (critterObjectDat.bleed(objHit.item_id) != 0)
                //     {
                //         animo.SpawnAnimoAtPoint(0, hitCoordinate); //blood
                //         if (AttackWasACrit)
                //         {
                //             animo.SpawnAnimoAtPoint(0, hitCoordinate + (Vector3.Up * 0.12f)); //blood
                //         }
                //     }
                //     else
                //     {//npc does not bleed
                //         animo.SpawnAnimoAtPoint(0xB, hitCoordinate);// a flash damage
                //     }
                // }
                // else
                // {//hit a non-npc object
                //     animo.SpawnAnimoAtPoint(0xB, hitCoordinate);// a flash/damage
                // }
            }

            if (objHit.majorclass == 1)
            {//npc has been hit, apply defenses
                var bodypartindex = BodyPartHit / 4;
                int cx = (int)critterObjectDat.toughness(objHit.item_id, bodypartindex);
                if (cx == -1)
                {
                    BodyPartHit = BodyPartHit & 4;
                    cx = critterObjectDat.toughness(objHit.item_id, 0);
                }
                if (objHit.IsPowerfull == 1)
                {
                    cx = (cx * 5) / 3;
                }

                if (cx >= finaldamage)
                {
                    finaldamage = 0;
                }
                else
                {
                    finaldamage -= cx;
                }

                var di = finaldamage / 4;
                if (di <= 3)
                {
                    di = 3;//this is used for animo later on?
                }
            }
            //apply damage
            damage.DamageObject(
                objToDamage: objHit,
                basedamage: finaldamage,
                damagetype: damageType,
                objList: UWTileMap.current_tilemap.LevelObjects,
                WorldObject: true,
                damagesource: 1,
                hitCoordinate: Vector3.Zero, ignoreVector: true);
            return 0;
        }

        /// <summary>
        /// Only certain weapons can use a poison enchantment. for the moment return true here.
        /// </summary>
        /// <returns></returns>
        static bool checkforPoisonableWeapon()
        {
            return true;
        }

        /// <summary>
        /// Gets a damage bonus based on the relative headings of the attacker and defender.
        /// </summary>
        static int CalcFlankingBonus(uwObject critter)
        {
            if (critter.majorclass == 1)
            {
                var defenderHeading = critter.heading;
                var attackerHeading = playerdat.heading >> 4;
                return CalcFlankingBonus(defenderHeading, attackerHeading);
            }
            else
            {
                return 0;
            }
        }

        static int CalcFlankingBonus(int defenderHeading, int attackerHeading)
        {
            var bonus = defenderHeading + 0xC - attackerHeading;
            bonus = bonus & 0x7;
            if (bonus > 4)
            {
                bonus = 8 - bonus;
            }
            return bonus;
        }

        /// <summary>
        /// Checks for a hit within the weapon radius+3 and gets the body positions where the target has been hit
        /// In my implementation an attack hit is currently handled by the raycast. 
        /// But I will still need to calculate the body part hit.
        /// </summary>
        /// <returns></returns>
        static bool checkAttackHit()
        {
            BodyPartHit = Rng.r.Next(0, 4);
            return true;
        }


        /// <summary>
        /// Offset into animation frams for the weapon type
        /// </summary>
        /// <returns></returns>
        static short WeaponAnimGroup
        {
            get
            {
                switch (currentMeleeWeaponSkillNo)
                {
                    case 3:
                        return 0;
                    case 4:
                        return 7;
                    case 5:
                        return 14;
                    default:
                        return 21;
                }
            }
        }

        static short WeaponAnimHandednessOffset
        {
            get
            {
                if (playerdat.isLefty)
                {
                    return 28;
                }
                else
                {
                    return 0;
                }
            }
        }

        static short WeaponAnimStrikeOffset
        {
            get
            {
                // if (!UWCharacter.Instance.MouseLookEnabled)
                // {
                //     if (Camera.main.ScreenToViewportPoint(Input.mousePosition).y > 0.666f)
                //     {
                //         return 2;//bash
                //     }
                //     else if (Camera.main.ScreenToViewportPoint(Input.mousePosition).y > 0.333f)
                //     {
                //         return 0;//Slash
                //     }
                //     else
                //     {
                //         return 4;//stab
                //     }
                // }
                // else
                // {
                switch (CurrentAttackSwingType) //random for now
                {
                    case 1:
                        return 2; //bash
                    case 2:
                        return 4;//stab
                    case 3:
                    default:
                        return 0; //slash
                }
                //}
            }
        }



    }//end class
}//end namespace