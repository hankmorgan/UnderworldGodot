using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        public static void switch_pic()
        {
            var WhoAmI = at(at(stackptr - 1));
            uimanager.instance.NPCPortrait.Texture = NPCPortrait(WhoAmI, 0);
            var newname =GameStrings.GetString(7, WhoAmI + 16);
            if (newname!="")
            {
                uimanager.instance.NPCNameLabel.Text = newname;
            }            
        }
    }   //end class
}//end namespace