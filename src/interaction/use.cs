namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the look verb
    /// </summary>
    public class use:UWClass
    {
        public static bool Use(int index, uwObject[] objList)
        {
            bool result;
            if (index<=objList.GetUpperBound(0))
            {
                var obj = objList[index];
                switch (obj.majorclass)
                {
                    case 5:
                        {
                            result = UseMajorClass5(obj,objList);
                            break;
                        }                    
                }
                //Check for use trigger on this action and try activate if so.
                if ((obj.is_quant==0) && (obj.link!=0))
                {
                    trigger.UseTrigger(
                        srcObject: obj, 
                        triggerIndex: obj.link, 
                        objList: objList);
                    return true;
                }
            }
          
            messageScroll.AddString(GameStrings.GetString(1,GameStrings.str_you_cannot_use_that_));
            return false;
        }

        public static bool UseMajorClass5(uwObject obj, uwObject[] objList)
        {
            switch (obj.minorclass)
            {
                case 2: //misc objects including readables
                    {
                        switch (obj.classindex)
                        {
                            case 6: // a readable sign. interaction is also a look
                                return writing.LookAt(obj);
                            case 0xE://tmap
                            case 0xF:
                                return tmap.LookAt(obj);
                            default:

                                return true;
                        }
                    }
                case 3: //buttons
                    {
                        return button.Use(obj);
                    }

            }

            return false;
        }

    }
}