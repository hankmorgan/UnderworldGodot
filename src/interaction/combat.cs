using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the look verb
    /// </summary>
    public class combat : UWClass
    {
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
            if (currentweapon!=null)
            {
                switch(attacktype)
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
        static bool JeweledDagger = false;

        /// <summary>
        /// Item ID for Fist object
        /// </summary>
        const int fist = 15;

        public static int WeaponCharge = 0;

        public static int PlayerAttackScore = 0;
        public static int PlayerAttackDamage = 0;


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
                bool MouseHeldDown = Input.IsMouseButtonPressed(MouseButton.Right);
                switch(stage)
                {
                    case CombatStages.Ready:
                        if (MouseHeldDown)
                        {
                            JeweledDagger = false;
                            PlayerAttackScore = 0;
                            PlayerAttackDamage = 0;
                            stage = CombatStages.Charging; 
                            CombatChargingLoop(delta);                           
                        }
                        break;
                    case CombatStages.Charging:
                        {
                            if (MouseHeldDown)
                            {
                                CombatChargingLoop(delta);
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
                                //start swing                            
                                Debug.Print($"Releasing attack at charge {WeaponCharge}");
                                stage = CombatStages.Swinging;
                                combattimer = 0;
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
                            if (combattimer>=0.2f)
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
                            if (combattimer>=0.2f)
                            {
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
            var result = uimanager.DoRayCast(position, 2f);
            if (result != null)
            {
                if (result.ContainsKey("collider") && result.ContainsKey("normal") && result.ContainsKey("position"))
                {
                    var obj = (StaticBody3D)result["collider"];
                    var normal = (Vector3)result["normal"];
                    var pos = (Vector3)result["position"];
                    Debug.Print(obj.Name);
                    string[] vals = obj.Name.ToString().Split("_");
                    switch (vals[0].ToUpper())
                    {
                        case "TILE":
                        case "WALL":
                        case "CEILING"://hit a wall/surface
                            Debug.Print("hit a wall. do selfdamage to the weapon");
                            return false;
                        default:
                                if (int.TryParse(vals[0], out int index))
                                {
                                    var hitobject = UWTileMap.current_tilemap.LevelObjects[index];
                                    if (hitobject!=null)
                                    {
                                        if (hitobject.majorclass==1)//hit an npc
                                        {
                                            Debug.Print($"{hitobject.a_name}");
                                            return PlayerHitsNPC(hitobject);   
                                        }
                                    }                                    
                                }
                                break;                            
                    }
                }               
            }
            return false; //miss attack
        }

        /// <summary>
        /// Do the attack calcs for the player hitting an npc
        /// </summary>
        /// <param name="critter"></param>
        /// <returns></returns>
        static bool PlayerHitsNPC(uwObject critter)
        {
            //calc final attack charge based on the % of charge built up in the weapon
            var finalcharge = mincharge+((maxcharge-mincharge)*WeaponCharge)/100; //this is kept later for damage calcs.
            
            //do attack calcs
            CalcPlayerAttackScores();

            //update npc AI/attitudes

            //process hit result crit, success, fail or critfail/

            //apply damages

            //post apply spell effect if applicable

            return true;
        }

        static void CalcPlayerAttackScores()
        {
            var weapon = currentweapon;   
            var weaponskill = playerdat.GetSkillValue(currentMeleeWeaponSkillNo);         
            
            if (_RES==GAME_UW2)
            {
                if (currentWeaponItemID == 10)
                {
                    JeweledDagger = true;                
                }
            }

            PlayerAttackScore = weaponskill + (playerdat.Attack>>1) + (playerdat.DEX/7) + playerdat.ValourBonus ;
            if (playerdat.difficuly == 1)
            {   //easy dificulty
                PlayerAttackScore +=7;
            }           
            

            //base damage calcs
            
            if (weaponskill == 2)
            {
                //do unarmed calcs for base damage
                PlayerAttackDamage = 4 + (playerdat.STR/6) + (playerdat.Unarmed<<1)/5;
            }
            else
            {
                var attacktype = Rng.r.Next(0,3);
                //do weapon based calcs, using a random attack type now.
                PlayerAttackDamage = (playerdat.STR/9) + CurrentWeaponBaseDamage(attacktype);
            }

            //Then get weapon enchantments
            if (currentweapon!=null)
            {
                var enchant = MagicEnchantment.GetSpellEnchantment(currentweapon,playerdat.InventoryObjects);
                if (enchant!=null)
                {
                    Debug.Print($"Weapon has enchantment {enchant.NameEnchantment} {enchant.SpellMajorClass} {enchant.SpellMinorClass}");
                }                
            }            
        }
    }//end class
}//end namespace