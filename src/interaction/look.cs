namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the look verb
    /// </summary>
    public class look:UWClass
    {
        public static bool LookAt(int index, uwObject[] objList)
        {
            if (index<=objList.GetUpperBound(0))
            {
                var obj = objList[index];
                switch (obj.majorclass)
                {
                    case 5:
                        {
                            return LookMajorClass5(obj,objList);
                        }
                    
                }
                //default string
                messageScroll.AddString(GameStrings.GetObjectNounUW(obj.item_id));
            }

            return false;
        }

        public static bool LookMajorClass5(uwObject obj, uwObject[] objList)
        {
            switch (obj.minorclass)
            {
                case 2: //misc objects including readables
                    {
                        switch (obj.classindex)
                        {
                            case 6: // a readable sign.
                                return writing.LookAt(obj);
                            case 0xE:
                            case 0xF:
                                return tmap.LookAt(obj);
                            default:

                                return true;
                        }
                    }

            }

            return false;
        }

    }
}