using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        /// <summary>
        /// Sets the pointer to the array of likes/dislikes for the npc
        /// </summary>
        public static void set_likes_dislikes()
        {
            Likes = at(stackptr-2);         

            var arrayindex = Likes;
            var entry = at(arrayindex);
            while (entry !=-1)
            {
                if (entry>=1000)
                {
                    Debug.Print($"NPC Likes Category {entry-1000} (example item: {GameStrings.GetObjectNounUW((entry-1000)<<4)})");
                }
                else
                {
                    Debug.Print($"NPC likes item {GameStrings.GetSimpleObjectNameUW(entry)}");
                }                
                arrayindex++;
                entry = at(arrayindex);
            }


            Dislikes = at(stackptr-1);

            arrayindex = Dislikes;
            entry = at(arrayindex);
            while (entry !=-1)
            {
                if (entry>=1000)
                {

                    Debug.Print($"NPC dislikes Category {entry-1000} (example item: {GameStrings.GetObjectNounUW((entry-1000)<<4)})");
                }
                else
                {
                    Debug.Print($"NPC dislikes item {GameStrings.GetSimpleObjectNameUW(entry)}");
                }                
                arrayindex++;
                entry = at(arrayindex);
            }
        }
    } //end class
}//end namespace