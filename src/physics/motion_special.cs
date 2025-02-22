using System.Diagnostics;

namespace Underworld
{    
    /// <summary>
    /// Handle quest and special events that are related to motion. Eg throwing basilisk oil into mud, talismans into lava, projectile detonation
    /// </summary>

    public partial class motion: Loader
    {

        static void OilOnMud()
        {
            Debug.Print("UW2 Handle oil on Mud");
        }

        static bool DetonateProjectile(uwObject projectile, int effectId)
        {
            Debug.Print("Detonate projectile");
            return true;
        }
    }//end class
}//end namespace