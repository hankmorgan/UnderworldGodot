namespace Underworld
{
    /// <summary>
    /// Class for all operations relating to player.dat
    /// </summary>
    public partial class playerdat : Loader
    {

        public static string CharName
        {
            get
            {
                var _charname = "";
                for (int i = 1; i < 14; i++)
                {
                    var alpha = GetAt(i);
                    if (alpha.ToString() != "\0")
                    {
                        _charname += (char)alpha;
                    }
                }
                return _charname;
            }
            set
            {
                var _chararray = value.ToCharArray();
                for (int i = 1; i < 14; i++)
                {
                    if (i - 1 < value.Length)
                    {
                        SetAt(i, (byte)_chararray[i - 1]);
                    }
                    else
                    {
                        SetAt(i, (byte)0);
                    }
                }
            }
        }
        public static int Body
        {
            get
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                return (GetAt(offset) >> 2) & 0x7;
            }
            set
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                byte existingValue = GetAt(offset);
                existingValue = (byte)(existingValue & 0xE3);
                value = value << 2;
                existingValue = (byte)(existingValue | value);
                SetAt(offset, existingValue);
            }
        }

        public static bool isFemale
        {
            get
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                return ((int)(GetAt(offset) >> 1) & 0x1) == 0x1;
            }
            set
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                byte existingValue = GetAt(offset);
                byte mask = (1 << 1);
                if (value)
                {//set
                    existingValue |= mask;
                }
                else
                {//unset
                    existingValue = (byte)(existingValue & (~mask));
                }
                SetAt(offset, existingValue);
            }
        }
        public static bool isLefty
        {
            get
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                return ((int)(GetAt(offset)) & 0x1) == 0x0;
            }
            set
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                byte existingValue = GetAt(offset);
                byte mask = (1);
                if (value)
                {//set
                    existingValue |= mask;
                }
                else
                {//unset
                    existingValue = (byte)(existingValue & (~mask));
                }
                SetAt(offset, existingValue);
            }
        }

        //Location Data
        public static int dungeon_level
        {
            get
            {
                return GetAt(0x5D);
            }
            set
            {
                SetAt(0x5D, (byte)value);
            }
        }
        //Character attributes
        public static int X
        {
            get
            {
                return GetAt16(0x55);
            }
        }

        public static int tileX
        {
            get
            {
                return X >> 8;
            }
            set
            {
                var tmp = GetAt16(0x55) & 0xFF;
                tmp |= value << 8;
                SetAt16(0x55,tmp);
            }
        }

        public static int Y
        {
            get
            {
                return GetAt16(0x57);
            }
        }

        public static int xpos
        {
            get
            {
                return X & 0x7;// need to confirm if correct
            }
        }
        public static int tileY
        {
            get
            {
                return Y >> 8;
            }
            set
            {
                var tmp = GetAt16(0x57) & 0xFF;
                tmp |= value << 8;
                SetAt16(0x57,tmp);
            }
        }

        public static int ypos
        {
            get
            {
                return Y & 0x7;// need to confirm if correct
            }
        }

        public static int Z
        {
            get
            {
                return GetAt16(0x59);
            }
        }

        public static int zpos
        {
            get
            {
                return Z >> 3;
            }
        }

        public static int camerazpos
        {
            get
            {
                return zpos + commonObjDat.height(127);
            }
        }

        public static int heading
        {
            get
            {
                return GetAt(0x5C);
            }
        }


        public static int VIT
        {
            get
            {
                return GetAt(0x36);
            }
            set
            {
                SetAt(0x36, (byte)value);
            }
        }

        
        public static int MaxVIT
        {
            get
            {
                return GetAt(0x37);
            }
            set
            {
                SetAt(0x37, (byte)value);
            }
        }

 
 
        public static int MANA
        {
            get
            {
                return GetAt(0x38);
            }
            set
            {
                SetAt(0x38, (byte)value);
            }
        }

        
        public static int MaxMANA
        {
            get
            {
                return GetAt(0x39);
            }
            set
            {
                SetAt(0x39, (byte)value);
            }
        }



        /// <summary>
        /// Player Hunger Level
        /// </summary>
        public static int play_hunger
        {
            get
            {
                return (int)GetAt(0x3A);
            }
            set
            {
                SetAt(0x3A, (byte)value);
            }
        }

        /// <summary>
        /// How drunk the player is
        /// </summary>
        public static int intoxication
        {
            get
            {
        
                return (GetAt16(0x62) >> 4) & 0x3F;
            }
            set
            {      
                var tmpValue = GetAt16(0x62);
                tmpValue &= 0xF03F;//clear bits
                tmpValue |= ((value & 0x3F)<<6);//set new value
                SetAt16(0x62,tmpValue);
            }
        }

    } //end class
}//end namespace