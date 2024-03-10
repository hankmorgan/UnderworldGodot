using System.Diagnostics;
using Godot;

namespace Underworld
{
    /// <summary>
    /// To replace objectloader
    /// </summary>
    public class uwObject : UWClass
    {
        public objectInstance instance;//TODO: this needs to be linked in all the object creation code.
        public short index;

        public string a_name
        {
            get
            {
                if (majorclass == 1)
                {
                    if (npc_whoami != 0)
                    {
                        return GameStrings.GetString(7, npc_whoami + 16);
                    }
                }
                return GameStrings.GetObjectNounUW(item_id);
            }
        }

        /// <summary>
        /// Check if the object should only have the base 4 bytes of static info.
        /// </summary>
        public bool IsStatic;

        /// <summary>
        /// Reference to the data location for this object. Will be either tilemap or player.dat inventory data.
        /// </summary>
        public byte[] DataBuffer;

        public bool isInventory = false;

        public int PTR;

        //The tiles these objects are in.
        public int tileX = 99;
        public int tileY = 99;

        public int majorclass
        {
            get
            {
                return item_id >> 6;
            }
        }

        //= obj.item_id >> 6;
        public int minorclass
        {
            get
            {
                return (item_id & 0x30) >> 4;
            }
        }

        public int classindex
        {
            get
            {
                return item_id & 0xF;
            }
        }

        public int OneF0Class
        {
            get
            {
                return (item_id & 0x1F0) >> 4;
            }
        }


        //******************Util ****************//



        byte GetAt(int index)
        {
            return DataBuffer[index];
        }

        int GetAt16(int index)
        {
            return (int)Loader.getAt(DataBuffer, index, 16);
        }

        int GetAt32(int index)
        {
            return (int)Loader.getAt(DataBuffer, index, 32);
        }

        void SetAt(int index, byte value)
        {
            DataBuffer[index] = value;
        }

        void SetAt16(int index, int value)
        {
            Loader.setAt(DataBuffer, index, 16, value);
        }

        void SetAt32(int index, int value)
        {
            Loader.setAt(DataBuffer, index, 32, value);
        }


        //***********************Properties*********************//

        /// <summary>
        /// Indentifier of what the object is.
        /// </summary>
        public int item_id
        {//0-8
            get
            {
                int val = (int)GetAt16(PTR);
                return DataLoader.ExtractBits(val, 0, 0x1FF);
            }
            set
            {
                int existingValue = GetAt16(PTR);
                existingValue &= 0xFE00; //Mask out current val
                SetAt16(PTR, existingValue | ((value & 0x1FF) << 0));
            }
        }

        /// <summary>
        /// Various Object Flags
        /// </summary>
        public short flags
        {//; //9-11
            get
            {
                int val = (int)GetAt16(PTR);
                return (short)DataLoader.ExtractBits(val, 9, 0x7);
            }
            set
            {
                int existingValue = GetAt16(PTR);
                existingValue &= 0xF1FF; //Mask out current val
                SetAt16(PTR, existingValue | ((value & 0x7) << 9));
            }
        }

        /// <summary>
        /// Flag shortcuts - Flag 0
        /// </summary>
        public short flags0
        {
            get 
            {
                return (short)(flags & 0x1);
            }
        }

        /// <summary>
        /// Flag shortcuts - Flag 1
        /// </summary>
        public short flags1
        {
            get 
            {
                return (short)((flags>>1) & 0x1);
            }
        }

        /// <summary>
        /// Flag shortcuts - Flag 2
        /// </summary>
        public short flags2
        {
            get 
            {
                return (short)((flags>>2) & 0x1);
            }
        }

        /// <summary>
        /// The enchantment flag for the object.
        /// </summary>
        public short enchantment
        {//;   //12  (short)(ExtractBits(Vals[0], 12, 1));
            get
            {
                int val = (int)GetAt16(PTR);
                return (short)DataLoader.ExtractBits(val, 12, 1);
            }
            set
            {
                int existingValue = GetAt16(PTR);
                existingValue &= 0xEFFF; //Mask out current val
                SetAt16(PTR, existingValue | ((value & 0x1) << 12));
            }
        }

        public short doordir
        {//;   //13 // (short)(ExtractBits(Vals[0], 13, 1))
            get
            {
                int val = (int)GetAt16(PTR);
                return (short)DataLoader.ExtractBits(val, 13, 1);
            }
            set
            {
                int existingValue = GetAt16(PTR);
                existingValue &= 0xDFFF; //Mask out current val
                SetAt16(PTR, existingValue | ((value & 0x1) << 13));
            }
        }

        public short invis     //14
        {//(short)(ExtractBits(Vals[0], 14, 1));
            get
            {
                int val = (int)GetAt16(PTR);
                return (short)DataLoader.ExtractBits(val, 14, 1);
            }
            set
            {
                int existingValue = GetAt16(PTR);
                existingValue &= 0xBFFF; //Mask out current val
                SetAt16(PTR, existingValue | ((value & 0x1) << 14));
            }
        }

        public short is_quant  //15
        {
            get
            {
                int val = (int)GetAt16(PTR);
                return (short)DataLoader.ExtractBits(val, 15, 1);
            }
            set
            {
                int existingValue = GetAt16(PTR);
                existingValue &= 0x7FFF; //Mask out current val
                SetAt16(PTR, existingValue | ((value & 0x1) << 15));
            }
        }

        public int Obsolete_texture; // Note: some objects don't have flags and use the whole lower byte as a texture number
                                     //(gravestone, picture, lever, switch, shelf, bridge, ..)
                                     //This variable is uses for shorthand usage of this property
                                     //Not used ??

        public short zpos
        {//; (short)(ExtractBits(Vals[1], 0, 0x7f)); 
            get
            {
                int val = (int)GetAt16(PTR + 2);
                return (short)DataLoader.ExtractBits(val, 0, 0x7f);
            }
            set
            {
                int existingValue = GetAt16(PTR + 2);
                existingValue &= 0xFF80; //Mask out current val
                SetAt16(PTR + 2, existingValue | ((value & 0x7F) << 0));
            }
        }

        public short heading
        {//;(short)(ExtractBits(Vals[1], 7, 0x7)); //bits 7-9
            get
            {
                int val = (int)GetAt16(PTR + 2);
                return (short)DataLoader.ExtractBits(val, 7, 0x7);
            }
            set
            {
                int existingValue = GetAt16(PTR + 2);
                existingValue &= 0xFC7F; //Mask out current val
                SetAt16(PTR + 2, existingValue | ((value & 0x7) << 7));
            }
        }

        //gets the current heading in radians
        public float heading_r
        {//not quite right. I need to check a bit more.
            get
            {
                var d = (double)(heading * 45);
                return (float)(d * (System.Math.PI / 180));
            }
        }

        public short ypos
        {//(short)(ExtractBits(Vals[1], 10, 0x7));
            get
            {
                int val = (int)GetAt16(PTR + 2);
                return (short)DataLoader.ExtractBits(val, 10, 0x7);
            }
            set
            {
                if (index > 256)
                {
                    if (value != ypos)
                    {
                        Debug.Print("Changing ypos for static object " + index);
                    }
                }
                int existingValue = GetAt16(PTR + 2);
                existingValue &= 0xE3FF; //Mask out current val
                SetAt16(PTR + 2, existingValue | ((value & 0x7) << 10));
            }
        }


        public short xpos
        {// (short)(ExtractBits(Vals[1], 13, 0x7));
            get
            {
                int val = (int)GetAt16(PTR + 2);
                return (short)DataLoader.ExtractBits(val, 13, 0x7);
            }
            set
            {
                if (index > 256)
                {
                    if (value != xpos)
                    {
                        Debug.Print("Changing xpos for static object  " + index);
                    }
                }
                int existingValue = GetAt16(PTR + 2);
                existingValue &= 0x1FFF; //Mask out current val
                SetAt16(PTR + 2, existingValue | ((value & 0x7) << 13));
            }
        }

        public short quality
        { // (short)(ExtractBits(Vals[2], 0, 0x3f))
            get
            {
                int val = (int)GetAt16(PTR + 4);
                return (short)DataLoader.ExtractBits(val, 0, 0x3f);
            }
            set
            {
                int existingValue = GetAt16(PTR + 4);
                existingValue &= 0xE3FF; //Mask out current val
                SetAt16(PTR + 4, existingValue | ((value & 0x3f) << 0));
            }
        }

        public short next
        {//(short)(ExtractBits(Vals[2], 6, 0x3ff));
            get
            {
                int val = (int)GetAt16(PTR + 4);
                return (short)DataLoader.ExtractBits(val, 6, 0x3ff);
            }
            set
            {
                int existingValue = GetAt16(PTR + 4);
                existingValue &= 0x003F; //Mask out current val
                SetAt16(PTR + 4, existingValue | ((value & 0x3FF) << 6));
            }
        }

        public short owner
        { // (short)(ExtractBits(Vals[2], 0, 0x3f))
            get
            {
                int val = (int)GetAt16(PTR + 6);
                return (short)DataLoader.ExtractBits(val, 0, 0x3f);
            }
            set
            {
                int existingValue = GetAt16(PTR + 6);
                existingValue &= 0xFFC0; //Mask out current val
                SetAt16(PTR + 6, existingValue | ((value & 0x3f) << 0));
            }
        }

        public short race
        {
            get
            {
                return (short)(owner & 0x1F);
            }
        }

        public short link
        {//(short)(ExtractBits(Vals[2], 6, 0x3ff));
            get
            {
                int val = (int)GetAt16(PTR + 6);
                return (short)DataLoader.ExtractBits(val, 6, 0x3ff);
            }
            set
            {
                int existingValue = GetAt16(PTR + 6);
                existingValue &= 0x003F; //Mask out current val
                SetAt16(PTR + 6, existingValue | ((value & 0x3FF) << 6));
            }
        }

        //Mobile Properties. Only available on objects with indices <256

        public byte npc_hp
        {//
            get
            {
                if (IsStatic) { return 0; }
                return GetAt(PTR + 0x8);
            }
            set
            {
                if (!IsStatic)
                {
                    SetAt(PTR + 0x8, value);
                }
            }
        }

        public short ProjectileHeading
        {
            get
            {
                if (IsStatic) { return 0; }
                return (short)GetAt(PTR + 0x9);
            }
            set
            {
                if (!IsStatic)
                {
                    SetAt(PTR + 0x9, (byte)value);
                }
            }
        }

        //public short ProjectileHeadingMinor//defection to the right of the missile from the major heading.
        //{
        //    get
        //    {
        //        if (IsStatic) { return 0; }
        //        int val = (int)DataLoader.getValAtAddress(DataBuffer, PTR + 0x9, 8);
        //        return (short)(DataLoader.ExtractBits(val, 0, 0x1F));
        //    }
        //    set
        //    {//E0
        //        if (!IsStatic)
        //        {
        //            value &= 0x1F; //Keep value in range;
        //            int val = (byte)(ProjectileHeadingMajor << 5) | (value & 0x1F);
        //            DataBuffer[PTR + 0x9] = (byte)val;
        //        }
        //    }

        //}

        //public short ProjectileHeadingMajor //Cardinal direction 0 to 7 of the missile. North = 0 turning clockwise to North west = 7
        //{
        //    get
        //    {
        //        if (IsStatic) { return 0; }
        //        int val = (int)DataLoader.getValAtAddress(DataBuffer, PTR + 0x9, 8);
        //        return (short)(DataLoader.ExtractBits(val, 5, 0x7));
        //    }
        //    set
        //    {
        //        if (!IsStatic)
        //        {
        //            value &= 0x7; //Keep value in range;
        //            int val = (byte)((value & 0x7) << 5) | (ProjectileHeadingMinor & 0x1F);
        //            DataBuffer[PTR + 0x9] = (byte)val;
        //        }
        //    }
        //}

        public byte MobileUnk_0xA
        {
            get
            {
                if (IsStatic) { return 0; }
                return GetAt(PTR + 0xa);
            }
            set
            {
                if (!IsStatic)
                {
                    SetAt(PTR + 0xA, (byte)value);
                }
            }
        }

        public byte npc_goal
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0xb);
                return (byte)(DataLoader.ExtractBits(val, 0, 0xF));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0xB);
                    existingValue &= 0xFFF0; //Mask out current val
                    SetAt16(PTR + 0xB, existingValue | ((value & 0xF) << 0));
                }
            }
        }

        public byte npc_gtarg
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0xb);
                return (byte)(DataLoader.ExtractBits(val, 4, 0xFF));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0xB);
                    existingValue &= 0xF00F; //Mask out current val
                    SetAt16(PTR + 0xB, existingValue | ((value & 0xFF) << 4));
                }
            }
        }

        public byte AnimationFrame
        {//formerly MobileUnk_0xB_12_F
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0xb);
                return (byte)(DataLoader.ExtractBits(val, 12, 0xF));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0xB);
                    existingValue &= 0x0FFF; //Mask out current val
                    SetAt16(PTR + 0xB, existingValue | ((value & 0xF) << 12));
                }
            }
        }

        public byte npc_level
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0xd);
                return (byte)(DataLoader.ExtractBits(val, 0, 0xF));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0xD);
                    existingValue &= 0xFFF0; //Mask out current val
                    SetAt16(PTR + 0xD, existingValue | ((value & 0xF) << 0));
                }
            }
        }

        public short MobileUnk_0xD_4_FF
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0xd);
                return (short)(DataLoader.ExtractBits(val, 4, 0xff));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0xD);
                    existingValue &= 0xF00F; //Mask out current val
                    SetAt16(PTR + 0xD, existingValue | ((value & 0xFF) << 4));
                }
            }
        }

        /// <summary>
        /// When set the npc is powerful and does extra damage and so on.
        /// </summary>
        public short IsPowerfull
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0xd);
                return (short)(DataLoader.ExtractBits(val, 0xA, 1));
            }
        }

        public short LootSpawnedFlag
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0xd);
                return (short)(DataLoader.ExtractBits(val, 12, 1));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0xD);
                    existingValue &= 0xEFFF; //Mask out current val
                    SetAt16(PTR + 0xD, existingValue | ((value & 0x1) << 12));
                }
            }
        }

        public short npc_talkedto
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0xd);
                return (short)(DataLoader.ExtractBits(val, 13, 1));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0xD);
                    existingValue &= 0xDFFF; //Mask out current val
                    SetAt16(PTR + 0xD, existingValue | ((value & 0x1) << 13));
                }
            }
        }

        public short npc_attitude
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0xd);
                return (short)(DataLoader.ExtractBits(val, 14, 0x3));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0xD);
                    existingValue &= 0x3FFF; //Mask out current val
                    SetAt16(PTR + 0xD, existingValue | ((value & 0x3) << 14));
                }
            }
        }

        public short MobileUnk_0xF_0_3F
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0xf);
                return (short)(DataLoader.ExtractBits(val, 0, 0x3F));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0xF);
                    existingValue &= 0xFFC0; //Mask out current val
                    SetAt16(PTR + 0xF, existingValue | ((value & 0x3F) << 0));
                }
            }
        }

        public short npc_height
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0xf);
                return (short)(DataLoader.ExtractBits(val, 6, 0x3F));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0xF);
                    existingValue &= 0xF03F; //Mask out current val
                    SetAt16(PTR + 0xF, existingValue | ((value & 0x3F) << 6));
                }
            }
        }

        public short MobileUnk_0xF_C_F
        {///Possibly used as a look up in to NPC charge modifiers?
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0xf);
                return (short)(DataLoader.ExtractBits(val, 0xC, 0xF));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0xF);
                    existingValue &= 0x0FFF; //Mask out current val
                    SetAt16(PTR + 0xF, existingValue | ((value & 0xF) << 0xC));
                }
            }
        }

        public short MobileUnk_0x11
        {
            get
            {
                if (IsStatic) { return 0; }
                return (short)GetAt(PTR + 0x11);
            }
            set
            {
                if (!IsStatic)
                {
                    SetAt(PTR + 0x11, (byte)value);
                }
            }
        }

        public short ProjectileSourceID
        {
            get
            {
                if (IsStatic) { return 0; }
                return (short)GetAt(PTR + 0x12);
            }
            set
            {
                if (!IsStatic)
                {
                    SetAt(PTR + 0x12, (byte)value);
                }
            }
        }

        public short MobileUnk_0x13
        {
            get
            {
                if (IsStatic) { return 0; }
                return (short)(GetAt(PTR + 0x13));
            }
            set
            {
                if (!IsStatic)
                {
                    SetAt(PTR + 0x13, (byte)value);
                }
            }
        }

        public short Projectile_Speed
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt(PTR + 0x14);
                return (short)(DataLoader.ExtractBits(val, 0, 0x8));
            }
            set
            {
                byte existingValue = (byte)GetAt16(PTR + 0x14);
                existingValue &= 0xF7; //Mask out current val
                SetAt(PTR + 0x14, (byte)(existingValue | ((value & 0x7) << 0)));
            }
        }

        public short Projectile_Pitch
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt(PTR + 0x14);
                return (short)(DataLoader.ExtractBits(val, 3, 0x1F));
            }
            set
            {
                byte existingValue = (byte)GetAt16(PTR + 0x14);
                existingValue &= 0x7; //Mask out current val
                SetAt(PTR + 0x14, (byte)(existingValue | ((value & 0x1F) << 3)));
            }
        }

        public short npc_animation
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt(PTR + 0x15);
                return (short)(DataLoader.ExtractBits(val, 0, 0x3F));
            }
            set
            {
                byte existingValue = (byte)GetAt(PTR + 0x15);
                existingValue &= 0xC0; //Mask out current val
                SetAt(PTR + 0x15, (byte)(existingValue | ((value & 0x3F) << 0x0)));
            }
        }

        //public short MobileUnk_0x15_4_1F
        //{
        //    get
        //    {
        //        if (IsStatic) { return 0; }
        //        int val = (int)Loader.getValAtAddress(DataBuffer, PTR + 0x15, 8);
        //        return (short)(DataLoader.ExtractBits(val, 4, 0x1F));
        //    }
        //    set
        //    {
        //        value &= 0x1F;//Keep in range
        //        int val = (value << 3) | (npc_animation & 0x7);
        //        DataBuffer[PTR + 0x15] = (byte)(val);
        //    }
        //}

        public short MobileUnk_0x16_0_F
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0x16);
                return (short)(DataLoader.ExtractBits(val, 0, 0xF));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0x16);
                    existingValue &= 0xFFF0; //Mask out current val
                    SetAt16(PTR + 0x16, (existingValue | ((value & 0xF) << 0x0)));
                }
            }
        }

        public short npc_yhome
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0x16);
                return (short)(DataLoader.ExtractBits(val, 4, 0x3F));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0x16);
                    existingValue &= 0xFC0F; //Mask out current val
                    SetAt16(PTR + 0x16, (existingValue | ((value & 0x3F) << 0x4)));
                }
            }
        }

        public short npc_xhome
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt16(PTR + 0x16);
                return (short)(DataLoader.ExtractBits(val, 10, 0x3F));
            }
            set
            {
                if (!IsStatic)
                {
                    int existingValue = GetAt16(PTR + 0x16);
                    existingValue &= 0x3FF; //Mask out current val
                    SetAt16(PTR + 0x16, (existingValue | ((value & 0x3F) << 10)));
                }
            }
        }
        public short npc_heading
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt(PTR + 0x18);
                return (short)(DataLoader.ExtractBits(val, 0, 0x1F));
            }
            set
            {
                byte existingValue = (byte)GetAt(PTR + 0x18);
                existingValue &= 0xE0; //Mask out current val
                SetAt(PTR + 0x18, (byte)(existingValue | ((value & 0x1F) << 0x0)));
            }
        }

        public short MobileUnk_0x18_5_7
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt(PTR + 0x18);
                return (short)(DataLoader.ExtractBits(val, 5, 0x7));
            }
            set
            {
                byte existingValue = (byte)GetAt(PTR + 0x18);
                existingValue &= 0x1F; //Mask out current val
                SetAt(PTR + 0x18, (byte)(existingValue | ((value & 0x7) << 0x5)));
            }
        }

        public short npc_hunger
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt(PTR + 0x19);
                return (short)(DataLoader.ExtractBits(val, 0, 0x3F));
            }
            set
            {
                byte existingValue = (byte)GetAt(PTR + 0x19);
                existingValue &= 0xC0; //Mask out current val
                SetAt(PTR + 0x19, (byte)(existingValue | ((value & 0x3f) << 0x0)));
            }
        }

        public short MobileUnk_0x19_6_3
        {
            get
            {
                if (IsStatic) { return 0; }
                int val = (int)GetAt(PTR + 0x19);
                return (short)(DataLoader.ExtractBits(val, 6, 0x3));
            }
            set
            {
                byte existingValue = (byte)GetAt(PTR + 0x19);
                existingValue &= 0x3F; //Mask out current val
                SetAt(PTR + 0x19, (byte)(existingValue | ((value & 0x3) << 6)));
            }
        }


        public short npc_whoami
        {
            get
            {
                if (IsStatic) { return 0; }
                return (short)GetAt(PTR + 0x1a);
            }
            set
            {
                SetAt(PTR + 0x1A, (byte)value);
            }
        }


        /// <summary>
        /// The X position of the projecile object in the world space (when not an NPC)
        /// </summary>
        /// Value is equal to the current (xhome<< 8) + (xpos <<5) +0xFh
        public int CoordinateX
        {
            get
            {
                return (short)GetAt16(PTR + 0xb);
            }
        }


        /// <summary>
        /// The Y position of the projecile object in the world space (when not an NPC)
        /// </summary>
        ///(yhome<<8) + (ypos<<5) +0xFh
        public int CoordinateY
        {
            get
            {
                return (short)GetAt16(PTR + 0xc);
            }
        }


        public int CoordinateZ
        {//(zpos<<3) + 0xFh is stored here
            get
            {
                return (short)GetAt16(PTR + 0xf);
            }
        }

        //Where are these values set???
        public short npc_health = 0;//Is this poisoning?
        public short npc_arms = 0;
        public short npc_power = 0;
        public short npc_name = 0;

        public Godot.Vector3 GetCoordinate(int tileX, int tileY)
        {//godot is y-up     
            return GetCoordinate(tileX, tileY, this.xpos, this.ypos, this.zpos);
        }

        public static Vector3 GetCoordinate(int tileX, int tileY, int _xpos, int _ypos, int _zpos)
        {
            float offX = GetXYCoordinate(tileX, _xpos);
            float offY = GetXYCoordinate(tileY, _ypos);
            float offZ = GetZCoordinate(_zpos);
            return new Godot.Vector3(-offX, offZ, offY);  //x is neg. probably technical debt from a bug in the unity version
        }


        /// <summary>
        /// Gets the world x or y coordinate for a given x or y value
        /// </summary>
        /// <param name="xypos"></param>
        /// <returns></returns>
        public static float GetXYCoordinate(int tilexy, int xypos)
        {
            xypos = (xypos * 3) + 1;
            float ResolutionXY = 23f;  // A tile has a 8x8 grid for object positioning.
            float BrushXY = 120f; //game world size of a tile.
            float offXY = (tilexy * BrushXY) + xypos * (BrushXY / ResolutionXY);
            return offXY / 100f;
        }

        /// <summary>
        /// Converts an world co-ordinate into a xpos or ypos value. (ignores tileXY)
        /// </summary>
        /// <param name="xypos"></param>
        /// <returns></returns>
        public static short FloatXYToXYPos(float offXY)
        {
            float BrushXY = 120f;
            float ResolutionXY = 23f;
            int tilexy = (int)(offXY / 1.2f);

            offXY = offXY * 100f;            
            short xypos = (short)((offXY - (tilexy * BrushXY)) / (BrushXY / ResolutionXY));

            // xypos = (xypos * 3) + 1;
            xypos = (short)((xypos-1)/3);

            if (xypos > 8)
            {
                xypos = 4;
            }
            if (xypos < 0)
            {
                xypos = 4;
            }
            return xypos;
        }


        /// <summary>
        /// Gets the world Z coordinate for a given zpos value
        /// </summary>
        /// <param name="_zpos"></param>
        /// <returns></returns>
        public static float GetZCoordinate(int _zpos)
        {
            float ResolutionZ = 128.0f; //UW has 127 posible z positions for an object in tile.
            float ceil = 32;// tileMap.CEILING_HEIGHT;
            float BrushZ = 15f;
            float offZ = (_zpos / ResolutionZ) * ceil * BrushZ;
            return offZ / 100.0f;
        }


        /// <summary>
        /// Converts a world coordinate to a zpos
        /// </summary>
        /// <param name="offZ"></param>
        /// <returns></returns>
         public static short FloatZToZPos(float offZ)
         {
            float ResolutionZ = 128.0f; //UW has 127 posible z positions for an object in tile.
            float ceil = 32;// tileMap.CEILING_HEIGHT;
            float BrushZ = 15f;
            offZ = offZ * 100f;
            //float offZ = (_zpos / ResolutionZ) * ceil * BrushZ;
            return (short)((offZ / (ceil * BrushZ)) * ResolutionZ);
         }


        public static int GetObjectSprite(uwObject obj)
        {
            if (obj != null)
            {
                return obj.item_id;
            }
            return -1;
        }

        /// <summary>
        /// Returns how much of a quantity this object really is.
        /// </summary>
        public int ObjectQuantity
        {
            get
            {
                if (is_quant == 1)
                {
                    if (link >= 0x200)
                    {
                        return 1;
                    }
                    else
                    {
                        return link;
                    }
                }
                return 1;
            }
        }

        public static int GetObjectQuantity(uwObject obj)
        {
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return obj.ObjectQuantity;
            }
        }       

         public static int ScaleDamage(int item_id, ref int basedamage, int damagetype)
         {
            if (_RES==GAME_UW2)
            {
                return ScaleDamageUW2(item_id: item_id, basedamage: ref basedamage, damagetype: damagetype);
            }
            else
            {
                return ScaleDamageUW1(item_id: item_id, basedamage: ref basedamage, damagetype: damagetype);
            }
         }


        static int ScaleDamageUW1(int item_id, ref int basedamage, int damagetype)
        {

            var scales = commonObjDat.scaleresistances(item_id);

            if ((scales & damagetype) != 0)
            { //has some sort of resistance
                if ((damagetype & 3) != 0) //magic resistances
                {
                    //damage & 3 != 0
                    //seg025_26A1_4BA:
                    var r = Rng.r.Next(0, 3);
                    if (r >= (scales & 0x3))
                    {
                        //seg025_26A1_4D5
                        damagetype &= 0xFC; //mask out magic damage. possibly means magic can be used in combo with other damage type??
                    }
                    else
                    {
                        return 0;
                    }
                }

                //seg025_26A1_4DD
                if ((scales & damagetype) != 0)
                { //the damage type is resisted after possibly masking out magic damage.
                    return 0;
                }
            }
            return basedamage;
        }
        /// <summary>
        /// Scales damage up or down based on the NPCs damage resistances
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="basedamage"></param>
        /// <param name="damagetype"></param>
        /// <returns></returns>
        static int ScaleDamageUW2(int item_id, ref int basedamage, int damagetype)
        {
            var scales = commonObjDat.scaleresistances(item_id);

            if ((scales & damagetype) != 0)
            { //has some sort of resistance
                if ((damagetype & 3) != 0) //magic resistances
                {
                    //damage & 3 != 0
                    //seg025_26A1_4BA:
                    var r = Rng.r.Next(0, 3);
                    if (r >= (scales & 0x3))
                    {
                        //seg025_26A1_4D5
                        damagetype &= 0xFC; //mask out magic damage. possibly means magic can be used in combo with other damage type??
                    }
                    else
                    {
                        return 0;
                    }
                }

                //seg025_26A1_4DD
                if ((scales & damagetype) != 0)
                { //the damage type is resisted after possibly masking out magic damage.
                    return 0;
                }
            }

            //if at this point magic has possibly been masked out. but checks need to be made to see how fire/ice damage interact with fire/ice resistances
            //Ice is unique to UW2 so this does not apply to UW1.
            //seg 25:429
            if ((damagetype & 8) != 0) //test for fire damage
            {
                if ((scales & 0x20) != 0) //test for ice resistance
                {
                    return VulnerableDamage(ref basedamage); //fire does double damage to ice resistance creatures?
                }
            }

            //seg025_26A1_4F5
            if ((damagetype & 0x20) == 0) //test for ice damage
            {
                return basedamage; 
            }
            else
            {//not ice damage
                if ((scales & 8) == 0)//test for no fire resistance
                {
                    return basedamage;  
                }
                else
                {
                    //if has fire ressistance
                    if ((scales & 0x28) == 0x28) //test for fire and ice together.
                    {
                        return basedamage; //when fire and ice together ice only does regular damage
                    }
                    else
                    {
                        return VulnerableDamage(ref basedamage); //if has no ice resistance, ice damage will do double damage
                    }
                }
            }

        }

        static int VulnerableDamage(ref int basedamage)
        {
            basedamage = System.Math.Min(127, basedamage << 1);
            return basedamage;
        }

    } //end class
}//end namespace