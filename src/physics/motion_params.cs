namespace Underworld
{
    /// <summary>
    /// A class based implementation of an array of motion params that UW uses for projectile calcs
    /// </summary>
    public class UWMotionParamArray: Loader
    {
        public byte[] data = new byte[0x26];
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

        public static int dseg_67d6_2584;

        //The class properties
        public int x_0
        {
            get
            {
                return (int)DataLoader.getAt(data, 0, 16);
            }
            set
            {
                DataLoader.setAt(data, 0, 16, value);
            }
        }
        public int y_2
        {
            get
            {
                return (int)DataLoader.getAt(data, 2, 16);
            }
            set
            {
                DataLoader.setAt(data, 2, 16, value);
            }
        }
        public int z_4
        {
            get
            {
                return (int)DataLoader.getAt(data, 4, 16);
            }
            set
            {
                DataLoader.setAt(data, 4, 16, value);
            }
        }

        public int unk_6
        {
            get
            {
                return (int)DataLoader.getAt(data, 6, 16);
            }
            set
            {
                DataLoader.setAt(data, 6, 16, value);
            }
        }
        public int unk_8
        {
            get
            {
                return (int)DataLoader.getAt(data, 8, 16);
            }
            set
            {
                DataLoader.setAt(data, 8, 16, value);
            }
        }

        public int unk_a
        {
            get
            {
                return (int)DataLoader.getAt(data, 0xA, 16);
            }
            set
            {
                DataLoader.setAt(data, 0xA, 16, value);
            }
        }
        public int unk_c
        {
            get
            {
                return (int)DataLoader.getAt(data, 0xC, 16);
            }
            set
            {
                DataLoader.setAt(data, 0xC, 16, value);
            }
        }
        public int unk_d
        {
            get
            {
                return (int)DataLoader.getAt(data, 0xD, 16);
            }
            set
            {
                DataLoader.setAt(data, 0xD, 16, value);
            }
        }
        public int unk_10
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x10, 16);
            }
            set
            {
                DataLoader.setAt(data, 0x10, 16, value);
            }
        }
        public int speed_12
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x12, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x12, 8, value);
            }
        }
        public int pitch_13
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x13, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x13, 8, value);
            }
        }
        public int unk_14
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x14, 16);
            }
            set
            {
                DataLoader.setAt(data, 0x14, 16, value);
            }
        }
        public int unk_16
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x16, 16);
            }
            set
            {
                DataLoader.setAt(data, 0x16, 16, value);
            }
        }
        public int mass_18
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x18, 16);
            }
            set
            {
                DataLoader.setAt(data, 0x18, 16, value);
            }
        }
        public int unk_1a
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x1A, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x1A, 8, value);
            }
        }
        public int hp_1b
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x1B, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x1B, 8, value);
            }
        }
        public int scaleresistances_1C
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x1C, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x1C, 8, value);
            }
        }
        public int heading_1E
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x1E, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x1E, 8, value);
            }
        }
        public int unk_1d
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x1D, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x1D, 8, value);
            }
        }

        public int index_20
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x20, 16);
            }
            set
            {
                DataLoader.setAt(data, 0x20, 16, value);
            }
        }
        public int radius_22
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x22, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x22, 8, value);
            }
        }
        public int height_23
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x23, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x23, 8, value);
            }
        }
        public int unk_24
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x24, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x24, 8, value);
            }
        }
        public int unk_25
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x25, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x25, 8, value);
            }
        }

        public OtherMotionArray SubArray = new OtherMotionArray();

        public static int[] TileAttributesArray;
        public static TileInfo TileRelatedToMotion_dseg_67d6_257E;

        

        // /// <summary>
        // /// Provides a look up into values starting at offset 6. To replicate vanilla array access while staying strongly typed.
        // /// </summary>
        // /// <param name="offset"></param>
        // /// <returns></returns>
        // public int GetParam6(int offset)
        // {
        //     switch (offset)
        //     {
        //         case 0:
        //             return unk_6;
        //         case 1:
        //             return unk_8;
        //     }
        //     return 0;
        // }
    }


    public class OtherMotionArray:UWClass
    {

        public byte[] data = new byte[0x20];
        public int Unk0_x//dseg_2562
        {
            get
            {
                return (int)DataLoader.getAt(data, 0, 16);
            }
            set
            {
                DataLoader.setAt(data, 0, 16, value);
            }
        }

        public int Unk2_y//dseg_2564
        {
            get
            {
                return (int)DataLoader.getAt(data, 2, 8);
            }
            set
            {
                DataLoader.setAt(data, 2, 8, value);
            }
        }
        public int Unk3_z//dseg_2565
        {
            get
            {
                return (int)DataLoader.getAt(data, 3, 8);
            }
            set
            {
                DataLoader.setAt(data, 3, 8, value);
            }
        }

        public int Unk4//dseg_2566
        {
            get
            {
                return (int)DataLoader.getAt(data, 4, 8);
            }
            set
            {
                DataLoader.setAt(data, 4, 8, value);
            }
        }
        public int Unk5//dseg_2567
        {
            get
            {
                return (int)DataLoader.getAt(data, 5, 16);
            }
            set
            {
                DataLoader.setAt(data, 5, 16, value);
            }
        }
        public int Unk7//dseg_2569
        {
            get
            {
                return (int)DataLoader.getAt(data, 7, 8);
            }
            set
            {
                DataLoader.setAt(data, 7, 8, value);
            }
        }
        public int Unk8//dseg_256a
        {
            get
            {
                return (int)DataLoader.getAt(data, 8, 8);
            }
            set
            {
                DataLoader.setAt(data, 8, 8, value);
            }
        }
        public int Unk9//dseg_256B
        {
            get
            {
                return (int)DataLoader.getAt(data, 9, 8);
            }
            set
            {
                DataLoader.setAt(data, 9, 8, value);
            }
        }
        public int UnkA//dseg 256C              
        {
            get
            {
                return (int)DataLoader.getAt(data, 0xA, 16);
            }
            set
            {
                DataLoader.setAt(data, 0xA, 16, value);
            }
        }
        public int Unkc_terrain
        {
            get
            {
                return (int)DataLoader.getAt(data, 0xC, 8);
            }
            set
            {
                DataLoader.setAt(data, 0xC, 8, value);
            }
        }
        public int UnkD
        {
            get
            {
                return (int)DataLoader.getAt(data, 0xD, 8);
            }
            set
            {
                DataLoader.setAt(data, 0xD, 8, value);
            }
        }
        public int UnkE
        {
            get
            {
                return (int)DataLoader.getAt(data, 0xE, 8);
            }
            set
            {
                DataLoader.setAt(data, 0xE, 8, value);
            }
        }
        public int UnkF
        {
            get
            {//to confirm is this a byte or a word?
                return (int)DataLoader.getAt(data, 0xF, 8);
            }
            set
            {
                DataLoader.setAt(data, 0xF, 8, value);
            }
        }

        public int Unk10
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x10, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x10, 8, value);
            }
        }

        public int Unk11
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x11, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x11, 8, value);
            }
        }
        public int Unk12
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x12, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x12, 8, value);
            }
        }
        public int Unk13
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x13, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x13, 8, value);
            }
        }        
        public int Unk14
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x14, 16);
            }
            set
            {
                DataLoader.setAt(data, 0x14, 16, value);
            }
        }
        public int Unk16
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x16, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x16, 8, value);
            }
        }
        public int Unk17_xpos
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x17, 8);
            }
            set
            {
                DataLoader.setAt(data, 0x17, 8, value);
            }
        }
        public int Unk18_ypos
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x18, 16);
            }
            set
            {
                DataLoader.setAt(data, 0x18, 16, value);
            }
        }
        public int Unk1A
        {
            get
            {
                return (int)DataLoader.getAt(data, 0x1A, 16);
            }
            set
            {
                DataLoader.setAt(data, 0x1A, 16, value);
            }
        }
    }//end class
}//end namespace