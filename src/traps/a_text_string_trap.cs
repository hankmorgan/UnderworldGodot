namespace Underworld
{
    public class a_text_string_trap : trap
    {
        public static void Activate(uwObject trapObj, uwObject[] objList)
        {
            int StringNo;
            switch (_RES)
            {
                case GAME_UW2:
                    StringNo = 32 * trapObj.quality + trapObj.owner;//I hope.
                    break;
                default:
                    StringNo = (64 * (playerdat.dungeon_level-1)) + trapObj.owner;
                    break;
            }
            uimanager.AddToMessageScroll(GameStrings.GetString(9, StringNo));
        }
    }//end class
}//end namespace