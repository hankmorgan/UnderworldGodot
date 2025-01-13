namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        /// <summary>
        /// Changes animation and frame for npc
        /// </summary>
        public static void set_sequence()
        {
            var var2 = GetConvoStackValueAtPtr(stackptr-1);
            var di = GetConvoStackValueAtPtr(stackptr-2);
            var whoami = GetConvoStackValueAtPtr(stackptr-3);
            var functionparam = (di<<3) | (var2 & 0x7);

            CallBacks.RunCodeOnNPCS_WhoAmI(set_sequence, whoami, new int[]{functionparam},false);
        }

        static void set_sequence(uwObject critter, int[] paramsarray)
        {  
            critter.AnimationFrame = (byte)(paramsarray[0] & 7);
            critter.npc_animation = (short)(paramsarray[0] >>3);
        }
    }//end class
}//end namespace