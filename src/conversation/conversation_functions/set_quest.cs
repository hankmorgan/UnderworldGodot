namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void set_quest()
        {
            var questno = GetConvoStackValueAtPtr(stack + stackptr - 2);   
            var newvalue = GetConvoStackValueAtPtr(stack + stackptr - 1);   
            playerdat.SetQuest(questno, newvalue);
        }
    }//end class
}//end namespace