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
                Debug.Print($"{frame} at {WeaponCharge} of {mincharge}");
                uimanager.ChangePower(frame);
                combattimer = 0f;
            }
        }

        /// <summary>
        /// Ends the combat attack
        /// </summary>
        public static void EndCombatLoop()
        {
            uimanager.instance.mousecursor.SetCursorToCursor(0);
            WeaponCharge = 0;
            stage = CombatStages.Ready;
            uimanager.ResetPower();
            combattimer = 0;
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
                            if (isWeapon(playerdat.PrimaryHandObject) == 2)
                            {
                                //ranged combat targeting (if player has ammo)
                                //check for ammo
                                //if no ammo cancel
                                //uimanager.instance.mousecursor.SetCursorToCursor(9);
                                var ammoType = rangedObjectDat.RangedWeaponType(playerdat.PrimaryHandObject.item_id);
                                var match = objectsearch.FindMatchInFullObjectList(0, 1, ammoType, playerdat.InventoryObjects);
                                if (match == null)
                                {
                                    uimanager.AddToMessageScroll($"{GameStrings.GetString(1, 207)}{GameStrings.GetSimpleObjectNameUW(16 + ammoType)}s");//sorry you have no Xs
                                    return;
                                }
                            }
                            else
                            {
                                //default mouse (in case cursor is stuck)
                                //uimanager.instance.mousecursor.SetCursorToCursor(0);
                                CurrentAttackSwingType = Rng.r.Next(0, 4);
                            }

                            OnHitSpell = 0;
                            JeweledDagger = false;
                            AttackScore = 0;
                            AttackDamage = 0;
                            AttackScoreFlankingBonus = 0;
                            AttackWasACrit = false;                            
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
                                switch (isWeapon(playerdat.PrimaryHandObject))
                                {
                                    case 1:
                                        //melee or fist
                                        uimanager.currentWeaponAnim = WeaponAnimGroup + WeaponAnimStrikeOffset + WeaponAnimHandednessOffset;
                                        break;
                                    case 2://ranged
                                        if (WeaponCharge >= mincharge)
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
                            if (uimanager.IsMouseInViewPort())
                            {
                                if (WeaponCharge >= mincharge)
                                {
                                    //start return swing   swing                            
                                    Debug.Print($"Releasing attack at charge {WeaponCharge}");
                                    
                                    combattimer = 0;
                                    if (isWeapon(playerdat.PrimaryHandObject) == 2)
                                    {
                                        //ranged
                                        //launch projectile if has ammo
                                        //check for ammo

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

                            CalculatePlayerAttackScores();

                            ExecuteAttack(playerdat.playerObject);
                            if (_RES == GAME_UW2)
                            {
                                if (OnHitSpell>0)
                                {
                                    if (AttackWasACrit || OnHitSpell == 6)
                                    {
                                        CastOnWeaponHitSpells();
                                    }                                
                                }
                            }

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

    }//end class
}//end namespace