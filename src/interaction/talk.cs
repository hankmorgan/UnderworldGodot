using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the talk verb
    /// </summary>
    public class talk : UWClass
    {
        public static bool Talk(int index, uwObject[] objList, bool WorldObject = true)
        {
            if (index <= objList.GetUpperBound(0))
            {
                var obj = objList[index];
                switch (obj.majorclass)
                {
                    case 1: //NPCs
                        {                           
                            ConversationVM.StartConversation(obj);
                            break;
                        }
                    default:
                        {
                            messageScroll.AddString(GameStrings.GetString(1, GameStrings.str_you_cannot_talk_to_that_));
                            break;
                        }
                }
            }
            return false;
        }        
    } //end class
} //end namespace