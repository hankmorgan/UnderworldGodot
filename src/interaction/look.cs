namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the look verb
    /// </summary>
    public class look : UWClass
    {
        public static bool LookAt(int index, uwObject[] objList)
        {
            bool result = false;
            trap.ObjectThatStartedChain = index;
            if (index <= objList.GetUpperBound(0))
            {
                var obj = objList[index];
                switch (obj.majorclass)
                {
                    case 4:
                        {
                            result = LookMajorClass4(obj, objList);
                            break;
                        }
                    case 5:
                        {
                            result = LookMajorClass5(obj, objList);
                            break;
                        }

                }
                if ((obj.is_quant == 0) && (obj.link != 0))
                {
                    var linkedObj = objList[obj.link];
                    if (linkedObj.item_id == 419)
                    {
                        trigger.LookTrigger(
                            srcObject: obj,
                            triggerIndex: obj.link,
                            objList: objList);
                        return true;
                    }
                }
                if (!result)
                {
                    //default string  when no overriding action has occured           
                    messageScroll.AddString(GameStrings.GetObjectNounUW(obj.item_id));
                }
            }

            return false;
        }

        public static bool LookMajorClass4(uwObject obj, uwObject[] objList)
        {
            switch (obj.minorclass)
            {
                case 3: //readables (up to index 8)
                    {
                        if (obj.classindex <= 8)
                        {
                            return Readable.LookAt(obj);
                        }
                        break;
                    }
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