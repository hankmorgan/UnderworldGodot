using System.Collections.Generic;
using Godot;

namespace Underworld
{
    public class doorkey : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (!WorldObject)
            {
                //become the object in hand.
                playerdat.ObjectInHand = obj.index;

                //change the mouse cursor
                uimanager.instance.mousecursor.SetCursorArt(obj.item_id);

                //print use message
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 7));
            }
            return true;
        }

        public static bool LookAt(uwObject obj , bool WorldObject)
        {
            if (WorldObject)
            {
                uimanager.AddToMessageScroll(GameStrings.GetObjectNounUW(obj.item_id));
            }
            else
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(5, obj.owner + 100));
            }
            return true;
        }

        public static bool UseOn(uwObject KeyObject, uwObject targetObject)
        {
            //if target object is door then use key on door.
            if ((targetObject.majorclass == 5) && (targetObject.minorclass == 0))
            {
                var doorInstance = (door)targetObject.instance;
                if (doorInstance.isOpen)
                    {
                    //6	That is already open.
                    uimanager.AddToMessageScroll(GameStrings.GetString(1,6));
                    }
                else
                    { //do key actions
                        if (doorInstance.Locked)
                        {
                            //locked. try and unlock
                            if (doorInstance.KeyIndex == KeyObject.owner)
                            {
                                //Unlock door
                                doorInstance.Locked = false;
                                door.ToggleDoor(doorInstance);
                                uimanager.AddToMessageScroll(GameStrings.GetString(1,4));
                            }
                            else
                            {
                                //2	The key does not fit.
                                uimanager.AddToMessageScroll(GameStrings.GetString(1,2));
                            }
                        }
                        else
                        {
                            //unlocked, try and lock
                            //4	The key locks the lock.
                            if (doorInstance.KeyIndex == KeyObject.owner)
                            {
                                //lock door
                                doorInstance.Locked = true;
                            }
                            else
                            {
                                //2	The key does not fit.
                                uimanager.AddToMessageScroll(GameStrings.GetString(1,2));
                            }
                            //
                        }
                    }
            }
            else
            {
                //else return no lock on that  message
                uimanager.AddToMessageScroll(GameStrings.GetString(1,3));
            }
            return true;
        }
    }// end class
}//end namespace