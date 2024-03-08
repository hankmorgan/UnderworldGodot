using System.Diagnostics;
using System.Numerics;
using Godot;

namespace Underworld
{

    public  partial class SpellCasting : UWClass
    {
        /// <summary>
        /// The current spell to cast after a delay.
        /// </summary>
        public static RunicMagic currentSpell;

        /// <summary>
        /// Logic for casting spells that are not from enchanted objects. Assumes all spell casting requirements have been met. 
        /// </summary>
        /// <param name="majorclass"></param>
        /// <param name="minorclass"></param>
        /// <param name="caster">Casting object (use if not the player casting)</param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static void CastSpell(int majorclass, int minorclass, uwObject caster, uwObject target, int tileX, int tileY, bool CastOnEquip, bool PlayerCast = true)
        {
            Debug.Print($"Casting {majorclass},{minorclass}");
            if (_RES==GAME_UW1)
            {
                if (playerdat.dungeon_level==8)
                {
                    return; //no casting in the ethereal void.
                }
            }
            //TODO if no magic allow bit return

            switch (majorclass)
            {
                //0-3 affect active player statuses
                //In this case use minorclass & 0xC0 as the spell stability class and minorclass & 0x3F as the minor class within the function.
                case 0://
                case 1://motion related
                case 2://
                case 3://
                    {
                        CastClass0123_Spells(majorclass, minorclass);
                        break;
                    }
                case 4://healing
                    CastClass4_Heal(minorclass);
                    break;
                case 5://projectile spells
                    {
                        currentSpell = new RunicMagic(majorclass,minorclass);
                        uimanager.instance.mousecursor.SetCursorToCursor(9);
                    }
                    break;
                case 6://spells that run code in an area around the player
                    break;
                case 7://spells with prompts and code callbacks.
                    {
                        currentSpell = new RunicMagic(majorclass,minorclass);
                        uimanager.instance.mousecursor.SetCursorToCursor(10);
                    }
                    break;
                case 8://spells that create or summon
                    CastClass8_Summoning(
                        minorclass: minorclass);
                    break;
                case 9://curses
                    CastClass9_Curse(
                        minorclass: minorclass,
                        CastOnEquip: CastOnEquip);
                    break;
                case 10://mana change spells
                    CastClass10_ManaBoost(minorclass);
                    break;
                case 11://misc or special spells
                    CastClassB_Spells(minorclass, minorclass & 0xC0);                    
                    break;
                case 12://not castable here
                    Debug.Print("Attempt to directly cast Class 0xC spell. This should not happen");
                    break;
                case 13://misc spells
                    break;
                case 14://cutscene spells.
                    break;
            }
        } 

        public static void CastCurrentSpellOnRayCastTarget(int index, uwObject[] objList)
        {
            if (currentSpell != null)
            {
                switch (currentSpell.SpellMajorClass)
                {
                    case 5://projectiles
                        //Casting on ray cast target should stop the spell from launching
                        uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_there_is_not_enough_room_to_release_that_spell_));
                        return;
                    case 7://click on object spells
                        CastClass7_Spells(
                            minorclass: currentSpell.SpellMinorClass, 
                            index: index, 
                            objList: objList);
                        currentSpell = null;
                        uimanager.instance.mousecursor.SetCursorToCursor();
                        break;
                }
            }
        }   

        public static void CastCurrentSpellAtPosition(Godot.Vector3 position) 
        {

        }

    }//end class
}//end namespace
