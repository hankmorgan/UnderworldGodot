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

        static void StudyMonster(int index, uwObject[] objList)
        {
            Debug.Print("STUDY MONSTER");
        }
    }//end class
}//end namespace