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
        public static int CharClass
        {
            get
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                return (GetAt(offset) >> 5) & 0x7;
            }
            set
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                byte existingValue = GetAt(offset);
                existingValue = (byte)(existingValue & 0x1F); //mask out charclass
                value = value << 5;
                existingValue = (byte)(existingValue | value);
                SetAt(offset, existingValue);
            }
        }
        public static int CharLevel
        {
            get { return GetAt(0x3E); }
            set { SetAt(0x3E, (byte)value); }
        }
        public static int Exp
        {
            get
            {
                return (int)GetAt32(0x4F) / 10;
            }
            set
            {
                SetAt32(0x4F, value * 10);
            }
        }
        public static int TrainingPoints
        {
            get { return GetAt(0x53); }
            set { SetAt(0x53, (byte)value); }
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
                SetAt(0x53, (byte)value);
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

        public static int STR
        {
            get
            {
                int value = (int)GetAt(0x1F);
                return value;
            }
            set
            {
                SetAt(0x1F, (byte)(value));
            }
        }
        public static int DEX
        {
            get
            {
                return (int)GetAt(0x20);
            }
            set
            {
                SetAt(0x20, (byte)(value));
            }
        }
        public static int INT
        {
            get
            {
                return (int)GetAt(0x21);
            }
            set
            {
                SetAt(0x21, (byte)(value));
            }
        }

        //Character skills
        public static int Attack
        {
            get
            {
                return (int)GetAt(0x22);
            }
            set
            {
                SetAt(0x22, (byte)(value));
            }
        }
        public static int Defense
        {
            get
            {
                return (int)GetAt(0x23);
            }
            set
            {
                SetAt(0x23, (byte)(value));
            }
        }
        public static int Unarmed
        {
            get
            {
                return (int)GetAt(0x24);
            }
            set
            {
                SetAt(0x24, (byte)(value));
            }
        }
        public static int Sword
        {
            get
            {
                return (int)GetAt(0x25);
            }
            set
            {
                SetAt(0x25, (byte)(value));
            }
        }
        public static int Axe
        {
            get
            {
                return (int)GetAt(0x26);
            }
            set
            {
                SetAt(0x26, (byte)(value));
            }
        }
        public static int Mace
        {
            get
            {
                return (int)GetAt(0x27);
            }
            set
            {
                SetAt(0x27, (byte)(value));
            }
        }
        public static int Missile
        {
            get
            {
                return (int)GetAt(0x28);
            }
            set
            {
                SetAt(0x28, (byte)(value));
            }
        }
        public static int ManaSkill
        {
            get
            {
                return (int)GetAt(0x29);
            }
            set
            {
                SetAt(0x29, (byte)(value));
            }
        }
        public static int Lore
        {
            get
            {
                return (int)GetAt(0x2A);
            }
            set
            {
                SetAt(0x2A, (byte)(value));
            }
        }
        public static int Casting
        {
            get
            {
                return (int)GetAt(0x2B);
            }
            set
            {
                SetAt(0x2B, (byte)(value));
            }
        }
        public static int Traps
        {
            get
            {
                return (int)GetAt(0x2C);
            }
            set
            {
                SetAt(0x2C, (byte)(value));
            }
        }
        public static int Search
        {
            get
            {
                return (int)GetAt(0x2D);
            }
            set
            {
                SetAt(0x2D, (byte)(value));
            }
        }
        public static int Track
        {
            get
            {
                return (int)GetAt(0x2E);
            }
            set
            {
                SetAt(0x2E, (byte)(value));
            }
        }
        public static int Sneak
        {
            get
            {
                return (int)GetAt(0x2F);
            }
            set
            {
                SetAt(0x2F, (byte)(value));
            }
        }
        public static int Repair
        {
            get
            {
                return (int)GetAt(0x30);
            }
            set
            {
                SetAt(0x30, (byte)(value));
            }
        }
        public static int Charm
        {
            get
            {
                return (int)GetAt(0x31);
            }
            set
            {
                SetAt(0x31, (byte)(value));
            }
        }
        public static int PickLock
        {
            get
            {
                return (int)GetAt(0x32);
            }
            set
            {
                SetAt(0x32, (byte)(value));
            }
        }
        public static int Acrobat
        {
            get
            {
                return (int)GetAt(0x33);
            }
            set
            {
                SetAt(0x33, (byte)(value));
            }
        }
        public static int Appraise
        {
            get
            {
                return (int)GetAt(0x34);
            }
            set
            {
                SetAt(0x34, (byte)(value));
            }
        }
        public static int Swimming
        {
            get
            {
                return (int)GetAt(0x35);
            }
            set
            {
                SetAt(0x35, (byte)(value));
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
    } //end class

}//end namespace