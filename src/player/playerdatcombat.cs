namespace Underworld
{

    /// <summary>
    /// Player data relating to combat
    /// </summary>
    public partial class playerdat: Loader
    {
        /// <summary>
        /// Does the player have their weapon drawn
        /// </summary>
        public static int play_drawn
        {
            get
            {
                if (_RES==GAME_UW2)
                    {
                        return GetAt(0x61) & 0x1;
                    }
                else
                    {
                        return GetAt(0x60) & 0x1;
                    }
                
            }
            set
            {
                if (_RES==GAME_UW2)
                    {
                        value = value & 0x1;
                        var tmp = GetAt(0x61);
                        tmp &= 0xFE;
                        tmp |= (byte)value;
                        SetAt(0x61,tmp);
                    }
                else
                    {
                        value = value & 0x1;
                        var tmp = GetAt(0x60);
                        tmp &= 0xFE;
                        tmp |= (byte)value;
                        SetAt(0x60,tmp);
                    }
            }
        }


        public static int[] LocationalArmourValues = new int[4];//equivilant to bytes 0-3 of critter data for the player

        public static int[] LocationalProtectionValues = new int[4];//Affects to hit chance for a body part.

        /// <summary>
        /// Bit flags to indicate what damage types the player is resistant to. 
        /// These bits will be the same for NPCs resistances. See the "scale" value in critter object.dat
        /// </summary>
        /// Known bits
        /// Bit 3 = Flameproof
        /// Bit 6 = Missileproof
        public static byte PlayerDamageTypeScale;


        /// <summary>
        /// A bonus applied from the valour spell. Gives a bonus of 10 + Casting/5
        /// </summary>
        public static int ValourBonus;

        public static bool PoisonedWeapon;

        /// <summary>
        /// Index of the last npc hit in combat
        /// </summary>
        public static int LastDamagedNPCIndex
        {
            get
            {
                if(_RES==GAME_UW2)
                {
                    return GetAt(0x309);
                }
                else
                {
                    return GetAt(0xBB);
                }
            }
            set
            {
                if(_RES==GAME_UW2)
                {
                    SetAt(0x309, (byte)value);
                }
                else
                {
                    SetAt(0xBB, (byte)value);
                }
            }
        }

        /// <summary>
        /// The type of npc (critterObjDat[9]) that the player last hit in combat
        /// </summary>
        public static int LastDamagedNPCType
        {
            get
            {
                if(_RES==GAME_UW2)
                {
                    return GetAt(0x30A);
                }
                else
                {
                    return GetAt(0xBC);
                }
            }
            set
            {
                if(_RES==GAME_UW2)
                {
                    SetAt(0x30A, (byte)value);
                }
                else
                {
                    SetAt(0xBC, (byte)value);
                }
            }
        }

        /// <summary>
        /// The last time the player hit an NPC in combat
        /// </summary>
        public static int LastDamagedNPCTime
        {
            get
            {
                if(_RES==GAME_UW2)
                {
                    return GetAt32(0x30B);
                }
                else
                {
                    return GetAt32(0xBD);
                }                
            }
            set
            {
                if (_RES==GAME_UW2)
                {
                    SetAt32(0x30B,value);
                }
                else
                {
                    SetAt32(0xBD,value);
                }                
            }
        }

        /// <summary>
        /// The tileX where damage was last applied to an NPC
        /// </summary>
        public static int LastDamagedNPCTileX
        {
            get
            {
                if(_RES==GAME_UW2)
                {
                    return GetAt(0x30F);
                }
                else
                {
                    return GetAt(0xC1);
                }
            }
            set
            {
                if(_RES==GAME_UW2)
                {
                    SetAt(0x30F, (byte)value);
                }
                else
                {
                    SetAt(0xC1, (byte)value);
                }
            }
        }

        /// <summary>
        /// The tile Y where the last damage was applied to an NPC
        /// </summary>
        public static int LastDamagedNPCTileY
        {
            get
            {
                if(_RES==GAME_UW2)
                {
                    return GetAt(0x310);
                }
                else
                {
                    return GetAt(0xC2);
                }
            }
            set
            {
                if(_RES==GAME_UW2)
                {
                    SetAt(0x310, (byte)value);
                }
                else
                {
                    SetAt(0xC2, (byte)value);
                }
            }
        }

    }//end class
}//end namespace