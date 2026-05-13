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
        public static void CombatChargingLoop()
        {
            switch (stage)
            {
                case CombatStages.Ready:
                    stage = CombatStages.Charging; //begin charging. start weapon swing pull back anim
                    IncreaseCharge();
                    break;
                case CombatStages.Charging: //building up charge
                    IncreaseCharge();
                    break;
            }
        }

        /// <summary>
        /// Increases the charge by weapon speed score every 16 units of a timer
        /// </summary>
        private static void IncreaseCharge()
        {
            if (CombatTimerDifference >= 0) // = should be every 16 units between a previuosly stored timer in vanilla but I'm unsure what time units they are. assuming 1 second. Feels a bit fast
            {
                
                CombatTimerDifference += (main.GlobalPITTimer - PreviousCombatPITTimer);
                PreviousCombatPITTimer = main.GlobalPITTimer;
                while (CombatTimerDifference>0x10)
                {
                    PlayerAttackCharge = Math.Min(PlayerAttackCharge + ChargeSpeed, 100);
                    var frame = 1 + (PlayerAttackCharge / 12);
                    uimanager.ChangePower(frame);
                    CombatTimerDifference -= 0x10;
                }
            }
        }

        /// <summary>
        /// Ends the combat attack
        /// </summary>
        public static void EndCombatLoop()
        {
            uimanager.instance.mousecursor.SetCursorToCursor(0);
            PlayerAttackCharge = 0;
            stage = CombatStages.Ready;
            uimanager.ResetPower();
            CombatTimerDifference = 0;
        }


        /// <summary>
        /// Processes the various stages of combat
        /// </summary>
        public static void CombatInputHandler(double delta)
        {
            if (uimanager.InteractionMode == uimanager.InteractionModes.ModeAttack)
            {
                if ((playerdat.ObjectInHand != -1)
                || (useon.CurrentItemBeingUsed != null)
                || (SpellCasting.currentSpell != null)
                || (uimanager.blockmouseinput))
                {
                    return;
                }
                bool MouseHeldDown = Input.IsMouseButtonPressed(MouseButton.Right);
                switch (stage)
                {
                    case CombatStages.Ready:
                        if (MouseHeldDown)
                        {    
                            PreviousCombatPITTimer = main.GlobalPITTimer;
                            CombatTimerDifference = 0;

                            if (isWeapon(playerdat.PrimaryHandObject) == 2)
                            {
                                //ranged combat targeting (if player has ammo)
                                //check for ammo
                                //if no ammo cancel
                                //uimanager.instance.mousecursor.SetCursorToCursor(9);
                                var ammoType = rangedObjectDat.RangedWeaponType(playerdat.PrimaryHandObject.item_id);
                                var foundammo = objectsearch.FindMatchInFullObjectList(0, 1, ammoType, playerdat.InventoryObjects);
                                if (foundammo == null)
                                {
                                    uimanager.AddToMessageScroll($"{GameStrings.GetString(1, 207)}{GameStrings.GetSimpleObjectNameUW(16 + ammoType)}s");//sorry you have no Xs
                                    return;
                                }
                            }
                            else
                            {
                                //Non vanilla behaviour. work out the attack type now based on where the mouse is in the view port.
                                int X1 = (int)((uimanager.ViewPortMouseXPos / 4) - uimanager.Window3DLeftBorder);//offset from the left side border
                                int Y1 = (int)((200f - uimanager.ViewPortMouseYPos / 4) - 54f);

                                if (X1 > uimanager.Window3DMaxX)
                                {
                                    X1 = uimanager.Window3DMaxX;
                                }
                                else
                                {
                                    if (X1 < 0)
                                    {
                                        X1 = 0;
                                    }
                                }

                                if (Y1 > uimanager.Window3DMaxY)
                                {
                                    Y1 = uimanager.Window3DMaxY;
                                }
                                else
                                {
                                    if (Y1 < 0)
                                    {
                                        Y1 = 0;
                                    }
                                }
                                //int segmentY = -1;
                                if (Y1 <= uimanager.Window3DMaxY / 3)
                                {
                                    WeaponSwingTypePlayer = 2;
                                }
                                else
                                {
                                    if (Y1 <= 2 * uimanager.Window3DMaxY / 3)
                                    {
                                        WeaponSwingTypePlayer = 0;
                                    }
                                    else
                                    {
                                        WeaponSwingTypePlayer = 1;
                                    }
                                }
                            }
                            Debug.Print($"Swing type {WeaponSwingTypePlayer}");

                            OnHitSpell = 0;
                            JeweledDagger = false;
                            AttackScore = 0;
                            AttackDamage = 0;
                            AttackScoreFlankingBonus = 0;
                            AttackWasACrit = false;

                            Debug.Print("PlacehholderInitialising PlayerWeaponSound to 1");
                            PlayerWeaponSound = 1;
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
                                CombatChargingLoop();
                                switch (isWeapon(playerdat.PrimaryHandObject))
                                {
                                    case 1:
                                        //melee or fist
                                        uimanager.currentWeaponAnim = WeaponAnimGroup + WeaponAnimStrikeOffset + WeaponAnimHandednessOffset;
                                        break;
                                    case 2://ranged
                                        if (PlayerAttackCharge >= mincharge)
                                        {
                                            //ranged weapon change targeting icon
                                            uimanager.instance.mousecursor.SetCursorToCursor(9);
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                stage = CombatStages.Release;
                            }
                            break;
                        }
                    case CombatStages.Release:
                        {
                            combatanimationtimer = 0;
                            if (uimanager.IsMouseInViewPort())
                            {
                                if (PlayerAttackCharge >= mincharge)
                                {
                                    //start return swing   swing                            
                                    Debug.Print($"Releasing attack at charge {PlayerAttackCharge}");
                                    if (isWeapon(playerdat.PrimaryHandObject) == 2)
                                    {
                                        //ranged
                                        //launch projectile if has ammo
                                        var ammoType = rangedObjectDat.RangedWeaponType(playerdat.PrimaryHandObject.item_id);
                                        //check if player has one some ammo.
                                        var foundammo = objectsearch.FindMatchInFullObjectList(0, 1, ammoType, playerdat.InventoryObjects);
                                        if (foundammo == null)
                                        {
                                            uimanager.AddToMessageScroll($"{GameStrings.GetString(1, 207)}{GameStrings.GetSimpleObjectNameUW(16 + ammoType)}s");//sorry you have no Xs
                                            return;
                                        }
                                        else
                                        {
                                            MissileRelease(playerdat.PrimaryHandObject.classindex, foundammo);
                                        }
                                        //if has ammo launch projectile
                                        EndCombatLoop();//reset
                                    }
                                    else
                                    {
                                        //melee
                                        uimanager.currentWeaponAnim = WeaponAnimGroup + WeaponAnimStrikeOffset + WeaponAnimHandednessOffset + 1;
                                        stage = CombatStages.Swinging;
                                    }
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
                            combatanimationtimer += delta;
                            if (combatanimationtimer >= 0.15f)
                            {
                                Debug.Print("Swing completed");
                                stage = CombatStages.Striking;
                            }
                            break;
                        }
                    case CombatStages.Striking:
                        {
                            //weapon has struck do combat calcs  (if melee) 
                            CalculatePlayerAttackScores();

                            ExecuteAttack(playerdat.playerObject);
                            if (_RES == GAME_UW2)
                            {
                                if (OnHitSpell > 0)
                                {
                                    if (AttackWasACrit || OnHitSpell == 6)
                                    {
                                        CastOnWeaponHitSpells();
                                    }
                                }
                            }

                            //when done start reset
                            stage = CombatStages.Resetting;
                            combatanimationtimer=0;
                            break;
                        }
                    case CombatStages.Resetting:
                        {
                            //do weapon put away anim until time   
                            combatanimationtimer += delta;
                            if (combatanimationtimer >= 0.2f)
                            {
                                uimanager.currentWeaponAnim = WeaponAnimGroup + WeaponAnimHandednessOffset + 6;
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

        static void MissileRelease(int RangedWeaponSubclass, uwObject foundAmmo)
        {
            //remove ammo from inventory
            var PlayerLaunched = true;

            if ((_RES == GAME_UW2) && (
                (RangedWeaponSubclass == 8) || (RangedWeaponSubclass == 9) || (RangedWeaponSubclass == 0xA)
            ))
            {
                motion.MissileFlagB = true;
            }
            else
            {
                motion.MissileFlagB = false;
            }

            motion.RangedAmmoItemID = foundAmmo.item_id;
            motion.RangedAmmoType = rangedObjectDat.ammotype(foundAmmo.item_id);

            motion.projectileXHome = playerdat.playerObject.npc_xhome;
            motion.projectileYHome = playerdat.playerObject.npc_yhome;
            motion.MissileLauncherHeadingBase = 1;
            motion.InitPlayerProjectileValues();

            var projectile = motion.PrepareProjectileObject(playerdat.playerObject);

            if (projectile != null)
            {
                projectile.flags = foundAmmo.flags;
                projectile.is_quant = foundAmmo.is_quant;
                projectile.link = 1;
                projectile.owner = foundAmmo.owner;
                projectile.doordir = foundAmmo.doordir;

                if (projectile.majorclass != 5)
                {
                    if (commonObjDat.rendertype(foundAmmo.item_id) != 2)
                    {
                        projectile.npc_whoami = projectile.heading;
                    }
                }
                ObjectCreator.Consume(foundAmmo, true);
            }
            else
            {
                //play soundeffect
                UWsoundeffects.PlaySoundEffectAtAvatar(UWsoundeffects.SoundEffectFail, 0x40, 0);
                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_need_more_space_to_fire_that_weapon_));
            }

            if ((motion.MissileFlagA == false) && (RangedWeaponSubclass == 8))
            {
                if (PlayerLaunched)
                {
                    //make a sound
                    UWsoundeffects.PlaySoundEffectAtAvatar(UWsoundeffects.SoundEffectBowTwang, 0x40, 0);
                }
            }
            motion.MissileFlagA = false;
        }

    }//end class
}//end namespace