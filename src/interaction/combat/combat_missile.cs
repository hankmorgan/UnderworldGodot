
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Class for interactions involving missile combat and projectile spell hits
    /// </summary>
    public partial class combat : UWClass
    {
        public static void MissileImpact(uwObject projectile, uwObject objectHit)
        {
            var diDamageMultipler = 1;
            Debug.Print($"Missile impact {projectile.a_name} on {objectHit.a_name}");
            if (_RES==GAME_UW2 && projectile.item_id == 0x1E)
            {
                //projectile is a UW2 Satellite
                if (projectile.ProjectileSourceID == objectHit.index)
                {
                    //satellite has hit it's caster
                    projectile.UnkBit_0X15_Bit7 = 0;
                    return ;
                }
            }

            var MissileDamage = rangedObjectDat.damage(projectile.item_id);
            if (projectile.ProjectileSourceID == 1)
            {
                //player hds launched the projectile
                if (rangedObjectDat.RangedWeaponType(projectile.item_id) == 0xC0)
                {
                    diDamageMultipler = (playerdat.Missile<<3) + 0xC0;
                    var SkillCheckResult = playerdat.SkillCheck(playerdat.Missile, 0xA);
                    switch (SkillCheckResult)
                    {
                        case playerdat.SkillCheckResult.CritFail:
                            diDamageMultipler -= 0x80;
                            break;
                        case playerdat.SkillCheckResult.CritSucess:
                            diDamageMultipler += 0xC0;
                            break;
                        default:
                            break;
                    }
                    MissileDamage = (MissileDamage * diDamageMultipler) >> 8;
                }
            }

            int var4_x;
            int var6_y;

            if (UWMotionParamArray.dseg_67d6_25BC==0)
            {
                var4_x = UWMotionParamArray.UnknownX_dseg_67d6_25BD;
                var6_y = UWMotionParamArray.UnknownY_dseg_67d6_25BE;
            }
            else
            {
                var4_x = UWMotionParamArray.dseg_67d6_25BF_X;
                var6_y = UWMotionParamArray.dseg_67d6_25C0_Y;
            }

            MissileAttackHit(
                projectileSource: projectile.ProjectileSourceID, 
                Projectile: projectile, 
                objectHit: objectHit, 
                X: var4_x, Y: var6_y, 
                damage: MissileDamage, 
                damageType: (byte)-rangedObjectDat.RangedWeaponType(projectile.item_id));

            if (objectHit.majorclass == 1)
            {
                projectile.UnkBit_0XA_Bit7 = 1;
            }

        }


        /// <summary>
        /// Applies the missile hit
        /// </summary>
        /// <param name="projectileSource"></param>
        /// <param name="Projectile"></param>
        /// <param name="objectHit"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="damage"></param>
        /// <param name="damageType"></param>
        static void MissileAttackHit(int projectileSource, uwObject Projectile, uwObject objectHit, int X, int Y, int damage, byte damageType)
        {
            Debug.Print("Missile Attack Hit");
            CombatHitTileX = X; CombatHitTileY = Y;
            AttackWasACrit = false;
            AttackScoreFlankingBonus = 0;
            BodyPartHit = PickBodyHitPoint(
                defenderZ: objectHit.zpos, 
                defenderTop: commonObjDat.height(objectHit.item_id) + objectHit.zpos, 
                attackerZ: Projectile.zpos, 
                attackerTop: commonObjDat.height(Projectile.item_id));

            AttackDamage = damage;
            FinalAttackCharge = 0x80;

            if (objectHit == playerdat.playerObject)
            {
                //todo sound effect at player position
            }
            else
            {
                //todo sound effect at hit position.
            }
            AttackerAppliesFinalDamage(
                attacker: projectileSource,
                damageType: damageType, 
                MissileAttack: true);
            
        }

    }//end namespace
}//end class