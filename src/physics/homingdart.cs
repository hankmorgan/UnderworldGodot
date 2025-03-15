using System;

namespace Underworld
{
    public partial class motion : Loader
    {
        static short TargetForHomingDart = -1;
        static short DistanceToHomingDartTarget = 0x7FFF;

        static short HomingDartXCoord;
        static short HomingDartYCoord;

        /// <summary>
        /// Searches of homing dart target and aims dart accordingly.
        /// </summary>
        /// <param name="projectile"></param>
        static void HomingDart(uwObject projectile)
        {
            short var8 = 3;
            short var6 = 3;

            var var2_x = (short)(projectile.npc_xhome - 1);
            var var4_y = (short)(projectile.npc_yhome - 1);

            var si = (short)(((projectile.ProjectileHeading + 0x20) & 0xFF) >> 6);
            var di = (short)(((projectile.heading + 1) & 0x7) >> 1);

            if (di > 1)
            {
                if (di == 3)
                {
                    var2_x--;
                }
                else
                {
                    var4_y--;
                }
            }
            if ((di & 1) == 0)
            {
                var8++;
            }
            else
            {
                var6++;
            }

            TargetForHomingDart = -1;
            DistanceToHomingDartTarget = 0x7FFF;
            HomingDartXCoord = (short)((projectile.npc_xhome << 3) + projectile.xpos);
            HomingDartYCoord = (short)((projectile.npc_yhome << 3) + projectile.ypos);

            CallBacks.RunCodeOnTargetsInArea(
                methodToCall: HomingDartTargeting, 
                Rng_arg0: 1, 
                srcItemIndex: projectile.ProjectileSourceID, 
                typeOfTargetArg8: 0, 
                tileX0: var2_x, tileY0: var4_y, 
                xWidth: var6, 
                yHeight: var8);

            if (TargetForHomingDart > 0)
            {
                //target found vector to target
                var targetObject = UWTileMap.current_tilemap.LevelObjects[TargetForHomingDart];
                var2_x = (short)((targetObject.npc_xhome << 3) + targetObject.xpos);
                var4_y = (short)((targetObject.npc_yhome << 3) + targetObject.ypos);
// var4_y = 0x104;//test case
// var2_x = 0x109;
// HomingDartYCoord = 0xF5;
// HomingDartXCoord = 0xF3;
// DistanceToHomingDartTarget = 0x25;
                var varE = SatelliteOrDartVectoring(HomingDartXCoord, HomingDartYCoord, var2_x, var4_y);//This returns wrong value in some circumstances. TO FIX
                if (DistanceToHomingDartTarget >= 0xC)
                {
                    if (DistanceToHomingDartTarget >= 0x20)
                    {
                        si = 6;
                    }
                    else
                    {
                        si = 4;
                    }
                }
                else
                {
                    si = 2;
                }

                projectile.ProjectileHeading = (ushort)GetDefelectionMaybe((short)projectile.ProjectileHeading, (short)(varE >> 8), (short)((si + 8) << 2));

                var var12_pitch = projectile.Projectile_Pitch;
                
                var var14_heightdiff = (targetObject.zpos - projectile.zpos);                
                var var16 = 0x10 + ((ushort)var14_heightdiff / 0x18);

                projectile.Projectile_Pitch = (short)((short)(((var12_pitch * (si + 4) + var16 * (8 - si)) / 0xC))  & 0x1F);
            }
            else
            {
                //no target found
                projectile.ProjectileHeading = (ushort)((projectile.ProjectileHeading + (Rng.r.Next(0x7FFF) & 0x7) - 3) & 0xFF);
                var varA_pitch = projectile.Projectile_Pitch;
                if ((varA_pitch & 0x4) == 0)
                {//height oscillation of the projectile.
                    if (varA_pitch >= 0xE)
                    {
                        if (varA_pitch <= 0x12)
                        {
                            //rnd
                            varA_pitch = (short)(varA_pitch + (Rng.r.Next(0x7FFF) & 0x3) - 1);
                        }
                        else
                        {
                            varA_pitch--;
                        }
                    }
                    else
                    {
                        varA_pitch++;
                    }
                }
                projectile.Projectile_Pitch = varA_pitch;
            }


            //seg006_1413_61B
            if ((Rng.r.Next(0x7fff) & 2) == 0)
            {
                //spawn vapour trail
                animo.SpawnAnimoInTile(0xE, projectile.xpos, projectile.ypos, projectile.zpos, projectile.tileX, projectile.tileY);
            }

            if (projectile.UnkBit_0XA_Bit7 == 0)
            {
                if ((Rng.r.Next(0x7FFF) & 3) == 0)
                {
                    if (projectile.npc_hp > 0)
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


        /// <summary>
        /// Checks if object is a closer target to the dart than the one already selected.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="obj"></param>
        /// <param name="tile"></param>
        /// <param name="srcIndex"></param>
        /// <returns></returns>
        public static bool HomingDartTargeting(int x, int y, uwObject obj, TileInfo tile, int srcIndex)
        {
            var targetXCoord = (obj.npc_xhome << 3) + obj.xpos;
            var targetYCoord = (obj.npc_yhome << 3) + obj.ypos;

            var si_dist = (short)(Math.Abs(targetXCoord- HomingDartXCoord) + Math.Abs(targetYCoord-HomingDartYCoord));

            if (si_dist < DistanceToHomingDartTarget)
            {
                TargetForHomingDart = obj.index;
                DistanceToHomingDartTarget = si_dist;
            }

            return false;
        }
    }//end class
}//end namespace