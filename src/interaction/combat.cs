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
        public static CombatStages stage = 0; 
        public static double combattimer = 0.0;

        /// <summary>
        /// Item ID for Fist object
        /// </summary>
        const int fist = 15;

        public static int WeaponCharge = 0;


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
                            //weapon has struck do combat calcs                            
                            Debug.Print("Striking");
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
    }//end class
}//end namespace