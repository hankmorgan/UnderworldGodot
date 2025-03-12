using System;

namespace Underworld
{
    public partial class motion : Loader
    {
        /// <summary>
        /// Handles the rotation of the satellite spell projectile around it's caster
        /// </summary>
        /// <param name="projectile"></param>
        static void Satellite(uwObject projectile)
        {
            var caster = UWTileMap.current_tilemap.LevelObjects[projectile.ProjectileSourceID];
            var casterXCoord = (caster.npc_xhome << 3) + caster.xpos;
            var casterYCoord = (caster.npc_yhome << 3) + caster.ypos;

            var ProjXCoord = (projectile.npc_xhome << 3) + projectile.xpos;
            var ProjYCoord = (projectile.npc_yhome << 3) + projectile.ypos;
            var di = SatelliteVectoring(ProjXCoord, ProjYCoord, casterXCoord, casterYCoord);
            var CalcedHeading_1C = (short)(di + 0x4000);
            var si_projectileheading = (short)Math.Abs(projectile.ProjectileHeading);
            if (si_projectileheading > 0x80)
            {
                si_projectileheading -= 0xFF;
            }
            if (si_projectileheading <= 0x40)
            {
                CalcedHeading_1C = (short)(CalcedHeading_1C - 32768);
            }

            var varA_distanceapart = (short)((short)Math.Abs(ProjXCoord - casterXCoord) + (short)Math.Abs(ProjYCoord - casterYCoord));
            var var1E = 2;
            if (varA_distanceapart >= 4)
            {
                var1E = 8;
            }

            var varC_orbit = (short)((((short)Math.Abs(varA_distanceapart - 4)) << 3) / var1E);
            if (varC_orbit > 0x40)
            {
                varC_orbit = 0x40;
            }

            var var1A_diff = GetRelativeHeadingByMagnitude(projectile.ProjectileHeading, CalcedHeading_1C, 0x20);

            var1A_diff = GetRelativeHeadingByMagnitude(var1A_diff, di>>8, varC_orbit);

            projectile.ProjectileHeading = (ushort)var1A_diff;
           
           
            var pitch_varE = projectile.Projectile_Pitch;
            var var10_zDiff =  0xF + (caster.zpos - projectile.zpos);
            var var12 = 0x10 + (var10_zDiff / 8);

            var newPitch_var14 = (pitch_varE + var12) /2;

            if ((Rng.r.Next(0x7FFF) & 0x3) == 0)
            {
                if (newPitch_var14>0xF)
                {
                    if(newPitch_var14>0x11)
                    {
                        newPitch_var14--;
                    }
                    else
                    {
                        newPitch_var14 = newPitch_var14 + Rng.r.Next(4) - 1;
                    }
                }
                else
                {
                    newPitch_var14++;
                }
            }

            projectile.Projectile_Pitch = (short)newPitch_var14;

            if (projectile.UnkBit_0X13_Bit0to6 < 0xF)
            {
                projectile.UnkBit_0X13_Bit0to6 = 0xF;
            }

            if (projectile.UnkBit_0XA_Bit7 == 0)
            {
                if (Rng.r.Next(32) == 1)
                {
                    if (projectile.npc_hp>0)
                    {
                        projectile.npc_hp--;
                    }
                }
            }
            else
            {   
                projectile.npc_hp = 0;
            }
        }


        static short SatelliteVectoring(int projectileX, int projectileY, int sourceX, int sourceY)
        {
            return 0;
        }


        static short GetRelativeHeadingByMagnitude(int srcHeading, int dstHeading, int Magnitude)
        {
            return 0;
        }
    }//end class
}//end namespace