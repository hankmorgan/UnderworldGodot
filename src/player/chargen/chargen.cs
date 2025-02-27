using System.Diagnostics;
using System.IO;
using System.Collections;
using Peaky.Coroutines;

namespace Underworld
{    
    /// <summary>
    /// Loads and processes character generation files.
    /// </summary>
    public class chargen : Loader
    {
        public static bool ChargenWaitForInput;
        static byte[] chargen_dat;
        static byte[] skills_dat;

        //public static int ChargenStage = 0;
        static int[] ClassSkillChoices = new int[] { 0x14, 0x14, 0x14, 0x14, 0x14, 0x14 };
        static int[] SkillChoices = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        static int ArrayPtr = 0;
        static bool firstSkill = true;
        public static int CurrentStage = 0;

        /// <summary>
        /// Rolls the starting skills for the player.
        /// </summary>
        private static void RollClassBaseSkills()
        {
            for (int i = 0; i < 6; i++)
            {
                if (ClassSkillChoices[i] != 0x14)
                {
                    Debug.Print($"Innate starting Skill {GameStrings.GetString(2, 31 + ClassSkillChoices[i])}");
                    RollSkill(ClassSkillChoices[i]);//calc class skills
                }
            }
        }

        /// <summary>
        /// Processes character generation by presenting questions to the player.
        /// </summary>
        public static void PresentChargenOptions(int ChargenStageRequested = 0)
        {
            if (chargen_dat == null || skills_dat == null)
            {
                InitChargenFiles();
            }

            switch (ChargenStageRequested)
            {
                case 0: //Initiation/gender question
                    playerdat.InitEmptyPlayer();
                    
                    uimanager.ClearChargenTextAndBody();
                    firstSkill = true;
                    ClassSkillChoices = new int[] { 0x14, 0x14, 0x14, 0x14, 0x14, 0x14 };
                    ArrayPtr = 0;

                    PrintChoices(ChargenStageRequested);//pick a gender    
                    break;
                case 1://handeness
                    PrintChoices(ChargenStageRequested);//pick handedness
                    break;
                case 2:
                    PrintChoices(ChargenStageRequested);//pick a class
                    break;
                case 3://pick skills until all skill choices are exhausted
                    Debug.Print("skills");
                    var res = GetSkillChoices(ref ArrayPtr, ref ClassSkillChoices, 0x36);
                    if (res)
                    {
                        if (firstSkill)
                        {
                            RollClassBaseSkills();
                            uimanager.PrintChargenSkills();
                            firstSkill = false;
                        }
                        PrintChoices(ChargenStageRequested);
                    }
                    else
                    {
                        //no more choices.
                        PresentChargenOptions(++CurrentStage);
                    }
                    break;
                case 4: // pick portrait
                    uimanager.clearchargenbuttons();
                    uimanager.chargenRows =5; uimanager.chargenCols = 1;
                    uimanager.EnableDisable(uimanager.instance.ChargenQuestion,false);
                    for (int i = 0; i<5;i++)
                    {
                        uimanager.CreateChargenPortraitButton(i, playerdat.isFemale);
                    }
                    break;
                case 5:
                    PrintChoices(ChargenStageRequested);//pick a difficulty
                    break;
                case 6://enter name
                    uimanager.clearchargenbuttons();
                    uimanager.EnableDisable(uimanager.instance.ChargenQuestion,false);
                    MessageDisplay.WaitingForTypedInput = true;
                    ChargenWaitForInput = true;
                    uimanager.instance.ChargenNameInput.Text = "";
                    uimanager.EnableDisable(uimanager.instance.ChargenNameBG,true);                    
                    uimanager.instance.TypedInput.Text = "";
                    _ = Coroutine.Run(
                            CharNameWaitForInput(),
                        main.instance
                        );
                    break;
                case 7:// confirm
                    PrintChoices(ChargenStageRequested);//Confirm
                    break;
            }
        }

        /// <summary>
        /// Accepts submitted answers to chargen questions.
        /// </summary>
        /// <param name="ChargenStageSubmitted"></param>
        /// <param name="choice"></param>

        public static void SubmitChargenOption(int ChargenStageSubmitted, int choice)
        {
            switch (ChargenStageSubmitted)
            {
                case 0://pick gender.
                    playerdat.isFemale = (choice == 1);
                    uimanager.instance.ChargenGender.Text = GameStrings.GetString(2, 9 + choice);
                    //advance to next stage
                    PresentChargenOptions(++CurrentStage);
                    break;
                case 1://pick handedness
                    playerdat.isLefty = (choice == 0);
                    PresentChargenOptions(++CurrentStage);
                    break;
                case 2://class
                    InitClassAttributes(choice);
                    uimanager.instance.ChargenClass.Text = GameStrings.GetString(2, 23 + choice);
                    uimanager.PrintChargenAttributes();
                    PresentChargenOptions(++CurrentStage);
                    break;
                case 3://process skill choice 
                    RollSkill(SkillChoices[choice]);
                    uimanager.PrintChargenSkills();
                    PresentChargenOptions(CurrentStage);
                    break;
                case 4://portrait
                    playerdat.Body = choice;
                    uimanager.SetChargenBodyImage(playerdat.Body, playerdat.isFemale);
                    PresentChargenOptions(++CurrentStage);
                    break;
                case 5://diffiulty
                    playerdat.difficuly = choice;                    
                    PresentChargenOptions(++CurrentStage);
                    break;
                case 6://name              
                    playerdat.CharName = uimanager.instance.ChargenNameInput.Text; 
                    uimanager.EnableDisable(uimanager.instance.ChargenNameBG,false);
                    uimanager.instance.ChargenName.Text = playerdat.CharName;                    
                    PresentChargenOptions(++CurrentStage);
                    break;
                case 7://confirm
                    //if yes start game. if no. restart chargen.
                    if (choice == 0)
                    {
                        //start game
                        playerdat.RecalculateHPManaMaxWeight(true);
                        playerdat.currentfolder = "DATA";
                        uimanager.EnableDisable(uimanager.instance.PanelChargen, false);
                        uimanager.instance.JourneyOnwards("DATA");
                    }
                    else
                    {
                        CurrentStage = 0;
                        PresentChargenOptions(0);
                    }
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
            playerdat.RecalculateHPManaMaxWeight(true);
            playerdat.play_hp = playerdat.max_hp;
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
            var questiontext = GameStrings.GetString(2, question);
            Debug.Print($"The question is {questiontext}");

            uimanager.clearchargenbuttons();

            if (noOfChoices <= 8)
            {
                uimanager.chargenCols = 1;
                uimanager.chargenRows = (int)noOfChoices;
            }
            else
            {
                uimanager.chargenCols = 2;
                uimanager.chargenRows = (int)(noOfChoices / 2);
            }

            for (int i = 0; i < noOfChoices; i++)
            {
                int choice = (int)getAt(chargen_dat, ptrToChoices + (i * 2), 8);
                uimanager.CreateChargenButton(
                    index: i,
                    text: GameStrings.GetString(2, choice));
                Debug.Print($"{i}. {GameStrings.GetString(2, choice)}");
            }

            if (questiontext != "")
            {
                uimanager.EnableDisable(uimanager.instance.ChargenQuestion, true);
            }
            else
            {
                uimanager.EnableDisable(uimanager.instance.ChargenQuestion, false);
            }
            uimanager.instance.ChargenQuestion.Text = questiontext;
            uimanager.instance.ChargenQuestion.Position = uimanager.CalculateChargenButtonPosition(-1);

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
            SkillChoices = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
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
                SkillChoices[di] = skills_dat[32 + si_record + di + 1];//stores the actual skill number for later matching
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
            if (skillValue == 0)
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
            skillValue += governingstat / skilldivisor;
            skillValue += Rng.r.Next(NoOfRolls);
            while (NoOfRolls > 0)
            {
                skillValue += (int)playerdat.SkillCheck(governingstat, 0x14);
                NoOfRolls--;
            }
            if (skillValue > 30)
            {
                skillValue = 30;
            }
            playerdat.SetSkillValue(skillno, skillValue);

        }

        static IEnumerator CharNameWaitForInput()
        {
            while (MessageDisplay.WaitingForTypedInput)
            {
                yield return new WaitOneFrame();
            }
            ChargenWaitForInput = false;
            SubmitChargenOption(6,0);
        }
    }//end class
}// end namespace