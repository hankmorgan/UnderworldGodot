using System;
using System.Diagnostics;

namespace Underworld
{
    public partial class SpellCasting : UWClass
    {
        //targeted spells
        public static void CastClass7_Spells(int minorclass, int index, uwObject[] objList)
        {
            if (_RES == GAME_UW2)
            {
                switch (minorclass)
                {
                    case 0:
                        //cause bleeding  
                        CauseBleeding(index, objList);     
                        break;
                    case 1:
                        //Causefear
                        break;
                    case 2:
                        //SmiteUndead
                        break;
                    case 3:
                        //Charm
                        break;
                    case 4:
                        //smite foe
                        break;
                    case 5:
                        //paralyse  
                        break;
                    case 6:
                        ///bleed (identical)
                        CauseBleeding(index, objList);
                        break;
                    case 7:
                        StudyMonster(index, objList);
                        break;
                    case 8:
                        //Dispel rune
                        break;
                    case 9:
                        //Repair
                        break;
                    case 10:
                        //disarm trap
                        break;
                    case 11:
                        //name enchantment
                        break;
                    case 12:
                        //unlock spell
                        break;
                    case 13:
                        //detect trap
                        break;
                    case 14:
                        //enchantment spell
                        break;
                    case 15:
                        //gate travel
                        break;
                }
            }
            else
            {//the uw1 list of class 7 spells is much shorter
                switch (minorclass)
                {
                    case 0:
                        //some sort of explosion?   
                        break;
                    case 1:
                        //Cause fear
                        break;
                    case 2:
                        //smite undeead
                        break;
                    case 3:
                        //ally
                        break;
                    case 4:
                        //poison
                        break;
                    case 5:
                        //paralyse
                        break;
                }
            }
        }

        /// <summary>
        /// UW2 only spell. Causes damage scaled by vulnerability
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        static void CauseBleeding(int index, uwObject[] objList)
        {
            var obj = objList[index];
            if (obj!=null)
            {
                if (obj.majorclass == 1)
                {
                    //npc
                    
                    var bleed = critterObjectDat.bleed(obj.item_id);
                    if (bleed==0)
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x12B));
                    }
                    else
                    {
                        var basedamage = 0xA + playerdat.Casting/2;
                        ScaledDamageOnNPCWithAnimo(
                            critter: obj, 
                            basedamage: basedamage, 
                            damagetype: 4, 
                            animoclassindex: 0);
                    }
                }
            }
        }

        static void StudyMonster(int index, uwObject[] objList)
        {
            Debug.Print("STUDY MONSTER");
        }

        
        
        /// <summary>
        /// Scales damage on the NPC based on it's vulnerabilities defined in critter object data
        /// Spawns an animo of the specified type to represent blood etc
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="basedamage"></param>
        /// <param name="damagetype"></param>
        /// <param name="UpdateUI"></param>
        public static void ScaledDamageOnNPCWithAnimo(uwObject critter, int basedamage, int damagetype, int animoclassindex, bool UpdateUI = true)
        {
            var noOfSplatters = basedamage;
             
            noOfSplatters = noOfSplatters / 4;
            if (noOfSplatters>3)
            {
                noOfSplatters = 3;
            }
            
            Debug.Print($"Spawn animo {animoclassindex} {noOfSplatters} times");

            npc.DamageNPC(critter, basedamage, damagetype);           
        }




    }//end class
}//end namespace