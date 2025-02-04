namespace Underworld
{    
    public partial class motion:UWClass
    {

        /// <summary>
        /// Initialises the params needed for calculating object motion.
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="MotionParams"></param>
        public static void InitMotionParams(uwObject projectile, motionarray MotionParams)
        {           
            bool isNPC = true;
            var itemid = projectile.item_id;

            //Common items
            MotionParams.index_20 = projectile.index;
            MotionParams.mass_18 = commonObjDat.mass(itemid);
            MotionParams.unk_1a = commonObjDat.unk6_4(itemid);
            MotionParams.unk_16 = commonObjDat.unk6_5678(itemid);
            MotionParams.scaleresistances_1C = commonObjDat.scaleresistances(itemid);
            MotionParams.unk_1d = 0;
            MotionParams.heading_1E = projectile.heading<<0xD;
            MotionParams.unk_24 = 0;
            MotionParams.radius_22 = commonObjDat.radius(itemid);
            MotionParams.height_23 = commonObjDat.height(itemid);
            MotionParams.x_0 = projectile.xpos;
            MotionParams.y_2 = projectile.ypos;
            MotionParams.z_4 = projectile.zpos;

            if (projectile.IsStatic)
            {//Not sure when a static projectile will hit this but including for completedness. (possibly collisions?)
                MotionParams.unk_a = 0;
                MotionParams.unk_10 = 0;
                MotionParams.unk_14 = 0;
                MotionParams.hp_1b = projectile.quality;
                MotionParams.x_0 += (projectile.tileX<<3);
                MotionParams.y_2 += (projectile.tileY<<3);
            }
            else
            {
                MotionParams.x_0 += (projectile.npc_xhome<<3);
                MotionParams.y_2 += (projectile.npc_yhome<<3);
                MotionParams.heading_1E = projectile.ProjectileHeading<<8;
                MotionParams.unk_25 = 1<< projectile.UnkBit_0XA_Bit456;
                MotionParams.pitch_13 = (projectile.Projectile_Pitch - 16) << 6;
                MotionParams.unk_10 = projectile.UnkBit_0X13_Bit7 * -4;
                MotionParams.hp_1b = projectile.npc_hp;  

                if (projectile.majorclass != 1)
                {
                    isNPC = false;
                    MotionParams.x_0 = projectile.CoordinateX;
                    MotionParams.y_2 = projectile.CoordinateY;
                    MotionParams.z_4 = projectile.CoordinateZ;
                }
                MotionParams.unk_14 = projectile.UnkBit_0X13_Bit0to6;
            }


            if (
                (projectile.majorclass!=1)
                &&
                (commonObjDat.maybeMagicObjectFlag(itemid) == false)
                &&
                ((MotionParams.unk_a | MotionParams.unk_10) == 0)
            )
            {
                if  (2+(MotionParams.unk_1a<<1) < MotionParams.unk_14)
                {
                    MotionParams.unk_14 = 0;
                }
                else
                {
                    MotionParams.unk_14 = projectile.UnkBit_0X13_Bit0to6 * (0x29 + (MotionParams.unk_1a<<2));
                    if (projectile.majorclass == 1)
                    {
                        MotionParams.unk_24 = 8;
                    }
                }
            }
            else
            {//seg030_2BB7_5F8
                MotionParams.unk_14 = MotionParams.unk_14 * 0x2F;
            }


            if (isNPC)
            {
                MotionParams.x_0 = (MotionParams.x_0<<5) + Rng.r.Next(32);
                MotionParams.y_2 = (MotionParams.y_2<<5) + Rng.r.Next(32);
                MotionParams.z_4 = (MotionParams.z_4<<3) + Rng.r.Next(8);
            }             
        }


    }//end class
}//end namespace