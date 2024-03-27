using System.Diagnostics;

namespace Underworld
{
    public class a_spell_trap:trap
    {
        public static void Activate(uwObject triggerObj, uwObject trapObj, uwObject[] objList)
        {
            Debug.Print($"Spelltrap Casting spell {trapObj.quality} {trapObj.owner}");

            SpellCasting.CastSpell(
                majorclass: trapObj.quality, 
                minorclass: trapObj.owner, 
                caster: trapObj, 
                target: triggerObj, 
                tileX: trapObj.tileX, 
                tileY: trapObj.tileY, 
                CastOnEquip: false, 
                PlayerCast: false);
        }
    }//end class
}//end namespace