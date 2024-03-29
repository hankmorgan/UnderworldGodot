using System;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the look verb
    /// </summary>
    public class combat : UWClass
    {
        public static uwObject currentweapon;  //if null then using fist
        public static int stage = 0;  //0=weapdrawn, 1 = charge bulding, 2=release attack, 3=do result, 4= wait for next strike window?
        public static double combattimer = 0.0;
       
        /// <summary>
        /// Item ID for Fist object
        /// </summary>
        const int fist = 15;

        public static int WeaponCharge = 0;

        static int ChargeSpeed
        {
            get
            {
                if (currentweapon==null)
                {
                    return weaponObjectDat.chargespeed(fist);
                }
                else
                {
                 return weaponObjectDat.chargespeed(currentweapon.item_id);
                }
            }
        }
        public static void CombatLoop(double delta)
        {
            stage = 1;
            combattimer += delta;
        
            if (combattimer > 0.0625) // = should be every 16 units between a previuosly stored timer in vanilla but I'm unsure what time units they are. assuming 1 second. Feels a bit fast
            {
                WeaponCharge = Math.Min(WeaponCharge + ChargeSpeed, 100);
                var frame = 1 + (WeaponCharge/12);
                Debug.Print($"{frame} at {WeaponCharge}");
                uimanager.ChangePower(frame);
                combattimer = 0f;
            }
        }

        public static void EndCombatLoop()
        {
            WeaponCharge = 0;
            stage = 0;
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
            if (weapon==null)
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
    }//end class
}//end namespace