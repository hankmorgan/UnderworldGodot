namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void set_quest()
        {
            var questno = GetConvoStackValueAtPtr(stackptr - 2);   
            var newvalue = GetConvoStackValueAtPtr(stackptr - 1);   
            playerdat.SetQuest(questno, newvalue);
        }
    }//end class
}//end namespace