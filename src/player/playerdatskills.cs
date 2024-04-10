using System.Diagnostics;
namespace Underworld
{
    /// <summary>
    /// Player.dat class for managing skills
    /// </summary>
    public partial class playerdat : Loader
    {
        //static Random r = new Random();
        public enum SkillCheckResult
        {
            CritFail = -1,
            Fail = 0,
            Success = 1,
            CritSucess = 2
        }


    public static SkillCheckResult SkillCheck(int skillValue, int targetValue)
    {        
        int score = (skillValue - targetValue) + Rng.r.Next(0, 31); //0 to 30;

        if (score <= 0x1c)
        {
            if (score <= 0xF)
            {
                if (score <= 2)
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
                return (int)GetAt32(0x4F);
            }
            set
            {
                SetAt32(0x4F, value);
            }
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
        /// Gets the str, dex or int score
        /// </summary>
        /// <param name="attributeNo"></param>
        /// <returns></returns>
        public static int GetAttributeValue (int attributeNo)
        {
            return (int)GetAt(0x1F+attributeNo);
        }


        /// <summary>
        /// Gets the skill from attack upwards
        /// </summary>
        /// <param name="skillNo"></param>
        /// <returns></returns>
         public static int GetSkillValue (int skillNo)
         {
            return (int)GetAt(0x22+skillNo);
         }


        /// <summary>
        /// Sets the skills at the index (attack = 1 upwards) to the specified value
        /// </summary>
        /// <param name="skillNo"></param>
        /// <param name="value"></param>
         public static void SetSkillValue(int skillNo, int value)
        {
            if (value > 30)
            {
                value = 30;
            }
            Debug.Print($"Setting skill {skillNo} to {value} TODO: Refresh player stats such as mana/vit/carry weight as needed");
            SetAt(0x22 + skillNo, (byte)value);           
        }

        public static void UpdateAttributes(bool IncreasePlayMana = true)
        {
            playerdat.max_mana = ((playerdat.ManaSkill + 1) * playerdat.INT) >> 3;
            playerdat.max_hp = (30 + (playerdat.STR * playerdat.play_level) / 5);
            if (IncreasePlayMana)
            {
                playerdat.play_mana = playerdat.max_mana;
            }      
            //TODO carry weight and others      
        }

        public static void SetDungeonLore(int dungeon, int newLore)
        {
            switch (_RES)
                {   
                    case GAME_UW2:
                        Debug.Print("Lore increase in UW2");
                        return;//TODO
                    default:           
                        if (dungeon <=8)
                        {
                        SetAt(0xC3 + dungeon, (byte)newLore);
                        }  
                        break;
                }
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

        /// <summary>
        /// Gets which of strength, dex or int 
        /// </summary>
        /// <param name="skillno"></param>
        /// <returns></returns>
        public static int GetGoverningAttribute(int skillno)
        {
            if (skillno<7)
            {
                return 0;
            }
            if (skillno<10)
            {
                return 2;
            }
            return 1;
        }

        /// <summary>
        /// Skillpoints available to spend
        /// </summary>
        public static int SkillPoints
        {
            get
            {
                return GetAt(0x53);
            }
            set
            {
                SetAt(0x53, (byte)value);
            }
        }

        /// <summary>
        /// Skillpoints at last exp gain.
        /// Stores the skillpoints from the last time the char gained exp. Used to calc if new skillpoints are to be awarded at exp gain. Checked against exp/1500
        /// </summary>
        public static int SkillPointsTotal
        {
            get
            {
                return GetAt(0x54);
            }
            set
            {
                SetAt(0x54, (byte)value);
            }
        }


        /// <summary>
        /// Increases the specified skill based on attributes and current skill level
        /// </summary>
        /// <param name="skillno"></param>
        /// <returns>1 if skill increases, 0 if no increase</returns>
        public static int IncreaseSkill(int skillno)
        {
            int[] governingRngRanges = new int[]{0x19,0x28,0xA};
            var governingAttribute = GetGoverningAttribute(0);
            
            int skillvalue = GetSkillValue(skillno);
            int attributeValue = GetAttributeValue(governingAttribute);

            if ((attributeValue<<1) < skillvalue)
            {//cap skill at 2xAttribute value
                return 0;
            }
            if (skillvalue>=30)
            { //already at max
                return 0;
            }            
            //increment by 1
            skillvalue++;
            SetSkillValue(skillNo: skillno, value: skillvalue);

            if (governingAttribute!=0)
            {
                if (governingAttribute/2 > skillvalue)
                {//gain another point when skill is less than half the attribute
                    skillvalue++;
                    SetSkillValue(skillNo: skillno, value: skillvalue);
                }
                if (skillvalue < governingAttribute)
                {//if skill is still less than attribute. add another point based on rng
                    var r = Rng.r.Next(governingRngRanges[governingAttribute]);
                    if (r < governingAttribute - skillvalue)
                    {
                        skillvalue++;
                        playerdat.GetSkillValue(skillno);
                        SetSkillValue(skillNo: skillno, value: skillvalue);
                    }
                }
            }

            if (skillno==8 && dungeon_level<=8)
            {
                //A lore skill increase has occured.
                //we need to store the lore values per dungeon level and reset
                //object  identification bits on every sprite object in the level.
                //but oddly enough not on objects the player has in inventory?!?
                SetDungeonLore(dungeon_level, Lore);
            }            
            return 1;
        }

        /// <summary>
        /// general skill function for trainers in UW2. Increases a single skill or a random skill within a category of skills;
        /// </summary>
        /// <param name="SkillNo"></param>
        /// <returns></returns>
        public static int IncreaseSkillGroup(int SkillNo)
        {
            var di = 0;
            var si = 0;
            var SkillRange_var8 = 0;
            var StartSkillNo_var6 = 0;
            var skillsincreased = 0;
            var SkillToUpdate_varC = 0;
            switch(SkillNo)
            {
                case -3:
                    StartSkillNo_var6 = 10;//Dexterity skills
                    SkillRange_var8 = 10;
                    si = 4;
                    break;
                case -2:
                    StartSkillNo_var6 = 7;//intelligence skills
                    SkillRange_var8 = 3;
                    si = 2;
                    break;
                case -1:
                    StartSkillNo_var6 = 0;//strength skills
                    SkillRange_var8 = 7;
                    si = 3;
                    break;
                default:
                    StartSkillNo_var6 = SkillNo; //single skill
                    SkillRange_var8 = 1;
                    si = 1;
                    break;

            }
            di = SkillRange_var8;

            while (si>0 && di>0)
            {
                if (
                    (StartSkillNo_var6 == 7)
                    && 
                    (ManaSkill<8)
                    && 
                    ((Rng.r.Next(0,0x8000) & 2) != 0)
                    )
                    {
                        SkillToUpdate_varC = 7;
                    }
                else
                    {
                        SkillToUpdate_varC = StartSkillNo_var6 + Rng.r.Next(0, SkillRange_var8);
                    }

                var tmp = IncreaseSkill(SkillToUpdate_varC);
                if (tmp == 1)
                {
                    skillsincreased = 1;
                }
                si--; di--;
            }

            return skillsincreased;
        }
    }//end class
}//end namespace