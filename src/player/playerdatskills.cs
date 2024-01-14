using System;
using System.Diagnostics;
namespace Underworld
{
    /// <summary>
    /// Player.dat class for managing skills
    /// </summary>
    public partial class playerdat : Loader
    {
        static Random r = new Random();
        public enum SkillCheckResult
        {
            CritFail = -1,
            Fail = 0,
            Success = 1,
            CritSucess = 2
        }


    public static SkillCheckResult SkillCheck(int skillValue, int targetValue)
    {
        
        int score = (skillValue - targetValue) + r.Next(0, 30); //0 to 29;

        if (score < 0x1d)
        {
            if (score < 0x10)
            {
                if (score < 3)
                {
                    Debug.Print("Skill roll " + skillValue + " vs " + targetValue + " Score = " + score + " (CritFail)");
                    return SkillCheckResult.CritFail;//0xffff //critical failure
                }
                else
                {
                    Debug.Print("Skill roll " + skillValue + " vs " + targetValue + " Score = " + score + " (Fail)");
                    return SkillCheckResult.Fail; //failure
                }
            }
            else
            {
                Debug.Print("Skill roll " + skillValue + " vs " + targetValue + " Score = " + score + " (Success)");
                return SkillCheckResult.Success; //sucess
            }
        }
        else
        { //more than 29
            Debug.Print("Skill roll " + skillValue + " vs " + targetValue + " Score = " + score + " (CritSuccess)");
            return SkillCheckResult.CritSucess; //critical sucess
        }
    }



        /// <summary>
        /// The character class of the player
        /// </summary>
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

        /// <summary>
        /// The progression level the player is at
        /// </summary>
        public static int play_level
        {
            get { return GetAt(0x3E); }
            set { SetAt(0x3E, (byte)value); }
        }

        /// <summary>
        /// The experience points in 0.1 points of the player
        /// </summary>
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


        /// <summary>
        /// The training points available to spend on skill ups
        /// </summary>
        public static int TrainingPoints
        {
            get { return GetAt(0x53); }
            set { SetAt(0x53, (byte)value); }
        }


        /// <summary>
        /// The player strength
        /// </summary>
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

        /// <summary>
        /// Player dexterity
        /// </summary>
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

        /// <summary>
        /// Player intelligence
        /// </summary>
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

        /// <summary>
        /// Gets the skill from attack=1 upwards
        /// </summary>
        /// <param name="skillNo"></param>
        /// <returns></returns>
         public static int GetSkillValue (int skillNo)
         {
            return (int)GetAt(0x21+skillNo);
         }


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

    }
}//end namespace