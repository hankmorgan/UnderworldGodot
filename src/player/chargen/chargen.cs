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
            PresentChargenOptions(0);//init and ask gender            
            SubmitChargenOption(0,1);//pick female gender

            PresentChargenOptions(1);//simulate handedness question
            SubmitChargenOption(1,1); //pick lefthandy

            PresentChargenOptions(2);//simulate class question
            SubmitChargenOption(2,0); //pick fighter

            SubmitChargenOption(4,1);//pick body #1.

            PresentChargenOptions(5);//difficulty question
            SubmitChargenOption(5,1);//difficulty #1

            SubmitChargenOption(6,0);//submit name

            PresentChargenOptions(7);//confirmation
            SubmitChargenOption(7,1);//submit 1 to confirmation.

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
                    playerdat.isFemale = (choice==1);
                    break;
                case 1://pick handedness
                    playerdat.isLefty = (choice==1);
                    break;
                case 2://class
                    InitClassAttributes(choice);
                    break;
                case 3://process skill choice
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
            Debug.Print($"NewClass is {GameStrings.GetString(2,23+newclass)}");
            playerdat.CharClass = newclass;
            playerdat.STR = skills_dat[newclass*4];
            playerdat.DEX = skills_dat[(newclass*4)+1];
            playerdat.INT = skills_dat[(newclass*4)+2];
            for (int i =0; i<0x14;i++)
            {
                playerdat.SetSkillValue(i,0);//initialise skills
            }
            int totalAttributeBonus = skills_dat[(newclass*4)+3];

            while (totalAttributeBonus>0)
            {//randomly assign attribute bonuses to the player.
                var bonustoAdd = 1 + Rng.r.Next(3);
                if (bonustoAdd>totalAttributeBonus)
                {
                    bonustoAdd = totalAttributeBonus;
                }
                //pick an attribute to increase
                var attribute = Rng.r.Next(3);
                switch(attribute)
                {
                    case 0:
                        playerdat.STR += bonustoAdd; break;
                    case 1:
                        playerdat.DEX += bonustoAdd; break;
                    case 2:
                        playerdat.INT += bonustoAdd; break;
                }
                totalAttributeBonus -=bonustoAdd;
            }

            Debug.Print($"Final player attributes STR:{playerdat.STR} DEX:{playerdat.DEX} INT:{playerdat.INT}");
        }

        static void PrintChoices(int QuestionNo)
        {
            var noOfChoices = getAt(chargen_dat, 8 + (QuestionNo * 18), 8);
            int ptrToChoices = (int)getAt(chargen_dat, 4 + (QuestionNo * 18), 32);
            int question = (int)getAt(chargen_dat, (QuestionNo * 18), 8);
            Debug.Print($"The question is {GameStrings.GetString(2, question)}");
            for (int i = 0; i<noOfChoices;i++)
            {        
                int choice = (int)getAt(chargen_dat, ptrToChoices + (i*2), 8);     
                Debug.Print($"{i}. {GameStrings.GetString(2, choice)}");
            }
        }

        /// <summary>
        /// Reads in the body of the chrgen file and stores the pointers to the answer strings
        /// </summary>
        static void InitChargenFiles()
        {
            var path_chargen = System.IO.Path.Combine(BasePath, "DATA", "CHRGEN.DAT");
            var path_skills = System.IO.Path.Combine(BasePath, "DATA", "SKILLS.DAT");

            if (System.IO.File.Exists(path_chargen) && System.IO.File.Exists(path_skills))
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
    }//end class
}// end namespace