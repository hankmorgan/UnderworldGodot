namespace Underworld
{
    /// <summary>
    ///  wrap the collisons_dseg and simplify data access
    ///structure is
    ///0=height==0,
    ///1=zpos,
    ///2=link<<6 | unk flags
    ///4= y<<6 + x
    /// </summary>
    public class CollisionRecord : DataLoader
    {
        public static byte[] Collisions_dseg_2520 = new byte[0x12C]; //unknown size

        public short index;

        /// <summary>
        /// Offset for the data in the byte array
        /// </summary>
        public short ptr
        {
            get { return (short)(index * 6); }
        }

        /// <summary>
        /// Object zpos + object height
        /// </summary>
        public byte height
        {
            get
            {
                return (byte)getAt(Collisions_dseg_2520, ptr, 8);
            }
            set
            {
                setAt(Collisions_dseg_2520, ptr, 8, value);
            }
        }

        public byte zpos
        {
            get
            {
                return (byte)getAt(Collisions_dseg_2520, ptr + 1, 8);
            }
            set
            {
                setAt(Collisions_dseg_2520, ptr + 1, 8, value);
            }
        }

        public short link
        {
            get
            {
                return (short)((getAt(Collisions_dseg_2520, ptr + 2, 16) >> 6) & 0x3FF);
            }
            set
            {
                value = (short)(value & 0x3FF);
                var tmp = (int)getAt(Collisions_dseg_2520, ptr + 2, 16);
                tmp = tmp & 0x3F;
                tmp = tmp | (value << 6);
                setAt(Collisions_dseg_2520, ptr + 2, 16, (int)tmp);
            }
        }

        public short quality  //for want of a better name currently
        {
            get
            {
                return (short)((getAt(Collisions_dseg_2520, ptr + 2, 16)) & 0x3F);
            }
            set
            {
                value = (short)(value & 0x3F);
                var tmp = getAt(Collisions_dseg_2520, ptr + 2, 16);
                tmp = tmp & 0xFFC0;
                tmp = (uint)((short)tmp | (short)value);
                setAt(Collisions_dseg_2520, ptr + 2, 16, (int)tmp);
            }
        }

        public short unkxyvalue
        {
            get
            {
                return (short)getAt(Collisions_dseg_2520, ptr + 4, 16);
            }
            set
            {
                setAt(Collisions_dseg_2520, ptr + 4, 16, value);
            }
        }

        public CollisionRecord(short _index)
        {
            index = _index;
        }
    }//end class
}