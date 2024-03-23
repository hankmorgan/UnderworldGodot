namespace Underworld
{
    /// <summary>
    /// Class for all operations relating to player.dat
    /// </summary>
    public partial class playerdat : Loader
    {

        /// <summary>
        /// Reference to the char name in the gamestrings for use in the conversation VM.
        /// </summary>
        public static int CharNameStringNo;
        public static string CharName
        {
            get
            {
                var _charname = "";
                for (int i = 1; i < 14; i++)
                {
                    var alpha = GetAt(i);
                    if (alpha.ToString() != "0")
                    {
                        _charname += (char)alpha;
                    }
                    else
                    {
                        return _charname;//end at terminator
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

        /// <summary>
        /// The Gender bit
        /// </summary>
        public static int gender
        {
            get
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                return (int)(GetAt(offset) >> 1) & 0x1;
            }
        }

        public static bool isFemale
        {
            get
            {
                return gender == 0x1;
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

        /// <summary>
        /// The full X co-ordinate in the map
        /// </summary>
        public static int X
        {
            get
            {
                return GetAt16(0x55);
            }
            set
            {
                SetAt16(0x55, value);
            }
        }

        /// <summary>
        /// The X tile the Player is in
        /// </summary>
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
                SetAt16(0x55, tmp);
            }
        }

        public static int xpos
        {
            get
            {
                return (X & 0xff) >> 5;
            }
            set
            {
                X = (value << 5);
            }        
        }

        /// <summary>
        /// The full Y Coordinate in the map
        /// </summary>
        public static int Y
        {
            get
            {
                return GetAt16(0x57);
            }
            set
            {
                SetAt16(0x57, value);
            }
        }


        /// <summary>
        /// The Y tile the player is in
        /// </summary>
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
                SetAt16(0x57, tmp);
            }
        }

        /// <summary>
        /// Player yposition in the tile. Player position appears to be a higher resolution than object positioning 
        /// so the below calc is a hack for initial development positioning of the player cha
        /// </summary>
        public static int ypos
        {
            get
            {
                return (Y & 0xff) >> 5;// need to confirm if correct
            }
            set
            {
                Y = (value << 5);
            }
        }

        public static int Z
        {
            get
            {
                return GetAt16(0x59);
            }
            set
            {
                SetAt(0x59, (byte)value);
            }
        }

        public static int zpos
        {
            get
            {
                return Z >> 3;
            }
            set
            {
                Z = (value << 3);
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
            set
            {
                SetAt(0x5C, (byte)value);
            }
        }


        public static int play_hp
        {
            get
            {
                return GetAt(0x36);
            }
            set
            {
                if (value>max_hp)
                {
                    value = max_hp;
                }
                SetAt(0x36, (byte)value);
                uimanager.RefreshHealthFlask();
                uimanager.RefreshStatsDisplay();
            }            
        }


        public static int max_hp
        {
            get
            {
                return GetAt(0x37);
            }
            set
            {
                SetAt(0x37, (byte)value);
                uimanager.RefreshHealthFlask();
                uimanager.RefreshStatsDisplay();
            }
        }



        public static int play_mana
        {
            get
            {
                return GetAt(0x38);
            }
            set
            {
                if (value>max_mana)
                {
                    value = max_mana;
                }
                SetAt(0x38, (byte)value);
                uimanager.RefreshManaFlask();
                uimanager.RefreshStatsDisplay();
            }
        }


        public static int max_mana
        {
            get
            {
                return GetAt(0x39);
            }
            set
            {
                SetAt(0x39, (byte)value);
                uimanager.RefreshManaFlask();
                uimanager.RefreshStatsDisplay();
            }
        }



        /// <summary>
        /// Player Hunger Level
        /// </summary>
        public static byte play_hunger
        {
            get
            {
                return GetAt(0x3A);
            }
            set
            {
                SetAt(0x3A, (byte)value);
            }
        }


        
        public static byte play_fatigue
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

        public static byte play_poison
        {
            get
            {
                switch (_RES)
                {//TODO double check this is right
                    case GAME_UW2:
                        return (byte)((GetAt(0x61) >> 1) & 0xf);
                    default:
                        return (byte)((GetAt(0x60) >> 2) & 0xf);
                }
            }
            set
            {
                switch (_RES)
                {//TODO double check this is right
                    case GAME_UW2:
                        {
                            var tmp = (byte)(GetAt(0x61) & 0xE1);
                            tmp = (byte)(tmp | ((value & 0xF) << 1));
                            SetAt(0x61, tmp);
                            break;
                        }
                    default:
                        {
                            var tmp = (byte)(GetAt(0x61) & 0xC3);
                            tmp = (byte)(tmp | ((value & 0xF) << 2));
                            SetAt(0x60, tmp);
                            break;
                        }
                }
                uimanager.RefreshHealthFlask();
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
                tmpValue |= ((value & 0x3F) << 6);//set new value
                SetAt16(0x62, tmpValue);
            }
        }

    } //end class
}//end namespace