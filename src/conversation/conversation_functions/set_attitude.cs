namespace Underworld
{
    public partial class ConversationVM : UWClass
    {

        /// <summary>
        /// Sets the attitude for the first found NPC that matches whoami
        /// </summary>
        public static void set_attitude()
        {
            var newAttitude_di = GetConvoStackValueAtPtr(stackptr - 1);
            var whoami = GetConvoStackValueAtPtr(stackptr - 2);
            CallBacks.RunCodeOnNPCS(npc.set_attitude_by_array, whoami, new int[] { newAttitude_di }, false);

            //Note there is buggy usage of this function in vanilla UW2 on the Prison Tower level 2. Goblin Guard conversation. 
            //Due to uninitialised memory caused by the reading of the conversation ARK files this will be called on an non-existant NPC to set attitude of 1.
            //As this engine does not replicate the same bug exactly it will try and update a different non-existant NPC
        }

    }//end class
}//end namespace