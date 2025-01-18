using System.Diagnostics;
namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void get_quest()
        {        
            var questno = GetConvoStackValueAtPtr(stackptr - 1); 
            result_register = GetQuest(questno);
            Debug.Print($"Getting quest {questno} which is {result_register}");
            return;
        }
 
        static int GetQuest(int questno)
        {
            if (questno<0){return 0;}
            if (_RES==GAME_UW2)
                {                       
                    if (questno < 144)//regular quest variables
                    {
                        return playerdat.GetQuest(questno);
                    }
                    else
                    {
                        return playerdat.GetQuest(144); //this is what uw2 does
                    }

                }
            else
                {
                    //UW1 simple quest no's only
                    if (questno<36)
                    {
                        return playerdat.GetQuest(questno);
                    }
                    else
                    {
                        return playerdat.GetQuest(36);
                    }
                }
        }
    }//end class
} //end namespace