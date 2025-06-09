using System;

namespace Underworld
{
    /// <summary>
    /// Class for all operations relating to player.dat
    /// </summary>
    public partial class playerdat : Loader  //should this inherit from uwobject?
    {
        public static string currentfolder;

        /// <summary>
        /// Reference to the char name in the gamestrings for use in the conversation VM.
        /// </summary>
        //public static int CharNameStringNo;
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

        public static int handednessvalue
        {
            get
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                return (int)GetAt(offset) & 0x1;
            }
        }

        /// <summary>
        /// pdat[] & 1 is 1 when player is right handed
        /// </summary>
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
                if (!value)
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
        /// Gets the world number that the player is currently in. 
        /// </summary>
        public static int CurrentWorld
        {
            get
            {
                return worlds.GetWorldNo(dungeon_level);
            }
        }

        /// <summary>
        /// The full X co-ordinate in the map
        /// </summary>
        public static int XCoordinate
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
        /// Note. Proper usage in game should be of the playerObject.tileY
        /// </summary>
        // public static int tileX_depreciated
        // {
        //     get
        //     {
        //         return X_depreciated >> 8;
        //     }
        //     set
        //     {
        //         var tmp = GetAt16(0x55) & 0xFF;
        //         value = value & 0x3F;
        //         tmp |= value << 8;
        //         SetAt16(0x55, tmp);
        //     }
        // }

        // public static int xpos
        // {
        //     get
        //     {
        //         return (X & 0xff) >> 5;
        //     }
        //     set
        //     {
        //         var tmp = X & 0xFF1F;
        //         tmp |= (value << 5);
        //         X = tmp;
        //     }
        // }

        /// <summary>
        /// The full Y Coordinate in the map
        /// </summary>
        public static int YCoordinate
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
        /// Note. Proper usage in game should be of the playerObject.tileY
        /// </summary>
        // public static int tileY_depreciated
        // {
        //     get
        //     {
        //         return Y_depreciated >> 8;
        //     }
        //     set
        //     {
        //         var tmp = GetAt16(0x57) & 0xFF;
        //         value = value & 0x3F;
        //         tmp |= value << 8;
        //         SetAt16(0x57, tmp);
        //     }
        // }

        /// <summary>
        /// Player yposition in the tile. Player position appears to be a higher resolution than object positioning 
        /// so the below calc is a hack for initial development positioning of the player char
        /// </summary>
        // public static int ypos
        // {
        //     get
        //     {
        //         return (Y & 0xff) >> 5;// need to confirm if correct
        //     }
        //     set
        //     {
        //         var tmp = Y & 0xFF1F;
        //         tmp |= (value << 5);
        //         Y = tmp;
        //     }
        // }

        public static int Z
        {
            get
            {
                return GetAt16(0x59);
            }
            set
            {
                SetAt16(0x59, value);
            }
        }

        // public static int zpos
        // {
        //     get
        //     {
        //         return Z >> 3;
        //     }
        //     set
        //     {
        //         Z = (value << 3);
        //     }
        // }

        // public static int camerazpos
        // {
        //     get
        //     {
        //         return zpos + commonObjDat.height(127);
        //     }
        // }


        /// <summary>
        /// heading minor and heading major. this is getting confusing...
        /// </summary>
        public static int heading_full
        {
            get
            {
                return GetAt16(0x5B);
            }
            set
            {
                SetAt16(0x5B, (byte)value);
            }
        }


        public static int heading_minor
        {
            get
            {
                return GetAt(0x5B);
            }
            set
            {
                SetAt(0x5B, (byte)value);
            }
        }


        /// <summary>
        /// Note this is not the full heading value! 
        /// </summary>
        public static int heading_major
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
                if (value > max_hp)
                {
                    value = max_hp;
                }
                SetAt(0x36, (byte)value);
                uimanager.RefreshHealthFlask();
                uimanager.RefreshStatsDisplay();
                playerObject.npc_hp = (byte)value;
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
                if (value > max_mana)
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
        /// Backup of players max mana for when they enter tybals lair. Also backup value for automap enabled state in UW1.
        /// </summary>
        public static int backup_mana
        {
            get
            {
                if (_RES != GAME_UW2)
                {
                    return GetAt(0xB1);
                }
                else
                {
                    return max_mana;
                }
            }
            set
            {
                if (_RES != GAME_UW2)
                {
                    SetAt(0xB1, (byte)value);
                }
            }
        }


        /// <summary>
        /// Diffculty level of the game, 0 for normal difficulty, 1 for easy difficulty. Affects mainly combat calcs
        /// </summary>
        public static int difficuly
        {
            get
            {
                if (_RES == GAME_UW2)
                {
                    return GetAt(0x302);
                }
                else
                {
                    return GetAt(0xB5);
                }
            }
            set
            {
                if (_RES == GAME_UW2)
                {
                    SetAt(0x302, (byte)value);
                }
                else
                {
                    SetAt(0xB5, (byte)value);
                }
            }
        }

        /// <summary>
        /// Changes the player EXP using the vanilla logic for EXP gains
        /// </summary>
        /// <param name="newEXP"></param>
        public static void ChangeExperience(int newEXP)
        {
            //value * 500 is the exp needed for next level up
            int[] LevelUpAt = new int[] { 0, 1, 2, 3, 4, 6, 8, 0xC, 0x10, 0x18, 0x20, 0x30, 0x40, 0x60, 0x80, 0xC0 };
            if (newEXP > 0)
            {
                newEXP = (newEXP + Rng.r.Next(0, 2)) / 2;
            }

            if (newEXP >= 0)
            {
                if (_RES == GAME_UW2)
                {
                    var world = worlds.GetWorldNo(dungeon_level);
                    world = world << 1;
                    world += 2;
                    if (world < play_level)
                    {//reduce xp gain if on a lower "world level" then the player level
                        newEXP = 1 + (newEXP / 2);
                    }
                }
                var newTotalSkillPoints = (Exp + newEXP) / 1500; //how many skill points the player will have gained at their total exp level
                if (SkillPointsTotal < newTotalSkillPoints)
                {
                    SkillPoints = SkillPoints + (newTotalSkillPoints - SkillPointsTotal);// calculate new level of skill points available to spend.
                    SkillPointsTotal = newTotalSkillPoints; //store total earned.
                }
                Exp = Math.Min(Exp + newEXP, 0x17700);//96000 (exp points are in units of 0.1)

                //Check if player can level up
                if (play_level < 0x10)
                {
                    var PointsToCheck = Exp / 500;
                    var pointsNeeded = LevelUpAt[play_level];
                    if (pointsNeeded <= PointsToCheck)
                    {//player can level up
                        LevelUp(play_level + 1);
                    }
                }

                UpdateAttributes(false);
                uimanager.RefreshStatsDisplay();
            }
            else
            {
                //negative exp
                Exp = Math.Max(0, Exp - newEXP);
            }
        }

        /// <summary>
        /// Levels up the character.
        /// </summary>
        /// <param name="newLevel"></param>
        static void LevelUp(int newLevel)
        {
            if (newLevel <= 0x10)
            {
                play_level = newLevel;
                SkillPoints += newLevel;
                if (!ConversationVM.InConversation)
                {
                    uimanager.AddToMessageScroll($"{GameStrings.GetString(1, GameStrings.str_you_have_attained_experience_level_)}{newLevel}");
                }
                RecalculateHPManaMaxWeight(false);
            }
        }

        /// <summary>
        /// Updates hp, mana and max weight values when leveling up or when related stats change.
        /// </summary>
        /// <param name="RestoreMana"></param>
        public static void RecalculateHPManaMaxWeight(bool RestoreMana)
        {
            max_hp = 0x1E + ((STR * play_level) / 5);
            max_mana = ((ManaSkill + 1) * INT) >> 3;
            if (RestoreMana)
            {
                play_mana = max_mana;
            }
            uimanager.RefreshManaFlask();
            uimanager.RefreshHealthFlask();
            //maxweight =  300 + (STR*13) //TODO
        }
    } //end class
}//end namespace