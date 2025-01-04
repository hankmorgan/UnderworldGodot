namespace Underworld
{
    public class doorkey : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (!WorldObject)
            {
                //flag we are using the key
                useon.CurrentItemBeingUsed = new useon(obj, WorldObject);
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
            bool isDoor = (targetObject.majorclass == 5) && (targetObject.minorclass == 0);
            if (a_lock.LockObject(targetObject)!=null)
            {//object has a lock
                if (isDoor)
                {
                    UsedOnDoor(KeyObject, targetObject);
                }
                else
                {
                    if (a_lock.GetIsLocked(targetObject))
                    {
                        a_lock.SetIsLocked(targetObject,false,0);
                        uimanager.AddToMessageScroll(GameStrings.GetString(1,5));
                    }
                    else
                    {
                        a_lock.SetIsLocked(targetObject,true,0);
                        uimanager.AddToMessageScroll(GameStrings.GetString(1,4));
                    }                
                }                
            }
            else
            {
                //else return no lock on that message
                uimanager.AddToMessageScroll(GameStrings.GetString(1,3));
            }
            return true;
        }

        /// <summary>
        /// Handles the extra actions of using a key on a door.
        /// </summary>
        /// <param name="KeyObject"></param>
        /// <param name="targetObject"></param>
        private static void UsedOnDoor(uwObject KeyObject, uwObject targetObject)
        {
            var doorInstance = (door)targetObject.instance;
            if (door.isOpen(targetObject))
            {
                //6	That is already open.
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 6));
            }
            else
            { //do key actions
                if ( a_lock.GetIsLocked(targetObject))
                {
                    //locked. try and unlock
                    if (a_lock.KeyIndex(targetObject) == KeyObject.owner)                    
                    {
                        //Unlock door
                        a_lock.SetIsLocked(targetObject, false, 0);
                       // doorInstance.Locked = false;
                        door.ToggleDoor(targetObject);
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 4));
                    }
                    else
                    {
                        //2	The key does not fit.
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 2));
                    }
                }
                else
                {
                    //unlocked, try and lock
                    //4	The key locks the lock.
                    if (a_lock.KeyIndex(targetObject) == KeyObject.owner)  
                    {
                        //lock door
                        //doorInstance.Locked = true;
                        a_lock.SetIsLocked(targetObject, true, 0);
                    }
                    else
                    {
                        //2	The key does not fit.
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 2));
                    }
                }
            }
        }
    }// end class
}//end namespace