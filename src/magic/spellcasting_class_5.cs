using System.Diagnostics;

namespace Underworld
{
    public partial class SpellCasting : UWClass
    {
        static int[] UW2MagicProjectileIDs = new int[] { 7, 5, 4, 6, 0xb, 0xc };//magicarrow,lightning,fireball,acid, homing dart,snowball
        static int[] UW1MagicProjectileIDs = new int[] { 7, 5, 4, 6 };// to confirm
        public static void CastMagicProjectile(uwObject caster, int minorclass)
        {
            int spellProjectileID;
            if (_RES == GAME_UW2)
            {
                spellProjectileID = UW2MagicProjectileIDs[minorclass - 1];
            }
            else
            {
                spellProjectileID = UW1MagicProjectileIDs[minorclass - 1];
            }

            var result = ProjectileSpell(caster, spellProjectileID);
            if (caster == playerdat.playerObject)
            {
                if (result)
                {
                    Debug.Print("Apply mana cost for sucessfully launched spell");
                }
                else
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_there_is_not_enough_room_to_release_that_spell_));
                }
                currentSpell = null;
                uimanager.instance.mousecursor.SetCursorToCursor(0);
            }
        }

        static bool ProjectileSpell(uwObject caster, int spellProjectileID)
        {
            motion.RangedAmmoItemID = spellProjectileID + 16;
            motion.RangedAmmoType = rangedObjectDat.ammotype(motion.RangedAmmoItemID);
            motion.projectileXHome = caster.npc_xhome;
            motion.projectileYHome = caster.npc_yhome;
            motion.spellXHome = caster.npc_xhome;
            motion.spellYHome = caster.npc_yhome;
            motion.MissileLauncherHeadingBase = 1;
            if (caster == playerdat.playerObject)
            {
                motion.InitPlayerProjectileValues();               
            }
            else
            {
                //npc or spell trap launcher
                if (caster.IsStatic)
                {                    
                    motion.projectileXHome = caster.tileX;
                    motion.projectileYHome = caster.tileY;                                    
                    motion.MissilePitch = 0;
                    motion.MissileLauncherHeadingBase = 0;
                }
                motion.MissileHeading = 0;
            }

            motion.MissileFlagB = true;
            var projectile = motion.PrepareProjectileObject(caster);
            motion.MissileFlagB = false;
            if (projectile != null)
            {
                objectInstance.RedrawFull(projectile);
                //Todo. Some sound effect logic
                return true;
            }
            else
            {
                return false;
            }
        }
    }//end class
}//end namespace

