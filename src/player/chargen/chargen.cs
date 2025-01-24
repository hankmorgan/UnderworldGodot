using System.Diagnostics;
using System.IO;

namespace Underworld
{
    /// <summary>
    /// Loads and processes character generation files.
    /// </summary>
    public class chargen : Loader
    {
        static byte[] chargen_dat;
        static byte[] skills_dat;

        //public static int ChargenStage = 0;

        public static void SimulateChargen()
        {
            int[] ClassSkillChoices = new int[] { 0x14, 0x14, 0x14, 0x14, 0x14, 0x14 };//The abilities the player will get as part of their class selection before a individual skills are choosen
            int ArrayPtr = 0;
            PresentChargenOptions(0);//init and ask gender            
            SubmitChargenOption(0, 1);//pick female gender

            PresentChargenOptions(1);//simulate handedness question
            SubmitChargenOption(1, 1); //pick lefthandy

            PresentChargenOptions(2);//simulate class question            
            SubmitChargenOption(2, 7); //pick class

            //skills
            bool firstSkill = true;
            while (GetSkillChoices(ref ArrayPtr, ref ClassSkillChoices, 0x36))
            {
                if (firstSkill)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (ClassSkillChoices[i] != 0x14) 
                        {
                            Debug.Print($"Innate starting Skill {GameStrings.GetString(2, 31 + ClassSkillChoices[i])}");
                            SubmitChargenOption(3, ClassSkillChoices[i]);//calc class skills
                        }
                    }
                    firstSkill = false;
                }
                PrintChoices(3);
            }

            SubmitChargenOption(4, 1);//pick body #1.

            PresentChargenOptions(5);//difficulty question
            SubmitChargenOption(5, 1);//difficulty #1

            SubmitChargenOption(6, 0);//submit name

            PresentChargenOptions(7);//confirmation
            SubmitChargenOption(7, 1);//submit 1 to confirmation.

        }
        /// <summary>
        /// Processes character generation by presenting questions to the player.
        /// </summary>
        public static void PresentChargenOptions(int ChargenStage = 0)
        {
            if (chargen_dat == null || skills_dat == null)
            {
                InitChargenFiles();
            }

            switch (ChargenStage)
            {
                case 0: //Initiation/gender question
                    //reset uI and present gender question
                    PrintChoices(ChargenStage);//pick a gender    
                    break;
                case 1://handeness
                    PrintChoices(ChargenStage);//pick handedness
                    break;
                case 2:
                    PrintChoices(ChargenStage);//pick a class
                    break;
                case 3://pick skills until all skill choices are exhausted
                    Debug.Print("skills");
                    break;
                case 4: // pick portrait
                    Debug.Print("portraits");
                    break;
                case 5:
                    PrintChoices(ChargenStage);//pick a difficulty
                    break;
                case 6://enter name
                    Debug.Print("name");
                    break;
                case 7:// confirm
                    PrintChoices(ChargenStage);//Confirm
                    break;
            }
        }

        /// <summary>
        /// Accepts submitted answers to chargen questions.
        /// </summary>
        /// <param name="ChargenStage"></param>
        /// <param name="choice"></param>

        public static void SubmitChargenOption(int ChargenStage, int choice)
        {
            switch (ChargenStage)
            {
                case 0://pick gender.
                    playerdat.isFemale = (choice == 1);
                    break;
                case 1://pick handedness
                    playerdat.isLefty = (choice == 1);
                    break;
                case 2://class
                    InitClassAttributes(choice);
                    break;
                case 3://process skill choice
                    RollSkill(choice);
                    break;
                case 4://portrait
                    playerdat.Body = choice;
                    break;
                case 5://diffiulty
                    playerdat.difficuly = choice;
                    break;
                case 6://name
                    playerdat.CharName = "Gronky";
                    break;
                case 7://confirm
                    //if yes start game. if no. restart chargen.
                    break;
            }
        }


        /// <summary>
        /// Initialises the base player attributes and intitial skill values.
        /// </summary>
        /// <param name="newclass"></param>
        static void InitClassAttributes(int newclass)
        {
            Debug.Print($"NewClass is {GameStrings.GetString(2, 23 + newclass)}");
            playerdat.CharClass = newclass;
            playerdat.STR = skills_dat[newclass * 4];
            playerdat.DEX = skills_dat[(newclass * 4) + 1];
            playerdat.INT = skills_dat[(newclass * 4) + 2];
            for (int i = 0; i < 0x14; i++)
            {
                playerdat.SetSkillValue(i, 0);//initialise skills
            }
            int totalAttributeBonus = skills_dat[(newclass * 4) + 3];

            while (totalAttributeBonus > 0)
            {//randomly assign attribute bonuses to the player.
                var bonustoAdd = 1 + Rng.r.Next(3);
                if (bonustoAdd > totalAttributeBonus)
                {
                    bonustoAdd = totalAttributeBonus;
                }
                //pick an attribute to increase
                var attribute = Rng.r.Next(3);
                switch (attribute)
                {
                    case 0:
                        playerdat.STR += bonustoAdd; break;
                    case 1:
                        playerdat.DEX += bonustoAdd; break;
                    case 2:
                        playerdat.INT += bonustoAdd; break;
                }
                totalAttributeBonus -= bonustoAdd;
            }

            Debug.Print($"Final player attributes STR:{playerdat.STR} DEX:{playerdat.DEX} INT:{playerdat.INT}");
        }

        /// <summary>
        /// Prints out the question and possible answers for the specified question no
        /// </summary>
        /// <param name="QuestionNo"></param>
        static void PrintChoices(int QuestionNo)
        {
            var noOfChoices = getAt(chargen_dat, 8 + (QuestionNo * 18), 8);
            int ptrToChoices = (int)getAt(chargen_dat, 4 + (QuestionNo * 18), 32);
            int question = (int)getAt(chargen_dat, (QuestionNo * 18), 8);
            Debug.Print($"The question is {GameStrings.GetString(2, question)}");
            for (int i = 0; i < noOfChoices; i++)
            {
                int choice = (int)getAt(chargen_dat, ptrToChoices + (i * 2), 8);
                Debug.Print($"{i}. {GameStrings.GetString(2, choice)}");
            }
        }

        /// <summary>
        /// Reads in the body of the chrgen file and stores the pointers to the answer strings
        /// </summary>
        static void InitChargenFiles()
        {
            var path_chargen = Path.Combine(BasePath, "DATA", "CHRGEN.DAT");
            var path_skills = Path.Combine(BasePath, "DATA", "SKILLS.DAT");

            if (File.Exists(path_chargen) && File.Exists(path_skills))
            {
                chargen_dat = File.ReadAllBytes(path_chargen);
                skills_dat = File.ReadAllBytes(path_skills);
                var ptrChargenBody = 0x90;

                //update values at offsets [4,5,6,7] in chargen_dat with pointers to strings.
                for (int i = 0; i < 8; i++)
                {
                    setAt(chargen_dat, 4 + (i * 18), 32, ptrChargenBody);
                    ptrChargenBody += 2;
                    while (chargen_dat[ptrChargenBody] != 0)
                    {
                        ptrChargenBody += 2;
                    }
                    ptrChargenBody += 2;
                }
            }
        }


        /// <summary>
        /// Extracts the skill choices for the select char class.
        /// </summary>
        /// <param name="ArrayPtr"></param>
        /// <param name="outSkills"></param>
        /// <param name="chargenPtr"></param>
        /// <returns></returns>
        static bool GetSkillChoices(ref int ArrayPtr, ref int[] outSkills, int chargenPtr)
        {
            var recordcounter_var2 = 0;
            var si_record = 0;

            while ((playerdat.CharClass * 5) + ArrayPtr > recordcounter_var2)
            {//seek to the skill choices
                si_record = si_record + skills_dat[32 + si_record] + 1;
                recordcounter_var2++;
            }

            if (ArrayPtr >= 5)
            {
                return false;
            }

            bool run = true;
            while (run)
            {
                switch (skills_dat[32 + si_record])
                {
                    case 0:
                        outSkills[ArrayPtr] = 0x14;
                        ArrayPtr++;
                        break;
                    case 1:
                        outSkills[ArrayPtr] = skills_dat[32 + si_record + 1];
                        var increment = skills_dat[32 + si_record] + 1;
                        si_record += increment;
                        ArrayPtr++;
                        break;
                    default:
                        chargen_dat[chargenPtr + 8] = skills_dat[32 + si_record];//store number of choices.
                        run = false;//end loop.
                        break;
                }

            }

            int di = 0;
            while (skills_dat[32 + si_record] > di)
            {
                //store the options in skills.dat as strings in chargen.
                var stringoffset = chargen_dat[chargenPtr + 4];
                setAt(chargen_dat, stringoffset + di * 2, 16, 0x1F + skills_dat[32 + si_record + di + 1]);
                //Debug.Print(GameStrings.GetString(2, (int)getAt(chargen_dat, stringoffset + di * 2, 16)));
                di++;
            }

            ArrayPtr++;
            return true;
        }

        /// <summary>
        /// Rolls for the skill increase when skillno is selected in chargen
        /// </summary>
        /// <param name="skillno"></param>
        static void RollSkill(int skillno)
        {
            int skilldivisor;
            int BaseIncrease;
            int NoOfRolls;
            int skillValue = playerdat.GetSkillValue(skillno);
            if(skillValue == 0)
            {
                BaseIncrease = 3;
                skilldivisor = 9;
                NoOfRolls = 3;
            }
            else
            {
                BaseIncrease = 1;
                skilldivisor = 0xD;
                NoOfRolls = 3;
            }
            int governingstat = playerdat.GetAttributeValue(playerdat.GetGoverningAttribute(skillno));
            skillValue += BaseIncrease;
            skillValue += governingstat/skilldivisor;
            skillValue += Rng.r.Next(NoOfRolls);
            while (NoOfRolls>0)
            {
                skillValue += (int)playerdat.SkillCheck(governingstat, 0x14);
                NoOfRolls--;
            }
            if (skillValue>30)
            {
                skillValue = 30;
            }
            playerdat.SetSkillValue(skillno, skillValue);

        }
    }//end class
}// end namespace