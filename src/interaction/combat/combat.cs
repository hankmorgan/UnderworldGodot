using System.Diagnostics;
using System.Net.Http.Headers;
using Godot;


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
                AttackDamage = (playerdat.STR / 9) + CurrentWeaponBaseDamage(WeaponSwingTypePlayer);
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
            NPCFinalAttackCharge = attackcharge;
            CurrentWeaponRadius = 2;//always this for NPCS, is this a vanilla bug that overwrites player weapon radius.
            AttackingCharacter = attacker;
            AttackDamage = critterObjectDat.attackdamage(attacker.item_id, attacktype) + (critterObjectDat.strength(attacker.item_id) / 5);
            AttackScore = critterObjectDat.chancetohit(attacker.item_id, attacktype) + (critterObjectDat.EquipmentDamageOrBaseHitChance(attacker.item_id) >> 1);

            if (attacker.IsPowerful == 1)
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
                Debug.Print($"CheckAttackHit = HIT on {DefendingCharacter.a_name} {DefendingCharacter.index}");
                //var defender = UWTileMap.current_tilemap.LevelObjects[DefenderIndex];
                if ((AttackingCharacter.index != 1) && (DefendingCharacter.index != 1) && (!DefendingCharacter.IsStatic))
                {
                    if (attacker.IsAlly == DefendingCharacter.IsAlly)
                    {
                        return false;
                    }
                }

                AttackScoreFlankingBonus = CalcFlankingBonus();
                var attackresult = CalcAttackResults();
                CombatMissImpactSound(attackresult);
                if (attackresult == 0)
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
                    return false;
                }
            }
            else
            {
                Debug.Print("CheckAttackHit = MISS");
                CombatMissImpactSound(0);
                return false;//swing and a miss
            }
        }

        static void CombatMissImpactSound(int attackresult)
        {
            if (attackresult != 0)//a hit
            {
                int var6 = 0;
                int var7 = 0;
                if (AttackingCharacter.index == 1)
                {
                    var6 = PlayerWeaponSound;
                }
                else
                {
                    if (AttackingCharacter.index < 0x100)
                    {
                        var6 = critterObjectDat.combatimpactsound_7_3(AttackingCharacter.item_id);
                    }
                    else
                    {
                        var6 = 1;
                    }
                }
                //seg024_DB9
                if (DefendingCharacter.index == 1)
                {
                    //sound is based on body part/armour piece hit
                    var var5slot = BodyPartHit + 1;
                    var ObjectInSlot = playerdat.GetInventorySlotObject(var5slot);
                    if (ObjectInSlot != null)
                    {
                        //uw2 values. need to dbl check for uw1
                        switch (ObjectInSlot.item_id)
                        {
                            case 0x20:
                            case 0x23:
                            case 0x26:
                            case 0x29:
                            case 0x2C://leather objects
                                var7 = 0;
                                break;
                            default:
                                var7 = 1;
                                break;
                        }
                    }
                    else
                    {
                        var7 = 1;
                    }
                }
                else
                {
                    //defender is not player
                    if (DefendingCharacter.index < 0x100)
                    {
                        var7 = critterObjectDat.combatimpactsound_4_3(DefendingCharacter.item_id);
                    }
                    else
                    {
                        var7 = 0;
                    }
                }

                //TODO: UW1 may have different values.
                var effectVar8 = 0;
                //Seg24_E37
                if (var6 != 1)
                {
                    if (var6 == 2)
                    {
                        if (var7 == 1)
                        {
                            effectVar8 = 7;
                        }
                        else
                        {
                            effectVar8 = 8;
                        }
                    }
                    else
                    {
                        effectVar8 = 8;
                    }
                }
                else
                {
                    effectVar8 = 7;
                }
                UWsoundeffects.PlaySoundEffectAtObject((byte)effectVar8, DefendingCharacter, 0);
            }
            else
            {
                //a miss
                //TODO check a global variable that is related to missile impacts. if set do not play sound.
                UWsoundeffects.PlaySoundEffectAtObject(effectNo: 0xA, obj: AttackingCharacter, volDelta: 0);
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
                        if (critterObjectDat.bleed(DefendingCharacter.item_id) != 0)
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
        static void AttackerAppliesFinalDamage(int damageType, int attacker, bool MissileAttack = false)
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
                AttackDamage = Rng.DiceRoll(NoOfLoops: damagequotient, diceRange: 6);
            }
            if (damageremainder != 0)
            {
                AttackDamage += Rng.DiceRoll(1, damageremainder);
            }
            int finaldamage;
            if (attacker == 1)
            {
                finaldamage = (AttackDamage * PlayerAttackCharge) >> 7;
            }
            else
            {
                finaldamage = (AttackDamage * NPCFinalAttackCharge) >> 7;
            }
            //var finaldamage = (AttackDamage * NPCFinalAttackCharge) >> 7;
            finaldamage += AttackScoreFlankingBonus;

            //Play hit sounds
            if (DefendingCharacter.index == 1)
            {
                //seg024_24E9_A25:
                UWsoundeffects.PlaySoundEffectAtAvatar(UWsoundeffects.SoundEffectHit1, 0x40, (byte)(AttackDamage << 2));
            }
            else
            {
                //seg024_24E9_A33:
                if (!MissileAttack)
                {
                    var SoundEffectVarC = 04;
                    if (_RES == GAME_UW2)
                    {
                        switch (PlayerWeaponSound)
                        {
                            case 2:
                                SoundEffectVarC = 0x1B; break;
                            case 0:
                            case 1:
                            default:
                                SoundEffectVarC = 4; break;
                        }
                    }

                    UWsoundeffects.PlaySoundEffectAtCoordinate((byte)SoundEffectVarC, CombatHitTileX, CombatHitTileY, finaldamage << 2);
                }
            }


            //seg024_24E9_A9B:
            if (DefendingCharacter.majorclass == 1)
            {
                //npc has been hit, apply defenses
                var bodypartindex = BodyPartHit % 4;
                int cx = (int)critterObjectDat.toughness(DefendingCharacter.item_id, bodypartindex);
                if (cx == -1)
                {
                    BodyPartHit = BodyPartHit & 4;
                    cx = critterObjectDat.toughness(DefendingCharacter.item_id, 0);
                }
                if (DefendingCharacter.IsPowerful == 1)
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

            //Seg24_B23
            var di_damage_level = finaldamage / 4;
            if (di_damage_level >= 4)
            {
                di_damage_level = 3;//used later on to set the intesity of the screen shake
            }
            if (DefendingCharacter.index == 1)
            {
                if (playerdat.difficuly == 1)
                {
                    finaldamage >>= 1;
                }
            }
            //apply damage
            var DamageObjectResult = damage.DamageObject(
                objToDamage: DefendingCharacter,
                basedamage: finaldamage,
                damagetype: damageType,
                objList: UWTileMap.current_tilemap.LevelObjects,
                WorldObject: true,
                damagesource: attacker);

            //Resume here.
            if (finaldamage != 0)
            {
                if (attacker != -1)//should be attackingcharacter != -1
                {
                    if (BodyPartHit >= 4)
                    {
                        BodyPartHit = 4;
                    }
                    if (DefendingCharacter.index != 1)
                    {
                        //seg024_24E9_BD5:
                        if (DefendingCharacter.majorclass == 1)
                        {
                            if (attacker == 1)
                            {
                                //seg024_24E9_BEB
                                uimanager.SetEyeLevel(DefendingCharacter.npc_hp, critterObjectDat.avghit(DefendingCharacter.item_id));
                            }
                            //seg024_24E9_C49:
                            if (critterObjectDat.bleed(DefendingCharacter.item_id) != 0)
                            {
                                animo.SpawnAnimoAtTarget(DefendingCharacter, 0, BodyHitZ[BodyPartHit], CombatHitTileX, CombatHitTileY);
                                if (AttackWasACrit && (attacker == 1))
                                {
                                    animo.SpawnAnimoAtTarget(DefendingCharacter, 0, BodyHitZ[BodyPartHit] - 2, CombatHitTileX, CombatHitTileY);
                                }
                                return;
                            }
                            else
                            {
                                DamageObjectResult = 0;
                            }
                        }
                        //seg024_24E9_CCD
                        // if (DamageObjectResult !=0)
                        // {
                        //     DefendingCharacter = null;//?
                        // }
                        if ((DefendingCharacter.OneF0Class == 0x14) || (DefendingCharacter.item_id == 0x1CF))
                        {
                            //door or moving door
                            if (AttackHitZ_dseg_67d6_24CE > DefendingCharacter.zpos)
                            {
                                AttackHitZ_dseg_67d6_24CE = DefendingCharacter.zpos + 2;
                            }
                        }
                        animo.SpawnAnimoAtTarget(DefendingCharacter, 1, -AttackHitZ_dseg_67d6_24CE, CombatHitTileX, CombatHitTileY);//a flash
                    }
                    else
                    {
                        //Seg24_BC0
                        //player was the defender.
                        motion.SetScreenShake(TypeOfShake: 0x20, duration: (byte)(di_damage_level * 5));
                    }
                }
            }
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
                switch (WeaponSwingTypePlayer) //random for now
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
            MotionCalcArray.z4 = (ushort)(AttackingCharacter.zpos + (commonObjDat.height(AttackingCharacter.item_id) * (AttackSwingHeightAdjust / 3) / 3)); //note when on zpos=0 and using a stab attack the calculated z4 will be out of bounds. this is vanilla behaviour.

            if (AttackingCharacter.index == 1)
            {
                MotionCalcArray.z4 += (ushort)(motion.PlayerCameraPitch_dseg_67d6_33D6 / 0x200);
            }

            AttackHitZ_dseg_67d6_24CE = (commonObjDat.height(AttackingCharacter.item_id) / 6) + MotionCalcArray.z4;
            MotionCalcArray.x0 = (ushort)(AttackingCharacter.xpos + (AttackingCharacter.tileX << 3));
            MotionCalcArray.y2 = (ushort)(AttackingCharacter.ypos + (AttackingCharacter.tileY << 3));
            var di_heading = (AttackingCharacter.heading << 5) + AttackingCharacter.npc_heading;

            int x0 = MotionCalcArray.x0; int y2 = MotionCalcArray.y2;
            motion.GetCoordinateInDirection(di_heading, CurrentWeaponRadius + 3, ref x0, ref y2);
            MotionCalcArray.x0 = (ushort)x0; MotionCalcArray.y2 = (ushort)y2;
            Debug.Print($"Calculating attack hit at point {MotionCalcArray.x0},{MotionCalcArray.y2},{MotionCalcArray.z4}");
            motion.ScanForCollisions(0, 1); //TODO. this check is failing to scan for some NPCs. 
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
                            if (DistanceSquared_var14 < var18_nearestcollision)
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