using System.Runtime.Serialization;

namespace Underworld
{
    /// <summary>
    /// For motion functions related to the creation and launching of projectiles
    /// </summary>
    public partial class motion : Loader
    {
        public static int AmmoItemID;
        public static int AmmoType;
        public static int spellXHome;
        public static int spellYHome;
        public static int MissileLauncherHeadingBase;

        public static int MissileHeading;
        public static int MissilePitch;

        public static bool MissileFlagA;
        public static bool MissileFlagB;


        public static bool InitPlayerProjectileValues()
        {
            //translate mouse x/y values into a pitch and heading for the projectile to follow
            if (_RES == GAME_UW2)
            {
                var X1 = (int)(uimanager.ViewPortMouseXPos / 4) - 16f;//ofset from the left side border
                var Y1 = (int)(200f - uimanager.ViewPortMouseYPos / 4) - 54f;

                if (X1 > 208)
                {
                    X1 = 208;
                }
                else
                {
                    if (X1 < 0)
                    {
                        X1 = 0;
                    }
                }

                if (Y1 > 128)
                {
                    Y1 = 128;
                }
                else
                {
                    if (Y1 < 0)
                    {
                        Y1 = 0;
                    }
                }

                MissileHeading = 1 + (int)(((X1 - 104f) * 5f) / 13f);
                //note missile pitch is initialised with a value from DSEG_33D6 but that value always appears to be 0.
                MissilePitch = (int)((Y1 - 64f) / 6f);
                if (Y1 < 42)
                {
                    return false;//drop action?
                }
                else
                {
                    return true;//throw action?
                }
            }
            return false;//todo UW1 version of the above.
        }

        public static uwObject PrepareProjectileObject(uwObject Launcher)
        {
            var slot = ObjectCreator.PrepareNewObject(AmmoItemID, ObjectFreeLists.ObjectListType.MobileList);
            if (slot != -1)
            {
                var projectile = UWTileMap.current_tilemap.LevelObjects[slot];
                //prep projectile.
                projectile.is_quant = 1;
                projectile.link = 1;  //?

                if (MissileLauncherHeadingBase != 0)
                {
                    MissileLauncherHeadingBase = Launcher.npc_heading;//todo account for the scenario where the launcher is a static
                }
                MissileLauncherHeadingBase = MissileLauncherHeadingBase + (Launcher.heading << 5);
                MissileLauncherHeadingBase = (MissileHeading + MissileLauncherHeadingBase + 0x100) & 0xFF;

                ObjectCreator.InitMobileObject(projectile, motion.spellXHome, motion.spellYHome);

                projectile.heading = (short)(MissileLauncherHeadingBase >> 5);
                projectile.npc_heading = (short)(MissileLauncherHeadingBase & 0x1F);
                projectile.ProjectileHeading = (ushort)MissileLauncherHeadingBase;
                projectile.doordir = 0;
                projectile.zpos = Launcher.zpos;
                projectile.xpos = Launcher.xpos;
                projectile.ypos = Launcher.ypos;

                var height = commonObjDat.height(Launcher.item_id);
                if (height != 0)
                {
                    projectile.zpos = (short)((((height * 5) & 0xFF) / 6) + projectile.zpos + (MissilePitch << 1));
                    if (Launcher == playerdat.playerObject)
                    {
                        //todo handle swimming player height adjustment
                    }
                }

                if (PlaceProjectileInWorld(projectile, Launcher, true))
                {
                    if (projectile.majorclass != 1)
                    {
                        projectile.CoordinateX = (Launcher.npc_xhome << 8) + (Launcher.xpos << 5) + 0xF;
                        projectile.CoordinateY = (Launcher.npc_yhome << 8) + (Launcher.ypos << 5) + 0xF;
                        projectile.CoordinateZ = Launcher.zpos << 3;
                        short ProjectileSourceID = 0;

                        if (Launcher.majorclass == 1)
                        {//launcher is an npc
                            if (!Launcher.IsStatic)
                            {
                                ProjectileSourceID = Launcher.index;
                            }
                        }
                        projectile.ProjectileSourceID = ProjectileSourceID;
                        projectile.UnkBit_0X15_Bit7 = 0;
                        projectile.UnkBit_0XA_Bit7 = 0;
                    }

                    projectile.Projectile_Pitch = (short)(MissilePitch + 0x10);
                    projectile.Projectile_Speed = 1;
                    projectile.UnkBit_0X13_Bit0to6 = (short)AmmoType;

                    if (commonObjDat.canhaveowner(AmmoType))
                    {
                        projectile.owner = 0;
                    }

                    var tileX = projectile.npc_xhome;
                    var tileY = projectile.npc_yhome;

                    projectile.tileX = tileX;
                    projectile.tileY = tileY;

                    var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                    projectile.next = tile.indexObjectList;
                    tile.indexObjectList = projectile.index;

                    if (MissileFlagB && MissileFlagA)
                    {
                        //TODO some logic around sound effects.
                    }

                    return projectile;
                }
                else
                {
                    //cannot place
                    ObjectFreeLists.ReleaseFreeObject(projectile);
                    return null;
                }
            }
            return null;
        }




        public static bool PlaceProjectileInWorld(uwObject projectile, uwObject launcher, bool ProjectFromLauncher)
        {
            MotionCalcArray.PtrToMotionCalc = new byte[0x20];
            MotionCalcArray.MotionArrayObjectIndexA = (short)projectile.item_id;
            MotionCalcArray.Radius8_base = (byte)commonObjDat.radius(projectile.item_id);
            MotionCalcArray.Height9 = (byte)commonObjDat.height(projectile.item_id);
            MotionCalcArray.x0 = (ushort)((projectile.npc_xhome << 3) + projectile.xpos);
            MotionCalcArray.y2 = (ushort)((projectile.npc_yhome << 3) + projectile.ypos);
            if (ProjectFromLauncher)
            {
                int X = MotionCalcArray.x0; int Y = MotionCalcArray.y2;
                GetCoordinateInDirection(projectile.ProjectileHeading, commonObjDat.radius(projectile.item_id), ref X, ref Y);
                MotionCalcArray.x0 = (ushort)X; MotionCalcArray.y2 = (ushort)Y;
            }
            MotionCalcArray.z4 = (ushort)projectile.zpos;
            ScanForCollisions_seg028_2941_C0E(1, 0);
            ProcessMotionTileHeights_seg028_2941_385(0);

            if (((short)MotionCalcArray.Unk14_collisoncount | (short)MotionCalcArray.Unk12) == 0)
            {
                //no collisions.
                if (MotionCalcArray.Unk14_collisoncount != 0)
                {
                    SortCollisions_seg028_2941_ED8();
                    if (MotionCalcArray.Unk15 != 0)
                    {
                        return false;
                    }
                }
                projectile.npc_xhome = (short)(MotionCalcArray.x0 >> 3);
                projectile.npc_yhome = (short)(MotionCalcArray.y2 >> 3);
                projectile.xpos = (short)(MotionCalcArray.x0 & 0x7);
                projectile.ypos = (short)(MotionCalcArray.y2 & 0x7);
                //this ignores zpos.
                return true;

            }
            else
            {
                //the object is going to be blocked by a collision
                return false;
            }

        }

        public static void GetCoordinateInDirection(int heading, int distance, ref int X0, ref int Y0)
        {
            var si_h = (short)(((0x140 - heading) & 0xFF) << 8);
            int xvar4 = 0; int yvar2 = 0;
            GetVectorForDirection(heading, ref xvar4, ref yvar2);
            xvar4 = ((X0 / 0x80) * distance) / 0x100;
            yvar2 = ((Y0 / 0x80) * distance) / 0x100;

            if (yvar2 <= 0)
            {
                if (yvar2 <= 0)
                {
                    yvar2--;
                }
            }
            else
            {
                yvar2++;
            }

            if (xvar4 <= 0)
            {
                if (xvar4 >= 0)
                {
                    xvar4--;
                }
            }
            else
            {
                xvar4++;
            }

            X0 = X0 + xvar4;
            Y0 = Y0 + yvar2;
        }

        public static void GetVectorForDirection(int heading, ref int X1, ref int Y1)
        {
            X1 = xvectors[heading >> 8];
            Y1 = yvectors[heading >> 8];
        }

    }//end class
}//end namespace