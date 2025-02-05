namespace Underworld
{
        /// <summary>
        /// A class based implementation of an array of motion params that UW uses for projectile calcs
        /// </summary>
        public class UWMotionParamArray
        {
            //globals
            public static int relatedtoheadinginMotion_dseg_67d6_25CA;
            public static int Likely_RadiusInMotion_dseg_67d6_25CC;
            public static int Likely_HeightInMotion_dseg_67d6_25CD;
            public static int MotionArrayObjectIndex_dseg_67d6_25CE;            
            public static int RelatedToMotionX_dseg_67d6_3FE;
            public static int RelatedToMotionY_dseg_67d6_400;
            public static int RelatedToMotionZ_dseg_67d6_402;
            public static int[] dseg_67d6_404 = new int[2];
            public static int dseg_67d6_410;
            public static int dseg_67d6_412;
            public static int GravityCollisionRelated_dseg_67d6_414;
            public static int MotionGlobal_dseg_67d6_40A_indexer;
            public static int dseg_67d6_40C_indexer;
            public static int MAYBEcollisionOrGravity_dseg_67d6_40E;            

            public int x_0;
            public int y_2;
            public int z_4;

            public int unk_6;
            public int unk_8;

            public int unk_a;
            public int unk_c;
            public int unk_d;
            public int unk_10;
            public int speed_12;
            public int pitch_13;
            public int unk_14;

            public int unk_16;
            public int mass_18;
            public int unk_1a;
            public int hp_1b;
            public int scaleresistances_1C;
            public int heading_1E;
            public int unk_1d;
            public int index_20;
            public int radius_22;
            public int height_23;
            public int unk_24;
            public int unk_25;

            public OtherMotionArray dseg_67d6_3FC_ptr_to_25C4_maybemotion = new OtherMotionArray();


            /// <summary>
            /// Provides a look up into values starting at offset 6. To replicate vanilla array access while staying strongly typed.
            /// </summary>
            /// <param name="offset"></param>
            /// <returns></returns>
            public int GetParam6(int offset)
            {
                switch (offset)
                {
                    case 0:
                        return unk_6;
                    case 1:
                        return unk_8;
                }
                return 0;
            }
        }


        public class OtherMotionArray
        {
            public int Unk0_x;
            public int Unk2_y;
            public int Unk4_z;
        }
}