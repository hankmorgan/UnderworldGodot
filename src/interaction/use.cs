using System.Runtime.Serialization.Formatters;

namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the use verb
    /// </summary>
    public class use : UWClass
    {
        public static bool Use(int index, uwObject[] objList)
        {
            trap.ObjectThatStartedChain = index;
            bool result=false;
            if (index <= objList.GetUpperBound(0))
            {
                var obj = objList[index];
                switch (obj.majorclass)
                {
                    case 5:
                        {
                            result = UseMajorClass5(obj, objList);
                            break;
                        }
                }
                //Check for use trigger on this action and try activate if so.
                if ((obj.is_quant == 0) && (obj.link != 0))
                {
                    result = trigger.UseTrigger(
                        srcObject: obj,
                        triggerIndex: obj.link,
                        objList: objList);
                }
            }
        if (!result)
        {
 messageScroll.AddString(GameStrings.GetString(1, GameStrings.str_you_cannot_use_that_));
        }
           
            return false;
        }

        public static bool UseMajorClass5(uwObject obj, uwObject[] objList)
        {
            switch (obj.minorclass)
            {
                case 0: // Doors
                    {
                        return door.Use(obj);

                    }
                case 2: //misc objects including readables
                    {
                        switch (obj.classindex)
                        {
                            case 1:
                            case 2: //rotary switches                                
                                return buttonrotary.Use(obj);
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