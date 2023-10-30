namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the look verb
    /// </summary>
    public class use:UWClass
    {
        public static bool Use(int index, uwObject[] objList)
        {
            if (index<=objList.GetUpperBound(0))
            {
                var obj = objList[index];
                switch (obj.majorclass)
                {
                    case 5:
                        {
                            return UseMajorClass5(obj,objList);
                        }
                    
                }
            }
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
                            default:

                                return true;
                        }
                    }

            }

            return false;
        }

    }
}