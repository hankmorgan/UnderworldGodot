using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Class for combat calculations.
    /// </summary>
    public partial class combat : UWClass
    {


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
        /// checks for target hit and apply combat calcs
        /// </summary>
        /// <returns></returns>
        // static bool ProcessAttackHit_DEPRECIATED()
        // {
        //     Debug.Print("DEPRECIATED");
        //     var position = mouseCursor.CursorPosition;
        //     if (!uimanager.IsMouseInViewPort())
        //     {
        //         position = mouseCursor.CursorPositionSub;//use mouselook position
        //     }
        //     var result = uimanager.DoRayCast(position, 2f, out Vector3 rayorigin);
        //     if (result != null)
        //     {
        //         if (result.ContainsKey("collider") && result.ContainsKey("normal") && result.ContainsKey("position"))
        //         {
        //             var obj = (StaticBody3D)result["collider"];
        //             var normal = (Vector3)result["normal"];
        //             var hitCoordinateEnd = (Vector3)result["position"];
        //             var hitCoordinate = rayorigin.Lerp(hitCoordinateEnd, 0.9f);

        //             Debug.Print(obj.Name);
        //             string[] vals = obj.Name.ToString().Split("_");
        //             switch (vals[0].ToUpper())
        //             {
        //                 case "TILE":
        //                 case "WALL":
        //                 case "CEILING"://hit a wall/surface
        //                     Debug.Print("hit a wall. do selfdamage to the weapon");
        //                     short xpos, ypos, zpos;
        //                     int tileX, tileY;
        //                     animo.PointToXYZ(point: hitCoordinate, xpos: out xpos, ypos: out ypos, zpos: out zpos, tileX: out tileX, tileY: out tileY);
        //                     animo.SpawnAnimoInTile(subclassindex: 0xB, xpos: xpos, ypos: ypos, zpos: (short)(zpos - 3), tileX: tileX, tileY: tileY);
        //                     return false;
        //                 default:
        //                     if (int.TryParse(vals[0], out int index))
        //                     {
        //                         var hitobject = UWTileMap.current_tilemap.LevelObjects[index];
        //                         if (hitobject != null)
        //                         {
        //                             Debug.Print($"{hitobject.a_name}");
        //                             return false;
        //                            // return PlayerHitsUWObject_DEPRECIATED(hitobject);
        //                         }
        //                     }
        //                     break;
        //             }
        //         }
        //     }
        //     return false; //miss attack
        // }


        /// <summary>
        /// Do the attack calcs for the player hitting an object
        /// </summary>
        /// <param name="defender"></param>
        /// <returns></returns>
        // static bool PlayerHitsUWObject_DEPRECIATED(uwObject defender)
        // {
        //     Debug.Print("DEPRECIATED");
        //     //calc final attack charge based on the % of charge built up in the weapon
        //     FinalAttackCharge = mincharge + ((maxcharge - mincharge) * WeaponCharge) / 100; //this is kept later for damage calcs.

        //     //do attack calcs
        //     CalcPlayerAttackScores();

        //     //execute attack
        //     if (ExecuteAttack(attacker: playerdat.playerObject))
        //     {
        //         if (_RES == GAME_UW2)
        //         {
        //             int currWeaponType = 0;
        //             if (currentweapon != null)
        //             {
        //                 currWeaponType = isWeapon(currentweapon);
        //             }
        //             //post apply spell effect if applicable
        //             switch (OnHitSpell)
        //             {
        //                 case 1:
        //                     {
        //                         //Debug.Print("Lifestealer");
        //                         if (currWeaponType > 0)
        //                         {
        //                             playerdat.HPRegenerationChange(-AttackDamage);
        //                         }
        //                         break;
        //                     }
        //                 case 2:
        //                     {//Debug.Print("Undeadbane"); 
        //                         if (currWeaponType > 0)
        //                         {
        //                             SpellCasting.SmiteUndead(defender.index, UWTileMap.current_tilemap.LevelObjects, playerdat.playerObject);
        //                         }                                
        //                         break;
        //                     }
        //                 case 3: 
        //                     {
        //                         //Debug.Print("Firedoom"); 
        //                         //explosion
        //                         var tile = UWTileMap.current_tilemap.Tiles[defender.tileX,defender.tileY];
        //                         var height = tile.floorHeight<<3;
        //                         if ( height< 0x80)
        //                         {
        //                             height = height + Rng.r.Next(0x80 - height);
        //                         }
        //                         animo.SpawnAnimoInTile(subclassindex: 2, xpos: 3, ypos: 3, zpos: (short)height, tileX: defender.tileX, tileY: defender.tileY);
        //                         //Do damage in area of tile.
        //                         damage.DamageObjectsInTile(defender.tileX, defender.tileY, 0, 1);                                
        //                         break;
        //                     }
        //                 case 4:
        //                     {
        //                         //Debug.Print("stonestrike"); 
        //                         SpellCasting.Paralyse(defender.index, UWTileMap.current_tilemap.LevelObjects, playerdat.playerObject);
        //                         break;
        //                     }
        //                 case 5:
        //                 case 6: 
        //                     {
        //                         if (defender.OneF0Class == 0x14)
        //                         {
        //                             //is door
        //                             SpellCasting.Unlock(defender.index, UWTileMap.current_tilemap.LevelObjects);
        //                         }
        //                     }
        //                 Debug.Print("Entry"); break;
        //                 //case 7: Debug.Print("unknownspecial 7"); break;
        //                 //case 8: Debug.Print("unknownspecial 8"); break;
        //             }
        //         }
        //     }

        //     return true;
        // }

        /// <summary>
        /// Calculates the player attack accuracy and base damage scores
        /// </summary>
        static void CalculatePlayerAttackScores()
        {
            CurrentWeaponRadius = commonObjDat.radius(currentWeaponItemID);
            var weaponskill = playerdat.GetSkillValue(currentMeleeWeaponSkillNo);

            if (_RES == GAME_UW2)
            {
                if (currentWeaponItemID == 10)
                {
                    JeweledDagger = true;//Used when killing the listener in the castle.
                }
            }

            AttackScore = weaponskill + (playerdat.Attack >> 1) + (playerdat.DEX / 7) + playerdat.ValourBonus;
            if (playerdat.difficuly == 1)
            {   //easy dificulty
                AttackScore += 7;
            }

            //base damage calcs
            if (currentMeleeWeaponSkillNo == 2)
            {
                //do unarmed calcs for base damage
                AttackDamage = 4 + (playerdat.STR / 6) + (playerdat.Unarmed << 1) / 5;
            }
            else
            {
                //Adjust damage by player str and swing type damage.
                AttackDamage = (playerdat.STR / 9) + CurrentWeaponBaseDamage(CurrentAttackSwingType);
            }

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
                                            {
                                                //this is possibly a bug in uw2 since the accuracy enchantments come to here.
                                                AttackDamage += (1 + enchant.SpellMinorClass << 1);
                                            }
                                            else
                                            {
                                                AttackScore += ((enchant.SpellMinorClass << 1) - 7);
                                            }
                                        }
                                        else
                                        {
                                            OnHitSpell = enchant.SpellMinorClass - 7;//eg lifestealer, firedoom, stone strike door unlocking
                                            if (OnHitSpell == 8)
                                            {
                                                //unknown special spell.
                                                AttackDamage += 5;
                                            }
                                        }
                                        break;
                                    }
                                default:
                                    {

                                        if ((enchant.SpellMinorClass & 8) == 0)
                                        {
                                            AttackScore += (1 + enchant.SpellMinorClass & 0x7);
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
            Debug.Print($"Final scores accuracy {AttackScore} basedamage {AttackDamage}");
        }


        /// <summary>
        /// Starts an NPC attack.
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="swingtype"></param>
        /// <param name="attackcharge"></param>
        /// <param name="attacktype"></param>
        /// <param name="poisondamage"></param>
        /// <returns></returns>
        public static bool NPCExecuteAttack(uwObject attacker, int swingtype, int attackcharge, int attacktype, int poisondamage)
        {
            CurrentAttackSwingType = swingtype;
            FinalAttackCharge = attackcharge;
            CurrentWeaponRadius = 2;//always his for NPCS.
            AttackingCharacter = attacker;
            AttackDamage = critterObjectDat.attackdamage(attacker.item_id, attacktype) + (critterObjectDat.strength(attacker.item_id) / 5);
            AttackScore = critterObjectDat.chancetohit(attacker.item_id, attacktype) + (critterObjectDat.EquipmentDamageOrBaseHitChance(attacker.item_id) >> 1);

            if (attacker.IsPowerfull == 1)
            {
                AttackScore += 7 + Rng.r.Next(6);
                AttackDamage += 4 + Rng.r.Next(0xC);
            }
            var attackresult = ExecuteAttack(attacker);
            if (attackresult)
            {
                if (DefendingCharacter.index == 1)
                {
                    if (playerdat.play_poison < poisondamage)
                    {
                        Debug.Print("apply poison to player");
                    }
                }
            }

            return attackresult;
        }

        /// <summary>
        /// Process a melee combat attack executed by either the player or an NPC.
        /// </summary>
        /// <param name="attacker"></param>
        /// <returns></returns>
        public static bool ExecuteAttack(uwObject attacker)
        {
            AttackingCharacter = attacker;
            DefendingCharacter = null;
            if (checkAttackHit())
            {
                //var defender = UWTileMap.current_tilemap.LevelObjects[DefenderIndex];
                if ((AttackingCharacter.index != 1) && (DefendingCharacter.index != 1) && (!DefendingCharacter.IsStatic))
                {
                    if (attacker.IsAlly == DefendingCharacter.IsAlly)
                    {
                        return false;
                    }
                }

                AttackScoreFlankingBonus = CalcFlankingBonus();
                if (CalcAttackResults() == 0)
                {
                    //A hit
                    AttackerAppliesFinalDamage(damageType: 4, attacker: AttackingCharacter.index, MissileAttack: false);
                    return true;
                }
                else
                {
                    // a miss
                    damage.DamageObject(
                        objToDamage: DefendingCharacter, 
                        basedamage: 0, 
                        damagetype: 4, 
                        objList: UWTileMap.current_tilemap.LevelObjects, 
                        WorldObject: true, 
                        damagesource: AttackingCharacter.index);
                    Debug.Print("Todo CombatMissImpactSound();");
                    return false;
                }
            }
            else
            {
                return false;//swing and a miss
            }
        }


        /// <summary>
        /// Finalises the attack results. Updates AttackDamage value and applies equipment damage on misses, critical hits to player and strikes on doors
        /// </summary>
        /// <param name="objHit"></param>
        /// <returns>0 if attack hit</returns>
        static int CalcAttackResults()
        {
            if (DefendingCharacter.majorclass == 1)
            {
                //npc
                if (DefendingCharacter.index == 1)
                {
                    //defender is the player. apply locational protections
                    AttackScore -= playerdat.LocationalProtectionValues[BodyPartHit];
                }

                var result = playerdat.SkillCheck(skillValue: AttackScore + AttackScoreFlankingBonus, targetValue: critterObjectDat.defence(DefendingCharacter.item_id));
                if (playerdat.PoisonedWeapon)
                {
                    if (checkforPoisonableWeapon())
                    {
                        if (critterObjectDat.PoisonVulnerability(DefendingCharacter.item_id) != 0)
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

                    if (DefendingCharacter.index == 1)
                    {
                        //a critical hit has landed on the player. Damage equipment
                        uimanager.FlashColour(0x23, uimanager.Cuts3DWin, 0.1f);
                        var si_slot = (BodyPartHit + 1) & 0x3;
                        if (si_slot == 3)
                        {
                            if (Rng.r.Next(5) == 0)
                            {
                                si_slot++;
                            }
                        }
                        else
                        {
                            if (si_slot <= 2)
                            {
                                si_slot = 7 + playerdat.handednessvalue;
                            }
                        }
                        Debug.Print($"Equipment damage to slot {si_slot} of {Rng.DiceRoll(2, 4)}");
                    }

                    return 0;//a hit
                }
                else
                {
                    if (result == playerdat.SkillCheckResult.CritFail)
                    {
                        if (AttackingCharacter.index == 1)
                        {
                            if (critterObjectDat.damagesWeaponOnCritMiss(DefendingCharacter.item_id))
                            {
                                var weaponselfdamage = Rng.DiceRoll(2, 3);
                                var si_slot = 8 - playerdat.handednessvalue;
                                Debug.Print($"Equipment damage to slot {si_slot} of {Rng.DiceRoll(2, 3)}");
                            }
                        }

                    }
                    return 1 - (int)result;
                }
            }
            else
            {
                // //did not hit an npc                

                if (AttackingCharacter.index == 1)
                {
                    //attack is player
                    if (DefendingCharacter.OneF0Class == 0x14)
                    {
                        //doors       
                        if (Rng.r.Next(0, 0xC) < (DefendingCharacter.item_id & 0x7) << 1)
                        {
                            var si_slot = 8 - playerdat.handednessvalue;
                            Debug.Print($"Equipment damage to slot {si_slot} of {Rng.DiceRoll(2, 4)}");
                        }
                       
                    }
                }
                return 0; //0 is a hit!
            }
        }


        /// <summary>
        /// Calculates the actual damage that applies on a sucessful attack
        /// </summary>
        /// <param name="damageType"></param>
        /// <param name="attacker"></param>
        /// <param name="MissileAttack"></param>
        /// <returns></returns>
        static int AttackerAppliesFinalDamage(int damageType, int attacker, bool MissileAttack = false)
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
            if (DefendingCharacter.index ==1)
            {
                //Debug.Print("playSoundEffect(3)");
            }
            else
            {
                //Debug.print(playsoundeffectAtcombatxy);
            }

            if (!MissileAttack)
            {
                //Do blood spatters.
                Debug.Print("Spatter blood");
            }

            if (DefendingCharacter.majorclass == 1)
            {
                if (critterObjectDat.bleed(DefendingCharacter.item_id) != 0)
                {
                    Debug.Print("TODO. place animo at hit body part");
                    animo.SpawnAnimoAtTarget(target: DefendingCharacter, subclassindex: 0, si_zpos: 5, tileX: DefendingCharacter.tileX, tileY: DefendingCharacter.tileY); //blood
                    if (AttackWasACrit)
                    {
                        animo.SpawnAnimoAtTarget(target: DefendingCharacter, subclassindex: 0, si_zpos: 3, tileX: DefendingCharacter.tileX, tileY: DefendingCharacter.tileY); //blood
                    }
                }
                else
                {//npc does not bleed
                    animo.SpawnAnimoAtTarget(target: DefendingCharacter, subclassindex: 0xB, si_zpos: 3, tileX: DefendingCharacter.tileX, tileY: DefendingCharacter.tileY); // a flash damage
                }
            }
            else
            {
                //hit a non-npc object
                animo.SpawnAnimoAtTarget(target: DefendingCharacter, subclassindex: 0xB, si_zpos: 3, tileX: DefendingCharacter.tileX, tileY: DefendingCharacter.tileY); // a flash damage
            }
        

            if (DefendingCharacter.majorclass == 1)
            {
                //npc has been hit, apply defenses
                var bodypartindex = BodyPartHit / 4;
                int cx = (int)critterObjectDat.toughness(DefendingCharacter.item_id, bodypartindex);
                if (cx == -1)
                {
                    BodyPartHit = BodyPartHit & 4;
                    cx = critterObjectDat.toughness(DefendingCharacter.item_id, 0);
                }
                if (DefendingCharacter.IsPowerfull == 1)
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
            }

            var di = finaldamage / 4;
            if (di <= 3)
            {
                di = 3;//this is used for animo later on?
            }
            if (DefendingCharacter.index == 1)
            {
                if (playerdat.difficuly == 1)
                {
                    finaldamage >>= 1;
                }
            }
            //apply damage
            damage.DamageObject(
                objToDamage: DefendingCharacter,
                basedamage: finaldamage,
                damagetype: damageType,
                objList: UWTileMap.current_tilemap.LevelObjects,
                WorldObject: true,
                damagesource: attacker);
            
            //Resume here.
            
            
            return 0;
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

        /// <summary>
        /// Checks for a hit within the weapon radius+3 and gets the body positions where the target has been hit
        /// </summary>
        /// <returns></returns>
        static bool checkAttackHit()
        {
            MotionCalcArray.PtrToMotionCalc = new byte[0x20];
            MotionCalcArray.MotionArrayObjectIndexA = AttackingCharacter.index;
            //var radius = (byte)commonObjDat.radius(currentWeaponItemID);
            MotionCalcArray.Radius8 = (byte)(CurrentWeaponRadius + 1);
            MotionCalcArray.Height9 = (byte)(((CurrentWeaponRadius << 1) + 1) << 2);
            MotionCalcArray.z4 = (ushort)(AttackingCharacter.zpos + (commonObjDat.height(AttackingCharacter.item_id) * (CurrentAttackSwingType / 3) / 3));

            if (AttackingCharacter.index == 1)
            {
                MotionCalcArray.z4 += (ushort)(motion.PlayerHeadingRelated_dseg_67d6_33D6 / 0x200);//this value is set in player motion. Assume 0 for now.
            }

            AttackHitZ_dseg_67d6_24CE = (commonObjDat.height(AttackingCharacter.item_id) / 6) + MotionCalcArray.z4;
            MotionCalcArray.x0 = (ushort)(AttackingCharacter.xpos + (AttackingCharacter.tileX << 3));
            MotionCalcArray.y2 = (ushort)(AttackingCharacter.ypos + (AttackingCharacter.tileY << 3));
            var di_heading = (AttackingCharacter.heading << 5) + AttackingCharacter.npc_heading;

            int x0 = MotionCalcArray.x0; int y2 = MotionCalcArray.y2;
            motion.GetCoordinateInDirection(di_heading, CurrentWeaponRadius + 3, ref x0, ref y2);
            MotionCalcArray.x0 = (ushort)x0; MotionCalcArray.y2 = (ushort)y2;

            motion.ScanForCollisions(0, 1);
            if (MotionCalcArray.Unk14_collisoncount != 0)
            {
                motion.SortCollisions();
                if (MotionCalcArray.Unk15 != 0)
                {
                    var si_hit = TestForClosestAttackCollisionHit();
                    if (si_hit >= 0)
                    {
                        var collisionRecord = motion.collisionTable[si_hit];
                        BodyPartHit = PickBodyHitPoint(
                            defenderZ: collisionRecord.zpos, defenderTop: collisionRecord.height,
                            attackerZ: MotionCalcArray.z4, attackerTop: MotionCalcArray.z4 + MotionCalcArray.Height9);

                        DefendingCharacter = UWTileMap.current_tilemap.LevelObjects[collisionRecord.link];
                        return true;
                    }
                }
            }
            else
            {
                //seg024_24E9_65C:
                motion.ProcessMotionTileHeights_seg028_2941_385(0);
                if (((MotionCalcArray.UnkC_terrain_base | MotionCalcArray.UnkE_base) & 0x300) != 0)
                {
                    Debug.Print("Todo SpawnImpactAnimo()");
                }

            }
            return false;
        }


        /// <summary>
        /// Tests the collisons with the weapon strike to find the closest one.
        /// </summary>
        /// <returns></returns>
        static int TestForClosestAttackCollisionHit()
        {
            var result = -1;
            var di_collisionindex = MotionCalcArray.Unk16_collisionindex;

            var var18_nearestcollision = 0x186a0;

            var var4 = MotionCalcArray.Unk16_collisionindex + MotionCalcArray.Unk15;

            var AttackerX = (AttackingCharacter.npc_xhome << 3) + AttackingCharacter.xpos;
            var AttackerY = (AttackingCharacter.npc_yhome << 3) + AttackingCharacter.ypos;

            while (di_collisionindex < var4)
            {
                var collisionRecord = motion.collisionTable[di_collisionindex];
                var collisionObject = UWTileMap.current_tilemap.LevelObjects[collisionRecord.link];
                if (collisionObject.majorclass != 6) //traps and triggers
                {
                    if (collisionObject.index != AttackingCharacter.index)
                    {
                        bool runBlock = false;
                        if (AttackingCharacter.index == 1)
                        {
                            if (collisionObject.IsStatic)
                            {
                                runBlock = true;
                            }
                            else
                            {
                                if (collisionObject.IsAlly == 0)
                                {
                                    runBlock = true;
                                }
                                else
                                {
                                    if (var4 - 1 == di_collisionindex)
                                    {
                                        if (var18_nearestcollision == 0x186A0)
                                        {
                                            runBlock = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            runBlock = true;
                        }


                        if (runBlock)
                        {
                            //Block_seg024_24E9_192:

                            var CollisionXY = collisionRecord.xyvalue & 0x3F;
                            CombatHitTileX = ((MotionCalcArray.x0 >> 3) + CollisionXY) & 0x3F;

                            CollisionXY = CombatHitTileX - (MotionCalcArray.x0 >> 3);
                            CombatHitTileY = (((collisionRecord.xyvalue - CollisionXY) / 0x40) + (MotionCalcArray.y2 >> 3)) & 0x3F;

                            var collisonXCoord = (CombatHitTileX << 3) + collisionObject.xpos;
                            var xDiff = AttackerX - collisonXCoord;

                            var collisonYCoord = (CombatHitTileY << 3) + collisionObject.ypos;
                            var yDiff = AttackerY - collisonYCoord;

                            var DistanceSquared_var14 = (xDiff * xDiff) + (yDiff * yDiff);
                            if (DistanceSquared_var14 >= var18_nearestcollision)
                            {
                                var18_nearestcollision = DistanceSquared_var14;
                                result = di_collisionindex;
                            }
                        }
                    }
                }

                di_collisionindex++;
            }

            if (result >= 0)
            {
                var collisionRecord = motion.collisionTable[result];
                var CollisionXY = collisionRecord.xyvalue & 0x3F;
                CombatHitTileX = ((MotionCalcArray.x0 >> 3) + CollisionXY) & 0x3F;

                CollisionXY = CombatHitTileX - (MotionCalcArray.x0 >> 3);
                CombatHitTileY = (((collisionRecord.xyvalue - CollisionXY) / 0x40) + (MotionCalcArray.y2 >> 3)) & 0x3F;
            }

            return result;
        }




        /// <summary>
        /// Picks the body part that has been hit in combat
        /// </summary>
        /// <param name="defenderZ"></param>
        /// <param name="defenderTop"></param>
        /// <param name="attackerZ"></param>
        /// <param name="attackerTop"></param>
        /// <returns></returns>
        public static int PickBodyHitPoint(int defenderZ, int defenderTop, int attackerZ, int attackerTop)
        {
            var di_defendermid = (defenderZ + defenderTop) >> 1;
            var si_attackermid = (attackerZ + attackerTop) >> 1;

            if (defenderZ + 1 <= si_attackermid)
            {
                //seg024_24E9_29:
                if (defenderTop - 1 >= attackerZ)
                {
                    //seg024_24E9_36
                    if (si_attackermid >= di_defendermid)
                    {
                        //seg024_24E9_4E:
                        if (Rng.r.Next(3) == 0)
                        {
                            return 3;
                        }
                    }
                    else
                    {
                        //seg024_24E9_3A
                        if (Rng.r.Next(2) != 0)
                        {
                            return 2;
                        }
                    }
                    //seg024_24E9_62:
                    if (Rng.r.Next(3) == 0)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }

                }
                else
                {
                    return 3;
                }
            }
            else
            {
                return 2;
            }
        }


        /// <summary>
        /// Only certain weapons can use a poison enchantment. for the moment return true here.
        /// </summary>
        /// <returns></returns>
        static bool checkforPoisonableWeapon()
        {
            Debug.Print("Checkforpoisonableweapon()");
            return true;
        }

        /// <summary>
        /// Gets a damage bonus based on the relative headings of the attacker and defender.
        /// </summary>
        static int CalcFlankingBonus()
        {
            if (DefendingCharacter.majorclass == 1)
            {
                var bonus = (DefendingCharacter.heading + 0xC - AttackingCharacter.heading) & 0x7;
                if (bonus > 4)
                {
                    bonus = 8 - bonus;
                }
                return bonus;
            }
            else
            {
                return 0;
            }
        }


    }//end class
}//end namespace