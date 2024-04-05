namespace Underworld
{
    public class smallblackrockgem:objectInstance
    {
        public static bool LookAt(uwObject obj, uwObject[] objList)
        {
            var warmth = "";
            if (obj.owner==1)
                {
                    warmth = GameStrings.GetString(1,357); //warm
                }
            else
                {
                    warmth = GameStrings.GetString(1,356); //cool
                }

            uimanager.AddToMessageScroll($"{GameStrings.GetString(1,GameStrings.str_you_see_)}a {warmth}{GameStrings.GetSimpleObjectNameUW(obj.item_id)}");
            return true;
        }
    }//end calss
}//end namespace