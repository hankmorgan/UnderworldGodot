using System.Diagnostics;

namespace Underworld
{
    public class a_text_string_trap : trap
    {
        public static void activate(uwObject trapObj, uwObject[] objList)
        {
            int StringNo;
            switch (_RES)
            {
                case GAME_UW2:
                    StringNo = 32 * trapObj.quality + trapObj.owner;//I hope.
                    break;
                default:
                    StringNo = (64 * uwsettings.instance.level) + trapObj.owner;
                    break;
            }
            messageScroll.AddString(GameStrings.GetString(9,StringNo));


            //Continue the trigger-trap chain if possible.
            if((trapObj.link!=0) && (trapObj.is_quant==0))
            {
                trigger.Trigger(
                    srcObject: trapObj, 
                    triggerIndex: trapObj.link, 
                    objList: objList);
            }
        }
    }
}