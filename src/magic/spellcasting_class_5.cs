using System.Diagnostics;

namespace Underworld
{
    public partial class SpellCasting : UWClass
    {
        public static void PlayerCastsMagicProjectile()
        {
            uimanager.AddToMessageScroll("Woosh! Fired off a projectile");
            currentSpell = null;
            uimanager.instance.mousecursor.SetCursorToCursor(0);
        }

        public static void ObjectCastsMagicProjectile(uwObject caster, int minorclass)
        {
           Debug.Print($"Object {caster} has fired off projectile spell {minorclass}");
        }
    }//end class
}//end namespace
        
