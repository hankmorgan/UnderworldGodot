using System.ComponentModel;

namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the use verb
    /// </summary>
    public class use : UWClass
    {
        public static bool Use(int index, uwObject[] objList, bool WorldObject = true)
        {
            if (index==-1){return false;}
            trap.ObjectThatStartedChain = index;
            bool result = false;
            if (index <= objList.GetUpperBound(0))
            {
                var obj = objList[index];
                switch (obj.majorclass)
                {
                    case 2:
                        {
                            result = UseMajorClass2(obj, objList, WorldObject);
                            break;
                        }
                    case 4:
                        {
                            result = UseMajorClass4(obj, objList, WorldObject);
                            break;
                        }
                    case 5:
                        {
                            result = UseMajorClass5(obj, objList, WorldObject);
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

        public static bool UseMajorClass2(uwObject obj, uwObject[] objList, bool WorldObject)
        {
            switch (obj.minorclass)
                {
                    case 0:
                        {
                            if (obj.classindex<0xF)
                            {//containers
                                return container.Use(obj, WorldObject);
                            }
                            else
                            {
                                //runebag
                                return runebag.Use(obj, WorldObject);
                            }
                        }
                    case 1:
                        {
                            if (obj.classindex<=7)
                            {//lights
                                return light.Use(obj,WorldObject);
                            } 
                            else
                            {
                                //wands
                            }                           
                            break;
                        }
                    case 2:
                        {   
                            switch (obj.classindex)
                            {
                                case 0xF:
                                    {//picketwatch in uw2, gold nugget in uw1
                                        if (_RES==GAME_UW2)
                                        {
                                            return pocketwatch.Use(obj, WorldObject);
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    case 3://food
                        {
                            return food.Use(obj, WorldObject);
                        }
                }
            return false;
        }

        public static bool UseMajorClass4(uwObject obj, uwObject[] objList, bool WorldObject)
        {
            switch (obj.minorclass)
            {
                case 3: //readables (up to index 8)
                    {
                        if (obj.classindex <= 8)
                        {
                            return Readable.LookAt(obj);
                        }
                        if (((obj.classindex == 0xB) && (_RES!=GAME_UW2)) || ((obj.classindex == 0xA) && (_RES==GAME_UW2)))
                        {//class 4-2-B in UW1, class 4-2-A in UW2
                            return map.Use(obj, WorldObject);
                        }
                        break;
                    }
            }
            return false;
        }

        public static bool UseMajorClass5(uwObject obj, uwObject[] objList, bool WorldObject)
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