using System.Diagnostics;

namespace Underworld
{
    public class a_spell_trap:trap
    {
        public static void Activate(int character, uwObject trapObj, uwObject[] objList)
        {
            Debug.Print($"Spelltrap Casting spell {trapObj.quality} {trapObj.owner}");
            var target = objList[character];
            SpellCasting.CastSpell(
                majorclass: trapObj.quality, 
                minorclass: trapObj.owner, 
                caster: trapObj, 
                target: target, 
                tileX: trapObj.tileX, 
                tileY: trapObj.tileY, 
                CastOnEquip: false, 
                PlayerCast: false);
        }
    }//end class
}//end namespace